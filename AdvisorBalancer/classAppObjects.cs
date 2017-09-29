using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Jojatekok.PoloniexAPI;
using Jojatekok.PoloniexAPI.WalletTools;
using Jojatekok.PoloniexAPI.TradingTools;
using Jojatekok.PoloniexAPI.MarketTools;
using TT = Jojatekok.PoloniexAPI.TradingTools;

namespace Advisor {


  public delegate void ddoOnWaitComplete();

  public class appRetargetOrderTask {
    private Boolean fWaitCancel = false;
    private List<appCmd> PoloQue = null;
    
    public Boolean CancelOrderComplete { get { return fWaitCancel; } set { fWaitCancel = value; doPostOrder(); } }
    public void doPostOrder() { 
    }
    public appRetargetOrderTask(List<appCmd>aPoloQue, ulong aOrderNum, OrderType aOT, double aPPC, double aQV ) {
      PoloQue = aPoloQue;
    }

  }

  public class appRefreshWait {
    private Boolean fWaitMarketsGetOpenOrders = false;
    private Boolean fWaitMarketsGetTrades = false;
    private Boolean fWaitTradingGetOpenOrders = false;
    private Boolean fWaitGetBalances = false;
    private Boolean fWaitTradingGetTrades = false;
    public Boolean WaitMarketsGetOpenOrders { get { return fWaitMarketsGetOpenOrders; } set { fWaitMarketsGetOpenOrders = value; chkChanges(); } }
    public Boolean WaitMarketsGetTrades { get { return fWaitMarketsGetTrades; } set { fWaitMarketsGetTrades = value; chkChanges(); } }
    public Boolean WaitTradingGetOpenOrders { get { return fWaitTradingGetOpenOrders; } set { fWaitTradingGetOpenOrders = value; chkChanges(); } }
    public Boolean WaitGetBalances { get { return fWaitGetBalances; } set { fWaitGetBalances = value; chkChanges(); } }
    public Boolean WaitTradingGetTrades { get { return fWaitTradingGetTrades; } set { fWaitTradingGetTrades = value; chkChanges(); } }

    private void chkChanges() {

      if (fWaitMarketsGetOpenOrders && fWaitMarketsGetTrades && fWaitTradingGetOpenOrders && fWaitGetBalances && fWaitTradingGetTrades) {
        OnWaitComplete();
      }

    }
    public ddoOnWaitComplete OnWaitComplete;

    public appRefreshWait(ddoOnWaitComplete onComplete) {
      WaitMarketsGetOpenOrders = false;
      WaitMarketsGetTrades = false;
      WaitTradingGetOpenOrders = false;
      WaitGetBalances = false;
      WaitTradingGetTrades = false;
      OnWaitComplete = onComplete;
    }
  }

  public class appMarket {
    public Form OwnerForm;
    public IniVar mv;
    public CurrencyPair CurPair;

    public IMarketData PrevMarketData;
    public IMarketData LastMarketData;
    public List<IMarketData> MarketDataHistory;

    public IList<Jojatekok.PoloniexAPI.MarketTools.ITrade> lastTradeHistoryMarket;
    public List<Jojatekok.PoloniexAPI.MarketTools.ITrade> tradeHistoryMarket;

    public IList<Jojatekok.PoloniexAPI.TradingTools.ITrade> lastTradeHistoryTrades;
    public List<Jojatekok.PoloniexAPI.TradingTools.ITrade> tradeHistoryTrades;

    public IList<Jojatekok.PoloniexAPI.TradingTools.IOrder> lastTradeOpenOrders;
    public List<Jojatekok.PoloniexAPI.TradingTools.IOrder> tradeOpenOrders;

    public List<TrollMessage> TrollMsg;

