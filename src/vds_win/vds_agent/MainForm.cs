using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace vds_agent
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void stayIcon_Click(object sender, EventArgs e)
        {
            this.WindowState = (this.WindowState == FormWindowState.Minimized) ? FormWindowState.Normal : FormWindowState.Minimized;
        }
    }
}
