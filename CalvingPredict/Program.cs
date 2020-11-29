using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalvingPredict
{
    class Program
    {
        static void Main(string[] args)
        {
            //1. Get animal's list for prediction
            List<SP_Predict_AnimalList_Result> animal_list = new List<SP_Predict_AnimalList_Result>();
            switch (Properties.Settings.Default.InputDataSoure)
            {
                case "database":
                    animal_list = GetAnimalIdList();//.Where(x => x.bolus_id == 300).ToList();
                    break;
                default:
                    // get animal list form input parameter
                    //if (args.Length == 0) System.Environment.Exit(-1);
                    //for (int i = 0; i < args.Length; i++)
                    //{
                    //    animal_list.Add(Convert.ToInt16(args[i]));
                    //}
                    break;
            }

            //3. run prediction routines
            if (animal_list != null)
            {
                Predict(animal_list);
            }
        }


        private static List<SP_Predict_AnimalList_Result> GetAnimalIdList()
        {
            List<SP_Predict_AnimalList_Result> result = new List<SP_Predict_AnimalList_Result>();
            //1. Define predict date
            DateTime predict_date = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Eastern Standard Time").Date;
            // get animal_id list from database
            try
            {
                using (DB_A4A060_csEntities context = new DB_A4A060_csEntities())
                {
                    result = context.SP_Predict_AnimalList(predict_date, Properties.Settings.Default.PredictInterval).ToList();
                    if (result.Count == 0)
                    {
                        result = null;
                    }
                }
            }
            catch (Exception)
            {

                result = null;
            }
            return result;
        }

        static void Predict(List<SP_Predict_AnimalList_Result> animal_list)
        {
            //1. Define predict date
            DateTime currentUTC = DateTime.UtcNow.AddHours(-Properties.Settings.Default.TimeIntervalPredict);
            DateTime predict_date = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(currentUTC, "Eastern Standard Time");
//test---------------------------------------------------------------------------------------------------------------------
  //          predict_date = DateTime.Parse("2020/11/29 13:00:00"); //2020 - 11 - 29 13:02:00.0000000
//test---------------------------------------------------------------------------------------------------------------------
            //3. run prediction routines
            PredictManager pm = new PredictManager();

            //3.1 Extract the temperature data from the database for the specified animal_id 
            //and for datetime from 0:00 on dat-2 to 18:00 on dat.
            double result = 0;
            double missing_gaps = 0;
            string email_massage = string.Empty;

            foreach (var item in animal_list)
            {
                result = pm.GetMeasDataByAnimalId(item.bolus_id, predict_date);
                missing_gaps = pm.MissingGapsProc(item.bolus_id, predict_date);

                //email preparation
                if (result >= 0.5 && (1 - missing_gaps) <= 0.5)
                {
                    email_massage += EmailMessageBuilder(item, predict_date, result, missing_gaps);
                } 
                // Save data in database
                pm.SaveResult(predict_date, item, result, missing_gaps);
            }
            //Send result----------------------------------------------------------------
            if (!string.IsNullOrEmpty(email_massage))
            {
                string addr = Properties.Settings.Default.emails;
                EmailBox em = new EmailBox();
                em.SendEmail(Properties.Settings.Default.email_sender, addr, email_massage, Properties.Settings.Default.email_subject);
            }
        }

        private static string EmailMessageBuilder(SP_Predict_AnimalList_Result item, DateTime predict_date, double result, double missing_gaps)
        {
            string msg = string.Empty;
            double lastprob = GetLastProb(item.bolus_id);

            if (lastprob > result || lastprob < 0)
            {
                //<Farm>. <Data /time>
                //Calving prediction for the next 24 hours for cow<animal_ID>.
                //Probability < Prob in %>.Missing data < missing_data in %>.

                msg = GetFarmNameByBolusID(item.bolus_id) + ". " + predict_date + "\r\n" +
                    "Calving prediction for the next 24 hours for cow #" + item.animal_id.Value + "\r\n" +
                    "Probability " + string.Format("{0:0.##}%", result) +
                    ".Missing data " + string.Format("{0:0.##}%", (1 - missing_gaps) * 100) + "\r\n";
            }
            else
            {
                //< Farm >. < Data / time >
                //INCREASED calving prediction for the next 24 hours for cow<animal_ID>.
                //Probability from < Prob in % last > to < Porb in % new>.
                //Missing data < missing_data in %>.
                msg = GetFarmNameByBolusID(item.bolus_id) + ". " + predict_date + "\r\n" +
                    "INCREASED calving prediction for the next 24 hours for cow #" + item.animal_id.Value + "\r\n" +
                    "Probability from " + string.Format("{0:0.##}%", lastprob) +" to "+ string.Format("{0:0.##}%", result)+
                    ".Missing data " + string.Format("{0:0.##}%", (1 - missing_gaps) * 100) + "\r\n";
            }
            return msg;
        }

        private static double GetLastProb(int bolus_id)
        {
            double result = 0;
            try
            {
                using (DB_A4A060_csEntities context = new DB_A4A060_csEntities())
                {
                    result = context.Predictions.OrderByDescending(t => t.id).Where(x => x.bolus_id == bolus_id).Take(1).SingleOrDefault().prob_calving;
                }
            }
            catch (Exception)
            {
                result = -1;
            }
            return result;
        }

        private static string GetFarmNameByBolusID(int bolus_id)
        {
            string result = string.Empty;
            try
            {
                using (DB_A4A060_csEntities context = new DB_A4A060_csEntities())
                {
                    result = context.SP_Farm_GetNameByBolus_ID(bolus_id).SingleOrDefault().Name;
                    ;
                }
            }
            catch (Exception)
            {
                result="N/A";
            }
            return result;
        }
    }
}
