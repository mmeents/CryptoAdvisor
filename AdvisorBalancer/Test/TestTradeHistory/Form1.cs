using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Jojatekok;
using Jojatekok.PoloniexAPI;
using Jojatekok.PoloniexAPI.WalletTools;
using Jojatekok.PoloniexAPI.TradingTools;
using Jojatekok.PoloniexAPI.MarketTools;
using TT = Jojatekok.PoloniexAPI.TradingTools;
using System.IO;
using System.Globalization;

namespace TestTradeHistory {
  public partial class Form1 : Form {
    public Form1() {
      InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e) {

      string aCurrency = "LTC";
      CurrencyPair aCP = new CurrencyPair("BTC", "LTC");
      TradeHistMngr aMng = new TradeHistMngr(aCurrency);
      BaseRates abr = new BaseRates();
      abr.USDBTCRate = 1776;


      String sFileName = "C:\\A Projects\\Advisor\\Advisor\\Test\\TestTradeHistory\\bin\\Debug\\THBTC-LTC.txt";
      string sLine = "";
      if (File.Exists(sFileName)) {
        StreamReader sIn = new StreamReader(sFileName);
        while (sLine != null) {
          sLine = sIn.ReadLine();
          if ((sLine != null) &&(sLine.Trim()!="")){
            Int32 sLineCount = sLine.ParseCount("\",");
            Jojatekok.PoloniexAPI.TradingTools.Trade lt = new Jojatekok.PoloniexAPI.TradingTools.Trade(
              Convert.ToUInt64(sLine.ParseString("\",", 0)),
              (Convert.ToString(sLine.ParseString("\",", 1)) == "Buy" ? OrderType.Buy : OrderType.Sell),
              Convert.ToDouble(sLine.ParseString("\",", 2)),
              Convert.ToDouble(sLine.ParseString("\",", 3)),
              Convert.ToDouble(sLine.ParseString("\",", 4)),
              Convert.ToDateTime(sLine.ParseString("\",", 5))
              );

            aMng.AddTrade(abr, aCP, lt);
          }
        }

        aMng.WriteShards();

      }
    }

    private void panel1_Paint(object sender, PaintEventArgs e) {
 //     BufferedGraphics bg = BufferedGraphicsManager.Current.Allocate(panel1.CreateGraphics(), panel1.DisplayRectangle);
 //     try {

 /*       float fWidth = bg.Graphics.VisibleClipBounds.Width;
        float fHeight = bg.Graphics.VisibleClipBounds.Height;
        double f20Height = fHeight * 0.2;
        double f15Height = fHeight * 0.145;
        double f20Width = fWidth * 0.2;
        double f15Width = fWidth * 0.15;
        Font fCur10 = new Font("Courier New", 10);
        SizeF sLocSize = bg.Graphics.MeasureString("00000.0000 0000", fCur10);
        double dMaxH = 1000;  
        for (Int32 i = 0; i<10000;i++){
          dMaxH = i * sLocSize.Height + 50;
          bg.Graphics.DrawString(Convert.ToDouble(i * 0.000001 + 0.0246).toStr8(), fCur10, Brushes.White, new PointF(20, sLocSize.Height * i));

        }
          

        bg.Render(panel1.CreateGraphics());
        Size s = new System.Drawing.Size(panel1.AutoScrollMinSize.Width, Convert.ToInt32(dMaxH));
        panel1.AutoScrollMinSize = s;
      } finally {
        bg.Dispose();
      }
*/
    }

    private void button2_Click(object sender, EventArgs e) {
      for (var i = 0; i < 100; i++) {
        textBox1.Text = Convert.ToString(Convert.ToString(i*100).GetHashCode()) + Environment.NewLine + textBox1.Text;
      }
      textBox1.Text = Convert.ToString(Convert.ToString("This is a Test").GetHashCode()) + Environment.NewLine + textBox1.Text;
      textBox1.Text = Convert.ToString(Convert.ToString("This is a Test").GetHashCode()) + Environment.NewLine + textBox1.Text;
    }
  }

