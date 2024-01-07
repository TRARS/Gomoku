using System.Windows;

namespace Gomoku.UI.Control.CustomControlEx.ChatAvatarEx
{
    public class cChatAvatar : System.Windows.Controls.Control
    {
        static cChatAvatar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(cChatAvatar), new FrameworkPropertyMetadata(typeof(cChatAvatar)));
        }
    }
}
