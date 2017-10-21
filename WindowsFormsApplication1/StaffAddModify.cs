using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class StaffAddModify : Form
    {
        private decimal id;
        public StaffAddModify()
        {
            InitializeComponent();
        }
        public StaffAddModify(decimal i)
        {
            id = i;
            InitializeComponent();

        }


        private void StaffAddModify_Load(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