    public class CurrencyShard {
      public TradeHistMngr Owner = null;
      public double BTCPricePerCoin = 0;
      public double AmountQuote = 0;
      public double AmountBase = 0;
      public double SmallBuyFeeQuote = 0;
      public double LargeBuyFeeQuote = 0;
      public double SmallSellFeeBase = 0;
      public double LargeSellFeeBase = 0;
      public CurrencyShard(TradeHistMngr aOwner, string sLine) {
        BTCPricePerCoin = Convert.ToDouble(sLine.ParseString("\", ", 0));
        AmountQuote = Convert.ToDouble(sLine.ParseString("\", ", 1));
        AmountBase = Convert.ToDouble(sLine.ParseString("\", ", 2));
        ReCalculateFees();
      }
      public CurrencyShard(TradeHistMngr aOwner, Double aBTCPricePaid, Double aAmountQuote, Double aAmountBase) {
        Owner = aOwner;
        BTCPricePerCoin = aBTCPricePaid;
        AmountQuote = aAmountQuote;
        AmountBase = aAmountBase;
      }
      public string ToDataLine() {
        string sDataLine = BTCPricePerCoin.toStr8() + ", " + AmountQuote.toStr8() + ", " + AmountBase.toStr8();
        return sDataLine;
      }
      public void ParseLine(string sLine) {
        BTCPricePerCoin = Convert.ToDouble(sLine.ParseString("\", ", 0));
        AmountQuote = Convert.ToDouble(sLine.ParseString("\", ", 1));
        AmountBase = Convert.ToDouble(sLine.ParseString("\", ", 2));
        ReCalculateFees();
      }
      public void ReCalculateFees() {
        SmallBuyFeeQuote = AmountQuote * 0.0015;
        LargeBuyFeeQuote = AmountQuote * 0.0025;
        SmallSellFeeBase = AmountBase * 0.0015;
        LargeSellFeeBase = AmountBase * 0.0025;
      }
    }

    public class BaseRates {
      public double USDBTCRate;
      public double BTCETHRate;
      public double BTCXMRRate;
    }

    public class TradeHistMngr {
      public String Currency;
      public String LastSellDetails = "";
      public SortedList<double, CurrencyShard> Shards;
      public double BTCAvgPricePaid = 0;
      public TradeHistMngr(string aCurrency) {
        Currency = aCurrency;
        Shards = new SortedList<double, CurrencyShard>();
      }

