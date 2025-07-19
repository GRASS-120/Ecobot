using GUI.UIFramework;

namespace GUI.Main
{
    public class PopupAView : PopupView<PopupAController>
    {
        // override OnBind для реализации доп логики
        protected override void OnOpen()
        {
            base.OnOpen();
        }
    }
}