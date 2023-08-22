using BSA.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSA.Service.ServiceModel
{
   public class SeedModel
    {
        public int Id { get; set; }

        [DisplayName("Seed Category")]
        public Nullable<int> SeedCategoryId { get; set; }
        [DisplayName("Seed Type")]
        public Nullable<int> SeedTypeId { get; set; }

        [DisplayName("Seed Name")]
        public string ProductName { get; set; }
        [DisplayName("Seed Category")]
        public string SeedCategory { get; set; }
        [DisplayName("Seed Name Bangla")]
        public string ProductNameBN { get; set; }
        [DisplayName("Member Name")]
        public string MemberName { get; set; }

        public string MemberId { get; set; }
        public string Remarks { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }

        [DisplayName("Contact Person Photo")]
        public string ImageUrl { get; set; }

        [DisplayName("Seed Picture")]
        public string PictureUrl { get; set; }
        
        public List<SelectModel> SeedTypes { get; set; }
        public List<SelectModel> Members { get; set; }
        public List<SelectModel> Categories { get; set; }
        public List<SelectModel> productNames { get; set; }

    }
}
