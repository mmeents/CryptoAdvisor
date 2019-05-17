using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using System.Reflection;

namespace C0DEC0RE {

  [Serializable]
	public class DbConnectionInfo	{
		private string m_connectionName=null;	
		private string m_userName=String.Empty;
		private string m_password=String.Empty;
		private string m_serverName=String.Empty;
		private string m_initialCatalog=String.Empty;
		private bool m_useIntegratedSecurity=false;
		public DbConnectionInfo()	{			
		}
		public DbConnectionInfo(string connectionName, string connectionString) {
			if (connectionName==null ||	connectionString==null) { 
			  throw new ArgumentNullException(); 
			}	else {
				m_connectionName=connectionName;
				SetConnectionString(connectionString);
			}
		}		
		public string ConnectionName {
			get	{	return m_connectionName; }
      set {
        if (value == null) { 
          throw new ArgumentNullException(); 
        }
        m_connectionName = value;
      }
		} 

		public string ConnectionString{
			get	{	return GetConnectionString();	}
			set	{ if (value==null){throw new ArgumentNullException();}
				SetConnectionString(value);
			}
		}
	
		public string UserName {	get { return m_userName; }set { m_userName = value; }}

		public string Password {	get { return m_password; }set { m_password = value; }}

		public string ServerName{	get { return m_serverName; }set { m_serverName = value; }}

		public string InitialCatalog{ get { return m_initialCatalog; }set { m_initialCatalog = value; }}

		public bool UseIntegratedSecurity {	get { return m_useIntegratedSecurity; }	set { m_useIntegratedSecurity = value; }}

		private string GetConnectionString() {		// Data Source=(local);Integrated Security=true;Database=ebcs;
			StringBuilder sb=new StringBuilder();
			sb.Append("Data Source=");
			sb.Append(m_serverName);
			sb.Append(";");
			sb.Append("Initial Catalog=");
			sb.Append(m_initialCatalog);
			sb.Append(";");
			if (m_useIntegratedSecurity==false)	{
				sb.Append("User ID=");
				sb.Append(m_userName);
				sb.Append(";");
				sb.Append("Password=");
				sb.Append(m_password);
				sb.Append(";");
			} else {
				sb.Append("Integrated Security=SSPI;");
			}
      sb.Append("TransparentNetworkIPResolution=False;");
			return sb.ToString();
		}

		private void SetConnectionString(string connstr){
			Hashtable connStringKeys=new Hashtable();
			string[] keysBySemicolon=connstr.Split(';');
			string[] keysByEquals;
			foreach(string keySemicolon in keysBySemicolon)	{
				keysByEquals=keySemicolon.Split('=');

				if (keysByEquals.Length==0)	{
					// do nothing
				}	else if (keysByEquals.Length==1) {
					// assume key name but no value
					connStringKeys.Add(keysByEquals[0].ToUpper(), "");
				}	else 	{
					connStringKeys.Add(keysByEquals[0].ToUpper(), keysByEquals[1]);
				}
			}

			if (connStringKeys.ContainsKey("SERVER")==true){
				m_serverName=(string)connStringKeys["SERVER"];
			}	else {
			  if (connStringKeys.ContainsKey("DATA SOURCE")==true){
			    m_serverName=(string)connStringKeys["DATA SOURCE"];
			  } else {
				  m_serverName="";
				}
			}

			if (connStringKeys.ContainsKey("DATABASE")==true)	{
				m_initialCatalog=(string)connStringKeys["DATABASE"];
			}	else {
			  if (connStringKeys.ContainsKey("INITIAL CATALOG")==true) {
			    m_initialCatalog=(string)connStringKeys["INITIAL CATALOG"];
			  } else {
				  m_initialCatalog="";
				}
			}		
			
			if (connStringKeys.ContainsKey("USER")==true) {
				m_userName=(string)connStringKeys["USER"];
			}	else {
				if (connStringKeys.ContainsKey("USER ID")==true) {
			    m_userName=(string)connStringKeys["USER ID"];
		    }	else {
			    m_userName="";
		    }
			}

			if (connStringKeys.ContainsKey("PASSWORD")==true)	{
				m_password=(string)connStringKeys["PASSWORD"];
			}	else{
				m_password="";
			}
			
			if (connStringKeys.ContainsKey("INTEGRATED SECURITY")==true) {
				m_useIntegratedSecurity=true;
			} else {
				m_useIntegratedSecurity=false;
			}
		}

		public override string ToString()	{
		  return m_connectionName + " (SqlServer)";				
		}
	
	} // end of class

  public class FileVar {
    string FileName;
    Dictionary<string, string> cache;
    public FileVar(string sFileName)
    {
      FileName = sFileName;
      cache = new Dictionary<string, string>();
    }
    private void SetVarValue(string VarName, string VarValue)
    {
      try
      {
        IniFile f = IniFile.FromFile(FileName);
        f["Variables"][VarName] = VarValue;
        f.Save(FileName);
        cache[VarName] = VarValue;
      }
      catch (Exception e)
      {
        throw e;
      }
    }
    private string GetVarValue(string VarName)
    {
      string result = "";
      try
      {
        if (cache.ContainsKey(VarName))
        {
          result = cache[VarName];
        }
        else
        {
          IniFile f = IniFile.FromFile(FileName);
          result = f["Variables"][VarName];
          cache[VarName] = result;
        }
      }
      catch { }
      return result;
    }
    public void RemoveVar(string VarName){ 
      IniFile f = IniFile.FromFile(FileName);        
      f["Variables"].DeleteKey(VarName);
      f.Save(FileName);
      if(cache.ContainsKey(VarName)){ 
        cache.Remove(VarName);
      }

    }
    public System.Collections.ObjectModel.ReadOnlyCollection<string> getVarNames(){ 
      IniFile f = IniFile.FromFile(FileName);        
      return f["Variables"].GetKeys(); 
    }
    public string this[string VarName] { get { return GetVarValue(VarName); } set { SetVarValue(VarName, value); } }
  }

