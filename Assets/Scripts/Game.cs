using UnityEngine;
using System;
using System.Collections;
using TMPro;

namespace unitrys{
    public class Game : MonoBehaviour
    {
        private GameObject _modeGameObject;
        private GameObject _menuGameObject;
        private GameObject _levelGameObject;
        private static Mode _mode;
        private Controls _controls;
        private TextMeshPro _readyText;
        private TextMeshPro _timeText;
        private TextMeshPro _levelText;
        private ConfigData _config;
        private bool _gameover;
        private bool _rendered;
        private static int _state;
        private static float _time;

        public static Mode GetMode(){
            return _mode;
        }

        public static float GetTime(){
            return _time;
        }

        public static int GetState(){
            return _state;
        }

        void Awake(){
            _controls = GameObject.Find("Controls").GetComponent<Controls>();
            TextAsset textAsset = Resources.Load<TextAsset>("config");
            _config = JsonUtility.FromJson<ConfigData>(textAsset.text);
            _readyText = GameObject.Find("Ready Text").GetComponent<TextMeshPro>();
            _timeText = GameObject.Find("Time Text").GetComponent<TextMeshPro>();
            _levelGameObject = GameObject.Find("Level");
            _levelText = _levelGameObject.transform.Find("Level Text").GetComponent<TextMeshPro>();
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
            if(!_gameover){
                if(!_rendered){
                    _readyText.SetText("");
                }
                _time += Time.deltaTime;
                DisplayTime(_time);
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
            _levelText.SetText("");
            _levelGameObject.SetActive(false);
            _timeText.SetText("");
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

            _levelText.SetText("");
            _levelGameObject.SetActive(false);
            _timeText.SetText("");
            _readyText.GetComponent<Animator>().Play("Ready", -1, 0f);
            _state = GameState.IN_GAME;
        }

        private void StartGame(){
            int startLevel = 0;
            #if UNITY_EDITOR
            if(_config.debug){
                TextAsset textAsset = Resources.Load<TextAsset>("debug");
                DebugData data = JsonUtility.FromJson<DebugData>(textAsset.text);
                _mode.history.Clear();
                data.Load(_mode.history, _mode.blocks);
                startLevel = data.level;
            }
            #endif
            _gameover = false;
            _rendered = false;
            _time = 0;
            DisplayLevel(startLevel);
            _levelGameObject.SetActive(true);
            DisplayTime(_time);
            _mode.StartGame(startLevel);
        }

        private void DisplayTime(float time){
            float minutes = Mathf.FloorToInt(time / 60);
            float seconds = Mathf.FloorToInt(time % 60);
            float milliSeconds = (time % 1) * 1000;
            _timeText.SetText(string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliSeconds));
        }

        private void DisplayLevel(int level){
            string lvl = level.ToString();
            _levelText.SetText(lvl.PadLeft(3)+"\n"+_mode.GetCurrentMaxLevel());
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