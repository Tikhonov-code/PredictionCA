using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalvingPredict
{

    class PredictManager
    {
        // Fields, properties, methods and events go here...
        private double[] Coeff = new double[] { 0.32058451, 0.7196646, 0.94382078, 1.16500222, 1.10568591, 0.89498176, 0.07614098, -0.89044069, -1.32887114, -0.6428216, -2.38349343 };
        private double[] TempVal = new double[] { 39.83241071, 39.71359649, 39.81732759, 39.92336283, 39.80069565, 39.66582609, 39.76964602, 39.8513913, 39.74206897, 39.59367521, 39.64912281 };
        private const double Intercept = -0.00183839;
        public PredictManager()
        {
        }
        //1. Extract the temperature data from the database for the specified animal_id 
        //   and for datetime from 0:00 on dat-2 to 18:00 on dat.

        internal double GetMeasDataByAnimalId(int bolus_id, DateTime predict_date)
        {
            List<SP_Predict_GetTempByAnimalID_Result> result = new List<SP_Predict_GetTempByAnimalID_Result>();
            List<SP_Predict_GetGapsInInterval_Result> factors = new List<SP_Predict_GetGapsInInterval_Result>();
            try
            {
                using (DB_A4A060_csEntities context = new DB_A4A060_csEntities())
                {
                    result = context.SP_Predict_GetTempByAnimalID(bolus_id, predict_date).ToList();
                    factors = context.SP_Predict_GetGapsInInterval(bolus_id, predict_date).ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetMeasDataByAnimalId : " + ex.Message);
            }
            //1.1 check if last 3 factors = 0
            if (Check3Lastfactors(result))
            {
                return -103;
            }
            //2. calculate probability
            double prob = Intercept;
            int i = 0;
            double TempInterval = 0;
            foreach (var item in result)
            {
                //System.Diagnostics.Debug.WriteLine("result="+ item.MaxValue.Value);
                TempInterval = item.MaxValue;
                if (factors[i].factor == 0 || factors[i].factor > Properties.Settings.Default.FactorMax)
                {
                    TempInterval = TempVal[i];
                }
                prob += Coeff[i] * TempInterval;
                i++;
            }
            //3. finish
            prob = 1.0 / (1 + Math.Exp(-prob));
            return prob;
        }

        private bool Check3Lastfactors(List<SP_Predict_GetTempByAnimalID_Result> result)
        {
            double sum = 0;
            for (int i = 8; i < 11; i++)
            {
                sum += result[i].MaxValue;
            }
            if (sum == 0)
            {
                return true;
            }
            return false;
        }

        internal List<Prediction> ReadData()
        {
            List<Prediction> result = new List<Prediction>();
            try
            {
                using (DB_A4A060_csEntities context = new DB_A4A060_csEntities())
                {
                    result = context.Predictions.OrderByDescending(x => x.date_stamp).ToList();
                }
            }
            catch (Exception ex)
            {
                result = null;
            }
            return result;
        }

        internal void SaveResult(DateTime predict_date, SP_Predict_AnimalList_Result item, double prob_calving, double missing_gaps)
        {
            int bolus_id = item.bolus_id;

            Prediction pr = new Prediction();
            pr.bolus_id = bolus_id;
            pr.analysis_date = predict_date;
            pr.prob_calving = prob_calving;
            pr.prop_missing = 1 - missing_gaps;
            pr.confidence = missing_gaps;
            pr.date_stamp = DateTime.UtcNow;
            try
            {
                using (DB_A4A060_csEntities context = new DB_A4A060_csEntities())
                {
                    context.Predictions.Add(pr);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        internal double MissingGapsProc(int bolus_id, DateTime predict_date)
        {
            double result = 0;
            try
            {
                using (DB_A4A060_csEntities context = new DB_A4A060_csEntities())
                {
                    result = context.SP_Predict_GetGapsStatByAnimalID(bolus_id, predict_date).SingleOrDefault().gaps.Value;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetMeasDataByAnimalId : " + ex.Message);
                result = -1;
            }

            return result;
        }
    }
}
