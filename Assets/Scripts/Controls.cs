using UnityEngine;
namespace unitrys{
  public class Controls{
    public const string UP_ACTION_ID = "move_up";
    public const string DOWN_ACTION_ID = "move_down";
    public const string LEFT_ACTION_ID = "move_left";
    public const string RIGHT_ACTION_ID = "move_right";
    public const string ROTATE_LEFT_ACTION_ID = "rotate_left";
    public const string ROTATE_RIGHT_ACTION_ID = "rotate_right";
    public const string SOFT_DROP_ACTION_ID = "soft_drop";
    public const string HARD_DROP_ACTION_ID = "hard_drop";
    public const string SELECT_ACTION_ID = "select";
    private bool _verticalAxisInUse;
    private bool _horizontalAxisInUse;
    private float _timer = 0.0f;
    
    private IControlsObserver _observer;

    public IControlsObserver observer{
      get{
        return _observer;
      }
      set{
        _observer = value;
      }
    }

    public void Update(int gameState){
      switch(gameState){
        case GameState.MENU:
          float vertical = Input.GetAxisRaw("Vertical");
          switch(vertical){
            case 0:
              _verticalAxisInUse = false;
              break;
            case 1:
              if(!_verticalAxisInUse){
                _observer.HandleAction(UP_ACTION_ID);
                _verticalAxisInUse = true;
              }
              break;
            case -1:
              if(!_verticalAxisInUse){
                _observer.HandleAction(DOWN_ACTION_ID);
                _verticalAxisInUse = true;
              }
              break;
          }

          float horizontal = Input.GetAxisRaw("Horizontal");
          switch(horizontal){
            case 0:
              _horizontalAxisInUse = false;
              break;
            case 1:
              if(!_horizontalAxisInUse){
                _observer.HandleAction(RIGHT_ACTION_ID);
                _horizontalAxisInUse = true;
              }
              break;
            case -1:
              if(!_horizontalAxisInUse){
                _observer.HandleAction(LEFT_ACTION_ID);
                _horizontalAxisInUse = true;
              }
              break;
          }

          if(Input.GetButtonDown("Fire1")){
            _observer.HandleAction(ROTATE_LEFT_ACTION_ID);
          }
          else if(Input.GetButtonDown("Fire2")){
            _observer.HandleAction(ROTATE_RIGHT_ACTION_ID);
          }
          else if(Input.GetButtonDown("Submit")){
            _observer.HandleAction(SELECT_ACTION_ID);
          }
          break;
        case GameState.IN_GAME:
          if(Input.GetAxisRaw("Vertical")==1){
            _observer.HandleAction(HARD_DROP_ACTION_ID);
          }
          else if(Input.GetAxisRaw("Vertical")==-1){
            _observer.HandleAction(SOFT_DROP_ACTION_ID);
          }
          else if (Input.GetAxisRaw("Horizontal")==-1){
            _observer.HandleAction(LEFT_ACTION_ID);
          }
          else if(Input.GetAxisRaw("Horizontal")==1){
            _observer.HandleAction(RIGHT_ACTION_ID);
          }
          if(Input.GetButtonDown("Fire1")){
            _observer.HandleAction(ROTATE_LEFT_ACTION_ID);
          }
          else if(Input.GetButtonDown("Fire2")){
            _observer.HandleAction(ROTATE_RIGHT_ACTION_ID);
          }
          if(Input.GetButton("Cancel")){
            _timer += Time.deltaTime;
          }
          else if(Input.GetButtonUp("Cancel")){
            int seconds = Mathf.FloorToInt(_timer % 60);
            _timer = 0.0f;
            _observer.HandleAction("Restart", seconds);
          }
          break;
      }
    }

    public static bool IsActionUp(string actionId){
      switch(actionId){
        case LEFT_ACTION_ID:
        case RIGHT_ACTION_ID:
          return Input.GetButtonUp("Horizontal");
        case HARD_DROP_ACTION_ID:
        case SOFT_DROP_ACTION_ID:
          return Input.GetButtonUp("Vertical");
      }
      return false;
    }
  }
}