    public Boolean TradeHistoryLoaded = false;
    public void ResetShards() {
      if (((Form1)OwnerForm).Balances.Keys.Contains(CurPair.QuoteCurrency)) {
        double aBal = ((Form1)OwnerForm).Balances[CurPair.QuoteCurrency].DBQuote;
        double aMakerFee = ((Form1)OwnerForm).Balances[CurPair.QuoteCurrency].ShardMngr.MakerFee;
        double aBalSum = 0;
        Int32 iTHI = 0;
        while ((iTHI <= tradeHistoryTrades.Count - 1) && (aBal >= aBalSum)) {
          if (tradeHistoryTrades[iTHI].Type == OrderType.Buy) {
            aBalSum = aBalSum + tradeHistoryTrades[iTHI].AmountQuote - (tradeHistoryTrades[iTHI].AmountQuote * aMakerFee);
          } else {
            aBalSum = aBalSum - (tradeHistoryTrades[iTHI].AmountQuote + (tradeHistoryTrades[iTHI].AmountQuote * aMakerFee));
          }
          if (aBal < aBalSum) {
            break;
          }
          iTHI++;
        }
        
        if (iTHI > tradeHistoryTrades.Count - 1) {
          iTHI = tradeHistoryTrades.Count - 1;
        }
        if(aBal < aBalSum){
          ((Form1)OwnerForm).Balances[CurPair.QuoteCurrency].ShardMngr.Shards.Clear();
          BaseRates aBR = ((Form1)OwnerForm).getBaseRates();
          for (Int32 i = iTHI; i >= 0; i--) {
            ((Form1)OwnerForm).Balances[CurPair.QuoteCurrency].AddTrade(aBR, CurPair, tradeHistoryTrades[i]);
          }
          ((Form1)OwnerForm).calculateAvgPricePaid(CurPair);
        }//  otherwise skip since trade history is not in yet.
      }
    }

    public void LoadTradeHistory() {
      String sFileName = CurPair.TradeHistoryFileName();
      string sLine = "";
      if (File.Exists(sFileName)) {
        StreamReader sIn = new StreamReader(sFileName);
        while (sLine != null) {
          sLine = sIn.ReadLine();
          if (sLine != null) {
            Int32 sLineCount = sLine.ParseCount("\",");
            Jojatekok.PoloniexAPI.TradingTools.Trade lt = new Jojatekok.PoloniexAPI.TradingTools.Trade(
              Convert.ToUInt64(sLine.ParseString("\",", 0)),
              (Convert.ToString(sLine.ParseString("\",", 1)) == "Buy" ? OrderType.Buy : OrderType.Sell),
              Convert.ToDouble(sLine.ParseString("\",", 2)),
              Convert.ToDouble(sLine.ParseString("\",", 3)),
              Convert.ToDouble(sLine.ParseString("\",", 4)),
              Convert.ToDateTime(sLine.ParseString("\",", 5))
              );
            tradeHistoryTrades.Insert(0, lt);        
          }
        }
        ResetShards();
      }
      TradeHistoryLoaded = true;

    }
    public void WriteTradeHistory() {
      string sFile = AppUtils.TradeHistoryFileName(CurPair);
      if (File.Exists(sFile)) {
        File.Delete(sFile);
      }
      Int32 i = tradeHistoryTrades.Count - 1;
      while (i >= 0) {
        if (i < 1000) {
          string sLine = tradeHistoryTrades[i].toDataLine();
          sLine.toDataFile(CurPair);
        }
        i = i - 1;
      }
    }

    public IOrderBook LastOrderBook;
    public Int64 Number1SpotCount = 0;
    public double AvgPricePaid = 0;
    public Boolean MarketFrozen = false;
    public Int32 BuyTickerCount = 0;
    public Int32 SellTickerCount = 0;
    public Int32 SellTickerHeight = 0;
    public Int32 LastSellTickerHeight = 0;
    public Int32 BuyTickerHeight = 0;
    public Int32 LastBuyTickerHeight = 0;
    public Int32 BuyTicsPerSec = 0;
    public Int32 SellTicksPerSec = 0;
    public double LastBTCEst;
    public double HighestBuy = 0;
    public double LowestSell = 0;
    public double BuyVol = 0;
    public double SellVol = 0;
    public List<double> BuyTicAvg;
    public List<double> SellTicAvg;
    public double FiveSecBuyTicAvg = 0;
    public double PerSecBuyTicAvg = 0;
    public double FiveSecSellTicAvg = 0;
    public double PerSecSellTicAvg = 0;

    public List<double> PriceChangeAvg;
    public double LastBuyPrice = 0;
    public double PerSecBuyPriceChangeAvg = 0;
    public double PerSecLastPriceChangeAvg = 0;
    public double PerSecPriceRateOfChange = 0;
    public List<double> PriceRateOfChangeHistory;
    public double PriceRateOfChangeAvg = 0;

