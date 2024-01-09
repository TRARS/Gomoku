using System;


namespace Gomoku.UI.Control.UserControlEx.ClientEx
{
    // 单例
    partial class uClient_viewmodel
    {
        private static readonly Lazy<uClient_viewmodel> lazyObject = new(() => new uClient_viewmodel());
        public static uClient_viewmodel Instance => lazyObject.Value;

        private uClient_viewmodel()
        {
            _ = new uClient_controller(this);
        }
    }

    // 按钮组VM
    partial class uClient_viewmodel
    {
        public MenuButtonGroupViewModel SinglePlayMenuButtonGroupViewModel { get; set; }
        public MenuButtonGroupViewModel MultiPlayMenuButtonGroupViewModel { get; set; }
    }

    // 棋盘交互VM
    partial class uClient_viewmodel
    {
        public ChessBoardViewModel ChessBoardViewModel { get; set; }
    }

    // 聊天交互VM
    partial class uClient_viewmodel
    {
        public ChatRoomJoinerViewModel ChatRoomJoinerViewModel { get; set; }
        public ChatHistoryViewModel ChatHistoryViewModel { get; set; }
        public ChatInputViewmodel ChatInputViewModel { get; set; }
        public ChatServerViewModel ChatServerViewModel { get; set; }
        public ChatClientViewModel ChatClientViewModel { get; set; }
    }
}
