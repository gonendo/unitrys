namespace unitrys{
  public class Sounds{
    public const string SOUND_MODE_SELECT = "SEI_mode_ok";
    public const string SOUND_READY = "ready";
    public const string SOUND_GO = "go";
    public const string SOUND_FALL = "SEB_fall";
    public const string SOUND_LOCK = "secchi01";
    public const string SOUND_GROUND = "secchi02";
    public const string SOUND_CLEAR = "SEB_disappear";
    public const string SOUND_TETRIS = "s_hakushu";
    public const string SOUND_GAMEOVER = "SEP_gameover";

    public static string GetPieceSoundId(string pieceName){
        switch(pieceName){
            case Piece.O:
                return "SEB_mino1";
            case Piece.J:
                return "SEB_mino2";
            case Piece.L:
                return "SEB_mino3";
            case Piece.Z:
                return "SEB_mino4";
            case Piece.S:
                return "SEB_mino5";
            case Piece.T:
                return "SEB_mino6";
            case Piece.I:
                return "SEB_mino7";
        }
        return null;
    }
  }
}