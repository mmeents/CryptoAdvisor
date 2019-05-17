using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExchangeSharp;

using C0DEC0RE;

namespace BittrexTrader {
  
  #region static includes 
    public static class appExt {
    public static Exception toAppLog(this Exception e, string sLocation) {
      try {
        string ToFile = MMExt.AppExeFolder() + "ProtoAlphaException" + DateTime.Now.toStrDate().Trim() + ".log";
        (DateTime.Now.toStrDateTime() + ":" + sLocation + ":" + e.toWalkExcTreePath()).toTextFileLine(ToFile);
      } catch { }
      return e;
    }
    public static void toAppLog(this string sLine) {
      try {
        string ToFile = MMExt.AppExeFolder() + "AppLog" + DateTime.Now.toStrDate().Trim() + ".log";
        (DateTime.Now.toStrDateTime() + ":" + sLine).toTextFileLine(ToFile);
      } catch { }
    }
    public static string toSat(this double aValue) { return (aValue * 100000000).toInt32T().toString();}
    public static string toSat(this decimal aValue) { return (aValue * 100000000).toInt32T().toString(); }

    public static string TradeHistoryFileName(this string cp) {
      string sFolder = MMExt.AppExeFolder() + "Logs";
      if (!Directory.Exists(sFolder)) { Directory.CreateDirectory(sFolder); }
      return sFolder + "\\TH" + cp + ".txt";
    }
    public static string toTradeHistoyFile(this string sMsg, string sMarket) {
      using (StreamWriter w = File.AppendText(sMarket.TradeHistoryFileName())) { w.WriteLine(sMsg); }
      return sMsg;
    }
    public static string toDataLine(this ExchangeOrderResult tTrade) { 
      string sRet = tTrade.OrderId+","+        
        tTrade.MarketSymbol + "," +
       (tTrade.IsBuy ? "Buy" : "Sell") + "," +
        tTrade.Price.toStr8() + "," +
        tTrade.AmountFilled.toStr8() +"," +        
        tTrade.Fees.toStr8() + "," +
        tTrade.FeesCurrency + "," +
        tTrade.OrderDate.toStrDateTime() + "," +        
        ((tTrade.Price * tTrade.AmountFilled)+(tTrade.IsBuy?tTrade.Fees:-1*tTrade.Fees)).toStr8() + "," +   // btc total extra 
      // tTrade.FillDate.toStrDateTime() + "," +
        tTrade.Result + "," +
      // tTrade.Amount.toStr8() + "," + 
      // tTrade.TradeId + "," +                
        tTrade.Message;
      return sRet;
    }
    public static ExchangeOrderResult TryParse(this string sDataLine) {
      ExchangeOrderResult ar = null;
      try { 
        string sOrderId = sDataLine.ParseString(",", 0);
        string sMarket = sDataLine.ParseString(",", 1);
        string sIsBuy = sDataLine.ParseString(",", 2);
        string sPrice = sDataLine.ParseString(",", 3);
        string sAmountFilled = sDataLine.ParseString(",", 4);
        string sFee = sDataLine.ParseString(",", 5);
        string sFeesCurrency = sDataLine.ParseString(",", 6);
        string sOrderDate = sDataLine.ParseString(",", 7);
        string sResult = sDataLine.ParseString(",", 9);
        string sMessage = sDataLine.ParseString(",", 10);

        ar = new ExchangeOrderResult();
        ar.OrderId = sOrderId;
        ar.MarketSymbol = sMarket;
        ar.IsBuy = (sIsBuy == "Buy");
        ar.Price = sPrice.toDouble().toDecimal();
        ar.AmountFilled = sAmountFilled.toDouble().toDecimal();
        ar.Fees = sFee.toDouble().toDecimal();
        ar.FeesCurrency = sFeesCurrency;
        ar.OrderDate = sOrderDate.toDateTime();
        Enum.TryParse<ExchangeAPIOrderResult>(sResult, out ExchangeAPIOrderResult aResult);
        ar.Result = aResult;
        ar.Message = sMessage;      
      } catch (Exception ee) { 
        ee.toAppLog("ExchangeOrderResultTryParse");      
      }
      return ar;
    }

