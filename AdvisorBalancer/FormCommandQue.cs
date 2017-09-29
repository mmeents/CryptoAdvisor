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
    
    #region App Processing Queue and process handlers

    private void tBotCmd_Tick(object sender, EventArgs e) {
      try {
        if (tBotCmd.Enabled) tBotCmd.Enabled = false;
        if (lBotQue.Count() > 0) {
          sBotCmdInProgress = lBotQue.Pop();
          ProcessBotCmd(sBotCmdInProgress);
        }
        if (lBotQue.Count() == 0) {
          sBotCmdInProgress = null;
        }
      } catch (Exception e01) {
        LogException("tbct", e01);
      }
      tBotCmd.Enabled = bBotQueOn;
    }

    private void ProcessBotCmd(appCmd aCmd) {
      string sCmd = aCmd.Cmd;
      string sLogDetail = (aCmd.Params == null ? "" : (aCmd.hasParam("CurrencyPair") ? " " + (((CurrencyPair)aCmd.Params["CurrencyPair"]).ToString()) : ""));
      try {

        switch (sCmd) {
          case "BootBot":
            lPoloQue.Insert(0, new appCmd("StartupTicker", null));            
            lPoloQue.Insert(0, new appCmd("GetBalances", null));
            lPoloQue.Insert(0, new appCmd("MarketsGetSummary", null));
            break;

          case "RefreshMarketDetails":
            //lPoloQue.ChkAdd(new appCmd("DisableControls", null));
            lPoloQue.ChkAdd(new appCmd("MarketsGetOpenOrders", aCmd.Params));
            lPoloQue.ChkAdd(new appCmd("MarketsGetTrades", aCmd.Params));
            lPoloQue.ChkAdd(new appCmd("TradingGetOpenOrders", aCmd.Params));
            lPoloQue.ChkAdd(new appCmd("GetBalances", null));
            lPoloQue.ChkAdd(new appCmd("TradingGetTrades", aCmd.Params));
            break;
          case "TrackingMarketTick":
            //        doProcessTrackingMarketTic(aCmd);
            break;
          case "ProcessResetAvgBal":
            //        doProcessResetAvgBal(aCmd);
            break;
          case "ProcessBalances":
            doProcessBalances();
            break;
          case "ProcessMarketSummary":
            doProcessMarketSummary();
            break;
          case "ProcessMarketTrades":
            doProcessMarketTrades(aCmd);
            break;
          case "ProcessMarketGetOpenOrders":
            doProcessMarketGetOpenOrders(aCmd);
            break;
          case "ProcessTradeTrades":
            doProcessTradeTrades(aCmd);
            break;
          case "ProcessOpenOrders":
            doProcessOpenOrders(aCmd);
            break;
          case "":
            break;
        }

      } catch (Exception e00) {
        LogException("PBC" + sCmd, e00);
      }
    }                 // Bot Process Command.

    private void doProcessMarketSummary() {
      bInWait = true;
      sWaitDesc = "Loading Markets";
      try {
        if (lastMarket != null) {
          IDictionary<CurrencyPair, IMarketData> its = lastMarket;
          foreach (CurrencyPair lm in its.Keys) {
            if (MarketsOfInterest.Keys.Contains(lm)) {
              MarketsOfInterest[lm].setUpdatedMarketData(its[lm]);
            } else {
              sWaitDesc = "Loading Market " + lm.ToString();
              appMarket x = new appMarket(lm, its[lm], this);
              MarketsOfInterest[lm] = x;
              x.LoadTradeHistory();
            }
          }
       //   EnsureEachBalanceHasMarket();
       //   foreach (CurrencyPair acp in MarketsOfInterest.Keys) {
       //     if (!lCurList.Contains(acp.QuoteCurrency)) {
       //       lCurList.Add(acp.QuoteCurrency.ToLower());
       //     }
       //   }
        }
      } catch (Exception ee) {
        LogException("doPMS", ee);
      } finally {
        bInWait = false;
      }
    }

    private void doProcessMarketTrades(appCmd aCmd) {
      try {
        CurrencyPair cp = (CurrencyPair)aCmd.Params["CurrencyPair"];
        IList<Jojatekok.PoloniexAPI.MarketTools.ITrade> theList = MarketsOfInterest[cp].lastTradeHistoryMarket;
        List<Jojatekok.PoloniexAPI.MarketTools.ITrade> TempList = new List<Jojatekok.PoloniexAPI.MarketTools.ITrade>();
        foreach (Jojatekok.PoloniexAPI.MarketTools.ITrade xT in theList) {
          if (!MarketsOfInterest[cp].tradeHistoryMarket.Contains(xT)) {
            TempList.Insert(0, xT);
          }
        }
        foreach (Jojatekok.PoloniexAPI.MarketTools.ITrade xT in TempList) {
          MarketsOfInterest[cp].tradeHistoryMarket.Insert(0, xT);
        }
        MarketsOfInterest[cp].UpdateTradeHistory();
      } catch (Exception e00) {
        LogException("PBMT", e00);
      }
      if (RefreshWait != null) {
        RefreshWait.WaitMarketsGetTrades = true;
      }
    }

    private void EnsureEachBalanceHasMarket() {      
      foreach (string sQuoteCur in Balances.Keys) {
        Boolean hasOneSelection = false;
        foreach (CurrencyPair cp in MarketsOfInterest.Keys) {
          if (cp.QuoteCurrency == sQuoteCur) {
            if (MarketsOfInterest[cp].Selected) {
              hasOneSelection = true;
              break;
            }
          }
        }
        if (!hasOneSelection) {
          foreach (CurrencyPair cp in MarketsOfInterest.Keys) {
            if (cp.QuoteCurrency == sQuoteCur) {
              MarketsOfInterest[cp].Selected = true;
              calculateAvgPricePaid(cp);
              break;
            }
          }
        }
      }
    }

    private void doProcessBalances() {
      try {
        if (lastBalance != null) {
          #region Sync Balances with Database
          IDictionary<string, IBalance> enlastBalance = lastBalance;
          IDictionary<string, appBalance> enBalances = Balances;
          double dTotalBTCValue = 0;
          BaseRates aBR = getBaseRates();
          foreach (string skey in enlastBalance.Keys) {   // add any new balances to Balances list if not already there.  
            if ((enlastBalance[skey].BitcoinValue > 0.0005) || ((skey == "USDT") && (enlastBalance[skey].QuoteAvailable > 0.5))) {
              if (!enBalances.Keys.Contains(skey)) {
                appBalance aAB = new appBalance(skey, enlastBalance[skey], MakerFee, TakerFee,aBR);
                Balances[skey] = aAB;
                Balances[skey].ShardMngr.VolumeToAccountFor = aAB.DBQuote;
              } else {
                Balances[skey].setLastBalance(enlastBalance[skey], skey, aBR);
              }
              if (skey == "USDT") {                
                dTotalBTCValue = dTotalBTCValue + enlastBalance[skey].QuoteAvailable / ((aBR.USDBTCRate !=0)? aBR.USDBTCRate:2500);
              } else {
                dTotalBTCValue = dTotalBTCValue + enlastBalance[skey].BitcoinValue;
              }
            }
          }

          foreach (string sQuoteCur in Balances.Keys) {
            if ((!enlastBalance.Keys.Contains(sQuoteCur)) || (enlastBalance[sQuoteCur].BitcoinValue < 0.0005)) {
              Balances[sQuoteCur].ShardMngr.Shards.Clear();
              Balances[sQuoteCur].ShardMngr.WriteShards();
              Balances.Remove(sQuoteCur);
            } else {
              Balances[sQuoteCur].PercentOfTotal = Balances[sQuoteCur].BitcoinValue / (dTotalBTCValue == 0 ? 1 : dTotalBTCValue);
              EnsureEachBalanceHasMarket();
            }
          }

         // foreach (string sQuoteCur in Balances.Keys) {
         //   if ((Balances[sQuoteCur].ShardMngr.Shards.Count==0) || (Balances[sQuoteCur].ShardMngr.QuoteVolume() < Balances[sQuoteCur].DBQuote)) {
         //     foreach (CurrencyPair cp in MarketsOfInterest.Keys) {
         //       if ((cp.QuoteCurrency == sQuoteCur)&&(MarketsOfInterest[cp].Selected)) {
         //         //Balances[sQuoteCur].ShardMngr.LoadShardsFromTradeHistory( )
         //         break;
         //       }
         //     }
         //   }
         // }

          TotalBTCValue = dTotalBTCValue;
          TotalUSDValue = dTotalBTCValue * BTCRate;
          #endregion
        }

      } catch (Exception e00) {
        LogException("PBA", e00);
      }

      if (RefreshWait != null) {
        RefreshWait.WaitGetBalances = true;
      }
    }

    private void resetBuyBtnPrice(CurrencyPair aMarket) {
      if (btnBuyM.Visible) {
        double aD = edWhaleDepth.Value.toDouble();
        IList<Jojatekok.PoloniexAPI.MarketTools.IOrder> xBO = MarketsOfInterest[aMarket].LastOrderBook.BuyOrders;
        double aSum = 0, aLastPrice = 0;
        foreach (Jojatekok.PoloniexAPI.MarketTools.IOrder iO in xBO) {
          if (aSum + iO.AmountBase > aD) {
            aLastPrice = iO.PricePerCoin + 0.00000001;
            break;
          } else {
            aSum = aSum + iO.AmountBase;
          }
        }
        if (aLastPrice > 0) {
          btnBuyM.Text = "B U Y" + Environment.NewLine + aLastPrice.toStr8();
          MarketsOfInterest[aMarket].LastWhaleRiderPriceBuy = aLastPrice;
        }
      }
      if (btnSellWhale.Visible) {
        double aD = edWhaleDepth.Value.toDouble();
        IList<Jojatekok.PoloniexAPI.MarketTools.IOrder> xBO = MarketsOfInterest[aMarket].LastOrderBook.SellOrders;
        double aSum = 0, aLastPrice = 0;
        foreach (Jojatekok.PoloniexAPI.MarketTools.IOrder iO in xBO) {
          if (aSum + iO.AmountBase > aD) {
            aLastPrice = iO.PricePerCoin - 0.00000001;
            break;
          } else {
            aSum = aSum + iO.AmountBase;
          }
        }
        if (aLastPrice > 0) {
          btnSellWhale.Text = "S E L L" + Environment.NewLine + aLastPrice.toStr8();
          MarketsOfInterest[aMarket].NextSellWhalePrice = aLastPrice;
        }
      }
    }

    private void doProcessMarketGetOpenOrders(appCmd aCmd) {
      try {
        CurrencyPair cp = (CurrencyPair)aCmd.Params["CurrencyPair"];
        //double QuoteVol = 0;
        if (MarketsOfInterest[cp].LastOrderBook != null) {
          //if( Balances.Keys.Contains( cp.QuoteCurrency)){
          //  QuoteVol = Balances[cp.QuoteCurrency].DBQuote;
          //}
          resetBuyBtnPrice(cp);

        } else {
          LogDetail("PMGOOn", "Last Market Open Order Book Null");
        }

      } catch (Exception e00) {
        LogException("PMGOO", e00);
      }
      if (RefreshWait != null) {
        RefreshWait.WaitMarketsGetOpenOrders = true;
      }
    }

    private void doProcessTradeTrades(appCmd aCmd) {
      try {
        Boolean skipCalAvg = false;
        CurrencyPair cp = (CurrencyPair)aCmd.Params["CurrencyPair"];
        IList<Jojatekok.PoloniexAPI.TradingTools.ITrade> xTrades = MarketsOfInterest[cp].lastTradeHistoryTrades;
        IList<Jojatekok.PoloniexAPI.TradingTools.ITrade> lMOI = MarketsOfInterest[cp].tradeHistoryTrades;
        if (xTrades != null) {
          List<Jojatekok.PoloniexAPI.TradingTools.ITrade> TempList = new List<Jojatekok.PoloniexAPI.TradingTools.ITrade>();
          Int32 iX = 0; Int32 iListSize = xTrades.Count;
          foreach (Jojatekok.PoloniexAPI.TradingTools.ITrade xT in xTrades) {
            iX++;
            Boolean bIsInList = false;
            foreach (Jojatekok.PoloniexAPI.TradingTools.ITrade cT in lMOI) {
              if ((cT.IdOrder == xT.IdOrder) &&
                  (cT.Time.toStrDateTime() == xT.Time.toStrDateTime()) &&
                  (cT.PricePerCoin.toStr8() == xT.PricePerCoin.toStr8()) &&
                  (cT.AmountQuote.toStr8() == xT.AmountQuote.toStr8())
                 ) {
                bIsInList = true;
                break;
              }
            }
            if (!bIsInList) {
              TempList.Insert(0, xT);
            }
            if (iX % 13 == 12) {
              sWaitDesc = "Processing " + Convert.ToDouble(iX / iListSize).toStr2() + "% complete";
            }
            Application.DoEvents();
          }

          BaseRates aBR = getBaseRates();          
          foreach (Jojatekok.PoloniexAPI.TradingTools.ITrade xT in TempList) {
            MarketsOfInterest[cp].tradeHistoryTrades.Insert(0, xT);
            xT.toDataLine().toDataFile(cp);
            if (Balances.Keys.Contains(cp.QuoteCurrency)) {
              MarketsOfInterest[cp].ResetShards();  // Balances[cp.QuoteCurrency].AddTrade(aBR, cp, xT);
              skipCalAvg = true;
            }
          }
          //if (MarketsOfInterest[cp].tradeHistoryTrades.Count > 1000) {
          //  while (MarketsOfInterest[cp].tradeHistoryTrades.Count > 1000) {
          //    Int32 iLast = MarketsOfInterest[cp].tradeHistoryTrades.Count - 1;
          //    MarketsOfInterest[cp].tradeHistoryTrades.RemoveAt(iLast);
          //  }
          //  MarketsOfInterest[cp].WriteTradeHistory();
          //}
        }
        if (!skipCalAvg) {
          calculateAvgPricePaid(cp);
        }

      } catch (Exception e00) {
        LogException("PBTT", e00);
      }
      if (RefreshWait != null) {
        RefreshWait.WaitTradingGetTrades = true;
      }
      bInWait = false;
    }

    public BaseRates getBaseRates() {
      BaseRates aBR = new BaseRates();
      aBR.USDBTCRate = 0;
      CurrencyPair acp = new CurrencyPair("USDT", "BTC");
      if (MarketsOfInterest.Keys.Contains(acp)) {
        aBR.USDBTCRate = MarketsOfInterest[acp].LastMarketData.PriceLast;
      }
      acp = new CurrencyPair("BTC", "ETH");
      if (MarketsOfInterest.Keys.Contains(acp)) {
        aBR.BTCETHRate = MarketsOfInterest[acp].LastMarketData.PriceLast;
      }
      acp = new CurrencyPair("BTC", "XMR");
      if (MarketsOfInterest.Keys.Contains(acp)) {
        aBR.BTCXMRRate = MarketsOfInterest[acp].LastMarketData.PriceLast;
      }
      return aBR;
    }

    public void calculateAvgPricePaid(CurrencyPair cp) {

      if (Balances.Keys.Contains(cp.QuoteCurrency)) {
        double Coin = Balances[cp.QuoteCurrency].DBQuote;
        double aAvg = Balances[cp.QuoteCurrency].ShardMngr.BTCAvgPricePaid(Coin);
        Balances[cp.QuoteCurrency].CostEstBTC = aAvg * Coin;
        MarketsOfInterest[cp].AvgPricePaid = aAvg;
        if (MarketDetailVisible && (FocusedMarket == cp)) {
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
        }
      } else {
        MarketsOfInterest[cp].AvgPricePaid = 0;
      }

    }

    private void doProcessOpenOrders(appCmd aCmd) {
      CurrencyPair cp = (CurrencyPair)aCmd.Params["CurrencyPair"];
      try {       

        IList<Jojatekok.PoloniexAPI.TradingTools.IOrder> xOrders = MarketsOfInterest[cp].lastTradeOpenOrders;
        if (xOrders != null) {
          List<Jojatekok.PoloniexAPI.TradingTools.IOrder> TempOrders = new List<Jojatekok.PoloniexAPI.TradingTools.IOrder>();
          foreach (Jojatekok.PoloniexAPI.TradingTools.IOrder xTOO in MarketsOfInterest[cp].tradeOpenOrders) {
            Boolean orderFound = false;
            foreach (Jojatekok.PoloniexAPI.TradingTools.IOrder xO in xOrders) {
              if (xO.IdOrder == xTOO.IdOrder) {
                orderFound = true;
                break;
              }
            }
            if (!orderFound) {
              TempOrders.Insert(0, xTOO);
              if (xTOO.IdOrder == MarketsOfInterest[cp].LastOrderBuyNum) {
                MarketsOfInterest[cp].LastOrderBuyNum = 0;
              }
              if (xTOO.IdOrder == MarketsOfInterest[cp].LastOrderSellNum) {
                MarketsOfInterest[cp].LastOrderSellNum = 0;
              }
            }
          }

          MarketsOfInterest[cp].tradeOpenOrders.Clear();
          foreach (Jojatekok.PoloniexAPI.TradingTools.IOrder xO in xOrders) {
            MarketsOfInterest[cp].tradeOpenOrders.Insert(0, xO);
          }
        }

        if (MarketsOfInterest[cp].NeedsTradeOrderRefresh) {
          MarketsOfInterest[cp].NeedsTradeOrderRefresh = false;
        }

      } catch (Exception e00) {
        LogException("PBOO", e00);
      }
      if (RefreshWait != null) {
        RefreshWait.WaitTradingGetOpenOrders = true;
      }
    }
    
    #endregion

  }
}
