namespace Advisor {
  partial class KeyConfig {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnSave = new System.Windows.Forms.Button();
      this.label3 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.edPrivateKey = new System.Windows.Forms.TextBox();
      this.edPublicKey = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.edPassword = new System.Windows.Forms.TextBox();
      this.label5 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // btnCancel
      // 
      this.btnCancel.BackColor = System.Drawing.Color.CornflowerBlue;
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(308, 183);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(75, 23);
      this.btnCancel.TabIndex = 20;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = false;
      // 
      // btnSave
      // 
      this.btnSave.BackColor = System.Drawing.Color.Red;
      this.btnSave.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnSave.Location = new System.Drawing.Point(196, 183);
      this.btnSave.Name = "btnSave";
      this.btnSave.Size = new System.Drawing.Size(75, 23);
      this.btnSave.TabIndex = 19;
      this.btnSave.Text = "Save";
      this.btnSave.UseVisualStyleBackColor = false;
      this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(32, 151);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(64, 13);
      this.label3.TabIndex = 18;
      this.label3.Text = "Private Key:";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(36, 111);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(60, 13);
      this.label2.TabIndex = 17;
      this.label2.Text = "Public Key:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(29, 88);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(163, 13);
      this.label1.TabIndex = 16;
      this.label1.Text = "Poloniex Public/Private API Keys";
      // 
      // edPrivateKey
      // 
      this.edPrivateKey.Location = new System.Drawing.Point(98, 148);
      this.edPrivateKey.Name = "edPrivateKey";
      this.edPrivateKey.Size = new System.Drawing.Size(430, 20);
      this.edPrivateKey.TabIndex = 15;
      // 
      // edPublicKey
      // 
      this.edPublicKey.Location = new System.Drawing.Point(98, 108);
      this.edPublicKey.Name = "edPublicKey";
      this.edPublicKey.Size = new System.Drawing.Size(430, 20);
      this.edPublicKey.TabIndex = 14;
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(36, 39);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(56, 13);
      this.label4.TabIndex = 22;
      this.label4.Text = "Password:";
      // 
      // edPassword
      // 
      this.edPassword.Location = new System.Drawing.Point(98, 36);
      this.edPassword.Name = "edPassword";
      this.edPassword.PasswordChar = '?';
      this.edPassword.Size = new System.Drawing.Size(430, 20);
      this.edPassword.TabIndex = 21;
      this.edPassword.UseSystemPasswordChar = true;
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(29, 14);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(193, 13);
      this.label5.TabIndex = 23;
      this.label5.Text = "Password to lock up the Poloniex Keys.";
      // 
      // KeyConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(563, 241);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.edPassword);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnSave);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.edPrivateKey);
      this.Controls.Add(this.edPublicKey);
      this.Name = "KeyConfig";
      this.Text = "KeyConfig";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox edPrivateKey;
    private System.Windows.Forms.TextBox edPublicKey;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TextBox edPassword;
    private System.Windows.Forms.Label label5;
  }
}