    public static string toProcessHistoryFile(this CProcess aProc) { 
      string sLine = aProc.toDataLine();
      using (StreamWriter w = File.AppendText(appExt.ProcessHistoryFileName())) { w.WriteLine(sLine); }
      return sLine;
    }
    public static string toDataLine(this CProcess aProc) {
      string x =  aProc.Id.toString() +","+  aProc.Cmd.Cmd; 
      foreach(string s in aProc.Cmd.Keys) { 
        x = x + ","+ s + ":"+ aProc.Cmd[s].toString();
      }
      if (aProc.InvokeBalances) { 
        x = x + ",InvokeBalances:true";
      }
      return x;
    }
    public static string ProcessHistoryFileName() {
      string sFolder = MMExt.AppExeFolder() + "Logs";
      if (!Directory.Exists(sFolder)) { Directory.CreateDirectory(sFolder); }
      return sFolder + "\\PL" +  DateTime.Now.toStrDate().Trim() + ".txt";      
    }
  }
  #endregion

  #region CObject C is 

  public class CObject : ConcurrentDictionary<string, object> {
    public CObject() : base() { }
    public Boolean Contains(String aKey) { return base.Keys.Contains(aKey); }
    public new object this[string aKey] { 
      get { return (Contains(aKey)? base[aKey]:null); } 
      set { base[aKey] = value; } 
    }
    public void Remove(string aKey) {
      if (Contains(aKey)) {
        object outcast;
        base.TryRemove(aKey, out outcast);
      }
    }
    public void Merge(CObject aObject, Boolean OnDupOverwiteExisting) {
      if (aObject != null) {
        if (OnDupOverwiteExisting) {
          foreach (string sKey in aObject.Keys) {
            base[sKey] = aObject[sKey];
          }
        } else {
          foreach (string sKey in aObject.Keys) {
            if (!Contains(sKey)) {
              base[sKey] = aObject[sKey];
            }
          }
        }
      }
    }
  }    
  
  public class CBook : ConcurrentDictionary<decimal, object> { 
    public CBook() : base() { 
    }
    public Boolean Contains(decimal aKey) { return base.Keys.Contains(aKey); }
    public new object this[decimal aKey] {
      get { return (Contains(aKey) ? base[aKey] : null); }
      set { base[aKey] = value; }
    }
    public void Remove(decimal aKey) {
      if (Contains(aKey)) {
        object outcast;
        base.TryRemove(aKey, out outcast);
      }
    }
    public decimal ElementKeyAt(Int32 iIndex) {      
      IEnumerable<decimal> lQS = base.Keys.OrderByDescending(x => (x));
      return lQS.ElementAt(iIndex);
    }
  }

  public class CQueue : ConcurrentDictionary<Int64, object> {
    public Int64 Nonce = Int64.MinValue;
    public CQueue() : base() { }
    public Boolean Contains(Int64 aKey) { 
      return base.Keys.Contains(aKey);
    }
    public object Add(object aObj) {
      Nonce++;
      base[Nonce] = aObj;
      return aObj;
    }
    public object Pop() {
      Object aR = null;
      if (Keys.Count > 0) {
        base.TryRemove(base.Keys.OrderBy(x => x).First(), out aR);
      }
      return aR;
    }
    public void Remove(Int64 aKey) {
      if (Contains(aKey)) {
        base.TryRemove(aKey, out object outcast);
      }
    }
  }

  public class CCache : ConcurrentDictionary<Int64, object> { 
    public Int64 Nonce = Int64.MaxValue;
    public Int64 Height{ get{ return Int64.MaxValue - Nonce; } }
    public Boolean Contains(Int64 aKey) {
      return base.Keys.Contains(aKey);
    }
    public Int32 Size = 200;
    public CCache() : base() { 
    }
    public object Add(object aObj) { 
      Nonce--;
      base[Nonce] = aObj;
      if (base.Keys.Count > Size) { 
        object toBurn = Pop();
      }
      return aObj;
    }
    public object Pop() {
      Object aR = null;
      if (Keys.Count > 0) {        
        base.TryRemove(base.Keys.OrderBy(x => x).Last(), out aR);
      }
      return aR;
    }
    public void Remove(Int64 aKey) {
      if (Contains(aKey)) {
        base.TryRemove(aKey, out object outcast);
      }
    }
  }

  public class CAvgDecimalCache : CCache {
    public CAvgDecimalCache() : base() { }
    public object Add(decimal aObj) {
      Nonce--;
      base[Nonce] = aObj;
      if (base.Keys.Count > Size) {
        object toBurn = Pop();
      }
      return aObj;
    }
    public new decimal Pop() {
      object aR = 0;
      if (Keys.Count > 0) {
        base.TryRemove(base.Keys.OrderBy(x => x).Last(), out aR);
      }
      return (decimal)aR;
    }
    public new decimal this[Int64 aKey] { get { return (decimal)base[aKey]; } set { base[aKey] = value; } }
    public decimal toAvg() { 
      decimal aAvg = 0;
      decimal aSum = 0;
      if (base.Keys.Count > 0) { 
        foreach(Int64 iKey in base.Keys) {
          aSum = aSum + (decimal)base[iKey];
        }
        aAvg = aSum / base.Keys.Count;
      }
      return aAvg;
    }
  }

