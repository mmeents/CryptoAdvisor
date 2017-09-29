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

namespace Advisor {


  public class BaseRates {
    public double USDBTCRate;
    public double BTCETHRate;
    public double BTCXMRRate;
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
      SmallBuyFeeQuote = AmountQuote * Owner.MakerFee;
      LargeBuyFeeQuote = AmountQuote * Owner.TakerFee;
      SmallSellFeeBase = AmountBase * Owner.MakerFee;
      LargeSellFeeBase = AmountBase * Owner.TakerFee;
    }
  }

  public class TradeHistMngr {
    public String Currency;
    public String LastSellDetails = "";
    public double VolumeToAccountFor = 0;
    public double MakerFee = 0;
    public double TakerFee = 0;
    public SortedList<double, CurrencyShard> Shards;
    public TradeHistMngr(string aCurrency, double aMakerFee, double aTakerFee) {
      Currency = aCurrency;
      MakerFee = aMakerFee;
      TakerFee = aTakerFee;
      Shards = new SortedList<double, CurrencyShard>();
      LoadShards();
    }
    public void AddTrade(BaseRates aMarketRates, CurrencyPair aMarketPair, TT.ITrade aTrade) {
      double aChkPrice = 0;
      double aTradeQuote = 0;
      double aTradeBase = 0;
      
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

      if (aTrade.Type == OrderType.Buy) {        

        if (Shards.ContainsKey(aChkPrice)) {
          Shards[aChkPrice].AmountBase = Shards[aChkPrice].AmountBase + aTradeBase;
          Shards[aChkPrice].AmountQuote = Shards[aChkPrice].AmountQuote + aTradeQuote;
        } else {
          Shards.Add(aChkPrice, new CurrencyShard(this, aChkPrice, aTradeQuote, aTradeBase));
        }        
        Shards[aChkPrice].ReCalculateFees();

        //LastSellDetails = aMarketPair.QuoteCurrency + " buy " + aTradeQuote.toStr8() + " at " + aChkPrice.toStr8() + " btc total " + aTradeBase.toStr8();
        //LastSellDetails.toLog("History" + aMarketPair.ToString());

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

              if (((Shards[aShardPriceKey].AmountQuote - QuoteVolToFind) * aShardPriceKey) < Shards[aShardPriceKey].LargeSellFeeBase) {
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

       // double ProfitLossValue = aTradeBase - BaseVolToFind;

       // LastSellDetails = aMarketPair.QuoteCurrency + " sold " + aTradeQuote.toStr8() + " at " + aChkPrice.toStr8() + " btc total " + aTradeBase.toStr8() + " cost " + BaseVolToFind.toStr8() + " profit " + ProfitLossValue.toStr8();
       // LastSellDetails.toLog("History"+aMarketPair.ToString());


      }

    }
    public Boolean LoadShards() {
      Boolean wasThereShards = false;
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
            wasThereShards = true;
          }
        }
      }
      return wasThereShards;
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
    public double QuoteVolume() {
      double aSum = 0;
      Int32 i = Shards.Count - 1;
      while (i >= 0) {
        double sLineKey = Shards.Keys[i];
        aSum = aSum + Shards[sLineKey].AmountQuote ;
        i = i - 1;
      }
      return aSum;
    }

    public void AuditShards(double aQuotedVol) {
      double QuoteVolToFind = 0;
      double BaseVolToFind = 0;
      Int32 iShardCount = Shards.Keys.Count - 1;
      Int32 iShard = iShardCount;
      List<CurrencyShard> TempListOfShards = new List<CurrencyShard>();
      while (iShard >= 0)  {  //  && (QuoteVolToFind > 0))
        double aShardPriceKey = Shards.Keys[iShard];
        BaseVolToFind = BaseVolToFind + Shards[aShardPriceKey].AmountBase;
        QuoteVolToFind = QuoteVolToFind + Shards[aShardPriceKey].AmountQuote;
        iShard--;
      }      

      if (QuoteVolToFind > aQuotedVol) {   // need to trim off extra's 
        double aTrimAmt = QuoteVolToFind - aQuotedVol;
        iShardCount = Shards.Keys.Count - 1;
        iShard = iShardCount;        
        while ((iShard >= 0) && (aTrimAmt > 0)) {  // trim off shards smaller than trim amount.
          double aShardPriceKey = Shards.Keys[iShard];
          if (Shards[aShardPriceKey].AmountQuote < aTrimAmt) {
            aTrimAmt = aTrimAmt - Shards[aShardPriceKey].AmountQuote;
            Shards.Remove(aShardPriceKey);
          }          
          iShard--;
        }

        if (aTrimAmt > 0) {
          iShardCount = Shards.Keys.Count - 1;   // next take the first amount from the top. 
          iShard = iShardCount;
          while ((iShard >= 0) && (aTrimAmt > 0)) {
            double aShardPriceKey = Shards.Keys[iShard];
            if (aTrimAmt < Shards[aShardPriceKey].AmountQuote ){
              Shards[aShardPriceKey].AmountQuote = Shards[aShardPriceKey].AmountQuote - aTrimAmt; 
              Shards[aShardPriceKey].AmountBase = Shards[aShardPriceKey].AmountBase - (aTrimAmt * aShardPriceKey);
              aTrimAmt = 0;
            } else {
              aTrimAmt = aTrimAmt - Shards[aShardPriceKey].AmountQuote;
              Shards.Remove(aShardPriceKey);
            }            
            iShard--;
          }
        }
      }
    }
    public double BTCAvgPricePaid(double aQuoteVol) {
      double aRet = 0;
      AuditShards(aQuoteVol);
      double QuoteVolToFind = aQuoteVol;
      double BaseVolToFind = 0; //aTradeBase;
      Int32 iShardCount = Shards.Keys.Count - 1;
      Int32 iShard = iShardCount;
      List<CurrencyShard> TempListOfShards = new List<CurrencyShard>();
      while ((iShard >= 0) && (QuoteVolToFind > 0)) {
        double aShardPriceKey = Shards.Keys[iShard];
        BaseVolToFind = BaseVolToFind + Shards[aShardPriceKey].AmountBase;
        QuoteVolToFind = QuoteVolToFind - Shards[aShardPriceKey].AmountQuote;
        iShard--;
      }

      if (QuoteVolToFind < 0) { 


      }

      aRet = BaseVolToFind / aQuoteVol;
      return aRet;
    }
    public void LoadShardsFromTradeHistory(appMarket aAM) { 

    }
  }

  public class appBalance {
    public string Currency;      // new     
    public double CostEstBTC;    // new
    public double PriceEstBTC;   // new

    public double DBQuote;       // polo
    public double BitcoinValue;   //polo 
    public double QuoteAvailable; //polo
    public double QuoteOnOrders;  //polo

    public double PercentOfTotal; //new  
    public double CircRadius { 
      get { 
         return Math.Sqrt( BitcoinValue / Math.PI ) ; 
      }
    }

    public TradeHistMngr ShardMngr;

    public appBalance(string aCur, IBalance aBalance, double MakerFee, double TakerFee, BaseRates aBR) {
      Currency = aCur;
      CostEstBTC = 0;
      PriceEstBTC = 0;
      if ((aCur == "USDT") && (aBalance.QuoteAvailable > 0)) {
        BitcoinValue = aBalance.QuoteAvailable / ((aBR.USDBTCRate != 0) ? aBR.USDBTCRate : 2500);
        QuoteAvailable = aBalance.QuoteAvailable;
        QuoteOnOrders = aBalance.QuoteOnOrders;
        DBQuote = aBalance.QuoteAvailable + aBalance.QuoteOnOrders;
      } else {
        BitcoinValue = aBalance.BitcoinValue;
        QuoteAvailable = aBalance.QuoteAvailable;
        QuoteOnOrders = aBalance.QuoteOnOrders;
        DBQuote = aBalance.QuoteAvailable + aBalance.QuoteOnOrders;
      }
      ShardMngr = new TradeHistMngr(aCur, MakerFee, TakerFee);
    }

    public void setLastBalance(IBalance aBalance, string aCur, BaseRates aBR) {
      if ((aCur == "USDT") && (aBalance.QuoteAvailable > 0)) {
        BitcoinValue = aBalance.QuoteAvailable / ((aBR.USDBTCRate != 0) ? aBR.USDBTCRate : 2500);
        QuoteAvailable = aBalance.QuoteAvailable;
        QuoteOnOrders = aBalance.QuoteOnOrders;
        DBQuote = aBalance.QuoteAvailable + aBalance.QuoteOnOrders;
      } else {
        BitcoinValue = aBalance.BitcoinValue;
        QuoteAvailable = aBalance.QuoteAvailable;
        QuoteOnOrders = aBalance.QuoteOnOrders;
        DBQuote = aBalance.QuoteAvailable + aBalance.QuoteOnOrders;
      }
      
      ShardMngr.VolumeToAccountFor = DBQuote;
    }

    public void AddTrade(BaseRates aBaseRate, CurrencyPair aCurPair, TT.ITrade aTrade) {
      ShardMngr.AddTrade(aBaseRate, aCurPair, aTrade);
    }

  }


}