      public void AddTrade(BaseRates aMarketRates, CurrencyPair aMarketPair, TT.ITrade aTrade) {
        double aChkPrice = 0;
        double aTradeQuote = 0;
        double aTradeBase = 0;
        //if (aTrade.Type == OrderType.Buy) {
          aTradeQuote = aTrade.AmountQuote;
          if (aMarketPair.BaseCurrency == "BTC") {
            aChkPrice = aTrade.PricePerCoin;
            aTradeBase = aTrade.AmountBase;
          } else if (aMarketPair.BaseCurrency == "USDT") {
            aChkPrice = aTrade.PricePerCoin / aMarketRates.USDBTCRate;
            aTradeBase = aTrade.AmountBase / aMarketRates.USDBTCRate;
          } else if (aMarketPair.BaseCurrency == "ETH") {
            aChkPrice = aTrade.PricePerCoin * aMarketRates.BTCETHRate;
            aTradeBase = aTrade.AmountBase * aMarketRates.BTCETHRate;
          } else if (aMarketPair.BaseCurrency == "XMR") {
            aChkPrice = aTrade.PricePerCoin * aMarketRates.BTCXMRRate;
            aTradeBase = aTrade.AmountBase * aMarketRates.BTCXMRRate;
          }
        //}

        if (aTrade.Type == OrderType.Buy) {
          aTradeQuote = aTradeQuote * 0.9985;  //  take off min fee 0.0015 from quote total.
          if (Shards.ContainsKey(aChkPrice)) {
            Shards[aChkPrice].AmountBase = Shards[aChkPrice].AmountBase + aTradeBase;
            Shards[aChkPrice].AmountQuote = Shards[aChkPrice].AmountQuote + aTradeQuote;
          } else {
            Shards.Add(aChkPrice, new CurrencyShard(this, aChkPrice, aTradeQuote, aTradeBase));
          }
          Shards[aChkPrice].ReCalculateFees();

         // LastSellDetails = aMarketPair.QuoteCurrency + " buy " + aTradeQuote.toStr8() + " at " + aChkPrice.toStr8() + " btc total " + aTradeBase.toStr8() ;
         // LastSellDetails.toLog("BTC-LTC");

        } else {

          double QuoteVolToFind = aTradeQuote;
          double BaseVolToFind = 0; //aTradeBase;
          Int32 iShardCount = Shards.Keys.Count - 1;
          Int32 iShard = iShardCount;
          List<CurrencyShard> TempListOfShards = new List<CurrencyShard>();
          List<CurrencyShard> TempListOfShards2 = new List<CurrencyShard>();
          CurrencyShard aTLS = null;
          while ((iShard >= 0) && (QuoteVolToFind > 0)) {
            double aShardPriceKey = Shards.Keys[iShard];
            if (aShardPriceKey < aChkPrice) {  // First Find Possible Wins
              if (QuoteVolToFind < Shards[aShardPriceKey].AmountQuote) { //  Partial shard...

                if (( (Shards[aShardPriceKey].AmountQuote - QuoteVolToFind) * aShardPriceKey) < Shards[aShardPriceKey].LargeSellFeeBase) {
                  CurrencyShard aCS = Shards[aShardPriceKey];
                  TempListOfShards.Add(aCS);
                } else {
                  aTLS = new CurrencyShard(null, aShardPriceKey, QuoteVolToFind, QuoteVolToFind * aShardPriceKey);
                  Shards[aShardPriceKey].AmountQuote = Shards[aShardPriceKey].AmountQuote - QuoteVolToFind;
                  Shards[aShardPriceKey].AmountBase = Shards[aShardPriceKey].AmountQuote * aShardPriceKey;
                  Shards[aShardPriceKey].ReCalculateFees();
                }
                QuoteVolToFind = 0;
                break;
              } else {
                QuoteVolToFind = QuoteVolToFind - Shards[aShardPriceKey].AmountQuote;
                BaseVolToFind = BaseVolToFind + Shards[aShardPriceKey].AmountBase;
                CurrencyShard aCS = Shards[aShardPriceKey];
                TempListOfShards.Add(aCS);  //  add to sell group for removal below. 
              }
            }  // if the shard was more than the sell price it's a loss.           
            iShard--;
          }
          if (TempListOfShards.Count > 0) {
            foreach (CurrencyShard aCS in TempListOfShards) {
              Shards.Remove(aCS.BTCPricePerCoin);
            }
          }

          if (QuoteVolToFind > 0) {  // Second PASS Find Possible Losses
            while ((iShard >= 0) && (QuoteVolToFind > 0)) {
              double aShardPriceKey = Shards.Keys[iShard];
              if (QuoteVolToFind < Shards[aShardPriceKey].AmountQuote) {
                aTLS = new CurrencyShard(null, aShardPriceKey, QuoteVolToFind, QuoteVolToFind * aShardPriceKey);
                Shards[aShardPriceKey].AmountQuote = Shards[aShardPriceKey].AmountQuote - QuoteVolToFind;
                Shards[aShardPriceKey].AmountBase = Shards[aShardPriceKey].AmountQuote * aShardPriceKey;
                QuoteVolToFind = 0;
                break;
              } else {
                QuoteVolToFind = QuoteVolToFind - Shards[aShardPriceKey].AmountQuote;
                BaseVolToFind = BaseVolToFind - Shards[aShardPriceKey].AmountBase;
                TempListOfShards2.Add(Shards[aShardPriceKey]);  //  add to sell group for removal below. 
              }
              iShard--;
            }
            if (TempListOfShards2.Count > 0) {
              foreach (CurrencyShard aCS in TempListOfShards2) {
                Shards.Remove(aCS.BTCPricePerCoin);
              }
            }
          }

          double UnAccountedForTotal = QuoteVolToFind;
          QuoteVolToFind = 0;
          BaseVolToFind = 0;
          foreach (CurrencyShard aCS in TempListOfShards) {
            BaseVolToFind = BaseVolToFind + aCS.AmountBase;
            QuoteVolToFind = QuoteVolToFind + aCS.AmountQuote;
          }
          foreach (CurrencyShard aCS in TempListOfShards2) {
            BaseVolToFind = BaseVolToFind + aCS.AmountBase;
            QuoteVolToFind = QuoteVolToFind + aCS.AmountQuote;
          }
          if (aTLS != null) {
            BaseVolToFind = BaseVolToFind + aTLS.AmountBase;
            QuoteVolToFind = QuoteVolToFind + aTLS.AmountQuote;
          }

          double btcAvgBuyPricePaid = BaseVolToFind / QuoteVolToFind;
          if (UnAccountedForTotal > 0) {
            QuoteVolToFind = QuoteVolToFind + UnAccountedForTotal;
            BaseVolToFind = BaseVolToFind + (UnAccountedForTotal * btcAvgBuyPricePaid);
            btcAvgBuyPricePaid = BaseVolToFind / QuoteVolToFind;
          }

          double ProfitLossValue = aTradeBase - BaseVolToFind;

         // LastSellDetails = aMarketPair.QuoteCurrency + " sold " + aTradeQuote.toStr8() + " at " + aChkPrice.toStr8() + " btc total " + aTradeBase.toStr8() + " cost " + BaseVolToFind.toStr8() + " profit " + ProfitLossValue.toStr8();
         // LastSellDetails.toLog("BTC-LTC");


        }

      }

