using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace unitrys{
    public class Tetrions : MonoBehaviour
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
            Debug.Log("animationEnd");
            SendMessageUpwards("RotationEnd", SendMessageOptions.DontRequireReceiver);
        }
    }
}