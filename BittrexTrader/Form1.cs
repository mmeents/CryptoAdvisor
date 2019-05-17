using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using C0DEC0RE;
using ExchangeSharp;

namespace BittrexTrader {

  public partial class Form1 : Form {

    string[] MarketFilter = { "USD-BTC", "USD-ETH", "USD-XMR", "USD-LTC", "USD-ADA",
      "BTC-LTC", "BTC-STRAT", "BTC-ADA", "BTC-VRC", "BTC-RVN", 
      //"BTC-ETH", "BTC-SC", "BTC-TRX",
      //"BTC-SOLVE", "BTC-WAVES", "BTC-ZEC", "BTC-OMG",
      "ETH-ADA" };

    public Form1() {
      InitializeComponent();
      LoadSettings();
    }

    #region Form Variables and Setup
    public CXCore XCore;
    public MMCredentialStore Settings;
    //public ExchangePoloniexAPI papi;
    public ExchangeBittrexAPI papi;
    public ExchangeBittrexAPI BittrexBookAPI;
    public ExchangeBittrexAPI BittrexTradeAPI;
    public ExchangeBittrexAPI BittrexTickerAPI;

    public CMarkets Markets;
    public CMarket FocusedMarket = null;    
    public CBalances Balances = null;
    public CTickerQueue TickersLanding;
    public CTradesQueue TradesLanding;
    public CBooksQueue BooksLanding;

    public IWebSocket wsTickers;
    public IWebSocket wsTradeFlow;    
    public IWebSocket wsOrderBooks;

    public Action<IReadOnlyCollection<KeyValuePair<String, ExchangeTicker>>> ActionTickerCallback;
    public Action<KeyValuePair<String, ExchangeTrade>> ActionTradeCallback;
    public Action<ExchangeOrderBook> ActionOrderBookCallback;

    public Int32 iDisplayMode = 0;
    public bool bInWait = false, bInStartup = true, bHasMarkets = false, bTickersUp = false;
    public Font fCur10; Font fCur9; Font fCur8; Font fCur7; Font fCur6; Font fLogo17; Font fLogo20;
    public Color ColorDefBack;

    public float fWidth = 0, fHeight = 0;
    public double f20Height = 0.2;
    public double f05Height = 0.065;
    public double f15Height = 0.145;
    public double f20Width = 0.2;
    public double f15Width = 0.15;
    public double f05Width = 0.05;

    public string sWaitDesc;
    public string sOrderToCancel = "";
    string SettingsFilePath;
    string SettingsFileName;
    public SizeF OffsetA, sfText; 

    public void LoadSettings() {

      XCore = new CXCore(this);
      Markets = new CMarkets();
      Balances = new CBalances(this);
      TickersLanding = new CTickerQueue();
      TradesLanding = new CTradesQueue();
      BooksLanding = new CBooksQueue();

      sWaitDesc = "";
      fLogo17 = new Font("Calisto MT", 17); fLogo20 = new Font("Calisto MT", 20, FontStyle.Bold);
      fCur10 = new Font("Courier New", 10); fCur9 = new Font("Courier New", 9); fCur8 = new Font("Courier New", 8);
      fCur7 = new Font("Courier New", 7); fCur6 = new Font("Courier New", 6);
      ColorDefBack = ColorTranslator.FromHtml("#08180F");     
      
      edPriceBuy.Visible = false;
      edQuantityBuy.Visible = false;
      edTotalBuy.Visible = false;

      SettingsFilePath = MMExt.MMConLocation();
      if (!Directory.Exists(SettingsFilePath)) Directory.CreateDirectory(SettingsFilePath);
      SettingsFileName = "\\BittrexTraderSettings.ini";      

      if (!File.Exists(SettingsFilePath+ SettingsFileName)) { // need api keys. 
        iDisplayMode = 10;
      } else {  // need password to unlock keys. 
        iDisplayMode = 20;
      }

    }

    private void Form1_Shown(object sender, EventArgs e) {
      XCore.Go();
      bInStartup = false;
      Form1_ResizeEnd(null, null);
    }

    private void Form1_FormClosed(object sender, FormClosedEventArgs e) {
      TakeDownSockets();
      SaveControls();
    }

    #endregion
    
    #region XCore Command Implementation

    public void ProcessXCoreCmds(CCmd Cmd) {
      try { 
        switch ( Cmd.Cmd){ 
          case "DoRefresh":
            DoRefresh();
            break;
          case "LoadMarkets":
            DoLoadMarketsAsync();
            break;
          case "SetTraderLoose":
            DoStartTrader();
            break;
          case "ProcessTickers":
            DoProcessTickers();
            break;
          case "ProcessTrades":
            DoProcessTrades();
            break;
          case "ProcessBooks":
            DoProcessBooks();
            break;
          case "DoGetBalances":
            DoGetBalancesBittrexAsync();
            break;
          case "DoGetCompletedTrades":          
            DoGetTradeHistoryAsync(FocusedMarket.MarketName);
            break;
          case "DoGetOpenOrders":
            DoGetOpenOrdersAsync();
            break;
          case "DoCancelOrder": 
            if (Cmd.Contains("OrderId")) {
              DoCancelOrderAsync((string)Cmd["OrderId"]);
            }
            break;
          case "DoPostOrder":         
            DoPostOrderAsync(Cmd);
            break;
          case "DoPerMinStats":
            DoPerMinStats();
            break;
          case "DoPer15SecStats":
            DoPer15SecStats();
            break;
        };
      } catch (Exception e) {
        e.toAppLog("XCoreCmd "+Cmd.Cmd);
      }
    }
    
    public bool bHasTradeAPI = false;
    public bool bHasBooksAPI = false;
    public void DoStartTrader() {
      string es = "0";
      try { 
        if (FocusedMarket is CMarket) {

          DoGetTradeHistoryAsync(FocusedMarket.MarketName);

          es = "0";
          string[] saMarket = { FocusedMarket.MarketName};
          es = "1";
          if (!bHasBooksAPI) {
            es = "3";
            string kpPolo = Settings["BittrexKP"];
            es = "4";
            string sPub = kpPolo.ParseString(" ", 0);
            string sPri = kpPolo.ParseString(" ", 1);
            es = "5";
            BittrexBookAPI = new ExchangeBittrexAPI();
            es = "6";          
            BittrexBookAPI.LoadAPIKeysUnsecure(sPub, sPri);
            bHasBooksAPI = true;
          }
          es = "7";
          ActionOrderBookCallback = delegate (ExchangeOrderBook aEOB) { BooksLanding.Add(aEOB);     };
          wsOrderBooks = BittrexBookAPI.GetFullOrderBookWebSocket(ActionOrderBookCallback, 20, saMarket);

          if (!bHasTradeAPI) {
            es = "8";
            string kpPolo = Settings["BittrexKP"];
            es = "9";
            string sPub = kpPolo.ParseString(" ", 0);
            string sPri = kpPolo.ParseString(" ", 1);
            es = "10";
            BittrexTradeAPI = new ExchangeBittrexAPI();
            es = "11";
            BittrexTradeAPI.LoadAPIKeysUnsecure(sPub, sPri);
            bHasTradeAPI = true;
          }
          es = "12";
          ActionTradeCallback = delegate (KeyValuePair<String, ExchangeTrade> r) { TradesLanding.Add(r); };
          wsTradeFlow = BittrexTradeAPI.GetTradesWebSocket(this.ActionTradeCallback, saMarket);

          XCore.AddCmd(new CCmd("DoGetOpenOrders"));
        }
      } catch(Exception e) { 
        e.toAppLog("Start Trader "+es);
      }
    }

    public void DoStopTrader() {
      if (wsTradeFlow is IWebSocket) {
        wsTradeFlow.Dispose();
      }
      if (wsOrderBooks is IWebSocket) {
        wsOrderBooks.Dispose();
      }
    }

    public void TakeDownSockets() {
      try {
        if (wsTickers is IWebSocket) {
          wsTickers.Dispose();
        }
        if (wsTradeFlow is IWebSocket) {
          wsTradeFlow.Dispose();
        }
        if (wsOrderBooks is IWebSocket) {
          wsOrderBooks.Dispose();
        }


      } catch (Exception e) {
        e.toAppLog("TakeDownSockets");
      }
    }

    public void DoChkFeeds() { 
      if((!bInStartup) && (TickersLanding is CTickerQueue)&& (TickersLanding.Count > 0)) {
        XCore.AddCmd(new CCmd("ProcessTickers"));
      }
      if ((!bInStartup) && (TradesLanding is CTradesQueue) && (TradesLanding.Count > 0)) {
        XCore.AddCmd(new CCmd("ProcessTrades"));
      }
      if ((!bInStartup) && (BooksLanding is CBooksQueue) && (BooksLanding.Count > 0)) {
        XCore.AddCmd(new CCmd("ProcessBooks"));
      }

      if(Balances.TotalBTCValue == 0) {
        Balances.RecomputeBases();
      }
    }

    public bool bHasTickerAPI = false;
    public async void DoLoadMarketsAsync() {
      if (!bHasTickerAPI) { 
        string es = "0";
        string kpPolo = Settings["BittrexKP"];
        es = "7";
        string sPub = kpPolo.ParseString(" ", 0);
        string sPri = kpPolo.ParseString(" ", 1);
        es = "8";
        BittrexTickerAPI = new ExchangeBittrexAPI();
        es = "9";
        BittrexTickerAPI.LoadAPIKeysUnsecure(sPub, sPri);
        bHasTickerAPI = true;
      }

      string ms = "";
      var xTask = await BittrexTickerAPI.GetTickersAsync();
      foreach (KeyValuePair<string, ExchangeTicker> x in xTask) {
        try {                    
          ms = x.Key;      
          if (MarketFilter.Contains(ms)) { 
            if (!Markets.Contains(ms)) {
              CMarket aMarket = new CMarket(this, ms);            
              Markets[ms] = aMarket;
            }
            ExchangeTicker et = x.Value;
            Markets[ms].AddTicker(et);
          }          
        } catch (Exception e) {
          e.toAppLog("MarketSummary foreach on " + ms);
        } 
      }

      if (!bTickersUp) { 
        ActionTickerCallback = delegate (IReadOnlyCollection<KeyValuePair<String, ExchangeTicker>> r) { TickersLanding.Add(r); };
      //  wsTickers = papi.GetTickersWebSocket(ActionTickerCallback);
        wsTickers = BittrexTickerAPI.GetTickersWebSocket(ActionTickerCallback, MarketFilter);
        bTickersUp = true;
      }
      bHasMarkets = true;    
    }

