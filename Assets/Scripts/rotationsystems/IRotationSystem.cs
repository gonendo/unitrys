
using System.Collections.Generic;
namespace unitrys{
    public interface IRotationSystem{
        int[] GetInitialState(string pieceName);
        int GetNumberOfRotationStates(string pieceName);
        List<Block> RotateClockWise(List<Block> blocks, Piece piece);
        List<Block> RotateCounterClockWise(List<Block> blocks, Piece piece);
        bool AllowStepReset();
        bool HardDropLock();
        bool SoftDropLock();
    }
}