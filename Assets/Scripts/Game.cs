using UnityEngine;
using System;
using System.Collections;
using TMPro;

namespace unitrys{
    public class Game : MonoBehaviour
    {
        private GameObject _modeGameObject;
        private GameObject _menuGameObject;
        private static Mode _mode;
        private Controls _controls;
        private TextMeshPro _readyText;
        private ConfigData _config;
        private bool _gameover;
        private bool _rendered;
        private int _state;

        public static Mode GetMode(){
            return _mode;
        }

        void Awake(){
            _controls = new Controls();
            TextAsset textAsset = Resources.Load<TextAsset>("config");
            _config = JsonUtility.FromJson<ConfigData>(textAsset.text);
            _readyText = GameObject.Find("Ready Text").GetComponent<TextMeshPro>();
            _gameover = true;
            _rendered = false;
        }

        // Start is called before the first frame update
        void Start()
        {
            DisplayMenu();
        }

        // Update is called once per frame
        void Update()
        {
            _controls.Update(_state);
            if(!_gameover){
                if(!_rendered){
                    _readyText.SetText("");
                }
                _mode.ProcessUpdate(Time.deltaTime);
                foreach(Block block in _mode.blocks){
                    block.Render();
                }
                _rendered = true;
            }
        }

        public void Restart(){
            _gameover = true;
            StopAllCoroutines();
            Action<string> action = StartNewMode;
            StartCoroutine(StartAtNextFrame(action, _mode.GetId()));
        }

        public void GameOver(){
            _gameover = true;
            Action<string> action = PlayGameOverAnimation;
            StartCoroutine(StartAtNextFrame(action, "GameOverAnimation"));
        }

        public void ReadyAnimationEnd(){
            StartGame();
        }

        public void DisplayMenu(){
            _gameover = true;
            StopAllCoroutines();
            if(_modeGameObject!=null){
                Destroy(_modeGameObject);
            }

            GameObject menuPrefab = Resources.Load<GameObject>("Prefabs/Menu");
            _menuGameObject = GameObject.Instantiate(menuPrefab, transform);
            _controls.observer = _menuGameObject.GetComponent<Menu>();
            _state = GameState.MENU;
        }

        public void StartNewMode(string modeId){
            if(_menuGameObject!=null){
                Destroy(_menuGameObject);
            }
            if(_modeGameObject!=null){
                Destroy(_modeGameObject);
            }

            _modeGameObject = new GameObject("Mode");
            _modeGameObject.transform.SetParent(transform);

            switch(modeId){
                case MasterMode.id:
                    _modeGameObject.AddComponent<MasterMode>();
                    _mode = _modeGameObject.GetComponent<MasterMode>();
                    break;
                case DeathMode.id:
                    _modeGameObject.AddComponent<DeathMode>();
                    _mode = _modeGameObject.GetComponent<DeathMode>();
                    break;
            }
            
            _controls.observer = _mode;

            _readyText.GetComponent<Animator>().Play("Ready", -1, 0f);
            _state = GameState.IN_GAME;
        }

        private void StartGame(){
            int startLevel = 0;
            #if UNITY_EDITOR
            if(_config.debug){
                TextAsset textAsset = Resources.Load<TextAsset>("debug");
                DebugData data = JsonUtility.FromJson<DebugData>(textAsset.text);
                data.Load(_mode.history, _mode.blocks);
                startLevel = data.level;
            }
            #endif
            _gameover = false;
            _rendered = false;
            _mode.StartGame(startLevel);
        }

        private void PlayGameOverAnimation(string coroutine){
            StartCoroutine(coroutine);
        }

        IEnumerator StartAtNextFrame(Action<string> action, string param){
            yield return new WaitForEndOfFrame();
            action(param);
        }

        IEnumerator GameOverAnimation(){
            for(int i=0; i <= _mode.GRID_HEIGHT+1; i++){
                yield return new WaitForSeconds(0.2f);
                for (int j = 0; j < _mode.GRID_WIDTH; j++)
                {
                    Block blockBelow = _mode.blocks.Find(b => b.x == j && b.y == i - 1);
                    if (blockBelow != null)
                    {
                        blockBelow.empty = true;
                    }
                    Block block = _mode.blocks.Find(b => b.x == j && b.y == i);
                    if (!block.empty)
                    {
                        block.ResetColor();
                    }
                }
			}
        }

        
    }
}