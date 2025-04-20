namespace GUI.UIFramework.Base
{
    public interface IWindowView
    {
        void Bind(WindowViewModel viewModel);
        void Close();
    }
}