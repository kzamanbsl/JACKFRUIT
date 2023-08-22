//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BSA.Data.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class BSAMember
    {
        public int Id { get; set; }
        public string MemberId { get; set; }
        public string MemberType { get; set; }
        public string MemberNameEn { get; set; }
        public string MemberNameBn { get; set; }
        public string BSARegistrationNo { get; set; }
        public Nullable<int> MemberOrder { get; set; }
        public string Delegate { get; set; }
        public string DelegateEmail { get; set; }
        public string LogoUrl { get; set; }
        public string AddressOne { get; set; }
        public string AddressTwo { get; set; }
        public string CorporateOffice { get; set; }
        public string TIN { get; set; }
        public string Website { get; set; }
        public string Telephone { get; set; }
        public string MobileNo { get; set; }
        public string Email { get; set; }
        public string OfficeEmail { get; set; }
        public bool Active { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string ModifedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
    }
}