    public void DoProcessTickers() { 
      string evalTest = "0";
      while (TickersLanding.Count > 0) {        
        try {
          IReadOnlyCollection<KeyValuePair<String, ExchangeTicker>> r = TickersLanding.Pop();
          evalTest = "2";
          if (r != (null) && (r is IReadOnlyCollection<KeyValuePair<String, ExchangeTicker>>)) {
            evalTest = "3";
            foreach (KeyValuePair<string, ExchangeTicker> kvp in r) {
              if (MarketFilter.Contains(kvp.Key)) { 
                evalTest = "4";
                if ((Markets[kvp.Key] != null) && (Markets[kvp.Key].MarketName == kvp.Key)) {
                  evalTest = "5";
                  Markets[kvp.Key].AddTicker(kvp.Value);
                }
              }
            }
          }
        } catch (Exception e) {
          e.toAppLog("Process Ticker " + evalTest);
        }
      }
    }

    public void DoProcessTrades() { 
      if (TradesLanding.Count > 0) { 
        while (TradesLanding.Count > 0) {
          KeyValuePair<string, ExchangeTrade> eKVP = TradesLanding.Pop();
          FocusedMarket.MarketTrades.Add(eKVP.Value);
        }        
      }
    }

    public void DoProcessBooks() {
      string es = "1";
      try {
         ExchangeOrderBook r = null;
         while (BooksLanding.Count > 0) {
          r = BooksLanding.Pop();
         }          
         if (r is ExchangeOrderBook) {
           FocusedMarket.Asks = r.Asks;
           FocusedMarket.Bids = r.Bids;
         }
        es = "2";
      } catch (Exception e) {
        e.toAppLog("ProcessMarketTrades " + es);
      }
    }

    public async void DoGetBalancesBittrexAsync() { 
      var xTask = await papi.OnGetBalancesAsync();
      try {
        foreach (string sCur in xTask.Keys) {
          if (xTask[sCur].QuoteAvailable > 0.0005) {
            Balances.AddUpdate(sCur, xTask[sCur]);       
          } else {
            Balances.Remove(sCur);
          }
        }

        foreach (string k in Balances.Keys) {
          if (!xTask.Keys.Contains(k)) {
            Balances.Remove(k);
          }
        }

        Balances.RecomputeBases();


      } catch (Exception e) {
        e.toAppLog("DoGetBalance");
      }      
    }

    public bool DGTHFTT = false;
    public async void DoGetTradeHistoryAsync(string sMarket) {
      CMarket aMarket = Markets[sMarket];
      if (aMarket is CMarket) {  
        try { 
          IEnumerable<ExchangeOrderResult> lEOR = await papi.GetCompletedOrderDetailsAsync( sMarket);        
          aMarket.TradeHisory.Changed = false;
          foreach(ExchangeOrderResult x in lEOR) {
            aMarket.TradeHisory.Add(x);
          }
          if ((aMarket.TradeHisory.Changed)||(!DGTHFTT)) {
            aMarket.ShitChanged();
            DGTHFTT = true;
          }        
        } catch (Exception e) { 
          e.toAppLog("DoGetTradeHist");
        }
      }
    }

    public async void DoGetOpenOrdersAsync() { 
      if (FocusedMarket is CMarket) {
        IEnumerable<ExchangeOrderResult> lEOR = await papi.GetOpenOrderDetailsAsync(FocusedMarket.MarketName);
        foreach(ExchangeOrderResult x in lEOR) { 
          if (!FocusedMarket.OpenOrders.Contains(x.OrderId)) {
            FocusedMarket.OpenOrders.Add(x);
          }
        }
        foreach(string sKey in FocusedMarket.OpenOrders.Keys) {
          bool bFound = false;
          foreach (ExchangeOrderResult x in lEOR) {
            if (x.OrderId == sKey) { bFound = true; break;}
          }
          if (!bFound) {
            FocusedMarket.OpenOrders.Remove(sKey);
          }
        }
        
      }
    }

    public async void DoPostOrderAsync(CCmd aCmd) {
      ExchangeOrderRequest aOrder = new ExchangeOrderRequest();
      aOrder.OrderType = OrderType.Limit;
      aOrder.MarketSymbol = (string)aCmd["MarketName"];
      aOrder.IsBuy = (bool)aCmd["IsBuy"];      
      aOrder.Amount = (decimal)aCmd["Amount"];
      aOrder.Price = (decimal)aCmd["Price"];
      try { 
        ExchangeOrderResult aResult = await papi.PlaceOrderAsync( aOrder );
      } catch(Exception e) { 
        e.toAppLog("DoPlaceOrder");
      }

      XCore.AddCmd(new CCmd("DoGetOpenOrders"));
      XCore.AddCmd(new CCmd("DoGetBalances"));

    }

    public async void DoCancelOrderAsync(string aOrderId) { 
      if (FocusedMarket.OpenOrders.Contains(aOrderId)) {
        try { 
        await papi.CancelOrderAsync(aOrderId);
        } catch (Exception e) {  
          e.toAppLog("CancelOrder");
        }
        CCmd aCmd = new CCmd("DoGetOpenOrders");
        aCmd["InvokeBalances"] = true;
        XCore.AddCmd(aCmd);
      }
    }

    public void DoPerMinStats() { 
      if (FocusedMarket is CMarket) {        
      }
    }

    public void DoPer15SecStats() {
      if (FocusedMarket is CMarket) {
        if (FocusedMarket.Asks is SortedDictionary<decimal, ExchangeOrderPrice>) {
          decimal dAsks = FocusedMarket.dAsksTotal;
          FocusedMarket.AsksChanged = dAsks - FocusedMarket.AsksTotal;
          if (FocusedMarket.AsksTotal != 0) { 
            FocusedMarket.AskAvgChange.Add(dAsks - FocusedMarket.AsksTotal);
          }
          FocusedMarket.AsksTotal = dAsks;
        }
        if (FocusedMarket.Bids is SortedDictionary<decimal, ExchangeOrderPrice>) {
          decimal dBids = FocusedMarket.dBidsTotal;
          FocusedMarket.BidsChanged = dBids - FocusedMarket.BidsTotal;
          if (FocusedMarket.BidsTotal != 0) { 
            FocusedMarket.BidsAvgChange.Add(dBids - FocusedMarket.BidsTotal);
          }
          FocusedMarket.BidsTotal = dBids;
        }
      }
    }

    #endregion

    #region Form Objects event handlers

