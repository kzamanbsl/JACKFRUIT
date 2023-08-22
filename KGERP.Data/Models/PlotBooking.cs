//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace KGERP.Data.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class PlotBooking
    {
        public long booking_id { get; set; }
        public string ClientAutoId { get; set; }
        public Nullable<double> LandPricePerKatha { get; set; }
        public Nullable<double> PlotSize { get; set; }
        public Nullable<double> LandValue { get; set; }
        public Nullable<double> Discount { get; set; }
        public Nullable<double> LandValueAfterDiscount { get; set; }
        public Nullable<double> AdditionalCost { get; set; }
        public string OtharCostName { get; set; }
        public Nullable<double> UtilityCost { get; set; }
        public Nullable<double> GrandTotal { get; set; }
        public Nullable<double> BokkingMoney { get; set; }
        public Nullable<double> RestOfAmount { get; set; }
        public Nullable<double> InstallMentAmount { get; set; }
        public string BlockNo { get; set; }
        public string PloatNo { get; set; }
        public string PloatSize { get; set; }
        public string Facing { get; set; }
        public Nullable<int> OneTime { get; set; }
        public Nullable<int> InstallMent { get; set; }
        public Nullable<int> NoOfInstallment { get; set; }
        public string Remarks { get; set; }
        public Nullable<System.DateTime> Booking_Date { get; set; }
        public string PayType { get; set; }
        public string BankName { get; set; }
        public string ChaqueNo { get; set; }
    }
}
