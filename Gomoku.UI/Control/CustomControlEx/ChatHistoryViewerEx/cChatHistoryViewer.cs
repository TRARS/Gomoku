using System.Windows;

namespace Gomoku.UI.Control.CustomControlEx.ChatHistoryViewerEx
{
    public partial class cChatHistoryViewer : System.Windows.Controls.Control
    {
        static cChatHistoryViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(cChatHistoryViewer), new FrameworkPropertyMetadata(typeof(cChatHistoryViewer)));
        }
    }

    public partial class cChatHistoryViewer
    {
        public object ChatMessages
        {
            get { return (object)GetValue(ChatMessagesProperty); }
            set { SetValue(ChatMessagesProperty, value); }
        }
        public static readonly DependencyProperty ChatMessagesProperty = DependencyProperty.Register(
            name: "ChatMessages",
            propertyType: typeof(object),
            ownerType: typeof(cChatHistoryViewer),
            typeMetadata: new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public object SystemMessages
        {
            get { return (object)GetValue(SystemMessagesProperty); }
            set { SetValue(SystemMessagesProperty, value); }
        }
        public static readonly DependencyProperty SystemMessagesProperty = DependencyProperty.Register(
            name: "SystemMessages",
            propertyType: typeof(object),
            ownerType: typeof(cChatHistoryViewer),
            typeMetadata: new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public object GameMessages
        {
            get { return (object)GetValue(GameMessagesProperty); }
            set { SetValue(GameMessagesProperty, value); }
        }
        public static readonly DependencyProperty GameMessagesProperty = DependencyProperty.Register(
            name: "GameMessages",
            propertyType: typeof(object),
            ownerType: typeof(cChatHistoryViewer),
            typeMetadata: new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public object WinnerMessages
        {
            get { return (object)GetValue(WinnerMessagesProperty); }
            set { SetValue(WinnerMessagesProperty, value); }
        }
        public static readonly DependencyProperty WinnerMessagesProperty = DependencyProperty.Register(
            name: "WinnerMessages",
            propertyType: typeof(object),
            ownerType: typeof(cChatHistoryViewer),
            typeMetadata: new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );
    }
}