    private void Form1_MouseDown(object sender, MouseEventArgs e) {
      fWidth = DisplayRectangle.Width;
      fHeight = DisplayRectangle.Height;
      f20Height = fHeight * 0.2;
      f05Height = fHeight * 0.065;
      f15Height = fHeight * 0.145;
      f20Width = fWidth * 0.2;
      f15Width = fWidth * 0.15;
      f05Width = fWidth * 0.05;
      Boolean handledClick = false;

      Int32 iRow = 0;
      Int32 iMin =0;
      Int32 iMax = 0;

      if ((iDisplayMode == 30) && (!handledClick)) {
        KeyValuePair<string, object>[] xList = Markets.ToArray();
        #region Ordered list 10x10 
        Int32 iCol = Convert.ToDouble(e.X / (fWidth / 8)).toInt32T();
        iRow = 0;
        if (e.Y < f05Height) {
          iRow = 20;
        } else if (e.Y < f20Height + 3 * f15Height) {
          iRow = Convert.ToDouble((e.Y - f05Height) / (f15Height / 2.5)).toInt32T();
        } else if (e.Y < 2 * f20Height + 2 * f15Height) {
          iRow = 10;
        } else if (e.Y < 2 * f20Height + 3 * f15Height) {
          iRow = 11;
        } else if (e.Y < 2 * f20Height + 4 * f15Height) {
          iRow = 12;
        }

        Int32 iRank = ((iRow * 8) + iCol);
        try {
          if ((iRank >= 0) && (Markets.Count > iRank) && (iRow <= 9)) {
            KeyValuePair<string, object> aKVP = xList.ElementAt(iRank);
            CMarket aMarket = (CMarket)aKVP.Value;            
            aMarket.Selected = true;
            FocusedMarket = Markets[aMarket.MarketName];
            XCore.AddCmd(new CCmd("SetTraderLoose"));
            iDisplayMode = 40;            
          }

        } catch { }
        #endregion
      }

      if ((iDisplayMode == 40) && (!handledClick)) {
        #region  Top of the screen takes you back.
        if (e.Y <= f05Height) {
          iDisplayMode = 30;
          DoStopTrader();
        }
        #endregion

        #region OnClick Shard
        Int32 iCount = FocusedMarket.QuoteShards.Count;
        if ((e.X <= Convert.ToInt32(3 * f05Width)) &&
            (e.Y >= f05Height + (3 * sfText.Height)) &&
            (e.Y <= f05Height + (3 * sfText.Height) + (sfText.Height * iCount))
           ) {
          Int32 iY = e.Y - Convert.ToInt32(f05Height + 3 * sfText.Height);
          Int32 iIndex = (iY / sfText.Height).toDouble().toInt32T();
          if ((iIndex >= 0) && (iIndex < iCount)) {
            decimal aKey = FocusedMarket.QuoteShards.ElementKeyAt(iIndex);
            CTradeHistoryManager.Shard s = FocusedMarket.QuoteShards[aKey];
            edQuantitySell.Value = s.Quantity;
            edQuantityBuy.Value = edQuantitySell.Value;
          }
        }
        #endregion

        #region OnClick Open Order processing
        Int32 iLeft = Convert.ToInt32(2 * f20Width - (f05Width / 2));
        Int32 iTop = Convert.ToInt32(fHeight / 2);
        Int32 iWidth = Convert.ToInt32(f20Width + 2 * f05Width);
        Int32 iHeight = Convert.ToInt32(fHeight / 2 - f05Height - 2);
        iCount = FocusedMarket.OpenOrders.Count;
        if (iCount > 0) {
          iMin = Convert.ToInt32(iTop + (-3 * sfText.Height) + (sfText.Height / 2));
          iMax = Convert.ToInt32(iMin + (iCount * sfText.Height));
          if ((e.X >= iLeft) && (e.X <= (iWidth + iLeft)) && (e.Y > iMin) && (e.Y <= iMax)) {
            Int32 iY = e.Y - iMin;
            Int32 iIndex = (iY / sfText.Height).toDouble().toInt32T();
            if ((iIndex >= 0) && (iIndex < iCount)) {
              sOrderToCancel = FocusedMarket.OpenOrders.Keys.ElementAt(iIndex);
              btnCancel.Left = iLeft;
              btnCancel.Top = (iMin + (iIndex * sfText.Height)).toDouble().toInt32T();
              if (!btnCancel.Visible) { btnCancel.Visible = true; }
            }
          }
        }
        #endregion

        #region Click to Right of buy Controls
        iLeft = Convert.ToInt32(3 * f05Width);
        iTop = Convert.ToInt32(f05Height);
        iWidth = Convert.ToInt32(f20Width + f05Width);
        iHeight = Convert.ToInt32(2 * f05Width);        
        Int32 iLeftSell = Convert.ToInt32(3 * f20Width + 2 * f05Width);
        iMin = iTop;
        iMax = (iTop + 86);
        // Buy side
        if ((e.X >= iLeft + OffsetA.Width + 132) && (e.X <= (iWidth + iLeft)) && (e.Y > iMin) && (e.Y <= iMax)) {
          if (Balances.Contains(FocusedMarket.QuoteCurrency)) {
            decimal dBal = Balances[FocusedMarket.QuoteCurrency].QuoteAvailable.toDecimal();
            edQuantityBuy.Value = dBal / edPriceBuy.Value;
          }
        }
        // sell Side
        if ((e.X >= iLeftSell + OffsetA.Width + 132) && (e.X <= (iWidth + iLeftSell)) && (e.Y > iMin) && (e.Y <= iMax)) {
          if (Balances.Contains(FocusedMarket.BaseCurrency)) {
            decimal dBal = Balances[FocusedMarket.BaseCurrency].QuoteAvailable.toDecimal();
            if (dBal > 0.0005m) {
              edQuantitySell.Value = dBal;
            }            
          }
        }

        #endregion

        #region Click on Buy Book
        iLeft = Convert.ToInt32(3 * f05Width);
        iTop = Convert.ToInt32(f05Height * 5);
        iWidth = Convert.ToInt32(f20Width + f05Width);
        iHeight = Convert.ToInt32(f20Height * 4 + f05Height * 2);
        if ((FocusedMarket != null) && (FocusedMarket.Bids is SortedDictionary<decimal, ExchangeOrderPrice>)) {
          iCount = FocusedMarket.Bids.Count();
          if (iCount > 0) {
            iMin = iTop + sfText.Height.toInt32();
            iMax = (iTop + iHeight - 20);
            if ((e.X >= iLeft) && (e.X <= (iWidth + iLeft)) && (e.Y > iMin) && (e.Y <= iMax)) {
              Int32 iY = e.Y - iMin;
              Int32 iIndex = (iY / sfText.Height).toDouble().toInt32T() - 1;
              if ((iIndex >= 0) && (iIndex < iCount)) {
                decimal dSumSell = 0;
                decimal dBal = 0;
                if (Balances.Contains(FocusedMarket.BaseCurrency)) {
                  dBal = Balances[FocusedMarket.BaseCurrency].QuoteAvailable.toDecimal();
                  for (Int32 i = 0; i <= iIndex; i++) {
                    dSumSell = dSumSell + FocusedMarket.Bids.ElementAt(i).Value.Amount;
                    if (dSumSell >= dBal) {
                      dSumSell = dBal;
                      break;
                    }
                  }
                }
                decimal dElementKey = FocusedMarket.Bids.Keys.ElementAt(iIndex);
                ExchangeOrderPrice aPrice = FocusedMarket.Bids[dElementKey];
                edPriceBuy.Value = aPrice.Price;
                edPriceSell.Value = aPrice.Price;
                if (dSumSell != 0) {
                  edQuantitySell.Value = dSumSell;
                }
              }
            }
          }
        }
        #endregion

        #region Click on Ask Book
        iLeft = Convert.ToInt32(3 * f20Width + 2 * f05Width);
        if ((FocusedMarket != null) && (FocusedMarket.Asks is SortedDictionary<decimal, ExchangeOrderPrice>)) {
          iCount = FocusedMarket.Asks.Count();
          if (iCount > 0) {
            iMin = iTop + sfText.Height.toInt32();
            iMax = (iTop + iHeight - 20);
            if ((e.X >= iLeft) && (e.X <= (iWidth + iLeft)) && (e.Y > iMin) && (e.Y <= iMax)) {
              Int32 iY = e.Y - iMin;
              Int32 iIndex = (iY / sfText.Height).toDouble().toInt32T() - 1;
              if ((iIndex >= 0) && (iIndex < iCount)) {
                decimal dSumSell = 0;
                decimal dSumBuy = 0;
                decimal dBal = 0;
                decimal dBalQuote = 0;
                if (Balances.Contains(FocusedMarket.QuoteCurrency)) {
                  dBal = Balances[FocusedMarket.QuoteCurrency].QuoteAvailable.toDecimal();    // market base btc usd cur                                   
                }

                if (Balances.Contains(FocusedMarket.BaseCurrency)) {
                  dBalQuote = Balances[FocusedMarket.BaseCurrency].QuoteAvailable.toDecimal();   //target cur
                }

                for (Int32 i = 0; i <= iIndex; i++) {  // sum up count of quote 
                  KeyValuePair<decimal, ExchangeOrderPrice> x = FocusedMarket.Asks.ElementAt(i);
                  dSumSell = dSumSell + x.Value.Amount * x.Value.Price ;
                  if (dSumSell >= dBal) {
                    dSumSell = dBal;
                    break;
                  }
                }

                decimal dElementKey = FocusedMarket.Asks.Keys.ElementAt(iIndex);
                ExchangeOrderPrice aPrice = FocusedMarket.Asks[dElementKey];
                edPriceBuy.Value = aPrice.Price;
                edPriceSell.Value = aPrice.Price;
                if (dSumSell != 0) {
                  edQuantityBuy.Value = dSumSell / aPrice.Price;
                }
              }
            }
          }
        }
        #endregion
      }


    }

    private void Form1_ResizeEnd(object sender, EventArgs e) {
      Graphics g = this.CreateGraphics();
      try {
        fWidth = g.VisibleClipBounds.Width;
        fHeight = g.VisibleClipBounds.Height;
        f20Height = fHeight * 0.2;
        f05Height = fHeight * 0.065;
        f15Height = fHeight * 0.145;
        f20Width = fWidth * 0.2;
        f15Width = fWidth * 0.15;
        f05Width = fWidth * 0.05;
        OffsetA = g.MeasureString("  Quantity:", fCur10);
        sfText = g.MeasureString("1.00001111 ", fCur10);
      } finally {
        g.Dispose();
      }
    }

    private void btnContinue_Click(object sender, EventArgs e) {
      string es = "0";
      string sPub, sPri;

      if (iDisplayMode == 10) { 
        if ((textBox1.Text != "")&&(textBox2.Text != "") &&(textBox3.Text != "")) {
          try {
            es = "1";
            Settings = new MMCredentialStore(textBox1.Text, SettingsFileName);
            sPub = textBox2.Text;
            sPri = textBox3.Text;
            es = "2";            
            Settings["BittrexKP"] = sPub+" "+sPri;
            es = "3";
            //papi = new ExchangePoloniexAPI();
            papi = new ExchangeBittrexAPI();
            es = "4";
            papi.LoadAPIKeysUnsecure(sPub, sPri);
            iDisplayMode = 30;
            CCmd aLM = new CCmd("LoadMarkets");
            aLM["InvokeBalances"] = true;
            XCore.AddCmd(aLM);
            LoadControls();
          } catch (Exception ea) { 
            throw ea.toAppLog("btnContinue "+es);
          }          
        } else {
          MessageBox.Show("Editors cannot be empty.");
        }        
      }

      if (iDisplayMode == 20) {
        if (textBox1.Text != "") {
          try {
            es = "5";
            Settings = new MMCredentialStore(textBox1.Text, SettingsFileName);
            es = "6";
            string kpPolo = Settings["BittrexKP"];
            es = "7";
            sPub = kpPolo.ParseString(" ", 0);
            sPri = kpPolo.ParseString(" ", 1);
            es = "8";            
            papi = new ExchangeBittrexAPI();
            es = "9";
            papi.LoadAPIKeysUnsecure(sPub, sPri);            
            iDisplayMode = 30;
            CCmd aLM = new CCmd("LoadMarkets");
            aLM["InvokeBalances"] = true;
            XCore.AddCmd(aLM);
            LoadControls();
          } catch (Exception ee) {
            throw ee.toAppLog("btnContinue " + es);
          }
        } else {
          MessageBox.Show("Editors cannot be empty.");
        }
      }
    }
    private void LoadControls() { 
      if (Settings["BuyPrice"] != "") {
        edQuantityBuy.Value = Settings["QuantityBuy"].toDouble().toDecimal();
        edQuantitySell.Value = Settings["QuantitySell"].toDouble().toDecimal();
        edPriceBuy.Value = Settings["PriceBuy"].toDouble().toDecimal();
        edPriceSell.Value = Settings["PriceSell"].toDouble().toDecimal();
      }      
    }
    private void SaveControls() {      
      Settings["QuantityBuy"] = edQuantityBuy.Value.toStr8();
      Settings["QuantitySell"] = edQuantitySell.Value.toStr8(); 
      Settings["PriceBuy"] = edPriceBuy.Value.toStr8();
      Settings["PriceSell"]= edPriceSell.Value.toStr8();
    }

