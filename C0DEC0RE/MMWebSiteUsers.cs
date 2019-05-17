using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Xml;
using System.Web;
using System.Web.Caching;
using System.Web.Security;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace C0DEC0RE {

  public class MMWebSiteConstants {
   public static String DatabaseName() { return "PD"; }
  }
    
  public class MMWebSiteContext {
    HttpContext flhc;
    User flu;
    public MMWebSiteContext(HttpContext hc) {
      flhc = hc;      
      UserManager um = new UserManager();
      MMStrUtl x = new MMStrUtl();      
      string userName = hc.User.Identity.Name;
      if (x.ParseCount(userName, "\\") == 2) {
        flu = um.GetUser(x.ParseString(userName, "\\", 1)); // since first part is domain in windows
      } else {
        flu = um.GetUser(userName);  // using forms authentication
      }   
    }
    public string varAt(string title, string sdefault) {
      string s = "";
      try { s = flhc.Request.QueryString.GetValues(title)[0].ToString(); } catch {
        try { s = flhc.Request.Form.GetValues(title)[0].ToString(); } catch {
          try { s = flhc.Request.Params.GetValues(title)[0].ToString(); } catch {
            s = sdefault;
          }
        }
      }
      return s;
    }
    public User ActiveUser { get { return flu; } set { flu = value; } }
  }

  public class MMTrace {
    public void Trace(Int32 UserID, string TraceType, string TraceDetail) {
      MMData d = new MMData();
      try {
        DataSet Log = d.GetStProcDataSet("PD", "exec dbo.sp_AddTrace @aT_U_ID, @aT_Type, @aT_Details",
          new StProcParam[] {
          new StProcParam("aT_U_ID", DbType.Int32, UserID),          
          new StProcParam("aT_Type", DbType.String, TraceType),
          new StProcParam("aT_Details", DbType.String, TraceDetail)
        });
      } catch {  }      
    }
  }

  public class User {
    private string pDomain = string.Empty;
    private string pLoginName = string.Empty;
    private string pPassword = string.Empty;
    private string pName = string.Empty;
    private string pStaffName = string.Empty;
    private string pEmail = string.Empty;
    private string pSalter = string.Empty;
    private string pNotes = string.Empty;
    private Int32 pUserID = 0;
    private Int32 pU_C_ID = 0;
    private bool pIsActive = true;
    DateTime pPasswordExpires = DateTime.MaxValue;
    private bool pIsUserAdmin = false;
    private bool pIsInhouseStaff = false;
    private bool pRecDirty = false;
    private string getSALT() {
      if (pSalter == string.Empty) {
        Guid salter = Guid.NewGuid();
        pSalter = salter.ToString();
      }
      return pSalter;
    }    
    public void EncodePassword(string pwd) {
      if (pSalter == string.Empty) { getSALT(); };
      MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
      Byte[] hashedBytes;
      UTF8Encoding encoder = new UTF8Encoding();
      hashedBytes = md5Hasher.ComputeHash(encoder.GetBytes(pwd + pSalter));
      pPassword = BitConverter.ToString(hashedBytes);
      pRecDirty = true;
    }
    public string Domain { get { return this.pDomain; } set { this.pDomain = value; RecDirty = true; } }
    public string LoginName { get { return this.pLoginName; } set { this.pLoginName = value; RecDirty = true; } }
    public string Name { get { return this.pName; } set { this.pName = value; RecDirty = true; } }
    public string Password { get { return this.pPassword; } set { pPassword = value; RecDirty = true; } }
    public bool IsActive { get { return pIsActive; } set { pIsActive = value; RecDirty = true; } }
    public string Email { get { return pEmail; } set { pEmail = value; RecDirty = true; } }
    public string Salter { get { return getSALT(); } set { pSalter = value; RecDirty = true;} }
    public Int32 UserID { get { return pUserID; } set { pUserID = value; RecDirty = true; } }
    public Int32 U_C_ID { get { return pU_C_ID; } set { pU_C_ID = value; RecDirty = true; } }
    public DateTime PasswordExpires { get { return pPasswordExpires; } set { pPasswordExpires = value; } }    
    public string StaffName { get { return pStaffName; } set { pStaffName = value; } }    
    public bool IsUserAdmin { get { return pIsUserAdmin; } set { pIsUserAdmin = value; RecDirty = true; } }
    public bool IsInhouseStaff { get { return pIsInhouseStaff; } set { pIsInhouseStaff = value; RecDirty = true; } }    
    public bool RecDirty { get { return pRecDirty; } set { pRecDirty = value; } }
    public string ClientName { get {
      MMData d = new MMData();
      string sClientName = "";
      if (U_C_ID != 0) {
        try {
          DataSet x = d.GetStProcDataSet(MMWebSiteConstants.DatabaseName(), "select C_Name from dbo.vw_Client where C_ID = @aCID ", 
            new StProcParam[] { new StProcParam("@aCID", DbType.Int32, U_C_ID) });
          if ((x.Tables.Count == 1) & (x.Tables[0].Rows.Count == 1)) {
            sClientName = Convert.ToString(x.Tables[0].Rows[0]["C_Name"]);
          }
        } catch { }
      }
      return sClientName; 
    }}    
  }

  public sealed class UnknownUser : User { }

  public sealed class UserManager {
  #region string constants 
    private static readonly string policy = "LocalPolicy";
    public static readonly string SelectUser =
      "SELECT U_ID, U_C_ID, U_Login, U_PASSWORD, U_Name, U_Email, U_SALT, U_IsUserAdmin, U_IsInhouseStaff, U_Domain, U_IsActive " +
      "FROM Users U ";
    public static readonly string StrCreateUserTable =
      "CREATE TABLE [dbo].[Users]([U_ID] [int] IDENTITY(1,1) NOT NULL,[U_C_ID] [int] NOT NULL,[U_Login] [varchar](255) NOT NULL,[U_Password] [varchar](500) NOT NULL,"+Environment.NewLine+
      "  [U_Name] [varchar](500) NOT NULL,[U_Email] [varchar](500) NOT NULL,	[U_SALT] [varchar](255) NOT NULL,	[U_IsUserAdmin] [bit] DEFAULT ((0)) NOT NULL,	[U_IsInhouseStaff] [bit] DEFAULT ((0)) NOT NULL,"+Environment.NewLine+
      "  [U_Domain] [varchar](50) NULL,	[U_IsActive] [bit] DEFAULT ((1)) NOT NULL, CONSTRAINT [PK_Users_U_ID] PRIMARY KEY CLUSTERED (	[U_ID] ASC) "+Environment.NewLine+
      "    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]"+Environment.NewLine+
      ") ON [PRIMARY] ";
    public static readonly string StrCreateAddUpdateStoredProc =
      "Create Procedure[dbo].[sp_AddUpdateUsers00] ("+Environment.NewLine+
      " @aU_ID int, @aU_C_ID int, @aU_Login varchar(255), @aU_Password varchar(500), @aU_Name varchar(500), @aU_Email varchar(500), @aU_SALT varchar(255), @aU_IsUserAdmin bit, @aU_IsInhouseStaff bit, @aU_Domain varchar(50), @aU_IsActive bit "+Environment.NewLine+
      ") as "+Environment.NewLine+
      "  set nocount on"+Environment.NewLine+
      "  declare @a int set @a = isnull((select U_ID from dbo.Users where (U_ID = @aU_ID)), 0)      "+Environment.NewLine+
      "  if (@a = 0) begin"+Environment.NewLine+
      "    Insert into dbo.Users(U_C_ID, U_Login, U_Password, U_Name, U_Email, U_SALT, U_IsUserAdmin, U_IsInhouseStaff, U_Domain, U_IsActive"+Environment.NewLine+
      "    ) values(@aU_C_ID, @aU_Login, @aU_Password, @aU_Name, @aU_Email, @aU_SALT, @aU_IsUserAdmin, @aU_IsInhouseStaff, @aU_Domain, @aU_IsActive)"+Environment.NewLine+
      "    set @a = @@IDENTITY"+Environment.NewLine+
      "  end else begin"+Environment.NewLine+
      "    Update dbo.Users set"+Environment.NewLine+
      "      U_C_ID = @aU_C_ID, U_Login = @aU_Login, U_Password = @aU_Password, U_Name = @aU_Name, U_Email = @aU_Email, U_SALT = @aU_SALT, "+Environment.NewLine+
      "    U_IsUserAdmin = @aU_IsUserAdmin, U_IsInhouseStaff = @aU_IsInhouseStaff, U_Domain = @aU_Domain, U_IsActive = @aU_IsActive"+Environment.NewLine+
      "    where U_ID = @aU_ID"+Environment.NewLine+
      "  end"+Environment.NewLine+
      "  select @aU_ID U_ID"+Environment.NewLine+
      "return";
  #endregion
    public void UserUpdate(User toUpdate) {
      MMData d = new MMData();      
      DataSet ud = d.GetStProcDataSet(MMWebSiteConstants.DatabaseName(), "exec dbo.sp_AddUpdateUsers00 @aU_ID, @aU_C_ID, @aU_Login, @aU_Password, @aU_Name, @aU_Email, @aU_SALT, @aU_IsUserAdmin, @aU_IsInhouseStaff, @aU_Domain, @aU_IsActive ", 
        new StProcParam[] {
        new StProcParam("@aU_ID", DbType.Int32, toUpdate.UserID),
        new StProcParam("@aU_C_ID", DbType.Int32, toUpdate.U_C_ID),
        new StProcParam("@aU_Login", DbType.String, toUpdate.LoginName),
        new StProcParam("@aU_Password", DbType.String, toUpdate.Password),
        new StProcParam("@aU_Name", DbType.String, toUpdate.Name),
        new StProcParam("@aU_Email", DbType.String, toUpdate.Email),
        new StProcParam("@aU_SALT", DbType.String, toUpdate.Salter),
        new StProcParam("@aU_IsUserAdmin", DbType.Boolean, toUpdate.IsUserAdmin),        
        new StProcParam("@aU_IsInhouseStaff", DbType.Boolean, toUpdate.IsInhouseStaff),        
        new StProcParam("@aU_Domain", DbType.String, toUpdate.Domain),
        new StProcParam("@aU_IsActive", DbType.Boolean, toUpdate.IsActive)
      });    
    }
    public User GetUser(string userName) {
      User user = new UnknownUser();
      MMData d = new MMData();        
      try {
          DataSet x = d.GetStProcDataSet(MMWebSiteConstants.DatabaseName(), SelectUser + " WHERE (U_Login = @aLogin) and (U_IsActive=1)",
          new StProcParam[] { new StProcParam("@aLogin", DbType.AnsiString, userName) });
        if ((x.Tables.Count == 1) & (x.Tables[0].Rows.Count == 1)) {
          user = BuildUserFromRow(x.Tables[0].Rows[0]);
        }        
      } catch { }
      return user;
    }
    public User GetUserID(string userID) {
      User user = new UnknownUser();
      MMData d = new MMData();
      try {
          DataSet x = d.GetStProcDataSet(MMWebSiteConstants.DatabaseName(), SelectUser + "WHERE (U_ID = cast( @aID as int ))",
          new StProcParam[] { new StProcParam("@aID", DbType.String, userID) });
        if ((x.Tables.Count == 1) & (x.Tables[0].Rows.Count == 1)) {
          user = BuildUserFromRow(x.Tables[0].Rows[0]);
        }
      }  catch { }
      return user;
    }
    private User BuildUserFromRow(DataRow userRecord) {      
      User user = new User();
      user.IsActive = Convert.ToBoolean(userRecord["U_IsActive"]);
      user.UserID = Convert.ToInt32(userRecord["U_ID"]);
      user.U_C_ID = Convert.ToInt32(userRecord["U_C_ID"]);
      user.Domain = userRecord["U_Domain"].ToString();
      user.LoginName = userRecord["U_Login"].ToString();
      user.Password = userRecord["U_PASSWORD"].ToString();
      user.Name = userRecord["U_Name"].ToString();
      user.Email = userRecord["U_Email"].ToString();
      user.Salter = userRecord["U_Salt"].ToString();
      user.IsUserAdmin = Convert.ToBoolean(userRecord["U_IsUserAdmin"]);
      user.IsInhouseStaff = Convert.ToBoolean(userRecord["U_IsInhouseStaff"]);      
      user.PasswordExpires = DateTime.Now.AddYears(20);
      user.RecDirty = false;      
      return user;
    }
    public bool Authenticate(string userName, string password) {
      bool authented = false;
      User user = GetUser(userName);
      if ((user is UnknownUser)|| (!user.IsActive)) {
        return authented;
      } else {        
        string encrPassword = user.Password;
        MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
        Byte[] hashedBytes;
        UTF8Encoding encoder = new UTF8Encoding();
        hashedBytes = md5Hasher.ComputeHash(encoder.GetBytes(password + user.Salter));
        string reHashedPassword = BitConverter.ToString(hashedBytes);
        authented = reHashedPassword == encrPassword ? true : false;        
      }
      return authented;
    }   
    private int getRoleID(string role) {
      int roleID = -1;   
      return roleID;
    }

    public void EnsureUserObject(string SqlConStrName) {

      string sSQL = "";
      


    } 
  }
 
  public class LocalRoleProvider : RoleProvider {
    private UserManager userManager = new UserManager();
    private string applicationName = "App";
    public override string ApplicationName {get {return applicationName;} set { applicationName = value;}}
    public override void CreateRole(string roleName) { throw new NotSupportedException("The method or operation is not implemented."); }
    public override void AddUsersToRoles(string[] usernames, string[] roleNames){throw new NotSupportedException("The method or operation is not implemented."); }   
    public override bool DeleteRole(string roleName, bool throwOnPopulatedRole) {throw new NotSupportedException("The method or operation is not implemented."); }
    public override string[] FindUsersInRole(string roleName, string usernameToMatch) {throw new NotSupportedException("The method or operation is not implemented."); }
    public override string[] GetAllRoles(){throw new NotSupportedException("The method or operation is not implemented."); }
    public override string[] GetRolesForUser(string username) {throw new NotSupportedException("The method or operation is not implemented."); }
    public override string[] GetUsersInRole(string roleName) {throw new NotSupportedException("The method or operation is not implemented."); }
    public override bool IsUserInRole(string username, string roleName) {throw new NotSupportedException("The method or operation is not implemented."); }
    public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames) {throw new NotSupportedException("The method or operation is not implemented."); }
    public override bool RoleExists(string roleName) {throw new NotSupportedException("The method or operation is not implemented."); }    
  }

  public class LocalMembershipProvider : MembershipProvider {
    private string applicationName = "";
    private string name = "LocalMembershipProvider";
    private UserManager userManager = new UserManager();
    private static readonly string policy = "LocalPolicy";
    private bool pEnablePasswordReset;
    private bool pEnablePasswordRetrieval;
    private bool pRequiresQuestionAndAnswer;
    private bool pRequiresUniqueEmail;
    private int pMaxInvalidPasswordAttempts;
    private int pPasswordAttemptWindow;
    private int pMinRequiredNumericPasswordLength;
    private int pMinRequiredNonAlphanumericCharacters;
    private int pMinRequiredPasswordLength;
    private string pPasswordStrengthRegularExpression;
    private int pPasswordExpiresInDays;
    private MembershipPasswordFormat pPasswordFormat = 0;
    public LocalMembershipProvider() { }
    public override string ApplicationName { get { return applicationName; } set { applicationName = value; } }
    public override string Name { get { return name; } }
    public override bool EnablePasswordReset { get { return pEnablePasswordReset; } }
    public override bool EnablePasswordRetrieval { get { return pEnablePasswordRetrieval; } }
    public override bool RequiresQuestionAndAnswer { get { return pRequiresQuestionAndAnswer; } }
    public override bool RequiresUniqueEmail { get { return pRequiresUniqueEmail; } }
    public override int MaxInvalidPasswordAttempts { get { return pMaxInvalidPasswordAttempts; } }
    public override int PasswordAttemptWindow { get { return pPasswordAttemptWindow; } }
    public override MembershipPasswordFormat PasswordFormat { get { return pPasswordFormat; } }
    public override int MinRequiredNonAlphanumericCharacters { get { return pMinRequiredNonAlphanumericCharacters; } }
    public override int MinRequiredPasswordLength { get { return pMinRequiredPasswordLength; } }
    public int PasswordExpiresInDays { get { return pPasswordExpiresInDays; } }
    public int MinRequiredNumericPasswordLength { get { return pMinRequiredNumericPasswordLength; } }
    public override string PasswordStrengthRegularExpression { get { return pPasswordStrengthRegularExpression; } }
    public bool IsValidPassword(string password) {      
      string regex = "";
      regex = @"[a-zA-Z0-9]{" + pMinRequiredPasswordLength + @",}";
      Regex regValidate = new Regex(regex);
      Match m = regValidate.Match(password);
      if (!m.Success) { return false; }
      if (pMinRequiredNumericPasswordLength > 0) {
        regex = @"[0-9]{" + pMinRequiredNumericPasswordLength + @",}";
        regValidate = new Regex(regex);
        m = regValidate.Match(password);
        if (!m.Success) {
          return false;
        }
      }
      return true;      
    }
    public override bool ChangePassword(string username, string oldPassword, string newPassword) { throw new NotSupportedException("The method or operation is not implemented."); }
    public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer) { throw new NotSupportedException("The method or operation is not implemented."); }

    public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status) {
      try {   // does user exist ? 
        User u = userManager.GetUser(username);
        if (!(u is UnknownUser)) {
          status = MembershipCreateStatus.DuplicateUserName;
          return BuildMembershipUser(u);
        } else {  //  if not add user.
            u = new User();            
            u.LoginName = username;
            u.EncodePassword(password);
            u.Email = email;
            userManager.UserUpdate(u);
            status = MembershipCreateStatus.Success;
            return BuildMembershipUser(userManager.GetUser(username));            
        }
      } catch {
        status = MembershipCreateStatus.ProviderError;
        return null;
      }      
    }

    public override bool DeleteUser(string username, bool deleteAllRelatedData) { throw new NotSupportedException("The method or operation is not implemented."); }
    private string GetConfigValue(string configValue, string defaultValue) {
      if (String.IsNullOrEmpty(configValue)) return defaultValue;
      return configValue;
    }
    public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config) {      
      if (config == null) { throw new ArgumentException("config"); }
      if (String.IsNullOrEmpty(name)) { name = "LocalMembershipProvider"; }
      if (string.IsNullOrEmpty(config["description"])) {
        config.Remove("description");
        config.Add("description", "Website Membership provider");
      }

      this.name = name;
      base.Initialize(name, config);
      //pApplicationName = GetConfigValue(config["applicationName"], System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
      pMaxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config["maxInvalidPasswordAttempts"], "5"));
      pPasswordAttemptWindow = Convert.ToInt32(GetConfigValue(config["passwordAttemptWindow"], "10"));
      pMinRequiredNonAlphanumericCharacters = 0; // Convert.ToInt32(GetConfigValue(config["minRequiredNonAlphanumericCharacters"], "1"));
      pMinRequiredPasswordLength = Convert.ToInt32(GetConfigValue(config["minRequiredPasswordLength"], "7"));
      pMinRequiredNumericPasswordLength = Convert.ToInt32(GetConfigValue(config["minRequiredNumericCharacters"], "0"));
      pPasswordStrengthRegularExpression = Convert.ToString(GetConfigValue(config["passwordStrengthRegularExpression"], ""));
      pEnablePasswordReset = Convert.ToBoolean(GetConfigValue(config["enablePasswordReset"], "true"));
      pEnablePasswordRetrieval = Convert.ToBoolean(GetConfigValue(config["enablePasswordRetrieval"], "true"));
      pRequiresQuestionAndAnswer = Convert.ToBoolean(GetConfigValue(config["requiresQuestionAndAnswer"], "false"));
      pRequiresUniqueEmail = Convert.ToBoolean(GetConfigValue(config["requiresUniqueEmail"], "true"));
      pPasswordExpiresInDays = Convert.ToInt32(GetConfigValue(config["passwordExpirationDays"], "-1"));
      //pWriteExceptionsToEventLog = Convert.ToBoolean(GetConfigValue(config["writeExceptionsToEventLog"], "true"));     
      
    }
    public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords) { throw new NotSupportedException("The method or operation is not implemented."); }
    public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords) { throw new NotSupportedException("The method or operation is not implemented."); }
    public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords) { throw new NotSupportedException("The method or operation is not implemented."); }
    public override int GetNumberOfUsersOnline() { throw new NotSupportedException("The method or operation is not implemented."); }
    public override string GetPassword(string username, string answer) { throw new NotSupportedException("The method or operation is not implemented."); }
    public override MembershipUser GetUser(string username, bool userIsOnline) {      
      User u = userManager.GetUser(username);
      if (!(u is UnknownUser)) {
        return BuildMembershipUser(u);
      } else {
        return null;
      }
    }
    public override MembershipUser GetUser(object providerUserKey, bool userIsOnline) { throw new NotSupportedException("The method or operation is not implemented."); }
    public override string GetUserNameByEmail(string email) { throw new NotSupportedException("The method or operation is not implemented."); }
    public override string ResetPassword(string username, string answer) { throw new NotSupportedException("The method or operation is not implemented."); }
    public override bool UnlockUser(string userName) { throw new NotSupportedException("The method or operation is not implemented."); }
    public override void UpdateUser(MembershipUser user) {throw new NotSupportedException("The method or operation is not implemented."); }
    public override bool ValidateUser(string username, string password) {
      return userManager.Authenticate(username, password);
    }
    private static MembershipUser BuildMembershipUser(User user) {
      MembershipUser membershipUser = null;
      membershipUser = new MembershipUser(
        "LocalMembershipProvider",
        user.LoginName,
        user.UserID.ToString(),
        user.Email,
        String.Empty,
        user.Name,
        true,
        false,
        DateTime.Now,
        DateTime.Now,
        DateTime.Now,
        DateTime.Now,
        new DateTime(1980, 1, 1));    
      return membershipUser;
    }
  }



}
