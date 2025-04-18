using GUI.UIFramework.Base;

namespace GUI.Main
{
    public class PopupABinder : PopupBinder<PopupAViewModel>
    {
        // override OnBind для реализации доп логики
        protected override void OnBind(PopupAViewModel model)
        {
            base.OnBind(model);
        }
    }
}