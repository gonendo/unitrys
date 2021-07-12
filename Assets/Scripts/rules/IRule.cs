namespace unitrys{
    public interface IRule{
        void IncreaseLevel(int increase, bool clearingLines);
        bool CheckGameOver();
    }
}