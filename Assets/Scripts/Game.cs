using UnityEngine;
using System;
using System.Collections;
using TMPro;

namespace unitrys{
    public class Game : MonoBehaviour
    {
        private GameObject _modeGameObject;
        private static Mode _mode;
        private Controls _controls;
        private static bool _gameover;
        private const bool DEBUG = false;

        public static Mode GetMode(){
            return _mode;
        }

        // Start is called before the first frame update
        void Start()
        {
            _gameover = true;
            StartNewMode();
        }

        // Update is called once per frame
        void Update()
        {
            _controls.Update();
            if(!_gameover){
                _mode.ProcessUpdate(Time.deltaTime);
                foreach(Block block in _mode.blocks){
                    block.Render();
                }
            }
        }

        public void Restart(){
            _gameover = true;
            StopAllCoroutines();
            Action action = StartNewMode;
            IEnumerator coroutine = StartAtNextFrame(action);
            StartCoroutine(coroutine);
        }

        public void GameOver(){
            _gameover = true;
            Action action = PlayGameOverAnimation;
            IEnumerator coroutine = StartAtNextFrame(action);
            StartCoroutine(coroutine);
        }

        private void StartNewMode(){
            if(_modeGameObject!=null){
                Destroy(_modeGameObject);
            }
            _modeGameObject = new GameObject("Mode");
            _modeGameObject.transform.SetParent(transform);
            _modeGameObject.AddComponent<MasterMode>();
            _mode = _modeGameObject.GetComponent<MasterMode>();
            _controls = new Controls(_mode);

            Action action = StartReadyGO;
            IEnumerator coroutine = StartAtNextFrame(action);
            StartCoroutine(coroutine);
        }

        private void StartReadyGO(){
            StartCoroutine("ReadyGo");
        }

        private void StartGame(){
            int startLevel = 0;
            if(DEBUG){
                TextAsset textAsset = Resources.Load<TextAsset>("debug");
                DebugData data = JsonUtility.FromJson<DebugData>(textAsset.text);
                data.Load(_mode.history, _mode.blocks);
                startLevel = data.level;
            }
            _gameover = false;
            _mode.StartGame(startLevel);
        }

        private void PlayGameOverAnimation(){
            StartCoroutine("GameOverAnimation");
        }

        IEnumerator StartAtNextFrame(Action action){
            yield return new WaitForEndOfFrame();
            action();
        }

        IEnumerator ReadyGo(){
            GameObject textObject = GameObject.Find("Ready Text");
            TextMeshPro tmp = textObject.GetComponent<TextMeshPro>();
            tmp.SetText("READY");
            yield return new WaitForSeconds(0.8f);
            tmp.SetText("GO !");
            yield return new WaitForSeconds(0.8f);
            tmp.SetText("");
            StartGame();
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