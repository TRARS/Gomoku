using System.Windows;

namespace Gomoku.UI.Control.CustomControlEx.MenuButtonEx
{
    public partial class cMenuButton : System.Windows.Controls.Button
    {
        static cMenuButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(cMenuButton), new FrameworkPropertyMetadata(typeof(cMenuButton)));
        }
    }

    public partial class cMenuButton
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            name: "Text",
            propertyType: typeof(string),
            ownerType: typeof(cMenuButton),
            typeMetadata: new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public ButtonType Type
        {
            get { return (ButtonType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }
        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            name: "Type",
            propertyType: typeof(ButtonType),
            ownerType: typeof(cMenuButton),
            typeMetadata: new FrameworkPropertyMetadata(ButtonType.Noamal, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public CornerRadius BorderCornerRadius
        {
            get { return (CornerRadius)GetValue(BorderCornerRadiusProperty); }
            set { SetValue(BorderCornerRadiusProperty, value); }
        }
        public static readonly DependencyProperty BorderCornerRadiusProperty = DependencyProperty.Register(
            name: "BorderCornerRadius",
            propertyType: typeof(CornerRadius),
            ownerType: typeof(cMenuButton),
            typeMetadata: new FrameworkPropertyMetadata(new CornerRadius(1), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public Point ClickPos
        {
            get { return (Point)GetValue(ClickPosProperty); }
            set { SetValue(ClickPosProperty, value); }
        }
        public static readonly DependencyProperty ClickPosProperty = DependencyProperty.Register(
            name: "ClickPos",
            propertyType: typeof(Point),
            ownerType: typeof(cMenuButton),
            typeMetadata: new FrameworkPropertyMetadata(new Point(0, 0), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );
    }
}