    public ulong LastOrderBuyNum = 0;
    public double LastOrderBuyPrice = 0;
    public double LastOrderBuyVol;

    public ulong LastOrderSellNum = 0;
    public double LastOrderSellPrice = 0;
    public double LastOrderSellVol = 0;

    public double LastWhaleRiderPriceBuy = 0;

    public double NextBuyPrice = 0;
    public double NextSellPrice = 0;
    public double NextSellWhalePrice = 0;

    public double ChartMax = 0;
    public double ChartMin = 0;
    public double ChartScale = 0;
    public double ChartLastPrice = 0;

    public decimal HoldAmount = 0;
    public decimal SellThreshold = 0;
    public decimal BuyThreshold = 0;
    public Boolean GoHold = false;

    private Boolean fSelected = false;
    public Boolean Selected {
      get { return fSelected; }
      set { fSelected = value; mv["isSelected"] = (value ? "true" : "false"); }
    }

    public appMarket(CurrencyPair aCurPair, IMarketData aMarketData, Form aForm) {
      mv = new IniVar("MarketSettings"+aCurPair.ToString());           
      OwnerForm = aForm;
      CurPair = aCurPair;
      PrevMarketData = aMarketData;
      LastMarketData = aMarketData;
      MarketFrozen = aMarketData.IsFrozen;
      MarketDataHistory = new List<IMarketData>();
      tradeHistoryMarket = new List<Jojatekok.PoloniexAPI.MarketTools.ITrade>();
      tradeHistoryTrades = new List<Jojatekok.PoloniexAPI.TradingTools.ITrade>();
      tradeOpenOrders = new List<Jojatekok.PoloniexAPI.TradingTools.IOrder>();
      BuyTicAvg = new List<double>();
      SellTicAvg = new List<double>();
      PriceChangeAvg = new List<double>();
      PriceRateOfChangeHistory = new List<double>();
      setUpdatedMarketData(aMarketData);
      LowestSell = aMarketData.PriceLast;
      fSelected = (mv["isSelected"] == "true");
      TrollMsg = new List<TrollMessage>();

      HoldAmount = 0;
      SellThreshold = 0;
      BuyThreshold = 0;

      string aVal = mv["HoldAmount"];
      decimal aDec = 0;
      decimal bDec = (Decimal.TryParse(aVal, out aDec) ? aDec : 1);
      HoldAmount = bDec;

      aVal = mv["SellThresh"];
      bDec = (Decimal.TryParse(aVal, out aDec) ? aDec : Convert.ToDecimal(0.02));
      SellThreshold = bDec;

      aVal = mv["BuyThresh"];
      bDec = (Decimal.TryParse(aVal, out aDec) ? aDec : Convert.ToDecimal(0.02));
      BuyThreshold = bDec;

      aVal = mv["GoHold"];
      if (aVal == "True") {
        GoHold = true;
      } else {
        GoHold = false;
      }
    }