  public class CCmd : CObject {
    public CCmd(String aCmd) : base() { base["cmd"] = aCmd; }
    public string Cmd { get { return base["cmd"].toString(); } set { base["cmd"] = value; } }
  };

  public class CCmdQueue : CQueue {
    public CCmdQueue() : base() { }
    public CCmd Add(CCmd aCmd) {
      Nonce++;
      base[Nonce] = aCmd;
      return aCmd;
    }
    public new CCmd Pop() {
      Object aR = null;
      if (Keys.Count > 0) {
        base.TryRemove(base.Keys.OrderBy(x => x).First(), out aR);
      }
      return (CCmd)aR;
    }
  }

  #endregion

  #region CXCore Components

  public class CProcess : CObject {
    public COperation Owner { get { return (COperation)base["owner"]; } set { base["owner"] = value; } }
    public Int64 Id { get { return (Int64)base["id"]; } set { base["id"] = value; } }
    public CCmd Cmd { get { return (CCmd)base["cmd"]; } set { base["cmd"] = value; } }
    public bool InvokeBalances { 
      get { return (base.Contains("InvokeBalances") ? (bool)base["InvokeBalances"]:false); } 
      set { base["InvokeBalances"] = value; } }
    public BackgroundWorker Worker { get { return (BackgroundWorker)base["worker"]; } set { base["worker"] = value; } }
    public CProcess(COperation aOwner, Int64 aId, CCmd aCmd) : base() {
      Int32 es = 0;
      try {
        if (aOwner != null) {
          es = 1;
          Owner = aOwner;
          Owner[aId] = this;
        }
        es = 2;
        Id = aId;
        es = 3;
        Cmd = aCmd;
        es = 4;
        if (aCmd.Contains("InvokeBalances") && ((bool)aCmd["InvokeBalances"] == true)) {
          es = 5;
          InvokeBalances = true;
          es = 6;
          Cmd.Remove("InvokeBalances");
        }
        es = 7;
        Worker = new BackgroundWorker();
        Worker.WorkerSupportsCancellation = false;
        Worker.DoWork += new DoWorkEventHandler(DoWorkAsync);
        Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(WorkerComplete);
        es = 8;
        if (aOwner != null) {
          Worker.RunWorkerAsync();
        }
      } catch (Exception e) { e.toAppLog("PProcessNew " + es.toString()); }
    }
    private void DoWorkAsync(object sender, DoWorkEventArgs args) {
      Owner.Owner.form1.ProcessXCoreCmds( Cmd );
    }
    private void WorkerComplete(object sender, RunWorkerCompletedEventArgs args) {
      string es = "0";
      try {
        Worker.Dispose();
        if (Owner.Owner.bLogOps) {
          this.toProcessHistoryFile();
        }

        es = "1";        
        if (InvokeBalances) {
          Owner.Owner.AddCmd(new CCmd("DoGetBalances")); 
        }
        
        es = "2";
        Owner.Remove(Id);
      } catch (Exception e) { e.toAppLog("WorkerComplete"+es); }
    }
  }

  public class COperation : CQueue {
    public CXCore Owner;
    public COperation(CXCore aOwner) : base() {
      if (aOwner != null) {
        Owner = aOwner;
      }
    }
    public CProcess Add(CProcess aProc) {
      Nonce++;
      base[Nonce] = aProc;
      return aProc;
    }
    public new CProcess Pop() {
      Object aR = null;
      if (Keys.Count > 0) {
        base.TryRemove(base.Keys.First(), out aR);
      }
      return (CProcess)aR;
    }
  }
  