    private void btnReloadBalances_Click(object sender, EventArgs e) {
      XCore.AddCmd(new CCmd("DoGetBalances"));
    }
    private void btnReloadOpenOrders_Click(object sender, EventArgs e) {
      XCore.AddCmd(new CCmd("DoGetOpenOrders"));
    }
    private void btnReloadOrderHistory_Click(object sender, EventArgs e) {
      XCore.AddCmd(new CCmd("DoGetCompletedTrades"));
    }
        
    private void edQuantityBuy_ValueChanged(object sender, EventArgs e) {
      decimal aValB = edQuantityBuy.Value * edPriceBuy.Value;
      decimal aValS = edQuantitySell.Value * edPriceSell.Value;
      if(aValB + aValB*0.0025m != edTotalBuy.Value) {
        edTotalBuy.Value = aValB + aValB * 0.0025m;
      }
      if (aValS + aValS*0.0025m != edTotalSell.Value) {
        edTotalSell.Value = aValS+aValS * 0.0025m;
      }
    }
    private void edTotalBuy_ValueChanged(object sender, EventArgs e) {
      decimal aValB = edQuantityBuy.Value * edPriceBuy.Value;      
      if (aValB + (aValB * 0.0025m) != edTotalBuy.Value) {
        edQuantityBuy.Value = (aValB - aValB*0.0025m)/edPriceBuy.Value;        
      }
      btnBuy.Text = "Buy " + edQuantityBuy.Value.toStr2() + " @ " + (edPriceBuy.Value * 100000000).toInt32T().ToString();

      decimal aValS = edQuantitySell.Value * edPriceSell.Value;
      if (aValS + (aValS * 0.0025m) != edTotalSell.Value) {
        edTotalSell.Value = (aValS - aValS*0.0025m)/edPriceSell.Value;        
      }
      btnSell.Text = "Sell " + edQuantitySell.Value.toStr2() + " @ " + (edPriceSell.Value * 100000000).toInt32T().ToString();
    }

    private void btnCancel_Click(object sender, EventArgs e) {
      if (sOrderToCancel != "") {
        CCmd aCmd = new CCmd("DoCancelOrder");
        aCmd["OrderId"] = sOrderToCancel;
        XCore.AddCmd(aCmd);
        sOrderToCancel = "";
        btnCancel.Visible = false;
      }
    }
    private void btnBuy_Click(object sender, EventArgs e) {     
      decimal dTotal = edQuantityBuy.Value * edPriceBuy.Value;
      if (Balances.Contains(FocusedMarket.QuoteCurrency) && 
        ( dTotal.toDouble() <= Balances[ FocusedMarket.QuoteCurrency].QuoteAvailable )) {
        CCmd aCmd = new CCmd("DoPostOrder");
        aCmd["MarketName"] = FocusedMarket.MarketName;
        aCmd["IsBuy"] = true;
        aCmd["Amount"] = edQuantityBuy.Value;
        aCmd["Price"] = edPriceBuy.Value;
        XCore.AddCmd(aCmd);
      }      
    }
    private void btnSell_Click(object sender, EventArgs e) {
      decimal dTotal = edQuantitySell.Value * edPriceSell.Value;
      if (Balances.Contains(FocusedMarket.BaseCurrency) &&
        (dTotal > 0.0005m) &&
        (edQuantitySell.Value.toDouble() <= Balances[FocusedMarket.BaseCurrency].QuoteAvailable)) {
        CCmd aCmd = new CCmd("DoPostOrder");
        aCmd["MarketName"] = FocusedMarket.MarketName;
        aCmd["IsBuy"] = false;
        aCmd["Amount"] = edQuantitySell.Value;
        aCmd["Price"] = edPriceSell.Value;
        XCore.AddCmd(aCmd);
      }
    }
     

    #endregion

    #region Form Rendering
    delegate void UpdateControlVisibilityCallback();
    private void DoUpdateControlVisibility() {      
      if (this.edTotalBuy.InvokeRequired) {
        UpdateControlVisibilityCallback d = new UpdateControlVisibilityCallback(DoUpdateControlVisibility);
        this.Invoke(d, new object[] { });
      } else {
        if (this.Visible) {
          Form1_ResizeEnd(null, null);
          Int32 iLeft = 0, iTop = 0, iWidth = 0, iHeight = 0;          

          if ((iDisplayMode == 0)|| (iDisplayMode == 86)|| (iDisplayMode == 30)) { 
            if (label1.Visible) label1.Visible = false;
            if (label2.Visible) label2.Visible = false;
            if (label3.Visible) label3.Visible = false;
            if (textBox1.Visible) textBox1.Visible = false;
            if (textBox2.Visible) textBox2.Visible = false;
            if (textBox3.Visible) textBox3.Visible = false;
            if (btnContinue.Visible) btnContinue.Visible = false;
          }
          if (iDisplayMode == 10) {          
            if (!label1.Visible) label1.Visible = true;
            if (!label2.Visible) label2.Visible = true;
            if (!label3.Visible) label3.Visible = true;
            if (!textBox1.Visible) textBox1.Visible = true;
            if (!textBox2.Visible) textBox2.Visible = true;
            if (!textBox3.Visible) textBox3.Visible = true;
            if (!btnContinue.Visible) btnContinue.Visible = true;
          }
                   
          if (iDisplayMode == 20) {
            if (!label1.Visible) label1.Visible = true;
            if (label2.Visible) label2.Visible = false;
            if (label3.Visible) label3.Visible = false;
            if (!textBox1.Visible) textBox1.Visible = true;
            if (textBox2.Visible) textBox2.Visible = false;
            if (textBox3.Visible) textBox3.Visible = false;
            if (!btnContinue.Visible) btnContinue.Visible = true;
          }

          if (iDisplayMode == 30) {

            if (edQuantityBuy.Visible) edQuantityBuy.Visible = false;
            if (edQuantitySell.Visible) edQuantitySell.Visible = false;
            if (edPriceBuy.Visible) edPriceBuy.Visible = false;
            if (edPriceSell.Visible) edPriceSell.Visible = false;
            if (edTotalBuy.Visible) edTotalBuy.Visible = false;
            if (edTotalSell.Visible) edTotalSell.Visible = false;

            if (btnSell.Visible) btnSell.Visible = false;
            if (btnBuy.Visible) btnBuy.Visible = false;
                        
            if (btnReloadOpenOrders.Visible) btnReloadOpenOrders.Visible = false;
            if (btnReloadOrderHistory.Visible) btnReloadOrderHistory.Visible = false;

          }

          if (iDisplayMode == 40) {

            iLeft = Convert.ToInt32(3 * f05Width+ OffsetA.Width);
            iTop = Convert.ToInt32(f05Height);
            iWidth = Convert.ToInt32(f20Width + f05Width);
            iHeight = Convert.ToInt32(2 * f05Width);            

            Int32 iLeftSell = Convert.ToInt32(3 * f20Width + 2 * f05Width + OffsetA.Width);            

            edQuantityBuy.Top = iTop;
            edQuantitySell.Top = edQuantityBuy.Top;
            edPriceBuy.Top = edQuantityBuy.Top + edQuantityBuy.Height + 5;
            edPriceSell.Top = edPriceBuy.Top;
            edTotalBuy.Top = edPriceBuy.Top + edPriceBuy.Height + 5;
            edTotalSell.Top = edTotalBuy.Top;
            btnBuy.Top =  edTotalBuy.Top + (btnBuy.Height * 1.5).toInt32T();
            btnSell.Top = btnBuy.Top;

            edQuantityBuy.Left = iLeft;            
            edPriceBuy.Left = iLeft;
            edTotalBuy.Left = iLeft;
            btnBuy.Left = (3 * f05Width).toInt32T();
            edQuantitySell.Left = iLeftSell;
            edPriceSell.Left = iLeftSell;
            edTotalSell.Left = iLeftSell;            
            btnSell.Left = (3 * f20Width + 2 * f05Width).toInt32T();

            if (!edQuantityBuy.Visible) edQuantityBuy.Visible = true;
            if (!edQuantitySell.Visible) edQuantitySell.Visible = true;
            if (!edPriceBuy.Visible) edPriceBuy.Visible = true;
            if (!edPriceSell.Visible) edPriceSell.Visible = true;
            if (!edTotalBuy.Visible) edTotalBuy.Visible = true;
            if (!edTotalSell.Visible) edTotalSell.Visible = true;
            
            if (Balances.Contains(FocusedMarket.QuoteCurrency)) {  //
              //FocusedMarket.QuoteCurrency
              double dAmount = Balances[FocusedMarket.QuoteCurrency].QuoteAvailable;
              if ((dAmount > 0.00050000) && (edTotalBuy.Value.toDouble() > dAmount)) {
                edQuantityBuy.Value = (dAmount.toDecimal() - (dAmount.toDecimal() * 0.0025m)) / edPriceBuy.Value;
              }
              if (edTotalBuy.Value.toDouble() <= Balances[FocusedMarket.QuoteCurrency].QuoteAvailable) {
                if (!btnBuy.Visible) btnBuy.Visible = true;
              } else {
                if (btnBuy.Visible) btnBuy.Visible = false;
              }
            } else {
              if (btnBuy.Visible) btnBuy.Visible = false;
            }
            
            if (Balances.Contains(FocusedMarket.BaseCurrency)) {  //
              double dAmount = Balances[FocusedMarket.BaseCurrency].QuoteAvailable;
              if ((dAmount>0)&&(edQuantitySell.Value.toDouble() >= dAmount)) {
                edQuantitySell.Value =  dAmount.toDecimal();
              }
              if (edTotalSell.Value.toDouble() <= Balances[FocusedMarket.BaseCurrency].QuoteAvailable) {
                if (!btnSell.Visible) btnSell.Visible = true;
              } else {
                if (btnSell.Visible) btnSell.Visible = false;
              }
            } else {
              if (btnSell.Visible) btnSell.Visible = false;
            }

            Int32 iBtnLeft = Convert.ToInt32( (3 * f20Width) + (f05Width/2));
            btnReloadOrderHistory.Left = iBtnLeft;
            btnReloadOpenOrders.Left = iBtnLeft;
            btnReloadBalances.Left = 0;

            btnReloadOrderHistory.Top = Convert.ToInt32((fHeight/2) - (6 * sfText.Height));
            btnReloadOpenOrders.Top =  Convert.ToInt32((fHeight/2) - (4 * sfText.Height));            
            btnReloadBalances.Top = (fHeight - f05Height).toInt32() + 5;

            if (!btnReloadBalances.Visible) btnReloadBalances.Visible = true;
            if (!btnReloadOpenOrders.Visible) btnReloadOpenOrders.Visible = true;
            if (!btnReloadOrderHistory.Visible) btnReloadOrderHistory.Visible = true;

          }
        }  
      }
    }

