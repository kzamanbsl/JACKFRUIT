using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bsa.lib.CustomModel
{
   public class BsaAGMModel
    {
        public int Id { get; set; }
        [Display(Name = "AGM Year")]
        public Nullable<int> AGMYear { get; set; }
        [Display(Name = "Member/ Dealer Id")]
        [Required]
        public string MemberOrDealerId { get; set; }

        [Display(Name = "Name of Dealer")]
        [Required]
        public string NameOfDealer { get; set; }
        [Required]
        public string Disrtict { get; set; }
        [Required]
        [Display(Name = "Mobile No")]
        public string PhoneNo { get; set; }
        [Required]
        [Display(Name = "Name of Representative")]
        public string NameOfRepresentative { get; set; }
        [Required]

        [Display(Name = "No of Person")]
        public Nullable<int> NoofPerson { get; set; }
        public string Remarks { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
    }
}
