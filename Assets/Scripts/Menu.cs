using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace unitrys{
    public class Menu : MonoBehaviour, IControlsObserver
    {
        private bool _rotating;
        private GameObject _tetrionsParent;
        private List<MenuEntry> _menuEntries;
        private int _count;
        private int _entryIndex;
        private List<GameObject> _tetrionsObjects;

        void Awake(){
            _tetrionsParent = GameObject.Find("Tetrions");
            _tetrionsObjects = new List<GameObject>();
            for(int i=0; i < 4; i++){
                _tetrionsObjects.Add(_tetrionsParent.transform.GetChild(i).gameObject);
            }
            _menuEntries = new List<MenuEntry>();
            _menuEntries.Add(new MenuEntry(MasterMode.id, MasterMode.TETRION_COLOR, "StartNewMode", MasterMode.id));
            _menuEntries.Add(new MenuEntry(DeathMode.id, DeathMode.TETRION_COLOR, "StartNewMode", DeathMode.id));
        }

        // Start is called before the first frame update
        void Start()
        {
            int entryIndex=0;
            int i=0;
            while(i < 4){
                MenuEntry entry = _menuEntries[entryIndex];
                GameObject tetrion = _tetrionsObjects[i];
                Color color;
                ColorUtility.TryParseHtmlString(entry.tetrionColor, out color);
                Utils.ChangeTetrionColor(tetrion, color);
                TextMeshPro tmp = tetrion.transform.Find("Text").GetComponent<TextMeshPro>();
                tmp.SetText(entry.modeName);
                i++;
                entryIndex++;
                if(entryIndex > _menuEntries.Count - 1){
                    entryIndex = 0;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void HandleAction(string actionId, object param=null){
            if(_rotating){
                return;
            }
            switch(actionId){
                case Controls.LEFT_ACTION_ID:
                    _rotating = true;
                    _tetrionsParent.GetComponent<Animator>().Play("MenuRotateMinus90", -1, 0);
                    _count--;
                    break;
                case Controls.RIGHT_ACTION_ID:
                    _rotating = true;
                    _tetrionsParent.GetComponent<Animator>().Play("MenuRotate90", -1, 0);
                    _count++;
                    break;
                case Controls.SELECT_ACTION_ID:
                    Game.GetSoundManager().PlaySound(Sounds.SOUND_MODE_SELECT);
                    MenuEntry entry = _menuEntries[_entryIndex];
                    StartCoroutine(ModeSelectSoundCheckEnd(entry));
                    break;
            }

            if(_count < 0){
                _entryIndex = _menuEntries.Count - Mathf.Abs(_count);
                if(_entryIndex < 0){
                    _entryIndex = _menuEntries.Count-1;
                    _count = -1;
                }
            }
            else{
                _entryIndex = _count;
                if(_entryIndex > _menuEntries.Count-1){
                    _entryIndex = 0;
                    _count = 0;
                }
            }
        }

        IEnumerator ModeSelectSoundCheckEnd(MenuEntry entry){
            while(Game.GetSoundManager().IsPlaying(Sounds.SOUND_MODE_SELECT)){
                yield return null;
            }
            SendMessageUpwards(entry.message, entry.messageParam, SendMessageOptions.DontRequireReceiver);
        }

        public void RotationEnd(){
            _rotating = false;
        }
    }

    public class MenuEntry{
        private string _modeName;
        private string _tetrionColor;
        private string _message;
        private object _messageParam;
        public MenuEntry(string modeName, string tetrionColor, string message, object messageParam){
            _modeName = modeName;
            _tetrionColor = tetrionColor;
            _message = message;
            _messageParam = messageParam;
        }
        public string modeName{
            get{
                return _modeName;
            }
        }
        public string tetrionColor{
            get{
                return _tetrionColor;
            }
        }
        public string message{
            get{
                return _message;
            }
        }
        public object messageParam{
            get{
                return _messageParam;
            }
        }
    }
}