    public void DoRefresh() {

      DoUpdateControlVisibility();
      DoChkFeeds();
      if (this.Visible) { 
        Graphics g = this.CreateGraphics();        
        try {        
          string es = "0";
          OffsetA = g.MeasureString("  Quantity:", fCur10);
          BufferedGraphics Canvas = BufferedGraphicsManager.Current.Allocate(g, this.DisplayRectangle);
          BufferedGraphics bg = Canvas;
          try {

            #region Drawing to Canvas 

            #region Basic Measurements
            Form1_ResizeEnd(null, null);
            Font fCur10 = new Font("Courier New", 10); Font fCur9 = new Font("Courier New", 9); 
            Font fCur8 = new Font("Courier New", 8); Font fCur6 = new Font("Courier New", 6);

            Int32 iLeft = 0, iTop = 0, iWidth = 0, iHeight = 0, iSellNo = 0;

            #endregion

            #region Draw Outlines
            /*
            Point pA = new Point(0, f05Height.toInt32());
            Point pB = new Point((4 * f20Width).toInt32(), f05Height.toInt32());

            Canvas.Graphics.DrawLine(Pens.AntiqueWhite, pA, pB);
            pA.Y = (pA.Y + f15Height).toInt32(); pB.Y = pA.Y;
            Canvas.Graphics.DrawLine(Pens.AntiqueWhite, pA, pB);
            pA.Y = (pA.Y + f15Height).toInt32(); pB.Y = pA.Y;
            Canvas.Graphics.DrawLine(Pens.AntiqueWhite, pA, pB);
            pA.Y = (pA.Y + f15Height).toInt32(); pB.Y = pA.Y;
            Canvas.Graphics.DrawLine(Pens.AntiqueWhite, pA, pB);
            pA.Y = (pA.Y + f15Height).toInt32(); pB.Y = pA.Y;
            Canvas.Graphics.DrawLine(Pens.AntiqueWhite, pA, pB);
    */
            /*   pA.Y = (pA.Y +  f05Height).toInt32(); pB.Y = pA.Y;
            Canvas.Graphics.DrawLine(Pens.AntiqueWhite, pA, pB);

            pA.Y = (pA.Y + f15Height).toInt32(); pB.Y = pA.Y;
            Canvas.Graphics.DrawLine(Pens.AntiqueWhite, pA, pB);
            pA.Y = (pA.Y + f15Height).toInt32(); pB.Y = pA.Y;
            Canvas.Graphics.DrawLine(Pens.AntiqueWhite, pA, pB);  */

            // vertical
            /*     pA.X = 0; pA.Y = f05Height.toInt32(); pB.X = 0; pB.Y = (fHeight * 0.65).toInt32() - 3;
            Canvas.Graphics.DrawLine(Pens.AntiqueWhite, pA, pB);
            pA.X = pA.X + f20Width.toInt32(); pB.X = pA.X;
            Canvas.Graphics.DrawLine(Pens.AntiqueWhite, pA, pB);
            pA.X = pA.X + f20Width.toInt32(); pB.X = pA.X;
            Canvas.Graphics.DrawLine(Pens.AntiqueWhite, pA, pB);
            pA.X = pA.X + f20Width.toInt32(); pB.X = pA.X;
            Canvas.Graphics.DrawLine(Pens.AntiqueWhite, pA, pB);
            pA.X = pA.X + f20Width.toInt32(); pB.X = pA.X;
            Canvas.Graphics.DrawLine(Pens.AntiqueWhite, pA, pB);  */
            //        pA.X = fWidth.toInt32()-1; pB.X = pA.X;
            //        Canvas.Graphics.DrawLine(Pens.AntiqueWhite, pA, pB);

            /*
            // break for balance row.
            pA.X = 0; pA.Y=2*f05Height.toInt32()+4*f15Height.toInt32(); pB.X=0; pB.Y= fHeight.toInt32()-1;
            Canvas.Graphics.DrawLine(Pens.AntiqueWhite, pA, pB);
            pA.X = pA.X + f20Width.toInt32(); pB.X=pA.X;
            Canvas.Graphics.DrawLine(Pens.AntiqueWhite, pA, pB);
            pA.X = pA.X + f20Width.toInt32(); pB.X = pA.X;
            Canvas.Graphics.DrawLine(Pens.AntiqueWhite, pA, pB);
            pA.X = pA.X + f20Width.toInt32(); pB.X = pA.X;
            Canvas.Graphics.DrawLine(Pens.AntiqueWhite, pA, pB);
            pA.X = pA.X + f20Width.toInt32(); pB.X = pA.X;
            Canvas.Graphics.DrawLine(Pens.AntiqueWhite, pA, pB);
            pA.X = fWidth.toInt32()-1; pB.X = pA.X;
            Canvas.Graphics.DrawLine(Pens.AntiqueWhite, pA, pB);  */
            #endregion

            #region top menu row
            string sTopLeft = XCore.daTime.Minute.ToString().PadLeft(2, '0') + ":" 
              + XCore.daTime.Second.ToString().PadLeft(2, '0') 
              + " n:" + XCore.Nonce.toString() 
              + " fw: "+ fWidth.toString();
            bg.Graphics.DrawString(sTopLeft, fCur10, Brushes.Chartreuse, new PointF(Convert.ToSingle(3), Convert.ToSingle(0)));

            //top right
            String sPoloQue = " Uptime: " + XCore.sUpTime + "; Tasks " + XCore.TheOp.Count.ToString()+" "+(bTickersUp?" TA":"NT");
            bg.Graphics.DrawString(sPoloQue, fCur10, Brushes.Chartreuse, new PointF(Convert.ToSingle(fWidth - bg.Graphics.MeasureString(sPoloQue, fCur10).Width), Convert.ToSingle(0)));

            //top middle.
            //string sLC = "";
            //string sRC = "";
            //  if (Balances.Keys.Contains("BTC")) {
            //sLC = TotalBTCValue.toStr8() + " / ";
            //sRC = " " + Convert.ToDouble(TotalBTCValue / ).toStr2() + " chuncks";
            //  }
            //SizeF aSF = bg.Graphics.MeasureString("RANK", fCur8);        
            //bg.Graphics.DrawString(sLC, fCur10, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(bg.Graphics.MeasureString(sLC, fCur10).Width - 10), Convert.ToSingle(2)));
            //bg.Graphics.DrawString(sRC, fCur10, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(), Convert.ToSingle(2)));

            #endregion

            if (iDisplayMode == 0) {          
              RenderWorkingProgress(bg);
            }

            if ((iDisplayMode == 30)&&(bHasMarkets)) {   // pick the market. 

              es = "301";
              KeyValuePair<string, object>[] xList = Markets.ToArray();          
              Int32 iRowN = 0;
              for (Int32 iDR = 0; iDR <= 7; iDR++) {
                for (Int32 iDMC = 0; iDMC <= 7; iDMC++) {
                  #region Row N
                  es = "303";
                  iRowN = (iDR * 8) + iDMC;
                  iLeft = (iDMC * (fWidth/8).toDouble()).toInt32T();
                  if (iRowN < xList.Length) {
                    es = "304";
                    KeyValuePair<string, object> iro = xList.ElementAt(iRowN);
                    es = "305";
                    string acp = iro.Key;
                    es = "306";
                    CMarket aMarket = (CMarket)iro.Value;
                    es = "307";
                    string sToProfit = "";
                    string sAbortAt = "";
                    string sMarket = "";
                
                    es = "310";
                    sMarket = aMarket.MarketName //aMarket.BaseCurrency + " " + aMarket.QuoteCurrency 
                      + " " + aMarket.PriceLast.toStr8() + aMarket.QuoteCurrency;
                    es = "311";
                    sToProfit = Convert.ToDouble(aMarket.PriceLast.toDouble() * 0.005).toStr8() + " to " + Convert.ToDouble(aMarket.PriceLast.toDouble() * 1.0050).toStr8();
                    sAbortAt = "" + Convert.ToDouble(aMarket.Bid.toDouble() ).toStr8() + "    " + aMarket.Ask.toDouble().toStr8();
                
                    es = "312";
                    SizeF sMarSize = bg.Graphics.MeasureString(sMarket, fCur10);
                    SizeF sLastPSize = bg.Graphics.MeasureString(sToProfit, fCur9);
                    SizeF sAbortPSize = bg.Graphics.MeasureString(sAbortAt, fCur9);
                    Brush theColor = Brushes.DarkBlue;
                    es = "313";
                    if (aMarket.Selected) {
                      es = "3131";
                      bg.Graphics.FillRectangle(theColor, 
                        new Rectangle(Convert.ToInt32(iLeft + 1), 
                          Convert.ToInt32(f05Height + (iDR * f15Height / 2.5) + 1), 
                          Convert.ToInt32(fWidth / 8), 
                          Convert.ToInt32(f15Height / 2.5)));
                      es = "3132";
                      bg.Graphics.DrawRectangle(Pens.Chartreuse, new Rectangle(Convert.ToInt32(iLeft + 1), Convert.ToInt32(f05Height + (iDR * f15Height / 2.5) + 1), Convert.ToInt32(fWidth / 8), Convert.ToInt32(f15Height / 2.5)));
                    } else {
                      es = "3133";
                      bg.Graphics.DrawRectangle(Pens.Chartreuse, new Rectangle(Convert.ToInt32(iLeft + 1), Convert.ToInt32(f05Height + (iDR * f15Height / 2.5) + 1), Convert.ToInt32(fWidth / 8), Convert.ToInt32(f15Height / 2.5)));
                    }
                    es = "3134";
                    if (aMarket.Sparked) {
                      es = "3135";
                      aMarket.Sparked = false;
                      es = "3136";
                      theColor = Brushes.DarkGreen;
                      es = "3137";
                      bg.Graphics.FillRectangle(theColor, new Rectangle(Convert.ToInt32(iLeft + 1), Convert.ToInt32(f05Height + (iDR * f15Height / 2.5) + 1), Convert.ToInt32(fWidth / 8), Convert.ToInt32(f15Height / 2.5)));
                    }

                    es = "314";
                    bg.Graphics.DrawString(sToProfit, fCur9, Brushes.Chartreuse, new PointF(Convert.ToSingle(iLeft + fWidth / 16 - sLastPSize.Width / 2), Convert.ToSingle(f05Height + (iDR * f15Height / 2.5) + sLastPSize.Height / 2)));
                    bg.Graphics.DrawString(sMarket, fCur10, Brushes.Chartreuse, new PointF(Convert.ToSingle(iLeft + fWidth / 16 - sMarSize.Width / 2), Convert.ToSingle(f05Height + (iDR * f15Height / 2.5) + 3 * sMarSize.Height / 2)));
                    bg.Graphics.DrawString(sAbortAt, fCur9, Brushes.Chartreuse, new PointF(Convert.ToSingle(iLeft + fWidth / 16 - sAbortPSize.Width / 2), Convert.ToSingle(f05Height + (iDR * f15Height / 2.5) + 5 * sAbortPSize.Height / 2)));
                  }
                  #endregion
                }
              }
            }

            if ((iDisplayMode == 40)&&(bHasMarkets)&&(FocusedMarket is CMarket)) {

              #region Control Captions

              iLeft = Convert.ToInt32(3 * f05Width);
              iTop = Convert.ToInt32(f05Height);
              iWidth = Convert.ToInt32(f20Width + f05Width);
              iHeight = Convert.ToInt32(2 * f05Width);
              OffsetA = bg.Graphics.MeasureString("  Quantity:", fCur10);
              Int32 iLeftSell = Convert.ToInt32(3 * f20Width + 2 * f05Width);

              bg.Graphics.DrawString("  Quantity:", fCur10, Brushes.WhiteSmoke, new PointF(iLeft, iTop));
              bg.Graphics.DrawString(" Buy Price:", fCur10, Brushes.WhiteSmoke, new PointF(iLeft, iTop+27));
              bg.Graphics.DrawString(" Buy Total:", fCur10, Brushes.WhiteSmoke, new PointF(iLeft, iTop+54));
              if (Balances.Contains(FocusedMarket.QuoteCurrency)) {
                bg.Graphics.DrawString(" "+ Balances[FocusedMarket.QuoteCurrency].QuoteAvailable.toStr8()+" "+ 
                  FocusedMarket.QuoteCurrency, fCur10, Brushes.WhiteSmoke, new PointF(iLeft+ OffsetA.Width + 132, iTop + 54));                
              } else {
                bg.Graphics.DrawString(" 0.0 "+ FocusedMarket.QuoteCurrency, fCur10,
                  Brushes.WhiteSmoke, new PointF(iLeft + OffsetA.Width + 132, iTop + 54));
              }

              bg.Graphics.DrawString("  Quantity:", fCur10, Brushes.WhiteSmoke, new PointF(iLeftSell, iTop));
              bg.Graphics.DrawString("Sell Price:", fCur10, Brushes.WhiteSmoke, new PointF(iLeftSell, iTop + 27));
              bg.Graphics.DrawString("Sell Total:", fCur10, Brushes.WhiteSmoke, new PointF(iLeftSell, iTop + 54));
              if (Balances.Contains(FocusedMarket.BaseCurrency)) {
                bg.Graphics.DrawString(" "+ Balances[FocusedMarket.BaseCurrency].QuoteAvailable.toStr8()+" "+ 
                  FocusedMarket.BaseCurrency, fCur10, Brushes.WhiteSmoke, new PointF(iLeftSell+OffsetA.Width + 132, iTop));
              } else {
                bg.Graphics.DrawString(" 0.0 "+ FocusedMarket.BaseCurrency, fCur10,
                  Brushes.WhiteSmoke, new PointF(iLeftSell + OffsetA.Width + 132, iTop));
              }

              #endregion 

              #region Draw Bid Buy Order Book

              iLeft = Convert.ToInt32(3 * f05Width);
              iTop = Convert.ToInt32(f05Height*5);
              iWidth = Convert.ToInt32(f20Width + f05Width);
              iHeight = Convert.ToInt32(f20Height * 4 + f05Height * 2);

              string sBidStats = "Bids : "+ FocusedMarket.BidsTotal.toDouble().toStr2() +" "+FocusedMarket.BidsChanged.toStr2()+" "+ FocusedMarket.BidsAvgChange.toAvg().toStr4();
              bg.Graphics.DrawString( sBidStats, fCur10, Brushes.WhiteSmoke, Convert.ToSingle(f05Width * 3), Convert.ToSingle(iTop - (sfText.Height)));

              double dSumBase = 0;
              iSellNo = 1; es = "1";
              try {
                if (FocusedMarket.Bids != null) {
                  es = "11";
                  Int32 iCount = FocusedMarket.Bids.Count();
                  es = "12";
                  KeyValuePair<decimal, ExchangeOrderPrice>[] lp = new KeyValuePair<decimal, ExchangeOrderPrice>[1000];
                  es = "13";
                  FocusedMarket.Bids.CopyTo(lp, 0);
                  es = "2";
                  foreach (KeyValuePair<decimal, ExchangeOrderPrice> kvp in lp) {
                    es = "3";
                    ExchangeOrderPrice oX = kvp.Value;
                    es = "4";                    
                    Boolean IsMyOrder = false;
                    foreach (string sKey in FocusedMarket.OpenOrders.Keys) {
                      if (FocusedMarket.OpenOrders[sKey].Price.toSat() == oX.Price.toSat()) {
                        IsMyOrder = true;
                        break;
                      }
                    }                    
                    dSumBase += (oX.Amount * oX.Price).toDouble();
                    if (IsMyOrder) {
                      es = "5";
                      bg.Graphics.DrawString(
                        oX.Price.toSat() + " " + oX.Amount.toDouble().toStr4P(10) + " " + (oX.Amount * oX.Price).toDouble().toStr8P(11) + " " + dSumBase.toStr4(),
                          fCur9, Brushes.Chartreuse, Convert.ToSingle(f05Width * 3), Convert.ToSingle(iTop + (iSellNo * sfText.Height)));
                    } else {
                      es = "6";                      
                      bg.Graphics.DrawString(
                        oX.Price.toSat() + " " + oX.Amount.toDouble().toStr4P(10) + " " + (oX.Amount * oX.Price).toDouble().toStr8P(11) + " " + dSumBase.toStr4(),
                          fCur9, Brushes.WhiteSmoke, Convert.ToSingle(f05Width * 3), Convert.ToSingle(iTop + (iSellNo * sfText.Height)));
                    }
                    es = "7";
                    iSellNo++;
                    if (iTop + (2 * sfText.Height) + (iSellNo * sfText.Height) > (iTop + iHeight - 20)) { break; }
                    if (iSellNo >= iCount) break;
                  }
                }
              } catch (Exception e) {
                e.toAppLog("Render Bid Book " + es);
              }
              #endregion

              #region User Shard Listings
              iLeft = Convert.ToInt32(5);
              iTop = Convert.ToInt32(f05Height);
              iWidth = Convert.ToInt32(3 * f05Width);
              iHeight = Convert.ToInt32(f20Height * 4 + f05Height * 2);
              iSellNo = 1;
              double dTotalQ = 0, dTotalB = 0, dShardB =0, dAvgPrice=0;
              if (FocusedMarket.QuoteShards.Count > 0) {
                IEnumerable<KeyValuePair<decimal, object>> lQS = FocusedMarket.QuoteShards.OrderByDescending(x => ((CTradeHistoryManager.Shard)x.Value).Price);                
                foreach(KeyValuePair<decimal, object> kvp in lQS) {
                  CTradeHistoryManager.Shard ls = (CTradeHistoryManager.Shard) kvp.Value;
                  dTotalQ = dTotalQ + ls.Quantity.toDouble();
                  dShardB = (ls.Price.toDouble() * ls.Quantity.toDouble());
                  dTotalB = dTotalB + dShardB;
                  string sOut = ""+ ls.Price.toSat()+ " "+ ls.Quantity.toStr4P(11)+" "+dShardB.toStr8();
                  bg.Graphics.DrawString(sOut, 
                    fCur8, Brushes.WhiteSmoke, Convert.ToSingle(iLeft),  Convert.ToSingle(iTop + 2 * sfText.Height + (iSellNo * sfText.Height)));
                  iSellNo++;
                }
                dAvgPrice = (dTotalQ!=0?dTotalB/dTotalQ:0);
              }
              bg.Graphics.DrawString(FocusedMarket.BaseCurrency + " "+dTotalQ.toStr4()+" Avg "+dAvgPrice.toSat(), 
                fCur8, Brushes.White, Convert.ToSingle(iLeft), Convert.ToSingle(iTop));
              bg.Graphics.DrawString(""+(FocusedMarket.PriceLast * 100000000).toInt32T().toString() +" sat .5% = "+ (FocusedMarket.PriceLast.toDouble() * 100000000 * 0.005).toString()+" sat", 
                fCur8, Brushes.White, Convert.ToSingle(iLeft), Convert.ToSingle(iTop+sfText.Height));

              #endregion

              #region Market Trade History

              iLeft = Convert.ToInt32(2 * f20Width - (f05Width / 2));          
              iTop = Convert.ToInt32(fHeight / 2) ;
              iWidth = Convert.ToInt32(f20Width + 2 * f05Width);
              iHeight = Convert.ToInt32(fHeight / 2 - f05Height-2);

              bg.Graphics.FillRectangle(Brushes.MidnightBlue, 
                new Rectangle(iLeft, 0, iWidth, (fHeight - f05Height).toInt32T()));
          

              String slTime = ""; String slPrice = ""; String slType = ""; String sCType = "";
              double dPrice = 0, dSumQVol = 0, dSumBVol = 0;
              Boolean bFTT = true;

              iSellNo = 6;    
              bg.Graphics.DrawString(FocusedMarket.MarketName+" Market History ", fCur10, Brushes.White, Convert.ToSingle(iLeft), Convert.ToSingle(iTop + sfText.Height + (iSellNo * sfText.Height)));   
              iSellNo = 7;
              Brush zBrush;

              try {
                //List<ExchangeTrade> xMOI = MarketHistory;
                if(FocusedMarket.MarketTrades.Count > 0) {               
                  es = "03";              
                  Dictionary<Int64, object> xMOI = FocusedMarket.MarketTrades.OrderBy(x => x.Key).ToDictionary(k => k.Key, v => v.Value );              
                  bFTT = true;
                  foreach (Int64 sKey in xMOI.Keys) {
                    es = "4";
                    ExchangeTrade xT = (ExchangeTrade)xMOI[sKey];
                    if (xT is ExchangeTrade) {
                      es = "4.5";
                      if (xT.IsBuy) {
                        sCType = "Buy ";
                      } else {
                        sCType = "Sell";
                      }

                      if (bFTT) {
                        es = "5";
                        dSumQVol = xT.Amount.toDouble();
                        dSumBVol = (xT.Amount * xT.Price).toDouble();
                        dPrice = xT.Price.toDouble();
                        slTime = xT.Timestamp.toStrTime(); es = "15";
                        slPrice = xT.Price.toStr8();
                        slType = sCType;
                        bFTT = false;
                      } else {
                        es = "6";
                        if ((slTime == xT.Timestamp.toStrTime()) && (slPrice == xT.Price.toStr8()) && (slType == sCType)) {
                          es = "7";
                          dSumQVol += xT.Amount.toDouble();
                          dSumBVol += (xT.Amount * xT.Price).toDouble();
                        } else {

                          es = "8";
                          if (sCType != "Buy ") { zBrush = Brushes.Chartreuse; } else { zBrush = Brushes.HotPink; }
                          es = "9";                      
                          string sMarket = FocusedMarket.MarketName;
                          string s = " " + xT.Timestamp.toStrTime() +" "+ sCType +
                            " " + xT.Price.toSat() + " " + sMarket.ParseString("-_", 0) +  // Vol quote actual                 
                            " " + dSumQVol.toStr8P(15) + " " + sMarket.ParseString("-_", 1) +  // price actual                    
                            " " + dSumBVol.toStr8P(11) + " " + sMarket.ParseString("-_", 0);
                          es = "10"; // total
                          bg.Graphics.DrawString(s, fCur8, zBrush, Convert.ToSingle(iLeft),
                            Convert.ToSingle(iTop + 2 * sfText.Height + (iSellNo * sfText.Height)));

                          dSumQVol = xT.Amount.toDouble();
                          dSumBVol = (xT.Amount * xT.Price).toDouble(); es = "11";
                          slTime = xT.Timestamp.toStrTime(); es = "12";
                          slPrice = xT.Price.toStr8();

                          es = "31";                  
                          iSellNo++;
                        }
                      }
                      if ((iTop + 2 * sfText.Height + (iSellNo * sfText.Height)) > (iTop + iHeight - 20)) { break; }  // manage height
                      es = "12";
                    }
                  }
                }
              } catch (Exception ee) {
                ee.toAppLog("RenderTradeHistory " + es);
              }

              #endregion

              #region User Trade History.  starts from middle - 5 and goes up.
              try { 
                bFTT = true;
                iSellNo = 6;
                //      string sOut = "  time  vol "+FocusedMarket.BaseCurrency+"  ";
                bg.Graphics.DrawString(FocusedMarket.MarketName + " User Trade History ", fCur10, Brushes.White, 
                  Convert.ToSingle(iLeft), Convert.ToSingle(iTop  - (iSellNo * sfText.Height)));
                iSellNo = 7;
                if (FocusedMarket.TradeHisory.Count > 0) {
                  IEnumerable<KeyValuePair<string, object>> lEOR = FocusedMarket.TradeHisory.OrderByDescending(x => ((ExchangeOrderResult)x.Value).OrderDate);
                  foreach(KeyValuePair<string, object> kvp in lEOR) {
                    ExchangeOrderResult xT = (ExchangeOrderResult)((KeyValuePair<string, object>)kvp).Value;
                    if (xT is ExchangeOrderResult) {
                      es = "14.5";
                      if (xT.IsBuy) {
                        sCType = "Buy ";
                      } else {
                        sCType = "Sell";
                      }

                  
                      es = "15";
                      dSumQVol = xT.AmountFilled.toDouble();
                      dSumBVol = (xT.AmountFilled * xT.Price).toDouble();
                      dPrice = xT.Price.toDouble();
                      slTime = xT.OrderDate.toStrTime(); es = "15";
                      slPrice = xT.Price.toStr8();
                      slType = sCType;                
                                   
                      es = "16";
                      if (sCType != "Buy ") { zBrush = Brushes.Chartreuse; } else { zBrush = Brushes.HotPink; }

                      es = "17";
                      string sMarket = FocusedMarket.MarketName;
                      string s = " " + sCType+
                        " " + xT.Price.toSat() + " " + sMarket.ParseString("-_", 0) +  // Vol quote actual                 
                        " " + dSumQVol.toStr8P(15) + " " + sMarket.ParseString("-_", 1) +  // price actual                    
                        " " + dSumBVol.toStr8P(11) + " " + sMarket.ParseString("-_", 0);
                      es = "18"; // total
                      bg.Graphics.DrawString(s, fCur8, zBrush, Convert.ToSingle(iLeft),
                        Convert.ToSingle(iTop - (iSellNo * sfText.Height)));
                  
                      es = "31";
                      iSellNo++;
                    
                  
                      if ((iTop  - (iSellNo * sfText.Height)) < (f05Height)) { break; }  // manage height
                      es = "12";
                    }

                  }
                }

              } catch (Exception eee) {
                eee.toAppLog("RedrawMyTradeHistory");
              }

              #endregion

              #region User Open Orders.     in middle going down. 

              iLeft = Convert.ToInt32(2 * f20Width - (f05Width / 2));
              iTop = Convert.ToInt32(fHeight / 2);
              iWidth = Convert.ToInt32(f20Width + 2 * f05Width);
              iHeight = Convert.ToInt32(fHeight / 2 - f05Height - 2);
          
              dPrice = 0; dSumQVol = 0; dSumBVol = 0;
              bFTT = true;

              iSellNo = -4;
              //      string sOut = "  time  vol "+FocusedMarket.BaseCurrency+"  ";
              bg.Graphics.DrawString(FocusedMarket.MarketName + " Open Orders ", fCur10, Brushes.White, 
                Convert.ToSingle(iLeft), Convert.ToSingle(iTop + (iSellNo * sfText.Height)));
              //       bg.Graphics.DrawString(sOut, fCur8, Brushes.White, Convert.ToSingle(iLeft), Convert.ToSingle(iTop + sfText.Height + ((iSellNo + 1) * sfText.Height)));
              iSellNo = -3;         

              try {
                //List<ExchangeTrade> xMOI = MarketHistory;
                if (FocusedMarket.OpenOrders.Count > 0) {
                  IEnumerable<KeyValuePair<string, object>> lEOR = FocusedMarket.OpenOrders.OrderByDescending(x => ((ExchangeOrderResult)x.Value).OrderDate);
                  foreach (KeyValuePair<string, object> kvp in lEOR) {
                    ExchangeOrderResult xT = (ExchangeOrderResult)((KeyValuePair<string, object>)kvp).Value;
                    if (xT is ExchangeOrderResult) {
                      es = "14.5";
                      if (xT.IsBuy) {
                        sCType = "Buy ";
                      } else {
                        sCType = "Sell";
                      }


                      es = "15";
                      dSumQVol = xT.Amount.toDouble();
                      dSumBVol = (xT.Amount * xT.Price).toDouble();
                      dPrice = xT.Price.toDouble();
                      slTime = xT.OrderDate.toStrTime(); es = "15";
                      slPrice = xT.Price.toStr8();
                      slType = sCType;

                      es = "16";
                      if (sCType != "Buy ") { zBrush = Brushes.Chartreuse; } else { zBrush = Brushes.HotPink; }

                      es = "17";
                      string sMarket = FocusedMarket.MarketName;
                      string s = " " + xT.OrderDate.toStrTime() + sCType +
                        " " + xT.Price.toSat() + " " + sMarket.ParseString("-_", 0) +  // Vol quote actual                 
                        " " + dSumQVol.toStr8P(15) + " " + sMarket.ParseString("-_", 1) +  // price actual                    
                        " " + dSumBVol.toStr8P(11) + " " + sMarket.ParseString("-_", 0);
                      es = "18"; // total
                      bg.Graphics.DrawString(s, fCur8, zBrush, Convert.ToSingle(iLeft),
                        Convert.ToSingle(iTop  + (iSellNo * sfText.Height)+ (sfText.Height/2)));

                      es = "31";
                      iSellNo++;


                      if (iSellNo  > 4 ) { break; }  // manage height
                      es = "12";
                    }

                  }


                }
              } catch (Exception ee) {
                ee.toAppLog("RenderTradeHistory " + es);
              }

              #endregion

              #region Draw Ask Sell Order Book

              //Boolean SellLineDrawn = false;
              //Boolean SellLine2Drawn = false;
              iTop = Convert.ToInt32(f05Height * 5);
              dSumBase = 0; iSellNo = 1; es = "1";
              try {
                if (FocusedMarket.Asks != null) {
                  KeyValuePair<decimal, ExchangeOrderPrice>[] lp = new KeyValuePair<decimal, ExchangeOrderPrice>[1000];
                  FocusedMarket.Asks.CopyTo(lp, 0);
                  Int32 iCount = FocusedMarket.Asks.Count();

                  string sAskStats = "Asks : " + FocusedMarket.AsksTotal.toDouble().toStr2() + " " + FocusedMarket.AsksChanged.toStr2()+ " " + FocusedMarket.AskAvgChange.toAvg().toStr4();
                  bg.Graphics.DrawString(sAskStats, fCur10, Brushes.WhiteSmoke, 
                    Convert.ToSingle((3 * f20Width + 2 * f05Width)),
                    Convert.ToSingle(iTop - (sfText.Height)));


                  es = "2";
                  foreach (KeyValuePair<decimal, ExchangeOrderPrice> kvp in lp) {
                    es = "3";
                    ExchangeOrderPrice oX = kvp.Value;
                    es = "4";
                    
                    Boolean IsMyOrder = false;
                    foreach(string sKey in FocusedMarket.OpenOrders.Keys) { 
                      if( FocusedMarket.OpenOrders[sKey].Price.toSat() == oX.Price.toSat()) {
                        IsMyOrder = true;
                        break;
                      }
                    }
                    
                    dSumBase += (oX.Amount * oX.Price).toDouble();
                    if (IsMyOrder) { es = "5";
                      bg.Graphics.DrawString(oX.Price.toSat() + " " + oX.Amount.toDouble().toStr4P(10) + " " + (oX.Amount * oX.Price).toDouble().toStr8P(11) + " " + dSumBase.toStr4(),
                        fCur9, Brushes.Chartreuse,
                        Convert.ToSingle((3 * f20Width + 2 * f05Width)),
                        Convert.ToSingle(iTop + (iSellNo * sfText.Height)));
                    } else {         es = "6";                      
                      bg.Graphics.DrawString(oX.Price.toSat() + " " + oX.Amount.toDouble().toStr4P(10) + " " + (oX.Amount * oX.Price).toDouble().toStr8P(11) + " " + dSumBase.toStr4(),
                        fCur9, Brushes.WhiteSmoke, 
                        Convert.ToSingle((3 * f20Width + 2 * f05Width)), 
                        Convert.ToSingle(iTop + (iSellNo * sfText.Height)));
                    }                es = "7";
                    iSellNo++;
                    if (iTop + (2 * sfText.Height) + (iSellNo * sfText.Height) > (fHeight - f05Height)) { break; }
                    if (iSellNo >= iCount) break;
                  }

                }
              } catch (Exception e) {
                e.toAppLog("Render Ask Book " + es);
              }
              #endregion

            }


            #region Row Balances
        
        
            double dLastL = f05Width*3;
            double dBalDispWidth = f20Width * 4;
            if (Balances.Count > 0) {
              foreach (string b in Balances.Keys) {
                double w = dBalDispWidth * Balances[b].PercentOfTotal;  //        was f20Height + 3 * f15Height
                bg.Graphics.FillRectangle(Brushes.DarkGreen, new Rectangle(Convert.ToInt32(dLastL), Convert.ToInt32(fHeight - f05Height-1), Convert.ToInt32(w), Convert.ToInt32(f05Height)));
                bg.Graphics.DrawRectangle(Pens.White, new Rectangle(Convert.ToInt32(dLastL), Convert.ToInt32(fHeight - f05Height -2 ), Convert.ToInt32(w), Convert.ToInt32(f05Height - 1)));
                dLastL = dLastL + w;
              }
              dLastL = f05Width * 3;
              foreach (string b in Balances.Keys) {
                string sQ = b + " " + Balances[b].DBQuote.toStr4();
                SizeF szQ = bg.Graphics.MeasureString(sQ, fCur6);
                double w = dBalDispWidth * Balances[b].PercentOfTotal;
                double lLeft = dLastL + w / 2 - szQ.Width / 2;
                if (lLeft < 0) { lLeft = 0; }
                bg.Graphics.DrawString(sQ, fCur10, Brushes.White, 
                  new PointF(Convert.ToSingle(lLeft), Convert.ToSingle(fHeight - f05Height + (f05Height / 2) - szQ.Height / 2 * 3)));
                bg.Graphics.DrawString(Balances[b].BitcoinValue.toStr8(), fCur10, Brushes.White, 
                  new PointF(Convert.ToSingle(lLeft), Convert.ToSingle(fHeight - f05Height + (f05Height / 2) - szQ.Height / 2)));
                dLastL = dLastL + w;
              }
              string sT = "" + Balances.TotalBTCValue.toStr8() + "BTC " + Balances.TotalUSDValue.toStr2() + " USDC";
              SizeF szT = bg.Graphics.MeasureString(sT, fCur10);
              bg.Graphics.DrawString(sT, fCur10, Brushes.White, new PointF(Convert.ToSingle(dBalDispWidth - szT.Width), Convert.ToSingle(fHeight - f05Height + (f05Height / 2) + szT.Height / 4)));

            }
        
            #endregion

            #endregion

            Canvas.Render(g);
          } catch (Exception e) {
            e.toAppLog("Refresh "+es);
          } finally {
            Canvas.Dispose();
          }

        } finally {
          g.Dispose();
        }
      }
    }

