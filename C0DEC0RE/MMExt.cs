using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Security.Cryptography;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Web.UI;


namespace C0DEC0RE {
    
  public static class MMExt {
    
    #region Salts ...
    public static byte[] defIV = new byte[] { 11, 13, 27, 31, 37, 41, 71, 87 };
    #endregion 

    #region Integers

    public static decimal toDecimal(this Int32 x)
    {
      decimal y = Convert.ToDecimal(x);
      return y;
    }

    #endregion

    #region Double

    public static Int32 toInt32(this double x)
    {
      Int32 y = Convert.ToInt32(x);  // rounds
      return y;
    }

    public static Int32 toInt32T(this double x)
    {
      Int32 y = Convert.ToInt32(x.toStr2().ParseString(".", 0));
      return y;
    }

    public static string toStr2(this double x)
    {
      string y = String.Format(CultureInfo.InvariantCulture, "{0:0.00}", x);
      return y;
    }
    public static string toStr2P(this double x, Int32 iDigitToPad)
    {
      string y = String.Format(CultureInfo.InvariantCulture, "{0:0.00}", x).PadLeft(iDigitToPad, ' ');
      return y;
    }
    public static string toStr4(this double x)
    {
      string y = String.Format(CultureInfo.InvariantCulture, "{0:0.0000}", x);
      return y;
    }
    public static string toStr4P(this double x, Int32 iDigitToPad)
    {
      string y = String.Format(CultureInfo.InvariantCulture, "{0:0.0000}", x).PadLeft(iDigitToPad, ' ');
      return y;
    }
    public static string toStr8(this double x)
    {
      string y = String.Format(CultureInfo.InvariantCulture, "{0:0.00000000}", x);
      return y;
    }

    public static string toStr8P(this double x, Int32 iDigitToPad)
    {
      string y = String.Format(CultureInfo.InvariantCulture, "{0:0.00000000}", x).PadLeft(iDigitToPad, ' ');
      return y;
    }

    public static decimal toDecimal(this double x)
    {
      decimal y = Convert.ToDecimal(x);
      return y;
    }

    public static double toPointsVertical(this double dIn){ 
      return (dIn * 72); 
    }

    public static double toPointsHorizontal(this double dIn){ 
      return (dIn * 9.72); 
    }

    #endregion

    #region Decimal 

    public static Int32 toInt32(this decimal x)
    {
      Int32 y = Convert.ToInt32(x);
      return y;
    }

    public static Int32 toInt32T(this decimal x)
    {
      Int32 y = Convert.ToInt32(x.toStr2().ParseString(".", 0));
      return y;
    }

    public static string toStr2(this decimal x)
    {
      string y = String.Format(CultureInfo.InvariantCulture, "{0:0.00}", x);
      return y;
    }
    public static string toStr2P(this decimal x, Int32 iDigitToPad)
    {
      string y = String.Format(CultureInfo.InvariantCulture, "{0:0.00}", x).PadLeft(iDigitToPad, ' ');
      return y;
    }
    public static string toStr4(this decimal x)
    {
      string y = String.Format(CultureInfo.InvariantCulture, "{0:0.0000}", x);
      return y;
    }
    public static string toStr4P(this decimal x, Int32 iDigitToPad)
    {
      string y = String.Format(CultureInfo.InvariantCulture, "{0:0.0000}", x).PadLeft(iDigitToPad, ' ');
      return y;
    }
    public static string toStr8(this decimal x)
    {
      string y = String.Format(CultureInfo.InvariantCulture, "{0:0.00000000}", x);
      return y;
    }

    public static string toStr8P(this decimal x, Int32 iDigitToPad)
    {
      string y = String.Format(CultureInfo.InvariantCulture, "{0:0.00000000}", x).PadLeft(iDigitToPad, ' ');
      return y;
    }

    public static double toDouble(this decimal x)
    {
      double y = Convert.ToDouble(x);
      return y;
    }

