using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalvingPredict
{
    class LogWriter
    {
        public void Write(string note)
        {
            Log lg = new Log
            {
                user_id = "Admin",
                page = "http://193.27.73.63/Predict/CPStart.html",
                function_query = "Predict",
                error = "",
                note = note,
                datestamp = DateTime.UtcNow,
                recipient = Properties.Settings.Default.emails
            };
            try
            {
                using (DB_A4A060_csEntities context = new DB_A4A060_csEntities())
                {
                    context.Logs.Add(lg);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                EmailBox em = new EmailBox();
                em.SendEmail(   "info@cattlescan.ca", 
                                Properties.Settings.Default.UserName, 
                                "Predict - " + ex.Message, 
                                "Predict error");
                throw;
            }
        }
    }
}
