using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BSA.Service.ServiceModel
{
    public class MemberModel 
    {
        public string ButtonName
        {
            get
            {
                return Id > 0 ? "Update" : "Save";
            }

        }
        public long Id { get; set; }

        [DisplayName("Member No")]
        public string MemberId { get; set; }
        [DisplayName("Member Category")]
        public string MemberType { get; set; }
        [DisplayName("Member Name")]
        public string MemberName { get; set; }

        public int MemberOrder { get; set; }
        [DisplayName("Representative")]
        public string Delegate { get; set; }

        [DisplayName("Representative Email")]
        public string DelegateEmail { get; set; } 

        [DisplayName("Company Name")]
        public string CompanyName { get; set; }
        public string Nationality { get; set; }

        [DisplayName("Company Logo")]
        public string LogoUrl { get; set; }

        [DisplayName("Company Address")]
        public string AddressOne { get; set; }

        [DisplayName("Address")]
        public string AddressTwo { get; set; }  

        [DisplayName("Website URL")]
        public string Website { get; set; }
         

        [DisplayName("Telephone")]
        public string Telephone { get; set; } 
        [DisplayName("Mobile No")]
        public string MobileNo { get; set; } 
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [DisplayName("Personal Email")]
        public string Email { get; set; }
        [DisplayName("Company Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string OfficeEmail { get; set; }
        [DisplayName("Mailing Address")]
         
        public bool Active { get; set; } 
         
        [DisplayName("TIN No")]
        public string TIN { get; set; }
         
        [DisplayName("Created Date")]
        public Nullable<System.DateTime> CreatedDate { get; set; }
        [DisplayName("Modified By")]
        public string ModifedBy { get; set; }
        [DisplayName("Modified Date")]
        public Nullable<System.DateTime> ModifiedDate { get; set; }

        public string ImageFileName { get; set; }
        public string ImagePath { get; set; }
        //====================================================== 



    }
}
