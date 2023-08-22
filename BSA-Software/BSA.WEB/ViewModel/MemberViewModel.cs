using BSA.Service.ServiceModel;
using BSA.Data.Models;
using BSA.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BSA.ViewModel
{
    public class MemberViewModel
    {
        public MemberModel MemberModel { get; set; }
        public MemberViewModel()
        {
            MemberModel memberModel = new MemberModel();
        }
        public List<SelectModel> Managers { get; set; }
 
        public List<SelectModel> Companies { get; set; }
        public List<SelectModel> Religions { get; set; }
        public List<SelectModel> BloodGroups { get; set; }
        public List<SelectModel> Countries { get; set; }
        public List<SelectModel> Districts { get; set; }
        public List<SelectModel> MaritalTypes { get; set; }
        public List<SelectModel> Genders { get; set; }
        
        public List<SelectModel> EmployeeCategories { get; set; }
        public List<SelectModel> Departments { get; set; }
        public List<SelectModel> Designations { get; set; }
        public List<SelectModel> OfficeTypes { get; set; }
        public List<SelectModel> DisverseMethods { get; set; }
        public List<SelectModel> JobCategories { get; set; }
        public List<SelectModel> JobTypes { get; set; }
        public List<SelectModel> MemberTypes { get; set; }
        public List<SelectModel> Banks { get; set; }
        public List<SelectModel> Shifts { get; set; }
        public List<SelectModel> SalaryGrades { get; set; }
        public List<SelectModel> BankBranches { get; set; }
    }
}