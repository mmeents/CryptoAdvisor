using System;
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
  public partial class Form1 : Form {

    #region Initialization

    Boolean bInStartup = true;

    IDictionary<CurrencyPair, IMarketData> lastMarket = null;
    public IDictionary<CurrencyPair, appMarket> MarketsOfInterest = null;

    IDictionary<string, IBalance> lastBalance = null;
    public Dictionary<string, appBalance> Balances = null;
    double TotalBTCValue = 0;
    double TotalUSDValue = 0;
    double BTCRate = 0;

    Boolean feedLive = false;
    List<TickerChangedEventArgs> feedQueue = null;
    Int64 feedTickNo = 0;

    List<TrollMessage> TrollFeedQueue = null;
    List<TrollMessage> TrollMsg = null;
    List<String> lCurList = null;
    
    PoloniexClient pc;
    KeyPair kpPolo = null;
    DateTime lastSummaryCheck;
    
    Boolean bBotQueOn = true;
    List<appCmd> lBotQue;
    appCmd sBotCmdInProgress = null;

    Boolean bPoloQueOn = true;
    List<appCmd> lPoloQue;
    appCmd sPoloCmdInProgress = null;

    Font fCur10; Font fCur9; Font fCur8; Font fCur7; Font fCur6; Font fLogo17; Font fLogo20;

    Boolean DoGetMarketHistory = false;
    Boolean DoGetTradeHistory = false;

    SortedList<double, appMarket> BestBuys;
    SortedList<double, appMarket> BestSells;
    SortedList<string, appMarket> AlphaMarkets;

    Boolean MarketDetailVisible = false;
    CurrencyPair FocusedMarket = null;
    CurrencyPair LastFocusedMarket = null;

    IniVar vSettings = null;
    Int32 iDisplayMode = 0;
    public appRefreshWait RefreshWait = null;
    public Boolean bInWait { get { return (iInWaitDepth > 0); } set {
      Int32 iWasInWait = iInWaitDepth;
      if (value) { iInWaitDepth++; } else { iInWaitDepth--; }
      if (iInWaitDepth < 0) {iInWaitDepth = 0;}      
    } } 
    public Int32 iInWaitDepth = 0;
    public String sWaitDesc = "";

    public double MakerFee = 0.0015;
    public double TakerFee = 0.0025;

    public bool FocusedMarketChanging = false;
    

    #endregion 

    #region Main Form Initialization startup and teardown.
    public Form1() {
      InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e) {
      fLogo17 = new Font("Calisto MT", 17); fLogo20 = new Font("Calisto MT", 20, FontStyle.Bold);
      fCur10 = new Font("Courier New", 10); fCur9 = new Font("Courier New", 9); fCur8 = new Font("Courier New", 8);
      fCur7 = new Font("Courier New", 7); fCur6 = new Font("Courier New", 6);
      Color ColorDefBack = System.Drawing.ColorTranslator.FromHtml("#08180F");
      LoadInitialSettings();
      lPoloQue = new List<appCmd>();
      lBotQue = new List<appCmd>();
      lBotQue.Insert(0, new appCmd("BootBot", null));      
      feedQueue = new List<TickerChangedEventArgs>();
      MarketsOfInterest = new Dictionary<CurrencyPair, appMarket>();

      TrollFeedQueue = new List<TrollMessage>();
      TrollMsg = new List<TrollMessage>();
      lCurList = new List<string>();

      float fHeight = this.Height;
      double f05Height = fHeight * 0.065;
      double f15Height = fHeight * 0.145;

      edHoldAmount.Top = Convert.ToInt32(f05Height + 2 * f15Height);
      edSellThresh.Top = edHoldAmount.Top - edHoldAmount.Height;
      edBuyThresh.Top = (edHoldAmount.Top + edHoldAmount.Height) + (edHoldAmount.Top - (edSellThresh.Top + edSellThresh.Height));
      cbGoHold.Top = edSellThresh.Top - edHoldAmount.Height;
 
      edBuyThresh.Visible = false;
      edSellThresh.Visible = false;
      edHoldAmount.Visible = false;
      cbGoHold.Visible = false;

      BestBuys = new SortedList<double, appMarket>();
      BestSells = new SortedList<double, appMarket>();
      AlphaMarkets = new SortedList<string, appMarket>();
      Balances = new Dictionary<string,appBalance>();
      this.Text = "Advisor -- " + AppUtils.LogFileName("Advisor");
      MarketDetailVisible = false;
      SetActionCtrVisibility(null);
      vSettings = new IniVar("Settings");
      string s = vSettings["edMarkup"];
      if (s != "") {
        try {
          edMarkup.Value = Convert.ToDecimal(s);
        } catch {
          edMarkup.Value = Convert.ToDecimal(1.0069);
        }
      } else {
        edMarkup.Value = Convert.ToDecimal(1.0069);
      }
      s = vSettings["edChunkSize"];
      if (s != "") {
        try {
          edChunkSize.Value = Convert.ToDecimal(s);
        } catch { 
        }
      }
      s = vSettings["edWhaleDepth"];
      if (s != "") {
        try {
          edWhaleDepth.Value = Convert.ToDecimal(s);
        } catch { 
        }
      }
      s = vSettings["OnlyBTC"];
      if (s == "" || s == "True") {
        cbOnlyBTC.Checked = true;
      } else {
        cbOnlyBTC.Checked = false;
      }
      s = vSettings["MakerFee"];
      if (s != "") {
        try {
          MakerFee = Convert.ToDouble(s);
        } catch {
        }
      } else { 
        vSettings["MakerFee"] = "0.0015";
      }
      s = vSettings["TakerFee"];
      if (s != "") {
        try {
          TakerFee = Convert.ToDouble(s);
        } catch {
        }
      } else {
        vSettings["TakerFee"] = "0.0025";
      }      
    }

    private void LoadInitialSettings() {
      try {
        kpPolo = KeyPair.ReadEncoded(true);
        if (kpPolo != null) {
          pc = new PoloniexClient(kpPolo.sPU(), kpPolo.sPR());
          pc.Live.OnTickerChanged += Live_OnTickerChanged;
        //  pc.Live.OnTrollboxMessage += Live_OnTrollboxMessage;
        }
      } catch (Exception e01) {
        LogException("LIS", e01);
        this.Close();
        throw e01;
      }
    }
       
    private void Form1_Shown(object sender, EventArgs e) {
      tDisplay.Enabled = true;
      tBotCmd.Enabled = true;
      tPoloCmd.Enabled = true;
      tClock.Enabled = true;
    }
    private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
      if (tPoloCmd.Enabled) {
        tPoloCmd.Enabled = false;
      }
      if (tBotCmd.Enabled) {
        tBotCmd.Enabled = false;
      }
      if (tDisplay.Enabled) {
        tDisplay.Enabled = false;
      }
      if (tClock.Enabled) {
        tClock.Enabled = false;
      }
    }
    #endregion 
       
    #region tClock
    DateTime daTime, daLastTime;
    double dmsecs = 0;
    Boolean ctFTT = true, mmFTT = true; 
    Int64 ftPerSec=0, ftPerMin = 0;
    Int64 iLastFeedTickNo = 0, iLastFeedMinTickNo=0, iLastUptimeMS=0;
    Int32 iLastMin = 0;
    DateTime upStartTime;
    string sUpTime = "";
    Boolean bShiftToggle = false;
    Boolean bIsLiveAvailable = false;
    private void tClock_Tick(object sender, EventArgs e) {
      daTime = DateTime.Now;
      Boolean bItsANewMin = false;
      if (ctFTT) {  // First time through just mark the start time. 
        ctFTT = false;
        upStartTime =daTime;
        iLastUptimeMS = daTime.Hour * 60 * 60 * 1000 + daTime.Minute * 60 * 1000 + daTime.Second * 1000 + daTime.Millisecond;
      } else {
        #region Initial Time measurements 
        Int64 msecs = daTime.Hour * 60*60*1000 + daTime.Minute * 60 * 1000 +daTime.Second * 1000 + daTime.Millisecond;
        Int64 lmsecs = daLastTime.Hour * 60 * 60 * 1000 + daLastTime.Minute * 60 * 1000 + daLastTime.Second * 1000 + daLastTime.Millisecond;
        dmsecs = (msecs - lmsecs)/1000;

        sUpTime = Convert.ToString(Convert.ToInt32( ((msecs - iLastUptimeMS) / 1000) / 60 / 60) ) + ":" + Convert.ToString(Convert.ToInt32( ((msecs - iLastUptimeMS) / 1000) / 60)%60 ) + ":" + Convert.ToString(Convert.ToInt32( ((msecs - iLastUptimeMS) / 1000))%60 )+"s";
        ftPerSec = feedTickNo - iLastFeedTickNo;
        SortedList<double, appMarket> iT = BestBuys;     // mark #1 spot.
        if (iT.Count > 0) {
          double dKey = iT.Keys[iT.Count - 1];
          iT[dKey].Number1SpotCount = iT[dKey].Number1SpotCount + 1;
        }
        if (daTime.Minute != iLastMin) {                 // on minute changes  
          iLastMin = daTime.Minute;
          ftPerMin = feedTickNo - iLastFeedMinTickNo;
          iLastFeedMinTickNo = feedTickNo;
          //bItsANewMin = true;
          if ((lPoloQue.Count == 0)&&(!mmFTT)) {
            DoGetMarketHistory = true;
          }
          if (mmFTT) { mmFTT = false; }
        }

        if ((feedTickNo < 3) || (ftPerMin < 2)) {
          bIsLiveAvailable = false;
        }
        #endregion 
        #region Compute updates to averages for each market.
        foreach (CurrencyPair cp in MarketsOfInterest.Keys) {
          MarketsOfInterest[cp].BuyTicsPerSec = MarketsOfInterest[cp].BuyTickerHeight - MarketsOfInterest[cp].LastBuyTickerHeight;
          MarketsOfInterest[cp].SellTicksPerSec = MarketsOfInterest[cp].SellTickerHeight - MarketsOfInterest[cp].LastSellTickerHeight;
          MarketsOfInterest[cp].BuyTicAvg.Insert(0, Convert.ToDouble(MarketsOfInterest[cp].BuyTicsPerSec));
          MarketsOfInterest[cp].SellTicAvg.Insert(0, Convert.ToDouble(MarketsOfInterest[cp].SellTicksPerSec));
          if (MarketsOfInterest[cp].BuyTicAvg.Count > 60) {
            MarketsOfInterest[cp].BuyTicAvg.RemoveAt(60);
          }
          if (MarketsOfInterest[cp].SellTicAvg.Count > 60) {
            MarketsOfInterest[cp].SellTicAvg.RemoveAt(60);
          }
          MarketsOfInterest[cp].PerSecBuyTicAvg = MarketsOfInterest[cp].BuyTicAvg.toAvg();
          MarketsOfInterest[cp].FiveSecBuyTicAvg = MarketsOfInterest[cp].BuyTicAvg.toAvg5();
          MarketsOfInterest[cp].PerSecSellTicAvg = MarketsOfInterest[cp].SellTicAvg.toAvg();
          MarketsOfInterest[cp].FiveSecSellTicAvg = MarketsOfInterest[cp].SellTicAvg.toAvg5();
          MarketsOfInterest[cp].LastBuyTickerHeight = MarketsOfInterest[cp].BuyTickerHeight;
          MarketsOfInterest[cp].LastSellTickerHeight = MarketsOfInterest[cp].SellTickerHeight;

          if (MarketsOfInterest[cp].LastBuyPrice == 0) {
            MarketsOfInterest[cp].LastBuyPrice = MarketsOfInterest[cp].LastMarketData.PriceLast;
          } else {
            double dDiff = ( MarketsOfInterest[cp].LastMarketData.PriceLast- MarketsOfInterest[cp].LastBuyPrice);
            MarketsOfInterest[cp].PriceChangeAvg.Insert(0, Convert.ToDouble(dDiff));
            if (MarketsOfInterest[cp].PriceChangeAvg.Count > 120) {
              MarketsOfInterest[cp].PriceChangeAvg.RemoveAt(120);
            }
          }

          MarketsOfInterest[cp].PerSecBuyPriceChangeAvg = MarketsOfInterest[cp].PriceChangeAvg.toAvg5();          

          if (MarketsOfInterest[cp].PerSecLastPriceChangeAvg == 0) {
            MarketsOfInterest[cp].PerSecPriceRateOfChange = 0;
            MarketsOfInterest[cp].PerSecLastPriceChangeAvg = MarketsOfInterest[cp].PerSecBuyPriceChangeAvg;            
          } else {
            MarketsOfInterest[cp].PerSecPriceRateOfChange = MarketsOfInterest[cp].PerSecBuyPriceChangeAvg - MarketsOfInterest[cp].PerSecLastPriceChangeAvg;
            MarketsOfInterest[cp].PerSecLastPriceChangeAvg = MarketsOfInterest[cp].PerSecBuyPriceChangeAvg;            
          }

          MarketsOfInterest[cp].PriceRateOfChangeHistory.Add(MarketsOfInterest[cp].PerSecPriceRateOfChange); //MarketsOfInterest[cp].PerSecBuyPriceChangeAvg);
          if (MarketsOfInterest[cp].PriceRateOfChangeHistory.Count > 15) {
            MarketsOfInterest[cp].PriceRateOfChangeHistory.RemoveAt(0);
          }
          MarketsOfInterest[cp].PriceRateOfChangeAvg = MarketsOfInterest[cp].PriceRateOfChangeHistory.toAvg();

          if ((MarketDetailVisible)&&(FocusedMarket!=null)&&(FocusedMarket==cp)){
            if (daTime.Second % 9 == 0) {
              Dictionary<string, object> aCMDParams = new Dictionary<string, object>();
              aCMDParams["CurrencyPair"] = FocusedMarket;
              lPoloQue.ChkAdd(new appCmd("MarketsGetOpenOrders", aCMDParams));
              lPoloQue.ChkAdd(new appCmd("MarketsGetTrades", aCMDParams));              
            }
          }
        }
        

        foreach (string sCur in Balances.Keys) {        // recompute sums of newly calculated amounts
          if (sCur != "BTC"){
            CurrencyPair acp = new CurrencyPair("BTC", sCur);
            if (MarketsOfInterest.Keys.Contains(acp)){
              Balances[sCur].BitcoinValue = Balances[sCur].DBQuote * MarketsOfInterest[acp].LastMarketData.PriceLast; 
            }
          }
        }
        #endregion
        #region Trade Algo direction depending upon mode.

        if ((!bIsLiveAvailable) && ((daTime.Second == 23)||(daTime.Second==53))) {
          lPoloQue.Add(new appCmd("MarketsGetSummary", null));
        }

        if ((daTime.Second == 45) && (lPoloQue.Count == 0)) {  // check balances on the 15 and 45. 
          lPoloQue.Add(new appCmd("GetBalances", null));
          DoGetTradeHistory = true;
        }
        //  && (daTime.Second == 30)
        if ((cbTradeGo.Checked) && ((daTime.Second == 40) || (daTime.Second == 10))) {

          foreach (double iBSK in BestSells.Keys) {
            appMarket am = BestSells[iBSK];
            if (am != null) {
              if ((am.GoHold) && (am.HoldAmount > 0)) { 
                //Balances[ am.CurPair.QuoteCurrency ].DBQuote  

                double HoldSell = Convert.ToDouble(MarketsOfInterest[am.CurPair].HoldAmount + MarketsOfInterest[am.CurPair].SellThreshold);
                double HoldBuy = Convert.ToDouble(MarketsOfInterest[am.CurPair].HoldAmount - MarketsOfInterest[am.CurPair].BuyThreshold);
                double SellWeightDiff = (Balances[am.CurPair.QuoteCurrency].DBQuote * MarketsOfInterest[am.CurPair].LastMarketData.OrderTopSell) - (HoldSell);
                double BuyWeightDiff = (Balances[am.CurPair.QuoteCurrency].DBQuote * MarketsOfInterest[am.CurPair].LastMarketData.OrderTopSell) - (HoldBuy);
                double TargetSellBtcVol = 0;
                double TargetSellQuoteVol = 0;                 
                double TargetSellPrice = 0;
                double TargetBuyBtcVol = 0;
                double TargetBuyQuoteVol = 0;  
                double TargetBuyPrice = 0;
                if (SellWeightDiff >= 0) {
                  TargetSellPrice = MarketsOfInterest[am.CurPair].LastMarketData.OrderTopSell; 
                  TargetSellBtcVol = (Balances[am.CurPair.QuoteCurrency].DBQuote * TargetSellPrice) - Convert.ToDouble(MarketsOfInterest[am.CurPair].HoldAmount);
                  TargetSellQuoteVol = TargetSellBtcVol / MarketsOfInterest[am.CurPair].LastMarketData.OrderTopSell;                 
                } else {
                  TargetSellPrice = HoldSell / Balances[am.CurPair.QuoteCurrency].DBQuote;
                  TargetSellQuoteVol = Convert.ToDouble(MarketsOfInterest[am.CurPair].SellThreshold) / TargetSellPrice;                  
                }

                if (BuyWeightDiff <= 0) {  // value less than thresh
                  TargetBuyBtcVol = Convert.ToDouble(MarketsOfInterest[am.CurPair].HoldAmount) - (Balances[am.CurPair.QuoteCurrency].DBQuote * MarketsOfInterest[am.CurPair].LastMarketData.OrderTopBuy);
                  TargetBuyQuoteVol = TargetBuyBtcVol / MarketsOfInterest[am.CurPair].LastMarketData.OrderTopBuy;                  
                } else {
                  TargetBuyPrice = HoldBuy / Balances[am.CurPair.QuoteCurrency].DBQuote;
                  TargetBuyQuoteVol = Convert.ToDouble(MarketsOfInterest[am.CurPair].BuyThreshold) / TargetBuyPrice;                  
                }


                if (am.LastOrderSellNum != 0) {
                  if ((am.LastOrderSellPrice != TargetSellPrice) || (am.LastOrderSellVol != TargetSellQuoteVol)) {
                    Boolean bOrderFound = false;
                    if (MarketsOfInterest[am.CurPair].tradeOpenOrders.Count > 0) {
                      foreach (TT.IOrder xT in MarketsOfInterest[am.CurPair].tradeOpenOrders) {
                        if (xT.IdOrder == am.LastOrderSellNum) {
                          bOrderFound = true;
                        }
                      }
                    }
                    if (bOrderFound) {
                      Dictionary<string, object> aCMDParams = new Dictionary<string, object>();
                      aCMDParams["CurrencyPair"] = am.CurPair;
                      aCMDParams["DoRefresh"] = true;
                      aCMDParams["OrderNum"] = am.LastOrderSellNum;
                      aCMDParams["IsMove"] = false;
                      lPoloQue.ChkAdd(new appCmd("TradingDeleteOrder", aCMDParams));
                    } else {
                      am.LastOrderSellNum = 0;
                    }
                  }                 
                } else {   // place order to sell

                  if ((Balances.Keys.Contains(am.CurPair.QuoteCurrency)) && (Balances[am.CurPair.QuoteCurrency].QuoteAvailable > TargetSellQuoteVol)) {
                    Dictionary<string, object> aCMDParams = new Dictionary<string, object>();
                    aCMDParams["CurrencyPair"] = am.CurPair;
                    aCMDParams["OrderType"] = OrderType.Sell;
                    aCMDParams["PricePerCoin"] = TargetSellPrice;
                    aCMDParams["QuoteVolume"] = TargetSellQuoteVol;
                    aCMDParams["DoRefresh"] = true;
                    lPoloQue.Add(new appCmd("TradingPostOrder", aCMDParams));

                  }
                }


                if (am.LastOrderBuyNum != 0) {
                  if ((am.LastOrderBuyPrice != TargetBuyPrice) || (am.LastOrderBuyVol != TargetBuyQuoteVol)) {
                    Boolean bOrderFound = false;
                    if (MarketsOfInterest[am.CurPair].tradeOpenOrders.Count > 0) {
                      foreach (TT.IOrder xT in MarketsOfInterest[am.CurPair].tradeOpenOrders) {
                        if (xT.IdOrder == am.LastOrderBuyNum) {
                          bOrderFound = true;
                        }
                      }
                    }
                    if (bOrderFound) {
                      Dictionary<string, object> aCMDParams = new Dictionary<string, object>();
                      aCMDParams["CurrencyPair"] = am.CurPair;
                      aCMDParams["DoRefresh"] = true;
                      aCMDParams["OrderNum"] = am.LastOrderBuyNum;
                      aCMDParams["IsMove"] = false;
                      lPoloQue.ChkAdd(new appCmd("TradingDeleteOrder", aCMDParams));
                    } else {
                      am.LastOrderBuyNum = 0;
                    }
                  }
                } else {

                  if ((Balances.Keys.Contains("BTC")) && (Balances["BTC"].QuoteAvailable > (TargetBuyBtcVol))) {
                    Dictionary<string, object> aCMDParams = new Dictionary<string, object>();
                    aCMDParams["CurrencyPair"] = am.CurPair;
                    aCMDParams["OrderType"] = OrderType.Buy;
                    aCMDParams["PricePerCoin"] = TargetBuyPrice;
                    aCMDParams["QuoteVolume"] = TargetBuyQuoteVol;
                    aCMDParams["DoRefresh"] = true;
                    lPoloQue.Add(new appCmd("TradingPostOrder", aCMDParams));
                  }
                }

              }
            }
          }

        }


        #region older trader
        // (daTime.Second == 15) ||  (daTime.Second == 38) 
        /*
        if (tbRunMode.Value == 3) {
          if ((daTime.Second == 20) || (daTime.Second == 40)) {
            bItsANewMin = true;
          }
        } else {
          if ((daTime.Second == 23) || (daTime.Second == 30) || (daTime.Second == 15) || (daTime.Second == 38)) {
            bItsANewMin = true;
          }
        }
        
        if (tbRunMode.Value == 0) {
          // off
        } else if (tbRunMode.Value == 1) {
          #region Buy Top 1 algo 
          if (bItsANewMin) {
            SortedList<double, appMarket> bb = BestBuys;
            if (bb.Count > 0) {
              double dKey = bb.Keys[bb.Count - 1];  //Find the #1 market.
              CurrencyPair FirstMarket = bb[dKey].CurPair;              
              if (!MarketDetailVisible){
                MarketDetailVisible = true;
              }
              if (FocusedMarket != FirstMarket) {  //First time through just switch to new market.                
                if ((MarketsOfInterest.Keys.Contains(FocusedMarket))&&(MarketsOfInterest[FocusedMarket].tradeOpenOrders.Count>0)){  // if old market has buy's out cancel them.
                  List<Jojatekok.PoloniexAPI.TradingTools.IOrder> chkOO = MarketsOfInterest[FocusedMarket].tradeOpenOrders;
                  Dictionary<string, object> aCMDParams = new Dictionary<string, object>();
                  aCMDParams["CurrencyPair"] = FocusedMarket;
                  aCMDParams["DoRefresh"] = true;  //if a order was placed but #1 spot switches cancel the order on previous market.
                  foreach (Jojatekok.PoloniexAPI.TradingTools.IOrder iO in chkOO) {
                    if (iO.Type == OrderType.Buy) {
                      aCMDParams["OrderNum"] = iO.IdOrder;
                      aCMDParams["IsMove"] = false;
                      lPoloQue.ChkAdd(new appCmd("TradingDeleteOrder", aCMDParams));
                    }
                  }
                }                
                FocusedMarket = FirstMarket;
              } else {                        
                if (!MarketsOfInterest[FocusedMarket].Selected) {
                  MarketsOfInterest[FocusedMarket].Selected = true;
                }
                Int32 iFocusMarketStatus = GetMarketState(FocusedMarket);
                if (iFocusMarketStatus == 1) {
                  if (MarketsOfInterest[FocusedMarket].Number1SpotCount > 20) {
                    btnBuyM_Click(sender, e);
                  }
                } else if ((iFocusMarketStatus == 2) || (iFocusMarketStatus==3)) {
                  if (MarketsOfInterest[FocusedMarket].NextBuyPrice != MarketsOfInterest[FocusedMarket].LastOrderBuyPrice) {
                    btnCancel_Click(sender, e);
                  }  
                } else if (iFocusMarketStatus == 4) {
                  btnSellM_Click(sender, e);
                }
              }               
            }
          }
          #endregion
        } else if (tbRunMode.Value == 2) {
          #region Shift Algo 
          SortedList<double, appMarket> bs = BestSells;
          if ((bItsANewMin)&&(bs.Count > 0)) {
            CurrencyPair FirstMarket = GetWorstSellMarket();  //Find the worst performing market with a vol.          
            if (FirstMarket != null) {
              if (!MarketDetailVisible) {
                MarketDetailVisible = true;
              }
              if (!bShiftToggle) {  // 0 or need to sell off                                  
                if (FocusedMarket != FirstMarket) {  //First time through just switch to new market.
                  FocusedMarket = FirstMarket;
                } else {
                  Int32 iFocusMarketStatus = GetMarketState(FocusedMarket);
                  if (iFocusMarketStatus == 4) {
                    btnSell_Click(sender, e);
                    bShiftToggle = !bShiftToggle;
                  } else if ((iFocusMarketStatus == 5) || (iFocusMarketStatus == 2) || (iFocusMarketStatus == 3)) {
                    btnCancel_Click(sender, e);
                  } else if (iFocusMarketStatus == 0) {
                   bShiftToggle = !bShiftToggle;
                  }
                }
              } else { // 1 or need to buy back                              
                FirstMarket = GetBestBuyMarket();
                if (FirstMarket != null) {
                  if (FocusedMarket != FirstMarket) {  //First time through just switch to new market.                  
                    FocusedMarket = FirstMarket;
                    if (!MarketsOfInterest[FocusedMarket].Selected) {
                      MarketsOfInterest[FocusedMarket].Selected = true;
                    }
                  } else { // next time through               
                    Int32 iFocusMarketStatus = GetMarketState(FocusedMarket);
                    if ((iFocusMarketStatus == 1) || (iFocusMarketStatus == 4)) {
                      btnBuy_Click(sender, e);
                      bShiftToggle = !bShiftToggle;
                    } else if ((iFocusMarketStatus == 5) || (iFocusMarketStatus == 2) || (iFocusMarketStatus == 3)) {
                      btnCancel_Click(sender, e);
                    } else if (iFocusMarketStatus == 0) {
                      bShiftToggle = !bShiftToggle;
                    }
                  }
                }
              }
            }
          }
          #endregion
        } else if (tbRunMode.Value == 3) {
          #region Sell Off Algo
          SortedList<double, appMarket> bs = BestSells;
          if ((bItsANewMin) && (bs.Count > 0)) {
            CurrencyPair FirstMarket = GetBestSellMarket();  //Find the worst performing market with a vol.          
            if (FirstMarket != null) {
              if (!MarketDetailVisible) {
                MarketDetailVisible = true;
              }
              if (!bShiftToggle) {  // 0 or need to place sell on whale                                  
                if (FocusedMarket != FirstMarket) {  //First time through just switch to new market.
                  FocusedMarket = FirstMarket;
                } else {
                  Int32 iFocusMarketStatus = GetMarketState(FocusedMarket);
                  if (iFocusMarketStatus == 4) {
                    btnSell_Click(sender, e);
                    bShiftToggle = !bShiftToggle;
                  } else if ((iFocusMarketStatus == 5) || (iFocusMarketStatus == 2) || (iFocusMarketStatus == 3)) {
                    btnCancel_Click(sender, e);
                  } else  {
                    bShiftToggle = !bShiftToggle;
                  }
                }
              } else { // 1 need to dump                               
                FirstMarket = GetBestSellMarket();
                if (FirstMarket != null) {
                  if (FocusedMarket != FirstMarket) {  //First time through just switch to new market.                  
                    FocusedMarket = FirstMarket;
                    if (!MarketsOfInterest[FocusedMarket].Selected) {
                      MarketsOfInterest[FocusedMarket].Selected = true;
                    }
                  } else { // next time through               
                    Int32 iFocusMarketStatus = GetMarketState(FocusedMarket);
                    if ((iFocusMarketStatus == 5) || (iFocusMarketStatus == 4)) {
                      btnSell_Click(sender, e);
                      bShiftToggle = !bShiftToggle;
                    } else if (iFocusMarketStatus == 0) {
                      bShiftToggle = !bShiftToggle;
                    }
                  }
                }
              }
            }
          }
            
          #endregion
        }
        #endregion
        */
        #endregion 
        #endregion
      }  
      daLastTime = daTime;
      iLastFeedTickNo = feedTickNo;
    }

    public CurrencyPair GetBestBuyMarket() {
      CurrencyPair acp = null;
      if ((BestSells.Count > 0) && (BestBuys.Count > 0)) {        
        SortedList<double, appMarket> bb = BestBuys;
        SortedList<double, appMarket> bs = BestSells;        
        // start by finding the worst. 
        Int32 i = 0;
        double dZeroKey = bs.Keys[0];  //Find the worst performing market with a vol.
        while ((i <= bs.Keys.Count - 1) &&
          ((bs[dZeroKey].CurPair.QuoteCurrency == "BTC") ||
           (!Balances.Keys.Contains(bs[dZeroKey].CurPair.QuoteCurrency)) ||
           ((Balances.Keys.Contains(bs[dZeroKey].CurPair.QuoteCurrency)) && (Balances[bs[dZeroKey].CurPair.QuoteCurrency].QuoteAvailable < 0.0005)) 
          )
        ) {
          i++;
          if (i <= bs.Keys.Count - 1) {
            dZeroKey = bs.Keys[i];
          }
        }

        if (bs.Keys.Contains(dZeroKey)) {
          acp = bs[dZeroKey].CurPair;
          if ((Balances.Keys.Contains(acp.QuoteCurrency)) && (Balances[acp.QuoteCurrency].QuoteAvailable < 0.0005)) {
            acp = null;
          }
        }
        CurrencyPair WorstSell = acp;

        // search fo the best...

        i = bb.Keys.Count - 1;
        dZeroKey = bb.Keys[i];  // Find best performing market.                   
        while ((i <= 0) && 
          ((bb[dZeroKey].CurPair.QuoteCurrency == "BTC") || 
           (bb[dZeroKey].CurPair != WorstSell) ||
           ((bb[dZeroKey].hasTradeHistory) && (bb[dZeroKey].TradeHistoryBuyVol > bb[dZeroKey].TradeHistorySellVol)) ||
           (bb[dZeroKey].PriceRateOfChangeAvg < 0))) {
          i--;
          if (i >= 0) {
            dZeroKey = bb.Keys[i];
          }
        }

        if ((i>=0)&&(bb.Keys.Contains(dZeroKey))) {
          acp = bb[dZeroKey].CurPair;          
        }
      }
      return acp;
    }

    public CurrencyPair GetWorstSellMarket() {
      CurrencyPair acp = null;
      if ((BestSells.Count>0)&&(BestBuys.Count>0)) {
        SortedList<double, appMarket> bb = BestBuys;
        SortedList<double, appMarket> bs = BestSells;                
        double dFirstKey = bb.Keys[bb.Count - 1];  //Find the #1 market.
        CurrencyPair FirstMarket = bb[dFirstKey].CurPair;
        Int32 i = 0;

        double dZeroKey = bs.Keys[0];  //Find the worst performing market with a vol.
        while ((i <= bs.Keys.Count - 1) && 
          ((bs[dZeroKey].CurPair.QuoteCurrency == "BTC") ||
           (!Balances.Keys.Contains(bs[dZeroKey].CurPair.QuoteCurrency)) ||
           ((Balances.Keys.Contains(bs[dZeroKey].CurPair.QuoteCurrency)) && (Balances[bs[dZeroKey].CurPair.QuoteCurrency].QuoteAvailable < 0.0005)) ||
           (bs[dZeroKey].CurPair == FirstMarket)
          )
        ) {
          i++;
          if(i<=bs.Keys.Count-1){
            dZeroKey = bs.Keys[i]; 
          }
        }

        if (bs.Keys.Contains(dZeroKey)) {
          acp = bs[dZeroKey].CurPair;
          if ((Balances.Keys.Contains(acp.QuoteCurrency)) && (Balances[acp.QuoteCurrency].QuoteAvailable < 0.0005)) {
            acp = null;
          }
        }
      }
      return acp;
    }

    public CurrencyPair GetBestSellMarket() {
      CurrencyPair acp = null;
      if ((BestSells.Count > 0) && (BestBuys.Count > 0)) {
        SortedList<double, appMarket> bs = BestSells;
        Int32 i = bs.Count-1;
        double dZeroKey = bs.Keys[i];  //Find the Best performing market with a vol.
        while ((i >= 0) &&
          ((bs[dZeroKey].CurPair.QuoteCurrency == "BTC") ||
           (!Balances.Keys.Contains(bs[dZeroKey].CurPair.QuoteCurrency)) ||
           ((Balances.Keys.Contains(bs[dZeroKey].CurPair.QuoteCurrency)) && (Balances[bs[dZeroKey].CurPair.QuoteCurrency].QuoteAvailable < 0.0005))
          )
        ) {
          i--;
          if (i >= 0) {
            dZeroKey = bs.Keys[i];
          }
        }

        if (bs.Keys.Contains(dZeroKey)) {
          acp = bs[dZeroKey].CurPair;
          if ((Balances.Keys.Contains(acp.QuoteCurrency)) && (Balances[acp.QuoteCurrency].QuoteAvailable < 0.0005)) {
            acp = null;
          }
        }
      }
      return acp;
    }
    #endregion

    #region UI interactions
    public Int32 GetMarketState(CurrencyPair acp) {

      Boolean hasQuote = false;
      Boolean hasBase = false;
      Boolean hasBuy = false;
      Boolean hasSell = false;

      if (Balances.Keys.Contains(acp.BaseCurrency)) {
        if (Balances[acp.BaseCurrency].QuoteAvailable > 0) {
          hasBase = true;
        }
      }

      if (Balances.Keys.Contains(acp.QuoteCurrency)) {
        if (Balances[acp.QuoteCurrency].QuoteAvailable > 0) {
          hasQuote = true;
        }
      }

      if (MarketsOfInterest[acp].tradeOpenOrders.Count > 0) {
        List<Jojatekok.PoloniexAPI.TradingTools.IOrder> xMOITOO = MarketsOfInterest[acp].tradeOpenOrders;
        foreach (Jojatekok.PoloniexAPI.TradingTools.IOrder xO in xMOITOO) {
          if (xO.Type == OrderType.Buy) {
            hasBuy = true;
          } else {
            hasSell = true;
          }
        }
      }

      Int32 iStatus = 0;
      if ((!hasBuy) && (!hasSell) && (hasBase) && (!hasQuote)) {
        iStatus = 1; // need to place order
      }
      if ((hasBuy) && (!hasSell) && (!hasQuote)) {
        iStatus = 2; // has order no fill
      }
      if ((hasBuy) && (!hasSell) && (hasQuote)) {
        iStatus = 3; // has order some fill
      }
      if ((!hasBuy) && (!hasSell) && (hasQuote)) {
        iStatus = 4;  // no buy order but has quote need to place sell order
      }
      if ((hasSell) && (!hasQuote)) {
        iStatus = 5;  // has sell order out
      }
      return iStatus;
    }
    private void Form1_MouseDown(object sender, MouseEventArgs e) {
      float fWidth = this.DisplayRectangle.Width;
      float fHeight = this.DisplayRectangle.Height;
      double f20Height = fHeight * 0.2;
      double f05Height = fHeight * 0.065;
      double f15Height = fHeight * 0.145;
      double f20Width = fWidth * 0.2;
      double f15Width = fWidth * 0.15;
      Boolean handledClick = false;

      if ((MarketDetailVisible) && (FocusedMarket != null)) {
        Int32 iLeft = Convert.ToInt32(0);
        Int32 iTop = Convert.ToInt32(f05Height);
        Int32 iWidth = Convert.ToInt32(f20Width);
        Int32 iHeight = Convert.ToInt32(f05Height + 4 * f15Height);
        if ((e.Y >= f05Height) && (e.Y <= f05Height + f05Height/2)) {
          if (e.X < (f20Width / 5)) {
            MarketsOfInterest[FocusedMarket].mv["MarketViewMode"] = "Shards";
            handledClick = true;
          } else if (e.X < (2 * f20Width / 5)) {
            MarketsOfInterest[FocusedMarket].mv["MarketViewMode"] = "Vars";
            handledClick = true;
          } else if (e.X < (3 * f20Width / 5)) {
            MarketsOfInterest[FocusedMarket].mv["MarketViewMode"] = "Charts";
            handledClick = true;
          }
        }
      }

      Int32 iRow = 0;
      if ((e.X < f20Width)&&(e.Y < f05Height)) {
        #region Menu button Bar
         iRow = 20; 
         if (e.X < f05Height * 1) {
           if (iDisplayMode != 0) {
             iDisplayMode = 0;
           }
         } else if (e.X < f05Height * 2) {
           if (iDisplayMode != 1) {
             iDisplayMode = 1;
           }
         } else if (e.X < f05Height * 3) {
           if (iDisplayMode != 2) {
             iDisplayMode = 2;
           }
         }
        #endregion
      } else if ((iDisplayMode == 0)&&(!handledClick)) {
        #region Rank Orderd list of top 20 markets 
         SortedList<double, appMarket> BestBuysT = BestBuys;
         Int32 iCol = Convert.ToDouble(e.X / f20Width).toInt32T();
         if (e.X < f20Width) { 
           iCol = 0; 
         } else if (e.X < f20Width * 2 ) { 
           iCol = 1; 
         } else if (e.X < f20Width * 3) {
           iCol = 2;
         } else if (e.X < f20Width * 4) {
           iCol = 3;
         } else if (e.X < f20Width * 5) {
           iCol = 4;
         }
       
         iRow = 0;
         if (e.Y < f05Height) {
           iRow = 20;
         } else if (e.Y < f20Height) {
           iRow = 0; 
         } else if (e.Y < f20Height + f15Height){
           iRow=1;
         } else if (e.Y < f20Height + 2* f15Height){
           iRow=2;
         } else if (e.Y < f20Height + 3 * f15Height) {
           iRow = 3;
         } else if (e.Y < 2*f20Height + 2 * f15Height) {
           iRow = 10;
         } else if (e.Y < 2*f20Height + 3 * f15Height) {
           iRow = 4;
         } else if (e.Y < 2 * f20Height + 4 * f15Height) {
           iRow = 5;
         }


         try {
           if (MarketDetailVisible) {
             if (((iRow == 1) || (iRow == 2) || (iRow == 3)) && ((iCol == 1) || (iCol == 2)||(iCol==3))) {

             } else {
               if (iRow < 4) {
                 MarketDetailVisible = false;
               }
             }
           } else {

             if (iRow < 4) {
               Int32 iRank = ((iRow * 5) + iCol);
               if (BestBuysT.Count > iRank) {
                 double indexIn = BestBuysT.Keys[BestBuysT.Count - 1 - iRank];
                 if (BestBuysT[indexIn].Selected) {
                   BestBuysT[indexIn].Selected = false;
                 } else {
                   BestBuysT[indexIn].Selected = true;
                 }
               }
             }

           }

           if ((iRow == 4) || (iRow == 5)) {
             #region X's on bottom
             if ((e.X > 0) && (e.X < 14)                          && (e.Y > 2 * f05Height + 4 * f15Height) && (e.Y < 2 * f05Height + 4 * f15Height + 14)) { 
               if (BestSells.Count>=1){ 
                 double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (0)];
                 BestSells[BestSellsKey].Selected = false;
                 if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                   MarketDetailVisible = false;
                 }               
               }
             } else if ((e.X > f20Width) && (e.X < f20Width + 14) && (e.Y > 2 * f05Height + 4 * f15Height) && (e.Y < 2 * f05Height + 4 * f15Height + 14)) { 
               if (BestSells.Count>=2){ 
                 double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (1)];
                 BestSells[BestSellsKey].Selected = false;
                 if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                   MarketDetailVisible = false;
                 }
               }
             } else if ((e.X > f20Width * 2) && (e.X < f20Width * 2 + 14) && (e.Y > 2 * f05Height + 4 * f15Height) && (e.Y < 2 * f05Height + 4 * f15Height + 14)) { 
               if (BestSells.Count>=3){ 
                 double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (2)];
                 BestSells[BestSellsKey].Selected = false;
                 if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                   MarketDetailVisible = false;
                 }
               }
             } else if ((e.X > f20Width * 3) && (e.X < f20Width * 3 + 14) && (e.Y > 2 * f05Height +4 * f15Height) && (e.Y < 2 * f05Height + 4 * f15Height + 14)) { 
               if (BestSells.Count>=4){ 
                 double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (3)];
                 BestSells[BestSellsKey].Selected = false;
                 if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                   MarketDetailVisible = false;
                 }
               }
             } else if ((e.X > f20Width * 4) && (e.X < f20Width * 4 + 14) && (e.Y > 2 * f05Height + 4 * f15Height) && (e.Y < 2 * f05Height + 4 * f15Height + 14)) { 
               if (BestSells.Count>=5){ 
                 double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (4)];
                 BestSells[BestSellsKey].Selected = false;
                 if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                   MarketDetailVisible = false;
                 }
               }
             } else if ((e.X > 0) && (e.X < 14) && (e.Y > 2 * f05Height + 5 * f15Height) && (e.Y < 2 * f05Height + 5 * f15Height + 14)) { 
               if (BestSells.Count>=6){ 
                 double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (5)];
                 BestSells[BestSellsKey].Selected = false;
                 if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                   MarketDetailVisible = false;
                 }
               }
             } else if ((e.X > f20Width) && (e.X < f20Width + 14) && (e.Y > 2 * f05Height + 5 * f15Height) && (e.Y < 2 * f05Height + 5 * f15Height + 14)) { 
               if (BestSells.Count>=7){ 
                 double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (6)];
                 BestSells[BestSellsKey].Selected = false;
                 if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                   MarketDetailVisible = false;
                 }
               }
             } else if ((e.X > f20Width * 2) && (e.X < f20Width * 2 + 14) && (e.Y > 2 * f05Height + 5 * f15Height) && (e.Y < 2 * f05Height + 5 * f15Height + 14)) { 
               if (BestSells.Count>=8){ 
                 double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (7)];
                 BestSells[BestSellsKey].Selected = false;
                 if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                   MarketDetailVisible = false;
                 }
               }
             } else if ((e.X > f20Width * 3) && (e.X < f20Width * 3 + 14) && (e.Y > 2 * f05Height + 5 * f15Height) && (e.Y < 2 * f05Height + 5 * f15Height + 14)) { 
               if (BestSells.Count>=9){ 
                 double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (8)];
                 BestSells[BestSellsKey].Selected = false;
                 if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                   MarketDetailVisible = false;
                 }
               }
             } else if ((e.X > f20Width * 4) && (e.X < f20Width * 4 + 14) && (e.Y > 2 * f05Height + 5 * f15Height) && (e.Y < 2 * f05Height + 5 * f15Height + 14)) {
               if (BestSells.Count >= 10) {
                 double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (9)];
                 BestSells[BestSellsKey].Selected = false;
                 if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                   MarketDetailVisible = false;
                 }
               }
             } else {
               if (!MarketDetailVisible) {
                 MarketDetailVisible = true;
               }
               Int32 iRank = (((iRow - 4) * 5) + iCol);
               if (BestSells.Count > iRank) {
                 double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (iRank)];
                 FocusedMarket = BestSells[BestSellsKey].CurPair;
               }
             }
             #endregion
           }

           if (iRow == 10) {
             if (MarketDetailVisible) {
               MarketDetailVisible = false;
               FocusedMarket = null;
             }
           }

       } catch{ }
         #endregion
      } else if ((iDisplayMode == 1)&&(!handledClick)) {
        #region Alpha ordered list 10x10 
         Int32 iCol = Convert.ToDouble(e.X / (f20Width / 2)).toInt32T();         
         iRow = 0;
         if (e.Y < f05Height) {
           iRow = 20;        
         } else if (e.Y < f20Height + 3 * f15Height) {
           iRow = Convert.ToDouble((e.Y - f05Height) / (f15Height / 2.5)).toInt32T();
         }else if (e.Y < 2 * f20Height + 2 * f15Height) {
           iRow = 10;
         } else if (e.Y < 2*f20Height + 3 * f15Height) {
           iRow = 11;
         } else if (e.Y < 2 * f20Height + 4 * f15Height) {
           iRow = 12;
         }
         
         Int32 iRank = ((iRow * 10)+iCol);
         try {
           if (MarketDetailVisible) {
             if (((iRow >= 2) && (iRow <= 8)) && ((iCol >= 2) && (iCol <= 8))) {

             } else {
               if (iRow < 9) {
                 MarketDetailVisible = false;
               }
             }
           } else {

             if ((iRank>=0)&&(AlphaMarkets.Count > iRank)) {
               string indexIn = AlphaMarkets.Keys[iRank];
               if (AlphaMarkets[indexIn].Selected) {
                 AlphaMarkets[indexIn].Selected = false;
               } else {
                 AlphaMarkets[indexIn].Selected = true;
               }
             }

           }

           if ((iRow == 11) || (iRow == 12)) {
             #region X's on bottom
             if ((e.X > 0) && (e.X < 14) && (e.Y > 2 * f20Height + 2 * f15Height) && (e.Y < 2 * f20Height + 2 * f15Height + 14)) {
               if (BestSells.Count >= 1) {
                 double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (0)];
                 BestSells[BestSellsKey].Selected = false;
                 if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                   MarketDetailVisible = false;
                 }
               }
             } else if ((e.X > f20Width) && (e.X < f20Width + 14) && (e.Y > 2 * f20Height + 2 * f15Height) && (e.Y < 2 * f20Height + 2 * f15Height + 14)) {
               if (BestSells.Count >= 2) {
                 double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (1)];
                 BestSells[BestSellsKey].Selected = false;
                 if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                   MarketDetailVisible = false;
                 }
               }
             } else if ((e.X > f20Width * 2) && (e.X < f20Width * 2 + 14) && (e.Y > 2 * f20Height + 2 * f15Height) && (e.Y < 2 * f20Height + 2 * f15Height + 14)) {
               if (BestSells.Count >= 3) {
                 double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (2)];
                 BestSells[BestSellsKey].Selected = false;
                 if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                   MarketDetailVisible = false;
                 }
               }
             } else if ((e.X > f20Width * 3) && (e.X < f20Width * 3 + 14) && (e.Y > 2 * f20Height + 2 * f15Height) && (e.Y < 2 * f20Height + 2 * f15Height + 14)) {
                 if (BestSells.Count >= 4) {
                   double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (3)];
                   BestSells[BestSellsKey].Selected = false;
                   if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                     MarketDetailVisible = false;
                   }
                 }
            } else if ((e.X > f20Width * 4) && (e.X < f20Width * 4 + 14) && (e.Y > 2 * f20Height + 2 * f15Height) && (e.Y < 2 * f20Height + 2 * f15Height + 14)) {
              if (BestSells.Count >= 5) {
                double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (4)];
                BestSells[BestSellsKey].Selected = false;
                if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                  MarketDetailVisible = false;
                }
              }
            } else if ((e.X > 0) && (e.X < 14) && (e.Y > 2 * f20Height + 3 * f15Height) && (e.Y < 2 * f20Height + 3 * f15Height + 14)) {
              if (BestSells.Count >= 6) {
                double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (5)];
                BestSells[BestSellsKey].Selected = false;
                if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                  MarketDetailVisible = false;
                }
              }
            } else if ((e.X > f20Width) && (e.X < f20Width + 14) && (e.Y > 2 * f20Height + 3 * f15Height) && (e.Y < 2 * f20Height + 3 * f15Height + 14)) {
              if (BestSells.Count >= 7) {
                double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (6)];
                BestSells[BestSellsKey].Selected = false;
                if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                  MarketDetailVisible = false;
                }
              }
            } else if ((e.X > f20Width * 2) && (e.X < f20Width * 2 + 14) && (e.Y > 2 * f20Height + 3 * f15Height) && (e.Y < 2 * f20Height + 3 * f15Height + 14)) {
              if (BestSells.Count >= 8) {
                double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (7)];
                BestSells[BestSellsKey].Selected = false;
                if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                  MarketDetailVisible = false;
                }
              }
            } else if ((e.X > f20Width * 3) && (e.X < f20Width * 3 + 14) && (e.Y > 2 * f20Height + 3 * f15Height) && (e.Y < 2 * f20Height + 3 * f15Height + 14)) {
              if (BestSells.Count >= 9) {
                double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (8)];
                BestSells[BestSellsKey].Selected = false;
                if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                  MarketDetailVisible = false;
                }
              }
            } else if ((e.X > f20Width * 4) && (e.X < f20Width * 4 + 14) && (e.Y > 2 * f20Height + 3 * f15Height) && (e.Y < 2 * f20Height + 3 * f15Height + 14)) {
              if (BestSells.Count >= 10) {
                double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (9)];
                BestSells[BestSellsKey].Selected = false;
                if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                  MarketDetailVisible = false;
                } 
            }
            } else {
              if (!MarketDetailVisible) {
                MarketDetailVisible = true;
              }
              iRank = (((iRow - 11) * 5) + iCol/2);
              if (BestSells.Count > iRank) {
                double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (iRank)];
                FocusedMarket = BestSells[BestSellsKey].CurPair;
              }
            }
             #endregion
           }

           if (iRow == 10) {
             if (MarketDetailVisible) {
               MarketDetailVisible = false;
               FocusedMarket = null;
             }
           }

         } catch { }
         #endregion
      } else if ((iDisplayMode == 2)&&(!handledClick)) {
        #region Trollo Watch 
        Int32 iCol = Convert.ToDouble(e.X / (f20Width / 2)).toInt32T();
        iRow = 0;
        if (e.Y < f05Height) {
          iRow = 20;
        } else if (e.Y < f20Height + 3 * f15Height) {
          iRow = 1; // Convert.ToDouble((e.Y - f05Height) / (f15Height / 2.5)).toInt32T();
        } else if (e.Y < 2 * f20Height + 2 * f15Height) {
          iRow = 10;
        } else if (e.Y < 2 * f20Height + 3 * f15Height) {
          iRow = 11;
        } else if (e.Y < 2 * f20Height + 4 * f15Height) {
          iRow = 12;
        }

        Int32 iRank = ((iRow * 10) + iCol);
        try {
          if (MarketDetailVisible) {
          //  if (((iRow >= 2) && (iRow <= 8)) && ((iCol >= 2) && (iCol <= 8))) {

          //  } else {
          //    if (iRow < 9) {
          //      MarketDetailVisible = false;
          //    }
          // }
          } else {         
            //
          }

          if ((iRow == 11) || (iRow == 12)) {
            #region X's on bottom
            if ((e.X > 0) && (e.X < 14) && (e.Y > 2 * f20Height + 2 * f15Height) && (e.Y < 2 * f20Height + 2 * f15Height + 14)) {
              if (BestSells.Count >= 1) {
                double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (0)];
                BestSells[BestSellsKey].Selected = false;
                if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                  MarketDetailVisible = false;
                }
              }
            } else if ((e.X > f20Width) && (e.X < f20Width + 14) && (e.Y > 2 * f20Height + 2 * f15Height) && (e.Y < 2 * f20Height + 2 * f15Height + 14)) {
              if (BestSells.Count >= 2) {
                double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (1)];
                BestSells[BestSellsKey].Selected = false;
                if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                  MarketDetailVisible = false;
                }
              }
            } else if ((e.X > f20Width * 2) && (e.X < f20Width * 2 + 14) && (e.Y > 2 * f20Height + 2 * f15Height) && (e.Y < 2 * f20Height + 2 * f15Height + 14)) {
              if (BestSells.Count >= 3) {
                double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (2)];
                BestSells[BestSellsKey].Selected = false;
                if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                  MarketDetailVisible = false;
                }
              }
            } else if ((e.X > f20Width * 3) && (e.X < f20Width * 3 + 14) && (e.Y > 2 * f20Height + 2 * f15Height) && (e.Y < 2 * f20Height + 2 * f15Height + 14)) {
              if (BestSells.Count >= 4) {
                double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (3)];
                BestSells[BestSellsKey].Selected = false;
                if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                  MarketDetailVisible = false;
                }
              }
            } else if ((e.X > f20Width * 4) && (e.X < f20Width * 4 + 14) && (e.Y > 2 * f20Height + 2 * f15Height) && (e.Y < 2 * f20Height + 2 * f15Height + 14)) {
              if (BestSells.Count >= 5) {
                double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (4)];
                BestSells[BestSellsKey].Selected = false;
                if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                  MarketDetailVisible = false;
                }
              }
            } else if ((e.X > 0) && (e.X < 14) && (e.Y > 2 * f20Height + 3 * f15Height) && (e.Y < 2 * f20Height + 3 * f15Height + 14)) {
              if (BestSells.Count >= 6) {
                double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (5)];
                BestSells[BestSellsKey].Selected = false;
                if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                  MarketDetailVisible = false;
                }
              }
            } else if ((e.X > f20Width) && (e.X < f20Width + 14) && (e.Y > 2 * f20Height + 3 * f15Height) && (e.Y < 2 * f20Height + 3 * f15Height + 14)) {
              if (BestSells.Count >= 7) {
                double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (6)];
                BestSells[BestSellsKey].Selected = false;
                if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                  MarketDetailVisible = false;
                }
              }
            } else if ((e.X > f20Width * 2) && (e.X < f20Width * 2 + 14) && (e.Y > 2 * f20Height + 3 * f15Height) && (e.Y < 2 * f20Height + 3 * f15Height + 14)) {
              if (BestSells.Count >= 8) {
                double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (7)];
                BestSells[BestSellsKey].Selected = false;
                if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                  MarketDetailVisible = false;
                }
              }
            } else if ((e.X > f20Width * 3) && (e.X < f20Width * 3 + 14) && (e.Y > 2 * f20Height + 3 * f15Height) && (e.Y < 2 * f20Height + 3 * f15Height + 14)) {
              if (BestSells.Count >= 9) {
                double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (8)];
                BestSells[BestSellsKey].Selected = false;
                if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                  MarketDetailVisible = false;
                }
              }
            } else if ((e.X > f20Width * 4) && (e.X < f20Width * 4 + 14) && (e.Y > 2 * f20Height + 3 * f15Height) && (e.Y < 2 * f20Height + 3 * f15Height + 14)) {
              if (BestSells.Count >= 10) {
                double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (9)];
                BestSells[BestSellsKey].Selected = false;
                if (FocusedMarket == BestSells[BestSellsKey].CurPair) {
                  MarketDetailVisible = false;
                }
              }
            } else {
              if (!MarketDetailVisible) {
                MarketDetailVisible = true;
              }
              iRank = (((iRow - 11) * 5) + iCol / 2);
              if (BestSells.Count > iRank) {
                double BestSellsKey = BestSells.Keys[BestSells.Count - 1 - (iRank)];
                FocusedMarket = BestSells[BestSellsKey].CurPair;
              }
            }
            #endregion
          }

          if (iRow == 10) {
            if (MarketDetailVisible) {
              MarketDetailVisible = false;
              FocusedMarket = null;
            }
          }

        } catch { }
        #endregion
      }      
    }

    Boolean inEdMarkupFlag = false;
    private void edMarkup_ValueChanged(object sender, EventArgs e) {
      vSettings["edMarkup"] = edMarkup.Value.toStr8();
      if (!inEdMarkupFlag) {
        inEdMarkupFlag = true;
        try {
          if ((FocusedMarket != null) && (MarketsOfInterest.Keys.Contains(FocusedMarket))) {
            if ((MarketsOfInterest[FocusedMarket].AvgPricePaid > 0)) {
              BaseRates aBR = getBaseRates();
              if (FocusedMarket.BaseCurrency == "BTC") {
                edPrice.Value = Convert.ToDecimal(MarketsOfInterest[FocusedMarket].AvgPricePaid) * edMarkup.Value;
              } else if (FocusedMarket.BaseCurrency == "USDT") {
                edPrice.Value = Convert.ToDecimal(MarketsOfInterest[FocusedMarket].AvgPricePaid * aBR.USDBTCRate) * edMarkup.Value;
              } else if (FocusedMarket.BaseCurrency == "ETH") {
                edPrice.Value = Convert.ToDecimal(MarketsOfInterest[FocusedMarket].AvgPricePaid / aBR.BTCETHRate) * edMarkup.Value;
              } else if (FocusedMarket.BaseCurrency == "XMR") {
                edPrice.Value = Convert.ToDecimal(MarketsOfInterest[FocusedMarket].AvgPricePaid / aBR.BTCXMRRate) * edMarkup.Value;
              }          
            } else {
              edPrice.Value = Convert.ToDecimal(MarketsOfInterest[FocusedMarket].LastBuyPrice + 0.00000001);
            }
          }
        } catch { }
        inEdMarkupFlag = false;
      }
    }

    private void edChunkSize_ValueChanged(object sender, EventArgs e) {
      vSettings["edChunkSize"] = edChunkSize.Value.toStr8();
      if (FocusedMarket!=null){
        resetBuyBtnPrice(FocusedMarket);
      }
    }

    private void edWhaleDepth_ValueChanged(object sender, EventArgs e) {
      vSettings["edWhaleDepth"] = edWhaleDepth.Value.toStr8();
      if (FocusedMarket != null) {
        resetBuyBtnPrice(FocusedMarket);
      }      
    }

    private void edPrice_ValueChanged(object sender, EventArgs e) {
      btnSellM.Text = "S E L L " + Environment.NewLine + edPrice.Value.toStr8();
     // btnBuyM.Text = "B U Y " + Environment.NewLine + edPrice.Value.toStr8();
    }

    private void btnBuyM_Click(object sender, EventArgs e) {
      if (Balances.Keys.Contains(FocusedMarket.BaseCurrency)) {
        DoDisableControls();
        string s = btnBuyM.Text.ParseString(Environment.NewLine, 1);
        double dPrice = Convert.ToDouble(s);
        double dBaseVol = Convert.ToDouble(Balances[FocusedMarket.BaseCurrency].QuoteAvailable);
        if (Convert.ToDouble(edChunkSize.Value) < dBaseVol) {
          dBaseVol = Convert.ToDouble(edChunkSize.Value);
        }
        if (FocusedMarket.BaseCurrency == "BTC") {
          Dictionary<string, object> aCMDParams = new Dictionary<string, object>();
          aCMDParams["CurrencyPair"] = FocusedMarket;
          aCMDParams["OrderType"] = OrderType.Buy;
          aCMDParams["PricePerCoin"] = dPrice;
          aCMDParams["QuoteVolume"] = Convert.ToDouble(dBaseVol / dPrice);
          aCMDParams["DoRefresh"] = true;
          lPoloQue.Add(new appCmd("TradingPostOrder", aCMDParams));
        }
      }
    }

    private void btnSellM_Click(object sender, EventArgs e) {
      if (Balances.Keys.Contains(FocusedMarket.QuoteCurrency)) {
        DoDisableControls();
        string s = btnSellM.Text.ParseString(Environment.NewLine, 1);
        double dPrice = Convert.ToDouble(s);
        double dVol = Convert.ToDouble(Balances[FocusedMarket.QuoteCurrency].QuoteAvailable);
        double dTestVol = edChunkSize.Value.toDouble() / dPrice;
        if (dVol > dTestVol) {
          dVol = dTestVol;
        }
        if (dVol > 0) {
          Dictionary<string, object> aCMDParams = new Dictionary<string, object>();
          aCMDParams["CurrencyPair"] = FocusedMarket;
          aCMDParams["OrderType"] = OrderType.Sell;
          aCMDParams["PricePerCoin"] = dPrice;
          aCMDParams["QuoteVolume"] = dVol;
          aCMDParams["DoRefresh"] = true;
          lPoloQue.Add(new appCmd("TradingPostOrder", aCMDParams));
        }
      }
    }

    private void btnBuy_Click(object sender, EventArgs e) {
      if (Balances.Keys.Contains(FocusedMarket.BaseCurrency)) {
        DoDisableControls();
        string s = btnBuy.Text.ParseString(Environment.NewLine, 1);
        double dPrice = Convert.ToDouble(s);
        double dBTCVol = Convert.ToDouble(Balances[FocusedMarket.BaseCurrency].QuoteAvailable);
        if (Convert.ToDouble(edChunkSize.Value) < dBTCVol) {
          dBTCVol = Convert.ToDouble(edChunkSize.Value);
        }
        if (FocusedMarket.BaseCurrency == "BTC") {
          Dictionary<string, object> aCMDParams = new Dictionary<string, object>();
          aCMDParams["CurrencyPair"] = FocusedMarket;
          aCMDParams["OrderType"] = OrderType.Buy;
          aCMDParams["PricePerCoin"] = dPrice;
          aCMDParams["QuoteVolume"] = Convert.ToDouble(dBTCVol / dPrice);
          aCMDParams["DoRefresh"] = true;
          lPoloQue.Add(new appCmd("TradingPostOrder", aCMDParams));
        }
      }
    }

    private void btnSell_Click(object sender, EventArgs e) {
      if (Balances.Keys.Contains(FocusedMarket.QuoteCurrency)) {
        DoDisableControls();
        string s = btnSell.Text.ParseString(Environment.NewLine, 1);
        double dPrice = Convert.ToDouble(s);
        double dVol = Convert.ToDouble(Balances[FocusedMarket.QuoteCurrency].QuoteAvailable);
        double dTestVol = edChunkSize.Value.toDouble() / dPrice;
        if (dVol > dTestVol) {
          dVol = dTestVol;
        }
        if (dVol > 0) {
          Dictionary<string, object> aCMDParams = new Dictionary<string, object>();
          aCMDParams["CurrencyPair"] = FocusedMarket;
          aCMDParams["OrderType"] = OrderType.Sell;
          aCMDParams["PricePerCoin"] = dPrice;
          aCMDParams["QuoteVolume"] = dVol;
          aCMDParams["DoRefresh"] = true;
          lPoloQue.Add(new appCmd("TradingPostOrder", aCMDParams));
        }
      }
    }

    private void btnSellWhale_Click(object sender, EventArgs e) {
      if (Balances.Keys.Contains(FocusedMarket.QuoteCurrency)) {
        DoDisableControls();
        string s = btnSellWhale.Text.ParseString(Environment.NewLine, 1);
        double dPrice = Convert.ToDouble(s);
        double dVol = Convert.ToDouble(Balances[FocusedMarket.QuoteCurrency].QuoteAvailable);
        double dTestVol = edChunkSize.Value.toDouble() / dPrice;
        if (dVol > dTestVol) {
          dVol = dTestVol;
        }
        if (dVol > 0) {
          Dictionary<string, object> aCMDParams = new Dictionary<string, object>();
          aCMDParams["CurrencyPair"] = FocusedMarket;
          aCMDParams["OrderType"] = OrderType.Sell;
          aCMDParams["PricePerCoin"] = dPrice;
          aCMDParams["QuoteVolume"] = dVol;
          aCMDParams["DoRefresh"] = true;
          lPoloQue.Add(new appCmd("TradingPostOrder", aCMDParams));
        }
      }
    }

    private void btnCancel_Click(object sender, EventArgs e) {
      DoDisableControls();
      if ((MarketsOfInterest.Keys.Contains(FocusedMarket))&&(MarketsOfInterest[FocusedMarket].tradeOpenOrders.Count>0)){
        List<Jojatekok.PoloniexAPI.TradingTools.IOrder> chkOO = MarketsOfInterest[FocusedMarket].tradeOpenOrders;
        Dictionary<string, object> aCMDParams = new Dictionary<string, object>();
        aCMDParams["CurrencyPair"] = FocusedMarket;
        aCMDParams["DoRefresh"] = true;
        foreach (Jojatekok.PoloniexAPI.TradingTools.IOrder iO in chkOO) { 
          aCMDParams["OrderNum"] = iO.IdOrder;
          aCMDParams["IsMove"] = false;
          lPoloQue.ChkAdd(new appCmd("TradingDeleteOrder", aCMDParams));
        }
      }
    }

    private void cbOnlyBTC_CheckedChanged(object sender, EventArgs e) {
      if (cbOnlyBTC.Checked) {
        vSettings["OnlyBTC"] = "True";
      } else { 
        vSettings["OnlyBTC"] = "False";
      }      
    }

    /*
    private void tbRunMode_ValueChanged(object sender, EventArgs e) {
      if (tbRunMode.Value == 0) {
        this.Text = "Advisor -- " + AppUtils.LogFileName("Advisor");
      } 
      if (tbRunMode.Value == 1) {
        this.Text = "Advisor -- Running Top 1 Algo ";        
      }
      if (tbRunMode.Value == 2) {
        this.Text = "Advisor -- Running Shift Algo ";        
      }
      if (tbRunMode.Value == 3) {
        this.Text = "Advisor -- Running Sell off ";
      } 
    }  */
    #endregion 

    #region Logging and Error Handeling 
    private void LogException(String sExceptionTag, Exception e) {
      try {
        String sMessage = e.Message;
        String sSource = e.Source;
        String sStack = e.StackTrace;
        ("*** Error(" + sExceptionTag + "): " + sMessage + "; Source: " + sSource + "; Stack: " + sStack).toLog("AdvisorLog");
        if (e.InnerException != null) {
          LogException(sExceptionTag + "A", e.InnerException);
        }
      } catch { }
    }

    private void LogDetail(String sDetailTag, String sDetail) {
      try {
        (" Detail(" + sDetailTag + "): " + sDetail).toLog("AdvisorLog");
      } catch { }
    }
    #endregion 

    private void edHoldAmount_ValueChanged(object sender, EventArgs e) {
      if ((FocusedMarket != null)&&(MarketsOfInterest.Keys.Contains(FocusedMarket))&&(!FocusedMarketChanging)) {
        MarketsOfInterest[FocusedMarket].mv["HoldAmount"] = edHoldAmount.Value.toStr8();
        MarketsOfInterest[FocusedMarket].HoldAmount = edHoldAmount.Value;
      }
    }

    private void edSellThresh_ValueChanged(object sender, EventArgs e) {
      if ((FocusedMarket != null) && (MarketsOfInterest.Keys.Contains(FocusedMarket)) && (!FocusedMarketChanging)) {
        MarketsOfInterest[FocusedMarket].mv["SellThresh"] = edSellThresh.Value.toStr8();
        MarketsOfInterest[FocusedMarket].SellThreshold = edSellThresh.Value;
      }
    }

    private void edBuyThresh_ValueChanged(object sender, EventArgs e) {
      if ((FocusedMarket != null) && (MarketsOfInterest.Keys.Contains(FocusedMarket)) && (!FocusedMarketChanging)) {
        MarketsOfInterest[FocusedMarket].mv["BuyThresh"] = edBuyThresh.Value.toStr8();
        MarketsOfInterest[FocusedMarket].BuyThreshold = edBuyThresh.Value;
      }
    }

    private void cbGoHold_CheckedChanged(object sender, EventArgs e) {
      if ((FocusedMarket != null) && (MarketsOfInterest.Keys.Contains(FocusedMarket)) && (!FocusedMarketChanging)) {
        if (cbGoHold.Checked) {
          MarketsOfInterest[FocusedMarket].mv["GoHold"] = "True";
          MarketsOfInterest[FocusedMarket].GoHold = true;
        } else {
          MarketsOfInterest[FocusedMarket].mv["GoHold"] = "False";
          MarketsOfInterest[FocusedMarket].GoHold = false;
        }
      } 
    }

    private void cbTradeGo_CheckedChanged(object sender, EventArgs e) {

    }


  }


}
