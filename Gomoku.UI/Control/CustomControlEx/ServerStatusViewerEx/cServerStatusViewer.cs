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
}
