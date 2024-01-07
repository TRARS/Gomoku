using System.Windows;

namespace Gomoku.UI.Control.CustomControlEx.MenuButtonGroupEx
{
    public partial class cMenuButtonGroup : System.Windows.Controls.Control
    {
        static cMenuButtonGroup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(cMenuButtonGroup), new FrameworkPropertyMetadata(typeof(cMenuButtonGroup)));
        }
    }

    public partial class cMenuButtonGroup
    {
        public object MenuButtons
        {
            get { return (object)GetValue(MenuButtonsProperty); }
            set { SetValue(MenuButtonsProperty, value); }
        }
        public static readonly DependencyProperty MenuButtonsProperty = DependencyProperty.Register(
            name: "MenuButtons",
            propertyType: typeof(object),
            ownerType: typeof(cMenuButtonGroup),
            typeMetadata: new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );
    }
}
