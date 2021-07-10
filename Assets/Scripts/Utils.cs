using UnityEngine;
namespace unitrys{
    public class Utils{
        public static void ChangeTetrionColor(GameObject tetrion, Color color){
            for(int i=0; i < 4; i++){
                GameObject child = tetrion.transform.GetChild(i).gameObject;
                MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();
                meshRenderer.material.color = color;
            }
        }
    }
}