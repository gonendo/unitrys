using UnityEngine;
using UnityEngine.InputSystem;
namespace unitrys{
    public class Controls : MonoBehaviour
    {
        public const string UP_ACTION_ID = "move_up";
        public const string DOWN_ACTION_ID = "move_down";
        public const string LEFT_ACTION_ID = "move_left";
        public const string RIGHT_ACTION_ID = "move_right";
        public const string ROTATE_LEFT_ACTION_ID = "rotate_left";
        public const string ROTATE_RIGHT_ACTION_ID = "rotate_right";
        public const string SOFT_DROP_ACTION_ID = "soft_drop";
        public const string HARD_DROP_ACTION_ID = "hard_drop";
        public const string SELECT_ACTION_ID = "select";
        public const string RESTART_ACTION_ID = "restart";
        public const string RETURN_TO_MENU_ACTION_ID = "return";
        public const string INPUT_ACTION_MOVE_UP = "moveUp";

        private float _timer = 0.0f;
        private bool _restarting;
        private InputAction _moveAction;
        private InputAction _restartAction;
        private Vector2 _moveActionValue;
        private IControlsObserver _observer;

        public IControlsObserver observer{
            get{
                return _observer;
            }
            set{
                _observer = value;
            }
        }

        void Awake(){
            _restartAction = GetComponent<PlayerInput>().currentActionMap["Restart"];
            _restartAction.performed += ctx => {
                _timer = 0.0f;
                _restarting = true;
            };
            _restartAction.canceled += ctx => {
                _restarting = false;
                if(Game.GetState() == GameState.IN_GAME){
                    int seconds = Mathf.FloorToInt(_timer % 60);
                    if(seconds >= 1){
                        _observer.HandleAction(RETURN_TO_MENU_ACTION_ID);
                    }
                    else{
                        _observer.HandleAction(RESTART_ACTION_ID);
                    }
                }
            };
            _moveAction = GetComponent<PlayerInput>().currentActionMap["Move"];
            _moveAction.performed += ctx => {
                _moveActionValue = _moveAction.ReadValue<Vector2>();
            };
            _moveAction.canceled += ctx => {
                _moveActionValue = Vector2.zero;
                if(Game.GetState() == GameState.IN_GAME){
                    _observer.HandleAction(INPUT_ACTION_MOVE_UP);
                }
            };
        }
    
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            if(_restarting){
                _timer += Time.deltaTime;
            }
            move();
        }

        private void move(){
            if(_moveActionValue.Equals(Vector2.zero)){
                return;
            }

            float axisX = _moveActionValue.x;
            float axisY = _moveActionValue.y;
            
            int gameState = Game.GetState();

            if(gameState == GameState.MENU){
                if(axisX==1){
                    _observer.HandleAction(RIGHT_ACTION_ID);
                }
                else if(axisX==-1){
                    _observer.HandleAction(LEFT_ACTION_ID);
                }
            }
            else if(gameState == GameState.IN_GAME){
                if(axisX>=0.7f){
                    _observer.HandleAction(RIGHT_ACTION_ID);
                }
                else if(axisX<=-0.7f){
                    _observer.HandleAction(LEFT_ACTION_ID);
                }
                if(axisY==1){
                    _observer.HandleAction(HARD_DROP_ACTION_ID);
                }
                else if(axisY==-1){
                    _observer.HandleAction(SOFT_DROP_ACTION_ID);
                }
            }
        }

        private void OnSelect(){
            if(Game.GetState()==GameState.MENU){
                _observer.HandleAction(SELECT_ACTION_ID);
            }
        }

        private void OnFire1(){
            int gameState = Game.GetState();
            if(gameState == GameState.MENU){
                _observer.HandleAction(SELECT_ACTION_ID);
            }
            else if(gameState == GameState.IN_GAME){
                _observer.HandleAction(ROTATE_LEFT_ACTION_ID);
            }
        }

        private void OnFire2(){
            int gameState = Game.GetState();
            if(gameState == GameState.MENU){
                _observer.HandleAction(SELECT_ACTION_ID);
            }
            else if(gameState == GameState.IN_GAME){
                _observer.HandleAction(ROTATE_RIGHT_ACTION_ID);
            }
        }
    }
}