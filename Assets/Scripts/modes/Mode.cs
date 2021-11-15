using System.Collections.Generic;
using UnityEngine;

namespace unitrys{
    public class Mode : MonoBehaviour, IControlsObserver
    {
        public int GRID_WIDTH = 10;
        public int GRID_HEIGHT = 20;
        private const float TARGET_DELTA_TIME = 1f/60;

        protected Dictionary<int[], int[]> _timings;
        protected Dictionary<int, int> _gravities;
        protected const int ARE_TIMINGS_INDEX = 0;
        protected const int LINE_ARE_TIMINGS_INDEX = 1;
        protected const int DAS_TIMINGS_INDEX = 2;
        protected const int LOCK_TIMINGS_INDEX = 3;
        protected const int LINECLEAR_TIMINGS_INDEX = 4;
        protected const int ARR_TIMINGS_INDEX = 5;

        private GameObject _tetrionPrefab;
        private GameObject _blockPrefab;
        private GameObject _tetrion;

        protected IRandomizer _randomizer;
        protected IRotationSystem _rotationSystem;
        protected IRule _rule;
        protected Theme _theme;
        protected List<Block> _blocks;
        protected List<Piece> _history;
        protected List<GameObject> _previewBlocks;
        protected List<int> _clearedLines;
        protected Level _level;
        protected int _maxLevel;

        protected int _lineARE;
        protected bool _waitForARE = false;
        protected bool _waitForDAS = false;
        protected bool _waitForLockDelay = false;
        protected bool _waitForLineClear = false;
        protected bool _waitForSoftDrop = false;
        protected bool _autoShift = false;
        protected bool _hardDrop = false;
        protected bool _firstPiece = true;
        protected bool _started = false;
        protected bool _rotateLeftPressed = false;
        protected bool _rotateRightPressed = false;
        protected bool _rotationDuringARE = false;

        protected int _minClearedLineIndex = -1;
        protected int _lines = 0;
        protected float _count = 0; //counter to know the number of rows to go down
        protected float _count2 = 0; //counter for entry delay
        protected float _count3 = 0; //counter for lock delay
        protected float _count4 = 0; //counter for das
        protected float _count5 = 0; //counter for line clear
        protected float _count6 = 0; //counter for arr
        protected float _count7 = 0; //counter for soft drop

        public Mode(){
            _theme = new Theme();
            _blocks = new List<Block>();
            _history = new List<Piece>();
            _history.Add(new Piece(Piece.EMPTY));
            _previewBlocks = new List<GameObject>();
            _timings = new Dictionary<int[], int[]>();
            _gravities = new Dictionary<int, int>();
            _clearedLines = new List<int>();

            SetTimings();
        }

        public Theme theme{
            get{
                return _theme;
            }
            set{
                _theme = value;
            }
        }

        public List<Block> blocks{
            get{
                return _blocks;
            }
        }

        public List<Piece> history{
            get{
                return _history;
            }
        }

        public int level{
            get{
                return _level.level;
            }
        }

