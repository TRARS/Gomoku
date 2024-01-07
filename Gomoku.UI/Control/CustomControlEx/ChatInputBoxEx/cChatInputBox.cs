using System.Windows;
using System.Windows.Media;

namespace Gomoku.UI.Control.CustomControlEx.ChatInputBoxEx
{
    public partial class cChatInputBox : System.Windows.Controls.TextBox
    {
        static cChatInputBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(cChatInputBox), new FrameworkPropertyMetadata(typeof(cChatInputBox)));
        }
    }

    public partial class cChatInputBox
    {
        public new Thickness Margin
        {
            get { return (Thickness)GetValue(MarginProperty); }
            set { SetValue(MarginProperty, value); }
        }
        public new static readonly DependencyProperty MarginProperty = DependencyProperty.Register(
            name: "Margin",
            propertyType: typeof(Thickness),
            ownerType: typeof(cChatInputBox),
            typeMetadata: new FrameworkPropertyMetadata(new Thickness(2), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public CornerRadius BorderCornerRadius
        {
            get { return (CornerRadius)GetValue(BorderCornerRadiusProperty); }
            set { SetValue(BorderCornerRadiusProperty, value); }
        }
        public static readonly DependencyProperty BorderCornerRadiusProperty = DependencyProperty.Register(
            name: "BorderCornerRadius",
            propertyType: typeof(CornerRadius),
            ownerType: typeof(cChatInputBox),
            typeMetadata: new FrameworkPropertyMetadata(new CornerRadius(0), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );


        public bool EnableSendButton
        {
            get { return (bool)GetValue(EnableSendButtonProperty); }
            set { SetValue(EnableSendButtonProperty, value); }
        }
        public static readonly DependencyProperty EnableSendButtonProperty = DependencyProperty.Register(
            name: "EnableSendButton",
            propertyType: typeof(bool),
            ownerType: typeof(cChatInputBox),
            typeMetadata: new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );


        public Thickness InputAreaBorderThickness
        {
            get { return (Thickness)GetValue(InputAreaBorderThicknessProperty); }
            set { SetValue(InputAreaBorderThicknessProperty, value); }
        }
        public static readonly DependencyProperty InputAreaBorderThicknessProperty = DependencyProperty.Register(
            name: "InputAreaBorderThickness",
            propertyType: typeof(Thickness),
            ownerType: typeof(cChatInputBox),
            typeMetadata: new FrameworkPropertyMetadata(new Thickness(1), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public SolidColorBrush InputAreaBorderBrush
        {
            get { return (SolidColorBrush)GetValue(InputAreaBorderBrushProperty); }
            set { SetValue(InputAreaBorderBrushProperty, value); }
        }
        public static readonly DependencyProperty InputAreaBorderBrushProperty = DependencyProperty.Register(
            name: "InputAreaBorderBrush",
            propertyType: typeof(SolidColorBrush),
            ownerType: typeof(cChatInputBox),
            typeMetadata: new FrameworkPropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#343540")), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public SolidColorBrush InputAreaBackground
        {
            get { return (SolidColorBrush)GetValue(InputAreaBackgroundProperty); }
            set { SetValue(InputAreaBackgroundProperty, value); }
        }
        public static readonly DependencyProperty InputAreaBackgroundProperty = DependencyProperty.Register(
            name: "InputAreaBackground",
            propertyType: typeof(SolidColorBrush),
            ownerType: typeof(cChatInputBox),
            typeMetadata: new FrameworkPropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#40414F")), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public CornerRadius InputAreaBorderCornerRadius
        {
            get { return (CornerRadius)GetValue(InputAreaBorderCornerRadiusProperty); }
            set { SetValue(InputAreaBorderCornerRadiusProperty, value); }
        }
        public static readonly DependencyProperty InputAreaBorderCornerRadiusProperty = DependencyProperty.Register(
            name: "InputAreaBorderCornerRadius",
            propertyType: typeof(CornerRadius),
            ownerType: typeof(cChatInputBox),
            typeMetadata: new FrameworkPropertyMetadata(new CornerRadius(2.5), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );
    }
}
