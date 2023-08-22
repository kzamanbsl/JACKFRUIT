using ExcelDataReader;
using KGERP.Data.CustomModel;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using KGERP.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace KGERP.Controllers
{
    [SessionExpire]
    public class VendorController : BaseController
    {
        private readonly IDistrictService _districtService;
        private readonly IUpazilaService _upazilaService;
        private readonly IVendorService _vendorService;
        private readonly IPaymentService _paymentService;
        private readonly IZoneService _zoneService;
        private readonly ISubZoneService _subZoneService;
        private readonly IHeadGLService _headGlService;
        public VendorController(IHeadGLService headGLService, IDistrictService districtService,
            IUpazilaService upazilaService,
            ISubZoneService subZoneService, IVendorService vendorService,
            IPaymentService paymentService, IZoneService zoneService)
        {
            this._vendorService = vendorService;
            this._paymentService = paymentService;
            this._zoneService = zoneService;
            this._subZoneService = subZoneService;
            this._districtService = districtService;
            this._upazilaService = upazilaService;
            this._headGlService = headGLService;
        }

        [SessionExpire]
        [HttpGet]
        public ActionResult AllCustomerIndex(int companyId)
        {
            if (companyId > 0)
            {
                Session["CompanyId"] = companyId;
            }
            return View();
        }


        [SessionExpire]
        [HttpPost]
        public ActionResult AllCustomer(int customerType)
        {
            //Server Side Parameter
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string searchValue = Request["search[value]"];
            string sortColumnName = Request["columns[" + Request["order[0][column]"] + "][name]"];
            string sortDirection = Request["order[0][dir]"];



            List<VendorModel> customerList = _vendorService.GetAllCustomers(customerType);
            int totalRows = customerList.Count;
            if (!string.IsNullOrEmpty(searchValue))//filter
            {
                customerList = customerList.Where(x => x.CompanyName.ToLower().Contains(searchValue.ToLower())
                                          || x.CustomerType.ToLower().Contains(searchValue.ToLower())
                                          || x.Name.ToLower().Contains(searchValue.ToLower())
                                          || x.Phone.ToLower().Contains(searchValue.ToLower())
                                          || x.Email.ToLower().Contains(searchValue.ToLower())
                                          || x.Email.ToLower().Contains(searchValue.ToLower())
                                          || x.Address.ToLower().Contains(searchValue.ToLower())
                                          ).ToList<VendorModel>();
            }
            int totalRowsAfterFiltering = customerList.Count;


            //sorting
            customerList = customerList.OrderBy(sortColumnName + " " + sortDirection).ToList<VendorModel>();

            //paging
            customerList = customerList.Skip(start).Take(length).ToList<VendorModel>();


            return Json(new { data = customerList, draw = Request["draw"], recordsTotal = totalRows, recordsFiltered = totalRowsAfterFiltering }, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        [HttpGet]
        public async Task< ActionResult> Index(int companyId,int  vendorTypeId)
        {
            if (GetCompanyId() > 0)
            {
                Session["CompanyId"] = GetCompanyId();
            }
           
            var vendors = await _vendorService.GetVendors(companyId, vendorTypeId);
            //if (vendorTypeId == 1)
            //{
            //    return View("SupplierIndex", vendors.ToPagedList(No_Of_Page, Size_Of_Page));
            //}
            return View(vendors);
        }


        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> CreateOrEdit(int id, int vendorTypeId)
        {
            ModelState.Clear();
            int companyId = Convert.ToInt32(Session["CompanyId"]);
            VendorViewModel vm = new VendorViewModel();
            vm.Vendor = _vendorService.GetVendorByType(id, vendorTypeId);
            vm.Months = _vendorService.GetMonthSelectModes();
            vm.Zones = _zoneService.GetZoneSelectModels(companyId);
            vm.Vendor.CompanyId = companyId;
            if (id > 0)
            {
                vm.CustomerCodes = _headGlService.GetUpdateCustomerCode(vendorTypeId, companyId);
            }
            else
            {
                vm.CustomerCodes = _headGlService.GetInsertCustomerCode(vendorTypeId, companyId);
            }
            vm.Countries = _districtService.GetCountriesSelectModels();
            vm.Districts = _districtService.GetDistrictSelectModels();

            if (vm.Vendor.VendorId > 0)
            {
                vm.Upazilas = _upazilaService.GetUpazilaSelectModelsByDistrict(vm.Vendor.DistrictId ?? 0);
                vm.SubZones = _zoneService.GetSubZoneSelectModelsByZone(vm.Vendor.ZoneId ?? 0);
            }
            else
            {
                vm.Upazilas = new List<SelectModel>();
                vm.SubZones = new List<SelectModel>();
            }
            if (companyId == (int)CompanyName.KrishibidFarmMachineryAndAutomobilesLimited)
            {
                return View("KFMALCreateOrEdit", vm);
            }

            return View(vm);

        }


        [SessionExpire]
        [HttpPost]
        public async Task<ActionResult> CreateOrEdit(VendorViewModel vm)
        {
            ModelState.Clear();
            string message = string.Empty;
            bool result = false;

            if (vm.VendorImageUpload != null)
            {
                string fileName = Path.GetFileNameWithoutExtension(vm.VendorImageUpload.FileName);
                string extension = Path.GetExtension(vm.VendorImageUpload.FileName);
                fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                vm.Vendor.ImageUrl = "~/Images/VendorImage/" + fileName;
                vm.VendorImageUpload.SaveAs(Path.Combine(Server.MapPath("~/Images/VendorImage/"), fileName));
            }
            if (vm.NomineeImageUpload != null)
            {
                string fileName = Path.GetFileNameWithoutExtension(vm.NomineeImageUpload.FileName);
                string extension = Path.GetExtension(vm.VendorImageUpload.FileName);
                fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                vm.Vendor.NomineeImageUrl = "~/Images/VendorImage/" + fileName;
                vm.NomineeImageUpload.SaveAs(Path.Combine(Server.MapPath("~/Images/VendorImage/"), fileName));
            }

            if (vm.Vendor.VendorId <= 0)
            {
                result =await _vendorService.SaveVendor(0, vm.Vendor, message);
                if (result)
                {
                    message = "Data saved successfully";
                }
            }

            else
            {
                result =await _vendorService.SaveVendor(vm.Vendor.VendorId, vm.Vendor, message);
                if (result)
                {
                    message = "Data updated successfully";
                }
            }
            TempData["message"] = message;
            return RedirectToAction("Index", new { companyId = vm.Vendor.CompanyId, vendorTypeId = vm.Vendor.VendorTypeId, isActive = vm.Vendor.IsActive });
        }

        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> PaymentIndex(int companyId)
        {
            if (GetCompanyId() > 0)
            {
                Session["CompanyId"] = GetCompanyId();
            }

            VendorModel vendorModel = new VendorModel();
            vendorModel = await _vendorService.GetCustomerPayments(companyId);
            return View(vendorModel);
        }

        //[SessionExpire]
        //[HttpGet]
        //public ActionResult PaymentIndex(int? Page_No, string searchText)
        //{
        //    if (GetCompanyId() > 0)
        //    {
        //        Session["CompanyId"] = GetCompanyId();
        //    }
        //    searchText = searchText ?? "";
        //    List<VendorModel> vendors = vendorService.GetCustomerPayments(searchText, Convert.ToInt32(Session["CompanyId"]), (int)Provider.Customer);
        //    int Size_Of_Page = 10;
        //    int No_Of_Page = Page_No ?? 1;
        //    return View(vendors.ToPagedList(No_Of_Page, Size_Of_Page));
        //}

        [SessionExpire]
        [HttpGet]
        public JsonResult GetCustomerPaymentInformation(int customerId)
        {
            VendorModel vendor = _vendorService.GetVendorPaymentStatus(customerId);

            var result = JsonConvert.SerializeObject(vendor, Formatting.None, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        [HttpGet]
        public JsonResult GetCustomerInformation(int customerId)
        {
            VendorModel vendor = _vendorService.GetVendor(customerId);

            var result = JsonConvert.SerializeObject(vendor, Formatting.None, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        [HttpGet]
        public JsonResult GetSupplierInformation(int supplierId)
        {
            VendorModel vendor = _vendorService.GetSupplier(supplierId);

            var result = JsonConvert.SerializeObject(vendor, Formatting.None, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        [HttpGet]
        public ActionResult CustomerReceivable(int id)
        {
            VendorViewModel vm = new VendorViewModel();
            vm.Vendor = _vendorService.GetVendor(id);
            vm.CustomerReceivables = _vendorService.GetCustomerReceivables(id);
            return View(vm);
        }

        [SessionExpire]
        [HttpGet]
        public ActionResult CustomerPayment(int id)
        {
            VendorViewModel vm = new VendorViewModel();
            vm.Vendor = _vendorService.GetVendor(id);
            vm.Payments = _paymentService.GetPaymentsByVendor(id);
            return View(vm);
        }


        [SessionExpire]
        [HttpGet]
        public ActionResult CustomerLedger(int id)
        {
            List<CustomerLedgerCustomModel> customerLedgers = _vendorService.GetCustomerLedger(id);
            return View(customerLedgers);
        }

        [HttpPost]
        public JsonResult AutoComplete(string prefix)
        {
            int companyId = Convert.ToInt32(Session["CompanyId"]);
            var customers = _vendorService.GetCustomerAutoComplete(prefix, companyId);

            return Json(customers);
        }

        [HttpGet]
        public JsonResult AutoCompleteCustomer(string prefix, int companyId)
        {
            var customers = _vendorService.GetCustomerAutoComplete(prefix, companyId);
            return Json(customers,  JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AutoCompleteByCompanyId(int companyId, string prefix)
        {
            var customers = _vendorService.GetCustomerAutoComplete(prefix, companyId);
            return Json(customers);
        }

        [HttpPost]
        public JsonResult CustomerAssociatesCustomerId(int customerId)
        {
            var customers = _vendorService.CustomerAssociatesCustomerId(customerId);
            return Json(customers);
        }

        [HttpPost]
        public JsonResult ClientAutoComplete(int companyId, string prefix)
        {
            var customers = _vendorService.GetClientAutoComplete(prefix, companyId);
            return Json(customers);
        }

        [HttpPost]
        public JsonResult SupplierAutoComplete(int companyId, string prefix)
        {
            //int companyId = Convert.ToInt32(Session["CompanyId"]);
            var customers = _vendorService.GetSupplierAutoComplete(prefix, companyId);
            return Json(customers);
        }

        [HttpGet]
        public JsonResult GetSupplierAutoComplete(int companyId, string prefix)
        {
            //int companyId = Convert.ToInt32(Session["CompanyId"]);
            var customers = _vendorService.GetSupplierAutoComplete(prefix, companyId);
            return Json(customers, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult RentCompanyAutoComplete(string prefix)
        {
            int companyId = Convert.ToInt32(Session["CompanyId"]);
            var customers = _vendorService.GetRentCompanyAutoComplete(prefix, companyId);
            return Json(customers);
        }

        [HttpPost]
        public JsonResult GetAutoGeneratedVendorCode(int upazilaId, int vendorTypeId)
        {
            int companyId = Convert.ToInt32(Session["CompanyId"]);
            string customerCode = _vendorService.GetAutoGeneratedVendorCode(companyId, upazilaId, vendorTypeId);
            return Json(customerCode);
        }


        [SessionExpire]
        [HttpGet]
        public ActionResult UploadVendor(int companyId, int vendorTypeId)
        {
            VendorModel model = new VendorModel
            {
                CompanyId = companyId,
                VendorTypeId = vendorTypeId
            };
            return View(model);
        }

        [SessionExpire]
        [HttpPost]
        public ActionResult UploadVendor(VendorModel model)
        {
            bool result = false;
            List<VendorModel> vendors = new List<VendorModel>();

            HttpPostedFileBase file = model.UploadedFile;
            if (file.ContentLength > 0)
            {
                using (var stream = file.InputStream)
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration
                            {
                                UseHeaderRow = true
                            }
                        });

                        var dataTable = dataSet.Tables[0];

                        for (int rowIterator = 1; rowIterator < dataTable.Rows.Count; rowIterator++)
                        {
                            VendorModel vendor = new VendorModel();

                            vendor.CompanyId = (int)Session["CompanyId"];
                            vendor.VendorTypeId = (int)Provider.Customer;
                            vendor.CustomerType = VendorsPaymentMethodEnum.Cash.ToString();
                            vendor.CountryId = 19; //Bangladesh

                            //From File
                            vendor.Name = dataTable.Rows[rowIterator][0]?.ToString();
                            vendor.Propietor = dataTable.Rows[rowIterator][1]?.ToString();
                            vendor.Phone = dataTable.Rows[rowIterator][2]?.ToString();
                            vendor.DistrictName = dataTable.Rows[rowIterator][3]?.ToString();
                            vendor.UpazilaName = dataTable.Rows[rowIterator][4]?.ToString();
                            vendor.ThanaName = dataTable.Rows[rowIterator][5]?.ToString();
                            vendor.Address = dataTable.Rows[rowIterator][6]?.ToString();
                            vendor.ZoneName = dataTable.Rows[rowIterator][7]?.ToString();
                            vendor.RegionName = dataTable.Rows[rowIterator][8]?.ToString();
                            vendor.SubZoneName = dataTable.Rows[rowIterator][9]?.ToString();
                            vendor.PaymentDue = dataTable.Rows[rowIterator][10] != null ? Convert.ToDecimal(dataTable.Rows[rowIterator][10]) : (decimal?)null;
                            vendor.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
                            vendor.CreatedDate = DateTime.Today;
                            vendor.IsActive = true;
                            vendors.Add(vendor);
                        }
                    }
                }
            }

            result = _vendorService.BulkCustomerSave(vendors);

            if (result)
            {
                return RedirectToAction("CommonCustomer","Configuration", new { companyId = model.CompanyId });
            }

            return View();
        }

        public ActionResult DownloadDemoFile()
        {
            string filePath = Server.MapPath("~/Content/Excel/CustomerUploadFileFormat.xls"); // Path to the demo XLS file
            string contentType = "application/vnd.ms-excel"; // Content type of the file
            return File(filePath, contentType, "CustomerUploadFileFormat.xls");
        }

        [HttpPost]
        public JsonResult GetCustomerSelectModelsByCompany(int companyId)
        {
            const int customer = 2;
            List<SelectModel> customers = _vendorService.GetCustomerSelectModelsByCompany(companyId, customer);
            return Json(customers, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        [HttpGet]
        public ActionResult Delete(int id, int vendorTypeId, bool isActive)
        {

            bool result = _vendorService.DeleteVendor(id);
            if (result)
            {
                TempData["message"] = "Data deleted successfully";
            }
            else
            {
                TempData["message"] = "Data can not be deleted";
            }

            return RedirectToAction("Index", new { companyId = Session["CompanyId"], vendorTypeId, isActive });
        }

        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> VendorDeedIndex(int companyId)
        {
            var model = await _vendorService.GetAllVendorDeed(companyId);

            return View(model);
        }

        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> CreateVendorDeed(int companyId, int vendorDeedId)
        {
            var model = new VendorDeedVm();

            model.CompanyId = companyId;
            if (vendorDeedId > 0)
            {
                model = await _vendorService.GetVendorDeedById(vendorDeedId);
            }

            return View(model);
        }

        [SessionExpire]
        [HttpPost]
        public async Task<ActionResult> CreateVendorDeed(VendorDeedVm model)
        {
            var vendorDeedId = await _vendorService.SaveVendorDeed(model);
            return RedirectToAction("CreateVendorDeed", "Vendor", new
            {
                companyId = model.CompanyId,
                vendorDeedId = vendorDeedId
            });
        }

        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> RemoveVendorDeed(int companyId,int vendorDeedId)
        {
            if (vendorDeedId > 0)
            {
                await _vendorService.RemoveVendorDeed(vendorDeedId);
            }

            return RedirectToAction("VendorDeedIndex", "Vendor", new
            {
                companyId = companyId
            });
        }

    }
}