//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CalvingPredict
{
    using System;
    using System.Collections.Generic;
    
    public partial class MeasData
    {
        public int id { get; set; }
        public string user_id { get; set; }
        public int bolus_id { get; set; }
        public string animal_id { get; set; }
        public double temperature { get; set; }
        public string bolus_timestamp { get; set; }
        public Nullable<System.DateTime> bolus_full_date { get; set; }
        public int base_station_id { get; set; }
        public bool is_average { get; set; }
        public string raw { get; set; }
    }
}