using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace Gomoku.UI.Control.CustomControlEx.ServerStatusViewerEx
{
    public partial class cServerStatusViewer : System.Windows.Controls.Control
    {
        static cServerStatusViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(cServerStatusViewer), new FrameworkPropertyMetadata(typeof(cServerStatusViewer)));
        }
    }

    public partial class cServerStatusViewer
    {
        public CornerRadius BackgroundCornerRadius
        {
            get { return (CornerRadius)GetValue(BackgroundCornerRadiusProperty); }
            set { SetValue(BackgroundCornerRadiusProperty, value); }
        }
        public static readonly DependencyProperty BackgroundCornerRadiusProperty = DependencyProperty.Register(
            name: "BackgroundCornerRadius",
            propertyType: typeof(CornerRadius),
            ownerType: typeof(cServerStatusViewer),
            typeMetadata: new FrameworkPropertyMetadata(new CornerRadius(0), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );
    }
}
