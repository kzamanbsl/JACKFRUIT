using BSA.Service.ServiceModel;
using BSA.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BSA.ViewModel
{
    public class AdminSetUpViewModel
    {
        public AdminSetUpModel AdminSetUp { get; set; }
        public List<SelectModel> Employees { get; set; }
        public List<SelectModel> StatusSelectModels { get; set; }
    }
}