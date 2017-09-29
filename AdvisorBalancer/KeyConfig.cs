using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Advisor {
  public partial class KeyConfig : Form {
    public KeyConfig() {
      InitializeComponent();
    }

    public string PrivateKey() {
      return edPrivateKey.Text;
    }
    public string PublicKey() {
      return edPublicKey.Text;
    }

    public string AppPassword() {
      return edPassword.Text;
    }

    private void KeyConfig_Shown(object sender, EventArgs e) {
      KeyPair kp = null;
      Boolean hasKP = true;
      try {
        kp = KeyPair.ReadEncoded(false);
      } catch {
        hasKP = false;
      }
      if (hasKP) {
        edPublicKey.Text = kp.sPU();
        edPrivateKey.Text = kp.sPR();
      }
    }

    private void btnSave_Click(object sender, EventArgs e) {

    }
  }

  public class KeyPair {
    private string AppPassword;
    private string PublicKey;
    private string PrivateKey;
    public KeyPair(string sAppPassword, string sPublicKey, string sPrivateKey) {
      AppPassword = sAppPassword;
      PublicKey = sPublicKey;
      PrivateKey = sPrivateKey;
    }
    public static KeyPair LoadEnc(string sPassword, string sKeyParEnc) {
      string sBox = sKeyParEnc.Decrypt();// su.Decrypt(sKeyParEnc);
      string sPublicKey = sBox.ParseString(" ", 0);
      string sPrivateKey = sBox.ParseString(" ", 1); 
      return new KeyPair(sPassword, sPublicKey, sPrivateKey);
    }
    public static KeyPair LoadEnc2(string sPassword, string sKeyParEnc) {
      string sBox = ""; 
      try {
        sBox = sKeyParEnc.toDecryptAES(sPassword);// su.Decrypt(sKeyParEnc);
      } catch (Exception eee) {
        MessageBoxButtons aMBB = MessageBoxButtons.OK;
        MessageBox.Show( "Advisor will now terminate.","Error: The Password did not unlock the Key.", aMBB);
        throw new Exception("Incorrect Password.");
      }
      string sPublicKey = sBox.ParseString(" ", 0);
      string sPrivateKey = sBox.ParseString(" ", 1);
      return new KeyPair(sPassword, sPublicKey, sPrivateKey);
    }
    public string ToStringEnc() {
      string s = PublicKey + " " + PrivateKey;
      return s.toAESCipher(AppPassword);
      //return s.Encrypt();
    }
    public string sPU() {
      return PublicKey;
    }
    public string sPR() {
      return PrivateKey;
    }

    public static KeyPair ReadEncoded(Boolean bGetFromUser) {
      KeyPair rKP = null;
      string sFileName = UserStorageFile();
      IniFile f1 = IniFile.FromFile(sFileName);
      String sMsgAlgo = f1["Msg"]["Ax", "0"];
      String sMsgText = f1["Msg"]["Xx"];
         

      if (sMsgAlgo == "0") {
        if ((sMsgText == null) && (bGetFromUser)) {
          rKP = DoGetKeysDlg();
          if (rKP != null) {
            rKP.WriteEncoded();
          }
        } else {
          string sPassword = "";
          FormPassword dlg = new FormPassword();
          DialogResult res = dlg.ShowDialog();
          if (res == DialogResult.OK) {
            sPassword = dlg.GetPassword();
          }
          rKP = LoadEnc( sPassword, sMsgText);
          rKP.WriteEncoded();
        }
      } else if (sMsgAlgo == "1") {

        string sPassword = "";
        FormPassword dlg = new FormPassword();
        DialogResult res = dlg.ShowDialog();
        if (res == DialogResult.OK) {
          sPassword = dlg.GetPassword();
        }
        rKP = LoadEnc2(sPassword, sMsgText);
      }
      return rKP;
    }
    public void WriteEncoded() {
      string sFileName = UserStorageFile();
      IniFile f = new IniFile();
      f["Msg"]["Ax"] = "1";
      f["Msg"]["Xx"] = this.ToStringEnc();
      f.Save(sFileName);
    }
    public static string UserStorageFile() {
      String sUserDataDir = Application.UserAppDataPath;
      String sAddressFile = sUserDataDir + "PoloAPIKeys.txt";
      if (!Directory.Exists(sUserDataDir)) {
        Directory.CreateDirectory(sUserDataDir);
      }
      return sAddressFile;
    }
    public static KeyPair DoGetKeysDlg() {
      KeyPair rKP = null;
      KeyConfig dlg = new KeyConfig();
      DialogResult res = dlg.ShowDialog();
      if (res == DialogResult.OK) {
        string sPW = dlg.AppPassword();
        string sPU = dlg.PublicKey();
        string sPR = dlg.PrivateKey();
        rKP = new KeyPair(sPW, sPU, sPR);
      }
      return rKP;
    }
  }
}
