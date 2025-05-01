using GUI.UIFramework;

namespace GUI.Main
{
    public class PopupAView : PopupView<PopupAViewModel>
    {
        // override OnBind для реализации доп логики
        protected override void OnBind(PopupAViewModel model)
        {
            base.OnBind(model);
        }
    }
}