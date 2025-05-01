namespace GUI.UIFramework
{
    public interface IWindowView
    {
        /// <summary>
        /// Привязывает модель представления к окну.
        /// </summary>
        /// <param name="viewModel">Модель представления окна.</param>
        void Bind(WindowViewModel viewModel);
        void Close();
    }
}