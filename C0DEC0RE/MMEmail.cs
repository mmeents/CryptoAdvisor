using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Net.Mail;

namespace C0DEC0RE
{
  
  public class Addr{
    private string fName;
    private string fEmail;
    public Addr(string sName, string sEmail){
      fName = sName;
      fEmail = sEmail;
    }     
    public string Name(){
      return this.fName; 
    }
    public string Email(){
      return this.fEmail;
    }
    public string MakeAddrString() {
      return "  <EmailAddress name=\""+fName+"\" email=\""+fEmail+"\" />";
    }
  }

  public class AddrList{
    private List<Addr> fTo;
    private List<Addr> fCC;
    private List<Addr> fBCC;
    public AddrList() { 
      fTo = new List<Addr>();
      fCC = new List<Addr>();
      fBCC = new List<Addr>();
    }
    public void AddTo(string sName, string sEmail) {
      Addr aTo = new Addr(sName, sEmail);
      fTo.Add(aTo);
    }
    public void AddCC(string sName, string sEmail) {
      Addr aTo = new Addr(sName, sEmail);
      fCC.Add(aTo);
    }
    public void AddBCC(string sName, string sEmail) {
      Addr aTo = new Addr(sName, sEmail);
      fBCC.Add(aTo);
    }
    public string MakeAddrListString() {
      StringBuilder r = new StringBuilder();
   //   r.Clear();
        
      r.AppendLine("<To>");
      if (fTo.Count>0){
        foreach(Addr a in fTo){
          r.AppendLine(a.MakeAddrString());
        }
      }
      r.AppendLine("</To>");

      r.AppendLine("<CC>");
      if (fCC.Count>0){
        foreach(Addr a in fCC){
          r.AppendLine(a.MakeAddrString());
        }
      }
      r.AppendLine("</CC>");

      r.AppendLine("<BCC>");
      if (fBCC.Count>0){
        foreach(Addr a in fBCC){
          r.AppendLine(a.MakeAddrString());
        }
      }
      r.AppendLine("</BCC>");     
      return r.ToString();
    }

  }

  public class EMailBuilder {
    private AddrList fAddrList;
    private String fSubject;
    private String fBody;
    private String fGMailAddr;
    private String fGMailPwd;
    public EMailBuilder(String aGMailAddr, String sGMailPwd, String sSubject, String sBody ) {
      fAddrList = new AddrList();
      fGMailAddr = aGMailAddr;
      fGMailPwd = sGMailPwd;
      fSubject = sSubject;
      fBody = sBody;
    }

    public String Subject { get { return fSubject; } set { fSubject = value; } }
    public String Body { get { return fBody; } set { fBody = value; } }
    public AddrList Recipients { get { return fAddrList; } set { fAddrList = value; } }

    public String MakeEmailXML() {      
      string r ="<Message>"+Recipients.MakeAddrListString()+
           "<Subject><![CDATA["+ Subject +"]]></Subject>"+
           "<Body><![CDATA["+Body.toBase64EncodedStr()+"]]></Body>"+
     // CompileAttachList2(sAttachments) + 
           "</Message>";
      return r;
    }

    public void Send() {
      SendMessage(MakeEmailXML());
    }

    public String SendMessage(String MsgParamXML) {
           
      String sResult = "";
      Boolean hasTo = false, hasSub = false, hasMess = false;
      XmlDocument Doc = new XmlDocument();
      MailMessage msg = new MailMessage();
      try {

        Doc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?>" + MsgParamXML);
        XmlElement DocRoot = Doc.DocumentElement;

        foreach (XmlElement z in DocRoot.ChildNodes) {
          if (z.Name == "To") {
            foreach (XmlElement y in z.ChildNodes) {
              try {
                //EmailAddress aTo = new EmailAddress(y.GetAttribute("name"), y.GetAttribute("email"));
                //msg.ToRecipients.Add(aTo);
                MailAddress aAddr = new MailAddress(y.GetAttribute("email"),y.GetAttribute("name"));
                msg.To.Add(aAddr);
                hasTo = true;
              } catch (Exception e01) {
                sResult = sResult + "Error: Setting Address for To Failed with " + e01.Message + "\n";
                throw;
              }
            }
          } else if (z.Name == "CC") {
            foreach (XmlElement y in z.ChildNodes) {
              try {
                MailAddress aAddr = new MailAddress(y.GetAttribute("email"),y.GetAttribute("name")); 
                msg.CC.Add(aAddr);
                
                hasTo = true;
              } catch (Exception e02) {
                sResult = sResult + "Error: Setting Address for CC Failed with " + e02.Message + "\n";
                throw;
              }
            }
          } else if (z.Name == "BCC") {
            foreach (XmlElement y in z.ChildNodes) {
              try {
                MailAddress aAddr = new MailAddress(y.GetAttribute("email"),y.GetAttribute("name")); 
                msg.Bcc.Add(aAddr);
                               
                hasTo = true;
              } catch (Exception e03) {
                sResult = sResult + "Error: Setting Address for BCC Failed with " + e03.Message + "\n";
                throw;
              }
            }
          } else if (z.Name == "Subject") {
            try {
              msg.Subject = z.InnerText;
              hasSub = true;
            } catch (Exception e04) {
              sResult = sResult + "Error: Setting Subject Failed with " + e04.Message + "\n";
              throw;
            }
          } else if (z.Name == "Body") {
            try {
              byte[] decbuff = Convert.FromBase64String(z.InnerText);
              msg.Body = System.Text.Encoding.UTF8.GetString(decbuff);
              msg.IsBodyHtml = true;
              hasMess = true;
            } catch (Exception e05) {
              sResult = sResult + "Error: Message Body Fails with " + e05.Message + "\n";
              throw;
            }
          } else if (z.Name == "Attachments") {            
            try{
            } catch (Exception e06) {
              sResult = sResult + "Error: Attachment Failed with " + e06.Message + "\n";
              throw;
            }
          }
        }

        msg.From = new MailAddress(fGMailAddr, "No Reply");

        if (hasTo && hasMess && hasSub) {
          try {                        

            /*  GMail Send 
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Credentials = new System.Net.NetworkCredential(fGMailAddr, fGMailPwd);
            smtp.Timeout = 30000;                           
            smtp.Send(msg);  */

            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.office365.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Credentials = new System.Net.NetworkCredential(fGMailAddr, fGMailPwd);
            smtp.Timeout = 30000;                           
            smtp.Send(msg);  


          } catch (Exception e07) {
            sResult = sResult + "Error: On Save Send Failed with " + e07.Message + " \n";
            throw;
          }
        } else {
          throw new Exception("Message lacked a Address or Subject or Message.");
        }
      } catch (Exception eA0) {
        sResult = (sResult.Trim() != "" ? sResult : "Error: " + eA0.Message);
      }      
      return "";      
    }  
  }
    
}
