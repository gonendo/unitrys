namespace unitrys{
    public interface IControlsObserver{
        void HandleAction(string actionId, object param=null);
    }
}