    public Boolean NeedsTradeOrderRefresh = false;
    public Boolean HadNeedsTradeOrderRefresh = false;
    public void setUpdatedMarketData(IMarketData aMD) {
      if (aMD.PriceLast == aMD.OrderTopSell) {
        BuyTickerHeight++;
      } else {
        SellTickerHeight++;
      }
      PrevMarketData = LastMarketData;
      LastMarketData = aMD;

      if ((tradeOpenOrders.Count > 0)&&(!NeedsTradeOrderRefresh)) {
        foreach (TT.IOrder aTO in tradeOpenOrders) {
          if (aTO.Type == OrderType.Buy) {
            if (aTO.PricePerCoin > aMD.PriceLast) {
              NeedsTradeOrderRefresh = true;
            }
          } else {
            if (aTO.PricePerCoin < aMD.PriceLast) {
              NeedsTradeOrderRefresh = true;
            }
          }          
        }
      }

      MarketDataHistory.Insert(0, aMD);
      if (MarketDataHistory.Count > 499) {
        MarketDataHistory.RemoveAt(499);
      }

      LastBTCEst = 0;
      if (CurPair.BaseCurrency == "BTC") {
        LastBTCEst = LastMarketData.PriceLast;
      } else if (CurPair.BaseCurrency == "USDT") {
        CurrencyPair acp = new CurrencyPair("USDT", "BTC");
        if (((Form1)OwnerForm).MarketsOfInterest.Keys.Contains(acp)) {
          LastBTCEst = LastMarketData.PriceLast / ((Form1)OwnerForm).MarketsOfInterest[acp].LastMarketData.PriceLast;
        }
      } else if (CurPair.BaseCurrency == "ETH") {
        CurrencyPair acp = new CurrencyPair("BTC", "ETH");
        if (((Form1)OwnerForm).MarketsOfInterest.Keys.Contains(acp)) {
          LastBTCEst = LastMarketData.PriceLast * ((Form1)OwnerForm).MarketsOfInterest[acp].LastMarketData.PriceLast;
        }
      } else if (CurPair.BaseCurrency == "XMR") {
        CurrencyPair acp = new CurrencyPair("BTC", "XMR");
        if (((Form1)OwnerForm).MarketsOfInterest.Keys.Contains(acp)) {
          LastBTCEst = LastMarketData.PriceLast * ((Form1)OwnerForm).MarketsOfInterest[acp].LastMarketData.PriceLast;
        }
      }

      BuyTickerCount = 0;
      SellTickerCount = 0;
      foreach (IMarketData m in MarketDataHistory) {
        if (HighestBuy == 0) {
          HighestBuy = m.PriceLast;
        }
        if (LowestSell == 0) {
          LowestSell = m.PriceLast;
        }
        if (m.PriceLast.toStr8() == m.OrderTopBuy.toStr8()) {
          BuyTickerCount++;
          if (m.PriceLast > HighestBuy) {
            HighestBuy = m.PriceLast;
          }
        } else if (m.PriceLast.toStr8() == m.OrderTopSell.toStr8()) {
          SellTickerCount++;
          if (m.PriceLast < LowestSell) {
            LowestSell = m.PriceLast;
          }
        }
      }

    }
    public void ResetStatics() {
      MarketDataHistory.Clear();
      BuyTickerCount = 0;
      SellTickerCount = 0;
    }

    public Boolean hasTradeHistory = false;
    public double TradeHistoryBuyVol = 0;
    public double TradeHistorySellVol = 0;
    public void UpdateTradeHistory() {
      BuyTickerCount = 0;
      SellTickerCount = 0;
      if (tradeHistoryMarket.Count > 0) {
        hasTradeHistory = true;
      }
      TradeHistoryBuyVol = 0;
      TradeHistorySellVol = 0;
      foreach (Jojatekok.PoloniexAPI.MarketTools.ITrade x in tradeHistoryMarket) {
        if (x.PricePerCoin > HighestBuy) {
          HighestBuy = x.PricePerCoin;
        }
        if (x.PricePerCoin < LowestSell) {
          LowestSell = x.PricePerCoin;
        }
        if (x.Type == OrderType.Buy) {
          TradeHistorySellVol += x.AmountBase;
        } else {
          TradeHistoryBuyVol += x.AmountBase;
        }
      }
    }
    public double BuyRank() {
      //return Convert.ToInt32( ( (BuyTickerCount * 500) - (SellTickerCount*400) )  );
      double aAvg = 0;
      if (CurPair.BaseCurrency == "BTC") {
        aAvg = PerSecBuyPriceChangeAvg * 100000000;
      } else if (CurPair.BaseCurrency == "USDT") {
        CurrencyPair acp = new CurrencyPair("USDT", "BTC");
        if (((Form1)OwnerForm).MarketsOfInterest.Keys.Contains(acp)) {
          aAvg = (PerSecBuyPriceChangeAvg / ((Form1)OwnerForm).MarketsOfInterest[acp].LastMarketData.PriceLast) * 100000000;
        }
      } else if (CurPair.BaseCurrency == "ETH") {
        CurrencyPair acp = new CurrencyPair("BTC", "ETH");
        if (((Form1)OwnerForm).MarketsOfInterest.Keys.Contains(acp)) {
          aAvg = (PerSecBuyPriceChangeAvg * ((Form1)OwnerForm).MarketsOfInterest[acp].LastMarketData.PriceLast) * 100000000;
        }
      } else if (CurPair.BaseCurrency == "XMR") {
        CurrencyPair acp = new CurrencyPair("BTC", "XMR");
        if (((Form1)OwnerForm).MarketsOfInterest.Keys.Contains(acp)) {
          aAvg = (PerSecBuyPriceChangeAvg * ((Form1)OwnerForm).MarketsOfInterest[acp].LastMarketData.PriceLast) * 100000000;
        }
      }
      // FiveSecSellTicAvg     + " " + BestSells[iro].PerSecPriceRateOfChange.toStr8()  PerSecBuyPriceChangeAvg  
      double result = (PerSecBuyPriceChangeAvg < 0 ? -1 : 1) * Convert.ToDouble(Math.Abs(FiveSecSellTicAvg * aAvg)) * ((hasTradeHistory && (TradeHistoryBuyVol > 0)) ? TradeHistorySellVol / TradeHistoryBuyVol : 1) * (TradeHistorySellVol < TradeHistoryBuyVol ? 0.86 : 1.69);
      if(double.IsNaN(result)){
        result = FiveSecSellTicAvg;
      }
      return result;
    }
    public double SellRank() {

      double sValue = (((Form1)OwnerForm).Balances.Keys.Contains(CurPair.QuoteCurrency) ? Convert.ToDouble(((Form1)OwnerForm).Balances[CurPair.QuoteCurrency].BitcoinValue - (((Form1)OwnerForm).Balances[CurPair.QuoteCurrency].DBQuote * (AvgPricePaid))) : 1);
      if (double.IsNaN(sValue)) {
        sValue = Convert.ToDouble(((SellTickerCount * 500) - (BuyTickerCount * 400)));
      }
      return sValue;
      //return Convert.ToDouble( ( (SellTickerCount * 500) -(BuyTickerCount*400) ) );
    }
  }

