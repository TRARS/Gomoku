using System.Windows;

namespace Gomoku.UI.Control.CustomControlEx.StoneButtonEx
{
    public partial class cStoneButton : System.Windows.Controls.Button
    {
        static cStoneButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(cStoneButton), new FrameworkPropertyMetadata(typeof(cStoneButton)));
        }
    }

    public partial class cStoneButton
    {
        public ButtonType Type
        {
            get { return (ButtonType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }
        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            name: "Type",
            propertyType: typeof(ButtonType),
            ownerType: typeof(cStoneButton),
            typeMetadata: new FrameworkPropertyMetadata(ButtonType.BlackStone, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public double Diameter
        {
            get { return (double)GetValue(DiameterProperty); }
            set { SetValue(DiameterProperty, value); }
        }
        public static readonly DependencyProperty DiameterProperty = DependencyProperty.Register(
            name: "Diameter",
            propertyType: typeof(double),
            ownerType: typeof(cStoneButton),
            typeMetadata: new FrameworkPropertyMetadata(31D, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public bool EnableHighLight
        {
            get { return (bool)GetValue(EnableHighLightProperty); }
            set { SetValue(EnableHighLightProperty, value); }
        }
        public static readonly DependencyProperty EnableHighLightProperty = DependencyProperty.Register(
            name: "EnableHighLight",
            propertyType: typeof(bool),
            ownerType: typeof(cStoneButton),
            typeMetadata: new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );
    }
}
