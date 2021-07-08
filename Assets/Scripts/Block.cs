using UnityEngine;

namespace unitrys{
    public class Block : MonoBehaviour
    {
        public int x; //rightwards
        public int y; //upwards
        private Color _color;
        private bool _started=false;
        private bool _locked;
        private bool _cleared;
        private bool _empty;
        private MeshRenderer _meshRenderer;
        private Texture _texture;

        void Awake(){
            _meshRenderer = GetComponent<MeshRenderer>();
            _texture = _meshRenderer.material.GetTexture("_MainTex");
            _started = true;
            color = _color;
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        public bool locked{
            get{
                return _locked;
            }
            set{
                _cleared = false;
                _locked = value;
            }
        }

        public bool empty{
            get{
                return _empty;
            }
            set{
                _cleared = false;
                _empty = value;
                if(_empty || y<=Game.GetMode().GRID_HEIGHT){
                    gameObject.SetActive(!_empty);
                }
            }
        }

        public void Render(){
            color = _color;
        }

        public Color color{
            get{
                return _color;
            }
            set{
                _color = value;
                if(_started){
                    if(_locked && !_cleared && !_empty){
                        Color darkerColor = _color * 0.5f;
                        darkerColor.a = 1;
                        _meshRenderer.material.color = darkerColor;
                    }
                    else if(!_meshRenderer.material.color.Equals(value)){
                        _meshRenderer.material.color = _color;
                    }
                }
            }
        }

        public void Clear(){
            _meshRenderer.material.SetTexture("_MainTex", null);
            _cleared = true;
            color = Color.white;
        }

        public void RestoreTexture(){
            _meshRenderer.material.SetTexture("_MainTex", _texture);
        }
    }
}