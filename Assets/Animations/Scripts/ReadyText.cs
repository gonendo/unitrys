using UnityEngine;

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

        public void OnAnimationEnd(){
            gameObject.SendMessageUpwards("ReadyAnimationEnd", SendMessageOptions.DontRequireReceiver);
        }
    }
}