      public void LoadShards() {
        String sFileName = Currency.CurrencyShardFileName();
        string sLine = "";
        if (File.Exists(sFileName)) {
          StreamReader sIn = new StreamReader(sFileName);
          while (sLine != null) {
            sLine = sIn.ReadLine();
            if (sLine != null) {
              CurrencyShard aCS = new CurrencyShard(this, sLine);
              if (Shards.Keys.Contains(aCS.BTCPricePerCoin)) {
                Shards[aCS.BTCPricePerCoin].AmountQuote += aCS.AmountQuote;
                Shards[aCS.BTCPricePerCoin].AmountBase += aCS.AmountBase;
              } else {
                Shards.Add(aCS.BTCPricePerCoin, aCS);
              }
            }
          }
        }
      }

      public void WriteShards() {
        String sFile = Currency.CurrencyShardFileName();
        if (File.Exists(sFile)) {
          File.Delete(sFile);
        }
        Int32 i = Shards.Count - 1;
        while (i >= 0) {
          double sLineKey = Shards.Keys[i];
          Shards[sLineKey].ToDataLine().toShardFile(Currency);
          i = i - 1;
        }
      }
    }

    public class appBalance {
      public string Currency;
      public double DBQuote;
      public double CostEstBTC;
      public double PriceEstBTC;
      public double BitcoinValue;
      public double QuoteAvailable;
      public double QuoteOnOrders;
      public double PercentOfTotal;
      public SortedList<double, TT.ITrade> Shards;
      public appBalance(string aCur, IBalance aBalance) {
        Currency = aCur;
        CostEstBTC = 0;
        PriceEstBTC = 0;
        BitcoinValue = aBalance.BitcoinValue;
        QuoteAvailable = aBalance.QuoteAvailable;
        QuoteOnOrders = aBalance.QuoteOnOrders;
        DBQuote = aBalance.QuoteAvailable + aBalance.QuoteOnOrders;
        Shards = new SortedList<double, TT.ITrade>();
      }
      public void setLastBalance(IBalance aBalance) {
        DBQuote = aBalance.QuoteAvailable + aBalance.QuoteOnOrders;
        BitcoinValue = aBalance.BitcoinValue;
        QuoteAvailable = aBalance.QuoteAvailable;
        QuoteOnOrders = aBalance.QuoteOnOrders;
      }
    }


