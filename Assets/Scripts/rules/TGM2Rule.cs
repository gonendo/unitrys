using UnityEngine;
namespace unitrys{
    public class TGM2Rule : IRule{
        private Mode _mode;
        
        public TGM2Rule(Mode mode){
            _mode = mode;
        }

        public void IncreaseLevel(int increase, bool clearingLines){
            if(clearingLines || string.Format("{0:D3}", _mode.level).Substring(1, 2) != "99"){
                int newLevel = Mathf.Min(_mode.level + increase, _mode.maxLevel);
                _mode.SetLevel(newLevel);
                _mode.gameObject.SendMessageUpwards("DisplayLevel", newLevel, SendMessageOptions.DontRequireReceiver);
            }
        }

        public bool CheckGameOver(){
            if(_mode is DeathMode && _mode.level >= 500){
                float time = Game.GetTime();
                float minutes = Mathf.FloorToInt(time / 60);
                float seconds = Mathf.FloorToInt(time % 60);
                float milliSeconds = (time % 1) * 1000;
                if(!(minutes <= 3 && (seconds < 25 || milliSeconds == 0))){
                    _mode.SetLevel(500);
                    _mode.gameObject.SendMessageUpwards("DisplayLevel", 500, SendMessageOptions.DontRequireReceiver);
                    return true;
                }
            }
            return _mode.level == _mode.maxLevel;
        }
    }
}