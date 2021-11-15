using UnityEngine;
using TMPro;

namespace unitrys{
    public class ReadyText : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void SetReadyText(){
            TextMeshPro text = gameObject.GetComponent<TextMeshPro>();
            text.SetText("READY");
            Game.GetSoundManager().PlaySound(Sounds.SOUND_READY);
        }

        public void SetGoText(){
            TextMeshPro text = gameObject.GetComponent<TextMeshPro>();
            text.SetText("GO !");
            Game.GetSoundManager().PlaySound(Sounds.SOUND_GO);
        }

        public void OnAnimationEnd(){
            gameObject.SendMessageUpwards("ReadyAnimationEnd", SendMessageOptions.DontRequireReceiver);
        }
    }
}