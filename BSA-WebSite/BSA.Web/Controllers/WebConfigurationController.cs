using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Bsa.lib.Implementation;
using Bsa.lib.CustomModel;
using System.Threading.Tasks;
using Bsa.lib.Utility;

namespace Bsa.Web.Controllers
{
    public class WebConfigurationController : Controller
    {
        private HttpContext httpContext;
        private readonly WebConfigurationService _service;
        public WebConfigurationController()
        {
        }
        public WebConfigurationController(WebConfigurationService configurationService)
        {
            _service = configurationService;
        }

        #region MD/Secretary Message        
        public async Task<ActionResult> MDSecretaryMessage()
        {
            //MdSecretaryMessageModel messageModel = new MdSecretaryMessageModel();
            MdSecretaryMessageModel messageModel;
            messageModel = await Task.Run(() => _service.GetMessage());
            //messageModel.CompanyList = new SelectList(_service.CompaniesDropDownList(), "Value", "Text");
            return View(messageModel);
        }
        [HttpPost]
        public async Task<ActionResult> MDSecretaryMessage(MdSecretaryMessageModel model)
        {
            if (model.ActionEum == ActionEnum.Add)
            {
                //Add 
                await _service.MessageAdd(model);
            }
            else if (model.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.MessageEdit(model);
            }
            else if (model.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.MessageDelete(model.ID);
            }
            else
            {
                return RedirectToAction("Error");
            }
            return RedirectToAction(nameof(MDSecretaryMessage));
        }
        #endregion
 
    }
}