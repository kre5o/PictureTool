using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RP3_Projekt
{
    public partial class Pattern : Form
    {
        public int nRows, nColls;

        private void numRows_ValueChanged(object sender, EventArgs e)
        {
            nRows = (int)numRows.Value;
        }

        private void numColl_ValueChanged(object sender, EventArgs e)
        {
            nColls = (int)numColl.Value;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        public Pattern()
        {
            InitializeComponent();
            nRows = (int) numRows.Value;
            nColls = (int)numColl.Value;
        }
    }
}
