using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace unitrys{
    public class Mode : MonoBehaviour
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

        private GameObject _tetrionPrefab;
        private GameObject _blockPrefab;
        private GameObject _tetrion;
        private DebugData _debugData;

        protected IRandomizer _randomizer;
        protected IRotationSystem _rotationSystem;
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
        protected bool _autoShift = false;
        protected bool _hardDrop = false;
        protected bool _firstPiece = true;

        protected int _minClearedLineIndex = -1;
        protected int _lines = 0;
        protected float _count = 0; //counter to know the number of rows to go down
        protected float _count2 = 0; //counter for entry delay
        protected float _count3 = 0; //counter for lock delay
        protected float _count4 = 0; //counter for das
        protected float _count5 = 0; //counter for line clear

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

        // Start is called before the first frame update
        void Start()
        {
            _tetrionPrefab = GetPrefab("Tetrion");
            _blockPrefab = GetPrefab("Block");

            _tetrion = GameObject.Instantiate(_tetrionPrefab, transform);

			for(int i=0; i < GRID_WIDTH; i++){
				for(int j=0; j < GRID_HEIGHT+4; j++){
					GameObject obj = GameObject.Instantiate(_blockPrefab, 
                        new Vector3(
                            _tetrion.transform.Find("Left").transform.position.x + (i+1)*_blockPrefab.transform.localScale.x,
                            _tetrion.transform.Find("Bottom").transform.position.y + (j+1)*_blockPrefab.transform.localScale.y,
                            0),
                        Quaternion.Euler(0f, 180f, 0f), 
                        transform);
                    Block b = obj.GetComponent<Block>();
                    b.texture = _theme.GetTexture();
                    b.x = i;
                    b.y = j;
                    b.empty = true;
                    _blocks.Add(b);
				}
			}

            if(_debugData!=null){
                _debugData.Load(_history, _blocks);
                StartGame(_debugData.level);
            }
            else{
                StartGame(0);
            }
        }

        public void SetDebugData(DebugData data){
            _debugData = data;
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void ProcessUpdate(float deltaTime){
            float delta = deltaTime/TARGET_DELTA_TIME;
            _count+=_level.gravity*delta;
            _count2+=delta;
            _count3+=delta;
            _count4+=delta;
            _count5+=delta;

            if(Controls.IsActionUp(Controls.LEFT_ACTION_ID) || 
                Controls.IsActionUp(Controls.RIGHT_ACTION_ID) || 
                Controls.IsActionUp(Controls.SOFT_DROP_ACTION_ID)){
                ResetDAS();
            }

            Piece piece = GetCurrentPiece();
            if(piece!=null && piece.name == Piece.EMPTY && !_waitForARE){
                RenderNextPiece();
            }

            if(_waitForDAS && (_count4 >= _level.das)){
                _waitForDAS = false;
                _autoShift = true;
            }
            
            if(_waitForLockDelay && (_count3 >= _level.lockDelay)){
                Sounds.play(Sounds.LOCK);
                _waitForLockDelay = false;
                GetCurrentPiece().locked = true;

                for(int j=0; j < 20; j++){
                    piece.MoveDown();
                }
                if(CheckLines()){
                    StartLineClear();
                }

                _count = 1;
            }

            if(_waitForARE && (_count2 >= _level.are+_lineARE)){
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
                int newLevel = _level.level+_clearedLines.Count;
                newLevel = Mathf.Min(newLevel, _maxLevel);
                SetLevel(newLevel);
                gameObject.SendMessageUpwards("DisplayLevel", newLevel, SendMessageOptions.DontRequireReceiver); //TODO
                
                DropLinesNaive();

                _minClearedLineIndex = -1;
                _clearedLines.Clear();
                _waitForLineClear = false;

                if(newLevel == _maxLevel){
                    gameObject.SendMessageUpwards("GameOver", SendMessageOptions.DontRequireReceiver);
                    return;
                }

                _lineARE = _level.lineARE;
                StartARE();
            }

            if(_count >= 1){
                int numRows = !_hardDrop ? 1 : (int)_count;
                for(int i=0; i < numRows; i++){
                    piece = GetCurrentPiece();
                    if(piece!=null && !_waitForLineClear && !_waitForARE){
                        if(!piece.MoveDown()){
                            if(!piece.locked){
                                if(!_waitForLockDelay){
                                    StartLockDelay();
                                }
                            }
                            else if(!_waitForLockDelay){
                                GiveNextPiece();
                            }
                        }
                        else if(piece.locked){
                            for(int j=0; j < 20; j++){
                                piece.MoveDown();
                            }
                            if(CheckLines()){
                                StartLineClear();
                            }
                            break;
                        }
                    }
                }
                _count=0;
            }
        }

        public void MovePiece(string actionId)
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
                        p.MoveLeft();
                        break;
                    case Controls.RIGHT_ACTION_ID:
                        p.MoveRight();
                        break;
                    case Controls.SOFT_DROP_ACTION_ID:
                        if (!_waitForARE && !p.MoveDown())
                        {
                            StartLockDelay(true);
                        }
                        break;
                    case Controls.HARD_DROP_ACTION_ID:
                        if (!_waitForARE && !_hardDrop)
                        {
                            Sounds.play(Sounds.HARD_DROP);
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

        public void RotatePiece(string actionId){
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

        public List<Block> blocks{
            get{
                return _blocks;
            }
        }

        private GameObject GetPrefab(string name){
            string[] guids = AssetDatabase.FindAssets(name, new[] {"Assets/Prefabs"});
            return AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private void StartGame(int level){
            SetLevel(level);
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
                        if ((_level.level < _maxLevel - 1) && string.Format("{0:D3}", _level.level).Substring(1, 2) != "99")
                        {
                            int newLevel = _level.level + 1;
                            SetLevel(newLevel);
                            gameObject.SendMessageUpwards("DisplayLevel", newLevel, SendMessageOptions.DontRequireReceiver); //TODO
                        }
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

        private void StartLockDelay(bool instant=false){
            if(_level.lockDelay > 0){
                _count3 = !instant ? 0 : _level.lockDelay;
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
                Sounds.play(Sounds.CLEAR);
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
            Sounds.play(Sounds.FALL);
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
                timings[LINECLEAR_TIMINGS_INDEX]
            );
        }

        protected void SetLevel(int level){
            _level = GetLevel(level);
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
    }
}