using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BSA.Controllers
{
    public class BaseController : Controller
    {
        public int GetCompanyId()
        {
            int companyId = Convert.ToInt32(HttpContext.Request.QueryString["companyId"]);
            return companyId;
        }
    }
}