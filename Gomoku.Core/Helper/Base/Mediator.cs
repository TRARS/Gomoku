using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Text;
using System.Windows.Media.Media3D;

namespace Gomoku.Core.Helper.Base
{
    // 
    public enum MessageType
    {
        WindowClose = 0,
        WindowMinimize,
        WindowMaximize,
        WindowPosReset,           //窗体位置恢复至左上角
    }

    // 包装类
    public record MessagePacket<T>(T message);

    // 限制为单例
    public sealed partial class Mediator
    {
        private static readonly Lazy<Mediator> lazyObject = new(() => new Mediator());
        public static Mediator Instance => lazyObject.Value;

        private Mediator() { }
    }

    // 注册&发送
    public sealed partial class Mediator
    {
        private Mediator _recipient => this;

        public void Register<T>(MessageType token, MessageHandler<object, T> handler) where T : class
        {
            StrongReferenceMessenger.Default.Register<T, string>(_recipient, Enum.GetName(typeof(MessageType), token)!, handler);
        }

        public void Send<T>(MessageType token, T args) where T : class
        {
            StrongReferenceMessenger.Default.Send(args, Enum.GetName(typeof(MessageType), token)!);
        }
    }
}
