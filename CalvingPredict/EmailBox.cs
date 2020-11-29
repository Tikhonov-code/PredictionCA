using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace CalvingPredict
{
    class EmailBox
    {
        public string SendEmail(string strFrom, string strTo, string MailMsg, string strSubject)
        {
            LogWriter lg = new LogWriter();
            string result = string.Empty;
            MailMessage m = new MailMessage();

            SmtpClient sc = new SmtpClient(Properties.Settings.Default.Host);
            m.From = new MailAddress(Properties.Settings.Default.UserName);
            m.To.Add(strTo);
            m.Subject = strSubject;
            m.Body = "Сообщение от: " + strFrom + " \r\nТекст:\r\n" + MailMsg;
            {
                try
                {
                    sc.Port = 587;
                    sc.Credentials = new System.Net.NetworkCredential(Properties.Settings.Default.UserName, Properties.Settings.Default.Password);
                    sc.EnableSsl = true;
                    sc.Send(m);
                    result = "Email Send successfully";
                }
                catch (Exception ex)
                {
                    result += ex.Message;
                    lg.Write("predict :" + ex.Message);
                }
            }

            return result;
        }

    }
}