    public static float toFloat(this double x){ 
      float y = Convert.ToSingle(x);
      return y;
    }

    public static float toPointsVertical(this float dIn){ 
      return (dIn * 72); 
    }

    public static float toPointsHorizontal(this float dIn){ 
      return (dIn * 9.72).toFloat(); 
    }
    
    #endregion

    #region Object
    public static Boolean isNull(this object aObj){ 
      Boolean isItNull = (aObj==null);
      if (!isItNull){
        isItNull = Convert.IsDBNull(aObj);
      }
      return isItNull;
    }
    public static bool toBoolean(this object aObj) {
      if (!aObj.isNull()) {
        return Convert.ToBoolean(aObj);
      } else { 
        return false;
      }
    }
    public static string toString(this object aObj) {      
      return Convert.ToString(aObj);
    }
    public static Int32 toInt32(this object aObj) {     
      Int32 r = -1;
      if(Int32.TryParse(aObj.toString(), out r)) {
        return r;
      } else {
        return -1;
      }      
    }
    public static double toDouble(this object aObj){ 
      double y = Convert.ToDouble(aObj);            
      return y;
    }
    public static DateTime toDateTime(this object aObj){ 
      DateTime aOut = Convert.ToDateTime(aObj);
      return aOut;
      }
    public static string toDollarStr(this object aObj){ 
      string sResult = "$0.00";
      decimal dValue = 0; 
      if ( decimal.TryParse( aObj.toString(), out dValue)){ 
        sResult = "$"+dValue.toStr2();
      } 
      return sResult;
    }
    public static string toURLDecode(this object aObj, Page pCurPage ){
      string sOut = Convert.ToString(aObj);
      sOut = pCurPage.Server.UrlDecode(sOut).Replace("%20", " ");      
      return sOut;      
    }
    public static string toURLDecodeForHTML(this object aObj, Page pCurPage ){
      string sOut = Convert.ToString(aObj);
      sOut = pCurPage.Server.UrlDecode(sOut).Replace("%20", " ").Replace("\n", "<br/>");      
      return sOut;      
    }
    public static string toURLEncoded(this object aObj, Page pCurPage ){ 
      string sOut = Convert.ToString(aObj);
      sOut = pCurPage.Server.UrlEncode(sOut);
      return sOut;
    }
    #endregion 

    #region Strings