  public class appCmd {
    public String Cmd;
    public Dictionary<string, object> Params;
    public appCmd(String aCmd, Dictionary<string, object> aParams) {
      Cmd = aCmd;
      Params = aParams;
    }
    public Boolean hasParam(string sParamName) {
      Boolean r = false;
      Dictionary<string, object> its = Params;  // make a copy incase it changes while searching. 
      foreach (string sKey in its.Keys) {
        if (sKey.Trim().ToLowerInvariant() == sParamName.Trim().ToLowerInvariant()) {
          r = true;
          break;
        }
      }
      return r;
    }
  }

  public class IniVar {
    string FileName;
    Dictionary<string, string> cache;
    public IniVar(string sFileName) {
      FileName = AppUtils.SettingFileName(sFileName); ;
      cache = new Dictionary<string, string>();
    }
    private void SetVarValue(string VarName, string VarValue) {
      try {
        IniFile f = IniFile.FromFile(FileName);
        f["Variables"][VarName] = VarValue;
        f.Save(FileName);
        cache[VarName] = VarValue;
      } catch (Exception e) {
        throw e;
      }
    }
    private string GetVarValue(string VarName) {
      string result = "";
      try {
        if (cache.ContainsKey(VarName)) {
          result = cache[VarName];
        } else {
          IniFile f = IniFile.FromFile(FileName);
          result = f["Variables"][VarName];
          cache[VarName] = result;
        }
      } catch { }
      return result;
    }
    public string this[string VarName] { get { return GetVarValue(VarName); } set { SetVarValue(VarName, value); } }
  }

