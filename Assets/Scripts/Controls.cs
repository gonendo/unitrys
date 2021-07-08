using UnityEngine;
namespace unitrys{
  public class Controls{
    public const string LEFT_ACTION_ID = "move_left";
    public const string RIGHT_ACTION_ID = "move_right";
    public const string ROTATE_LEFT_ACTION_ID = "rotate_left";
    public const string ROTATE_RIGHT_ACTION_ID = "rotate_right";
    public const string SOFT_DROP_ACTION_ID = "soft_drop";
    public const string HARD_DROP_ACTION_ID = "hard_drop";
    
    private Mode _mode;

    public Controls(Mode mode){
      _mode = mode;
    }

    public void Update(){
      if(Input.GetAxisRaw("Vertical")==1){
        _mode.MovePiece(HARD_DROP_ACTION_ID);
      }
      else if(Input.GetAxisRaw("Vertical")==-1){
        _mode.MovePiece(SOFT_DROP_ACTION_ID);
      }
      else if (Input.GetAxisRaw("Horizontal")==-1){
        _mode.MovePiece(LEFT_ACTION_ID);
      }
      else if(Input.GetAxisRaw("Horizontal")==1){
        _mode.MovePiece(RIGHT_ACTION_ID);
      }
      if(Input.GetButtonDown("Fire1")){
        _mode.RotatePiece(ROTATE_LEFT_ACTION_ID);
      }
      else if(Input.GetButtonDown("Fire2")){
        _mode.RotatePiece(ROTATE_RIGHT_ACTION_ID);
      }
      else if(Input.GetButtonUp("Cancel")){
        _mode.SendMessageUpwards("Restart", SendMessageOptions.DontRequireReceiver);
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