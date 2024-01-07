using System.Windows.Controls;

namespace Gomoku.UI.Control.UserControlEx.ClientEx
{
    public partial class uClient : UserControl
    {
        public uClient()
        {
            InitializeComponent();

            this.DataContext = uClient_viewmodel.Instance;
        }
    }
}
