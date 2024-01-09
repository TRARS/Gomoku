using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace Gomoku.UI.Control.CustomControlEx.ConfirmWindowEx
{
    public partial class cConfirmWindow : System.Windows.Controls.Control
    {
        static cConfirmWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(cConfirmWindow), new FrameworkPropertyMetadata(typeof(cConfirmWindow)));
        }
    }

    public partial class cConfirmWindow
    {
        public CornerRadius BorderCornerRadius
        {
            get { return (CornerRadius)GetValue(BorderCornerRadiusProperty); }
            set { SetValue(BorderCornerRadiusProperty, value); }
        }
        public static readonly DependencyProperty BorderCornerRadiusProperty = DependencyProperty.Register(
            name: "BorderCornerRadius",
            propertyType: typeof(CornerRadius),
            ownerType: typeof(cConfirmWindow),
            typeMetadata: new FrameworkPropertyMetadata(new CornerRadius(0), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public string FirstName
        {
            get { return (string)GetValue(FirstNameProperty); }
            set { SetValue(FirstNameProperty, value); }
        }
        public static readonly DependencyProperty FirstNameProperty = DependencyProperty.Register(
            name: "FirstName",
            propertyType: typeof(string),
            ownerType: typeof(cConfirmWindow),
            typeMetadata: new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public string FirstValue
        {
            get { return (string)GetValue(FirstValueProperty); }
            set { SetValue(FirstValueProperty, value); }
        }
        public static readonly DependencyProperty FirstValueProperty = DependencyProperty.Register(
            name: "FirstValue",
            propertyType: typeof(string),
            ownerType: typeof(cConfirmWindow),
            typeMetadata: new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public string SecondName
        {
            get { return (string)GetValue(SecondNameProperty); }
            set { SetValue(SecondNameProperty, value); }
        }
        public static readonly DependencyProperty SecondNameProperty = DependencyProperty.Register(
            name: "SecondName",
            propertyType: typeof(string),
            ownerType: typeof(cConfirmWindow),
            typeMetadata: new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public string SecondValue
        {
            get { return (string)GetValue(SecondValueProperty); }
            set { SetValue(SecondValueProperty, value); }
        }
        public static readonly DependencyProperty SecondValueProperty = DependencyProperty.Register(
            name: "SecondValue",
            propertyType: typeof(string),
            ownerType: typeof(cConfirmWindow),
            typeMetadata: new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public string ConfirmText
        {
            get { return (string)GetValue(ConfirmTextProperty); }
            set { SetValue(ConfirmTextProperty, value); }
        }
        public static readonly DependencyProperty ConfirmTextProperty = DependencyProperty.Register(
            name: "ConfirmText",
            propertyType: typeof(string),
            ownerType: typeof(cConfirmWindow),
            typeMetadata: new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public AsyncRelayCommand<object?> ConfirmCommand
        {
            get { return (AsyncRelayCommand<object?>)GetValue(ConfirmCommandProperty); }
            set { SetValue(ConfirmCommandProperty, value); }
        }
        public static readonly DependencyProperty ConfirmCommandProperty = DependencyProperty.Register(
            name: "ConfirmCommand",
            propertyType: typeof(AsyncRelayCommand<object?>),
            ownerType: typeof(cConfirmWindow),
            typeMetadata: new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public AsyncRelayCommand<object?> CancelCommand
        {
            get { return (AsyncRelayCommand<object?>)GetValue(CancelCommandProperty); }
            set { SetValue(CancelCommandProperty, value); }
        }
        public static readonly DependencyProperty CancelCommandProperty = DependencyProperty.Register(
            name: "CancelCommand",
            propertyType: typeof(AsyncRelayCommand<object?>),
            ownerType: typeof(cConfirmWindow),
            typeMetadata: new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );
    }
}