    public static class AppUtils {
      public static string toStr8(this double x) {
        string y = String.Format(CultureInfo.InvariantCulture, "{0:0.00000000} ", x);
        return y;
      }
      public static string toStr8(this decimal x) {
        string y = String.Format(CultureInfo.InvariantCulture, "{0:0.00000000} ", x);
        return y;
      }
      public static string toStr4(this double x) {
        string y = String.Format(CultureInfo.InvariantCulture, "{0:0.0000} ", x);
        return y;
      }
      public static string toStr4P(this double x, Int32 iDigitToPad) {
        string y = String.Format(CultureInfo.InvariantCulture, "{0:0.0000} ", x).PadLeft(iDigitToPad, ' ');
        return y;
      }
      public static string toStr4(this decimal x) {
        string y = String.Format(CultureInfo.InvariantCulture, "{0:0.0000} ", x);
        return y;
      }
      public static string toStr2(this double x) {
        string y = String.Format(CultureInfo.InvariantCulture, "{0:0.00} ", x);
        return y;
      }
      public static string toStr2P(this double x, Int32 iDigitToPad) {
        string y = String.Format(CultureInfo.InvariantCulture, "{0:0.00} ", x).PadLeft(iDigitToPad, ' ');
        return y;
      }
      public static string toStrDateTime(this DateTime x) {
        string y = String.Format(CultureInfo.InvariantCulture, "{0:yyyy-MM-dd hh:mm:ss.FFF} ", x);
        return y;
      }
      public static string toStrDate(this DateTime x) {
        string y = String.Format(CultureInfo.InvariantCulture, "{0:yyyy-MM-dd} ", x);
        return y;
      }
      public static string toStrTime(this DateTime x) {
        string y = String.Format(CultureInfo.InvariantCulture, "{0:mm:ss.FFF} ", x);
        return y;
      }
      public static int ParseCount(this string content, string delims) {
        return content.Split(delims.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length;
      }
      public static string ParseString(this string content, string delims, int take) {
        string[] split = content.Split(delims.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        return (take >= split.Length ? "" : split[take]);
      }
      public static decimal toDecimal(this Int32 x) {
        decimal y = Convert.ToDecimal(x);
        return y;
      }
      public static double toDouble(this decimal x) {
        double y = Convert.ToDouble(x);
        return y;
      }
      public static Int32 toInt32(this decimal x) {
        Int32 y = Convert.ToInt32(x);
        return y;
      }
      public static Int32 toInt32(this double x) {
        Int32 y = Convert.ToInt32(x);
        return y;
      }

      public static double toSum(this List<double> x) {
        double r = 0;
        foreach (double dValue in x) {
          r = r + dValue;
        }
        if (x.Count > 0) {
          return r;
        } else {
          return 0;
        }
      }

      public static double toAvg(this List<double> x) {
        double r = 0;
        foreach (double dValue in x) {
          r = r + dValue;
        }
        if (x.Count > 0) {
          return r / x.Count;
        } else {
          return 0;
        }
      }
      public static double toAvg5(this List<double> x) {
        double r = 0;
        Int32 i = 1;
        List<double> Avg5 = new List<double>();
        foreach (double dValue in x) {
          if (i % 5 == 0) {
            r = r + dValue;
            Avg5.Add(r / 5);
          } else {
            r = r + dValue;
          }
          i++;
        }
        if (Avg5.Count > 0) {
          return Avg5.toAvg();
        } else {
          return 0;
        }
      }

      public static string CurrencyShardFileName(this string Currency) {
        //return UserLogLocation() + "CH" + Currency + ".txt";
        //  return "C:\\MMREF\\Investor\\Proto\\Advisor\\Test\\TestTradeHistory\\bin\\Debug\\CHLTC.txt";
        return "C:\\A Projects\\Advisor\\Advisor\\Test\\TestTradeHistory\\bin\\Debug\\CHLTC.txt";
     
      }
      public static string TradeHistoryFileName(this CurrencyPair cp) {
        return UserLogLocation() + "TH" + cp.BaseCurrency + "-" + cp.QuoteCurrency + ".txt";
      }
      public static string SettingFileName(string sSettingName) {
        return UserLogLocation() + sSettingName + ".ini";
      }
      public static string LogFileName(string sLogName) {
        // return UserLogLocation() + sLogName + DateTime.Now.toStrDate().Trim() + ".txt";
        //return "C:\\MMREF\\Investor\\Proto\\Advisor\\Test\\TestTradeHistory\\bin\\Debug\\Log"+sLogName+".txt";
        return "C:\\A Projects\\Advisor\\Advisor\\Test\\TestTradeHistory\\bin\\Debug\\Log" + sLogName + ".txt";
      }
      public static string toLog(this string sMsg, string sLogName) {
        using (StreamWriter w = File.AppendText(LogFileName(sLogName))) { w.WriteLine(DateTime.Now.toStrDateTime() + ":" + sMsg); }
        return sMsg;
      }
      public static string toShardFile(this string sMsg, string Currency) {
        using (StreamWriter w = File.AppendText(Currency.CurrencyShardFileName())) { w.WriteLine(sMsg); }
        return sMsg;
      }
      public static string toDataFile(this string sMsg, CurrencyPair cp) {
        using (StreamWriter w = File.AppendText(cp.TradeHistoryFileName())) { w.WriteLine(sMsg); }
        return sMsg;
      }
      public static string toDataLine(this Jojatekok.PoloniexAPI.TradingTools.ITrade tTrade) {
        string sRet = tTrade.IdOrder.ToString() + "," + tTrade.Type.ToString() + "," + tTrade.PricePerCoin.toStr8() + "," + tTrade.AmountQuote.toStr8() + "," + tTrade.AmountBase.toStr8() + "," + tTrade.Time.toStrDateTime();
        return sRet;
      }
      public static string UserLogLocation() {
        String sUserDataDir = Application.CommonAppDataPath + "\\";
        if (!Directory.Exists(sUserDataDir)) {
          Directory.CreateDirectory(sUserDataDir);
        }
        return sUserDataDir;
      }

      public static string Pop(this List<string> que) {
        string sMsg = "";
        if (que.Count > 0) {
          sMsg = que[0];
          que.RemoveAt(0);
        }
        return sMsg;
      }
      public static void Add(this List<string> que, string sMsg) {
        que.Add(sMsg);
      }
      public static void Insert(this List<string> que, string sMsg) {
        que.Insert(0, sMsg);
      }

    }
  
}