  public class MMConMgr {
    private string sDefaultName = "ConnectGroupAlpha";
    private string sDefaultPass = "mConMgrBaseAlpha";
    public string FileName = "";
    public string sProvider = "System.Data.SqlClient";
    public FileVar ivFile;
    private KeyPair kpBaseKey;
    public MMConMgr() {      
      typeof(ConfigurationElementCollection).GetField("bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ConfigurationManager.ConnectionStrings, false);
      ConfigurationManager.ConnectionStrings.Clear();
      kpBaseKey = new KeyPair(KeyType.AES, sDefaultPass);
      FileName = MMExt.MMConLocation() + "\\" + sDefaultName + ".cons";
      if (!Directory.Exists(MMExt.MMConLocation() + "\\"))
      {
        Directory.CreateDirectory(MMExt.MMConLocation() + "\\");
      }
      ivFile = new FileVar(FileName);
      Load();
    }
    public MMConMgr(string sFileName, string sPassword)
    {
      typeof(ConfigurationElementCollection).GetField("bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ConfigurationManager.ConnectionStrings, false);
      ConfigurationManager.ConnectionStrings.Clear();
      kpBaseKey = new KeyPair(KeyType.AES, sPassword);
      FileName = MMExt.MMConLocation() + "\\" + sFileName + ".cons";
      if (!Directory.Exists(MMExt.MMConLocation() + "\\"))
      {
        Directory.CreateDirectory(MMExt.MMConLocation() + "\\");
      }
      ivFile = new FileVar(FileName);
      Load();
    }
    public void Load()
    {
      string s = ivFile["ConnectionCount"];
      if (s == "")
      {
        ivFile["ConnectionCount"] = "0";
        s = "0";
      }
      Int32 iConCount = 0;
      if(Int32.TryParse(s, out iConCount)){
        if(iConCount > 0) {
          for(Int32 i = 1; i <= iConCount; i++) {
            string sConName = ivFile["Con" + i.ToString() + "Name"];
            string sConConnection = kpBaseKey.NextKeyPair(i).toDecryptAES(ivFile["Con" + i.ToString() + "String"]);
            string sConProvider = ivFile["Con" + i.ToString() + "Provider"];
            Add(sConName, sConConnection, sConProvider);
          }
        }
      }
    }
    public void Add(string sConName, string sConStr, string sConPro)
    {
      ConnectionStringSettings cs = new ConnectionStringSettings(sConName, sConStr, sConPro);
      ConfigurationManager.ConnectionStrings.Add(cs);
    }
    public void Write()
    {
      Int32 iConCount = ConfigurationManager.ConnectionStrings.Count;
      ivFile["ConnectionCount"] = iConCount.ToString();
      Int32 i = 1;
      foreach (ConnectionStringSettings sx in ConfigurationManager.ConnectionStrings)
      {
        ivFile["Con" + i.ToString() + "Name"] = sx.Name;
        ivFile["Con" + i.ToString() + "String"] = kpBaseKey.NextKeyPair(i).toAESCipher(sx.ConnectionString);
        ivFile["Con" + i.ToString() + "Provider"] = sx.ProviderName;
        i++;
      }
    }
    public bool Edit(string sConName) {            
      ConnectionStringSettings cx = GetConnectionStringSetting(sConName);      
      ConnectionDetail aCD = new ConnectionDetail();
      if (cx!=null){
        aCD.dbCI = new DbConnectionInfo(sConName, cx.ConnectionString);
      } else {
        aCD.dbCI = new DbConnectionInfo();
      }
      aCD.dbCI.UseIntegratedSecurity = false;
      bool bOK = false;
      if (aCD.ShowDialog() == DialogResult.OK) {
        bOK = true;
        if (cx==null){
          Add(aCD.dbCI.ConnectionName, aCD.dbCI.ConnectionString, sProvider);  
        } else {
          Int32 iIndex = ConfigurationManager.ConnectionStrings.IndexOf(cx);
          ConfigurationManager.ConnectionStrings[iIndex].Name = aCD.dbCI.ConnectionName;
          ConfigurationManager.ConnectionStrings[iIndex].ConnectionString = aCD.dbCI.ConnectionString;
        }

      }
      return bOK;
    }
    public ConnectionStringSettings GetConnectionStringSetting(string sConName){
      ConnectionStringSettings cs = null;
      foreach (ConnectionStringSettings sx in ConfigurationManager.ConnectionStrings){
        if (sx.Name == sConName) {          
          cs = sx;
          break;



        }
      }
      return cs;      
    }
  }
 
}
