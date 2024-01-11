using Gomoku.Core.Playground.Base;
using System;

namespace Gomoku.Core.Playground
{
    /// <summary>
    /// 联机对战适用
    /// </summary>
    public partial class MultiPlayground : Base.Playground
    {
        private static readonly Lazy<MultiPlayground> lazyObject = new(() => new(PlaygroundType.MultiPlay));
        public static MultiPlayground Instance => lazyObject.Value;

        private MultiPlayground(PlaygroundType flag) : base(flag)
        {

        }
    }
}
