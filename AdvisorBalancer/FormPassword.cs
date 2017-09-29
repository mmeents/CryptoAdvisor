using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Advisor {
  public partial class FormPassword : Form {
    public FormPassword() {
      InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e) {
      if (textBox1.Text == "") {
        this.DialogResult = DialogResult.None;
      }
    }

    public string GetPassword() {
      return textBox1.Text;
    }
  }
}