        public int maxLevel{
            get{
                return _maxLevel;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            _tetrionPrefab = Resources.Load<GameObject>("Prefabs/Tetrion");
            _blockPrefab = Resources.Load<GameObject>("Prefabs/Block");

            _tetrion = GameObject.Instantiate(_tetrionPrefab, transform);
            for(int i=0; i < _tetrion.transform.childCount; i++){
                GameObject child = _tetrion.transform.GetChild(i).gameObject;
                child.layer = Layers.FOREGROUND;
            }
            Utils.ChangeTetrionColor(_tetrion, GetTetrionColor());

			for(int i=0; i < GRID_WIDTH; i++){
				for(int j=0; j < GRID_HEIGHT+4; j++){
					GameObject obj = GameObject.Instantiate(_blockPrefab, 
                        new Vector3(
                            _tetrion.transform.Find("Left").transform.position.x + (i+1)*_blockPrefab.transform.localScale.x,
                            _tetrion.transform.Find("Bottom").transform.position.y + (j+1)*_blockPrefab.transform.localScale.y,
                            0),
                        Quaternion.Euler(0f, 180f, 0f), 
                        transform);
                    obj.layer = Layers.FOREGROUND;
                    Block b = obj.GetComponent<Block>();
                    b.texture = _theme.GetTexture();
                    b.x = i;
                    b.y = j;
                    b.empty = true;
                    _blocks.Add(b);
				}
			}

            RenderPreview();
        }

        // Update is called once per frame
        void Update()
        {
        }

        public virtual void HandleAction(string actionId, object param=null){
            if(actionId == Controls.ROTATE_LEFT_ACTION_ID){
                _rotateLeftPressed = true;
            }
            else if(actionId == Controls.ROTATE_RIGHT_ACTION_ID){
                _rotateRightPressed = true;
            }
            if(!_started){
                return;
            }
            switch(actionId){
                case Controls.RESTART_ACTION_ID:
                    SendMessageUpwards("Restart", SendMessageOptions.DontRequireReceiver);
                    break;
                case Controls.RETURN_TO_MENU_ACTION_ID:
                    SendMessageUpwards("DisplayMenu", SendMessageOptions.DontRequireReceiver);
                    break;
                case Controls.HARD_DROP_ACTION_ID:
                case Controls.SOFT_DROP_ACTION_ID:
                case Controls.LEFT_ACTION_ID:
                case Controls.RIGHT_ACTION_ID:
                    MovePiece(actionId);
                    break;
                case Controls.ROTATE_LEFT_ACTION_ID:
                    if(_waitForARE){
                        _rotationDuringARE = true;
                    }
                    RotatePiece(actionId);
                    break;
                case Controls.RELEASE_ROTATE_LEFT_ACTION_ID:
                    _rotateLeftPressed = false;
                    break;
                case Controls.ROTATE_RIGHT_ACTION_ID:
                    if(_waitForARE){
                        _rotationDuringARE = true;
                    }
                    RotatePiece(actionId);
                    break;
                case Controls.RELEASE_ROTATE_RIGHT_ACTION_ID:
                    _rotateRightPressed = false;
                    break;
                case Controls.INPUT_ACTION_MOVE_UP:
                    ResetDAS();
                    break;
            }
        }

        public void ProcessUpdate(float deltaTime){
            float delta = deltaTime/TARGET_DELTA_TIME;
            _count+=_level.gravity*delta;
            _count2+=delta;
            _count3+=delta;
            _count4+=delta;
            _count5+=delta;
            _count6+=delta;
            _count7+=delta;

            Piece piece = GetCurrentPiece();
            if(piece!=null && piece.name == Piece.EMPTY && !_waitForARE){
                RenderNextPiece();
            }

            if(_waitForDAS && (_count4 >= _level.das)){
                _waitForDAS = false;
                _autoShift = true;
                _count6 = 0;
            }
            
            if(_waitForLockDelay && (_count3 >= _level.lockDelay)){
                Game.GetSoundManager().PlaySound(Sounds.SOUND_LOCK);
                _waitForLockDelay = false;
                GetCurrentPiece().locked = true;
                _count = 1;
            }

            if(_waitForARE && (_count2 >= _level.are+_lineARE)){
                if(!_rotationDuringARE){
                    if(_rotateLeftPressed){
                        RotatePiece(Controls.ROTATE_LEFT_ACTION_ID);
                    }
                    else if(_rotateRightPressed){
                        RotatePiece(Controls.ROTATE_RIGHT_ACTION_ID);
                    }
                }
                _rotationDuringARE = false;
                _waitForARE = false;
                _hardDrop = false;
                RenderNextPiece();
            }

            if(_waitForLineClear && _count5 >= _level.lineClear/2){
                foreach(int lineIndex in _clearedLines){
                    foreach(Block block in _blocks){
                        if(block.y == lineIndex){
                            block.empty = true;
                            block.RestoreTexture();
                        }
                    }
                }
            }

            if(_waitForLineClear && _count5 >= _level.lineClear){
                _lines += _clearedLines.Count;
                gameObject.SendMessageUpwards("DisplayLines", _lines, SendMessageOptions.DontRequireReceiver); //TODO
                _rule.IncreaseLevel(_clearedLines.Count, true);
                
                DropLinesNaive();

                _minClearedLineIndex = -1;
                _clearedLines.Clear();
                _waitForLineClear = false;

                if(_rule.CheckGameOver()){
                    gameObject.SendMessageUpwards("GameOver", SendMessageOptions.DontRequireReceiver);
                    return;
                }

                _lineARE = _level.lineARE;
                StartARE();
            }

            if(_waitForSoftDrop && _count7 >= 1){
                _waitForSoftDrop = false;
            }

            if(_count >= 1){
                int numRows = Mathf.FloorToInt(_count);
                for(int i=0; i < numRows; i++){
                    piece = GetCurrentPiece();
                    if(piece!=null && !_waitForLineClear && !_waitForARE){
                        if(!piece.MoveDown()){
                            if(!piece.locked){
                                if(!_waitForLockDelay){
                                    if(!Game.GetSoundManager().IsPlaying(Sounds.SOUND_GROUND)){
                                        Game.GetSoundManager().PlaySound(Sounds.SOUND_GROUND);
                                    }
                                    StartLockDelay(_hardDrop && _rotationSystem.HardDropLock());
                                }
                            }
                            else{
                                if(CheckLines()){
                                    StartLineClear();
                                }
                                else{
                                    GiveNextPiece();
                                }
                            }
                        }
                        else if(piece.locked){
                            if(_rotationSystem.AllowStepReset()){
                                piece.locked = false;
                                StartLockDelay();
                            }
                            else{
                                for(int j=0; j < GRID_HEIGHT; j++){
                                    piece.MoveDown();
                                }
                            }
                        }
                    }
                }
                _count=0;
            }
        }

        private void MovePiece(string actionId)
        {
            if ((_waitForDAS && (actionId == Controls.LEFT_ACTION_ID || actionId == Controls.RIGHT_ACTION_ID)) ||
            _waitForLineClear || (_waitForARE && _autoShift))
            {
                return;
            }

            Piece p = !_waitForARE ? GetCurrentPiece() : GetNextPiece();

            if (p != null && !p.locked)
            {
                switch (actionId)
                {
                    case Controls.LEFT_ACTION_ID:
                    case Controls.RIGHT_ACTION_ID:
                        if(!_autoShift || _count6 >= 1){
                            for(int i=0; i < _level.arr; i++){
                                if(actionId==Controls.LEFT_ACTION_ID){
                                    p.MoveLeft();
                                }
                                else{
                                    p.MoveRight();
                                }
                            }
                            _count6=0;
                        }
                        break;
                    case Controls.SOFT_DROP_ACTION_ID:
                        SoftDrop(p);
                        break;
                    case Controls.HARD_DROP_ACTION_ID:
                        if (!_waitForARE && !_hardDrop)
                        {
                            _count = GRID_HEIGHT;
                            _hardDrop = true;
                        }
                        break;
                }

                if (actionId != Controls.HARD_DROP_ACTION_ID && !_autoShift)
                {
                    StartDAS();
                }
            }
        }

        private void RotatePiece(string actionId){
            Piece piece = GetCurrentPiece();
            List<Block> rotatedBlocks = null;
            if(piece!=null && !_waitForLineClear){
                if(piece.locked || piece.name == Piece.EMPTY){
                    piece = GetNextPiece();
                    if(piece==null){
                        return;
                    }
                    if(!piece.rendered){
                        RenderPiece(piece, false);
                    }
                }

                rotatedBlocks = actionId == Controls.ROTATE_LEFT_ACTION_ID ? 
                    _rotationSystem.RotateCounterClockWise(_blocks, piece) : _rotationSystem.RotateClockWise(_blocks, piece);

                if(rotatedBlocks!=null){
                    foreach(Block block in piece.blocks){
                        block.empty = true;
                    }

                    piece.blocks = rotatedBlocks;

                    piece.rotationState += actionId == Controls.ROTATE_LEFT_ACTION_ID ? -1 : 1;
                    int numRotations = _rotationSystem.GetNumberOfRotationStates(piece.name);

                    if(piece.rotationState <= -numRotations || piece.rotationState >= numRotations){
                        piece.rotationState=0;
                    }

                    if(piece.visible){
                        foreach(Block block in piece.blocks){
                            block.color = piece.color;
                            block.empty = false;
                        }
                    }
                }
            }
        }


        public void StartGame(int level){
            SetLevel(level);
            _started = true;
            GiveNextPiece();
        }

        private void RenderPreview(){
            Piece piece = GetNextPiece();
            int[] shapeBlocks = _rotationSystem.GetInitialState(piece.name);
            Color color = _theme.GetColor(piece.name);
            float posX = _tetrion.transform.Find("Top").transform.position.x - 1.5f;
            float posY = _tetrion.transform.Find("Top").transform.position.y + 3.5f;
            int x=0;
            int y=-1;

            for(int i=0; i < _previewBlocks.Count; i++){
                Destroy(_previewBlocks[i]);
            }
            _previewBlocks.Clear();

            for(int i=0; i < shapeBlocks.Length; i++){
                if(shapeBlocks[i]==1){
                    GameObject obj = GameObject.Instantiate(_blockPrefab, 
                        new Vector3(
                            posX + x*_blockPrefab.transform.localScale.x,
                            posY - y,
                            0),
                        Quaternion.Euler(0f, 180f, 0f),
                        transform);
                    obj.layer = Layers.FOREGROUND;
                    Block b = obj.GetComponent<Block>();
                    b.texture = _theme.GetTexture();
                    b.color = color;
                    _previewBlocks.Add(obj);
                }
                x++;
                if(x==4){
                    x=0;
                    y++;
                }
            }
            
            if(!_firstPiece){
                Game.GetSoundManager().PlaySound(piece.soundId);
            }
        }

        private void RenderPiece(Piece piece, bool visible=true){
            piece.visible = visible;
            int yOffset = 0;
            if(!piece.visible){
                yOffset = 3;
            }

            piece.color = _theme.GetColor(piece.name);
            int[] shapeBlocks = _rotationSystem.GetInitialState(piece.name);
            
            if(shapeBlocks!=null){
                int x=0;
                int y=-1;
                for(int i=0; i < shapeBlocks.Length; i++){
                    if(shapeBlocks[i]==1){
                        int blockX = 3+x;
                        int blockY = GRID_HEIGHT-(y+1)+yOffset;
                        Block block = _blocks.Find(block => block.x == blockX && block.y == blockY);
                        block.color = !visible && blockY < GRID_HEIGHT ? Color.black : piece.color;
                        block.empty = false;
                        piece.blocks.Add(block);
                        piece.boundingBoxCoordinates.Add(block, new int[]{x, y+1});
                    }
                    
                    x++;
                    if(x==4){
                        x=0;
                        y++;
                    }
                }
            }

            piece.rendered = true;
        }

        private void RenderNextPiece()
        {
            if (_history.Count > 0)
            {
                if (!_firstPiece)
                {
                    _history.RemoveAt(0);
                }
                Piece next = GetCurrentPiece();
                if (next != null)
                {
                    List<int[]> topRowBlocks = new List<int[]>();
                    foreach (Block block in _blocks)
                    {
                        if (block.y >= GRID_HEIGHT - 4 && block.y <= GRID_HEIGHT - 1 && !block.empty)
                        {
                            topRowBlocks.Add(new int[]{block.x, block.y});
                        }
                    }
                    if (!next.rendered)
                    {
                        RenderPiece(next);
                    }
                    else
                    {
                        List<Block> newBlocks = new List<Block>();
                        Dictionary<Block,int[]> newBoundingBoxCoordinates = new Dictionary<Block, int[]>();
                        foreach (Block block in next.blocks)
                        {
                            Block blockBelow = _blocks.Find(b => b.x == block.x && b.y == block.y - 3);
                            blockBelow.color = next.color;
                            blockBelow.empty = false;
                            blockBelow.locked = false;
                            newBlocks.Add(blockBelow);
                            int[] coordinates;
                            next.boundingBoxCoordinates.TryGetValue(block, out coordinates);
                            newBoundingBoxCoordinates.Add(blockBelow, coordinates);
                        }
                        foreach (Block block in next.blocks){
                            if(newBlocks.IndexOf(block)==-1){
                                block.empty = true;
                            }
                        }
                        next.blocks = newBlocks;
                        next.boundingBoxCoordinates = newBoundingBoxCoordinates;
                        next.visible = true;
                    }

                    //checking game over
                    foreach (Block block in next.blocks)
                    {
                        foreach (int[] topRowBlock in topRowBlocks)
                        {
                            if ((block.x == topRowBlock[0]) && (block.y == topRowBlock[1]))
                            {
                                next.locked = true;
                                gameObject.SendMessageUpwards("GameOver", SendMessageOptions.DontRequireReceiver);
                                return;
                            }
                        }
                    }

                    //increase level
                    if (next.name != Piece.EMPTY)
                    {
                        _rule.IncreaseLevel(1, false);
                    }
                    
                    RenderPreview();
                }

                _firstPiece = false;
            }
        }

        private void StartARE(){
            ResetDAS();
            if(_level.are > 0){
                _count2 = 0;
                _waitForARE = true;
            }
        }

        private void StartLineClear(){
            if(_level.lineClear > 0){
                _count5 = 0;
                _waitForLineClear = true;
            }
        }

        protected void StartLockDelay(bool instantLock=false){
            if(_level.lockDelay > 0){
                _count3 = !instantLock ? 0 : _level.lockDelay;
                _waitForLockDelay = true;
            }
        }

        private void StartDAS(){
            if(_level.das > 0){
                _count4 = 0;
                _waitForDAS = true;
            }
        }

        private void ResetDAS(){
            _count4 = 0;
            _waitForDAS = false;
            _autoShift = false;
        }

        private bool CheckLines(){
            for(int i=0; i < GRID_HEIGHT; i++){
                int filled = 0;
                foreach(Block block in _blocks){
                    if(block.y==i && !block.empty){
                        filled++;
                    }
                }
                if(filled == GRID_WIDTH){
                    _clearedLines.Add(i);
                    if(_minClearedLineIndex == -1){
                        _minClearedLineIndex = i;
                    }
                    else{
                        _minClearedLineIndex = Mathf.Min(i, _minClearedLineIndex);
                    }
                }
            }
            
            if(_clearedLines.Count > 0){
                Game.GetSoundManager().PlaySound(Sounds.SOUND_CLEAR);
                if(_clearedLines.Count == 4){
                    Game.GetSoundManager().PlaySound(Sounds.SOUND_TETRIS);
                }
            }

            foreach(int lineIndex in _clearedLines){
                foreach(Block block in _blocks){
                    if(block.y == lineIndex){
                        block.Clear();
                    }
                }
            }

            return _clearedLines.Count > 0;
        }

        private void DropLinesNaive(){
            Game.GetSoundManager().PlaySound(Sounds.SOUND_FALL);
            for(int y=_minClearedLineIndex+1; y <= GRID_HEIGHT; y++){
                for(int x=0; x < GRID_WIDTH; x++){
                    Block blockToDrop = _blocks.Find(block => block.x == x && block.y == y);
                    if(!blockToDrop.empty){
                        int numLinesToDrop=0;
                        foreach(int clearedLineY in _clearedLines){
                            if(blockToDrop.y > clearedLineY){
                                numLinesToDrop++;
                            }
                        }
                        Block blockBelow = _blocks.Find(block => block.x == blockToDrop.x && block.y == blockToDrop.y - numLinesToDrop);
                        blockBelow.empty = false;
                        blockBelow.color = blockToDrop.color;
                        blockBelow.locked = true;
                        blockToDrop.empty = true;
                        blockToDrop.locked = false;
                    }
                }
            }
        }

        protected virtual bool SoftDrop(Piece p){
            if(!_waitForSoftDrop && !_waitForARE){
                bool softDropLock = false;
                for(int i=0; i < _level.arr; i++){
                    if(!p.MoveDown()){
                        softDropLock = _rotationSystem.SoftDropLock();
                        if(!_hardDrop && !_waitForLockDelay){
                            Game.GetSoundManager().PlaySound(Sounds.SOUND_GROUND);
                        }
                        StartLockDelay(softDropLock);
                        break;
                    }
                }
                _count7 = 0;
                _waitForSoftDrop = true;
                return softDropLock;
            }
            return false;
        }

        protected Level GetLevel(int level){
            if(_level!=null && _level.level == level){
                return _level;
            }

            float gravity = 0.0f;
            foreach(KeyValuePair<int,int> values in _gravities){
                if(level >= values.Key){
                    gravity = values.Value / 256.0f; // Rows Per Frame
                }
            }

            int[] timings = GetTimings(level);

            return new Level(
                level, 
                gravity, 
                timings[ARE_TIMINGS_INDEX], 
                timings[LINE_ARE_TIMINGS_INDEX], 
                timings[DAS_TIMINGS_INDEX], 
                timings[LOCK_TIMINGS_INDEX], 
                timings[LINECLEAR_TIMINGS_INDEX],
                timings[ARR_TIMINGS_INDEX]
            );
        }

        public void SetLevel(int level){
            _level = GetLevel(level);
        }

        public virtual int GetCurrentMaxLevel(){
            return _maxLevel;
        }

        protected int[] GetTimings(int level){
            int[] timings = {};
            foreach(KeyValuePair<int[],int[]> values in _timings){
                if( (level >= values.Key[values.Key.GetLowerBound(0)]) && (level <= values.Key[values.Key.GetUpperBound(0)]) ){
                    timings = values.Value;
                    break;
                }
            }
            return timings;
        }

        protected virtual void SetTimings(){
        }

        protected void GiveNextPiece(){
            _lineARE = 0;
            StartARE();
        }

        protected virtual Piece GetCurrentPiece(){
            if(_history.Count>0){
                return _history[0];
            }
            else{
                _history.Insert(0, _randomizer.GetNextPiece());
                return _history[0];
            }
        }

        protected virtual Piece GetNextPiece(){
            if(_history.Count>1){
                return _history[1];
            }
            else{
                _history.Add(_randomizer.GetNextPiece());
                return _history[1];
            }
        }

        protected virtual Color GetTetrionColor(){
            return new Color();
        }

        public virtual string GetId(){
            return "Mode";
        }
    }
}