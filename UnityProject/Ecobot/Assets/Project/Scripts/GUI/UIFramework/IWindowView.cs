namespace GUI.UIFramework
{
    public interface IWindowView
    {
        public void Bind(WindowController controller);
        public void Close();
    }
}