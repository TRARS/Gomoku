using Gomoku.Core.Helper.Base;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Gomoku.UI.Control.UserControlEx.TitleBarEx
{
    public partial class uTitleBar : UserControl
    {
        public uTitleBar()
        {
            InitializeComponent();
            this.Title = DefualtTitle();
        }
    }

    public partial class uTitleBar
    {
        #region 缺省按钮的
        private void ResetPosButton_Click(object sender, RoutedEventArgs e)
        {
            Mediator.Instance.Send<MessagePacket<Vector>>(MessageType.WindowPosReset, new MessagePacket<Vector>(new Vector(0, 0)));
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Mediator.Instance.Send<object>(MessageType.WindowMinimize, "");
        }
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            Mediator.Instance.Send<object>(MessageType.WindowMaximize, "");
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Mediator.Instance.Send<object>(MessageType.WindowClose, "");
        }
        #endregion
    }

    public partial class uTitleBar
    {
        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            name: "IsActive",
            propertyType: typeof(bool),
            ownerType: typeof(uTitleBar),
            typeMetadata: new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            name: "Title",
            propertyType: typeof(string),
            ownerType: typeof(uTitleBar),
            typeMetadata: new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );
    }

    public partial class uTitleBar
    {
        private string DefualtTitle()
        {
            var AssemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
            var LastWriteTime = $"{System.IO.File.GetLastWriteTime(this.GetType().Assembly.Location):yyyy-MM-dd HH:mm:ss}";

            return $"{AssemblyName} ({LastWriteTime})";
        }
    }
}
