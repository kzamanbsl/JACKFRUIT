using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using KGERP.ViewModel;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace KGERP.Controllers
{
    [SessionExpire]
    public class EducationController : Controller
    {
        private readonly IEducationService educationService;
        private readonly IDropDownItemService dropDownItemService;
        public EducationController(IEducationService educationService, IDropDownItemService dropDownItemService)
        {
            this.educationService = educationService;
            this.dropDownItemService = dropDownItemService;
        }
        public ActionResult Index(long id)
        {
            var educations = educationService.GetEducations(id);
            return View(educations.ToList());
        }



        public ActionResult CreateOrEdit(long id, int educationId)
        {
            EducationViewModel vm = new EducationViewModel();
            vm.Education = educationService.GetEducation(id, educationId);
            vm.Examinations = dropDownItemService.GetDropDownItemSelectModels(7);
            vm.Subjects = dropDownItemService.GetDropDownItemSelectModels(42);
            vm.Institutions = dropDownItemService.GetDropDownItemSelectModels(40);
            vm.Years = Helper.GetYears();
            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOrEdit(EducationViewModel vm)
        {
            if (vm.Education.CertificateUpload != null)
            {
                string fileName = Path.GetFileNameWithoutExtension(vm.Education.CertificateUpload.FileName);
                string extension = Path.GetExtension(vm.Education.CertificateUpload.FileName);
                fileName = fileName + "_" + DateTime.Now.ToString("yymmssfff") + extension;
                vm.Education.CertificateName = fileName;
                vm.Education.CertificateUpload.SaveAs(Path.Combine(Server.MapPath("~/FileUpload/EducationCertificate/"), fileName));
            }
            if (vm.Education.EducationId <= 0)
            {
                educationService.SaveEducation(0, vm.Education, out string message);
            }
            else
            {
                educationService.SaveEducation(vm.Education.EducationId, vm.Education, out string message);
            }

            return RedirectToAction("Index", new { id = vm.Education.Id });
        }


        public ActionResult Delete(long id, int educationId)
        {
            if (id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EducationModel model = educationService.GetEducation(id, educationId);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id, int educationId)
        {
            bool result = educationService.DeleteEducation(id, educationId);
            return RedirectToAction("Index", new { id });
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult Download(long id, int educationId)
        {
            string fileName = educationService.GetCertificateName(educationId);
            if (string.IsNullOrEmpty(fileName))
            {
                TempData["message"] = "No File is uploaded";
                return Redirect("/Education/Index?id=" + id.ToString());
            }
            string path = Server.MapPath("~/FileUpload/EducationCertificate/" + fileName);
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

    }
}
