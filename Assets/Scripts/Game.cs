using UnityEngine;
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
            StartCoroutine("StartNewModeAtNextFrame");
        }

        IEnumerator StartNewModeAtNextFrame(){
            yield return new WaitForEndOfFrame();
            StartNewMode();
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
        }

        public static Mode GetMode(){
            return _mode;
        }
    }
}