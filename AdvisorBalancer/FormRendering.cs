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

    #region Display and UI control Rendering

    private void tDisplay_Tick(object sender, EventArgs e) {
      try {
        if (tDisplay.Enabled) {
          tDisplay.Enabled = false;
        }
        BufferedGraphics Canvas = BufferedGraphicsManager.Current.Allocate(this.CreateGraphics(), this.DisplayRectangle);
        try {

          try {
            RenderMarkets(Canvas);
          } catch (Exception e01) {
            LogException("d1", e01);
          }

          try {
            RenderWorkingProgress(Canvas);
          } catch (Exception e01) {
            LogException("d0", e01);
          }


          Canvas.Render(this.CreateGraphics());

        } finally {
          Canvas.Dispose();
        }
        if (!tDisplay.Enabled) {
          tDisplay.Enabled = true;
        }
      } catch (Exception e01) {
        LogException("d5", e01);
      }

    }
    
    public void SetActionCtrVisibility(CurrencyPair afm) {

      if (MarketDetailVisible) {

        if (!btnBuy.Visible) { btnBuy.Visible = true; }
        if (!btnSell.Visible) { btnSell.Visible = true; }
        if (!btnCancel.Visible) { btnCancel.Visible = true; }
        if (!edPrice.Visible) { edPrice.Visible = true; }
        if (!edMarkup.Visible) { edMarkup.Visible = true; }
        if (!btnBuyM.Visible) { btnBuyM.Visible = true; }
        if (!btnSellM.Visible) { btnSellM.Visible = true; }
        if (!edWhaleDepth.Visible) { edWhaleDepth.Visible = true; }
        if (!btnSellWhale.Visible) { btnSellWhale.Visible = true; }

        string sMarketViewMode = MarketsOfInterest[FocusedMarket].mv["MarketViewMode"];                
        if (sMarketViewMode == "Shards") {
          if (edBuyThresh.Visible) { edBuyThresh.Visible = false; }
          if (edSellThresh.Visible) { edSellThresh.Visible = false; }
          if (edHoldAmount.Visible) { edHoldAmount.Visible = false; }
          if (cbGoHold.Visible) { cbGoHold.Visible = false; }
        } else if (sMarketViewMode == "Vars") {
          if (!edSellThresh.Visible) { edSellThresh.Visible = true; }
          if (!edHoldAmount.Visible) { edHoldAmount.Visible = true; }
          if (!edBuyThresh.Visible) { edBuyThresh.Visible = true; }
          if (!cbGoHold.Visible) { cbGoHold.Visible = true; }
        } else if (sMarketViewMode == "Charts") {
          if (edBuyThresh.Visible) { edBuyThresh.Visible = false; }
          if (edSellThresh.Visible) { edSellThresh.Visible = false; }
          if (edHoldAmount.Visible) { edHoldAmount.Visible = false; }
          if (cbGoHold.Visible) { cbGoHold.Visible = false; }
        }
              

        if ((afm != null) && (MarketsOfInterest.Keys.Contains(afm) && (MarketsOfInterest[afm].tradeOpenOrders.Count > 0))) {
          if (!btnCancel.Enabled) { btnCancel.Enabled = true; }
        } else {
          if (btnCancel.Enabled) { btnCancel.Enabled = false; }
        }

        if (afm != null) {
          if ((Balances.Keys.Contains(afm.QuoteCurrency)) && (Balances[afm.QuoteCurrency].QuoteAvailable > 0.0005)) {
            btnSell.Enabled = true;
            btnSellM.Enabled = true;
            btnSellWhale.Enabled = true;
          } else {
            if (btnSell.Enabled) { btnSell.Enabled = false; }
            if (btnSellM.Enabled) { btnSellM.Enabled = false; }
            if (btnSellWhale.Enabled) { btnSellWhale.Enabled = false; }
          }
          if ((Balances.Keys.Contains(afm.BaseCurrency)) && (Balances[afm.BaseCurrency].QuoteAvailable > 0.0005)) {
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
        if (btnSellWhale.Visible) { btnSellWhale.Visible = false; }

        if (edBuyThresh.Visible) { edBuyThresh.Visible = false; }
        if (edSellThresh.Visible) { edSellThresh.Visible = false; }
        if (edHoldAmount.Visible) { edHoldAmount.Visible = false; }
        if (cbGoHold.Visible) { cbGoHold.Visible = false; }
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
            bg.Graphics.DrawString("Crypto Investment Advisor ", fLogo20, Brushes.Chartreuse, new PointF(Convert.ToSingle(iLeft + iWidth / 2 - sLocSize.Width / 2), Convert.ToSingle(iTop + iHeight / 2 - 2 * sLocSize.Height)));
            bg.Graphics.DrawString(sWaitDesc, fCur10, Brushes.White, new PointF(Convert.ToSingle(iLeft + iWidth / 2 - sLocSize.Width / 2), Convert.ToSingle(iTop + iHeight / 2 + 0 * sLocSize.Height)));
            
          } else {

            Int32 iLeft = Convert.ToInt32(4 * f20Width - f20Width / 2);
            Int32 iTop = Convert.ToInt32(f20Height - f20Height / 2);
            Int32 iWidth = Convert.ToInt32(f20Width * 1.25 );
            Int32 iHeight = Convert.ToInt32(f15Height * 0.69);
            bg.Graphics.FillRectangle(Brushes.DarkSeaGreen, new Rectangle(iLeft, iTop, iWidth, iHeight));
            bg.Graphics.DrawString("Processing Please Wait ", fCur10, Brushes.White, new PointF(Convert.ToSingle(iLeft + iWidth / 2 - sLocSize.Width / 4), Convert.ToSingle(iTop + iHeight / 2 - sLocSize.Height/4 )));
            bg.Graphics.DrawString(sWaitDesc, fCur9, Brushes.Chartreuse,             new PointF(Convert.ToSingle(iLeft + iWidth / 2 - sLocSize.Width / 4), Convert.ToSingle(iTop + iHeight / 2 + sLocSize.Height/4 )));

          }

        }
      } catch (Exception e) {
        LogException("R1", e);
        throw e;
      }
    }

    private void RenderMarkets(BufferedGraphics bg) {
      try {
        if (MarketsOfInterest.Count > 0) {
          #region Initialize vars
          Int32 iMarketCount = MarketsOfInterest.Count;
          BestBuys.Clear();
          BestSells.Clear();
          AlphaMarkets.Clear();
          BaseRates aBaseRate = getBaseRates();

          foreach (CurrencyPair cp in MarketsOfInterest.Keys) {
            #region Build Sorted Lists
            Boolean doFilterOut = false;
            Boolean doFilterOut2 = false;
            if (MarketsOfInterest[cp].LastMarketData.Volume24HourBase < 40) {
              doFilterOut = true;
            }
            if (cbOnlyBTC.Checked) {
              if (cp.BaseCurrency != "BTC") {
                doFilterOut = true;
                doFilterOut2 = true;
              }
            }
            if (!doFilterOut) {
              try {
                double iBuyRank = MarketsOfInterest[cp].BuyRank();
                if (BestBuys.ContainsKey(iBuyRank)) {
                  while (BestBuys.ContainsKey(iBuyRank) && (!double.IsNaN(iBuyRank))) {
                    iBuyRank = iBuyRank + 0.00000001;
                  }
                }
                BestBuys.Add(iBuyRank, MarketsOfInterest[cp]);
              } catch { }
            }
            try {
              if (MarketsOfInterest[cp].Selected) {
                double iSellRank = MarketsOfInterest[cp].SellRank();
                if ((BestSells.ContainsKey(iSellRank)) && ( !double.IsNaN(iSellRank ))){
                  while (BestSells.ContainsKey(iSellRank)&&(!double.IsNaN(iSellRank))){
                    iSellRank = iSellRank + 0.00000001;
                  }
                }
                BestSells.Add(iSellRank, MarketsOfInterest[cp]);
              }
            } catch { }
            try {
              if (!doFilterOut2) {
                string sMarket = cp.QuoteCurrency + " " + cp.BaseCurrency;
                if (!AlphaMarkets.ContainsKey(sMarket)) {
                  AlphaMarkets.Add(cp.QuoteCurrency + " " + cp.BaseCurrency, MarketsOfInterest[cp]);
                }
              }
            } catch { }
            #endregion
          }

          float fWidth = bg.Graphics.VisibleClipBounds.Width;
          float fHeight = bg.Graphics.VisibleClipBounds.Height;
          SolidBrush aBF = new SolidBrush( BackColor);
          bg.Graphics.FillRectangle(aBF, 0, 0, fWidth, fHeight);
          double f20Height = fHeight * 0.2;
          double f05Height = fHeight * 0.065;
          double f15Height = fHeight * 0.145;
          double f20Width = fWidth * 0.2;
          double f15Width = fWidth * 0.15;
          SizeF sLocSize = bg.Graphics.MeasureString("    0.00000000 BTC ", fCur6);
          Boolean DidMarketHistory = false;
          if ((DoGetMarketHistory) && (lPoloQue.Count == 0)) {
            DidMarketHistory = true;
            DoGetMarketHistory = false;
          }
          Boolean DidTradeHistory = false;
          if (DoGetTradeHistory) {
            DidTradeHistory = true;
            DoGetTradeHistory = false;
          }

          CurrencyPair acpb = new CurrencyPair("USDT", "BTC");
          double dBTCPrice = 0;
          if (MarketsOfInterest.Keys.Contains(acpb)) {
            dBTCPrice = MarketsOfInterest[acpb].LastMarketData.PriceLast;
            BTCRate = dBTCPrice;
            TotalUSDValue = TotalBTCValue * BTCRate;
          }

          List<Jojatekok.PoloniexAPI.TradingTools.IOrder> xMOITOO = null;
          if ((FocusedMarket != null) && (MarketsOfInterest.Keys.Contains(FocusedMarket))) {
            xMOITOO = MarketsOfInterest[FocusedMarket].tradeOpenOrders;
          }

          #endregion
          #region Draw sorted list markets
          #region top menu row
          string sTopLeft = daTime.Minute.ToString().PadLeft(2, '0') + ":" + daTime.Second.ToString().PadLeft(2, '0') + " " + ftPerSec.ToString().PadLeft(2, ' ') + "/Sec " + ftPerMin.ToString().PadLeft(4, ' ') + "/Min  ";
          if (cbOnlyBTC.Left != Convert.ToInt32(bg.Graphics.MeasureString(sTopLeft, fCur10).Width) + 10) {
            cbOnlyBTC.Left = Convert.ToInt32(bg.Graphics.MeasureString(sTopLeft, fCur10).Width) + 10;
            cbOnlyBTC.Top = 1;
            cbTradeGo.Left = cbOnlyBTC.Left + cbOnlyBTC.Width;
            cbTradeGo.Top = 1;
          }
         // if (tbRunMode.Left != Convert.ToInt32(cbOnlyBTC.Left + cbOnlyBTC.Width + 20)) {
         //   tbRunMode.Left = Convert.ToInt32(cbOnlyBTC.Left + cbOnlyBTC.Width + 20);
         // }
          SizeF sfModeWidth = bg.Graphics.MeasureString("Shift", fCur10);
          Int32 iModeWidth = Convert.ToInt32(sfModeWidth.Width);
          string sRunMode = "";
         // if (tbRunMode.Value == 0) {
         //   sRunMode = "Off";
         // } else if (tbRunMode.Value == 1) {
         //   sRunMode = "Buy Top";
         // } else {
         //   sRunMode = "Shift";
         // }
          bg.Graphics.DrawString(sTopLeft, fCur10, Brushes.Chartreuse, new PointF(Convert.ToSingle(3), Convert.ToSingle(0)));
         // if (tbRunMode.Value == 0) {
         //   bg.Graphics.DrawString(sRunMode, fCur10, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(tbRunMode.Left + tbRunMode.Width + 10), Convert.ToSingle(tbRunMode.Top + 1)));
         // } else {
         //   bg.Graphics.DrawString(sRunMode, fCur10, Brushes.Chartreuse, new PointF(Convert.ToSingle(tbRunMode.Left + tbRunMode.Width + 10), Convert.ToSingle(tbRunMode.Top + 1)));
         // }

          //top right
          String sPoloQue = "BTC " + dBTCPrice.toStr2() + " Uptime: " + sUpTime + "  Polo Que Len: " + lPoloQue.Count.ToString();
          bg.Graphics.DrawString(sPoloQue, fCur10, Brushes.Chartreuse, new PointF(Convert.ToSingle(fWidth - bg.Graphics.MeasureString(sPoloQue, fCur10).Width), Convert.ToSingle(0)));

          //top middle.
          string sLC = "";
          string sRC = "";
          //  if (Balances.Keys.Contains("BTC")) {
          sLC = TotalBTCValue.toStr8() + " / ";
          sRC = " " + Convert.ToDouble(TotalBTCValue / Convert.ToDouble(edChunkSize.Value)).toStr2() + " chuncks";
          //  }
          SizeF aSF = bg.Graphics.MeasureString("RANK", fCur8);
          edChunkSize.Left = Convert.ToInt32(fWidth / 2 - edChunkSize.Width / 2);
          bg.Graphics.DrawString(sLC, fCur10, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(edChunkSize.Left - bg.Graphics.MeasureString(sLC, fCur10).Width - 10), Convert.ToSingle(2)));
          bg.Graphics.DrawString(sRC, fCur10, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(edChunkSize.Left + edChunkSize.Width + 10), Convert.ToSingle(2)));
          if (iDisplayMode == 0) {
            bg.Graphics.FillRectangle(Brushes.DarkSeaGreen, 1, Convert.ToSingle(f05Height / 2), Convert.ToSingle(f05Height), Convert.ToSingle(f05Height / 2));
            bg.Graphics.DrawRectangle(Pens.Chartreuse, 1, Convert.ToSingle(f05Height / 2), Convert.ToSingle(f05Height), Convert.ToSingle(f05Height / 2));            
            bg.Graphics.DrawRectangle(Pens.Chartreuse, Convert.ToSingle(f05Height) + 2, Convert.ToSingle(f05Height / 2), Convert.ToSingle(f05Height), Convert.ToSingle(f05Height / 2));
            bg.Graphics.DrawRectangle(Pens.Chartreuse, Convert.ToSingle(2 * f05Height) + 2, Convert.ToSingle(f05Height / 2), Convert.ToSingle(f05Height), Convert.ToSingle(f05Height / 2));
            bg.Graphics.DrawString("Rank", fCur8, Brushes.Chartreuse, new PointF(Convert.ToSingle(f05Height/2 - aSF.Width / 2), Convert.ToSingle(f05Height / 2 + (fCur6.Height / 2))));
            bg.Graphics.DrawString("All", fCur8, Brushes.Chartreuse, new PointF(Convert.ToSingle(3*f05Height/2 - aSF.Width / 2), Convert.ToSingle(f05Height / 2 + (fCur6.Height /2))));
            bg.Graphics.DrawString("Trol", fCur8, Brushes.Chartreuse, new PointF(Convert.ToSingle(5 * f05Height / 2 - aSF.Width / 2), Convert.ToSingle(f05Height / 2 + (fCur6.Height / 2))));
          } else if (iDisplayMode == 1) {            
            bg.Graphics.DrawRectangle(Pens.Chartreuse, 1, Convert.ToSingle(f05Height / 2), Convert.ToSingle(f05Height), Convert.ToSingle(f05Height / 2));
            bg.Graphics.FillRectangle(Brushes.DarkSeaGreen, Convert.ToSingle(f05Height) + 2, Convert.ToSingle(f05Height / 2), Convert.ToSingle(f05Height), Convert.ToSingle(f05Height / 2));
            bg.Graphics.DrawRectangle(Pens.Chartreuse, Convert.ToSingle(f05Height) + 2, Convert.ToSingle(f05Height / 2), Convert.ToSingle(f05Height), Convert.ToSingle(f05Height / 2));
            bg.Graphics.DrawRectangle(Pens.Chartreuse, Convert.ToSingle(2 * f05Height) + 2, Convert.ToSingle(f05Height / 2), Convert.ToSingle(f05Height), Convert.ToSingle(f05Height / 2));
            bg.Graphics.DrawString("Rank", fCur8, Brushes.Chartreuse, new PointF(Convert.ToSingle(f05Height/2 - aSF.Width / 2), Convert.ToSingle(f05Height / 2 + (fCur6.Height / 2))));
            bg.Graphics.DrawString("All", fCur8, Brushes.Chartreuse, new PointF(Convert.ToSingle(3 * f05Height/2 - aSF.Width / 2), Convert.ToSingle(f05Height / 2 + (fCur6.Height /2))));
            bg.Graphics.DrawString("Trol", fCur8, Brushes.Chartreuse, new PointF(Convert.ToSingle(5 * f05Height / 2 - aSF.Width / 2), Convert.ToSingle(f05Height / 2 + (fCur6.Height / 2))));
          } else if (iDisplayMode == 2) {
            bg.Graphics.DrawRectangle(Pens.Chartreuse, 1,                                    Convert.ToSingle(f05Height / 2), Convert.ToSingle(f05Height), Convert.ToSingle(f05Height / 2));
            bg.Graphics.DrawRectangle(Pens.Chartreuse, Convert.ToSingle(f05Height) + 2,      Convert.ToSingle(f05Height / 2), Convert.ToSingle(f05Height), Convert.ToSingle(f05Height / 2));
            bg.Graphics.FillRectangle(Brushes.DarkSeaGreen, Convert.ToSingle(2*f05Height) + 2, Convert.ToSingle(f05Height / 2), Convert.ToSingle(f05Height), Convert.ToSingle(f05Height / 2));
            bg.Graphics.DrawRectangle(Pens.Chartreuse,      Convert.ToSingle(2*f05Height) + 2, Convert.ToSingle(f05Height / 2), Convert.ToSingle(f05Height), Convert.ToSingle(f05Height / 2));
            bg.Graphics.DrawString("Rank", fCur8, Brushes.Chartreuse, new PointF(Convert.ToSingle(f05Height / 2 - aSF.Width / 2), Convert.ToSingle(f05Height / 2 + (fCur6.Height / 2))));
            bg.Graphics.DrawString("All", fCur8, Brushes.Chartreuse, new PointF(Convert.ToSingle(3 * f05Height / 2 - aSF.Width / 2), Convert.ToSingle(f05Height / 2 + (fCur6.Height / 2))));
            bg.Graphics.DrawString("Trol", fCur8, Brushes.Chartreuse, new PointF(Convert.ToSingle(5 * f05Height / 2 - aSF.Width / 2), Convert.ToSingle(f05Height / 2 + (fCur6.Height / 2))));
          }

          #endregion

          if (iDisplayMode == 0) {           

            for (Int32 iDMC = 0; iDMC < 5; iDMC++) {
              #region Row 1
              if (iDMC < BestBuys.Count) {
                double iro = BestBuys.Keys[BestBuys.Count - 1 - iDMC];
                CurrencyPair acp = BestBuys[iro].CurPair;
                if ((DidMarketHistory) && (!MarketDetailVisible)) {
                  Dictionary<string, object> aCMDParams = new Dictionary<string, object>();
                  aCMDParams["CurrencyPair"] = acp;
                  lPoloQue.ChkAdd(new appCmd("MarketsGetTrades", aCMDParams));
                }
                string sLastPrice = ""; string sMarket2 = "";
                string sBookPrice = ""; string sBookPrice2 = "";
                string sMarket = "";
                if (acp.BaseCurrency == "USDT") {
                  sMarket = acp.BaseCurrency + " " + acp.QuoteCurrency + " ".PadLeft(BestBuys[iro].LastMarketData.PriceLast.toStr2().Length,' ') + "  " + BestBuys[iro].PerSecBuyPriceChangeAvg.toStr8();
                  sMarket2 = (" ".PadLeft(acp.BaseCurrency.Length + 1 + acp.QuoteCurrency.Length + 1, ' ')) + BestBuys[iro].LastMarketData.PriceLast.toStr2();
                  sLastPrice = BestBuys[iro].LowestSell.toStr2() + " " + Convert.ToDouble(BestBuys[iro].LastMarketData.PriceLast * 0.0069).toStr2() + " " + BestBuys[iro].HighestBuy.toStr2();
                  sBookPrice = BestBuys[iro].LastMarketData.OrderTopBuy.toStr2();
                  sBookPrice2 = (" ".PadLeft(BestBuys[iro].LastMarketData.OrderTopBuy.toStr2().Length+1, ' ')) + " " + BestBuys[iro].LastMarketData.OrderTopSell.toStr2();

                } else {
                  sMarket = acp.BaseCurrency + " " + acp.QuoteCurrency + " ".PadLeft( BestBuys[iro].LastMarketData.PriceLast.toStr8().Length, ' ') + "  " + BestBuys[iro].PerSecBuyPriceChangeAvg.toStr8();
                  sMarket2 = (" ".PadLeft(acp.BaseCurrency.Length+1 + acp.QuoteCurrency.Length+1, ' ')) + BestBuys[iro].LastMarketData.PriceLast.toStr8();
                  sLastPrice = BestBuys[iro].LowestSell.toStr8() + " " + Convert.ToDouble(BestBuys[iro].LastMarketData.PriceLast * 0.0069).toStr8() + " " + BestBuys[iro].HighestBuy.toStr8();
                  sBookPrice = BestBuys[iro].LastMarketData.OrderTopBuy.toStr8();
                  sBookPrice2 = (" ".PadLeft(BestBuys[iro].LastMarketData.OrderTopBuy.toStr8().Length+1)) + " " + BestBuys[iro].LastMarketData.OrderTopSell.toStr8();
                
                }                        // " " +BestBuys[iro].LastBTCEst.toStr8()+
                sLastPrice = "#1 spot " + BestBuys[iro].Number1SpotCount.ToString() + " " + GetMarketState(acp).ToString() + " " + BestBuys[iro].PriceRateOfChangeAvg.toStr8();
                string sMarRank = "Tic " + BestBuys[iro].PerSecSellTicAvg.toStr2() + "/S " + BestBuys[iro].FiveSecSellTicAvg.toStr2() + "/5s B " + BestBuys[iro].BuyTickerCount.ToString() + " S " + BestBuys[iro].SellTickerCount.ToString();
                string sTradeH = "" + (BestBuys[iro].hasTradeHistory ? "V B " + BestBuys[iro].TradeHistoryBuyVol.toStr4() + " S " + BestBuys[iro].TradeHistorySellVol.toStr4() : "");
                SizeF sMarSize = bg.Graphics.MeasureString(sMarket, fCur10);
                SizeF sfMarketRank = bg.Graphics.MeasureString(sMarRank, fCur10);
                SizeF sLastPSize = bg.Graphics.MeasureString(sLastPrice, fCur9);
                SizeF sLastBSize = bg.Graphics.MeasureString(sBookPrice2, fCur9);
                SizeF sfTradeH = bg.Graphics.MeasureString(sTradeH, fCur8);
                if (BestBuys[iro].Selected) {
                  //Brushes.MidnightBlue
                  Brush theColor = Brushes.DarkBlue;
                  if ((FocusedMarket != null) && (BestBuys[iro].CurPair == FocusedMarket)) {
                    theColor = Brushes.MidnightBlue;
                  }
                  bg.Graphics.FillRectangle(theColor, new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(f05Height + 1), Convert.ToInt32(f20Width), Convert.ToInt32(f15Height)));
                  bg.Graphics.DrawRectangle(Pens.Chartreuse, new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(f05Height + 1), Convert.ToInt32(f20Width), Convert.ToInt32(f15Height)));
                } else {
                  bg.Graphics.DrawRectangle(Pens.Chartreuse, new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(f05Height + 1), Convert.ToInt32(f20Width), Convert.ToInt32(f15Height)));
                }
                Brush bQuoteFill;
                if (BestBuys[iro].PrevMarketData.OrderTopBuy >= BestBuys[iro].LastMarketData.PriceLast) {               
                  bQuoteFill = Brushes.HotPink;
                } else if (BestBuys[iro].PrevMarketData.OrderTopSell <= BestBuys[iro].LastMarketData.PriceLast) {              
                  bQuoteFill = Brushes.Chartreuse;
                } else {
                  bQuoteFill = Brushes.WhiteSmoke;
                }
                bg.Graphics.DrawString(sLastPrice, fCur9, Brushes.WhiteSmoke,  new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastPSize.Width / 2),  Convert.ToSingle(f05Height + f15Height / 2 - sLastPSize.Height / 2 - 2 * sLastPSize.Height)));
                bg.Graphics.DrawString(sBookPrice, fCur9, Brushes.HotPink,     new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastBSize.Width / 2),  Convert.ToSingle(f05Height + f15Height / 2 - sLastPSize.Height / 2 - sLastPSize.Height)));
                bg.Graphics.DrawString(sBookPrice2, fCur9, Brushes.Chartreuse, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastBSize.Width / 2),  Convert.ToSingle(f05Height + f15Height / 2 - sLastPSize.Height / 2 - sLastPSize.Height)));
                bg.Graphics.DrawString(sMarket, fCur10, Brushes.WhiteSmoke,    new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sMarSize.Width / 2),    Convert.ToSingle(f05Height + f15Height / 2 - sMarSize.Height / 2)));
                bg.Graphics.DrawString(sMarket2, fCur10, bQuoteFill,           new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sMarSize.Width / 2),    Convert.ToSingle(f05Height + f15Height / 2 - sMarSize.Height / 2)));
                bg.Graphics.DrawString(sMarRank, fCur10, Brushes.WhiteSmoke,   new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sfMarketRank.Width / 2),Convert.ToSingle(f05Height + f15Height / 2 - sMarSize.Height / 2 + sMarSize.Height)));
                bg.Graphics.DrawString(sTradeH, fCur8, Brushes.WhiteSmoke,     new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sfTradeH.Width / 2),    Convert.ToSingle(f05Height + f15Height / 2 - sMarSize.Height / 2 + 2 * sMarSize.Height)));
              }
              #endregion
              #region Row 2
              if (iDMC + 5 < BestBuys.Count) {
                double iro = BestBuys.Keys[BestBuys.Count - 1 - (iDMC + 5)];
                CurrencyPair acp = BestBuys[iro].CurPair;
                if ((DidMarketHistory) && (!MarketDetailVisible)) {
                  Dictionary<string, object> aCMDParams = new Dictionary<string, object>();
                  aCMDParams["CurrencyPair"] = acp;
                  lPoloQue.ChkAdd(new appCmd("MarketsGetTrades", aCMDParams));
                }
                string sLastPrice = ""; string sMarket2 = "";
                string sBookPrice = ""; string sBookPrice2 = "";
                string sMarket = "";
                if (acp.BaseCurrency == "USDT") {
                  sMarket = acp.BaseCurrency + " " + acp.QuoteCurrency + " ".PadLeft(BestBuys[iro].LastMarketData.PriceLast.toStr2().Length, ' ') + "  " + BestBuys[iro].PerSecBuyPriceChangeAvg.toStr8();
                  sMarket2 = (" ".PadLeft(acp.BaseCurrency.Length + 1 + acp.QuoteCurrency.Length + 1, ' ')) + BestBuys[iro].LastMarketData.PriceLast.toStr2();
                  sLastPrice = BestBuys[iro].LowestSell.toStr2() + " " + Convert.ToDouble(BestBuys[iro].LastMarketData.PriceLast * 0.0069).toStr2() + " " + BestBuys[iro].HighestBuy.toStr2();
                  sBookPrice = BestBuys[iro].LastMarketData.OrderTopBuy.toStr2();
                  sBookPrice2 = (" ".PadLeft(BestBuys[iro].LastMarketData.OrderTopBuy.toStr2().Length + 1, ' ')) + " " + BestBuys[iro].LastMarketData.OrderTopSell.toStr2();

                } else {
                  sMarket = acp.BaseCurrency + " " + acp.QuoteCurrency + " ".PadLeft(BestBuys[iro].LastMarketData.PriceLast.toStr8().Length, ' ') + "  " + BestBuys[iro].PerSecBuyPriceChangeAvg.toStr8();
                  sMarket2 = (" ".PadLeft(acp.BaseCurrency.Length + 1 + acp.QuoteCurrency.Length + 1, ' ')) + BestBuys[iro].LastMarketData.PriceLast.toStr8();
                  sLastPrice = BestBuys[iro].LowestSell.toStr8() + " " + Convert.ToDouble(BestBuys[iro].LastMarketData.PriceLast * 0.0069).toStr8() + " " + BestBuys[iro].HighestBuy.toStr8();
                  sBookPrice = BestBuys[iro].LastMarketData.OrderTopBuy.toStr8();
                  sBookPrice2 = (" ".PadLeft(BestBuys[iro].LastMarketData.OrderTopBuy.toStr8().Length + 1)) + " " + BestBuys[iro].LastMarketData.OrderTopSell.toStr8();

                }
                sLastPrice = "#1 spot " + BestBuys[iro].Number1SpotCount.ToString() + " " + GetMarketState(acp).ToString() + " " + BestBuys[iro].PriceRateOfChangeAvg.toStr8();

                string sMarRank = "Tic " + BestBuys[iro].PerSecSellTicAvg.toStr2() + " " + BestBuys[iro].FiveSecSellTicAvg.toStr2() + " B " + BestBuys[iro].BuyTickerCount.ToString() + " S " + BestBuys[iro].SellTickerCount.ToString();
                string sTradeH = "" + (BestBuys[iro].hasTradeHistory ? "V B " + BestBuys[iro].TradeHistoryBuyVol.toStr4() + " S " + BestBuys[iro].TradeHistorySellVol.toStr4() : "");
                SizeF sfTradeH = bg.Graphics.MeasureString(sTradeH, fCur8);
                SizeF sfMarketRank = bg.Graphics.MeasureString(sMarRank, fCur10);
                SizeF sMarSize = bg.Graphics.MeasureString(sMarket, fCur10);
                SizeF sLastPSize = bg.Graphics.MeasureString(sLastPrice, fCur9);
                SizeF sLastBSize = bg.Graphics.MeasureString(sBookPrice2, fCur9);
                if (BestBuys[iro].Selected) {
                  Brush theColor = Brushes.DarkBlue;
                  if ((FocusedMarket != null) && (BestBuys[iro].CurPair == FocusedMarket)) {
                    theColor = Brushes.MidnightBlue;
                  }
                  bg.Graphics.FillRectangle(theColor, new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(f05Height + f15Height + 1), Convert.ToInt32(f20Width), Convert.ToInt32(f15Height)));
                  bg.Graphics.DrawRectangle(Pens.Chartreuse, new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(f05Height + f15Height + 1), Convert.ToInt32(f20Width), Convert.ToInt32(f15Height)));
                } else {
                  bg.Graphics.DrawRectangle(Pens.Chartreuse, new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(f05Height + f15Height + 1), Convert.ToInt32(f20Width), Convert.ToInt32(f15Height)));
                }
                Brush bQuoteFill;
                if (BestBuys[iro].PrevMarketData.OrderTopBuy >= BestBuys[iro].LastMarketData.PriceLast) {
                  bQuoteFill = Brushes.HotPink;
                } else if (BestBuys[iro].PrevMarketData.OrderTopSell <= BestBuys[iro].LastMarketData.PriceLast) {
                  bQuoteFill = Brushes.Chartreuse;
                } else {
                  bQuoteFill = Brushes.WhiteSmoke;
                }

                bg.Graphics.DrawString(sLastPrice, fCur9, Brushes.WhiteSmoke,  new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastPSize.Width / 2),   Convert.ToSingle(f20Height + f15Height / 2 - sLastPSize.Height / 2 - 1.5 * sMarSize.Height)));
                bg.Graphics.DrawString(sBookPrice, fCur9, Brushes.HotPink,     new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastBSize.Width / 2),   Convert.ToSingle(f20Height + f15Height / 2 - sLastPSize.Height / 2 - 0.5 * sLastPSize.Height)));
                bg.Graphics.DrawString(sBookPrice2, fCur9, Brushes.Chartreuse, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastBSize.Width / 2),   Convert.ToSingle(f20Height + f15Height / 2 - sLastPSize.Height / 2 - 0.5 * sLastPSize.Height)));
                bg.Graphics.DrawString(sMarket, fCur10, Brushes.WhiteSmoke,    new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sMarSize.Width / 2),     Convert.ToSingle(f20Height + f15Height / 2 + sMarSize.Height / 2))); 
                bg.Graphics.DrawString(sMarket2, fCur10, bQuoteFill,           new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sMarSize.Width / 2),     Convert.ToSingle(f20Height + f15Height / 2 + sMarSize.Height / 2)));
                bg.Graphics.DrawString(sMarRank, fCur10, Brushes.WhiteSmoke,   new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sfMarketRank.Width / 2), Convert.ToSingle(f20Height + f15Height / 2 + sMarSize.Height / 2 + sMarSize.Height)));
                bg.Graphics.DrawString(sTradeH, fCur8, Brushes.WhiteSmoke,     new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sfTradeH.Width / 2),     Convert.ToSingle(f20Height + f15Height / 2 + sMarSize.Height / 2 + 2 * sMarSize.Height)));
              }
              #endregion
              #region Row 3
              if (iDMC + 10 < BestBuys.Count) {
                double iro = BestBuys.Keys[BestBuys.Count - 1 - (iDMC + 10)];
                CurrencyPair acp = BestBuys[iro].CurPair;
                //       if (DidMarketHistory) {
                //         Dictionary<string, object> aCMDParams = new Dictionary<string, object>();
                //         aCMDParams["CurrencyPair"] = acp;
                //         lPoloQue.ChkAdd(new appCmd("MarketsGetTrades", aCMDParams));
                //       }
                string sLastPrice = ""; string sMarket2 = "";
                string sBookPrice = ""; string sBookPrice2 = "";
                string sMarket = "";
                if (acp.BaseCurrency == "USDT") {
                  sMarket = acp.BaseCurrency + " " + acp.QuoteCurrency + " ".PadLeft(BestBuys[iro].LastMarketData.PriceLast.toStr2().Length, ' ') + "  " + BestBuys[iro].PerSecBuyPriceChangeAvg.toStr8();
                  sMarket2 = (" ".PadLeft(acp.BaseCurrency.Length + 1 + acp.QuoteCurrency.Length + 1, ' ')) + BestBuys[iro].LastMarketData.PriceLast.toStr2();
                  sLastPrice = BestBuys[iro].LowestSell.toStr2() + " " + Convert.ToDouble(BestBuys[iro].LastMarketData.PriceLast * 0.0069).toStr2() + " " + BestBuys[iro].HighestBuy.toStr2();
                  sBookPrice = BestBuys[iro].LastMarketData.OrderTopBuy.toStr2();
                  sBookPrice2 = (" ".PadLeft(BestBuys[iro].LastMarketData.OrderTopBuy.toStr2().Length + 1, ' ')) + " " + BestBuys[iro].LastMarketData.OrderTopSell.toStr2();

                } else {
                  sMarket = acp.BaseCurrency + " " + acp.QuoteCurrency + " ".PadLeft(BestBuys[iro].LastMarketData.PriceLast.toStr8().Length, ' ') + "  " + BestBuys[iro].PerSecBuyPriceChangeAvg.toStr8();
                  sMarket2 = (" ".PadLeft(acp.BaseCurrency.Length + 1 + acp.QuoteCurrency.Length + 1, ' ')) + BestBuys[iro].LastMarketData.PriceLast.toStr8();
                  sLastPrice = BestBuys[iro].LowestSell.toStr8() + " " + Convert.ToDouble(BestBuys[iro].LastMarketData.PriceLast * 0.0069).toStr8() + " " + BestBuys[iro].HighestBuy.toStr8();
                  sBookPrice = BestBuys[iro].LastMarketData.OrderTopBuy.toStr8();
                  sBookPrice2 = (" ".PadLeft(BestBuys[iro].LastMarketData.OrderTopBuy.toStr8().Length + 1)) + " " + BestBuys[iro].LastMarketData.OrderTopSell.toStr8();

                }
                sLastPrice = "#1 spot " + BestBuys[iro].Number1SpotCount.ToString() + " " + GetMarketState(acp).ToString() + " " + BestBuys[iro].PriceRateOfChangeAvg.toStr8();

                string sMarRank = "Tic " + BestBuys[iro].PerSecSellTicAvg.toStr2() + " " + BestBuys[iro].FiveSecSellTicAvg.toStr2() + " B " + BestBuys[iro].BuyTickerCount.ToString() + " S " + BestBuys[iro].SellTickerCount.ToString();
                string sTradeH = "" + (BestBuys[iro].hasTradeHistory ? "V B " + BestBuys[iro].TradeHistoryBuyVol.toStr4() + " S " + BestBuys[iro].TradeHistorySellVol.toStr4() : "");
                SizeF sfTradeH = bg.Graphics.MeasureString(sTradeH, fCur8);
                SizeF sMarSize = bg.Graphics.MeasureString(sMarket, fCur10);
                SizeF sfMarketRank = bg.Graphics.MeasureString(sMarRank, fCur10);
                SizeF sLastPSize = bg.Graphics.MeasureString(sLastPrice, fCur9);
                SizeF sLastBSize = bg.Graphics.MeasureString(sBookPrice2, fCur9);
                if (BestBuys[iro].Selected) {
                  bg.Graphics.FillRectangle(Brushes.DarkBlue, new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(f05Height + 2 * f15Height + 1), Convert.ToInt32(f20Width), Convert.ToInt32(f15Height)));
                  bg.Graphics.DrawRectangle(Pens.Chartreuse, new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(f05Height + 2 * f15Height + 1), Convert.ToInt32(f20Width), Convert.ToInt32(f15Height)));
                } else {
                  bg.Graphics.DrawRectangle(Pens.Chartreuse, new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(f05Height + 2 * f15Height + 1), Convert.ToInt32(f20Width), Convert.ToInt32(f15Height)));
                }
                Brush bQuoteFill;
                if (BestBuys[iro].PrevMarketData.OrderTopBuy >= BestBuys[iro].LastMarketData.PriceLast) {
                  bQuoteFill = Brushes.HotPink;
                } else if (BestBuys[iro].PrevMarketData.OrderTopSell <= BestBuys[iro].LastMarketData.PriceLast) {
                  bQuoteFill = Brushes.Chartreuse;
                } else {
                  bQuoteFill = Brushes.WhiteSmoke;
                }
                bg.Graphics.DrawString(sLastPrice, fCur9, Brushes.WhiteSmoke,  new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastPSize.Width / 2),   Convert.ToSingle(f20Height + f15Height + f15Height / 2 - sLastPSize.Height / 2 - 2 * sLastPSize.Height)));
                bg.Graphics.DrawString(sBookPrice, fCur9, Brushes.HotPink,     new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastBSize.Width / 2),   Convert.ToSingle(f20Height + f15Height + f15Height / 2 - sLastPSize.Height / 2 - sLastPSize.Height)));
                bg.Graphics.DrawString(sBookPrice2, fCur9, Brushes.Chartreuse, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastBSize.Width / 2),   Convert.ToSingle(f20Height + f15Height + f15Height / 2 - sLastPSize.Height / 2 - sLastPSize.Height)));
                bg.Graphics.DrawString(sMarket, fCur10, Brushes.WhiteSmoke,    new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sMarSize.Width / 2),     Convert.ToSingle(f20Height + f15Height + f15Height / 2 - sMarSize.Height / 2)));
                bg.Graphics.DrawString(sMarket2, fCur10, bQuoteFill,           new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sMarSize.Width / 2),     Convert.ToSingle(f20Height + f15Height + f15Height / 2 - sMarSize.Height / 2)));
                bg.Graphics.DrawString(sMarRank, fCur10, Brushes.WhiteSmoke,   new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sfMarketRank.Width / 2), Convert.ToSingle(f20Height + f15Height + f15Height / 2 - sMarSize.Height / 2 + sMarSize.Height)));
                bg.Graphics.DrawString(sTradeH, fCur8, Brushes.WhiteSmoke,     new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sfTradeH.Width / 2),     Convert.ToSingle(f20Height + f15Height + f15Height / 2 - sMarSize.Height / 2 + 2 * sMarSize.Height)));
              }
              #endregion
              #region Row 4
              if (iDMC + 15 < BestBuys.Count) {
                double iro = BestBuys.Keys[BestBuys.Count - 1 - (iDMC + 15)];
                CurrencyPair acp = BestBuys[iro].CurPair;
                string sLastPrice = ""; string sMarket2 = "";
                string sBookPrice = ""; string sBookPrice2 = "";
                string sMarket = "";
                if (acp.BaseCurrency == "USDT") {
                  sMarket = acp.BaseCurrency + " " + acp.QuoteCurrency + " ".PadLeft(BestBuys[iro].LastMarketData.PriceLast.toStr2().Length, ' ') + "  " + BestBuys[iro].PerSecBuyPriceChangeAvg.toStr8();
                  sMarket2 = (" ".PadLeft(acp.BaseCurrency.Length + 1 + acp.QuoteCurrency.Length + 1, ' ')) + BestBuys[iro].LastMarketData.PriceLast.toStr2();
                  sLastPrice = BestBuys[iro].LowestSell.toStr2() + " " + Convert.ToDouble(BestBuys[iro].LastMarketData.PriceLast * 0.0069).toStr2() + " " + BestBuys[iro].HighestBuy.toStr2();
                  sBookPrice = BestBuys[iro].LastMarketData.OrderTopBuy.toStr2();
                  sBookPrice2 = (" ".PadLeft(BestBuys[iro].LastMarketData.OrderTopBuy.toStr2().Length + 1, ' ')) + " " + BestBuys[iro].LastMarketData.OrderTopSell.toStr2();

                } else {
                  sMarket = acp.BaseCurrency + " " + acp.QuoteCurrency + " ".PadLeft(BestBuys[iro].LastMarketData.PriceLast.toStr8().Length, ' ') + "  " + BestBuys[iro].PerSecBuyPriceChangeAvg.toStr8();
                  sMarket2 = (" ".PadLeft(acp.BaseCurrency.Length + 1 + acp.QuoteCurrency.Length + 1, ' ')) + BestBuys[iro].LastMarketData.PriceLast.toStr8();
                  sLastPrice = BestBuys[iro].LowestSell.toStr8() + " " + Convert.ToDouble(BestBuys[iro].LastMarketData.PriceLast * 0.0069).toStr8() + " " + BestBuys[iro].HighestBuy.toStr8();
                  sBookPrice = BestBuys[iro].LastMarketData.OrderTopBuy.toStr8();
                  sBookPrice2 = (" ".PadLeft(BestBuys[iro].LastMarketData.OrderTopBuy.toStr8().Length + 1)) + " " + BestBuys[iro].LastMarketData.OrderTopSell.toStr8();

                }
                sLastPrice = "#1 spot " + BestBuys[iro].Number1SpotCount.ToString() + " " + GetMarketState(acp).ToString() + " " + BestBuys[iro].PriceRateOfChangeAvg.toStr8();

                string sMarRank = "Tic " + BestBuys[iro].PerSecSellTicAvg.toStr2() + " " + BestBuys[iro].FiveSecSellTicAvg.toStr2() + " B " + BestBuys[iro].BuyTickerCount.ToString() + " S " + BestBuys[iro].SellTickerCount.ToString();
                string sTradeH = "" + (BestBuys[iro].hasTradeHistory ? "V B " + BestBuys[iro].TradeHistoryBuyVol.toStr4() + " " + BestBuys[iro].tradeHistoryMarket.Count.ToString() + " S " + BestBuys[iro].TradeHistorySellVol.toStr4() : "");
                SizeF sfTradeH = bg.Graphics.MeasureString(sTradeH, fCur8);
                SizeF sMarSize = bg.Graphics.MeasureString(sMarket, fCur10);
                SizeF sfMarketRank = bg.Graphics.MeasureString(sMarRank, fCur10);
                SizeF sLastPSize = bg.Graphics.MeasureString(sLastPrice, fCur9);
                SizeF sLastBSize = bg.Graphics.MeasureString(sBookPrice2, fCur9);
                if (BestBuys[iro].Selected) {
                  bg.Graphics.FillRectangle(Brushes.DarkBlue, new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(f05Height + 3 * f15Height + 1), Convert.ToInt32(f20Width), Convert.ToInt32(f15Height)));
                  bg.Graphics.DrawRectangle(Pens.Chartreuse, new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(f05Height + 3 * f15Height + 1), Convert.ToInt32(f20Width), Convert.ToInt32(f15Height)));
                } else {
                  bg.Graphics.DrawRectangle(Pens.Chartreuse, new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(f05Height + 3 * f15Height + 1), Convert.ToInt32(f20Width), Convert.ToInt32(f15Height)));
                }
                Brush bQuoteFill;
                if (BestBuys[iro].PrevMarketData.OrderTopBuy >= BestBuys[iro].LastMarketData.PriceLast) {
                  bQuoteFill = Brushes.HotPink;
                } else if (BestBuys[iro].PrevMarketData.OrderTopSell <= BestBuys[iro].LastMarketData.PriceLast) {
                  bQuoteFill = Brushes.Chartreuse;
                } else {
                  bQuoteFill = Brushes.WhiteSmoke;
                }
                bg.Graphics.DrawString(sLastPrice, fCur9, Brushes.WhiteSmoke,  new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastPSize.Width / 2),   Convert.ToSingle(f20Height + 2 * f15Height + f15Height / 2 - sLastPSize.Height / 2 - 2 * sLastPSize.Height)));
                bg.Graphics.DrawString(sBookPrice, fCur9, Brushes.HotPink,     new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastBSize.Width / 2),   Convert.ToSingle(f20Height + 2 * f15Height + f15Height / 2 - sLastPSize.Height / 2 - sLastPSize.Height)));
                bg.Graphics.DrawString(sBookPrice2, fCur9, Brushes.Chartreuse, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastBSize.Width / 2),   Convert.ToSingle(f20Height + 2 * f15Height + f15Height / 2 - sLastPSize.Height / 2 - sLastPSize.Height)));
                bg.Graphics.DrawString(sMarket, fCur10, Brushes.WhiteSmoke,    new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sMarSize.Width / 2),     Convert.ToSingle(f20Height + 2 * f15Height + f15Height / 2 - sMarSize.Height / 2)));
                bg.Graphics.DrawString(sMarket2, fCur10, bQuoteFill,           new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sMarSize.Width / 2),     Convert.ToSingle(f20Height + 2 * f15Height + f15Height / 2 - sMarSize.Height / 2)));
                bg.Graphics.DrawString(sMarRank, fCur10, Brushes.WhiteSmoke,   new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sfMarketRank.Width / 2), Convert.ToSingle(f20Height + 2 * f15Height + f15Height / 2 - sMarSize.Height / 2 + sMarSize.Height)));
                bg.Graphics.DrawString(sTradeH, fCur8, Brushes.WhiteSmoke,     new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sfTradeH.Width / 2),     Convert.ToSingle(f20Height + 2 * f15Height + f15Height / 2 - sMarSize.Height / 2 + 2 * sMarSize.Height)));
              }
              #endregion

              #region Row 1
              if (iDMC < BestSells.Count) {
                double iro = BestSells.Keys[BestSells.Count - 1 - (iDMC)];
                CurrencyPair acp = BestSells[iro].CurPair;
            //    if ((DidTradeHistory) && (MarketDetailVisible)) {
            //      Dictionary<string, object> aCMDParams = new Dictionary<string, object>();
            //      aCMDParams["CurrencyPair"] = acp;
            //      lPoloQue.ChkAdd(new appCmd("TradingGetTrades", aCMDParams));
            //    }
                string sLastPrice = ""; string sMarket2 = "";
                string sBookPrice = ""; string sBookPrice2 = "";
                string sMarket = "";
                if (acp.BaseCurrency == "USDT") {
                  sMarket = acp.BaseCurrency + " " + acp.QuoteCurrency + " ".PadLeft(BestSells[iro].LastMarketData.PriceLast.toStr2().Length, ' ') + "  " + BestSells[iro].PerSecBuyPriceChangeAvg.toStr8();
                  sMarket2 = (" ".PadLeft(acp.BaseCurrency.Length + 1 + acp.QuoteCurrency.Length + 1, ' ')) + BestSells[iro].LastMarketData.PriceLast.toStr2();
                  sLastPrice = BestSells[iro].LowestSell.toStr2() + " " + Convert.ToDouble(BestSells[iro].LastMarketData.PriceLast * 0.0069).toStr2() + " " + BestSells[iro].HighestBuy.toStr2();
                  sBookPrice = BestSells[iro].LastMarketData.OrderTopBuy.toStr2();
                  sBookPrice2 = (" ".PadLeft(BestSells[iro].LastMarketData.OrderTopBuy.toStr2().Length + 1, ' ')) + " " + BestSells[iro].LastMarketData.OrderTopSell.toStr2();

                } else {
                  sMarket = acp.BaseCurrency + " " + acp.QuoteCurrency + " ".PadLeft(BestSells[iro].LastMarketData.PriceLast.toStr8().Length, ' ') + "  " + BestSells[iro].PerSecBuyPriceChangeAvg.toStr8();
                  sMarket2 = (" ".PadLeft(acp.BaseCurrency.Length + 1 + acp.QuoteCurrency.Length + 1, ' ')) + BestSells[iro].LastMarketData.PriceLast.toStr8();
                  sLastPrice = BestSells[iro].LowestSell.toStr8() + " " + Convert.ToDouble(BestSells[iro].LastMarketData.PriceLast * 0.0069).toStr8() + " " + BestSells[iro].HighestBuy.toStr8();
                  sBookPrice = BestSells[iro].LastMarketData.OrderTopBuy.toStr8();
                  sBookPrice2 = (" ".PadLeft(BestSells[iro].LastMarketData.OrderTopBuy.toStr8().Length + 1)) + " " + BestSells[iro].LastMarketData.OrderTopSell.toStr8();

                }
                string sMarRank = "tic " + BestSells[iro].PerSecSellTicAvg.toStr2() + " " + BestSells[iro].FiveSecSellTicAvg.toStr2() + " B " + BestSells[iro].BuyTickerCount.ToString() + " S " + BestSells[iro].SellTickerCount.ToString();
                string sTradeH = "" + /*(BestSells[iro].tradeHistoryTrades.Count > 0 ?*/ "Avg: " + BestSells[iro].AvgPricePaid.toStr8() + " -> " + Convert.ToDouble(BestSells[iro].AvgPricePaid * 1.0069).toStr8() + " "; //: "");
                string sValueChange = (Balances.Keys.Contains(acp.QuoteCurrency) ? Balances[acp.QuoteCurrency].DBQuote.toStr4() + " " + Convert.ToDouble(Balances[acp.QuoteCurrency].BitcoinValue - (Balances[acp.QuoteCurrency].DBQuote * (BestSells[iro].AvgPricePaid * 1.0069))).toStr8() : "") + " " + Convert.ToDouble(BestSells[iro].LastMarketData.PriceLast - (BestSells[iro].AvgPricePaid * 1.0069)).toStr8();
                SizeF sfValueChange = bg.Graphics.MeasureString(sValueChange, fCur10);
                SizeF sfTradeH = bg.Graphics.MeasureString(sTradeH, fCur10);
                SizeF sMarSize = bg.Graphics.MeasureString(sMarket, fCur10);
                SizeF sfMarketRank = bg.Graphics.MeasureString(sMarRank, fCur10);
                SizeF sLastPSize = bg.Graphics.MeasureString(sLastPrice, fCur9);
                SizeF sLastBSize = bg.Graphics.MeasureString(sBookPrice2, fCur9);
                if (BestSells[iro].Selected) {
                  Brush theColor = Brushes.DarkBlue;
                  if ((FocusedMarket != null) && (BestSells[iro].CurPair == FocusedMarket)) {
                    theColor = Brushes.MidnightBlue;
                  }                      ///       f05Height + 4 * f15Height
                  bg.Graphics.FillRectangle(theColor,        new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(2 * f05Height + 4 * f15Height + 1), Convert.ToInt32(f20Width), Convert.ToInt32(f15Height)));
                  //bg.Graphics.FillRectangle(Brushes.Black, new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(2 * f20Height + 2 * f15Height + 1), Convert.ToInt32(14), Convert.ToInt32(14)));
                  bg.Graphics.DrawString("X", fCur10, Brushes.Red, new PointF(Convert.ToSingle(iDMC * f20Width + 1), Convert.ToSingle(2 * f05Height + 4 * f15Height + 1)));
                } else {
                  bg.Graphics.DrawRectangle(Pens.Chartreuse, new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(2 * f05Height + 4 * f15Height + 1), Convert.ToInt32(f20Width), Convert.ToInt32(f15Height)));
                }
                Brush bQuoteFill;
                if (BestSells[iro].PrevMarketData.OrderTopBuy >= BestSells[iro].LastMarketData.PriceLast) {
                  bQuoteFill = Brushes.HotPink;
                } else if (BestSells[iro].PrevMarketData.OrderTopSell <= BestSells[iro].LastMarketData.PriceLast) {
                  bQuoteFill = Brushes.Chartreuse;
                } else {
                  bQuoteFill = Brushes.WhiteSmoke;
                }
                bg.Graphics.DrawString(sLastPrice, fCur9, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastPSize.Width / 2),       Convert.ToSingle(2 * f05Height + 4 * f15Height + f15Height / 2 - sLastPSize.Height / 2 - 5 * sLastPSize.Height / 2)));
                bg.Graphics.DrawString(sBookPrice, fCur9, Brushes.HotPink,       new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastBSize.Width / 2),    Convert.ToSingle(2 * f05Height + 4 * f15Height + f15Height / 2 - sLastPSize.Height / 2 - 3 * sLastPSize.Height / 2)));
                bg.Graphics.DrawString(sBookPrice2, fCur9, Brushes.Chartreuse,   new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastBSize.Width / 2),    Convert.ToSingle(2 * f05Height + 4 * f15Height + f15Height / 2 - sLastPSize.Height / 2 - 3 * sLastPSize.Height / 2)));
                bg.Graphics.DrawString(sMarket, fCur10, Brushes.WhiteSmoke,      new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sMarSize.Width / 2),      Convert.ToSingle(2 * f05Height + 4 * f15Height + f15Height / 2 - sMarSize.Height / 2 - 1 * sMarSize.Height / 2)));
                bg.Graphics.DrawString(sMarket2, fCur10, bQuoteFill,             new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sMarSize.Width / 2),      Convert.ToSingle(2 * f05Height + 4 * f15Height + f15Height / 2 - sMarSize.Height / 2 - 1 * sMarSize.Height / 2)));
                bg.Graphics.DrawString(sMarRank, fCur10, Brushes.WhiteSmoke,     new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sfMarketRank.Width / 2),  Convert.ToSingle(2 * f05Height + 4 * f15Height + f15Height / 2 - sMarSize.Height / 2 + 1 * sMarSize.Height / 2)));
                bg.Graphics.DrawString(sTradeH, fCur10, Brushes.WhiteSmoke,      new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sfTradeH.Width / 2),      Convert.ToSingle(2 * f05Height + 4 * f15Height + f15Height / 2 - sMarSize.Height / 2 + 3 * sMarSize.Height / 2)));
                bg.Graphics.DrawString(sValueChange, fCur10, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sfValueChange.Width / 2), Convert.ToSingle(2 * f05Height + 4 * f15Height + f15Height / 2 - sMarSize.Height / 2 + 5 * sMarSize.Height / 2)));

              }
              #endregion
              #region Row 2
              if (iDMC + 5 < BestSells.Count) {
                double iro = BestSells.Keys[BestSells.Count - (iDMC + 6)];
                CurrencyPair acp = BestSells[iro].CurPair;
              //  if (DidTradeHistory) {
              //    Dictionary<string, object> aCMDParams = new Dictionary<string, object>();
              //    aCMDParams["CurrencyPair"] = acp;
              //    lPoloQue.ChkAdd(new appCmd("TradingGetTrades", aCMDParams));
              //  }
                string sLastPrice = ""; string sMarket2 = "";
                string sBookPrice = ""; string sBookPrice2 = "";
                string sMarket = "";
                if (acp.BaseCurrency == "USDT") {
                  sMarket = acp.BaseCurrency + " " + acp.QuoteCurrency + " ".PadLeft(BestSells[iro].LastMarketData.PriceLast.toStr2().Length, ' ') + "  " + BestSells[iro].PerSecBuyPriceChangeAvg.toStr8();
                  sMarket2 = (" ".PadLeft(acp.BaseCurrency.Length + 1 + acp.QuoteCurrency.Length + 1, ' ')) + BestSells[iro].LastMarketData.PriceLast.toStr2();
                  sLastPrice = BestSells[iro].LowestSell.toStr2() + " " + Convert.ToDouble(BestSells[iro].LastMarketData.PriceLast * 0.0069).toStr2() + " " + BestSells[iro].HighestBuy.toStr2();
                  sBookPrice = BestSells[iro].LastMarketData.OrderTopBuy.toStr2();
                  sBookPrice2 = (" ".PadLeft(BestSells[iro].LastMarketData.OrderTopBuy.toStr2().Length + 1, ' ')) + " " + BestSells[iro].LastMarketData.OrderTopSell.toStr2();

                } else {
                  sMarket = acp.BaseCurrency + " " + acp.QuoteCurrency + " ".PadLeft(BestSells[iro].LastMarketData.PriceLast.toStr8().Length, ' ') + "  " + BestSells[iro].PerSecBuyPriceChangeAvg.toStr8();
                  sMarket2 = (" ".PadLeft(acp.BaseCurrency.Length + 1 + acp.QuoteCurrency.Length + 1, ' ')) + BestSells[iro].LastMarketData.PriceLast.toStr8();
                  sLastPrice = BestSells[iro].LowestSell.toStr8() + " " + Convert.ToDouble(BestSells[iro].LastMarketData.PriceLast * 0.0069).toStr8() + " " + BestSells[iro].HighestBuy.toStr8();
                  sBookPrice = BestSells[iro].LastMarketData.OrderTopBuy.toStr8();
                  sBookPrice2 = (" ".PadLeft(BestSells[iro].LastMarketData.OrderTopBuy.toStr8().Length + 1)) + " " + BestSells[iro].LastMarketData.OrderTopSell.toStr8();

                }
                string sMarRank = "Tic " + BestSells[iro].PerSecSellTicAvg.toStr2() + " " + BestSells[iro].FiveSecSellTicAvg.toStr2() + " B " + BestSells[iro].BuyTickerCount.ToString() + " S " + BestSells[iro].SellTickerCount.ToString();
                string sTradeH = "" + /*(BestSells[iro].tradeHistoryTrades.Count > 0 ?*/ "Avg: " + BestSells[iro].AvgPricePaid.toStr8() + " -> " + Convert.ToDouble(BestSells[iro].AvgPricePaid * 1.0069).toStr8() + " "; //: "");
                string sValueChange = (Balances.Keys.Contains(acp.QuoteCurrency) ? Balances[acp.QuoteCurrency].DBQuote.toStr4() + " " + Convert.ToDouble(Balances[acp.QuoteCurrency].BitcoinValue - (Balances[acp.QuoteCurrency].DBQuote * (BestSells[iro].AvgPricePaid * 1.0069))).toStr8() : "") + " " + Convert.ToDouble(BestSells[iro].LastMarketData.PriceLast - (BestSells[iro].AvgPricePaid * 1.0069)).toStr8();
                SizeF sfValueChange = bg.Graphics.MeasureString(sValueChange, fCur10);
                SizeF sfTradeH = bg.Graphics.MeasureString(sTradeH, fCur10);
                SizeF sfMarketRank = bg.Graphics.MeasureString(sMarRank, fCur10);
                SizeF sLastPSize = bg.Graphics.MeasureString(sLastPrice, fCur9);
                SizeF sMarSize = bg.Graphics.MeasureString(sMarket, fCur10);
                SizeF sLastBSize = bg.Graphics.MeasureString(sBookPrice2, fCur9);
                if (BestSells[iro].Selected) {
                  Brush theColor = Brushes.DarkBlue;
                  if ((FocusedMarket != null) && (BestSells[iro].CurPair == FocusedMarket)) {
                    theColor = Brushes.MidnightBlue;
                  }
                  bg.Graphics.FillRectangle(theColor, new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(2 * f05Height + 5 * f15Height + 1), Convert.ToInt32(f20Width), Convert.ToInt32(f15Height)));
                  bg.Graphics.DrawString("X", fCur10, Brushes.Red, new PointF(Convert.ToSingle(iDMC * f20Width), Convert.ToSingle(2 * f05Height + 5 * f15Height)));
                } else {
                  bg.Graphics.DrawRectangle(Pens.Chartreuse, new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(2 * f05Height + 5 * f15Height + 1), Convert.ToInt32(f20Width), Convert.ToInt32(f15Height)));
                }
                Brush bQuoteFill;
                if (BestSells[iro].PrevMarketData.OrderTopBuy >= BestSells[iro].LastMarketData.PriceLast) {
                  bQuoteFill = Brushes.HotPink;
                } else if (BestSells[iro].PrevMarketData.OrderTopSell <= BestSells[iro].LastMarketData.PriceLast) {
                  bQuoteFill = Brushes.Chartreuse;
                } else {
                  bQuoteFill = Brushes.WhiteSmoke;
                }
                bg.Graphics.DrawString(sLastPrice, fCur9, Brushes.WhiteSmoke,    new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastPSize.Width / 2),    Convert.ToSingle(2 * f05Height + 5 * f15Height + f15Height / 2 - sLastPSize.Height / 2 - 3 * sLastPSize.Height)));
                bg.Graphics.DrawString(sBookPrice, fCur9, Brushes.HotPink,       new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastBSize.Width / 2),    Convert.ToSingle(2 * f05Height + 5 * f15Height + f15Height / 2 - sLastPSize.Height / 2 - 2 * sLastPSize.Height)));
                bg.Graphics.DrawString(sBookPrice2, fCur9, Brushes.Chartreuse,   new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastBSize.Width / 2),    Convert.ToSingle(2 * f05Height + 5 * f15Height + f15Height / 2 - sLastPSize.Height / 2 - 2 * sLastPSize.Height)));
                bg.Graphics.DrawString(sMarket, fCur10, Brushes.WhiteSmoke,      new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sMarSize.Width / 2),      Convert.ToSingle(2 * f05Height + 5 * f15Height + f15Height / 2 - sMarSize.Height / 2 - 1 * sLastPSize.Height)));
                bg.Graphics.DrawString(sMarket2, fCur10, bQuoteFill,             new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sMarSize.Width / 2),      Convert.ToSingle(2 * f05Height + 5 * f15Height + f15Height / 2 - sMarSize.Height / 2 - 1 * sLastPSize.Height)));
                bg.Graphics.DrawString(sMarRank, fCur10, Brushes.WhiteSmoke,     new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sfMarketRank.Width / 2),  Convert.ToSingle(2 * f05Height + 5 * f15Height + f15Height / 2 - sMarSize.Height / 2 + 0 * sMarSize.Height)));
                bg.Graphics.DrawString(sTradeH, fCur10, Brushes.WhiteSmoke,      new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sfTradeH.Width / 2),      Convert.ToSingle(2 * f05Height + 5 * f15Height + f15Height / 2 - sMarSize.Height / 2 + 1 * sMarSize.Height)));
                bg.Graphics.DrawString(sValueChange, fCur10, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sfValueChange.Width / 2), Convert.ToSingle(2 * f05Height + 5 * f15Height + f15Height / 2 - sMarSize.Height / 2 + 2 * sMarSize.Height)));

              }
              #endregion
            }

            #region Row Balances
            //2*f20Height+ 2*f15Height 
            double dLastL = 0;
            if (Balances.Keys.Count > 0) {
              foreach (string b in Balances.Keys) {
                double w = fWidth * Balances[b].PercentOfTotal;  //        was f20Height + 3 * f15Height
                bg.Graphics.FillRectangle(Brushes.DarkGreen, new Rectangle(Convert.ToInt32(dLastL), Convert.ToInt32(f05Height + 4 * f15Height), Convert.ToInt32(w), Convert.ToInt32(f05Height)));
                bg.Graphics.DrawRectangle(Pens.White, new Rectangle(Convert.ToInt32(dLastL), Convert.ToInt32(f05Height + 4 * f15Height + 1), Convert.ToInt32(w), Convert.ToInt32(f05Height - 1)));
                dLastL = dLastL + w;
              }
              dLastL = 0;
              foreach (string b in Balances.Keys) {
                string sQ = b + " " + Balances[b].DBQuote.toStr4();
                SizeF szQ = bg.Graphics.MeasureString(sQ, fCur6);
                double w = fWidth * Balances[b].PercentOfTotal;
                double lLeft = dLastL + w / 2 - szQ.Width / 2;
                if (lLeft < 0) { lLeft = 0; }
                bg.Graphics.DrawString(sQ, fCur10, Brushes.White, new PointF(Convert.ToSingle(lLeft), Convert.ToSingle(f05Height + 4 * f15Height + (f05Height / 2) - szQ.Height / 2 * 3)));
                bg.Graphics.DrawString(Balances[b].BitcoinValue.toStr8(), fCur10, Brushes.White, new PointF(Convert.ToSingle(lLeft), Convert.ToSingle(f05Height + 4 * f15Height + (f05Height / 2) - szQ.Height / 2)));
                dLastL = dLastL + w;
              }
              string sT = TotalBTCValue.toStr8() + "BTC " + TotalUSDValue.toStr2() + "USD";
              SizeF szT = bg.Graphics.MeasureString(sT, fCur10);
              bg.Graphics.DrawString(sT, fCur10, Brushes.White, new PointF(Convert.ToSingle(fWidth - szT.Width), Convert.ToSingle(f05Height + 4 * f15Height + (f05Height / 2) + szT.Height / 4)));

            }
            #endregion

          } else if (iDisplayMode == 1) {

            for (Int32 iDR = 0; iDR <= 9; iDR++) {
              for (Int32 iDMC = 0; iDMC <= 9; iDMC++) {
                #region Row N
                if ((iDR * 10) + iDMC < AlphaMarkets.Count) {
                  string iro = AlphaMarkets.Keys[((iDR * 10) + iDMC)];
                  CurrencyPair acp = AlphaMarkets[iro].CurPair;
                  string sLastPrice = "";
                  string sMarket = "";
                  if (acp.BaseCurrency == "USDT") {
                    sMarket = acp.BaseCurrency + " " + acp.QuoteCurrency + " " + AlphaMarkets[iro].LastMarketData.PriceLast.toStr2();
                    sLastPrice = Convert.ToDouble(AlphaMarkets[iro].LastMarketData.PriceLast * 0.0069).toStr2() + " " + AlphaMarkets[iro].PerSecBuyPriceChangeAvg.toStr2();
                  } else {
                    sMarket = acp.BaseCurrency + " " + acp.QuoteCurrency + " " + AlphaMarkets[iro].LastMarketData.PriceLast.toStr8();
                    sLastPrice = Convert.ToDouble(AlphaMarkets[iro].LastMarketData.PriceLast * 0.0069).toStr8() + " " + AlphaMarkets[iro].PerSecBuyPriceChangeAvg.toStr8();
                  }
                  SizeF sMarSize = bg.Graphics.MeasureString(sMarket, fCur10);
                  SizeF sLastPSize = bg.Graphics.MeasureString(sLastPrice, fCur9);
                  if (AlphaMarkets[iro].Selected) {
                    //Brushes.MidnightBlue
                    Brush theColor = Brushes.DarkBlue;
                    if ((FocusedMarket != null) && (AlphaMarkets[iro].CurPair == FocusedMarket)) {
                      theColor = Brushes.MidnightBlue;
                    }
                    bg.Graphics.FillRectangle(theColor, new Rectangle(Convert.ToInt32(iDMC * f20Width / 2 + 1), Convert.ToInt32(f05Height + (iDR * f15Height / 2.5) + 1), Convert.ToInt32(f20Width / 2), Convert.ToInt32(f15Height / 2.5)));
                    bg.Graphics.DrawRectangle(Pens.Chartreuse, new Rectangle(Convert.ToInt32(iDMC * f20Width / 2 + 1), Convert.ToInt32(f05Height + (iDR * f15Height / 2.5) + 1), Convert.ToInt32(f20Width / 2), Convert.ToInt32(f15Height / 2.5)));
                  } else {
                    bg.Graphics.DrawRectangle(Pens.Chartreuse, new Rectangle(Convert.ToInt32(iDMC * f20Width / 2 + 1), Convert.ToInt32(f05Height + (iDR * f15Height / 2.5) + 1), Convert.ToInt32(f20Width / 2), Convert.ToInt32(f15Height / 2.5)));
                  }

                  bg.Graphics.DrawString(sLastPrice, fCur9, Brushes.Chartreuse, new PointF(Convert.ToSingle(iDMC * f20Width / 2 + f20Width / 4 - sLastPSize.Width / 2), Convert.ToSingle(f05Height + (iDR * f15Height / 2.5) + sLastPSize.Height / 2)));
                  bg.Graphics.DrawString(sMarket, fCur10, Brushes.Chartreuse, new PointF(Convert.ToSingle(iDMC * f20Width / 2 + f20Width / 4 - sMarSize.Width / 2), Convert.ToSingle(f05Height + (iDR * f15Height / 2.5) + 3 * sMarSize.Height / 2)));
                }
                #endregion
              }
            }

            for (Int32 iDMC = 0; iDMC < 5; iDMC++) {
              #region Row 1
              if (iDMC < BestSells.Count) {
                double iro = BestSells.Keys[BestSells.Count - 1 - (iDMC)];
                CurrencyPair acp = BestSells[iro].CurPair;
                //if ((DidTradeHistory) && (MarketDetailVisible)) {
                //  Dictionary<string, object> aCMDParams = new Dictionary<string, object>();
                //  aCMDParams["CurrencyPair"] = acp;
                //  lPoloQue.ChkAdd(new appCmd("TradingGetTrades", aCMDParams));
                //}
                string sLastPrice = ""; string sMarket2 = "";
                string sBookPrice = ""; string sBookPrice2 = "";
                string sMarket = "";
                if (acp.BaseCurrency == "USDT") {
                  sMarket = acp.BaseCurrency + " " + acp.QuoteCurrency + " ".PadLeft(BestSells[iro].LastMarketData.PriceLast.toStr2().Length, ' ') + "  " + BestSells[iro].PerSecBuyPriceChangeAvg.toStr8();
                  sMarket2 = (" ".PadLeft(acp.BaseCurrency.Length + 1 + acp.QuoteCurrency.Length + 1, ' ')) + BestSells[iro].LastMarketData.PriceLast.toStr2();
                  sLastPrice = BestSells[iro].LowestSell.toStr2() + " " + Convert.ToDouble(BestSells[iro].LastMarketData.PriceLast * 0.0069).toStr2() + " " + BestSells[iro].HighestBuy.toStr2();
                  sBookPrice = BestSells[iro].LastMarketData.OrderTopBuy.toStr2();
                  sBookPrice2 = (" ".PadLeft(BestSells[iro].LastMarketData.OrderTopBuy.toStr2().Length + 1, ' ')) + " " + BestSells[iro].LastMarketData.OrderTopSell.toStr2();

                } else {
                  sMarket = acp.BaseCurrency + " " + acp.QuoteCurrency + " ".PadLeft(BestSells[iro].LastMarketData.PriceLast.toStr8().Length, ' ') + "  " + BestSells[iro].PerSecBuyPriceChangeAvg.toStr8();
                  sMarket2 = (" ".PadLeft(acp.BaseCurrency.Length + 1 + acp.QuoteCurrency.Length + 1, ' ')) + BestSells[iro].LastMarketData.PriceLast.toStr8();
                  sLastPrice = BestSells[iro].LowestSell.toStr8() + " " + Convert.ToDouble(BestSells[iro].LastMarketData.PriceLast * 0.0069).toStr8() + " " + BestSells[iro].HighestBuy.toStr8();
                  sBookPrice = BestSells[iro].LastMarketData.OrderTopBuy.toStr8();
                  sBookPrice2 = (" ".PadLeft(BestSells[iro].LastMarketData.OrderTopBuy.toStr8().Length + 1)) + " " + BestSells[iro].LastMarketData.OrderTopSell.toStr8();

                }
                string sMarRank = "tic " + BestSells[iro].PerSecSellTicAvg.toStr2() + " " + BestSells[iro].FiveSecSellTicAvg.toStr2() + " B " + BestSells[iro].BuyTickerCount.ToString() + " S " + BestSells[iro].SellTickerCount.ToString();
                string sTradeH = "" + /*(BestSells[iro].tradeHistoryTrades.Count > 0 ?*/ "Avg: " + BestSells[iro].AvgPricePaid.toStr8() + " -> " + Convert.ToDouble(BestSells[iro].AvgPricePaid * 1.0069).toStr8() + " "; //: "");
                string sValueChange = (Balances.Keys.Contains(acp.QuoteCurrency) ? Balances[acp.QuoteCurrency].DBQuote.toStr4() + " " + Convert.ToDouble(Balances[acp.QuoteCurrency].BitcoinValue - (Balances[acp.QuoteCurrency].DBQuote * (BestSells[iro].AvgPricePaid * 1.0069))).toStr8() : "") + " " + Convert.ToDouble(BestSells[iro].LastMarketData.PriceLast - (BestSells[iro].AvgPricePaid * 1.0069)).toStr8();
                SizeF sfValueChange = bg.Graphics.MeasureString(sValueChange, fCur10);
                SizeF sfTradeH = bg.Graphics.MeasureString(sTradeH, fCur10);
                SizeF sMarSize = bg.Graphics.MeasureString(sMarket, fCur10);
                SizeF sfMarketRank = bg.Graphics.MeasureString(sMarRank, fCur10);
                SizeF sLastPSize = bg.Graphics.MeasureString(sLastPrice, fCur9);
                SizeF sLastBSize = bg.Graphics.MeasureString(sBookPrice2, fCur9);
                if (BestSells[iro].Selected) {
                  Brush theColor = Brushes.DarkBlue;
                  if ((FocusedMarket != null) && (BestSells[iro].CurPair == FocusedMarket)) {
                    theColor = Brushes.MidnightBlue;
                  }                      ///       f05Height + 4 * f15Height
                  bg.Graphics.FillRectangle(theColor, new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(2 * f05Height + 4 * f15Height + 1), Convert.ToInt32(f20Width), Convert.ToInt32(f15Height)));
                  //bg.Graphics.FillRectangle(Brushes.Black, new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(2 * f20Height + 2 * f15Height + 1), Convert.ToInt32(14), Convert.ToInt32(14)));
                  bg.Graphics.DrawString("X", fCur10, Brushes.Red, new PointF(Convert.ToSingle(iDMC * f20Width + 1), Convert.ToSingle(2 * f05Height + 4 * f15Height + 1)));
                } else {
                  bg.Graphics.DrawRectangle(Pens.Chartreuse, new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(2 * f05Height + 4 * f15Height + 1), Convert.ToInt32(f20Width), Convert.ToInt32(f15Height)));
                }
                Brush bQuoteFill;
                if (BestSells[iro].PrevMarketData.OrderTopBuy >= BestSells[iro].LastMarketData.PriceLast) {
                  bQuoteFill = Brushes.HotPink;
                } else if (BestSells[iro].PrevMarketData.OrderTopSell <= BestSells[iro].LastMarketData.PriceLast) {
                  bQuoteFill = Brushes.Chartreuse;
                } else {
                  bQuoteFill = Brushes.WhiteSmoke;
                }
                bg.Graphics.DrawString(sLastPrice, fCur9, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastPSize.Width / 2), Convert.ToSingle(2 * f05Height + 4 * f15Height + f15Height / 2 - sLastPSize.Height / 2 - 5 * sLastPSize.Height / 2)));
                bg.Graphics.DrawString(sBookPrice, fCur9, Brushes.HotPink, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastBSize.Width / 2), Convert.ToSingle(2 * f05Height + 4 * f15Height + f15Height / 2 - sLastPSize.Height / 2 - 3 * sLastPSize.Height / 2)));
                bg.Graphics.DrawString(sBookPrice2, fCur9, Brushes.Chartreuse, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastBSize.Width / 2), Convert.ToSingle(2 * f05Height + 4 * f15Height + f15Height / 2 - sLastPSize.Height / 2 - 3 * sLastPSize.Height / 2)));
                bg.Graphics.DrawString(sMarket, fCur10, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sMarSize.Width / 2), Convert.ToSingle(2 * f05Height + 4 * f15Height + f15Height / 2 - sMarSize.Height / 2 - 1 * sMarSize.Height / 2)));
                bg.Graphics.DrawString(sMarket2, fCur10, bQuoteFill, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sMarSize.Width / 2), Convert.ToSingle(2 * f05Height + 4 * f15Height + f15Height / 2 - sMarSize.Height / 2 - 1 * sMarSize.Height / 2)));
                bg.Graphics.DrawString(sMarRank, fCur10, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sfMarketRank.Width / 2), Convert.ToSingle(2 * f05Height + 4 * f15Height + f15Height / 2 - sMarSize.Height / 2 + 1 * sMarSize.Height / 2)));
                bg.Graphics.DrawString(sTradeH, fCur10, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sfTradeH.Width / 2), Convert.ToSingle(2 * f05Height + 4 * f15Height + f15Height / 2 - sMarSize.Height / 2 + 3 * sMarSize.Height / 2)));
                bg.Graphics.DrawString(sValueChange, fCur10, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sfValueChange.Width / 2), Convert.ToSingle(2 * f05Height + 4 * f15Height + f15Height / 2 - sMarSize.Height / 2 + 5 * sMarSize.Height / 2)));

              }
              #endregion
              #region Row 2
              if (iDMC + 5 < BestSells.Count) {
                double iro = BestSells.Keys[BestSells.Count - (iDMC + 6)];
                CurrencyPair acp = BestSells[iro].CurPair;
                //if (DidTradeHistory) {
                //  Dictionary<string, object> aCMDParams = new Dictionary<string, object>();
                //  aCMDParams["CurrencyPair"] = acp;
                //  lPoloQue.ChkAdd(new appCmd("TradingGetTrades", aCMDParams));
                //}
                string sLastPrice = ""; string sMarket2 = "";
                string sBookPrice = ""; string sBookPrice2 = "";
                string sMarket = "";
                if (acp.BaseCurrency == "USDT") {
                  sMarket = acp.BaseCurrency + " " + acp.QuoteCurrency + " ".PadLeft(BestSells[iro].LastMarketData.PriceLast.toStr2().Length, ' ') + "  " + BestSells[iro].PerSecBuyPriceChangeAvg.toStr8();
                  sMarket2 = (" ".PadLeft(acp.BaseCurrency.Length + 1 + acp.QuoteCurrency.Length + 1, ' ')) + BestSells[iro].LastMarketData.PriceLast.toStr2();
                  sLastPrice = BestSells[iro].LowestSell.toStr2() + " " + Convert.ToDouble(BestSells[iro].LastMarketData.PriceLast * 0.0069).toStr2() + " " + BestSells[iro].HighestBuy.toStr2();
                  sBookPrice = BestSells[iro].LastMarketData.OrderTopBuy.toStr2();
                  sBookPrice2 = (" ".PadLeft(BestSells[iro].LastMarketData.OrderTopBuy.toStr2().Length + 1, ' ')) + " " + BestSells[iro].LastMarketData.OrderTopSell.toStr2();

                } else {
                  sMarket = acp.BaseCurrency + " " + acp.QuoteCurrency + " ".PadLeft(BestSells[iro].LastMarketData.PriceLast.toStr8().Length, ' ') + "  " + BestSells[iro].PerSecBuyPriceChangeAvg.toStr8();
                  sMarket2 = (" ".PadLeft(acp.BaseCurrency.Length + 1 + acp.QuoteCurrency.Length + 1, ' ')) + BestSells[iro].LastMarketData.PriceLast.toStr8();
                  sLastPrice = BestSells[iro].LowestSell.toStr8() + " " + Convert.ToDouble(BestSells[iro].LastMarketData.PriceLast * 0.0069).toStr8() + " " + BestSells[iro].HighestBuy.toStr8();
                  sBookPrice = BestSells[iro].LastMarketData.OrderTopBuy.toStr8();
                  sBookPrice2 = (" ".PadLeft(BestSells[iro].LastMarketData.OrderTopBuy.toStr8().Length + 1)) + " " + BestSells[iro].LastMarketData.OrderTopSell.toStr8();

                }
                string sMarRank = "Tic " + BestSells[iro].PerSecSellTicAvg.toStr2() + " " + BestSells[iro].FiveSecSellTicAvg.toStr2() + " B " + BestSells[iro].BuyTickerCount.ToString() + " S " + BestSells[iro].SellTickerCount.ToString();
                string sTradeH = "" + /*(BestSells[iro].tradeHistoryTrades.Count > 0 ?*/ "Avg: " + BestSells[iro].AvgPricePaid.toStr8() + " -> " + Convert.ToDouble(BestSells[iro].AvgPricePaid * 1.0069).toStr8() + " "; //: "");
                string sValueChange = (Balances.Keys.Contains(acp.QuoteCurrency) ? Balances[acp.QuoteCurrency].DBQuote.toStr4() + " " + Convert.ToDouble(Balances[acp.QuoteCurrency].BitcoinValue - (Balances[acp.QuoteCurrency].DBQuote * (BestSells[iro].AvgPricePaid * 1.0069))).toStr8() : "") + " " + Convert.ToDouble(BestSells[iro].LastMarketData.PriceLast - (BestSells[iro].AvgPricePaid * 1.0069)).toStr8();
                SizeF sfValueChange = bg.Graphics.MeasureString(sValueChange, fCur10);
                SizeF sfTradeH = bg.Graphics.MeasureString(sTradeH, fCur10);
                SizeF sfMarketRank = bg.Graphics.MeasureString(sMarRank, fCur10);
                SizeF sLastPSize = bg.Graphics.MeasureString(sLastPrice, fCur9);
                SizeF sMarSize = bg.Graphics.MeasureString(sMarket, fCur10);
                SizeF sLastBSize = bg.Graphics.MeasureString(sBookPrice2, fCur9);
                if (BestSells[iro].Selected) {
                  Brush theColor = Brushes.DarkBlue;
                  if ((FocusedMarket != null) && (BestSells[iro].CurPair == FocusedMarket)) {
                    theColor = Brushes.MidnightBlue;
                  }
                  bg.Graphics.FillRectangle(theColor, new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(2 * f05Height + 5 * f15Height + 1), Convert.ToInt32(f20Width), Convert.ToInt32(f15Height)));
                  bg.Graphics.DrawString("X", fCur10, Brushes.Red, new PointF(Convert.ToSingle(iDMC * f20Width), Convert.ToSingle(2 * f05Height + 5 * f15Height)));
                } else {
                  bg.Graphics.DrawRectangle(Pens.Chartreuse, new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(2 * f05Height + 5 * f15Height + 1), Convert.ToInt32(f20Width), Convert.ToInt32(f15Height)));
                }
                Brush bQuoteFill;
                if (BestSells[iro].PrevMarketData.OrderTopBuy >= BestSells[iro].LastMarketData.PriceLast) {
                  bQuoteFill = Brushes.HotPink;
                } else if (BestSells[iro].PrevMarketData.OrderTopSell <= BestSells[iro].LastMarketData.PriceLast) {
                  bQuoteFill = Brushes.Chartreuse;
                } else {
                  bQuoteFill = Brushes.WhiteSmoke;
                }
                bg.Graphics.DrawString(sLastPrice, fCur9, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastPSize.Width / 2), Convert.ToSingle(2 * f05Height + 5 * f15Height + f15Height / 2 - sLastPSize.Height / 2 - 3 * sLastPSize.Height)));
                bg.Graphics.DrawString(sBookPrice, fCur9, Brushes.HotPink, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastBSize.Width / 2), Convert.ToSingle(2 * f05Height + 5 * f15Height + f15Height / 2 - sLastPSize.Height / 2 - 2 * sLastPSize.Height)));
                bg.Graphics.DrawString(sBookPrice2, fCur9, Brushes.Chartreuse, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastBSize.Width / 2), Convert.ToSingle(2 * f05Height + 5 * f15Height + f15Height / 2 - sLastPSize.Height / 2 - 2 * sLastPSize.Height)));
                bg.Graphics.DrawString(sMarket, fCur10, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sMarSize.Width / 2), Convert.ToSingle(2 * f05Height + 5 * f15Height + f15Height / 2 - sMarSize.Height / 2 - 1 * sLastPSize.Height)));
                bg.Graphics.DrawString(sMarket2, fCur10, bQuoteFill, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sMarSize.Width / 2), Convert.ToSingle(2 * f05Height + 5 * f15Height + f15Height / 2 - sMarSize.Height / 2 - 1 * sLastPSize.Height)));
                bg.Graphics.DrawString(sMarRank, fCur10, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sfMarketRank.Width / 2), Convert.ToSingle(2 * f05Height + 5 * f15Height + f15Height / 2 - sMarSize.Height / 2 + 0 * sMarSize.Height)));
                bg.Graphics.DrawString(sTradeH, fCur10, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sfTradeH.Width / 2), Convert.ToSingle(2 * f05Height + 5 * f15Height + f15Height / 2 - sMarSize.Height / 2 + 1 * sMarSize.Height)));
                bg.Graphics.DrawString(sValueChange, fCur10, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sfValueChange.Width / 2), Convert.ToSingle(2 * f05Height + 5 * f15Height + f15Height / 2 - sMarSize.Height / 2 + 2 * sMarSize.Height)));

              }
              #endregion
            }

            #region Row Balances
            //2*f20Height+ 2*f15Height 
            double dLastL = 0;
            if (Balances.Keys.Count > 0) {
              foreach (string b in Balances.Keys) {
                double w = fWidth * Balances[b].PercentOfTotal;  //        was f20Height + 3 * f15Height
                bg.Graphics.FillRectangle(Brushes.DarkGreen, new Rectangle(Convert.ToInt32(dLastL), Convert.ToInt32(f05Height + 4 * f15Height), Convert.ToInt32(w), Convert.ToInt32(f05Height)));
                bg.Graphics.DrawRectangle(Pens.White, new Rectangle(Convert.ToInt32(dLastL), Convert.ToInt32(f05Height + 4 * f15Height), Convert.ToInt32(w), Convert.ToInt32(f05Height)));
                dLastL = dLastL + w;
              }
              dLastL = 0;
              foreach (string b in Balances.Keys) {
                string sQ = b + " " + Balances[b].DBQuote.toStr4();
                SizeF szQ = bg.Graphics.MeasureString(sQ, fCur6);
                double w = fWidth * Balances[b].PercentOfTotal;
                double lLeft = dLastL + w / 2 - szQ.Width / 2;
                if (lLeft < 0) { lLeft = 0; }
                bg.Graphics.DrawString(sQ, fCur10, Brushes.White, new PointF(Convert.ToSingle(lLeft), Convert.ToSingle(f05Height + 4 * f15Height + (f05Height / 2) - szQ.Height / 2 * 3)));
                bg.Graphics.DrawString(Balances[b].BitcoinValue.toStr8(), fCur10, Brushes.White, new PointF(Convert.ToSingle(lLeft), Convert.ToSingle(f05Height + 4 * f15Height + (f05Height / 2) - szQ.Height / 2)));
                dLastL = dLastL + w;
              }
              string sT = TotalBTCValue.toStr8() + "BTC " + TotalUSDValue.toStr2() + "USD";
              SizeF szT = bg.Graphics.MeasureString(sT, fCur10);
              bg.Graphics.DrawString(sT, fCur10, Brushes.White, new PointF(Convert.ToSingle(fWidth - szT.Width), Convert.ToSingle(f05Height + 4 * f15Height + (f05Height / 2) + szT.Height / 4)));

            }
            #endregion

          } else if (iDisplayMode == 2) {

            #region Draw Circles

            Int32 iLeft = Convert.ToInt32(0);
            Int32 iTop = Convert.ToInt32(f05Height);
            Int32 iWidth = Convert.ToInt32(5 * f20Width);
            Int32 iHeight = Convert.ToInt32(f15Height * 4 );
            bg.Graphics.FillRectangle(Brushes.Black, new Rectangle(iLeft, iTop, iWidth, iHeight));
            SizeF sfText = bg.Graphics.MeasureString("1.00001111 ", fCur10);
            Int32 iSellNo = 2;
            Double BTCScale = 100;
            Boolean hasBTC = false;
            double cr = 1;
            Int32 invCount = 0;
            foreach (string b in Balances.Keys) {
              if (b == "BTC") {
                cr = Balances[b].CircRadius * BTCScale;
                hasBTC = true;
              } else {
                invCount = invCount + 1;
              }
            }

            if (hasBTC) {
              bg.Graphics.FillEllipse(Brushes.DarkGoldenrod, new Rectangle(
                 Convert.ToInt32(iWidth / 2 - cr),
                 Convert.ToInt32(2 * iHeight / 3 - cr),
                 Convert.ToInt32(cr * 2),
                 Convert.ToInt32(cr * 2))
                );
              bg.Graphics.DrawEllipse(Pens.White, new Rectangle(
                 Convert.ToInt32(iWidth / 2 - cr),
                 Convert.ToInt32(2 * iHeight / 3 - cr),
                 Convert.ToInt32(cr * 2),
                 Convert.ToInt32(cr * 2))
              );

              string sQ = "BTC " + Balances["BTC"].DBQuote.toStr4();
              SizeF szQ = bg.Graphics.MeasureString(sQ, fCur6);
              bg.Graphics.DrawString(sQ, fCur10, Brushes.White,
                new PointF(Convert.ToSingle(iWidth / 2 - szQ.Width / 2), Convert.ToSingle(2 * iHeight / 3 - szQ.Height)));
              bg.Graphics.DrawString(Balances["BTC"].BitcoinValue.toStr8(), fCur10, Brushes.White,
                new PointF(Convert.ToSingle(iWidth / 2 - szQ.Width / 2), Convert.ToSingle(2 * iHeight / 3 + szQ.Height)));
            } else {
              cr = 60;
            }     

            if (invCount > 0) {
              double theta = Math.PI*(360 / invCount)/ 180;
              double xtheta = Math.PI;
              Int32 xCount = 0;  
              foreach (string b in Balances.Keys) {
                if (b != "BTC") {
                  CurrencyPair aQuoteMarket = null;
                  foreach (double iro in BestSells.Keys) {
                    CurrencyPair acp = BestSells[iro].CurPair;
                    if (acp.QuoteCurrency == b) {
                      aQuoteMarket = acp;
                    }
                  }
                  xCount = xCount + 1;
                  double ir = Balances[b].CircRadius * BTCScale;
                  double CirTanX = Math.Cos(xtheta) * cr;
                  double CirTanY = Math.Sin(xtheta) * cr;
                  double InvCirX = CirTanX + Math.Cos(xtheta) * ir;
                  double InvCirY = CirTanY + Math.Sin(xtheta) * ir;

                  bg.Graphics.FillEllipse(Brushes.DarkGoldenrod, new Rectangle(
                      Convert.ToInt32(iWidth / 2 + InvCirX - ir),
                      Convert.ToInt32(2 * iHeight / 3 + InvCirY - ir),
                      Convert.ToInt32(ir * 2),
                      Convert.ToInt32(ir * 2))
                  );

                  bg.Graphics.DrawEllipse(Pens.White, new Rectangle(
                      Convert.ToInt32(iWidth / 2 + InvCirX - ir),
                      Convert.ToInt32(2 * iHeight / 3 + InvCirY - ir),
                      Convert.ToInt32(ir * 2),
                      Convert.ToInt32(ir * 2))
                  );

                  string sQ2 = b +" "+ Balances[b].DBQuote.toStr4();
                  SizeF szQ2 = bg.Graphics.MeasureString(sQ2, fCur6);

                  if (aQuoteMarket != null) {
                    string sQ1 = MarketsOfInterest[aQuoteMarket].HoldAmount.toStr4() + " " +
                      MarketsOfInterest[aQuoteMarket].SellThreshold.toStr4() + " " + 
                      MarketsOfInterest[aQuoteMarket].BuyThreshold.toStr4();
                    double HoldSell = Convert.ToDouble( MarketsOfInterest[aQuoteMarket].HoldAmount + MarketsOfInterest[aQuoteMarket].SellThreshold);
                    double HoldBuy = Convert.ToDouble(MarketsOfInterest[aQuoteMarket].HoldAmount - MarketsOfInterest[aQuoteMarket].BuyThreshold);

                    bg.Graphics.DrawString(sQ1, fCur10, Brushes.White,
                        new PointF(Convert.ToSingle(iWidth / 2 + InvCirX - szQ2.Width / 2), Convert.ToSingle(2 * iHeight / 3 + InvCirY - (szQ2.Height * 2))));

                    double SellWeightDiff = (Balances[b].DBQuote *  MarketsOfInterest[aQuoteMarket].LastMarketData.OrderTopSell) - (HoldSell);
                    double BuyWeightDiff = (Balances[b].DBQuote * MarketsOfInterest[aQuoteMarket].LastMarketData.OrderTopSell) - (HoldBuy);
                    

                    if (SellWeightDiff >= 0) {
                      double TargetSellBtcVol = (Balances[b].DBQuote * MarketsOfInterest[aQuoteMarket].LastMarketData.OrderTopSell) - Convert.ToDouble(MarketsOfInterest[aQuoteMarket].HoldAmount);
                      double TargetSellQuoteVol = TargetSellBtcVol / MarketsOfInterest[aQuoteMarket].LastMarketData.OrderTopSell;

                      if (MarketsOfInterest[aQuoteMarket].GoHold) {
                        bg.Graphics.DrawString(TargetSellQuoteVol.toStr4() + " @ " + MarketsOfInterest[aQuoteMarket].LastMarketData.OrderTopSell.toStr8() + " = " + TargetSellBtcVol.toStr8(), fCur10, Brushes.White,
                          new PointF(Convert.ToSingle(iWidth / 2 + InvCirX - szQ2.Width / 2), Convert.ToSingle(2 * iHeight / 3 + InvCirY - (szQ2.Height * (12 + xCount)))));
                      }
                    } else {
                      double TargetSellPrice = HoldSell / Balances[b].DBQuote;
                      double TargetSellQuoteVol = Convert.ToDouble(MarketsOfInterest[aQuoteMarket].SellThreshold) / TargetSellPrice;
                      if (MarketsOfInterest[aQuoteMarket].GoHold) {
                        bg.Graphics.DrawString(TargetSellQuoteVol.toStr4() + " @ " + TargetSellPrice.toStr8() + " = " + (TargetSellPrice * TargetSellQuoteVol).toStr8(), fCur10, Brushes.White,
                          new PointF(Convert.ToSingle(iWidth / 2 + InvCirX - szQ2.Width / 2), Convert.ToSingle(2 * iHeight / 3 + InvCirY - (szQ2.Height * (12 + xCount)))));
                      }
                    }

                    if (BuyWeightDiff <= 0) {  // value less than thresh

                      double TargetBuyBtcVol = Convert.ToDouble(MarketsOfInterest[aQuoteMarket].HoldAmount) - (Balances[b].DBQuote * MarketsOfInterest[aQuoteMarket].LastMarketData.OrderTopBuy);
                      double TargetBuyQuoteVol = TargetBuyBtcVol / MarketsOfInterest[aQuoteMarket].LastMarketData.OrderTopBuy;

                      if (MarketsOfInterest[aQuoteMarket].GoHold) {
                        bg.Graphics.DrawString(TargetBuyQuoteVol.toStr4() + " @ " + MarketsOfInterest[aQuoteMarket].LastMarketData.OrderTopSell.toStr8() + " = " + TargetBuyBtcVol.toStr8(), fCur10, Brushes.White,
                          new PointF(Convert.ToSingle(iWidth / 2 + InvCirX - szQ2.Width / 2), Convert.ToSingle(2 * iHeight / 3 + InvCirY + (szQ2.Height * (12 + xCount)))));
                      }
                    } else {
                      double TargetBuyPrice = HoldBuy / Balances[b].DBQuote;
                      double TargetBuyQuoteVol = Convert.ToDouble(MarketsOfInterest[aQuoteMarket].BuyThreshold) / TargetBuyPrice;
                      if (MarketsOfInterest[aQuoteMarket].GoHold) {
                        bg.Graphics.DrawString(TargetBuyQuoteVol.toStr4() + " @ " + TargetBuyPrice.toStr8() + " = " + (TargetBuyPrice * TargetBuyQuoteVol).toStr8(), fCur10, Brushes.White,
                          new PointF(Convert.ToSingle(iWidth / 2 + InvCirX - szQ2.Width / 2), Convert.ToSingle(2 * iHeight / 3 + InvCirY + (szQ2.Height * (12 + xCount)))));
                      }
                    }


                  }
                  bg.Graphics.DrawString(sQ2, fCur10, Brushes.White,
                      new PointF(Convert.ToSingle(iWidth / 2 + InvCirX - szQ2.Width / 2), Convert.ToSingle(2 * iHeight / 3 + InvCirY - szQ2.Height)));
                  bg.Graphics.DrawString(Balances[b].BitcoinValue.toStr8(), fCur10, Brushes.White,
                      new PointF(Convert.ToSingle(iWidth / 2 + InvCirX - szQ2.Width / 2), Convert.ToSingle(2 * iHeight / 3 + InvCirY + szQ2.Height)));



                  xtheta = xtheta - theta;
                }
              }
            }
            #endregion 

            #region Row Balances
            //2*f20Height+ 2*f15Height 
            double dLastL = 0;
            if (Balances.Keys.Count > 0) {
              foreach (string b in Balances.Keys) {
                double w = fWidth * Balances[b].PercentOfTotal;  //        was f20Height + 3 * f15Height
                bg.Graphics.FillRectangle(Brushes.DarkGreen, new Rectangle(Convert.ToInt32(dLastL), Convert.ToInt32(f05Height + 4 * f15Height), Convert.ToInt32(w), Convert.ToInt32(f05Height)));
                bg.Graphics.DrawRectangle(Pens.White, new Rectangle(Convert.ToInt32(dLastL), Convert.ToInt32(f05Height + 4 * f15Height), Convert.ToInt32(w), Convert.ToInt32(f05Height)));
                dLastL = dLastL + w;
              }
              dLastL = 0;
              foreach (string b in Balances.Keys) {
                string sQ = b + " " + Balances[b].DBQuote.toStr4();
                SizeF szQ = bg.Graphics.MeasureString(sQ, fCur6);
                double w = fWidth * Balances[b].PercentOfTotal;
                double lLeft = dLastL + w / 2 - szQ.Width / 2;
                if (lLeft < 0) { lLeft = 0; }
                bg.Graphics.DrawString(sQ, fCur10, Brushes.White, new PointF(Convert.ToSingle(lLeft), Convert.ToSingle(f05Height + 4 * f15Height + (f05Height / 2) - szQ.Height / 2 * 3)));
                bg.Graphics.DrawString(Balances[b].BitcoinValue.toStr8(), fCur10, Brushes.White, new PointF(Convert.ToSingle(lLeft), Convert.ToSingle(f05Height + 4 * f15Height + (f05Height / 2) - szQ.Height / 2)));
                dLastL = dLastL + w;
              }
              string sT = TotalBTCValue.toStr8() + "BTC " + TotalUSDValue.toStr2() + "USD";
              SizeF szT = bg.Graphics.MeasureString(sT, fCur10);
              bg.Graphics.DrawString(sT, fCur10, Brushes.White, new PointF(Convert.ToSingle(fWidth - szT.Width), Convert.ToSingle(f05Height + 4 * f15Height + (f05Height / 2) + szT.Height / 4)));

            }
            #endregion

            for (Int32 iDMC = 0; iDMC < 5; iDMC++) {
              #region Row 1
              if (iDMC < BestSells.Count) {
                double iro = BestSells.Keys[BestSells.Count - 1 - (iDMC)];
                CurrencyPair acp = BestSells[iro].CurPair;
               // if ((DidTradeHistory) && (MarketDetailVisible)) {
                //  Dictionary<string, object> aCMDParams = new Dictionary<string, object>();
               //   aCMDParams["CurrencyPair"] = acp;
               //   lPoloQue.ChkAdd(new appCmd("TradingGetTrades", aCMDParams));
               // }
                string sLastPrice = ""; string sMarket2 = "";
                string sBookPrice = ""; string sBookPrice2 = "";
                string sMarket = "";
                if (acp.BaseCurrency == "USDT") {
                  sMarket = acp.BaseCurrency + " " + acp.QuoteCurrency + " ".PadLeft(BestSells[iro].LastMarketData.PriceLast.toStr2().Length, ' ') + "  " + BestSells[iro].PerSecBuyPriceChangeAvg.toStr8();
                  sMarket2 = (" ".PadLeft(acp.BaseCurrency.Length + 1 + acp.QuoteCurrency.Length + 1, ' ')) + BestSells[iro].LastMarketData.PriceLast.toStr2();
                  sLastPrice = BestSells[iro].LowestSell.toStr2() + " " + Convert.ToDouble(BestSells[iro].LastMarketData.PriceLast * 0.0069).toStr2() + " " + BestSells[iro].HighestBuy.toStr2();
                  sBookPrice = BestSells[iro].LastMarketData.OrderTopBuy.toStr2();
                  sBookPrice2 = (" ".PadLeft(BestSells[iro].LastMarketData.OrderTopBuy.toStr2().Length + 1, ' ')) + " " + BestSells[iro].LastMarketData.OrderTopSell.toStr2();

                } else {
                  sMarket = acp.BaseCurrency + " " + acp.QuoteCurrency + " ".PadLeft(BestSells[iro].LastMarketData.PriceLast.toStr8().Length, ' ') + "  " + BestSells[iro].PerSecBuyPriceChangeAvg.toStr8();
                  sMarket2 = (" ".PadLeft(acp.BaseCurrency.Length + 1 + acp.QuoteCurrency.Length + 1, ' ')) + BestSells[iro].LastMarketData.PriceLast.toStr8();
                  sLastPrice = BestSells[iro].LowestSell.toStr8() + " " + Convert.ToDouble(BestSells[iro].LastMarketData.PriceLast * 0.0069).toStr8() + " " + BestSells[iro].HighestBuy.toStr8();
                  sBookPrice = BestSells[iro].LastMarketData.OrderTopBuy.toStr8();
                  sBookPrice2 = (" ".PadLeft(BestSells[iro].LastMarketData.OrderTopBuy.toStr8().Length + 1)) + " " + BestSells[iro].LastMarketData.OrderTopSell.toStr8();

                }
                string sMarRank = "tic " + BestSells[iro].PerSecSellTicAvg.toStr2() + " " + BestSells[iro].FiveSecSellTicAvg.toStr2() + " B " + BestSells[iro].BuyTickerCount.ToString() + " S " + BestSells[iro].SellTickerCount.ToString();
                string sTradeH = "" + /*(BestSells[iro].tradeHistoryTrades.Count > 0 ?*/ "Avg: " + BestSells[iro].AvgPricePaid.toStr8() + " -> " + Convert.ToDouble(BestSells[iro].AvgPricePaid * 1.0069).toStr8() + " "; //: "");
                string sValueChange = (Balances.Keys.Contains(acp.QuoteCurrency) ? Balances[acp.QuoteCurrency].DBQuote.toStr4() + " " + Convert.ToDouble(Balances[acp.QuoteCurrency].BitcoinValue - (Balances[acp.QuoteCurrency].DBQuote * (BestSells[iro].AvgPricePaid * 1.0069))).toStr8() : "") + " " + Convert.ToDouble(BestSells[iro].LastMarketData.PriceLast - (BestSells[iro].AvgPricePaid * 1.0069)).toStr8();
                SizeF sfValueChange = bg.Graphics.MeasureString(sValueChange, fCur10);
                SizeF sfTradeH = bg.Graphics.MeasureString(sTradeH, fCur10);
                SizeF sMarSize = bg.Graphics.MeasureString(sMarket, fCur10);
                SizeF sfMarketRank = bg.Graphics.MeasureString(sMarRank, fCur10);
                SizeF sLastPSize = bg.Graphics.MeasureString(sLastPrice, fCur9);
                SizeF sLastBSize = bg.Graphics.MeasureString(sBookPrice2, fCur9);
                if (BestSells[iro].Selected) {
                  Brush theColor = Brushes.DarkBlue;
                  if ((FocusedMarket != null) && (BestSells[iro].CurPair == FocusedMarket)) {
                    theColor = Brushes.MidnightBlue;
                  }                      ///       f05Height + 4 * f15Height
                  bg.Graphics.FillRectangle(theColor, new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(2 * f05Height + 4 * f15Height + 1), Convert.ToInt32(f20Width), Convert.ToInt32(f15Height)));
                  //bg.Graphics.FillRectangle(Brushes.Black, new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(2 * f20Height + 2 * f15Height + 1), Convert.ToInt32(14), Convert.ToInt32(14)));
                  bg.Graphics.DrawString("X", fCur10, Brushes.Red, new PointF(Convert.ToSingle(iDMC * f20Width + 1), Convert.ToSingle(2 * f05Height + 4 * f15Height + 1)));
                } else {
                  bg.Graphics.DrawRectangle(Pens.Chartreuse, new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(2 * f05Height + 4 * f15Height + 1), Convert.ToInt32(f20Width), Convert.ToInt32(f15Height)));
                }
                Brush bQuoteFill;
                if (BestSells[iro].PrevMarketData.OrderTopBuy >= BestSells[iro].LastMarketData.PriceLast) {
                  bQuoteFill = Brushes.HotPink;
                } else if (BestSells[iro].PrevMarketData.OrderTopSell <= BestSells[iro].LastMarketData.PriceLast) {
                  bQuoteFill = Brushes.Chartreuse;
                } else {
                  bQuoteFill = Brushes.WhiteSmoke;
                }
                bg.Graphics.DrawString(sLastPrice, fCur9, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastPSize.Width / 2), Convert.ToSingle(2 * f05Height + 4 * f15Height + f15Height / 2 - sLastPSize.Height / 2 - 5 * sLastPSize.Height / 2)));
                bg.Graphics.DrawString(sBookPrice, fCur9, Brushes.HotPink, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastBSize.Width / 2), Convert.ToSingle(2 * f05Height + 4 * f15Height + f15Height / 2 - sLastPSize.Height / 2 - 3 * sLastPSize.Height / 2)));
                bg.Graphics.DrawString(sBookPrice2, fCur9, Brushes.Chartreuse, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastBSize.Width / 2), Convert.ToSingle(2 * f05Height + 4 * f15Height + f15Height / 2 - sLastPSize.Height / 2 - 3 * sLastPSize.Height / 2)));
                bg.Graphics.DrawString(sMarket, fCur10, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sMarSize.Width / 2), Convert.ToSingle(2 * f05Height + 4 * f15Height + f15Height / 2 - sMarSize.Height / 2 - 1 * sMarSize.Height / 2)));
                bg.Graphics.DrawString(sMarket2, fCur10, bQuoteFill, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sMarSize.Width / 2), Convert.ToSingle(2 * f05Height + 4 * f15Height + f15Height / 2 - sMarSize.Height / 2 - 1 * sMarSize.Height / 2)));
                bg.Graphics.DrawString(sMarRank, fCur10, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sfMarketRank.Width / 2), Convert.ToSingle(2 * f05Height + 4 * f15Height + f15Height / 2 - sMarSize.Height / 2 + 1 * sMarSize.Height / 2)));
                bg.Graphics.DrawString(sTradeH, fCur10, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sfTradeH.Width / 2), Convert.ToSingle(2 * f05Height + 4 * f15Height + f15Height / 2 - sMarSize.Height / 2 + 3 * sMarSize.Height / 2)));
                bg.Graphics.DrawString(sValueChange, fCur10, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sfValueChange.Width / 2), Convert.ToSingle(2 * f05Height + 4 * f15Height + f15Height / 2 - sMarSize.Height / 2 + 5 * sMarSize.Height / 2)));

              }
              #endregion
              #region Row 2
              if (iDMC + 5 < BestSells.Count) {
                double iro = BestSells.Keys[BestSells.Count - (iDMC + 6)];
                CurrencyPair acp = BestSells[iro].CurPair;
               // if (DidTradeHistory) {
               //   Dictionary<string, object> aCMDParams = new Dictionary<string, object>();
               //   aCMDParams["CurrencyPair"] = acp;
               //   lPoloQue.ChkAdd(new appCmd("TradingGetTrades", aCMDParams));
               // }
                string sLastPrice = ""; string sMarket2 = "";
                string sBookPrice = ""; string sBookPrice2 = "";
                string sMarket = "";
                if (acp.BaseCurrency == "USDT") {
                  sMarket = acp.BaseCurrency + " " + acp.QuoteCurrency + " ".PadLeft(BestSells[iro].LastMarketData.PriceLast.toStr2().Length, ' ') + "  " + BestSells[iro].PerSecBuyPriceChangeAvg.toStr8();
                  sMarket2 = (" ".PadLeft(acp.BaseCurrency.Length + 1 + acp.QuoteCurrency.Length + 1, ' ')) + BestSells[iro].LastMarketData.PriceLast.toStr2();
                  sLastPrice = BestSells[iro].LowestSell.toStr2() + " " + Convert.ToDouble(BestSells[iro].LastMarketData.PriceLast * 0.0069).toStr2() + " " + BestSells[iro].HighestBuy.toStr2();
                  sBookPrice = BestSells[iro].LastMarketData.OrderTopBuy.toStr2();
                  sBookPrice2 = (" ".PadLeft(BestSells[iro].LastMarketData.OrderTopBuy.toStr2().Length + 1, ' ')) + " " + BestSells[iro].LastMarketData.OrderTopSell.toStr2();

                } else {
                  sMarket = acp.BaseCurrency + " " + acp.QuoteCurrency + " ".PadLeft(BestSells[iro].LastMarketData.PriceLast.toStr8().Length, ' ') + "  " + BestSells[iro].PerSecBuyPriceChangeAvg.toStr8();
                  sMarket2 = (" ".PadLeft(acp.BaseCurrency.Length + 1 + acp.QuoteCurrency.Length + 1, ' ')) + BestSells[iro].LastMarketData.PriceLast.toStr8();
                  sLastPrice = BestSells[iro].LowestSell.toStr8() + " " + Convert.ToDouble(BestSells[iro].LastMarketData.PriceLast * 0.0069).toStr8() + " " + BestSells[iro].HighestBuy.toStr8();
                  sBookPrice = BestSells[iro].LastMarketData.OrderTopBuy.toStr8();
                  sBookPrice2 = (" ".PadLeft(BestSells[iro].LastMarketData.OrderTopBuy.toStr8().Length + 1)) + " " + BestSells[iro].LastMarketData.OrderTopSell.toStr8();

                }
                string sMarRank = "Tic " + BestSells[iro].PerSecSellTicAvg.toStr2() + " " + BestSells[iro].FiveSecSellTicAvg.toStr2() + " B " + BestSells[iro].BuyTickerCount.ToString() + " S " + BestSells[iro].SellTickerCount.ToString();
                string sTradeH = "" + /*(BestSells[iro].tradeHistoryTrades.Count > 0 ?*/ "Avg: " + BestSells[iro].AvgPricePaid.toStr8() + " -> " + Convert.ToDouble(BestSells[iro].AvgPricePaid * 1.0069).toStr8() + " "; //: "");
                string sValueChange = (Balances.Keys.Contains(acp.QuoteCurrency) ? Balances[acp.QuoteCurrency].DBQuote.toStr4() + " " + Convert.ToDouble(Balances[acp.QuoteCurrency].BitcoinValue - (Balances[acp.QuoteCurrency].DBQuote * (BestSells[iro].AvgPricePaid * 1.0069))).toStr8() : "") + " " + Convert.ToDouble(BestSells[iro].LastMarketData.PriceLast - (BestSells[iro].AvgPricePaid * 1.0069)).toStr8();
                SizeF sfValueChange = bg.Graphics.MeasureString(sValueChange, fCur10);
                SizeF sfTradeH = bg.Graphics.MeasureString(sTradeH, fCur10);
                SizeF sfMarketRank = bg.Graphics.MeasureString(sMarRank, fCur10);
                SizeF sLastPSize = bg.Graphics.MeasureString(sLastPrice, fCur9);
                SizeF sMarSize = bg.Graphics.MeasureString(sMarket, fCur10);
                SizeF sLastBSize = bg.Graphics.MeasureString(sBookPrice2, fCur9);
                if (BestSells[iro].Selected) {
                  Brush theColor = Brushes.DarkBlue;
                  if ((FocusedMarket != null) && (BestSells[iro].CurPair == FocusedMarket)) {
                    theColor = Brushes.MidnightBlue;
                  }
                  bg.Graphics.FillRectangle(theColor, new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(2 * f05Height + 5 * f15Height + 1), Convert.ToInt32(f20Width), Convert.ToInt32(f15Height)));
                  bg.Graphics.DrawString("X", fCur10, Brushes.Red, new PointF(Convert.ToSingle(iDMC * f20Width), Convert.ToSingle(2 * f05Height + 5 * f15Height)));
                } else {
                  bg.Graphics.DrawRectangle(Pens.Chartreuse, new Rectangle(Convert.ToInt32(iDMC * f20Width + 1), Convert.ToInt32(2 * f05Height + 5 * f15Height + 1), Convert.ToInt32(f20Width), Convert.ToInt32(f15Height)));
                }
                Brush bQuoteFill;
                if (BestSells[iro].PrevMarketData.OrderTopBuy >= BestSells[iro].LastMarketData.PriceLast) {
                  bQuoteFill = Brushes.HotPink;
                } else if (BestSells[iro].PrevMarketData.OrderTopSell <= BestSells[iro].LastMarketData.PriceLast) {
                  bQuoteFill = Brushes.Chartreuse;
                } else {
                  bQuoteFill = Brushes.WhiteSmoke;
                }
                bg.Graphics.DrawString(sLastPrice, fCur9, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastPSize.Width / 2), Convert.ToSingle(2 * f05Height + 5 * f15Height + f15Height / 2 - sLastPSize.Height / 2 - 3 * sLastPSize.Height)));
                bg.Graphics.DrawString(sBookPrice, fCur9, Brushes.HotPink, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastBSize.Width / 2), Convert.ToSingle(2 * f05Height + 5 * f15Height + f15Height / 2 - sLastPSize.Height / 2 - 2 * sLastPSize.Height)));
                bg.Graphics.DrawString(sBookPrice2, fCur9, Brushes.Chartreuse, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sLastBSize.Width / 2), Convert.ToSingle(2 * f05Height + 5 * f15Height + f15Height / 2 - sLastPSize.Height / 2 - 2 * sLastPSize.Height)));
                bg.Graphics.DrawString(sMarket, fCur10, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sMarSize.Width / 2), Convert.ToSingle(2 * f05Height + 5 * f15Height + f15Height / 2 - sMarSize.Height / 2 - 1 * sLastPSize.Height)));
                bg.Graphics.DrawString(sMarket2, fCur10, bQuoteFill, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sMarSize.Width / 2), Convert.ToSingle(2 * f05Height + 5 * f15Height + f15Height / 2 - sMarSize.Height / 2 - 1 * sLastPSize.Height)));
                bg.Graphics.DrawString(sMarRank, fCur10, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sfMarketRank.Width / 2), Convert.ToSingle(2 * f05Height + 5 * f15Height + f15Height / 2 - sMarSize.Height / 2 + 0 * sMarSize.Height)));
                bg.Graphics.DrawString(sTradeH, fCur10, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sfTradeH.Width / 2), Convert.ToSingle(2 * f05Height + 5 * f15Height + f15Height / 2 - sMarSize.Height / 2 + 1 * sMarSize.Height)));
                bg.Graphics.DrawString(sValueChange, fCur10, Brushes.WhiteSmoke, new PointF(Convert.ToSingle(iDMC * f20Width + f20Width / 2 - sfValueChange.Width / 2), Convert.ToSingle(2 * f05Height + 5 * f15Height + f15Height / 2 - sMarSize.Height / 2 + 2 * sMarSize.Height)));

              }
              #endregion
            }
          }


          #endregion
          #region Draw Focused Market
          if (MarketDetailVisible) {
            if (FocusedMarket != null) {

              #region Detect if Focus Market changed and react if so
              if (LastFocusedMarket != FocusedMarket) {
                LastFocusedMarket = FocusedMarket;
                if (MarketsOfInterest.Keys.Contains(FocusedMarket)) {
                  DoDisableControls();
                  Dictionary<string, object> aCmd = new Dictionary<string, object>();
                  aCmd["CurrencyPair"] = FocusedMarket;
                  lBotQue.ChkAdd(new appCmd("RefreshMarketDetails", aCmd));
                  MarketsOfInterest[FocusedMarket].LastOrderBook = null;
                  if (MarketsOfInterest[FocusedMarket].AvgPricePaid > 0) {
                    if (FocusedMarket.BaseCurrency == "BTC") {
                      edPrice.Value = Convert.ToDecimal(MarketsOfInterest[FocusedMarket].AvgPricePaid) * edMarkup.Value;
                    } else if (FocusedMarket.BaseCurrency == "USDT") {
                      edPrice.Value = Convert.ToDecimal(MarketsOfInterest[FocusedMarket].AvgPricePaid * aBaseRate.USDBTCRate) * edMarkup.Value;
                    } else if (FocusedMarket.BaseCurrency == "ETH") {
                      edPrice.Value = Convert.ToDecimal(MarketsOfInterest[FocusedMarket].AvgPricePaid / aBaseRate.BTCETHRate) * edMarkup.Value;
                    } else if (FocusedMarket.BaseCurrency == "XMR") {
                      edPrice.Value = Convert.ToDecimal(MarketsOfInterest[FocusedMarket].AvgPricePaid / aBaseRate.BTCXMRRate) * edMarkup.Value;
                    }                   
                  } else {
                    edPrice.Value = Convert.ToDecimal(MarketsOfInterest[FocusedMarket].LastBuyPrice + 0.00000001);
                  }
                }

                if ((FocusedMarket != null)&&(MarketsOfInterest.Keys.Contains(FocusedMarket))&&(!FocusedMarketChanging)) {
                  FocusedMarketChanging = true;
                  string aVal = MarketsOfInterest[FocusedMarket].mv["HoldAmount"];
                  decimal aDec = 0;
                  decimal bDec = (Decimal.TryParse(aVal, out aDec) ? aDec : 1);
                  edHoldAmount.Value = bDec;

                  aVal = MarketsOfInterest[FocusedMarket].mv["SellThresh"];
                  bDec = (Decimal.TryParse(aVal, out aDec) ? aDec : Convert.ToDecimal(0.02));
                  edSellThresh.Value = bDec;

                  aVal = MarketsOfInterest[FocusedMarket].mv["BuyThresh"];
                  bDec = (Decimal.TryParse(aVal, out aDec) ? aDec : Convert.ToDecimal(0.02));
                  edBuyThresh.Value = bDec;

                  aVal = MarketsOfInterest[FocusedMarket].mv["GoHold"];
                  if (aVal == "True") {
                    cbGoHold.Checked = true;
                  } else {
                    cbGoHold.Checked = false;
                  }                  

                  FocusedMarketChanging = false; 
                }
    

              }
              #endregion

              Int32 iLeft = Convert.ToInt32(f20Width);
              Int32 iTop = Convert.ToInt32(f05Height);
              Int32 iWidth = Convert.ToInt32(f20Width * 3);
              Int32 iHeight = Convert.ToInt32( 4 * f15Height);
              bg.Graphics.FillRectangle(Brushes.MidnightBlue, new Rectangle(iLeft, iTop, iWidth, iHeight));

              string sMarket = MarketsOfInterest[FocusedMarket].CurPair.ToString();
              SizeF sfMarket = bg.Graphics.MeasureString(sMarket, fCur10);

              bg.Graphics.DrawString(sMarket, fCur10, Brushes.Chartreuse, new PointF(Convert.ToSingle(iLeft + 5), Convert.ToSingle(iTop + sfMarket.Height)));
              sfMarket = bg.Graphics.MeasureString("Whale By Depth: ", fCur10);
              bg.Graphics.DrawString("Whale By Depth: ", fCur10, Brushes.White, new PointF(Convert.ToSingle(iLeft + 5), Convert.ToSingle(iTop + sfMarket.Height * 2.5)));
              edWhaleDepth.Top = iTop + Convert.ToInt32(sfMarket.Height * 2.5);
              edWhaleDepth.Left = iLeft + 5 + Convert.ToInt32(sfMarket.Width);

              if (MarketsOfInterest[FocusedMarket].LastOrderBook != null) {                
                Jojatekok.PoloniexAPI.MarketTools.IOrderBook ob = MarketsOfInterest[FocusedMarket].LastOrderBook;
                Int32 iSellNo = 4;
                Int32 iMaxPad = 4; Int32 iLen;  // Measure Max leng for padding on sell
                Int32 iMaxPad2 = 4; Int32 iLen2;
                double dBuyChunkPrice = 0;
                double dBuyDepthPrice = 0;
                double dSellChunkPrice = 0;
                double dSellDepthPrice = 0;
                double aD = edWhaleDepth.Value.toDouble();
                double dChunkSizeValue = 0;
                if (FocusedMarket.BaseCurrency == "BTC") {
                  dChunkSizeValue = edChunkSize.Value.toDouble();
                } else if (FocusedMarket.BaseCurrency == "USDT") {
                  dChunkSizeValue = edChunkSize.Value.toDouble()*aBaseRate.USDBTCRate;
                } else if (FocusedMarket.BaseCurrency == "ETH") {
                  dChunkSizeValue = aBaseRate.BTCETHRate/ edChunkSize.Value.toDouble();
                } else if (FocusedMarket.BaseCurrency == "XMR") {
                  dChunkSizeValue = aBaseRate.BTCXMRRate/ edChunkSize.Value.toDouble();
                }  
                double dBookSum = 0;
                dSellDepthPrice = edPrice.Value.toDouble();
                foreach (Jojatekok.PoloniexAPI.MarketTools.IOrder oX in ob.SellOrders) {
                  #region First pass do measurements & calculations on sell side.
                  dBookSum = dBookSum + oX.AmountBase;
                  if ((dBookSum > dChunkSizeValue) && (dBuyChunkPrice == 0)) {
                    dBuyChunkPrice = oX.PricePerCoin;
                  }
                  iLen = oX.AmountQuote.toStr2().Length;
                  iLen2 = oX.AmountBase.toStr4().Length;
                  if (iLen > iMaxPad) {
                    iMaxPad = iLen;
                  }
                  if (iLen2 > iMaxPad2) {
                    iMaxPad2 = iLen2;
                  }
                  #endregion
                }                  
                dBookSum = 0;
                dBuyDepthPrice = MarketsOfInterest[FocusedMarket].LastWhaleRiderPriceBuy;
                if (Balances.Keys.Contains(FocusedMarket.QuoteCurrency)) {
                  dChunkSizeValue = Balances[FocusedMarket.QuoteCurrency].QuoteAvailable;
                }
                foreach (Jojatekok.PoloniexAPI.MarketTools.IOrder oX in ob.BuyOrders) {
                  #region First pass do measurements & calculations on buy side.
                  dBookSum = dBookSum + oX.AmountQuote;
                  if ((dBookSum > dChunkSizeValue) && (dSellChunkPrice == 0)) {
                    dSellChunkPrice = oX.PricePerCoin;
                  }
                  iLen = oX.AmountQuote.toStr2().Length;
                  iLen2 = oX.AmountBase.toStr4().Length;
                  if (iLen > iMaxPad) {
                    iMaxPad = iLen;
                  }
                  if (iLen2 > iMaxPad2) {
                    iMaxPad2 = iLen2;
                  }
                  #endregion
                }

                #region Calculations on Balances and Chunk Sizes.
                double dQuoteBal = 0;
                double dBaseBal = 0;
                if (FocusedMarket.BaseCurrency == "BTC") {
                  
                  if ((Balances.Keys.Contains(FocusedMarket.QuoteCurrency)) && (Balances[FocusedMarket.QuoteCurrency].QuoteAvailable > 0.0005)) {
                    dQuoteBal = Balances[FocusedMarket.QuoteCurrency].QuoteAvailable;
                    double dqbase = dSellChunkPrice * dQuoteBal;
                    if (dqbase > Convert.ToDouble(edChunkSize.Value)) {
                      dqbase = Convert.ToDouble(edChunkSize.Value);
                      dQuoteBal = dqbase / dSellChunkPrice;
                    }
                  }

                  if ((Balances.Keys.Contains(FocusedMarket.BaseCurrency)) && (Balances[FocusedMarket.BaseCurrency].QuoteAvailable > 0.0005)) {
                    dBaseBal = Balances[FocusedMarket.BaseCurrency].QuoteAvailable;
                    if (Convert.ToDouble(edChunkSize.Value) < dBaseBal) {
                      dBaseBal = Convert.ToDouble(edChunkSize.Value);
                    }
                  }
                } else if (FocusedMarket.BaseCurrency == "USDT"){
                  double dUSDTChunk = aBaseRate.USDBTCRate * edChunkSize.Value.toDouble(); 
                  if ((Balances.Keys.Contains(FocusedMarket.QuoteCurrency)) && (Balances[FocusedMarket.QuoteCurrency].QuoteAvailable > 0.0005)) {
                    dQuoteBal = Balances[FocusedMarket.QuoteCurrency].QuoteAvailable;
                    double dqbase = dSellChunkPrice * dQuoteBal;
                    if (dqbase > Convert.ToDouble(dUSDTChunk)) {
                      dqbase = Convert.ToDouble(dUSDTChunk);
                      dQuoteBal = dqbase / dSellChunkPrice;
                    }
                  }

                  if ((Balances.Keys.Contains(FocusedMarket.BaseCurrency)) && (Balances[FocusedMarket.BaseCurrency].QuoteAvailable > 0.0005)) {
                    dBaseBal = Balances[FocusedMarket.BaseCurrency].QuoteAvailable;
                    if (dUSDTChunk < dBaseBal) {
                      dBaseBal = Convert.ToDouble(dUSDTChunk);
                    }
                  }
                } else if (FocusedMarket.BaseCurrency == "ETH") {
                  double dETHChunk = edChunkSize.Value.toDouble() / (aBaseRate.BTCETHRate!=0?aBaseRate.BTCETHRate:1);
                  if ((Balances.Keys.Contains(FocusedMarket.QuoteCurrency)) && (Balances[FocusedMarket.QuoteCurrency].QuoteAvailable > 0.0005)) {
                    dQuoteBal = Balances[FocusedMarket.QuoteCurrency].QuoteAvailable;
                    double dqbase = dSellChunkPrice * dQuoteBal;
                    if (dqbase > Convert.ToDouble(dETHChunk)) {
                      dqbase = Convert.ToDouble(dETHChunk);
                      dQuoteBal = dqbase / dSellChunkPrice;
                    }
                  }

                  if ((Balances.Keys.Contains(FocusedMarket.BaseCurrency)) && (Balances[FocusedMarket.BaseCurrency].QuoteAvailable > 0.0005)) {
                    dBaseBal = Balances[FocusedMarket.BaseCurrency].QuoteAvailable;
                    if (dETHChunk < dBaseBal) {
                      dBaseBal = Convert.ToDouble(dETHChunk);
                    }
                  }
                } else if (FocusedMarket.BaseCurrency == "XMR") {
                  double dXMRChunk = edChunkSize.Value.toDouble() / (aBaseRate.BTCXMRRate != 0 ? aBaseRate.BTCXMRRate : 1);
                  if ((Balances.Keys.Contains(FocusedMarket.QuoteCurrency)) && (Balances[FocusedMarket.QuoteCurrency].QuoteAvailable > 0.0005)) {
                    dQuoteBal = Balances[FocusedMarket.QuoteCurrency].QuoteAvailable;
                    double dqbase = dSellChunkPrice * dQuoteBal;
                    if (dqbase > Convert.ToDouble(dXMRChunk)) {
                      dqbase = Convert.ToDouble(dXMRChunk);
                      dQuoteBal = dqbase / dSellChunkPrice;
                    }
                  }

                  if ((Balances.Keys.Contains(FocusedMarket.BaseCurrency)) && (Balances[FocusedMarket.BaseCurrency].QuoteAvailable > 0.0005)) {
                    dBaseBal = Balances[FocusedMarket.BaseCurrency].QuoteAvailable;
                    if (dXMRChunk < dBaseBal) {
                      dBaseBal = Convert.ToDouble(dXMRChunk);
                    }
                  }
                }

                if (MarketsOfInterest[FocusedMarket].NeedsTradeOrderRefresh && (!MarketsOfInterest[FocusedMarket].HadNeedsTradeOrderRefresh)) {
                  MarketsOfInterest[FocusedMarket].HadNeedsTradeOrderRefresh = true;
                  Dictionary<string, object> aCmd = new Dictionary<string, object>();
                  aCmd["CurrencyPair"] = FocusedMarket;
                  lBotQue.ChkAdd(new appCmd("RefreshMarketDetails", aCmd));
                }                    

                #endregion

                #region Layout buttons 
                double dSumBase = 0;
                iSellNo = 1;    // draw Buys.             
                SizeF sfText = bg.Graphics.MeasureString("1.00001111 " + Convert.ToDouble(55.22).toStr2P(iMaxPad) + Convert.ToDouble(55.22).toStr4P(iMaxPad2) + Convert.ToDouble(55.22).toStr4(), fCur6);
                btnBuy.Left = Convert.ToInt32(iLeft + f20Width);
                btnBuy.Top = Convert.ToInt32(iTop + 4 * f15Height - btnBuy.Height);
                btnBuy.Text = "B U Y " + Environment.NewLine + Convert.ToDouble(dBuyChunkPrice).toStr8();

                btnSell.Left = Convert.ToInt32(iLeft + 2 * f20Width - btnSell.Width);
                btnSell.Top = Convert.ToInt32(iTop + 4 * f15Height - btnSell.Height);
                btnSell.Text = "S E L L" + Environment.NewLine + Convert.ToDouble(dSellChunkPrice).toStr8();

                btnCancel.Left = Convert.ToInt32(iLeft + f20Width + btnBuy.Width);
                btnCancel.Width = Convert.ToInt32((iLeft + 2 * f20Width - btnSell.Width) - (iLeft + f20Width + btnBuy.Width));
                btnCancel.Top = Convert.ToInt32(iTop + 4 * f15Height - btnBuy.Height);

                MarketsOfInterest[FocusedMarket].NextBuyPrice = MarketsOfInterest[FocusedMarket].LastWhaleRiderPriceBuy;
                MarketsOfInterest[FocusedMarket].NextSellPrice = edPrice.Value.toDouble();
                
                btnBuyM.Left = Convert.ToInt32(iLeft + f20Width);
                btnBuyM.Top = Convert.ToInt32(btnBuy.Top - btnBuyM.Height);
                btnBuyM.Width = Convert.ToInt32(f20Width / 2);
                
                btnSellM.Left = Convert.ToInt32(iLeft + f20Width + f20Width / 2);
                btnSellM.Top = Convert.ToInt32(btnSell.Top - btnSellM.Height);
                btnSellM.Width = Convert.ToInt32(f20Width / 2);

                btnSellWhale.Left = Convert.ToInt32(iLeft + 2 * f20Width + f20Width / 3 - sfText.Width / 2);
                btnSellWhale.Top = Convert.ToInt32(iTop + f15Height / 2 - (3 * sLocSize.Height));

                edPrice.Left = Convert.ToInt32(iLeft + 2 * f20Width - edPrice.Width);
                edPrice.Top = Convert.ToInt32(iTop + 4 * f15Height - btnSell.Height - btnSellM.Height - edPrice.Height);
                
                edMarkup.Left = edPrice.Left - edMarkup.Width - Convert.ToInt32(bg.Graphics.MeasureString(" =", fCur10).Width);
                edMarkup.Top = Convert.ToInt32(iTop + 4 * f15Height - btnSellM.Height - btnSell.Height - edPrice.Height);
                
                float fNewLeft = edMarkup.Left - bg.Graphics.MeasureString("A0.00000001*", fCur10).Width;

                bg.Graphics.DrawString(" =", fCur9, Brushes.White,
                  new PointF(Convert.ToSingle(edMarkup.Left + edMarkup.Width), Convert.ToSingle(edMarkup.Top)));

                string sAvPrPa = "";
                if (FocusedMarket.BaseCurrency == "BTC") {
                  sAvPrPa = "A" + MarketsOfInterest[FocusedMarket].AvgPricePaid.toStr8().Trim() + "*";
                } else if (FocusedMarket.BaseCurrency == "USDT") {
                  sAvPrPa = "A" + Convert.ToDouble( aBaseRate.USDBTCRate* MarketsOfInterest[FocusedMarket].AvgPricePaid ).toStr2().Trim() + "*";
                } else if (FocusedMarket.BaseCurrency == "ETH") {
                  sAvPrPa = "A" + Convert.ToDouble(MarketsOfInterest[FocusedMarket].AvgPricePaid/aBaseRate.BTCETHRate).toStr8().Trim() + "*";
                } else if (FocusedMarket.BaseCurrency == "XMR") {
                  sAvPrPa = "A" + Convert.ToDouble(MarketsOfInterest[FocusedMarket].AvgPricePaid/aBaseRate.BTCXMRRate).toStr8().Trim() + "*";
                } 
                bg.Graphics.DrawString(sAvPrPa, fCur9, Brushes.White,
                  new PointF(Convert.ToSingle(fNewLeft), Convert.ToSingle(edMarkup.Top)));

                fNewLeft = Convert.ToSingle(f20Width * 2 + 5);
                bg.Graphics.DrawString("BUY " + Convert.ToDouble(dBaseBal / Convert.ToDouble(edPrice.Value)).toStr8() + " " + FocusedMarket.QuoteCurrency + " at " + edPrice.Value.toStr8(), fCur9, Brushes.White,
                  new PointF(Convert.ToSingle(fNewLeft), Convert.ToSingle(edMarkup.Top - 6 * sfText.Height)));
                bg.Graphics.DrawString("  total: " + Convert.ToDouble(dBaseBal).toStr8() + " " + FocusedMarket.BaseCurrency, fCur9, Brushes.White,
                  new PointF(Convert.ToSingle(fNewLeft), Convert.ToSingle(edMarkup.Top - 5 * sfText.Height)));

                bg.Graphics.DrawString("SELL " + dQuoteBal.toStr8() + " " + FocusedMarket.QuoteCurrency + " at " + edPrice.Value.toStr8(), fCur9, Brushes.White,
                  new PointF(Convert.ToSingle(fNewLeft), Convert.ToSingle(edMarkup.Top - 3 * sfText.Height)));
                bg.Graphics.DrawString("  total:" + Convert.ToDouble(dQuoteBal * Convert.ToDouble(edPrice.Value)).toStr8() + " " + FocusedMarket.BaseCurrency, fCur9, Brushes.White,
                  new PointF(Convert.ToSingle(fNewLeft), Convert.ToSingle(edMarkup.Top - 2 * sfText.Height)));

                string sLastPrice = "";
                if (FocusedMarket.BaseCurrency == "USDT") {
                  sMarket = FocusedMarket.BaseCurrency + " " + FocusedMarket.QuoteCurrency + " " + MarketsOfInterest[FocusedMarket].LastMarketData.PriceLast.toStr2();
                  sLastPrice = MarketsOfInterest[FocusedMarket].LastMarketData.OrderTopBuy.toStr2() + " " + MarketsOfInterest[FocusedMarket].LastMarketData.OrderTopSell.toStr2();
                } else {
                  sMarket = FocusedMarket.BaseCurrency + " " + FocusedMarket.QuoteCurrency + " " + MarketsOfInterest[FocusedMarket].LastMarketData.PriceLast.toStr8() + " " + MarketsOfInterest[FocusedMarket].PerSecBuyPriceChangeAvg.toStr8();
                  sLastPrice = MarketsOfInterest[FocusedMarket].LastMarketData.OrderTopBuy.toStr8() + " " + MarketsOfInterest[FocusedMarket].LastMarketData.OrderTopSell.toStr8();
                }

                bg.Graphics.DrawString(sLastPrice,
                   fCur9, Brushes.WhiteSmoke, Convert.ToSingle(fNewLeft), Convert.ToSingle(edMarkup.Top - 8 * sfText.Height));

                bg.Graphics.DrawString(sMarket,
                   fCur9, Brushes.WhiteSmoke, Convert.ToSingle(fNewLeft), Convert.ToSingle(edMarkup.Top - 9 * sfText.Height));
                
                #endregion

                #region Draw Buy Order Book

                Boolean SellLineDrawn = false;
                Boolean SellLine2Drawn = false;
                foreach (Jojatekok.PoloniexAPI.MarketTools.IOrder oX in ob.BuyOrders) {
                  // start a range.
                  Boolean IsMyOrder = false;
                  foreach (Jojatekok.PoloniexAPI.TradingTools.IOrder xO in xMOITOO) {
                    if (xO.PricePerCoin == oX.PricePerCoin) {
                      IsMyOrder = true;
                    }
                  }
                  dSumBase += oX.AmountBase;
                  if (IsMyOrder) {
                    bg.Graphics.DrawString(oX.PricePerCoin.toStr8() + " " + oX.AmountQuote.toStr2P(iMaxPad) + " " + oX.AmountBase.toStr4P(iMaxPad2) + " " + dSumBase.toStr4(),
                      fCur9, Brushes.Chartreuse, Convert.ToSingle(iLeft + f20Width / 3 - sfText.Width / 2), Convert.ToSingle(iTop + f15Height / 2 + (iSellNo * sLocSize.Height)));
                  } else {
                    bg.Graphics.DrawString(oX.PricePerCoin.toStr8() + " " + oX.AmountQuote.toStr2P(iMaxPad) + " " + oX.AmountBase.toStr4P(iMaxPad2) + " " + dSumBase.toStr4(),
                      fCur9, Brushes.WhiteSmoke, Convert.ToSingle(iLeft + f20Width / 3 - sfText.Width / 2), Convert.ToSingle(iTop + f15Height / 2 + (iSellNo * sLocSize.Height)));
                  }
                  if ((dBuyDepthPrice >= oX.PricePerCoin) && (!SellLineDrawn)) {
                    SellLineDrawn = true;
                    bg.Graphics.DrawLine(Pens.Chartreuse,
                      new Point(
                        Convert.ToInt32(iLeft),
                        Convert.ToInt32(iTop + f15Height / 2 + ((iSellNo) * sLocSize.Height) + 2)),
                      new Point(
                        Convert.ToInt32(iLeft + f20Width),
                        Convert.ToInt32(iTop + f15Height / 2 + ((iSellNo) * sLocSize.Height) + 2)
                        ));
                  }
                  if ((dSellChunkPrice >= oX.PricePerCoin) && (!SellLine2Drawn)) {
                    SellLine2Drawn = true;
                    bg.Graphics.DrawLine(Pens.HotPink,
                      new Point(
                        Convert.ToInt32(iLeft),
                        Convert.ToInt32(iTop + f15Height / 2 + ((iSellNo + 1) * sLocSize.Height))),
                      new Point(
                        Convert.ToInt32(iLeft + f20Width),
                        Convert.ToInt32(iTop + f15Height / 2 + ((iSellNo + 1) * sLocSize.Height))
                        ));
                  }
                  iSellNo++;
                  if (iTop + (4 * sfText.Height) + (iSellNo * sLocSize.Height) > (iTop + 4 * f15Height - 20)) { break; }
                }
                #endregion

                #region Run Mode Ques   
         /*     if (tbRunMode.Value == 2) {
                  if (!bShiftToggle) {  // Buy
                    bg.Graphics.DrawString("Next up... SELL",
                        fCur9, Brushes.WhiteSmoke, Convert.ToSingle(iLeft + 2 * f20Width + f20Width / 3 - sfText.Width / 2), Convert.ToSingle(iTop + f15Height / 2 - (3 * sLocSize.Height)));
                  } else {
                    bg.Graphics.DrawString("Next up... BUY",
                        fCur9, Brushes.WhiteSmoke, Convert.ToSingle(iLeft + 2 * f20Width + f20Width / 3 - sfText.Width / 2),
                        Convert.ToSingle(iTop + f15Height / 2 - (4 * sLocSize.Height)));
                  }
                }  */
                #endregion

                #region Draw Sells Order Book
                dSumBase = 0;
                iSellNo = 1;
                SellLineDrawn = false;
                SellLine2Drawn = false;
                foreach (Jojatekok.PoloniexAPI.MarketTools.IOrder oX in ob.SellOrders) {
                  // start a range.
                  Boolean IsMyOrder = false;
                  foreach (Jojatekok.PoloniexAPI.TradingTools.IOrder xO in xMOITOO) {
                    if (xO.PricePerCoin == oX.PricePerCoin) {
                      IsMyOrder = true;
                    }
                  }
                  dSumBase += oX.AmountBase;
                  if (IsMyOrder) {
                    bg.Graphics.DrawString(oX.PricePerCoin.toStr8() + " " + oX.AmountQuote.toStr2P(iMaxPad) + " " + oX.AmountBase.toStr4P(iMaxPad2) + " " + dSumBase.toStr4(),
                      fCur9, Brushes.HotPink, Convert.ToSingle(iLeft + 2 * f20Width + f20Width / 3 - sfText.Width / 2), Convert.ToSingle(iTop + f15Height / 2 + (iSellNo * sLocSize.Height)));
                  } else {
                    bg.Graphics.DrawString(oX.PricePerCoin.toStr8() + " " + oX.AmountQuote.toStr2P(iMaxPad) + " " + oX.AmountBase.toStr4P(iMaxPad2) + " " + dSumBase.toStr4(),
                      fCur9, Brushes.WhiteSmoke, Convert.ToSingle(iLeft + 2 * f20Width + f20Width / 3 - sfText.Width / 2), Convert.ToSingle(iTop + f15Height / 2 + (iSellNo * sLocSize.Height)));
                  }
                  if ((dBuyChunkPrice <= oX.PricePerCoin) && (!SellLineDrawn)) {
                    SellLineDrawn = true;
                    bg.Graphics.DrawLine(Pens.Chartreuse,
                      new Point(
                        Convert.ToInt32(iLeft + 2 * f20Width),
                        Convert.ToInt32(iTop + f15Height / 2 + ((iSellNo + 1) * sLocSize.Height) + 2)),
                      new Point(
                        Convert.ToInt32(iLeft + 2 * f20Width + f20Width),
                        Convert.ToInt32(iTop + f15Height / 2 + ((iSellNo + 1) * sLocSize.Height) + 2)
                        ));
                  }
                  if ((dSellDepthPrice <= oX.PricePerCoin) && (!SellLine2Drawn)) {
                    SellLine2Drawn = true;
                    bg.Graphics.DrawLine(Pens.HotPink,
                      new Point(
                        Convert.ToInt32(iLeft + 2 * f20Width),
                        Convert.ToInt32(iTop + f15Height / 2 + ((iSellNo + 1) * sLocSize.Height) + 2)),
                      new Point(
                        Convert.ToInt32(iLeft + 2 * f20Width + f20Width),
                        Convert.ToInt32(iTop + f15Height / 2 + ((iSellNo + 1) * sLocSize.Height) + 2)
                        ));
                  }

                  iSellNo++;
                  if (iTop + (4 * sfText.Height) + (iSellNo * sLocSize.Height) > (iTop + 4 * f15Height - 20)) { break; }
                }
                #endregion

                #region My Open Orders
                float iX = Convert.ToSingle(2 * f20Width);
                iSellNo = 9;
                bg.Graphics.DrawString("Open Orders",
                   fCur9, Brushes.Chartreuse, Convert.ToSingle(fNewLeft), Convert.ToSingle(edMarkup.Top - 2 * sfText.Height - (iSellNo * sLocSize.Height)));
                if (xMOITOO.Count > 0) {
                  iMaxPad = 0;
                  foreach (Jojatekok.PoloniexAPI.TradingTools.IOrder oX in xMOITOO) {
                    iLen = oX.AmountQuote.toStr2().Length;
                    if (iLen > iMaxPad) {
                      iMaxPad = iLen;
                    }
                  }
                  iSellNo = 10;
                  foreach (Jojatekok.PoloniexAPI.TradingTools.IOrder oX in xMOITOO) {
                    Brush zBrush;
                    if (oX.Type == OrderType.Buy) {
                      zBrush = Brushes.Chartreuse;
                    } else {
                      zBrush = Brushes.HotPink;
                    }
                    bg.Graphics.DrawString("  " + oX.PricePerCoin.toStr8() + " " + oX.AmountQuote.toStr2P(iMaxPad) + " " + oX.AmountBase.toStr8(),
                       fCur9, zBrush, Convert.ToSingle(fNewLeft), Convert.ToSingle(edMarkup.Top - 2 * sfText.Height - (iSellNo * sLocSize.Height)));
                    iSellNo++;
                  }
                }
                #endregion

                #region My Trade History
                if ((MarketsOfInterest[FocusedMarket].tradeHistoryTrades != null) && (MarketsOfInterest[FocusedMarket].tradeHistoryTrades.Count > 0)) {
                  iSellNo++;
                  String slTime = ""; String slPrice = ""; String slType = ""; String sCType = "";
                  double dPrice = 0, dSumQVol = 0, dSumBVol = 0;
                  Boolean bFTT = true;
                  IList<Jojatekok.PoloniexAPI.TradingTools.ITrade> xmTrades = MarketsOfInterest[FocusedMarket].tradeHistoryTrades;
                  bg.Graphics.DrawString("Trading History", fCur9, Brushes.Chartreuse,
                    Convert.ToSingle(fNewLeft), Convert.ToSingle(edMarkup.Top - 2 * sfText.Height - (iSellNo * sLocSize.Height)));
                  iSellNo++;
                  foreach (Jojatekok.PoloniexAPI.TradingTools.ITrade xT in xmTrades) {
                    Brush zBrush;
                    if (xT.Type == OrderType.Buy) {
                      sCType = "Buy";
                    } else {
                      sCType = "Sell";
                    }

                    if (bFTT == true) {
                      dSumQVol = xT.AmountQuote;
                      dSumBVol = xT.AmountBase;
                      dPrice = xT.PricePerCoin;
                      slTime = xT.Time.toStrTime();
                      slPrice = xT.PricePerCoin.toStr8();
                      slType = sCType;
                      bFTT = false;
                    } else {
                      if ((slTime == xT.Time.toStrTime()) && (slPrice == xT.PricePerCoin.toStr8()) && (slType == sCType)) {
                        dSumQVol += xT.AmountQuote;
                        dSumBVol += xT.AmountBase;
                      } else {
                        if (slType == "Buy") {
                          zBrush = Brushes.Chartreuse;
                        } else {
                          zBrush = Brushes.HotPink;
                        }
                        string s = "  " + slTime + " " + slPrice + " " + dSumBVol.toStr4() + " " + FocusedMarket.BaseCurrency + " " + dSumQVol.toStr2() + " " + FocusedMarket.QuoteCurrency;
                        bg.Graphics.DrawString(s, fCur8, zBrush, Convert.ToSingle(fNewLeft), Convert.ToSingle(edMarkup.Top - 2 * sfText.Height - (iSellNo * sLocSize.Height)));
                        dSumQVol = xT.AmountQuote;
                        dSumBVol = xT.AmountBase;
                        dPrice = xT.PricePerCoin;
                        slTime = xT.Time.toStrTime();
                        slPrice = xT.PricePerCoin.toStr8();
                        slType = sCType;
                        iSellNo++;
                      }
                    }

                    if ((edMarkup.Top - 2 * sfText.Height - (iSellNo * sLocSize.Height)) < (f05Height)) { break; }

                  }
                }
                #endregion

                #region Balance Shard Vars Listings

                iLeft = Convert.ToInt32(0);
                iTop = Convert.ToInt32(f05Height);
                iWidth = Convert.ToInt32(f20Width);
                iHeight = Convert.ToInt32(f15Height * 4);
                
                bg.Graphics.FillRectangle(Brushes.MidnightBlue, new Rectangle(iLeft, iTop, iWidth, iHeight));
                if (MarketsOfInterest[FocusedMarket].mv["MarketViewMode"] == null) {
                  MarketsOfInterest[FocusedMarket].mv["MarketViewMode"] = "Shards";
                }
                string sMarketViewMode = MarketsOfInterest[FocusedMarket].mv["MarketViewMode"];                
                if (sMarketViewMode == "Shards") {
                  bg.Graphics.FillRectangle(Brushes.DarkSeaGreen, 1,                           Convert.ToSingle(iTop), Convert.ToSingle(f20Width/5), Convert.ToSingle(f05Height / 2));
                  bg.Graphics.DrawRectangle(Pens.Chartreuse, 1,                                Convert.ToSingle(iTop), Convert.ToSingle(f20Width/5), Convert.ToSingle(f05Height / 2));
                  bg.Graphics.DrawRectangle(Pens.Chartreuse, Convert.ToSingle(f20Width / 5)+2, Convert.ToSingle(iTop), Convert.ToSingle(f20Width/5), Convert.ToSingle(f05Height / 2));
                  bg.Graphics.DrawRectangle(Pens.Chartreuse, Convert.ToSingle(2*f20Width / 5) + 2, Convert.ToSingle(iTop), Convert.ToSingle(f20Width / 5), Convert.ToSingle(f05Height / 2));
                  bg.Graphics.DrawString("Shds", fCur8, Brushes.Chartreuse, new PointF(Convert.ToSingle(f20Width / 10 - aSF.Width / 2), Convert.ToSingle(iTop + (fCur6.Height / 2))));
                  bg.Graphics.DrawString("Vars", fCur8, Brushes.Chartreuse, new PointF(Convert.ToSingle(3 * f20Width / 10 - aSF.Width / 2), Convert.ToSingle(iTop + (fCur6.Height / 2))));
                  bg.Graphics.DrawString("Charts", fCur8, Brushes.Chartreuse, new PointF(Convert.ToSingle(5 * f20Width / 10 - aSF.Width / 2), Convert.ToSingle(iTop + (fCur6.Height / 2))));
                } else if (sMarketViewMode == "Vars") {
                  bg.Graphics.DrawRectangle(Pens.Chartreuse, 1, Convert.ToSingle(iTop), Convert.ToSingle(f20Width / 5), Convert.ToSingle(f05Height / 2));
                  bg.Graphics.FillRectangle(Brushes.DarkSeaGreen, Convert.ToSingle(f20Width / 5) + 2, Convert.ToSingle(iTop), Convert.ToSingle(f20Width / 5), Convert.ToSingle(f05Height / 2));
                  bg.Graphics.DrawRectangle(Pens.Chartreuse, Convert.ToSingle(f20Width / 5) + 2, Convert.ToSingle(iTop), Convert.ToSingle(f20Width / 5), Convert.ToSingle(f05Height / 2));
                  bg.Graphics.DrawRectangle(Pens.Chartreuse, Convert.ToSingle(2 * f20Width / 5) + 2, Convert.ToSingle(iTop), Convert.ToSingle(f20Width / 5), Convert.ToSingle(f05Height / 2));
                  bg.Graphics.DrawString("Shds", fCur8, Brushes.Chartreuse, new PointF(Convert.ToSingle(f20Width / 10 - aSF.Width / 2), Convert.ToSingle(iTop + (fCur6.Height / 2))));
                  bg.Graphics.DrawString("Vars", fCur8, Brushes.Chartreuse, new PointF(Convert.ToSingle(3 * f20Width / 10 - aSF.Width / 2), Convert.ToSingle(iTop + (fCur6.Height / 2))));
                  bg.Graphics.DrawString("Charts", fCur8, Brushes.Chartreuse, new PointF(Convert.ToSingle(5 * f20Width / 10 - aSF.Width / 2), Convert.ToSingle(iTop + (fCur6.Height / 2))));
                } else if (sMarketViewMode == "Charts") {
                  bg.Graphics.DrawRectangle(Pens.Chartreuse, 1, Convert.ToSingle(iTop), Convert.ToSingle(f20Width / 5), Convert.ToSingle(f05Height / 2));                  
                  bg.Graphics.DrawRectangle(Pens.Chartreuse, Convert.ToSingle(f20Width / 5) + 2, Convert.ToSingle(iTop), Convert.ToSingle(f20Width / 5), Convert.ToSingle(f05Height / 2));
                  bg.Graphics.FillRectangle(Brushes.DarkSeaGreen, Convert.ToSingle(2*f20Width / 5) + 2, Convert.ToSingle(iTop), Convert.ToSingle(f20Width / 5), Convert.ToSingle(f05Height / 2));
                  bg.Graphics.DrawRectangle(Pens.Chartreuse, Convert.ToSingle(2 * f20Width / 5) + 2, Convert.ToSingle(iTop), Convert.ToSingle(f20Width / 5), Convert.ToSingle(f05Height / 2));

                  bg.Graphics.DrawString("Shds", fCur8, Brushes.Chartreuse, new PointF(Convert.ToSingle(f20Width / 10 - aSF.Width / 2), Convert.ToSingle(iTop + (fCur6.Height / 2))));
                  bg.Graphics.DrawString("Vars", fCur8, Brushes.Chartreuse, new PointF(Convert.ToSingle(3 * f20Width / 10 - aSF.Width / 2), Convert.ToSingle(iTop + (fCur6.Height / 2))));
                  bg.Graphics.DrawString("Charts", fCur8, Brushes.Chartreuse, new PointF(Convert.ToSingle(5 * f20Width / 10 - aSF.Width / 2), Convert.ToSingle(iTop + (fCur6.Height / 2))));
                }

                if (sMarketViewMode == "Shards") {
                  if (edBuyThresh.Visible) { edBuyThresh.Visible = false; }
                  if (edSellThresh.Visible) { edSellThresh.Visible = false; }
                  if (edHoldAmount.Visible) { edHoldAmount.Visible = false; }
                  if (cbGoHold.Visible) { cbGoHold.Visible = false; }

                  if (Balances.Keys.Contains(FocusedMarket.QuoteCurrency)) {
                    iSellNo = 4;
                    bg.Graphics.DrawString(FocusedMarket.QuoteCurrency + " Shards ", fCur8, Brushes.White, Convert.ToSingle(iLeft), Convert.ToSingle(iTop + (iSellNo * sLocSize.Height)));
                    iSellNo = 6; double dQuoteTotal = 0;
                    foreach (double dSPrice in Balances[FocusedMarket.QuoteCurrency].ShardMngr.Shards.Keys) {
                      CurrencyShard aCS = Balances[FocusedMarket.QuoteCurrency].ShardMngr.Shards[dSPrice];
                      string sPrice = aCS.BTCPricePerCoin.toStr8();
                      dQuoteTotal = dQuoteTotal + aCS.AmountQuote - aCS.SmallBuyFeeQuote;
                      string s = "  " + sPrice + " x " + Convert.ToDouble(aCS.AmountQuote - aCS.SmallBuyFeeQuote).toStr4P(10) + " = " + aCS.AmountBase.toStr8() + " " + FocusedMarket.BaseCurrency;
                      bg.Graphics.DrawString(s, fCur8, Brushes.White, Convert.ToSingle(iLeft), Convert.ToSingle(iTop + (iSellNo * sLocSize.Height)));
                      iSellNo++;
                      if ((iTop + 2 * sfText.Height + (iSellNo * sLocSize.Height)) > (iTop + iHeight)) {
                        break;
                      }
                    }

                    bg.Graphics.DrawString(FocusedMarket.QuoteCurrency + " Shards Total: "+Convert.ToDouble(dQuoteTotal).toStr8(), fCur8, Brushes.White, Convert.ToSingle(iLeft), Convert.ToSingle(iTop + (4 * sLocSize.Height)));
                    
                  }
                } else if (sMarketViewMode == "Vars") {
                  if (!edBuyThresh.Visible) { edBuyThresh.Visible = true; }
                  if (!edSellThresh.Visible) { edSellThresh.Visible = true; }
                  if (!edHoldAmount.Visible) { edHoldAmount.Visible = true; }
                  if (!cbGoHold.Visible) { cbGoHold.Visible = true; }


                  edHoldAmount.Top = Convert.ToInt32(f05Height + 2 * f15Height);
                  edSellThresh.Top = edHoldAmount.Top - edHoldAmount.Height;
                  edBuyThresh.Top = (edHoldAmount.Top + edHoldAmount.Height) + (edHoldAmount.Top - (edSellThresh.Top + edSellThresh.Height));
                  cbGoHold.Top = edSellThresh.Top - edHoldAmount.Height;

                  double HoldSell = Convert.ToDouble(MarketsOfInterest[FocusedMarket].HoldAmount + MarketsOfInterest[FocusedMarket].SellThreshold);
                  double HoldBuy = Convert.ToDouble(MarketsOfInterest[FocusedMarket].HoldAmount - MarketsOfInterest[FocusedMarket].BuyThreshold);

                  double SellWeightDiff = (Balances[FocusedMarket.QuoteCurrency].DBQuote * MarketsOfInterest[FocusedMarket].LastMarketData.OrderTopSell) - (HoldSell);
                  double BuyWeightDiff = (Balances[FocusedMarket.QuoteCurrency].DBQuote * MarketsOfInterest[FocusedMarket].LastMarketData.OrderTopSell) - (HoldBuy);

                  if (SellWeightDiff >= 0) {
                    double TargetSellBtcVol = (Balances[FocusedMarket.QuoteCurrency].DBQuote * MarketsOfInterest[FocusedMarket].LastMarketData.OrderTopSell) - Convert.ToDouble(MarketsOfInterest[FocusedMarket].HoldAmount);
                    double TargetSellQuoteVol = TargetSellBtcVol / MarketsOfInterest[FocusedMarket].LastMarketData.OrderTopSell;

                      bg.Graphics.DrawString(TargetSellQuoteVol.toStr4() + " @ " + MarketsOfInterest[FocusedMarket].LastMarketData.OrderTopSell.toStr8() + " = " + TargetSellBtcVol.toStr8(), fCur10, Brushes.Green,
                        new PointF(Convert.ToSingle(12), Convert.ToSingle(iHeight / 4 )));
                    
                  } else {
                    double TargetSellPrice = HoldSell / Balances[FocusedMarket.QuoteCurrency].DBQuote;
                    double TargetSellQuoteVol = Convert.ToDouble(MarketsOfInterest[FocusedMarket].SellThreshold) / TargetSellPrice;
                      bg.Graphics.DrawString(TargetSellQuoteVol.toStr4() + " @ " + TargetSellPrice.toStr8() + " = " + (TargetSellPrice * TargetSellQuoteVol).toStr8(), fCur10, Brushes.Red,
                        new PointF(Convert.ToSingle(12), Convert.ToSingle(iHeight /4 )));
                    
                  }

                  if (BuyWeightDiff <= 0) {  // value less than thresh
                    double TargetBuyBtcVol = Convert.ToDouble(MarketsOfInterest[FocusedMarket].HoldAmount) - (Balances[FocusedMarket.QuoteCurrency].DBQuote * MarketsOfInterest[FocusedMarket].LastMarketData.OrderTopBuy);
                    double TargetBuyQuoteVol = TargetBuyBtcVol / MarketsOfInterest[FocusedMarket].LastMarketData.OrderTopBuy;

                    bg.Graphics.DrawString(TargetBuyQuoteVol.toStr4() + " @ " + MarketsOfInterest[FocusedMarket].LastMarketData.OrderTopSell.toStr8() + " = " + TargetBuyBtcVol.toStr8(), fCur10, Brushes.Green,
                      new PointF(Convert.ToSingle(12), Convert.ToSingle(3 * iHeight / 4 )));
                  } else {
                    double TargetBuyPrice = HoldBuy / Balances[FocusedMarket.QuoteCurrency].DBQuote;
                    double TargetBuyQuoteVol = Convert.ToDouble(MarketsOfInterest[FocusedMarket].BuyThreshold) / TargetBuyPrice;
                    bg.Graphics.DrawString(TargetBuyQuoteVol.toStr4() + " @ " + TargetBuyPrice.toStr8() + " = " + (TargetBuyPrice * TargetBuyQuoteVol).toStr8(), fCur10, Brushes.Red,
                      new PointF(Convert.ToSingle(12), Convert.ToSingle(3 * iHeight / 4 )));
                  }

                } else if (sMarketViewMode == "Charts") { 
                  appMarket aAM = MarketsOfInterest[FocusedMarket];
                  if (aAM != null){
                    DrawChart(bg, aAM);
                  }
                  if (edBuyThresh.Visible) { edBuyThresh.Visible = false ; }
                  if (edSellThresh.Visible) { edSellThresh.Visible = false; }
                  if (edHoldAmount.Visible) { edHoldAmount.Visible = false; }
                  if (cbGoHold.Visible) { cbGoHold.Visible = false; }

                }              

                #endregion

                #region Market Trade History
                 if (FocusedMarket != null){
                   iLeft = Convert.ToInt32(4*f20Width);
                   iTop = Convert.ToInt32(f05Height);
                   iWidth = Convert.ToInt32(f20Width);
                   iHeight = Convert.ToInt32(f15Height * 4-1);
                   bg.Graphics.FillRectangle(Brushes.MidnightBlue, new Rectangle(iLeft, iTop, iWidth, iHeight));

                   String slTime = ""; String slPrice = ""; String slType = ""; String sCType = "";
                   double dPrice = 0, dSumQVol = 0, dSumBVol = 0;
                   Boolean bFTT = true;

                   iSellNo = 2;
                   bg.Graphics.DrawString(FocusedMarket.QuoteCurrency + " Market History ", fCur8, Brushes.White, Convert.ToSingle(iLeft), Convert.ToSingle(iTop + 2 * sfText.Height + (iSellNo * sLocSize.Height)));
                   iSellNo = 4;
                   List<Jojatekok.PoloniexAPI.MarketTools.ITrade> xMOI = MarketsOfInterest[FocusedMarket].tradeHistoryMarket;
                   foreach (Jojatekok.PoloniexAPI.MarketTools.ITrade xT in xMOI ) {
                     Brush zBrush;
                     if (xT.Type == OrderType.Buy) {
                       sCType = "Buy";
                     } else {
                       sCType = "Sell";
                     }

                     if (bFTT == true) {
                       dSumQVol = xT.AmountQuote;
                       dSumBVol = xT.AmountBase;
                       dPrice = xT.PricePerCoin;
                       slTime = xT.Time.toStrTime();
                       slPrice = xT.PricePerCoin.toStr8();
                       slType = sCType;
                       bFTT = false;
                     } else {
                       if ((slTime == xT.Time.toStrTime()) && (slPrice == xT.PricePerCoin.toStr8()) && (slType == sCType)) {
                         dSumQVol += xT.AmountQuote;
                         dSumBVol += xT.AmountBase;
                       } else {
                         if (slType == "Buy") {
                           zBrush = Brushes.Chartreuse;
                         } else {
                           zBrush = Brushes.HotPink;
                         }
                         string s = "  " + slTime + " " + slPrice + " " + dSumBVol.toStr4() + " " + FocusedMarket.BaseCurrency + " " + dSumQVol.toStr2() + " " + FocusedMarket.QuoteCurrency;
                         bg.Graphics.DrawString(s, fCur8, zBrush, Convert.ToSingle(iLeft), Convert.ToSingle(iTop + 2 * sfText.Height + (iSellNo * sLocSize.Height)));
                         dSumQVol = xT.AmountQuote;
                         dSumBVol = xT.AmountBase;
                         dPrice = xT.PricePerCoin;
                         slTime = xT.Time.toStrTime();
                         slPrice = xT.PricePerCoin.toStr8();
                         slType = sCType;
                         iSellNo++;
                       }
                     }

                     if ((iTop + 2 * sfText.Height + (iSellNo * sLocSize.Height)) > (iTop + iHeight-20)) { break; }

                   }                   
                }

                #endregion

              }

            }
          }
          SetActionCtrVisibility(FocusedMarket);
          #endregion
        }
      } catch (Exception e) {
        LogException("R1", e);
        throw e;
      }
    }


    public void DrawChart(BufferedGraphics bg, appMarket aAM) {
      float fWidth = bg.Graphics.VisibleClipBounds.Width;
      float fHeight = bg.Graphics.VisibleClipBounds.Height;
      double f20Height = fHeight * 0.2;
      double f05Height = fHeight * 0.065;
      double f15Height = fHeight * 0.145;
      double f20Width = fWidth * 0.2;
      double f15Width = fWidth * 0.15;
      SizeF sLocSize = bg.Graphics.MeasureString("    0.00000000 BTC ", fCur6);
      Int32 iLeft = Convert.ToInt32(0);
      Int32 iTop = Convert.ToInt32(3 * f05Height / 2 + 5);
      Int32 iWidth = Convert.ToInt32(f20Width);
      Int32 iHeight = Convert.ToInt32(4 * f15Height - f05Height/2 - 6);

      bg.Graphics.DrawRectangle(Pens.WhiteSmoke, new Rectangle(iLeft, iTop, iWidth, iHeight));


    }
    #endregion

  }



}
