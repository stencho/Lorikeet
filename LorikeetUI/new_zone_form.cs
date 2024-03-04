using LorikeetLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LorikeetUI {
    public partial class new_zone_form : Form {
        public new_zone_form() {
            InitializeComponent();
        }

        private void new_zone_form_Load(object sender, EventArgs e) {

        }

        private void button1_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.OK;
            
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
