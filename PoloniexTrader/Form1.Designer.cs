namespace PoloTrader {
  partial class Form1 {
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
      this.edQuantityBuy = new System.Windows.Forms.NumericUpDown();
      this.edPriceBuy = new System.Windows.Forms.NumericUpDown();
      this.edTotalBuy = new System.Windows.Forms.NumericUpDown();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.textBox2 = new System.Windows.Forms.TextBox();
      this.textBox3 = new System.Windows.Forms.TextBox();
      this.btnContinue = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.edQuantitySell = new System.Windows.Forms.NumericUpDown();
      this.edPriceSell = new System.Windows.Forms.NumericUpDown();
      this.edTotalSell = new System.Windows.Forms.NumericUpDown();
      this.btnReloadBalances = new System.Windows.Forms.Button();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.btnReloadOpenOrders = new System.Windows.Forms.Button();
      this.btnReloadOrderHistory = new System.Windows.Forms.Button();
      this.btnBuy = new System.Windows.Forms.Button();
      this.btnSell = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.edLongShort = new System.Windows.Forms.TrackBar();
      ((System.ComponentModel.ISupportInitialize)(this.edQuantityBuy)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.edPriceBuy)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.edTotalBuy)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.edQuantitySell)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.edPriceSell)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.edTotalSell)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.edLongShort)).BeginInit();
      this.SuspendLayout();
      // 
      // edQuantityBuy
      // 
      this.edQuantityBuy.BackColor = System.Drawing.Color.DimGray;
      this.edQuantityBuy.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.edQuantityBuy.DecimalPlaces = 8;
      this.edQuantityBuy.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.edQuantityBuy.ForeColor = System.Drawing.Color.FloralWhite;
      this.edQuantityBuy.ImeMode = System.Windows.Forms.ImeMode.Off;
      this.edQuantityBuy.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
      this.edQuantityBuy.Location = new System.Drawing.Point(379, 334);
      this.edQuantityBuy.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
      this.edQuantityBuy.Name = "edQuantityBuy";
      this.edQuantityBuy.Size = new System.Drawing.Size(129, 19);
      this.edQuantityBuy.TabIndex = 13;
      this.edQuantityBuy.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
      this.edQuantityBuy.Visible = false;
      this.edQuantityBuy.ValueChanged += new System.EventHandler(this.edQuantityBuy_ValueChanged);
      // 
      // edPriceBuy
      // 
      this.edPriceBuy.BackColor = System.Drawing.Color.DimGray;
      this.edPriceBuy.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.edPriceBuy.DecimalPlaces = 8;
      this.edPriceBuy.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.edPriceBuy.ForeColor = System.Drawing.Color.FloralWhite;
      this.edPriceBuy.Increment = new decimal(new int[] {
            1,
            0,
            0,
            524288});
      this.edPriceBuy.Location = new System.Drawing.Point(379, 363);
      this.edPriceBuy.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
      this.edPriceBuy.Name = "edPriceBuy";
      this.edPriceBuy.Size = new System.Drawing.Size(129, 19);
      this.edPriceBuy.TabIndex = 12;
      this.edPriceBuy.Value = new decimal(new int[] {
            2,
            0,
            0,
            65536});
      this.edPriceBuy.Visible = false;
      this.edPriceBuy.ValueChanged += new System.EventHandler(this.edQuantityBuy_ValueChanged);
      // 
      // edTotalBuy
      // 
      this.edTotalBuy.BackColor = System.Drawing.Color.DimGray;
      this.edTotalBuy.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.edTotalBuy.DecimalPlaces = 8;
      this.edTotalBuy.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.edTotalBuy.ForeColor = System.Drawing.Color.FloralWhite;
      this.edTotalBuy.Increment = new decimal(new int[] {
            5,
            0,
            0,
            262144});
      this.edTotalBuy.Location = new System.Drawing.Point(379, 392);
      this.edTotalBuy.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
      this.edTotalBuy.Name = "edTotalBuy";
      this.edTotalBuy.Size = new System.Drawing.Size(129, 19);
      this.edTotalBuy.TabIndex = 11;
      this.edTotalBuy.Value = new decimal(new int[] {
            2,
            0,
            0,
            65536});
      this.edTotalBuy.Visible = false;
      this.edTotalBuy.ValueChanged += new System.EventHandler(this.edTotalBuy_ValueChanged);
      // 
      // textBox1
      // 
      this.textBox1.Location = new System.Drawing.Point(223, 57);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(214, 20);
      this.textBox1.TabIndex = 0;
      // 
      // textBox2
      // 
      this.textBox2.Location = new System.Drawing.Point(223, 83);
      this.textBox2.Name = "textBox2";
      this.textBox2.Size = new System.Drawing.Size(415, 20);
      this.textBox2.TabIndex = 1;
      this.textBox2.Visible = false;
      // 
      // textBox3
      // 
      this.textBox3.Location = new System.Drawing.Point(223, 109);
      this.textBox3.Name = "textBox3";
      this.textBox3.Size = new System.Drawing.Size(415, 20);
      this.textBox3.TabIndex = 2;
      this.textBox3.Visible = false;
      // 
      // btnContinue
      // 
      this.btnContinue.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnContinue.Location = new System.Drawing.Point(223, 168);
      this.btnContinue.Name = "btnContinue";
      this.btnContinue.Size = new System.Drawing.Size(75, 23);
      this.btnContinue.TabIndex = 3;
      this.btnContinue.Text = "&Continue";
      this.btnContinue.UseVisualStyleBackColor = true;
      this.btnContinue.Click += new System.EventHandler(this.btnContinue_Click);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.ForeColor = System.Drawing.Color.RoyalBlue;
      this.label1.Location = new System.Drawing.Point(43, 57);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(174, 17);
      this.label1.TabIndex = 18;
      this.label1.Text = "Password to lock api keys:";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.ForeColor = System.Drawing.Color.RoyalBlue;
      this.label2.Location = new System.Drawing.Point(56, 84);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(160, 17);
      this.label2.TabIndex = 19;
      this.label2.Text = "Poloniex API Public Key:";
      this.label2.Visible = false;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.ForeColor = System.Drawing.Color.RoyalBlue;
      this.label3.Location = new System.Drawing.Point(51, 112);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(166, 17);
      this.label3.TabIndex = 20;
      this.label3.Text = "Poloniex API Private Key:";
      this.label3.Visible = false;
      // 
      // edQuantitySell
      // 
      this.edQuantitySell.BackColor = System.Drawing.Color.DimGray;
      this.edQuantitySell.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.edQuantitySell.DecimalPlaces = 8;
      this.edQuantitySell.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.edQuantitySell.ForeColor = System.Drawing.Color.FloralWhite;
      this.edQuantitySell.ImeMode = System.Windows.Forms.ImeMode.Off;
      this.edQuantitySell.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
      this.edQuantitySell.Location = new System.Drawing.Point(562, 334);
      this.edQuantitySell.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
      this.edQuantitySell.Name = "edQuantitySell";
      this.edQuantitySell.Size = new System.Drawing.Size(129, 19);
      this.edQuantitySell.TabIndex = 23;
      this.edQuantitySell.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
      this.edQuantitySell.Visible = false;
      this.edQuantitySell.ValueChanged += new System.EventHandler(this.edQuantityBuy_ValueChanged);
      // 
      // edPriceSell
      // 
      this.edPriceSell.BackColor = System.Drawing.Color.DimGray;
      this.edPriceSell.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.edPriceSell.DecimalPlaces = 8;
      this.edPriceSell.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.edPriceSell.ForeColor = System.Drawing.Color.FloralWhite;
      this.edPriceSell.Increment = new decimal(new int[] {
            1,
            0,
            0,
            524288});
      this.edPriceSell.Location = new System.Drawing.Point(562, 363);
      this.edPriceSell.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
      this.edPriceSell.Name = "edPriceSell";
      this.edPriceSell.Size = new System.Drawing.Size(129, 19);
      this.edPriceSell.TabIndex = 22;
      this.edPriceSell.Value = new decimal(new int[] {
            2,
            0,
            0,
            65536});
      this.edPriceSell.Visible = false;
      this.edPriceSell.ValueChanged += new System.EventHandler(this.edQuantityBuy_ValueChanged);
      // 
      // edTotalSell
      // 
      this.edTotalSell.BackColor = System.Drawing.Color.DimGray;
      this.edTotalSell.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.edTotalSell.DecimalPlaces = 8;
      this.edTotalSell.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.edTotalSell.ForeColor = System.Drawing.Color.FloralWhite;
      this.edTotalSell.Increment = new decimal(new int[] {
            5,
            0,
            0,
            262144});
      this.edTotalSell.Location = new System.Drawing.Point(562, 392);
      this.edTotalSell.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
      this.edTotalSell.Name = "edTotalSell";
      this.edTotalSell.Size = new System.Drawing.Size(129, 19);
      this.edTotalSell.TabIndex = 21;
      this.edTotalSell.Value = new decimal(new int[] {
            2,
            0,
            0,
            65536});
      this.edTotalSell.Visible = false;
      this.edTotalSell.ValueChanged += new System.EventHandler(this.edTotalBuy_ValueChanged);
      // 
      // btnReloadBalances
      // 
      this.btnReloadBalances.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.btnReloadBalances.ImageIndex = 2;
      this.btnReloadBalances.ImageList = this.imageList1;
      this.btnReloadBalances.Location = new System.Drawing.Point(12, 484);
      this.btnReloadBalances.Name = "btnReloadBalances";
      this.btnReloadBalances.Size = new System.Drawing.Size(75, 25);
      this.btnReloadBalances.TabIndex = 24;
      this.btnReloadBalances.Text = "Balances";
      this.btnReloadBalances.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.btnReloadBalances.UseVisualStyleBackColor = true;
      this.btnReloadBalances.Visible = false;
      this.btnReloadBalances.Click += new System.EventHandler(this.btnReloadBalances_Click);
      // 
      // imageList1
      // 
      this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList1.Images.SetKeyName(0, "spinner.gif");
      this.imageList1.Images.SetKeyName(1, "icon_priority.gif");
      this.imageList1.Images.SetKeyName(2, "reload.png");
      // 
      // btnReloadOpenOrders
      // 
      this.btnReloadOpenOrders.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnReloadOpenOrders.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
      this.btnReloadOpenOrders.ImageList = this.imageList1;
      this.btnReloadOpenOrders.Location = new System.Drawing.Point(550, 235);
      this.btnReloadOpenOrders.Name = "btnReloadOpenOrders";
      this.btnReloadOpenOrders.Size = new System.Drawing.Size(88, 20);
      this.btnReloadOpenOrders.TabIndex = 25;
      this.btnReloadOpenOrders.Text = "Refresh Orders";
      this.btnReloadOpenOrders.TextAlign = System.Drawing.ContentAlignment.TopCenter;
      this.btnReloadOpenOrders.UseVisualStyleBackColor = true;
      this.btnReloadOpenOrders.Visible = false;
      this.btnReloadOpenOrders.Click += new System.EventHandler(this.btnReloadOpenOrders_Click);
      // 
      // btnReloadOrderHistory
      // 
      this.btnReloadOrderHistory.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.btnReloadOrderHistory.ImageList = this.imageList1;
      this.btnReloadOrderHistory.Location = new System.Drawing.Point(549, 210);
      this.btnReloadOrderHistory.Name = "btnReloadOrderHistory";
      this.btnReloadOrderHistory.Size = new System.Drawing.Size(89, 19);
      this.btnReloadOrderHistory.TabIndex = 26;
      this.btnReloadOrderHistory.Text = "Refresh Trades";
      this.btnReloadOrderHistory.TextAlign = System.Drawing.ContentAlignment.TopCenter;
      this.btnReloadOrderHistory.UseVisualStyleBackColor = true;
      this.btnReloadOrderHistory.Visible = false;
      this.btnReloadOrderHistory.Click += new System.EventHandler(this.btnReloadOrderHistory_Click);
      // 
      // btnBuy
      // 
      this.btnBuy.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnBuy.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
      this.btnBuy.ImageList = this.imageList1;
      this.btnBuy.Location = new System.Drawing.Point(295, 431);
      this.btnBuy.Name = "btnBuy";
      this.btnBuy.Size = new System.Drawing.Size(213, 20);
      this.btnBuy.TabIndex = 27;
      this.btnBuy.Text = "Buy At 647469";
      this.btnBuy.TextAlign = System.Drawing.ContentAlignment.TopCenter;
      this.btnBuy.UseVisualStyleBackColor = true;
      this.btnBuy.Visible = false;
      this.btnBuy.Click += new System.EventHandler(this.btnBuy_Click);
      // 
      // btnSell
      // 
      this.btnSell.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnSell.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
      this.btnSell.ImageList = this.imageList1;
      this.btnSell.Location = new System.Drawing.Point(562, 431);
      this.btnSell.Name = "btnSell";
      this.btnSell.Size = new System.Drawing.Size(213, 20);
      this.btnSell.TabIndex = 28;
      this.btnSell.Text = "Sell At 647469";
      this.btnSell.TextAlign = System.Drawing.ContentAlignment.TopCenter;
      this.btnSell.UseVisualStyleBackColor = true;
      this.btnSell.Visible = false;
      this.btnSell.Click += new System.EventHandler(this.btnSell_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnCancel.ForeColor = System.Drawing.Color.OrangeRed;
      this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
      this.btnCancel.ImageList = this.imageList1;
      this.btnCancel.Location = new System.Drawing.Point(504, 280);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(40, 20);
      this.btnCancel.TabIndex = 29;
      this.btnCancel.Text = "X";
      this.btnCancel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Visible = false;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // edLongShort
      // 
      this.edLongShort.Location = new System.Drawing.Point(371, 156);
      this.edLongShort.Maximum = 1;
      this.edLongShort.Name = "edLongShort";
      this.edLongShort.Size = new System.Drawing.Size(66, 45);
      this.edLongShort.TabIndex = 30;
      this.edLongShort.TickStyle = System.Windows.Forms.TickStyle.Both;
      this.edLongShort.Visible = false;
      this.edLongShort.ValueChanged += new System.EventHandler(this.edLongShort_ValueChanged);
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Black;
      this.ClientSize = new System.Drawing.Size(984, 521);
      this.Controls.Add(this.edLongShort);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnSell);
      this.Controls.Add(this.btnBuy);
      this.Controls.Add(this.btnReloadOrderHistory);
      this.Controls.Add(this.btnReloadOpenOrders);
      this.Controls.Add(this.btnReloadBalances);
      this.Controls.Add(this.edQuantitySell);
      this.Controls.Add(this.edPriceSell);
      this.Controls.Add(this.edTotalSell);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.btnContinue);
      this.Controls.Add(this.textBox3);
      this.Controls.Add(this.textBox2);
      this.Controls.Add(this.textBox1);
      this.Controls.Add(this.edQuantityBuy);
      this.Controls.Add(this.edPriceBuy);
      this.Controls.Add(this.edTotalBuy);
      this.KeyPreview = true;
      this.Name = "Form1";
      this.Text = "P O L O N I E X   T r a d e r   ";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
      this.Shown += new System.EventHandler(this.Form1_Shown);
      this.ResizeEnd += new System.EventHandler(this.Form1_ResizeEnd);
      this.SizeChanged += new System.EventHandler(this.Form1_ResizeEnd);
      this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
      ((System.ComponentModel.ISupportInitialize)(this.edQuantityBuy)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.edPriceBuy)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.edTotalBuy)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.edQuantitySell)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.edPriceSell)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.edTotalSell)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.edLongShort)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.NumericUpDown edQuantityBuy;
    private System.Windows.Forms.NumericUpDown edPriceBuy;
    private System.Windows.Forms.NumericUpDown edTotalBuy;
    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.TextBox textBox2;
    private System.Windows.Forms.TextBox textBox3;
    private System.Windows.Forms.Button btnContinue;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.NumericUpDown edQuantitySell;
    private System.Windows.Forms.NumericUpDown edPriceSell;
    private System.Windows.Forms.NumericUpDown edTotalSell;
    private System.Windows.Forms.Button btnReloadBalances;
    private System.Windows.Forms.ImageList imageList1;
    private System.Windows.Forms.Button btnReloadOpenOrders;
    private System.Windows.Forms.Button btnReloadOrderHistory;
    private System.Windows.Forms.Button btnBuy;
    private System.Windows.Forms.Button btnSell;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.TrackBar edLongShort;
  }
}

