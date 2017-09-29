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
  partial class Form1 {

    #region Live Ticker Processing
    private void TakeLiveUp() {
      LogDetail("TakeLiveUp", "");
      pc.Live.Start();
      pc.Live.SubscribeToTickerAsync();    
      tReadWorker.Enabled = true;
      feedLive = true;
      if (bInStartup) {
        bInStartup = false;
        if (!cbOnlyBTC.Visible) { cbOnlyBTC.Visible = true; }        
        if (!edChunkSize.Visible) { edChunkSize.Visible = true; }
      }
    }
    private void TakeLiveDown() {
      feedLive = false;
      tReadWorker.Enabled = false;
      LogDetail("TakeLiveDown", "");
      pc.Live.Stop();
    }

    void Live_OnTickerChanged(object sender, TickerChangedEventArgs e) {
      feedQueue.Add(e);
      feedTickNo++;
    }

    private void tReadWorker_Tick(object sender, EventArgs e) {
      if (tReadWorker.Enabled) {
        tReadWorker.Enabled = false;
      }
      try {

        while (feedQueue.Count > 0) {
          if (feedQueue.Count > 0) {
            TickerChangedEventArgs x = feedQueue[0];
            if (MarketsOfInterest.Keys.Contains(x.CurrencyPair)) {
              MarketsOfInterest[x.CurrencyPair].setUpdatedMarketData(x.MarketData);
            }
            feedQueue.RemoveAt(0);
          }
        }       

      } finally {
        if (feedLive) {
          tReadWorker.Enabled = true;
        }
      }
    }
    #endregion 

    #region Polo API Requests and Polo Queue processing

    private void tPoloCmd_Tick(object sender, EventArgs e) {
      try {
        //if (!cbPause.Checked) {
        if (tPoloCmd.Enabled) tPoloCmd.Enabled = false;
        if (lPoloQue.Count() > 0) {
          sPoloCmdInProgress = lPoloQue.Pop();
          ProcessPoloMsg(sPoloCmdInProgress);
        }
        if (lPoloQue.Count() == 0) {
          sPoloCmdInProgress = null;
        }
        //  cbPause.Checked = true;
        //}
      } catch (Exception e01) {
        LogException("PoloTimeTic", e01);
      }
      tPoloCmd.Enabled = bPoloQueOn;
    }
    private void ProcessPoloMsg(appCmd aCmd) {

      switch (aCmd.Cmd) {

        case "MarketsGetSummary":
          DoMarketsGetSummary();
          break;
        case "MarketsGetOpenOrders":
          DoMarketsGetOpenOrders(aCmd.Params);
          break;
        case "MarketsGetTrades":
          DoMarketsGetTrades(aCmd.Params);
          break;
        case "TradingGetTrades":
          DoTradingGetTrades(aCmd.Params);
          break;
        case "TradingGetOpenOrders":
          DoTradingGetOpenOrders(aCmd.Params);
          break;
        case "TradingPostOrder":
          DoTradingPostOrder(aCmd.Params);
          break;
        case "TradingDeleteOrder":
          DoTradingDeleteOrder(aCmd.Params);
          break;
        case "GetBalances":
          DoWalletGetBalances();
          break;
        case "StartupTicker":
          TakeLiveUp();
          break;
        case "DisableControls":
          DoDisableControls();
          break;
        case "EnableControls":
          DoEnableControls();
          break;
      }
    }                  // Primary Process Messages on cmd exec  

    private void DoWalletGetBalances() {
      try {
        sWaitDesc = "Loading Balances";
        LogDetail("GetBalances", "");

        var xTask = pc.Wallet.GetBalancesAsync();
        lastBalance = xTask.Result;
        lBotQue.ChkAdd(new appCmd("ProcessBalances", null));
      } catch (Exception e00) {
        LogException("dwgb0", e00);
      }
    }                   //WalletGetBalances  

    private void DoMarketsGetTrades(Dictionary<string, object> msgParams) {
      try {
        sWaitDesc = "Loading Market History";
        CurrencyPair cp = (CurrencyPair)msgParams["CurrencyPair"];
        LogDetail("MarketsGetTrades", cp.ToString());
        var xTask = pc.Markets.GetTradesAsync(cp);
        MarketsOfInterest[cp].lastTradeHistoryMarket = xTask.Result;
        lBotQue.ChkAdd(new appCmd("ProcessMarketTrades", msgParams));
      } catch (Exception e01) {
        LogException("dmgt0", e01);
      }
    }

    private void DoMarketsGetSummary() {
      try {
        sWaitDesc = "Loading Market Summary";
        LogDetail("MarketsGetSummary", "");
        var xTask = pc.Markets.GetSummaryAsync();
        lastMarket = xTask.Result;
        lastSummaryCheck = DateTime.Now;
        lBotQue.ChkAdd(new appCmd("ProcessMarketSummary", null));
      } catch (Exception e01) {
        LogException("dmgs0", e01);
      }
    }                        // MarketsGetSummary  

    private void DoTradingGetOpenOrders(Dictionary<string, object> msgParams) {
      try {
        sWaitDesc = "Loading Open Orders";
        CurrencyPair cp = (CurrencyPair)msgParams["CurrencyPair"];
        LogDetail("TradingGetOpenOrders", cp.ToString());
        var xTask = pc.Trading.GetOpenOrdersAsync(cp);
        MarketsOfInterest[cp].lastTradeOpenOrders = xTask.Result;
        lBotQue.ChkAdd(new appCmd("ProcessOpenOrders", msgParams));
      } catch (Exception e02) {
        LogException("dtgoo", e02);
      }
    }

    private void DoTradingPostOrder(Dictionary<string, object> msgParams) {
      try {
        CurrencyPair cp = (CurrencyPair)msgParams["CurrencyPair"];
        OrderType aType = (OrderType)msgParams["OrderType"];
        double aPPC = (double)msgParams["PricePerCoin"];
        double aQV = (double)msgParams["QuoteVolume"];
        Boolean bDoRefresh = (Boolean)msgParams["DoRefresh"];

        var xTask = pc.Trading.PostOrderAsync(cp, aType, aPPC, aQV);
        ulong OrderNum = xTask.Result;

        LogDetail("Order-" + Convert.ToString(OrderNum), cp.ToString() + 
           aType.ToString() + "Price:" + aPPC.toStr8() + " vol:" + aQV.toStr8());

        if (aType == OrderType.Buy) {
          MarketsOfInterest[cp].LastOrderBuyNum = OrderNum;
          MarketsOfInterest[cp].LastOrderBuyPrice = aPPC;
          MarketsOfInterest[cp].LastOrderBuyVol = aQV;
        } else {
          MarketsOfInterest[cp].LastOrderSellNum = OrderNum;
          MarketsOfInterest[cp].LastOrderSellPrice = aPPC;
          MarketsOfInterest[cp].LastOrderSellVol = aQV;
        }

        MarketsOfInterest[cp].tradeOpenOrders.Insert(0, 
          new Jojatekok.PoloniexAPI.TradingTools.Order(OrderNum, aType, aPPC, aQV, aPPC * aQV));

        if (bDoRefresh) {
          lBotQue.Add(new appCmd("RefreshMarketDetails", msgParams));
        }

      } catch (Exception e01) {
        LogException("dtpo", e01);
      }
    }

    private void DoTradingGetTrades(Dictionary<string, object> msgParams) {
      bInWait = true;
      sWaitDesc = "Processing Trade Hisory";
      try {
        CurrencyPair cp = (CurrencyPair)msgParams["CurrencyPair"];
        DateTime dtFrom = DateTime.Now;
        Boolean GoShortenResults = false;
        if (MarketsOfInterest[cp].tradeHistoryTrades.Count > 0) {
          dtFrom = MarketsOfInterest[cp].tradeHistoryTrades[0].Time;
          GoShortenResults = true;
        }
        if (GoShortenResults == true) {
          LogDetail("TradingGetTrades", "Short "+dtFrom.toStrDate()+" "+ cp.ToString());
          var xTask = pc.Trading.GetTradesAsync(cp, dtFrom.ToUniversalTime(), DateTime.Now.ToUniversalTime());
          MarketsOfInterest[cp].lastTradeHistoryTrades = xTask.Result;
        } else {
          LogDetail("TradingGetTrades", "Full "+cp.ToString());
          var xTask = pc.Trading.GetTradesAsync(cp);
          MarketsOfInterest[cp].lastTradeHistoryTrades = xTask.Result;
        }
        lBotQue.ChkAdd(new appCmd("ProcessTradeTrades", msgParams));
      } catch (Exception e01) {
        LogException("dtgt0", e01);
      }
    }                     // Trading Get Trades

    private void DoMarketsGetOpenOrders(Dictionary<string, object> msgParams) {
      try {
        sWaitDesc = "Loading Open Orders";
        CurrencyPair cp = (CurrencyPair)msgParams["CurrencyPair"];
        LogDetail("MarketsGetOpenOrders", cp.ToString());
        var xTask = pc.Markets.GetOpenOrdersAsync(cp);
        MarketsOfInterest[cp].LastOrderBook = xTask.Result;
        lBotQue.ChkAdd(new appCmd("ProcessMarketGetOpenOrders", msgParams));
      } catch (Exception e01) {
        LogException("dmgoo0", e01);
      }
    }

    private void DoTradingDeleteOrder(Dictionary<string, object> msgParams) {
      try {
        sWaitDesc = "Canceling Order";
        CurrencyPair cp = (CurrencyPair)msgParams["CurrencyPair"];
        ulong OrderNum = (ulong)msgParams["OrderNum"];
        Boolean isMoveOrder = (Boolean)msgParams["IsMove"];
        Boolean bDoRefresh = (Boolean)msgParams["DoRefresh"];
        var xTask = pc.Trading.DeleteOrderAsync(cp, OrderNum);
        bool success = xTask.Result;
        LogDetail("CancelOrder-" + Convert.ToString(OrderNum), cp.ToString() + " Status: " + (success ? "success" : "failed"));
        if (success) {          
          if (MarketsOfInterest[cp].tradeOpenOrders.Count > 0) {
            List<TT.IOrder> tempOrders = MarketsOfInterest[cp].tradeOpenOrders;
            foreach (TT.IOrder xT in tempOrders) {
              if (xT.IdOrder == OrderNum) {
                MarketsOfInterest[cp].tradeOpenOrders.Remove(xT);
              }
            }
          }
          if (MarketsOfInterest[cp].LastOrderBuyNum == OrderNum) {
            MarketsOfInterest[cp].LastOrderBuyNum = 0;
          }
          if (MarketsOfInterest[cp].LastOrderSellNum == OrderNum) {
            MarketsOfInterest[cp].LastOrderSellNum = 0;
          }
        }
        if (bDoRefresh) {
          lBotQue.Add(new appCmd("RefreshMarketDetails", msgParams));
        }
      } catch (Exception e01) {
        LogException("dtdo", e01);
      }
    }

    public void DoDisableControls() {
      bInWait = true;
      sWaitDesc = "Refreshing Market Data";
      LogDetail("DisableControls", "");
      if (RefreshWait == null) {
        RefreshWait = new appRefreshWait(DoAddEnable);
      }
      if (MarketDetailVisible) {
        if (!btnBuy.Visible) { btnBuy.Visible = true; }
        if (!btnSell.Visible) { btnSell.Visible = true; }
        if (!btnCancel.Visible) { btnCancel.Visible = true; }
        if (!edPrice.Visible) { edPrice.Visible = true; }
        if (!edMarkup.Visible) { edMarkup.Visible = true; }
        if (!btnBuyM.Visible) { btnBuyM.Visible = true; }
        if (!btnSellM.Visible) { btnSellM.Visible = true; }
        if (!edWhaleDepth.Visible) { edWhaleDepth.Visible = true; }

        if (btnCancel.Enabled) { btnCancel.Enabled = false; }
        if (btnSell.Enabled) { btnSell.Enabled = false; }
        if (btnSellM.Enabled) { btnSellM.Enabled = false; }
        if (btnBuy.Enabled) { btnBuy.Enabled = false; }
        if (btnBuyM.Enabled) { btnBuyM.Enabled = false; }
      } else {
        if (btnBuy.Visible) { btnBuy.Visible = false; }
        if (btnSell.Visible) { btnSell.Visible = false; }
        if (btnCancel.Visible) { btnCancel.Visible = false; }
        if (edPrice.Visible) { edPrice.Visible = false; }
        if (edMarkup.Visible) { edMarkup.Visible = false; }
        if (btnBuyM.Visible) { btnBuyM.Visible = false; }
        if (btnSellM.Visible) { btnSellM.Visible = false; }
        if (edWhaleDepth.Visible) { edWhaleDepth.Visible = false; }
      }
    }

    public void DoEnableControls() {
      bInWait = false;
      LogDetail("EnableControls", "");
      if (MarketDetailVisible) {
        if (!btnBuy.Visible) { btnBuy.Visible = true; }
        if (!btnSell.Visible) { btnSell.Visible = true; }
        if (!btnCancel.Visible) { btnCancel.Visible = true; }
        if (!edPrice.Visible) { edPrice.Visible = true; }
        if (!edMarkup.Visible) { edMarkup.Visible = true; }
        if (!btnBuyM.Visible) { btnBuyM.Visible = true; }
        if (!btnSellM.Visible) { btnSellM.Visible = true; }
        if (!edWhaleDepth.Visible) { edWhaleDepth.Visible = true; }

        if ((FocusedMarket != null) && (MarketsOfInterest.Keys.Contains(FocusedMarket) && (MarketsOfInterest[FocusedMarket].tradeOpenOrders.Count > 0))) {
          if (!btnCancel.Enabled) { btnCancel.Enabled = true; }
        } else {
          if (btnCancel.Enabled) { btnCancel.Enabled = false; }
        }

        if (FocusedMarket != null) {
          if ((Balances.Keys.Contains(FocusedMarket.QuoteCurrency)) && (Balances[FocusedMarket.QuoteCurrency].QuoteAvailable > 0.0005)) {
            btnSell.Enabled = true;
            btnSellM.Enabled = true;
          } else {
            if (btnSell.Enabled) { btnSell.Enabled = false; }
            if (btnSellM.Enabled) { btnSellM.Enabled = false; }
          }
          if ((Balances.Keys.Contains(FocusedMarket.BaseCurrency)) && (Balances[FocusedMarket.BaseCurrency].QuoteAvailable > 0.0005)) {
            btnBuy.Enabled = true;
            btnBuyM.Enabled = true;
          } else {
            if (btnBuy.Enabled) { btnBuy.Enabled = false; }
            if (btnBuyM.Enabled) { btnBuyM.Enabled = false; }
          }
        }
      } else {
        if (btnBuy.Visible) { btnBuy.Visible = false; }
        if (btnSell.Visible) { btnSell.Visible = false; }
        if (btnCancel.Visible) { btnCancel.Visible = false; }
        if (edPrice.Visible) { edPrice.Visible = false; }
        if (edMarkup.Visible) { edMarkup.Visible = false; }
        if (btnBuyM.Visible) { btnBuyM.Visible = false; }
        if (btnSellM.Visible) { btnSellM.Visible = false; }
        if (edWhaleDepth.Visible) { edWhaleDepth.Visible = false; }
      }
    }

    public void DoAddEnable() {
      lPoloQue.ChkAdd(new appCmd("EnableControls", null));
    }

    #endregion

  }
}
