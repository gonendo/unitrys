namespace unitrys{
  public class Level{
    private int _level;
    private float _gravity;
    private int _are;
    private int _lineARE;
    private int _das;
    private int _lockDelay;
    private int _lineClear;

    public int level{
      get{
        return _level;
      }
    }
    public float gravity{
      get{
        return _gravity;
      }
    }
    public int are{
      get{
        return _are;
      }
    }
    public int lineARE{
      get{
        return _lineARE;
      }
    }
    public int das{
      get{
        return _das;
      }
    }
    public int lockDelay{
      get{
        return _lockDelay;
      }
    }
    public int lineClear{
      get{
        return _lineClear;
      }
    }

    public Level(int level, float gravity, int are, int lineARE, int das, int lockDelay, int lineClear){
      _level = level;
      _gravity = gravity;
      _are = are;
      _lineARE = lineARE;
      _das = das;
      _lockDelay = lockDelay;
      _lineClear = lineClear;
    }
  }
}