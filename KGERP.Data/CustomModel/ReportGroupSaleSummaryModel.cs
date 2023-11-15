using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace KGERP.Data.CustomModel
{
    public class ReportGroupSaleSummaryModel
    {
        public string ReportTitle { get; set; }

        [DisplayName("Company")]
        public int CompanyId { get; set; }

        
        [DisplayName("From Date")]
        public Nullable<System.DateTime> FromDate { get; set; }

        [DisplayName("To Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<System.DateTime> ToDate { get; set; }

        public string StrFromDate { get; set; }
        public string StrToDate { get; set; }

        public string ReportType { get; set; }
        public string ReportName { get; set; }

       
        public int? ProductCategoryId { get; set; }
        public int? ProductSubCategoryId { get; set; }
        public int? ProductId { get; set; }



        public int? ZoneFk { get; set; }
        public int? RegionFk { get; set; }
        public int? SubZoneFk { get; set; } //Territory
        public int? CustomerId { get; set; }

        public List<SelectModel> ProductCategoryList { get; set; } =new List<SelectModel>();
        public SelectList ProductSubCategoryList { get; set; } = new SelectList(new List<object>());
        public SelectList ProductList { get; set; } = new SelectList(new List<object>());


        public SelectList ZoneList { get; set; } = new SelectList(new List<object>());
        public SelectList ZoneDivisionList { get; set; } = new SelectList(new List<object>());
        public SelectList SubZoneList { get; set; } = new SelectList(new List<object>());
        public SelectList CustomerList { get; set; } = new SelectList(new List<object>());


    }
}