  public static class AppUtils {
    public static byte[] desIV = new byte[] { 11, 13, 27, 31, 37, 41, 71, 87 }; 
    public static string toBase64EncodeText(this string Text) {
      byte[] encBuff = System.Text.Encoding.UTF8.GetBytes(Text);
      return Convert.ToBase64String(encBuff);
    }
    public static string toBase64DecodeText(this string Text) {
      byte[] decbuff = Convert.FromBase64String(Text);
      string s = System.Text.Encoding.UTF8.GetString(decbuff);
      return s;
    }
    public static Int32 toInt32T(this double x) {
      Int32 y = Convert.ToInt32(x.toStr2().ParseString(".", 0));
      return y;
    }
    public static string toStr8(this double x) {
      string y = String.Format(CultureInfo.InvariantCulture, "{0:0.00000000}", x);
      return y;
    }
    public static string toStr8(this decimal x) {
      string y = String.Format(CultureInfo.InvariantCulture, "{0:0.00000000}", x);
      return y;
    }
    public static string toStr4(this double x) {
      string y = String.Format(CultureInfo.InvariantCulture, "{0:0.0000}", x);
      return y;
    }
    public static string toStr4P(this double x, Int32 iDigitToPad) {
      string y = String.Format(CultureInfo.InvariantCulture, "{0:0.0000}", x).PadLeft(iDigitToPad, ' ');
      return y;
    }
    public static string toStr4(this decimal x) {
      string y = String.Format(CultureInfo.InvariantCulture, "{0:0.0000}", x);
      return y;
    }
    public static string toStr2(this double x) {
      string y = String.Format(CultureInfo.InvariantCulture, "{0:0.00}", x);
      return y;
    }
    public static string toStr2P(this double x, Int32 iDigitToPad) {
      string y = String.Format(CultureInfo.InvariantCulture, "{0:0.00}", x).PadLeft(iDigitToPad, ' ');
      return y;
    }
    public static string toStrDateTime(this DateTime x) {
      string y = String.Format(CultureInfo.InvariantCulture, "{0:yyyy-MM-dd hh:mm:ss.FFF}", x);
      return y;
    }
    public static string toStrDate(this DateTime x) {
      string y = String.Format(CultureInfo.InvariantCulture, "{0:yyyy-MM-dd}", x);
      return y;
    }
    public static string toStrTime(this DateTime x) {
      string y = String.Format(CultureInfo.InvariantCulture, "{0:mm:ss.FFF}", x);
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
      return UserLogLocation() + "CH" + Currency + ".txt";
    }
    public static string TradeHistoryFileName(this CurrencyPair cp) {
      return UserLogLocation() + "TH" + cp.BaseCurrency + "-" + cp.QuoteCurrency + ".txt";
    }
    public static string SettingFileName(string sSettingName) {
      return UserLogLocation() + sSettingName + ".ini";
    }
    public static string LogFileName(string sLogName) {
      return UserLogLocation() + sLogName + DateTime.Now.toStrDate().Trim() + ".txt";
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

    public static appCmd Pop(this List<appCmd> que) {
      appCmd x = null;
      if (que.Count > 0) {
        x = que[0];
        que.Remove(x);
      }
      return x;
    }
    public static Boolean ChkAdd(this List<appCmd> que, appCmd aAppCmd) {
      Boolean cmdFound = false;
      List<appCmd> enQue = que;
      foreach (appCmd x in enQue) {   // check that cmd is not already in stack.
        if (cmdFound) { break; }
        if (x.Cmd == aAppCmd.Cmd) {
          if (aAppCmd.Params == null) {
            cmdFound = true;
          } else {
            if (aAppCmd.Params.ContainsKey("CurrencyPair")) {
              if (x.Params != null) {
                if (x.Params.ContainsKey("CurrencyPair")) {
                  cmdFound = (aAppCmd.Params["CurrencyPair"].ToString() == x.Params["CurrencyPair"].ToString());
                }
              }
            }
          }
        }
      }
      if ((!cmdFound) || (aAppCmd.Cmd == "TradingPostOrder") || (aAppCmd.Cmd == "TradingDeleteOrder")) {
        que.Add(aAppCmd);
      }
      return !cmdFound;
    }

    public static string toHexStr(this byte[] byteArray) {
      string outString = "";
      foreach (Byte b in byteArray)
        outString += b.ToString("X2");
      return outString;
    }
    public static byte[] toByteArray(this string hexString) {
      byte[] returnBytes = new byte[hexString.Length / 2];
      for (int i = 0; i < returnBytes.Length; i++)
        returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
      return returnBytes;
    }
    public static string toDESCipher(this string sText, string sPassword) {
      string sResult = "";
      PasswordDeriveBytes aPDB = new PasswordDeriveBytes(sPassword, null);
      DESCryptoServiceProvider aCSP = new DESCryptoServiceProvider();
      aCSP.Key = aPDB.CryptDeriveKey("DES", "SHA1", 64, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });
      aCSP.IV = desIV;
      MemoryStream ms = new MemoryStream();
      CryptoStream encStream = new CryptoStream(ms, aCSP.CreateEncryptor(), CryptoStreamMode.Write);
      StreamWriter sw = new StreamWriter(encStream);
      sw.WriteLine(sText.toBase64EncodeText());
      sw.Close();
      encStream.Close();
      byte[] buffer = ms.ToArray();
      ms.Close();
      sResult = buffer.toHexStr();
      return sResult;
    }
    public static string toDecryptDES(this string sDESCipherText, string sPassword) {
      PasswordDeriveBytes aPDB = new PasswordDeriveBytes(sPassword, null);
      DESCryptoServiceProvider aCSP = new DESCryptoServiceProvider();
      aCSP.Key = aPDB.CryptDeriveKey("DES", "SHA1", 64, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });
      aCSP.IV = AppUtils.desIV;

