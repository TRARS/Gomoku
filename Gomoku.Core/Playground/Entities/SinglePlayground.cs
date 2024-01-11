using Gomoku.Core.Playground.Base;
using System;

namespace Gomoku.Core.Playground
{
    /// <summary>
    /// 本机对战适用
    /// </summary>
    public partial class SinglePlayground : Base.Playground
    {
        private static readonly Lazy<SinglePlayground> lazyObject = new(() => new(PlaygroundType.SinglePlay));
        public static SinglePlayground Instance => lazyObject.Value;

        private SinglePlayground(PlaygroundType flag) : base(flag)
        {

        }
    }
}
