using UnityEngine.SceneManagement;
using UnityEngine;

namespace unitrys{
    public class Main : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            SceneManager.LoadScene("Game", LoadSceneMode.Additive);
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}