      MemoryStream ms = new MemoryStream(sDESCipherText.toByteArray());
      CryptoStream encStream = new CryptoStream(ms, aCSP.CreateDecryptor(), CryptoStreamMode.Read);
      StreamReader sr = new StreamReader(encStream);
      string val = sr.ReadLine().toBase64DecodeText();
      sr.Close();
      encStream.Close();
      ms.Close();
      return val;
    }
    public static string toAESCipher(this string sText, string sPassword) {
      string sResult = "";
      PasswordDeriveBytes aPDB = new PasswordDeriveBytes(sPassword, null);
      AesCryptoServiceProvider aASP = new AesCryptoServiceProvider();
      AesManaged aes = new AesManaged();
      aes.Key = aPDB.GetBytes(32);
      aes.IV = aPDB.GetBytes(16);
      MemoryStream ms = new MemoryStream();
      CryptoStream encStream = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
      StreamWriter sw = new StreamWriter(encStream);
      sw.WriteLine(sText.toBase64EncodeText());
      sw.Close();
      encStream.Close();
      byte[] buffer = ms.ToArray();
      ms.Close();
      sResult = buffer.toHexStr();
      return sResult;
    }
    public static string toDecryptAES(this string sDESCipherText, string sPassword) {
      string val = "";
      PasswordDeriveBytes aPDB = new PasswordDeriveBytes(sPassword, null);
      AesCryptoServiceProvider aASP = new AesCryptoServiceProvider();
      AesManaged aes = new AesManaged();
      aes.Key = aPDB.GetBytes(32);
      aes.IV = aPDB.GetBytes(16);
      MemoryStream ms = new MemoryStream(sDESCipherText.toByteArray());
      CryptoStream encStream = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
      StreamReader sr = new StreamReader(encStream);
      val = sr.ReadLine().toBase64DecodeText();
      sr.Close();
      encStream.Close();
      ms.Close();
      return val;
    }

    public static string Encrypt(this string StringToEncrypt) {      
      DESCryptoServiceProvider des = new DESCryptoServiceProvider();
      des.Key = "MUY5RUEzQTY3Rjc5QkU5OQ==".toBase64DecodeText().toByteArray();
      des.IV = "OTRCOTc5MDNCQTc4RjkzRg==".toBase64DecodeText().toByteArray();
      MemoryStream ms = new MemoryStream();
      CryptoStream encStream = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
      StreamWriter sw = new StreamWriter(encStream);
      sw.WriteLine(StringToEncrypt);
      sw.Close();
      encStream.Close();
      byte[] buffer = ms.ToArray();
      ms.Close();
      return buffer.toHexStr();
    }
    public static string Decrypt(this string CipherString) {
      DESCryptoServiceProvider des = new DESCryptoServiceProvider();
      des.Key = "MUY5RUEzQTY3Rjc5QkU5OQ==".toBase64DecodeText().toByteArray();
      des.IV = "OTRCOTc5MDNCQTc4RjkzRg==".toBase64DecodeText().toByteArray();
      MemoryStream ms = new MemoryStream(CipherString.toByteArray());
      CryptoStream encStream = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read);
      StreamReader sr = new StreamReader(encStream);
      string val = sr.ReadLine();
      sr.Close();
      encStream.Close();
      ms.Close();
      return val;
    }

  }

  public class TrollMessage {
    public string MessageText;
    public string MessageNumber;
    public string SenderName;
    public string SenderRep;
    public DateTime When;
    public TrollMessage(string aMessageNumber, String aSenderName, string aSenderRep, string aMessageText) {
      MessageNumber = aMessageNumber;
      SenderName = aSenderName;
      SenderRep = aSenderRep;
      MessageText = aMessageText;
      When = DateTime.Now;
    }
    public string toLogString() {
      return "DD=" + When.toStrTime() + "&MN=" + MessageNumber + "&SR=" + SenderRep + "&SN=" + SenderName + "&SM=" + MessageText;
    }
    public string toDisplayTxt() {
      return When.toStrTime() + " " + SenderName + ":" + MessageText;
    }
  }


}
