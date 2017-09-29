namespace Advisor {
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
      this.tDisplay = new System.Windows.Forms.Timer(this.components);
      this.tBotCmd = new System.Windows.Forms.Timer(this.components);
      this.tPoloCmd = new System.Windows.Forms.Timer(this.components);
      this.tReadWorker = new System.Windows.Forms.Timer(this.components);
      this.tClock = new System.Windows.Forms.Timer(this.components);
      this.btnBuy = new System.Windows.Forms.Button();
      this.btnSell = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.edChunkSize = new System.Windows.Forms.NumericUpDown();
      this.edPrice = new System.Windows.Forms.NumericUpDown();
      this.edMarkup = new System.Windows.Forms.NumericUpDown();
      this.btnBuyM = new System.Windows.Forms.Button();
      this.btnSellM = new System.Windows.Forms.Button();
      this.edWhaleDepth = new System.Windows.Forms.NumericUpDown();
      this.cbOnlyBTC = new System.Windows.Forms.CheckBox();
      this.btnSellWhale = new System.Windows.Forms.Button();
      this.edHoldAmount = new System.Windows.Forms.NumericUpDown();
      this.edSellThresh = new System.Windows.Forms.NumericUpDown();
      this.edBuyThresh = new System.Windows.Forms.NumericUpDown();
      this.cbTradeGo = new System.Windows.Forms.CheckBox();
      this.cbGoHold = new System.Windows.Forms.CheckBox();
      ((System.ComponentModel.ISupportInitialize)(this.edChunkSize)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.edPrice)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.edMarkup)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.edWhaleDepth)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.edHoldAmount)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.edSellThresh)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.edBuyThresh)).BeginInit();
      this.SuspendLayout();
      // 
      // tDisplay
      // 
      this.tDisplay.Interval = 250;
      this.tDisplay.Tick += new System.EventHandler(this.tDisplay_Tick);
      // 
      // tBotCmd
      // 
      this.tBotCmd.Tick += new System.EventHandler(this.tBotCmd_Tick);
      // 
      // tPoloCmd
      // 
      this.tPoloCmd.Interval = 250;
      this.tPoloCmd.Tick += new System.EventHandler(this.tPoloCmd_Tick);
      // 
      // tReadWorker
      // 
      this.tReadWorker.Tick += new System.EventHandler(this.tReadWorker_Tick);
      // 
      // tClock
      // 
      this.tClock.Interval = 1000;
      this.tClock.Tick += new System.EventHandler(this.tClock_Tick);
      // 
      // btnBuy
      // 
      this.btnBuy.Location = new System.Drawing.Point(868, 645);
      this.btnBuy.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.btnBuy.Name = "btnBuy";
      this.btnBuy.Size = new System.Drawing.Size(129, 72);
      this.btnBuy.TabIndex = 0;
      this.btnBuy.Text = "B U Y";
      this.btnBuy.UseVisualStyleBackColor = true;
      this.btnBuy.Click += new System.EventHandler(this.btnBuy_Click);
      // 
      // btnSell
      // 
      this.btnSell.Location = new System.Drawing.Point(1202, 645);
      this.btnSell.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.btnSell.Name = "btnSell";
      this.btnSell.Size = new System.Drawing.Size(129, 72);
      this.btnSell.TabIndex = 1;
      this.btnSell.Text = "S E L L ";
      this.btnSell.UseVisualStyleBackColor = true;
      this.btnSell.Click += new System.EventHandler(this.btnSell_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Location = new System.Drawing.Point(1005, 645);
      this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(194, 72);
      this.btnCancel.TabIndex = 2;
      this.btnCancel.Text = "C A N C E L";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // edChunkSize
      // 
      this.edChunkSize.DecimalPlaces = 8;
      this.edChunkSize.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.edChunkSize.Increment = new decimal(new int[] {
            5,
            0,
            0,
            262144});
      this.edChunkSize.Location = new System.Drawing.Point(848, 3);
      this.edChunkSize.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.edChunkSize.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
      this.edChunkSize.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            262144});
      this.edChunkSize.Name = "edChunkSize";
      this.edChunkSize.Size = new System.Drawing.Size(138, 30);
      this.edChunkSize.TabIndex = 4;
      this.edChunkSize.ThousandsSeparator = true;
      this.edChunkSize.Value = new decimal(new int[] {
            2,
            0,
            0,
            65536});
      this.edChunkSize.Visible = false;
      this.edChunkSize.ValueChanged += new System.EventHandler(this.edChunkSize_ValueChanged);
      // 
      // edPrice
      // 
      this.edPrice.DecimalPlaces = 8;
      this.edPrice.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.edPrice.Increment = new decimal(new int[] {
            5,
            0,
            0,
            524288});
      this.edPrice.Location = new System.Drawing.Point(1192, 497);
      this.edPrice.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.edPrice.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
      this.edPrice.Name = "edPrice";
      this.edPrice.Size = new System.Drawing.Size(105, 23);
      this.edPrice.TabIndex = 5;
      this.edPrice.ThousandsSeparator = true;
      this.edPrice.Value = new decimal(new int[] {
            2,
            0,
            0,
            65536});
      this.edPrice.Visible = false;
      this.edPrice.ValueChanged += new System.EventHandler(this.edPrice_ValueChanged);
      // 
      // edMarkup
      // 
      this.edMarkup.DecimalPlaces = 5;
      this.edMarkup.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.edMarkup.ImeMode = System.Windows.Forms.ImeMode.Disable;
      this.edMarkup.Increment = new decimal(new int[] {
            5,
            0,
            0,
            327680});
      this.edMarkup.Location = new System.Drawing.Point(1053, 497);
      this.edMarkup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.edMarkup.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
      this.edMarkup.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            327680});
      this.edMarkup.Name = "edMarkup";
      this.edMarkup.Size = new System.Drawing.Size(80, 23);
      this.edMarkup.TabIndex = 6;
      this.edMarkup.ThousandsSeparator = true;
      this.edMarkup.Value = new decimal(new int[] {
            2,
            0,
            0,
            65536});
      this.edMarkup.Visible = false;
      this.edMarkup.ValueChanged += new System.EventHandler(this.edMarkup_ValueChanged);
      // 
      // btnBuyM
      // 
      this.btnBuyM.Location = new System.Drawing.Point(868, 580);
      this.btnBuyM.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.btnBuyM.Name = "btnBuyM";
      this.btnBuyM.Size = new System.Drawing.Size(226, 65);
      this.btnBuyM.TabIndex = 7;
      this.btnBuyM.Text = "B U Y";
      this.btnBuyM.UseVisualStyleBackColor = true;
      this.btnBuyM.Visible = false;
      this.btnBuyM.Click += new System.EventHandler(this.btnBuyM_Click);
      // 
      // btnSellM
      // 
      this.btnSellM.Location = new System.Drawing.Point(1095, 580);
      this.btnSellM.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.btnSellM.Name = "btnSellM";
      this.btnSellM.Size = new System.Drawing.Size(236, 65);
      this.btnSellM.TabIndex = 8;
      this.btnSellM.Text = "S E L L";
      this.btnSellM.UseVisualStyleBackColor = true;
      this.btnSellM.Visible = false;
      this.btnSellM.Click += new System.EventHandler(this.btnSellM_Click);
      // 
      // edWhaleDepth
      // 
      this.edWhaleDepth.DecimalPlaces = 5;
      this.edWhaleDepth.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.edWhaleDepth.ImeMode = System.Windows.Forms.ImeMode.Disable;
      this.edWhaleDepth.Increment = new decimal(new int[] {
            5,
            0,
            0,
            327680});
      this.edWhaleDepth.Location = new System.Drawing.Point(868, 178);
      this.edWhaleDepth.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.edWhaleDepth.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.edWhaleDepth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            327680});
      this.edWhaleDepth.Name = "edWhaleDepth";
      this.edWhaleDepth.Size = new System.Drawing.Size(80, 23);
      this.edWhaleDepth.TabIndex = 9;
      this.edWhaleDepth.ThousandsSeparator = true;
      this.edWhaleDepth.Value = new decimal(new int[] {
            2,
            0,
            0,
            65536});
      this.edWhaleDepth.Visible = false;
      this.edWhaleDepth.ValueChanged += new System.EventHandler(this.edWhaleDepth_ValueChanged);
      // 
      // cbOnlyBTC
      // 
      this.cbOnlyBTC.AutoSize = true;
      this.cbOnlyBTC.BackColor = System.Drawing.Color.Black;
      this.cbOnlyBTC.ForeColor = System.Drawing.Color.White;
      this.cbOnlyBTC.Location = new System.Drawing.Point(141, 5);
      this.cbOnlyBTC.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.cbOnlyBTC.Name = "cbOnlyBTC";
      this.cbOnlyBTC.Size = new System.Drawing.Size(101, 24);
      this.cbOnlyBTC.TabIndex = 10;
      this.cbOnlyBTC.Text = "Only BTC";
      this.cbOnlyBTC.UseVisualStyleBackColor = false;
      this.cbOnlyBTC.Visible = false;
      this.cbOnlyBTC.CheckedChanged += new System.EventHandler(this.cbOnlyBTC_CheckedChanged);
      // 
      // btnSellWhale
      // 
      this.btnSellWhale.Location = new System.Drawing.Point(1202, 152);
      this.btnSellWhale.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.btnSellWhale.Name = "btnSellWhale";
      this.btnSellWhale.Size = new System.Drawing.Size(129, 55);
      this.btnSellWhale.TabIndex = 12;
      this.btnSellWhale.Text = "S E L L ";
      this.btnSellWhale.UseVisualStyleBackColor = true;
      this.btnSellWhale.Click += new System.EventHandler(this.btnSellWhale_Click);
      // 
      // edHoldAmount
      // 
      this.edHoldAmount.DecimalPlaces = 8;
      this.edHoldAmount.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.edHoldAmount.Increment = new decimal(new int[] {
            5,
            0,
            0,
            262144});
      this.edHoldAmount.Location = new System.Drawing.Point(84, 412);
      this.edHoldAmount.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.edHoldAmount.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
      this.edHoldAmount.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            262144});
      this.edHoldAmount.Name = "edHoldAmount";
      this.edHoldAmount.Size = new System.Drawing.Size(138, 30);
      this.edHoldAmount.TabIndex = 13;
      this.edHoldAmount.ThousandsSeparator = true;
      this.edHoldAmount.Value = new decimal(new int[] {
            2,
            0,
            0,
            65536});
      this.edHoldAmount.Visible = false;
      this.edHoldAmount.ValueChanged += new System.EventHandler(this.edHoldAmount_ValueChanged);
      // 
      // edSellThresh
      // 
      this.edSellThresh.DecimalPlaces = 5;
      this.edSellThresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.edSellThresh.ImeMode = System.Windows.Forms.ImeMode.Disable;
      this.edSellThresh.Increment = new decimal(new int[] {
            5,
            0,
            0,
            327680});
      this.edSellThresh.Location = new System.Drawing.Point(111, 362);
      this.edSellThresh.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.edSellThresh.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
      this.edSellThresh.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            327680});
      this.edSellThresh.Name = "edSellThresh";
      this.edSellThresh.Size = new System.Drawing.Size(80, 23);
      this.edSellThresh.TabIndex = 14;
      this.edSellThresh.ThousandsSeparator = true;
      this.edSellThresh.Value = new decimal(new int[] {
            2,
            0,
            0,
            65536});
      this.edSellThresh.Visible = false;
      this.edSellThresh.ValueChanged += new System.EventHandler(this.edSellThresh_ValueChanged);
      // 
      // edBuyThresh
      // 
      this.edBuyThresh.DecimalPlaces = 5;
      this.edBuyThresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.edBuyThresh.ImeMode = System.Windows.Forms.ImeMode.Disable;
      this.edBuyThresh.Increment = new decimal(new int[] {
            5,
            0,
            0,
            327680});
      this.edBuyThresh.Location = new System.Drawing.Point(111, 472);
      this.edBuyThresh.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.edBuyThresh.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
      this.edBuyThresh.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            327680});
      this.edBuyThresh.Name = "edBuyThresh";
      this.edBuyThresh.Size = new System.Drawing.Size(80, 23);
      this.edBuyThresh.TabIndex = 15;
      this.edBuyThresh.ThousandsSeparator = true;
      this.edBuyThresh.Value = new decimal(new int[] {
            2,
            0,
            0,
            65536});
      this.edBuyThresh.Visible = false;
      this.edBuyThresh.ValueChanged += new System.EventHandler(this.edBuyThresh_ValueChanged);
      // 
      // cbTradeGo
      // 
      this.cbTradeGo.AutoSize = true;
      this.cbTradeGo.BackColor = System.Drawing.Color.Black;
      this.cbTradeGo.ForeColor = System.Drawing.Color.White;
      this.cbTradeGo.Location = new System.Drawing.Point(273, 3);
      this.cbTradeGo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.cbTradeGo.Name = "cbTradeGo";
      this.cbTradeGo.Size = new System.Drawing.Size(88, 24);
      this.cbTradeGo.TabIndex = 16;
      this.cbTradeGo.Text = "Trading";
      this.cbTradeGo.UseVisualStyleBackColor = false;
      this.cbTradeGo.CheckedChanged += new System.EventHandler(this.cbTradeGo_CheckedChanged);
      // 
      // cbGoHold
      // 
      this.cbGoHold.AutoSize = true;
      this.cbGoHold.BackColor = System.Drawing.Color.Black;
      this.cbGoHold.ForeColor = System.Drawing.Color.White;
      this.cbGoHold.Location = new System.Drawing.Point(98, 235);
      this.cbGoHold.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.cbGoHold.Name = "cbGoHold";
      this.cbGoHold.Size = new System.Drawing.Size(132, 24);
      this.cbGoHold.TabIndex = 17;
      this.cbGoHold.Text = "Maintain Hold";
      this.cbGoHold.UseVisualStyleBackColor = false;
      this.cbGoHold.Visible = false;
      this.cbGoHold.CheckedChanged += new System.EventHandler(this.cbGoHold_CheckedChanged);
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(24)))), ((int)(((byte)(16)))));
      this.ClientSize = new System.Drawing.Size(1818, 889);
      this.Controls.Add(this.cbGoHold);
      this.Controls.Add(this.cbTradeGo);
      this.Controls.Add(this.edBuyThresh);
      this.Controls.Add(this.edSellThresh);
      this.Controls.Add(this.edHoldAmount);
      this.Controls.Add(this.btnSellWhale);
      this.Controls.Add(this.cbOnlyBTC);
      this.Controls.Add(this.edWhaleDepth);
      this.Controls.Add(this.btnSellM);
      this.Controls.Add(this.btnBuyM);
      this.Controls.Add(this.edMarkup);
      this.Controls.Add(this.edPrice);
      this.Controls.Add(this.edChunkSize);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnSell);
      this.Controls.Add(this.btnBuy);
      this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.Name = "Form1";
      this.Text = "Advisor";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
      this.Load += new System.EventHandler(this.Form1_Load);
      this.Shown += new System.EventHandler(this.Form1_Shown);
      this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
      ((System.ComponentModel.ISupportInitialize)(this.edChunkSize)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.edPrice)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.edMarkup)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.edWhaleDepth)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.edHoldAmount)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.edSellThresh)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.edBuyThresh)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Timer tDisplay;
    private System.Windows.Forms.Timer tBotCmd;
    private System.Windows.Forms.Timer tPoloCmd;
    private System.Windows.Forms.Timer tReadWorker;
    private System.Windows.Forms.Timer tClock;
    private System.Windows.Forms.Button btnBuy;
    private System.Windows.Forms.Button btnSell;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.NumericUpDown edChunkSize;
    private System.Windows.Forms.NumericUpDown edPrice;
    private System.Windows.Forms.NumericUpDown edMarkup;
    private System.Windows.Forms.Button btnBuyM;
    private System.Windows.Forms.Button btnSellM;
    private System.Windows.Forms.NumericUpDown edWhaleDepth;
    private System.Windows.Forms.CheckBox cbOnlyBTC;
    private System.Windows.Forms.Button btnSellWhale;
    private System.Windows.Forms.NumericUpDown edHoldAmount;
    private System.Windows.Forms.NumericUpDown edSellThresh;
    private System.Windows.Forms.NumericUpDown edBuyThresh;
    private System.Windows.Forms.CheckBox cbTradeGo;
    private System.Windows.Forms.CheckBox cbGoHold;
  }
}

