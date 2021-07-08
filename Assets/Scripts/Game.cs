using UnityEngine;
using System;
using System.Collections;

namespace unitrys{
    public class Game : MonoBehaviour
    {
        private GameObject _modeGameObject;
        private static Mode _mode;
        private Controls _controls;
        private static bool _gameover;
        private const bool DEBUG = false;

        // Start is called before the first frame update
        void Start()
        {
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

        IEnumerator StartAtNextFrame(Action action){
            yield return new WaitForEndOfFrame();
            action();
        }

        public void StartNewMode(){
            if(_modeGameObject!=null){
                Destroy(_modeGameObject);
            }
            _modeGameObject = new GameObject("Mode");
            _modeGameObject.transform.SetParent(transform);
            _modeGameObject.AddComponent<MasterMode>();
            _mode = _modeGameObject.GetComponent<MasterMode>();
            if(DEBUG){
                TextAsset textAsset = Resources.Load<TextAsset>("debug");
                _mode.SetDebugData(JsonUtility.FromJson<DebugData>(textAsset.text));
            }
            _controls = new Controls(_mode);
            _gameover = false;
        }

        public void GameOver(){
            _gameover = true;
            Action action = PlayGameOverAnimation;
            IEnumerator coroutine = StartAtNextFrame(action);
            StartCoroutine(coroutine);
        }

        private void PlayGameOverAnimation(){
            StartCoroutine("GameOverAnimation");
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

        public static Mode GetMode(){
            return _mode;
        }
    }
}