    #region Parse strings
    public static int ParseCount(this string content, string delims)
    {
      return content.Split(delims.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length;
    }
    public static string ParseString(this string content, string delims, int take)
    {
      string[] split = content.Split(delims.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
      return (take >= split.Length ? "" : split[take]);
    }

    public static string[] toCsvSplit(this string line) {
      List<string> result = new List<string>();
      string currentStr = "";
      bool inSkipNextQuote = false;
      bool inQuotes = false;
      for (int i = 0; i < line.Length; i++) {
        if ((line[i] == '\"') && (!inSkipNextQuote)) {
          if (i + 1 < line.Length) {
            if (line[i + 1] == '\"') {
              inSkipNextQuote = true;
              currentStr = currentStr + line[i];
            }
          }
          if (!inSkipNextQuote) {
            inQuotes = !inQuotes;
          }
        } else if ((line[i] == '\"') && (inSkipNextQuote)) {
          //currentStr = currentStr + line[i];  // double dbl quote is a representation of single double quote
          inSkipNextQuote = false;
        } else if (line[i] == ',') {
          if (!inQuotes) {
            result.Add(currentStr);
            currentStr = "";
          } else
            currentStr = currentStr + line[i]; // If in quotes, just add it 
        } else { // Add any other character to current string
          currentStr = currentStr + line[i];
        }
      }
      result.Add(currentStr);
      return result.ToArray(); // Return array of all strings
    }
    #endregion 

    #region crypto strings

    #region byte[]

    public static string toHexStr(this byte[] byteArray)
    {
      string outString = "";
      foreach (Byte b in byteArray)
        outString += b.ToString("X2");
      return outString;
    }

    public static byte[] toByteArray(this string hexString)
    {
      byte[] returnBytes = new byte[hexString.Length / 2];
      for (int i = 0; i < returnBytes.Length; i++)
        returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
      return returnBytes;
    }

    #endregion

    public static Stream ToStream(this string Text) {      
      return new MemoryStream(Encoding.UTF8.GetBytes(Text));
    }

    public static string toString(this Stream stream) {      
      stream.Seek(0,SeekOrigin.Begin);
      using(StreamReader reader = new StreamReader(stream,Encoding.UTF8)) {
        return reader.ReadToEnd();
      }      
    }

    public static string toHashSHA512(this string Text) {      
      var bytes = Encoding.UTF8.GetBytes(Text);
      using(var hash = SHA512.Create()) {
        var hashedInputBytes = hash.ComputeHash(bytes);        
        var hashedInputStringBuilder = new StringBuilder(128);
        foreach(var b in hashedInputBytes)
          hashedInputStringBuilder.Append(b.ToString("X2"));
        return hashedInputStringBuilder.ToString();
      }      
    }

    public static string toBase64EncodedStr(this string Text)
    {
      byte[] encBuff = System.Text.Encoding.UTF8.GetBytes(Text);
      return Convert.ToBase64String(encBuff);
    }
    public static string toBase64DecodedStr(this string Text)
    {
      byte[] decbuff = Convert.FromBase64String(Text);
      string s = System.Text.Encoding.UTF8.GetString(decbuff);
      return s;
    }

    public static string toAESCipher(this KeyPair akp, string sText)
    {
      string sResult = "";
      try {
        AesCryptoServiceProvider aASP = new AesCryptoServiceProvider();
        AesManaged aes = new AesManaged();
        aes.Key = akp.getKey;
        aes.IV = akp.getIV;
        MemoryStream ms = new MemoryStream();
        CryptoStream encStream = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
        StreamWriter sw = new StreamWriter(encStream);
        sw.WriteLine(sText.toBase64EncodedStr());
        sw.Close();
        encStream.Close();
        byte[] buffer = ms.ToArray();
        ms.Close();
        sResult = buffer.toHexStr();
      } catch (Exception e) {
        throw e;
      }
      return sResult;
    }
    public static string toDecryptAES(this KeyPair akp, string sAESCipherText)
    {
      string val = ""; 
      try {        
        AesCryptoServiceProvider aASP = new AesCryptoServiceProvider();
        AesManaged aes = new AesManaged();
        aes.Key = akp.getKey;
        aes.IV = akp.getIV;
        MemoryStream ms = new MemoryStream(sAESCipherText.toByteArray());
        CryptoStream encStream = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
        StreamReader sr = new StreamReader(encStream);      
        val = sr.ReadToEnd();
        val = val.toBase64DecodedStr();
        sr.Close();
        encStream.Close();
        ms.Close();
      } catch (Exception e) {
        throw e;
      }
      return val;
    }

    public static string toDESCipher(this KeyPair akp, string sText)
    {
      string sResult = "";
      DESCryptoServiceProvider aCSP = new DESCryptoServiceProvider();
      aCSP.Key = akp.getKey;
      aCSP.IV = akp.getIV;
      MemoryStream ms = new MemoryStream();
      CryptoStream encStream = new CryptoStream(ms, aCSP.CreateEncryptor(), CryptoStreamMode.Write);
      StreamWriter sw = new StreamWriter(encStream);
      sw.WriteLine(sText.toBase64EncodedStr());
      sw.Close();
      encStream.Close();
      byte[] buffer = ms.ToArray();
      ms.Close();
      sResult = buffer.toHexStr();
      return sResult;
    }
    public static string toDecryptDES(this KeyPair akp, string sDESCipherText)
    {
      DESCryptoServiceProvider aCSP = new DESCryptoServiceProvider();
      aCSP.Key = akp.getKey;
      aCSP.IV = akp.getIV;
      MemoryStream ms = new MemoryStream(sDESCipherText.toByteArray());
      CryptoStream encStream = new CryptoStream(ms, aCSP.CreateDecryptor(), CryptoStreamMode.Read);
      StreamReader sr = new StreamReader(encStream);
      string val = sr.ReadToEnd().toBase64DecodedStr();
      sr.Close();
      encStream.Close();
      ms.Close();
      return val;
    }

    #endregion

    #region Excel Column Logic from string  thanks https://stackoverflow.com/a/1951526 
    public static Int32 toExcelColInt(this string sCellColLetter){ 
      int retVal = 0;
      string col = sCellColLetter.ToUpper();
      for (int iChar = col.Length - 1; iChar >= 0; iChar--){
          char colPiece = col[iChar];
          int colNum = colPiece - 64;
          retVal = retVal + colNum * (int)Math.Pow(26, col.Length - (iChar + 1));
      }
      return retVal;      
    }

    public static string toExcelColLetters(this Int32 iColIndex){ 
      string columnString = "";
      decimal columnNumber = iColIndex;
      while (columnNumber > 0) {
          decimal currentLetterNumber = (columnNumber - 1) % 26;
          char currentLetter = (char)(currentLetterNumber + 65);
          columnString = currentLetter + columnString;
          columnNumber = (columnNumber - (currentLetterNumber + 1)) / 26;
      }
      return columnString;
    }

    #endregion 

    #endregion

    #region Dates and Times

    public static string toStrDateTime(this DateTime x)
    {
      string y = String.Format(CultureInfo.InvariantCulture, "{0:yyyy-MM-dd hh:mm:ss.FFF tt}", x);
      return y;
    }
    public static string toStrDate(this DateTime x)
    {
      string y = String.Format(CultureInfo.InvariantCulture, "{0:yyyy-MM-dd}", x);
      return y;
    }
    public static string toStrTime(this DateTime x)
    {
      string y = String.Format(CultureInfo.InvariantCulture, "{0:hh:mm:ss}", x);
      return y;
    }
    public static string ToStrDateMM(this DateTime x) {
      string y = String.Format(CultureInfo.InvariantCulture, "{0:MM/dd/yyyy hh:mm}", x);
      return y;
    }
    public static string toStrDay(this DateTime x){ 
      string y = String.Format(CultureInfo.InvariantCulture, "{0:MM/dd/yyyy}", x);
      return y;
    }

    #endregion

    #region Dates and Times
    public static Boolean hasFirstRow(this DataSet ds) {      
      return ((ds.Tables.Count>0)&&(ds.Tables[0].Rows.Count>0));
    }

    public static DataRow toFirstRow(this DataSet ds){ 
      return ds.Tables[0].Rows[0];
    }

    #endregion
    
    #region Files and Locations

    public static string SettingFileName(string sSettingName)
    {
      return UserLogLocation() + sSettingName + ".ini";
    }

    public static string LogFileName(string sLogName){
      return UserLogLocation() + sLogName + DateTime.Now.toStrDate().Trim() + ".txt";
    }

    public static string UserLogLocation(){
      String sUserDataDir = Application.CommonAppDataPath + "\\";
      if (!Directory.Exists(sUserDataDir))
      {
        Directory.CreateDirectory(sUserDataDir);
      }
      return sUserDataDir;
    }

    public static string toLog(this string sMsg, string sLogName){
      string sLogFileName = LogFileName(sLogName);
      if (File.Exists(sLogFileName)) { 
        using (StreamWriter w = File.AppendText(sLogFileName)) { 
          w.WriteLine(DateTime.Now.toStrDateTime() + ":" + sMsg); 
        }
      } else {
        using (StreamWriter w = File.CreateText(sLogFileName)) {
          w.WriteLine(DateTime.Now.toStrDateTime() + ":" + sMsg);
        }
      }
      return sMsg;
    }

    public static string toTextFile(this string sMsg, string sLogFileName){ 
      try{
        using (StreamWriter w = File.AppendText(sLogFileName)) { w.Write(sMsg); }
      } catch (Exception ee) {      
        throw ee.toLogException("C0DEC0RE.toTextFile");      
      }
      return sMsg;
    }

    public static string toTextFileLine(this string sMsg, string sLogFileName){ 
      try{
        using (StreamWriter w = File.AppendText(sLogFileName)) { w.WriteLine(sMsg); }
      } catch (Exception ee) {
        throw ee.toLogException("C0DEC0RE.toTextFileLine");
      }
      return sMsg;
    }

    public static string MMConLocation() {
      string sCommon = Application.CommonAppDataPath;
      sCommon = sCommon.Substring(0, sCommon.LastIndexOf('\\'));
      sCommon = sCommon.Substring(0, sCommon.LastIndexOf('\\'));
      sCommon = sCommon.Substring(0, sCommon.LastIndexOf('\\') + 1);
      return sCommon + "MMCommons";
    }

    public static string AppExeFolder() {
      return Path.GetDirectoryName(Application.ExecutablePath) + "\\";
    }

    public static string toFileContentHash(this string aFileName) {
        string FileHash = "";
        try {                
          using (MD5 md = MD5.Create()) {
            using (FileStream InStream = File.OpenRead(aFileName)){
              FileHash = md.ComputeHash(InStream).toHexStr();
              InStream.Dispose();
            }                    
          }
        }catch (Exception e){
          FileHash = "Error:" + e.Message;
          if (FileHash.Length > 100){
              FileHash = FileHash.Substring(0, 100);
          }              
        }
        return FileHash;
     }
        
    #endregion

    #region Exceptions  

    public static string cTraceTableCreateSQL (){ 
      return "CREATE TABLE [dbo].[TraceException]( "+
      "	[TE_ID] [bigint] IDENTITY(1,1) NOT NULL, "+
      "	[TE_U_ID] [int] NOT NULL, "+
      "	[TE_Date] [datetime] DEFAULT (getdate()) NULL, "+
      "	[TE_Location] [varchar](800) NULL, "+
      "	[TE_Details] [varchar](max) NULL, "+
      "  CONSTRAINT [PK_TraceError] PRIMARY KEY CLUSTERED ([TE_ID] ASC) "+
      "   WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] "+
      ") ON [PRIMARY] ";       
    }
    public static string toWalkExcTreePath(this Exception e){ 
      string sThisExcStr = "e.Msg:"+e.Message+Environment.NewLine;        
      if (e.InnerException != null) {
        sThisExcStr = sThisExcStr +"e.Inn:"+ e.InnerException.toWalkExcTreePath();
      }
      return sThisExcStr;
    }
    public static Exception toTraceException(this Exception e, User UserX, string sLocation){
      try {
        MMData d = new MMData();
        string sDetails = e.toWalkExcTreePath();
        d.ExecuteStoredProc("PD", "insert into dbo.TraceException (TE_U_ID, TE_Location, TE_Details) values (@aU_ID, @aLoc, @aDetails) ", new StProcParam[]{
          new StProcParam("@aU_ID", System.Data.DbType.Int32, UserX.UserID),
          new StProcParam("@aLoc", System.Data.DbType.String, sLocation),
          new StProcParam("@aDetails", System.Data.DbType.String, sDetails)
        });
      } catch {
        //  if db is out then lets not add another to pile on log. 
      }
      return e;
    } 

    public static Exception toLogException(this Exception e, string sLogName) {
      try {
        e.toWalkExcTreePath().toTextFile(sLogName);        
      }catch{}
      return e;
    }

    #endregion
    
  }



}
