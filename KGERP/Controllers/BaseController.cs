using System;
using System.Web.Mvc;

namespace KGERP.Controllers
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