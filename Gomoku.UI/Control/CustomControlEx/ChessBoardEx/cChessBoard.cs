using System.Windows;

namespace Gomoku.UI.Control.CustomControlEx.ChessBoardEx
{
    public partial class cChessBoard : System.Windows.Controls.Control
    {
        static cChessBoard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(cChessBoard), new FrameworkPropertyMetadata(typeof(cChessBoard)));
        }
    }

    public partial class cChessBoard
    {
        public CornerRadius ChessBoardCornerRadius
        {
            get { return (CornerRadius)GetValue(ChessBoardCornerRadiusProperty); }
            set { SetValue(ChessBoardCornerRadiusProperty, value); }
        }
        public static readonly DependencyProperty ChessBoardCornerRadiusProperty = DependencyProperty.Register(
            name: "ChessBoardCornerRadius",
            propertyType: typeof(CornerRadius),
            ownerType: typeof(cChessBoard),
            typeMetadata: new FrameworkPropertyMetadata(new CornerRadius(5), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );


    }
}
