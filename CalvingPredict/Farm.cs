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
    
    public partial class Farm
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public System.Data.Entity.Spatial.DbGeography GeoPosition { get; set; }
        public string AspNetUser_Id { get; set; }
        public string Phones { get; set; }
        public string Emails { get; set; }
        public string PhoneStatus { get; set; }
        public string EmailStatus { get; set; }
        public Nullable<int> StationId { get; set; }
        public Nullable<int> vas_dairy_id { get; set; }
    }
}