    private void RenderWorkingProgress(BufferedGraphics bg) {
      try {
        if ((bInWait) || (bInStartup)) {
          float fWidth = bg.Graphics.VisibleClipBounds.Width;
          float fHeight = bg.Graphics.VisibleClipBounds.Height;
          double f20Height = fHeight * 0.2;
          double f15Height = fHeight * 0.145;
          double f20Width = fWidth * 0.2;
          double f15Width = fWidth * 0.15;
          SizeF sLocSize = bg.Graphics.MeasureString("SolutionStudio.Net presents...", fLogo17);

          if (bInStartup) {
            Int32 iLeft = Convert.ToInt32(f20Width + f20Width / 2);
            Int32 iTop = Convert.ToInt32(f20Height);
            Int32 iWidth = Convert.ToInt32(f20Width * 2);
            Int32 iHeight = Convert.ToInt32(f15Height * 2);

            bg.Graphics.FillRectangle(Brushes.DarkSeaGreen, new Rectangle(iLeft, iTop, iWidth, iHeight));
            bg.Graphics.DrawString("Poloniex Trader ", fLogo20, Brushes.Chartreuse, new PointF(Convert.ToSingle(iLeft + iWidth / 2 - sLocSize.Width / 2), Convert.ToSingle(iTop + iHeight / 2 - 2 * sLocSize.Height)));
            bg.Graphics.DrawString(sWaitDesc, fCur10, Brushes.White, new PointF(Convert.ToSingle(iLeft + iWidth / 2 - sLocSize.Width / 2), Convert.ToSingle(iTop + iHeight / 2 + 0 * sLocSize.Height)));

          } else {

            Int32 iLeft = Convert.ToInt32(4 * f20Width - f20Width / 2);
            Int32 iTop = Convert.ToInt32(f20Height - f20Height / 2);
            Int32 iWidth = Convert.ToInt32(f20Width * 1.25);
            Int32 iHeight = Convert.ToInt32(f15Height * 0.69);
            bg.Graphics.FillRectangle(Brushes.DarkSeaGreen, new Rectangle(iLeft, iTop, iWidth, iHeight));
            bg.Graphics.DrawString("Processing Please Wait ", fCur10, Brushes.White, new PointF(Convert.ToSingle(iLeft + iWidth / 2 - sLocSize.Width / 4), Convert.ToSingle(iTop + iHeight / 2 - sLocSize.Height / 4)));
            bg.Graphics.DrawString(sWaitDesc, fCur9, Brushes.Chartreuse, new PointF(Convert.ToSingle(iLeft + iWidth / 2 - sLocSize.Width / 4), Convert.ToSingle(iTop + iHeight / 2 + sLocSize.Height / 4)));

          }

        }
      } catch (Exception e) {
        e.toAppLog("RenderWorking");               
      }
    }

    #endregion
  }
}
