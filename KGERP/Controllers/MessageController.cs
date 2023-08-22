using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System.Web.Mvc;

namespace KGERP.Controllers
{
    [SessionExpire]
    public class MessageController : BaseController
    {
        private readonly IMessageService messageService;
        public MessageController(IMessageService messageService)
        {
            this.messageService = messageService;
        }

        [SessionExpire]
        public ActionResult Message(int companyId)
        {
            MessageModel model = new MessageModel { CompanyId = companyId };
            return View(model);
        }

        [SessionExpire]
        public ActionResult SendMessage(MessageModel model)
        {
            int noOfMessageSent = messageService.SendMessage(model);
            return RedirectToAction("Message", new { model.CompanyId });
        }


    }
}