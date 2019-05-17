using System;
using System.Text;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Xml;
using System.Web;
using System.Web.Caching;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace C0DEC0RE {

  public class StProcParam {
    DbType fParamType;
    string fVarName;
    object fVarValue;
    public StProcParam(string aVarName, DbType aParamType, object aVarValue) {
      fParamType = aParamType;
      fVarName = aVarName;
      fVarValue = aVarValue;
    }
    public string VarName { get { return fVarName; } set { fVarName = value; } }
    public DbType ParamType { get { return fParamType; } set { fParamType = value; } }
    public object VarValue { get { return fVarValue; } set { fVarValue = value; } }
  }
  
  public class MMData{
    public MMData(){}

    public DataSet GetDataSet(string QueryString) {
      Database db = DatabaseFactory.CreateDatabase();            
      DbCommand dbCommand = db.GetSqlStringCommand(QueryString);
      dbCommand.CommandTimeout = 1800;
      return db.ExecuteDataSet(dbCommand);
    }
    public DataSet GetDataSet(string ConnectionName, string QueryString) {      
      Database db = DatabaseFactory.CreateDatabase(ConnectionName);
      DbCommand dbCommand = db.GetSqlStringCommand(QueryString);
      dbCommand.CommandTimeout = 1800;
      return db.ExecuteDataSet(dbCommand);
    }
    public void ExecuteSQLStr(string QueryString) {
      Database db = DatabaseFactory.CreateDatabase();
      DbCommand dbCommand = db.GetSqlStringCommand(QueryString);
      dbCommand.CommandTimeout = 1800;
      DataSet t = db.ExecuteDataSet(dbCommand);
    }
    public void ExecuteSQLStr(string ConnectionName, string QueryString) {
      Database db = DatabaseFactory.CreateDatabase(ConnectionName);
      DbCommand dbCommand = db.GetSqlStringCommand(QueryString);
      dbCommand.CommandTimeout = 1800;
      DataSet t = db.ExecuteDataSet(dbCommand);
    }
    public void ExecuteStoredProc(string ConnectionName, string ProcedureCode, StProcParam[] ParamsList) {
      Database db = DatabaseFactory.CreateDatabase(ConnectionName);
      string sqlCommand = ProcedureCode;
      DbCommand dbSelectCmd = db.GetSqlStringCommand(ProcedureCode);
      dbSelectCmd.CommandTimeout = 1800;
      foreach (StProcParam p in ParamsList) {
        db.AddInParameter(dbSelectCmd, p.VarName, p.ParamType, p.VarValue);
      }
      db.ExecuteNonQuery(dbSelectCmd);
    }
    public DataSet GetStProcDataSet(string ConnectionName, string ProcedureCode, StProcParam[] ParamsList) {
      Database db = DatabaseFactory.CreateDatabase(ConnectionName);
      string sqlCommand = ProcedureCode;
      DbCommand dbSelectCmd = db.GetSqlStringCommand(ProcedureCode);
      dbSelectCmd.CommandTimeout = 1800;
      foreach (StProcParam p in ParamsList) {
        db.AddInParameter(dbSelectCmd, p.VarName, p.ParamType, p.VarValue);
      }
      return db.ExecuteDataSet(dbSelectCmd);
    }  
  }
  
  public class RCData{  
    public DbConnectionInfo CI;
    public RCData( DbConnectionInfo ConInfo ){
      CI = ConInfo;	
    }
    public DataSet GetDataSet(string QueryString) {
      Database db = new SqlDatabase(CI.ConnectionString);
      DbCommand dbCommand = db.GetSqlStringCommand(QueryString);
      dbCommand.CommandTimeout = 1800;
      return db.ExecuteDataSet(dbCommand);
    }    
    public void ExecuteSQLStr(string QueryString) {
      Database db = new SqlDatabase(CI.ConnectionString);
      DbCommand dbCommand = db.GetSqlStringCommand(QueryString);
      dbCommand.CommandTimeout = 1800;
      DataSet t = db.ExecuteDataSet(dbCommand);
    }    
    public void ExecuteStoredProc(string ProcedureCode, StProcParam[] ParamsList) {
      Database db = new SqlDatabase(CI.ConnectionString);
      string sqlCommand = ProcedureCode;
      DbCommand dbSelectCmd = db.GetSqlStringCommand(ProcedureCode);
      dbSelectCmd.CommandTimeout = 1800;
      foreach (StProcParam p in ParamsList) {
        db.AddInParameter(dbSelectCmd, p.VarName, p.ParamType, p.VarValue);
      }
      db.ExecuteNonQuery(dbSelectCmd);
    }
    public DataSet GetStProcDataSet(string ProcedureCode, StProcParam[] ParamsList) {
      Database db = new SqlDatabase(CI.ConnectionString);
      string sqlCommand = ProcedureCode;
      DbCommand dbSelectCmd = db.GetSqlStringCommand(ProcedureCode);
      dbSelectCmd.CommandTimeout = 1800;
      foreach (StProcParam p in ParamsList) {
        db.AddInParameter(dbSelectCmd, p.VarName, p.ParamType, p.VarValue);
      }
      return db.ExecuteDataSet(dbSelectCmd);
    }  
  }

  public class MMStrUtl {
    public int ParseCount(string content, string delims) {
      return content.Split(delims.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length;
    }
    public string ParseString(string content, string delims, int take) {
      string[] split = content.Split(delims.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
      return (take >= split.Length ? "" : split[take]);
    }
    public string stripOutChars(string content, string delims) {
      Int32 chunckCount = ParseCount(content, delims);
      string sTemp = "";
      for (var i = 0; i <= chunckCount - 1; i++) {
        sTemp = sTemp + ParseString(content, delims, i);
      }
      return sTemp;
    }
    public string TranslateToHTMLEncoded(string str) {
      string s = str.Replace("\n", "<br>");
      return s;
    }
    public string DayToStr(DateTime dt) {
      string sm = "0" + Convert.ToString(dt.Month);
      sm = sm.Substring(sm.Length-2, 2);
      string sd = "0" + Convert.ToString(dt.Day);
      sd = sd.Substring(sm.Length - 2, 2);
      return sm + "/" + sd + "/" + Convert.ToString(dt.Year); 
    }
    public string Base64EncodeText(string Text) {
      byte[] encBuff = System.Text.Encoding.UTF8.GetBytes(Text);
      return Convert.ToBase64String(encBuff);
    }
    public string Base64DecodeText(string Text) {
      byte[] decbuff = Convert.FromBase64String(Text);
      string s = System.Text.Encoding.UTF8.GetString(decbuff);
      return s;
    }
    public string ByteToHex(byte[] byteArray) {
      string outString = "";
      foreach (Byte b in byteArray)
        outString += b.ToString("X2");
      return outString;
    }
    public byte[] HexToByte(string hexString) {
      byte[] returnBytes = new byte[hexString.Length / 2];
      for (int i = 0; i < returnBytes.Length; i++)
        returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
      return returnBytes;
    }
    public string Encrypt(string StringToEncrypt) {
      string key = Base64DecodeText("MUY5RUEzQTY3Rjc5QkU5OQ==");
      string iv = Base64DecodeText("OTRCOTc5MDNCQTc4RjkzRg==");
      DESCryptoServiceProvider des = new DESCryptoServiceProvider();
      des.Key = HexToByte(key);
      des.IV = HexToByte(iv);
      MemoryStream ms = new MemoryStream();
      CryptoStream encStream = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
      StreamWriter sw = new StreamWriter(encStream);
      sw.WriteLine(StringToEncrypt);
      sw.Close();
      encStream.Close();
      byte[] buffer = ms.ToArray();
      ms.Close();
      return ByteToHex(buffer);
    }
    public string Decrypt(string CipherString) {
      string key = Base64DecodeText("MUY5RUEzQTY3Rjc5QkU5OQ==");
      string iv = Base64DecodeText("OTRCOTc5MDNCQTc4RjkzRg==");
      DESCryptoServiceProvider des = new DESCryptoServiceProvider();
      des.Key = HexToByte(key);
      des.IV = HexToByte(iv);
      MemoryStream ms = new MemoryStream(HexToByte(CipherString));
      CryptoStream encStream = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read);
      StreamReader sr = new StreamReader(encStream);
      string val = sr.ReadLine();
      sr.Close();
      encStream.Close();
      ms.Close();
      return val;
    }
  }

  public class SysVar {
    Cache fCache;
    Boolean fOverrideLocalCache;
    public SysVar(Cache LocalCacheAccess = null) {
      
      fCache = LocalCacheAccess;
      if (fCache == null) {
        fOverrideLocalCache = true;
      }
      fOverrideLocalCache = false;
    }
    private void SetVarValue(string DatabaseName, string VarName, string VarValue) {
      try {
        MMData d = new MMData();
        d.ExecuteStoredProc(DatabaseName, "exec dbo.sp_SetDBVars @VarName, @VarValue",
          new StProcParam[] {
            new StProcParam("@VarName", DbType.String, VarName), 
            new StProcParam("@VarValue", DbType.String, VarValue)
          }
        );
        if (fCache != null) {
          fCache[VarName] = VarValue;
        }
      } catch (Exception e) {
        throw e;
      }
    }

    private string GetVarValue(string DatabaseName, string VarName) {
      string result = "";
      MMData d = new MMData();
      try {

        if ((fCache != null)&&(!OverrideLocalCache)) { 
          if (fCache[VarName]==null){            
            DataSet ds = d.GetStProcDataSet(DatabaseName, "SELECT dv_VarValue, dv_VarName FROM DBVars WHERE dv_VarName = @VarName ",
              new StProcParam[] { new StProcParam("@VarName", DbType.String, VarName) });

            if ((ds.Tables.Count > 0) & (ds.Tables[0].Rows.Count > 0)) {
                try { result = Convert.ToString(ds.Tables[0].Rows[0]["dv_VarValue"]); }  catch { result = ""; }
            }
            if (fCache != null) {
              fCache[VarName] = result;
            }
          }
        }

        if ((fCache == null) || (OverrideLocalCache)) {          
          DataSet ds = d.GetStProcDataSet(DatabaseName, "SELECT dv_VarValue, dv_VarName FROM DBVars WHERE dv_VarName = @VarName ",
            new StProcParam[] { new StProcParam("@VarName", DbType.String, VarName) });

          if ((ds.Tables.Count > 0) & (ds.Tables[0].Rows.Count > 0)) {
              try { result = Convert.ToString(ds.Tables[0].Rows[0]["dv_VarValue"]); }  catch { result = ""; }
          }
          if (fCache != null) {
            fCache[VarName] = result;
          }
        } else {
          result = fCache[VarName].ToString();
        }

      } catch { }
      return result;
    }
    public string this[string DatabaseName, string VarName] { get { return GetVarValue(DatabaseName, VarName); } set { SetVarValue(DatabaseName, VarName, value); } } 
    public Boolean OverrideLocalCache { get { return fOverrideLocalCache; } set { fOverrideLocalCache = value; } }
  }

  public class MMSysVar {            
    private void SetVarValue(string DatabaseName, string VarName, string VarValue) {
      try {
        MMData d = new MMData();
        d.ExecuteStoredProc(DatabaseName, "exec dbo.sp_SetDBVars @VarName, @VarValue",
          new StProcParam[] {
            new StProcParam("@VarName", DbType.String, VarName), 
            new StProcParam("@VarValue", DbType.String, VarValue)
          }
        );        
      } catch (Exception e) {
        throw e;
      }
    }

    private string GetVarValue(string DatabaseName, string VarName) {
      string result = "";
      MMData d = new MMData();
      try {
        
        DataSet ds = d.GetStProcDataSet(DatabaseName, "SELECT dv_VarValue, dv_VarName FROM DBVars WHERE dv_VarName = @VarName ",
          new StProcParam[] { new StProcParam("@VarName", DbType.String, VarName) });

        if ((ds.Tables.Count > 0) & (ds.Tables[0].Rows.Count > 0)) {
          try { result = Convert.ToString(ds.Tables[0].Rows[0]["dv_VarValue"]); } catch { result = ""; }
        }
      } catch { }
      return result;
    }
    public string this[string DatabaseName, string VarName] { get { return GetVarValue(DatabaseName, VarName); } set { SetVarValue(DatabaseName, VarName, value); } }    
  }
  
  public class SysVarDBRC {    
    DbConnectionInfo CI;
    public SysVarDBRC(String ConnectionString) {
      CI = new DbConnectionInfo("adatabasename", ConnectionString);      
    }
    private void SetVarValue(string VarName, string VarValue) {
      try {
        RCData d = new RCData(CI);
        d.ExecuteStoredProc("exec dbo.sp_SetDBVars @VarName, @VarValue",
          new StProcParam[] {
            new StProcParam("@VarName", DbType.String, VarName), 
            new StProcParam("@VarValue", DbType.String, VarValue)
          }
        );        
      } catch { }
    }

    private string GetVarValue( string VarName) {
      string result = "";
      try {
        
        RCData d = new RCData(CI);
        DataSet ds = d.GetStProcDataSet( "SELECT dv_VarValue, dv_VarName FROM DBVars WHERE VarName = @VarName ",
          new StProcParam[] {
            new StProcParam("@VarName", DbType.String, VarName)
          });

        if ((ds.Tables.Count > 0) & (ds.Tables[0].Rows.Count > 0)) {
          try { result = Convert.ToString(ds.Tables[0].Rows[0]["dv_VarValue"]); } catch { result = ""; }
        }       
        
      } catch { }
      return result;
    }
    public string this[string VarName] { get { return GetVarValue(VarName); } set { SetVarValue( VarName, value); } } 
    
  }

  public static class MIMEAssistant {
    private static readonly Dictionary<string, string> MIMETypesDictionary = new Dictionary<string, string> {
      {"ai", "application/postscript"},
      {"aif", "audio/x-aiff"},
      {"aifc", "audio/x-aiff"},
      {"aiff", "audio/x-aiff"},
      {"asc", "text/plain"},
      {"atom", "application/atom+xml"},
      {"au", "audio/basic"},
      {"avi", "video/x-msvideo"},
      {"bcpio", "application/x-bcpio"},
      {"bin", "application/octet-stream"},
      {"bmp", "image/bmp"},
      {"cdf", "application/x-netcdf"},
      {"cgm", "image/cgm"},
      {"class", "application/octet-stream"},
      {"cpio", "application/x-cpio"},
      {"cpt", "application/mac-compactpro"},
      {"csh", "application/x-csh"},
      {"css", "text/css"},
      {"dcr", "application/x-director"},
      {"dif", "video/x-dv"},
      {"dir", "application/x-director"},
      {"djv", "image/vnd.djvu"},
      {"djvu", "image/vnd.djvu"},
      {"dll", "application/octet-stream"},
      {"dmg", "application/octet-stream"},
      {"dms", "application/octet-stream"},
      {"doc", "application/msword"},
      {"docx","application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
      {"dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template"},
      {"docm","application/vnd.ms-word.document.macroEnabled.12"},
      {"dotm","application/vnd.ms-word.template.macroEnabled.12"},
      {"dtd", "application/xml-dtd"},
      {"dv", "video/x-dv"},
      {"dvi", "application/x-dvi"},
      {"dxr", "application/x-director"},
      {"eps", "application/postscript"},
      {"etx", "text/x-setext"},
      {"exe", "application/octet-stream"},
      {"ez", "application/andrew-inset"},
      {"gif", "image/gif"},
      {"gram", "application/srgs"},
      {"grxml", "application/srgs+xml"},
      {"gtar", "application/x-gtar"},
      {"hdf", "application/x-hdf"},
      {"hqx", "application/mac-binhex40"},
      {"htm", "text/html"},
      {"html", "text/html"},
      {"ice", "x-conference/x-cooltalk"},
      {"ico", "image/x-icon"},
      {"ics", "text/calendar"},
      {"ief", "image/ief"},
      {"ifb", "text/calendar"},
      {"iges", "model/iges"},
      {"igs", "model/iges"},
      {"jnlp", "application/x-java-jnlp-file"},
      {"jp2", "image/jp2"},
      {"jpe", "image/jpeg"},
      {"jpeg", "image/jpeg"},
      {"jpg", "image/jpeg"},
      {"js", "application/x-javascript"},
      {"kar", "audio/midi"},
      {"latex", "application/x-latex"},
      {"lha", "application/octet-stream"},
      {"lzh", "application/octet-stream"},
      {"m3u", "audio/x-mpegurl"},
      {"m4a", "audio/mp4a-latm"},
      {"m4b", "audio/mp4a-latm"},
      {"m4p", "audio/mp4a-latm"},
      {"m4u", "video/vnd.mpegurl"},
      {"m4v", "video/x-m4v"},
      {"mac", "image/x-macpaint"},
      {"man", "application/x-troff-man"},
      {"mathml", "application/mathml+xml"},
      {"me", "application/x-troff-me"},
      {"mesh", "model/mesh"},
      {"mid", "audio/midi"},
      {"midi", "audio/midi"},
      {"mif", "application/vnd.mif"},
      {"mov", "video/quicktime"},
      {"movie", "video/x-sgi-movie"},
      {"mp2", "audio/mpeg"},
      {"mp3", "audio/mpeg"},
      {"mp4", "video/mp4"},
      {"mpe", "video/mpeg"},
      {"mpeg", "video/mpeg"},
      {"mpg", "video/mpeg"},
      {"mpga", "audio/mpeg"},
      {"ms", "application/x-troff-ms"},
      {"msh", "model/mesh"},
      {"mxu", "video/vnd.mpegurl"},
      {"nc", "application/x-netcdf"},
      {"oda", "application/oda"},
      {"ogg", "application/ogg"},
      {"pbm", "image/x-portable-bitmap"},
      {"pct", "image/pict"},
      {"pdb", "chemical/x-pdb"},
      {"pdf", "application/pdf"},
      {"pgm", "image/x-portable-graymap"},
      {"pgn", "application/x-chess-pgn"},
      {"pic", "image/pict"},
      {"pict", "image/pict"},
      {"png", "image/png"}, 
      {"pnm", "image/x-portable-anymap"},
      {"pnt", "image/x-macpaint"},
      {"pntg", "image/x-macpaint"},
      {"ppm", "image/x-portable-pixmap"},
      {"ppt", "application/vnd.ms-powerpoint"},
      {"pptx","application/vnd.openxmlformats-officedocument.presentationml.presentation"},
      {"potx","application/vnd.openxmlformats-officedocument.presentationml.template"},
      {"ppsx","application/vnd.openxmlformats-officedocument.presentationml.slideshow"},
      {"ppam","application/vnd.ms-powerpoint.addin.macroEnabled.12"},
      {"pptm","application/vnd.ms-powerpoint.presentation.macroEnabled.12"},
      {"potm","application/vnd.ms-powerpoint.template.macroEnabled.12"},
      {"ppsm","application/vnd.ms-powerpoint.slideshow.macroEnabled.12"},
      {"ps", "application/postscript"},
      {"qt", "video/quicktime"},
      {"qti", "image/x-quicktime"},
      {"qtif", "image/x-quicktime"},
      {"ra", "audio/x-pn-realaudio"},
      {"ram", "audio/x-pn-realaudio"},
      {"ras", "image/x-cmu-raster"},
      {"rdf", "application/rdf+xml"},
      {"rgb", "image/x-rgb"},
      {"rm", "application/vnd.rn-realmedia"},
      {"roff", "application/x-troff"},
      {"rtf", "text/rtf"},
      {"rtx", "text/richtext"},
      {"sgm", "text/sgml"},
      {"sgml", "text/sgml"},
      {"sh", "application/x-sh"},
      {"shar", "application/x-shar"},
      {"silo", "model/mesh"},
      {"sit", "application/x-stuffit"},
      {"skd", "application/x-koan"},
      {"skm", "application/x-koan"},
      {"skp", "application/x-koan"},
      {"skt", "application/x-koan"},
      {"smi", "application/smil"},
      {"smil", "application/smil"},
      {"snd", "audio/basic"},
      {"so", "application/octet-stream"},
      {"spl", "application/x-futuresplash"},
      {"src", "application/x-wais-source"},
      {"sv4cpio", "application/x-sv4cpio"},
      {"sv4crc", "application/x-sv4crc"},
      {"svg", "image/svg+xml"},
      {"swf", "application/x-shockwave-flash"},
      {"t", "application/x-troff"},
      {"tar", "application/x-tar"},
      {"tcl", "application/x-tcl"},
      {"tex", "application/x-tex"},
      {"texi", "application/x-texinfo"},
      {"texinfo", "application/x-texinfo"},
      {"tif", "image/tiff"},
      {"tiff", "image/tiff"},
      {"tr", "application/x-troff"},
      {"tsv", "text/tab-separated-values"},
      {"txt", "text/plain"},
      {"ustar", "application/x-ustar"},
      {"vcd", "application/x-cdlink"},
      {"vrml", "model/vrml"},
      {"vxml", "application/voicexml+xml"},
      {"wav", "audio/x-wav"},
      {"wbmp", "image/vnd.wap.wbmp"},
      {"wbmxl", "application/vnd.wap.wbxml"},
      {"wml", "text/vnd.wap.wml"},
      {"wmlc", "application/vnd.wap.wmlc"},
      {"wmls", "text/vnd.wap.wmlscript"},
      {"wmlsc", "application/vnd.wap.wmlscriptc"},
      {"wrl", "model/vrml"},
      {"xbm", "image/x-xbitmap"},
      {"xht", "application/xhtml+xml"},
      {"xhtml", "application/xhtml+xml"},
      {"xls", "application/vnd.ms-excel"},                        
      {"xml", "application/xml"},
      {"xpm", "image/x-xpixmap"},
      {"xsl", "application/xml"},
      {"xlsx","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
      {"xltx","application/vnd.openxmlformats-officedocument.spreadsheetml.template"},
      {"xlsm","application/vnd.ms-excel.sheet.macroEnabled.12"},
      {"xltm","application/vnd.ms-excel.template.macroEnabled.12"},
      {"xlam","application/vnd.ms-excel.addin.macroEnabled.12"},
      {"xlsb","application/vnd.ms-excel.sheet.binary.macroEnabled.12"},
      {"xslt", "application/xslt+xml"},
      {"xul", "application/vnd.mozilla.xul+xml"},
      {"xwd", "image/x-xwindowdump"},
      {"xyz", "chemical/x-xyz"},
      {"zip", "application/zip"}
    };

    public static string GetMIMEType(string fileName) {
      //get file extension
      string extension = Path.GetExtension(fileName).ToLowerInvariant();

      if (extension.Length > 0 &&
          MIMETypesDictionary.ContainsKey(extension.Remove(0, 1))) {
        return MIMETypesDictionary[extension.Remove(0, 1)];
      }
      return "unknown/unknown";
    }
  }


}