  public class CXCore : CObject {
    public readonly bool bLogOps = false;
    public long Nonce = 0;
    public Form1 form1;
    public CCmdQueue CmdQue;
    public COperation TheOp;
    public CProcess NextProcess;
    private BackgroundWorker Tic;
    private BackgroundWorker Toc;
    public CXCore(Form1 aForm1) : base() {
      daTime = DateTime.Now;
      daStartupTime = daTime;
      iStartUpTime = daStartupTime.ToFileTimeUtc();

      form1 = aForm1;
      CmdQue = new CCmdQueue();
      TheOp = new COperation(this);
      Tic = new BackgroundWorker();
      Tic.WorkerSupportsCancellation = false;
      Tic.DoWork += new DoWorkEventHandler(DoTic);
      Tic.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DoTicComplete);
      Toc = new BackgroundWorker();
      Toc.WorkerSupportsCancellation = false;
      Toc.DoWork += new DoWorkEventHandler(DoToc);
      Toc.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DoTocComplete);
    }    
    public DateTime daTime { get { return (DateTime)base["daTime"]; } set { base["daTime"] = value; } }
    public DateTime daStartupTime { get { return (DateTime)base["daStartupTime"]; } set { base["daStartupTime"] = value; } }
    public long iStartUpTime;
    public string sUpTime;
    //private bool ctFTT = false;
    private long iRefreshMarker;
    private long i15SecMarker;
    private Int32 iLastMin = 0;
    private void DoTic(object sender, DoWorkEventArgs args) {
      #region Time measurements 
      daTime = DateTime.Now;      
      long iTime = daTime.ToFileTimeUtc();
      Int32 iMin = Convert.ToInt32((((iTime - iStartUpTime) / 10000000) / 60) % 60);
      sUpTime = Convert.ToString(Convert.ToInt32(((iTime - iStartUpTime) / 10000000) / 60 / 60)) + ":" + Convert.ToString(Convert.ToInt32(((iTime - iStartUpTime) / 10000000) / 60) % 60) + ":" + Convert.ToString(Convert.ToInt32(((iTime - iStartUpTime) / 10000000)) % 60) + "s";
      if (((iTime - iRefreshMarker)/10000) > 245) {                // both are in milliseconds is about 4 ticks per second. 
        iRefreshMarker = iTime;       
        AddDoRefresh();        
      }

      if (((iTime - i15SecMarker) / 10000) > 15000) {                // both are in milliseconds is about 4 ticks per second. 
        i15SecMarker = iTime;
        AddCmd(new CCmd("DoPer15SecStats"));
      }
      

      if (iMin != iLastMin) {  // on the minute change check balances and trade history. 
        AddCmd(new CCmd("DoPerMinStats"));
   //     AddCmd(new CCmd("DoGetCompletedTrades"));
        iLastMin = iMin;
      }
      #endregion
      System.Threading.Thread.Sleep(32);  // 62.5 is around 16 per second.
    }
    private void DoTicComplete(object sender, RunWorkerCompletedEventArgs args) {
      Toc.RunWorkerAsync();
    }    
    private void DoTocComplete(object sender, RunWorkerCompletedEventArgs args) {
      Tic.RunWorkerAsync();
    }
    private void DoToc(object sender, DoWorkEventArgs args) {
      Int32 es = 0;
      try {
        es = 1;
        if (CmdQue.Count > 0) {
          es = 2;
          CCmd Cmd = CmdQue.Pop();
          Nonce++;
          NextProcess = new CProcess(TheOp, Nonce, Cmd);          
        }
        
      } catch (Exception e) {
        e.toAppLog("DoToc " + es.toString());
      }
    }
    public void Go() { Tic.RunWorkerAsync(); }
    public void AddCmd(CCmd aCmd ) { CmdQue.Add(aCmd);  }
    public void AddDoRefresh() { AddCmd(new CCmd("DoRefresh")); }

  }

  #endregion

  #region Markets

  public class CTickerCache : CCache { 
    public CTickerCache() : base() { 
    }
    public ExchangeTicker Add(ExchangeTicker aObj) {
      Nonce--;
      base[Nonce] = aObj;
      if (base.Keys.Count > Size) {
        object toBurn = Pop();
      }
      return aObj;
    }
    public new ExchangeTicker Pop() {
      Object aR = null;
      if (Keys.Count > 0) {
        base.TryRemove(base.Keys.OrderBy(x => x).Last(), out aR);
      }
      return (ExchangeTicker)aR;
    }
    public new ExchangeTicker  this[Int64 aKey] { get { return (ExchangeTicker)base[aKey]; } set { base[aKey] = value; } }
  }

  public class CTickerQueue : CQueue { 
    public CTickerQueue() : base() { 
    }
    public IReadOnlyCollection<KeyValuePair<string, ExchangeTicker>> Add(IReadOnlyCollection<KeyValuePair<string, ExchangeTicker>> aObj) {
      Nonce++;
      base.TryAdd(Nonce, aObj );
      base[Nonce] = aObj;
      return aObj;      
    }
    public new IReadOnlyCollection<KeyValuePair<string, ExchangeTicker>> Pop() {
      Object aR = null;
      if (Keys.Count > 0) {
        base.TryRemove(base.Keys.First(), out aR);
      }
      return (IReadOnlyCollection<KeyValuePair<string, ExchangeTicker>>)aR;
    }
  }

  public class CTradesQueue : CQueue { 
    public CTradesQueue() : base() { 
    }
    public KeyValuePair<string, ExchangeTrade> Add(KeyValuePair<string, ExchangeTrade> aObj) {
      Nonce++;
      base.TryAdd(Nonce, aObj);
      base[Nonce] = aObj;
      return aObj;
    }
    public new KeyValuePair<string, ExchangeTrade> Pop() {
      Object aR = null;
      if (Keys.Count > 0) {
        base.TryRemove(base.Keys.First(), out aR);
      }
      return (KeyValuePair<string, ExchangeTrade>)aR;    
    }
    public new ExchangeTrade this[Int64 aKey] { get { return (ExchangeTrade)base[aKey]; } set { base[aKey] = value; } }
  }

  public class CMarketTradesCache : CCache {
    public CMarketTradesCache() : base() {
    }
    public ExchangeTrade Add(ExchangeTrade aObj) {
      Nonce--;
      base[Nonce] = aObj;
      if (base.Keys.Count > Size) {
        object toBurn = Pop();
      }
      return aObj;
    }
    public new ExchangeTrade Pop() {
      Object aR = null;
      if (Keys.Count > 0) {
        base.TryRemove(base.Keys.Last(), out aR);
      }
      return (ExchangeTrade)aR;
    }
    public new ExchangeTrade this[Int64 aKey] { get { return (ExchangeTrade)base[aKey]; } set { base[aKey] = value; } }
  }

  public class COrders : CObject { 
    //CMarket Owner;
    string MarketName;    
    public bool Changed = false; 
    public COrders(string sMarketName, bool bLoadFromFile) : base() { 
      MarketName = sMarketName;
      if (bLoadFromFile) { 
        LoadFromFile();
      }
    }
    public void LoadFromFile() {
      try {
        string sFileName = MarketName.TradeHistoryFileName();
        if (File.Exists(sFileName)) {
          StreamReader sIn = new StreamReader(sFileName);
          string sLine = sIn.ReadLine();
          while (sLine != null) {
            try {
              ExchangeOrderResult x = sLine.TryParse();
              if (x is ExchangeOrderResult) {
                this[x.OrderId] = x;
              }
            } catch (Exception ee) {
              ee.toAppLog("Orders.LoadFromFile.Inner");
            }
            sLine = sIn.ReadLine();
          }
        }
      } catch (Exception e) {
        e.toAppLog("Orders.LoadFromFile");
      }
    }
    public void SaveToFile(string sFileName) {
      try {
        foreach (KeyValuePair<string, object> kvp in this.OrderBy(value => ((ExchangeOrderResult)value.Value).OrderDate)) {
          try {
            ((ExchangeOrderResult)kvp.Value).toDataLine().toTextFileLine(sFileName);
          } catch (Exception ee) {
            ee.toAppLog("Orders.SaveToFile.atLine");
          }
        }
      } catch (Exception e) {
        e.toAppLog("Orders.SaveToFile");
      }
    }
    public void Add(ExchangeOrderResult eor) {
      Changed = true;
      if (!Contains(eor.OrderId)) {
        this[eor.OrderId] = eor;
        if(eor.Result != ExchangeAPIOrderResult.Pending) { 
          eor.toDataLine().toTradeHistoyFile(eor.MarketSymbol);    // log it.
        }        
      } else {
        this[eor.OrderId] = eor;
      }      
    }
    public new ExchangeOrderResult this[string aKey] { get { return (ExchangeOrderResult)base[aKey]; } set { base[aKey] = value; } }
  }

  public class CBooksQueue : CQueue {
    public CBooksQueue() : base() {
    }
    public ExchangeOrderBook Add(ExchangeOrderBook aObj) {
      Nonce++;
      base.TryAdd(Nonce, aObj);
      base[Nonce] = aObj;
      return aObj;
    }
    public new ExchangeOrderBook Pop() {
      Object aR = null;
      if (Keys.Count > 0) {
        base.TryRemove(base.Keys.First(), out aR);
      }
      return (ExchangeOrderBook)aR;
    }
  }

  public class CMarket : CObject {
    public Form1 Owner;
    public string MarketName    { get { return base["MarketName"].toString(); }    set { base["MarketName"] = value; } }
    public string BaseCurrency  { get { return base["BaseCurrency"].toString(); }  set { base["BaseCurrency"] = value; } }
    public double BaseCurrencyBal { get { return (Owner.Balances.Contains(BaseCurrency) ? Owner.Balances[BaseCurrency].DBQuote:0);  }}

    public string QuoteCurrency { get { return base["QuoteCurrency"].toString(); } set { base["QuoteCurrency"] = value; } }
    public double QuoteCurrencyBal { get { return (Owner.Balances.Contains(QuoteCurrency)? Owner.Balances[QuoteCurrency].DBQuote : 0); } }

    public decimal Ask           { get { return base["Ask"].toDouble().toDecimal(); }       set { base["Ask"] = value; } }
    public decimal PriceLast     { get { return base["PriceLast"].toDouble().toDecimal(); } set { base["PriceLast"] = value; } }
    public decimal Bid           { get { return base["Bid"].toDouble().toDecimal(); ; }     set { base["Bid"] = value; } }

    public decimal dAsksTotal { get { return (from x in Asks select x.Value.Price * x.Value.Amount).Sum(); } }
    public decimal dBidsTotal { get { return (from x in Bids select x.Value.Price * x.Value.Amount).Sum(); } }

    public decimal AsksTotal { get { return base["AsksTotal"].toDouble().toDecimal(); } set { base["AsksTotal"] = value; } }
    public decimal BidsTotal { get { return base["BidsTotal"].toDouble().toDecimal(); } set { base["BidsTotal"] = value; } }

    public decimal AsksChanged { get { return base["AsksChanged"].toDouble().toDecimal(); } set { base["AsksChanged"] = value; } }
    public CAvgDecimalCache AskAvgChange { get { return (CAvgDecimalCache)base["AskAvgChange"]; } set { base["AskAvgChange"] = value; } }
    public decimal BidsChanged { get { return base["BidsChanged"].toDouble().toDecimal(); } set { base["BidsChanged"] = value; } }
    public CAvgDecimalCache BidsAvgChange { get { return (CAvgDecimalCache)base["BidsAvgChange"]; } set { base["BidsAvgChange"] = value; } }

    public decimal VolQuote { get { return base["VolQuote"].toDouble().toDecimal(); ; } set { base["VolQuote"] = value; } }
    public decimal VolBase { get { return base["VolBase"].toDouble().toDecimal(); ; } set { base["VolBase"] = value; } }

    public bool Selected { get { return (bool)base["Selected"].toBoolean();} set { 
        if (value) { 
          Owner.Markets.ClearSelection();
        }
        base["Selected"] = value; 
    } }
    public bool Sparked { get { return (bool)base["Sparked"].toBoolean(); } set { base["Sparked"] = value; } }
    
    public ExchangeTicker TickerLast;
    public CTickerCache Tickers;

    public SortedDictionary<decimal, ExchangeOrderPrice> Asks = null;
    public SortedDictionary<decimal, ExchangeOrderPrice> Bids = null;

    public CMarketTradesCache MarketTrades = null;       
    public COrders TradeHisory = null;    
    public CTradeHistoryManager QuoteShards = null;

    public COrders OpenOrders = null;

    public CMarket(Form1 aOwner, string sMarketName) : base() {
      Owner = aOwner;
      MarketName = sMarketName;
      Selected = false;
      Tickers = new CTickerCache();
      MarketTrades = new CMarketTradesCache();
      TradeHisory = new COrders(MarketName, true);
      QuoteShards = new CTradeHistoryManager(this, MarketName, 0.0025);
      OpenOrders = new COrders(MarketName, false);

      AskAvgChange = new CAvgDecimalCache(); // { get { return (CAvgDecimalCache)base["AskAvgChange"]; } set { base["AskAvgChange"] = value; } }
      AskAvgChange.Size = 20;
      BidsAvgChange = new CAvgDecimalCache(); // { get { return (CAvgDecimalCache)base["BidsAvgChange"]; } set { base["BidsAvgChange"] = value; } }
      BidsAvgChange.Size = 20;

  }
  public void AddTicker(ExchangeTicker et) { 
      Tickers.Add(et);
      ExchangeVolume ev = et.Volume;
      TickerLast = et;
      Ask = et.Ask;
      Bid = et.Bid;
      PriceLast = et.Last;
      BaseCurrency = ev.BaseCurrency;
      QuoteCurrency = ev.QuoteCurrency;
      VolBase = ev.BaseCurrencyVolume;
      VolQuote = ev.QuoteCurrencyVolume;      
      if (!Sparked) { Sparked = true; }
    }

    public void ShitChanged() { 
      double QuotedCurToFind = BaseCurrencyBal;
      QuoteShards.SetToBalance(QuotedCurToFind);
    }
  }

  public class CMarkets : CObject {
    public CMarkets() : base() {
    }
    public void ClearSelection() {       
      foreach(string sMarket in Keys) { 
        if (((CMarket)base[sMarket]).Selected) {
          ((CMarket)base[sMarket]).Selected = false;
        }
      }
    }
    public new CMarket this[string aKey] { get { return (CMarket)base[aKey]; } set { base[aKey] = value; } }     
  }

  #endregion

  #region Balances
  
  public class CBalances : CObject {
    public class BaseRate {
      CBalances Owner;
      public BaseRate(CBalances aOwner) { Owner = aOwner; }
      public double USDBTCRate { get { 
          CMarket x = Owner.Owner.Markets["USD-BTC"];
          return (x is CMarket ? x.PriceLast.toDouble():4200); } }      
      public double toUSD(double aBTC) { return aBTC * USDBTCRate; }      
    }
    public class CBalance : CObject {
      public CBalances Owner;
      public string Currency { get { return base["Currency"].toString(); } set { base["Currency"] = value; } }
      public double BitcoinValue { get { return base["BitcoinValue"].toDouble(); } set { base["BitcoinValue"] = value; } }
      public double QuoteAvailable { get { return base["QuoteAvailable"].toDouble(); } set { base["QuoteAvailable"] = value; } }
      public double QuoteOnOrders { get { return base["QuoteOnOrders"].toDouble(); } set { base["QuoteOnOrders"] = value; } }
      public double DBQuote { get { return base["DBQuote"].toDouble(); } set { base["DBQuote"] = value; } }
      public double PercentOfTotal { get { return base["PercentOfTotal"].toDouble(); } set { base["PercentOfTotal"] = value; } }
      public CBalance(CBalances aOwner, string aCurrency, Balance aBalance) : base() {
        Owner = aOwner;
        Currency = aCurrency;
        //  aBalance.QuoteAvailable;
        //BitcoinValue = aBalance.BitcoinValue;
        QuoteAvailable = aBalance.QuoteAvailable;
        QuoteOnOrders = aBalance.QuoteOnOrders;
        DBQuote = aBalance.QuoteAvailable;
        if ((Currency == "USD") && (QuoteAvailable > 0)) {
        //  BitcoinValue = aBalance.QuoteAvailable / ((Owner.aBR.USDCBTCRate != 0) ? Owner.aBR.USDCBTCRate : 2500);
          QuoteAvailable = aBalance.QuoteAvailable;
          QuoteOnOrders = aBalance.QuoteOnOrders;
          DBQuote = aBalance.QuoteAvailable + aBalance.QuoteOnOrders;
        } else {
        //  BitcoinValue = aBalance.BitcoinValue;
          QuoteAvailable = aBalance.QuoteAvailable;
          QuoteOnOrders = aBalance.QuoteOnOrders;
          DBQuote = aBalance.QuoteAvailable+aBalance.QuoteOnOrders;
        }
      }  // CBalancd Constructor.      
    }   // CBalance class   
    private Form1 Owner;
    public BaseRate aBR;
    public double TotalBTCValue =0;
    public double TotalUSDValue = 0;
    public CBalances(Form1 aOwner): base() {
      Owner = aOwner;
      aBR = new BaseRate(this);
    }
    public new CBalance this[string aKey] { get { return (CBalance)base[aKey]; } set { base[aKey] = value; } }
    public void AddUpdate(string sCurrency, Balance ReportedBalance) {
      if (!Contains(sCurrency)) {
        this[sCurrency] = new CBalances.CBalance(this, sCurrency, ReportedBalance);
      } else {
        CBalances.CBalance wb = (CBalances.CBalance)this[sCurrency];
        if ((sCurrency == "USD") && (ReportedBalance.QuoteAvailable > 0)) {
          wb.BitcoinValue = 0; // ReportedBalance.QuoteAvailable / ((aBR.USDCBTCRate != 0) ? aBR.USDCBTCRate : 5000);
          wb.QuoteAvailable = ReportedBalance.QuoteAvailable;
          wb.QuoteOnOrders = ReportedBalance.QuoteOnOrders;
          wb.DBQuote = ReportedBalance.QuoteAvailable + ReportedBalance.QuoteOnOrders;
        } else {
          wb.BitcoinValue = 0;// ReportedBalance.BitcoinValue;
          wb.QuoteAvailable = ReportedBalance.QuoteAvailable;
          wb.QuoteOnOrders = ReportedBalance.QuoteOnOrders;
          wb.DBQuote = ReportedBalance.QuoteAvailable + ReportedBalance.QuoteOnOrders;
        }
      }
    }
    public new void Remove(string sCurrency) {
      if (Contains(sCurrency)) {
        base.Remove(sCurrency);
      }
    }
    public void RecomputeBases() {
      double z = 0;
      string sMarket ="";
      foreach (string skey in Keys) {
        if (skey == "BTC") {
          this[skey].BitcoinValue = this[skey].QuoteAvailable;
        } else if (skey == "USD") {
          this[skey].BitcoinValue = this[skey].QuoteAvailable / ((aBR.USDBTCRate != 0) ? aBR.USDBTCRate : 5000);          
        } else {
          sMarket = "BTC-"+skey;
          CMarket aM = Owner.Markets[sMarket];
          if (aM is CMarket) { 
            this[skey].BitcoinValue = this[skey].DBQuote * aM.PriceLast.toDouble();          
          } else {
            this[skey].BitcoinValue = 0;
          }
        }
        z = z + this[skey].BitcoinValue;
      }
      TotalBTCValue = z;
      TotalUSDValue = aBR.toUSD(this.TotalBTCValue);      
      foreach (string skey in Keys) {
        this[skey].PercentOfTotal = this[skey].BitcoinValue / (TotalBTCValue == 0 ? 1 : TotalBTCValue);
      }

      if (Owner is Form1 ) {
        if(Owner.FocusedMarket is CMarket) { 
          Owner.FocusedMarket.ShitChanged();
        }
      }
    }
  }

  #endregion 

  #region Trade History Manager 

  public class CTradeHistoryManager : CBook {    
    public class Shard{
      public CTradeHistoryManager Owner;
      public decimal Price;
      public decimal Quantity;
      public decimal Fee;
      public Shard(CTradeHistoryManager aOwner, decimal aPricePaid, decimal aQuantity, decimal aFee) { 
        Owner = aOwner;
        Price = aPricePaid;
        Quantity = aQuantity;
        Fee = aFee;
      }
    }
    public CMarket Owner;
    public string MarketName;    
    public double TradeFeePercent;
    public CTradeHistoryManager(CMarket aOwner, string aMarketName, double aTradeFeePercent) : base() { 
      Owner = aOwner;
      MarketName = aMarketName;
      TradeFeePercent = aTradeFeePercent;
    }
    public new Shard this[decimal key] { get{ return (Shard) base[key]; }  set { base[key]=value; } }
    public void AddTrade(ExchangeOrderResult aResult) { 
      decimal aFee = aResult.Fees;
      decimal aVol =  aResult.AmountFilled;
      decimal aPrice = aResult.Price;
      if (aResult.IsBuy) {
        if (Contains(aPrice)){ 
          Shard aShard = this[aPrice];
          aShard.Quantity = aShard.Quantity + aVol;
          aShard.Fee = aShard.Fee + aFee;
        } else {
          this[aPrice] = new Shard(this, aPrice, aVol, aFee);
        }
      }
    }
    public void SetToBalance(double aQuoteBalance) {
      double QuoteVolToFind = aQuoteBalance;
      IEnumerable<KeyValuePair<string, object>> lEOR =  Owner.TradeHisory.Where(x => ((ExchangeOrderResult)x.Value).IsBuy).OrderByDescending( x => ((ExchangeOrderResult)x.Value).OrderDate);
      Clear();
      double ShardSum = 0;      
      foreach(object x in lEOR) {         
        ExchangeOrderResult eor = (ExchangeOrderResult)((KeyValuePair<string, object>)x).Value;
        if (ShardSum + eor.AmountFilled.toDouble() <= QuoteVolToFind) { 
          AddTrade(eor);
          ShardSum = ShardSum + eor.AmountFilled.toDouble();
        } else {
          if (ShardSum >= QuoteVolToFind) { break;}
          decimal aPrice = eor.Price;
          decimal aVol = (QuoteVolToFind.toDecimal() - ShardSum.toDecimal() + 0.00000001m) ;
          decimal aFee = eor.Fees * aVol/eor.AmountFilled;           
          this[aPrice] = new Shard(this, aPrice, aVol, aFee);
          ShardSum = ShardSum + aVol.toDouble();
          break;
        }
      }

    }
  }

  #endregion

}
