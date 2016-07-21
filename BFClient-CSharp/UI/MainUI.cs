using System.Collections;
using System.Windows.Forms;

namespace BFClient_CSharp.UI
{
    public partial class MainUI : Form
    {
        private ArrayList changeList = new ArrayList();

        public MainUI()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            Application.Exit();
        }
    }
}
