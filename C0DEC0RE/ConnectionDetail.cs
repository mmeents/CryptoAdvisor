using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace C0DEC0RE {

  public partial class ConnectionDetail : Form {
    public DbConnectionInfo dbCI = null; 
    public ConnectionDetail(){
      InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e){
      if (dbCI != null){
        dbCI.ConnectionName = textBox1.Text;
        dbCI.ServerName = textBox2.Text;
        dbCI.InitialCatalog = textBox3.Text;
        dbCI.UserName = textBox4.Text;
        dbCI.Password = textBox5.Text;        
      }
    }

    private void ConnectionDetail_Shown(object sender, EventArgs e){

      if (dbCI != null) {
        textBox1.Text = dbCI.ConnectionName;
        textBox2.Text = dbCI.ServerName;
        textBox3.Text = dbCI.InitialCatalog;
        textBox4.Text = dbCI.UserName;
        textBox5.Text = dbCI.Password;
      }

    }
  }
}
