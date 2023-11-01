using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using KGERP.Data.Models;
using KGERP.Service.Implementation;
using KGERP.Service.Implementation.Configuration;
using KGERP.Service.Implementation.FTP;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel.FTP_Models;
using KGERP.Utility;

namespace KGERP.Controllers
{
    [SessionExpire]
    public class ConfigurationController : Controller
    {

        private HttpContext httpContext;
        private readonly ConfigurationService _service;
        private readonly ICompanyService _companyService;
        private readonly IFTPService _ftpService;
        private readonly IDepartmentService _departmentService = new DepartmentService();
        private readonly HrDesignationService _hrDesignationService = new HrDesignationService();
        private readonly ERPEntities _db;

        public ConfigurationController(IFTPService ftpService, ICompanyService companyService, ConfigurationService configurationService, ERPEntities db)
        {
            _service = configurationService;
            _companyService = companyService;
            _ftpService = ftpService;
            _db = db;
        }

        #region User Role Menuitem

        // Client user menu assign
        public ActionResult ClientUserMenuAssignment()
        {
            var viewData = new ClientMenu()
            {
                Companies = _db.Companies.Where(x => x.IsCompany == true && x.IsActive == true).ToList(),
            };

            viewData.CompanyList = new SelectList(viewData.Companies, "CompanyId", "Name");
            return View(viewData);
        }

        // Client menu assign post

        [HttpPost]
        public ActionResult ClientUserMenuAssignment(ClientMenu model)
        {
            if (model.CompanyId != null)
            {
                ClientMenu viewData = new ClientMenu()
                {
                    Companies = _db.Companies.Where(x => x.IsCompany == true && x.IsActive == true).ToList(),
                    CompanyMenus = _db.CompanyMenus.Where(x => x.CompanyId == model.CompanyId && x.IsActive == true).ToList(),
                    CompanySubMenus = _db.CompanySubMenus.Where(x => x.CompanyId == model.CompanyId && x.IsActive == true).ToList(),
                    CompanyUserMenus = _db.CompanyUserMenus.Where(x => x.UserId == model.UserId).ToList(),
                };

                var allowedMenus = _db.CompanyUserMenus.Where(x => x.UserId == "ISS0001" && x.IsActive == true).ToList();


                var filteredCompanyMenus = new List<CompanyMenu>();

                foreach (var checkedItem in allowedMenus)
                {
                    var matchingMenu = viewData.CompanyMenus.FirstOrDefault(menu => menu.CompanyMenuId == checkedItem.CompanyMenuId);

                    if (matchingMenu != null && !filteredCompanyMenus.Any(menu => menu.CompanyMenuId == matchingMenu.CompanyMenuId))
                    {
                        filteredCompanyMenus.Add(matchingMenu);
                    }
                }
                viewData.CompanyMenus = filteredCompanyMenus;

                foreach (var checkedItem in allowedMenus)
                {
                    foreach (var item in viewData.CompanyUserMenus)
                    {
                        if (checkedItem.CompanySubMenuId == item.CompanySubMenuId)
                        {
                            ClientUserMenu data = new ClientUserMenu()
                            {
                                UserMenuId = item.CompanyUserMenuId,
                                IsActive = item.IsActive,
                                UserId = item.UserId,
                                MenuId = item.CompanyMenuId,
                                MenuName = _db.CompanyMenus.Find(item.CompanyMenuId).Name,
                                SubMenuId = item.CompanySubMenuId,
                                SubMenuName = _db.CompanySubMenus.Find(item.CompanySubMenuId).Name,
                                SubMenuController = _db.CompanySubMenus.Find(item.CompanySubMenuId).Controller,
                                SubMenuAction = _db.CompanySubMenus.Find(item.CompanySubMenuId).Action
                            };

                            // Check if ClientUserMenus is null, if so, initialize it
                            if (viewData.ClientUserMenus == null)
                            {
                                viewData.ClientUserMenus = new List<ClientUserMenu>();
                            }

                            viewData.ClientUserMenus.Add(data);
                        }
                    }
                }   

                viewData.CompanyList = new SelectList(viewData.Companies, "CompanyId", "Name");
                return View(viewData);
            }

            return View(model);
        }

        public JsonResult ClientCompanyUserMenuEdit(int id, bool isActive)
        {

            var permission = _db.CompanyUserMenus.Find(id);
            permission.IsActive = isActive;
            _db.SaveChanges();

            ClientMenu model = new ClientMenu()
            {
                CompanySubMenuId = id,
                IsActive = isActive
            };

            return Json(new { menuid = model.CompanySubMenuId, updatedstatus =  model.IsActive});
        }

        // User menu assign
        public async Task<ActionResult> UserMenuAssignment(int companyId)
        {
            VMUserMenuAssignment vmUserMenuAssignment = new VMUserMenuAssignment();
            vmUserMenuAssignment.CompanyList = new SelectList(_service.CompaniesDropDownList(), "Value", "Text");

            return View(vmUserMenuAssignment);
        }

        [HttpPost]
        public async Task<ActionResult> UserMenuAssignment(VMUserMenuAssignment model)
        {
            VMUserMenuAssignment vmUserMenuAssignment = new VMUserMenuAssignment();
            vmUserMenuAssignment = await _service.UserMenuAssignmentGet(model);
            return View(vmUserMenuAssignment);
        }

        public JsonResult CompanyUserMenuEdit(int id, bool isActive)
        {
            VMUserMenuAssignment model = new VMUserMenuAssignment
            {
                IsActive = isActive,
                CompanyUserMenusId = id
            };
            CompanyUserMenu companyUserMenu = _service.CompanyUserMenuEdit(model);
            return Json(new { menuid = companyUserMenu.CompanyUserMenuId, updatedstatus = companyUserMenu.IsActive });
        }

        #endregion

        public async Task<ActionResult> AccountingCostCenter(int companyId)
        {
            VMUserMenu vmUserMenu = await Task.Run(() => _service.AccountingCostCenterGet(companyId));

            return View(vmUserMenu);
        }

        [HttpPost]
        public async Task<ActionResult> AccountingCostCenter(VMUserMenu model)
        {

            if (model.ActionEum == ActionEnum.Add)
            {
                //Add 
                await _service.AccountingCostCenterAdd(model);
            }
            else if (model.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.AccountingCostCenterEdit(model);
            }
            else if (model.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.AccountingCostCenterDelete(model.ID);
            }
            else
            {
                return View("Error");
            }
            return RedirectToAction(nameof(AccountingCostCenter), new { companyId = model.CompanyFK });
        }

        #region User Menu
        public async Task<ActionResult> UserMenu()
        {
            VMUserMenu vmUserMenu;
            vmUserMenu = await Task.Run(() => _service.UserMenuGet());
            vmUserMenu.CompanyList = new SelectList(_service.CompaniesDropDownList(), "Value", "Text");

            return View(vmUserMenu);
        }

        [HttpPost]
        public async Task<ActionResult> UserMenu(VMUserMenu model)
        {

            if (model.ActionEum == ActionEnum.Add)
            {
                //Add 
                await _service.UserMenuAdd(model);
            }
            else if (model.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.UserMenuEdit(model);
            }
            else if (model.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.UserMenuDelete(model.ID);
            }
            else
            {
                return RedirectToAction("Error");
            }
            return RedirectToAction(nameof(UserMenu), new { companyId = model.CompanyFK });
        }

        #endregion

        #region User Submenu
        public async Task<ActionResult> UserSubMenu()
        {
            VMUserSubMenu vmUserSubMenu;
            vmUserSubMenu = await Task.Run(() => _service.UserSubMenuGet());
            vmUserSubMenu.UserMenuList = new SelectList(_service.CompanyMenusDropDownList(), "Value", "Text");
            vmUserSubMenu.CompanyList = new SelectList(_service.CompaniesDropDownList(), "Value", "Text");

            return View(vmUserSubMenu);
        }

        [HttpPost]
        public async Task<ActionResult> UserSubMenu(VMUserSubMenu model)
        {
            if (model.ActionEum == ActionEnum.Add)
            {
                await _service.UserSubMenuAdd(model);
            }
            else if (model.ActionEum == ActionEnum.Edit)
            {
                await _service.UserSubMenuEdit(model);
            }
            else if (model.ActionEum == ActionEnum.Delete)
            {
                await _service.UserSubMenuDelete(model.ID);
            }
            else
            {
                return RedirectToAction("Error");
            }
            return RedirectToAction(nameof(UserSubMenu), new { companyId = model.CompanyFK });
        }
        #endregion

        public async Task<ActionResult> CommonUnitGet(int companyId)
        {
            var dataList = await Task.Run(() => _service.CompanyMenusGet(companyId));
            var list = dataList.Select(x => new { Value = x.ID, Text = x.Name }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        #region Unit
        public async Task<JsonResult> SingleCommonUnit(int id)
        {

            VMCommonUnit model = new VMCommonUnit();
            model = await _service.GetSingleCommonUnit(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> CommonUnit(int companyId)
        {

            VMCommonUnit vmCommonUnit = new VMCommonUnit();
            vmCommonUnit = await Task.Run(() => _service.GetUnit(companyId));
            return View(vmCommonUnit);
        }

        [HttpPost]
        public async Task<ActionResult> CommonUnit(VMCommonUnit vmCommonUnit)
        {

            if (vmCommonUnit.ActionEum == ActionEnum.Add)
            {
                //Add 
                await _service.UnitAdd(vmCommonUnit);
            }
            else if (vmCommonUnit.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.UnitEdit(vmCommonUnit);
            }
            else if (vmCommonUnit.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.UnitDelete(vmCommonUnit.ID);
            }
            else
            {
                return View("Error");
            }
            return RedirectToAction(nameof(CommonUnit), new { companyId = vmCommonUnit.CompanyFK });
        }

        [HttpPost]
        public async Task<ActionResult> CommonUnitDelete(VMCommonUnit vmCommonUnit)
        {

            if (vmCommonUnit.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.UnitDelete(vmCommonUnit.ID);
            }
            else
            {
                return View("Error");
            }
            return RedirectToAction(nameof(CommonUnit));
        }

        #endregion

        public async Task<JsonResult> SingleProductCategory(int id)
        {
            var model = await _service.GetSingleProductCategory(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> SingleCommonProductSubcategory(int id)
        {
            var model = await _service.GetSingleProductSubCategory(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> CommonProductSubCategoryGet(int companyId, int categoryId)
        {

            var vmCommonProductSubCategory = await Task.Run(() => _service.CommonProductSubCategoryGet(companyId, categoryId));
            var list = vmCommonProductSubCategory.Select(x => new { Value = x.ID, Text = x.Name }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> CommonProductGet(int companyId, int productSubCategoryId)
        {
            var vmCommonProductSubCategory = await Task.Run(() => _service.CommonProductGet(companyId, productSubCategoryId));
            var list = vmCommonProductSubCategory.Select(x => new { Value = x.ID, Text = x.Name }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        #region Common Zone

        [HttpGet]
        public async Task<ActionResult> CommonZone(int companyId)
        {
            VMCommonZone vmCommonZone = new VMCommonZone();
            vmCommonZone = await Task.Run(() => _service.GetZones(companyId));
            vmCommonZone.EmployeeList = _service.GetEmployeeSelectModels(companyId);
            return View(vmCommonZone);
        }

        [HttpGet]
        public async Task<ActionResult> CommonZonesList(int companyId)
        {
            var ZoneList = new SelectList(_service.CommonZonesDropDownList(companyId), "Value", "Text");
            return Json(ZoneList, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> CommonZone(VMCommonZone vmCommonZone)
        {

            if (vmCommonZone.ActionEum == ActionEnum.Add)
            {
                //Add 
                await _service.ZoneAdd(vmCommonZone);
            }
            else if (vmCommonZone.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.ZonesEdit(vmCommonZone);
            }
            else if (vmCommonZone.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.ZonesDelete(vmCommonZone.ID);
            }
            else
            {
                return View("Error");
            }
            return RedirectToAction(nameof(CommonZone), new { companyId = vmCommonZone.CompanyFK });
        }

        #endregion

        #region Common Region

        [HttpGet]
        public async Task<ActionResult> GetRegionList(int companyId, int zoneId = 0)
        {
            var model = await Task.Run(() => _service.GetRegionSelectList(companyId, zoneId));
            var list = model.Select(x => new { Value = x.Value, Text = x.Text }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> CommonRegion(int companyId, int zoneId = 0)
        {

            VMCommonRegion vmCommonRegion = new VMCommonRegion();

            vmCommonRegion = await Task.Run(() => _service.GetRegions(companyId, zoneId));
            vmCommonRegion.ZoneList = new SelectList(_service.CommonZonesDropDownList(companyId), "Value", "Text");
            vmCommonRegion.EmployeeList = _service.GetEmployeeSelectModels(companyId);

            return View(vmCommonRegion);
        }

        [HttpPost]
        public async Task<ActionResult> CommonRegion(VMCommonRegion vmCommonRegion)
        {

            if (vmCommonRegion.ActionEum == ActionEnum.Add)
            {
                //Add 

                await _service.RegionAdd(vmCommonRegion);
            }
            else if (vmCommonRegion.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.RegionEdit(vmCommonRegion);
            }
            else if (vmCommonRegion.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.RegionDelete(vmCommonRegion.ID);
            }
            else
            {
                return View("Error");
            }
            return RedirectToAction(nameof(CommonRegion), new { companyId = vmCommonRegion.CompanyFK, zoneId = vmCommonRegion.ZoneId });
        }


        #endregion

        #region Common Area

        [HttpGet]
        public async Task<ActionResult> GetAreaList(int companyId, int zoneId = 0, int regionId = 0)
        {
            var model = await Task.Run(() => _service.GetAreaSelectList(companyId, zoneId, regionId));
            var list = model.Select(x => new { Value = x.Value, Text = x.Text }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> CommonArea(int companyId, int zoneId = 0, int regionId = 0)
        {

            VMCommonArea vmCommonArea = new VMCommonArea();

            vmCommonArea = await Task.Run(() => _service.GetAreas(companyId, zoneId, regionId));
            vmCommonArea.ZoneList = new SelectList(_service.CommonZonesDropDownList(companyId), "Value", "Text");
            vmCommonArea.RegionList = _service.GetRegionSelectList(companyId, zoneId);
            vmCommonArea.EmployeeList = _service.GetEmployeeSelectModels(companyId);

            return View(vmCommonArea);
        }

        [HttpPost]
        public async Task<ActionResult> CommonArea(VMCommonArea vmCommonArea)
        {

            if (vmCommonArea.ActionEum == ActionEnum.Add)
            {
                //Add 

                await _service.AreaAdd(vmCommonArea);
            }
            else if (vmCommonArea.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.AreaEdit(vmCommonArea);
            }
            else if (vmCommonArea.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.AreaDelete(vmCommonArea.ID);
            }
            else
            {
                return View("Error");
            }
            return RedirectToAction(nameof(CommonArea), new { companyId = vmCommonArea.CompanyFK, zoneId = vmCommonArea.ZoneId, regionId = vmCommonArea.RegionId });
        }


        #endregion

        #region Common SubZone

        [HttpGet]
        public async Task<ActionResult> GetSubZoneList(int companyId, int zoneId = 0, int regionId = 0)
        {
            var model = await Task.Run(() => _service.GetSubZoneSelectList(companyId, zoneId, regionId));
            var list = model.Select(x => new { Value = x.Value, Text = x.Text }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> CommonSubZone(int companyId, int zoneId = 0, int regionId = 0, int areaId = 0)
        {

            VMCommonSubZone vmCommonSubZone = new VMCommonSubZone();
            vmCommonSubZone = await Task.Run(() => _service.GetSubZones(companyId, zoneId, regionId, areaId));
            vmCommonSubZone.ZoneList = new SelectList(_service.CommonZonesDropDownList(companyId), "Value", "Text");
            vmCommonSubZone.RegionList = _service.GetRegionSelectList(companyId, zoneId);
            vmCommonSubZone.AreaList = _service.GetAreaSelectList(companyId, zoneId, regionId);
            vmCommonSubZone.EmployeeList = _service.GetEmployeeSelectModels(companyId);
            return View(vmCommonSubZone);
        }

        [HttpPost]
        public async Task<ActionResult> CommonSubZone(VMCommonSubZone vmCommonSubZone)
        {

            if (vmCommonSubZone.ActionEum == ActionEnum.Add)
            {
                //Add 
                await _service.SubZoneAdd(vmCommonSubZone);
            }
            else if (vmCommonSubZone.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.SubZonesEdit(vmCommonSubZone);
            }
            else if (vmCommonSubZone.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.SubZonesDelete(vmCommonSubZone.ID);
            }
            else
            {
                return View("Error");
            }
            return RedirectToAction(nameof(CommonSubZone), new { companyId = vmCommonSubZone.CompanyFK, zoneId = vmCommonSubZone.ZoneId, regionId = vmCommonSubZone.RegionId, areaId = vmCommonSubZone.AreaId });
        }

        #endregion


        #region Common Finish Product Category

        public async Task<ActionResult> CommonFinishProductCategory(int companyId)
        {

            VMCommonProductCategory vmCommonProductCategory = new VMCommonProductCategory();
            vmCommonProductCategory = await Task.Run(() => _service.GetFinishProductCategory(companyId, "F"));
            return View(vmCommonProductCategory);
        }

        [HttpPost]
        public async Task<ActionResult> CommonFinishProductCategory(VMCommonProductCategory vmCommonProductCategory)
        {

            if (vmCommonProductCategory.ActionEum == ActionEnum.Add)
            {
                //Add 
                vmCommonProductCategory.ProductType = "F";
                await _service.ProductFinishCategoryAdd(vmCommonProductCategory);
            }
            else if (vmCommonProductCategory.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.ProductFinishCategoryEdit(vmCommonProductCategory);
            }
            else if (vmCommonProductCategory.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.ProductFinishCategoryDelete(vmCommonProductCategory.ID);
            }
            else
            {
                return RedirectToAction("Error");
            }
            return RedirectToAction(nameof(CommonFinishProductCategory), new { companyId = vmCommonProductCategory.CompanyFK });
        }

        #endregion

        #region Common Finish Product SubCategory

        public async Task<ActionResult> CommonFinishProductSubCategory(int companyId, int categoryId = 0)
        {

            VMCommonProductSubCategory vmCommonProductSubCategory = new VMCommonProductSubCategory();
            vmCommonProductSubCategory = await Task.Run(() => _service.GetProductSubCategory(companyId, categoryId, "F"));
            return View(vmCommonProductSubCategory);
        }

        [HttpPost]
        public async Task<ActionResult> CommonFinishProductSubCategory(VMCommonProductSubCategory vmCommonProductSubCategory)
        {

            if (vmCommonProductSubCategory.ActionEum == ActionEnum.Add)
            {
                //Add 
                vmCommonProductSubCategory.ProductType = "F";

                await _service.ProductSubCategoryAdd(vmCommonProductSubCategory);
            }
            else if (vmCommonProductSubCategory.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.ProductSubCategoryEdit(vmCommonProductSubCategory);
            }
            else if (vmCommonProductSubCategory.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.ProductSubCategoryDelete(vmCommonProductSubCategory.ID);
            }
            else
            {
                return RedirectToAction("Error");
            }
            return RedirectToAction(nameof(CommonFinishProductSubCategory), new { companyId = vmCommonProductSubCategory.CompanyFK, categoryId = vmCommonProductSubCategory.Common_ProductCategoryFk });
        }

        #endregion

        #region Common Finish Product

        public async Task<ActionResult> CommonFinishProduct(int companyId, int categoryId = 0, int subCategoryId = 0)
        {

            VMCommonProduct vmCommonProduct = new VMCommonProduct();
            vmCommonProduct = await Task.Run(() => _service.GetProduct(companyId, categoryId, subCategoryId, "F"));

            vmCommonProduct.UnitList = new SelectList(_service.UnitDropDownList(companyId), "Value", "Text");


            return View(vmCommonProduct);
        }

        public async Task<ActionResult> FeedCommonFinishProduct(int companyId, int categoryId = 0, int subCategoryId = 0)
        {

            VMCommonProduct vmCommonProduct = new VMCommonProduct();
            vmCommonProduct = await Task.Run(() => _service.GetProduct(companyId, categoryId, subCategoryId, "F"));

            vmCommonProduct.UnitList = new SelectList(_service.UnitDropDownList(companyId), "Value", "Text");


            return View(vmCommonProduct);
        }

        public async Task<ActionResult> GCCLCommonFinishProduct(int companyId, int categoryId = 0, int subCategoryId = 0)
        {

            VMCommonProduct vmCommonProduct = new VMCommonProduct();
            vmCommonProduct = await Task.Run(() => _service.GetProduct(companyId, categoryId, subCategoryId, "F"));

            vmCommonProduct.UnitList = new SelectList(_service.UnitDropDownList(companyId), "Value", "Text");


            return View(vmCommonProduct);
        }

        [HttpPost]
        public async Task<ActionResult> CommonFinishProduct(VMCommonProduct vmCommonProduct)
        {

            //if (vmCommonProduct.ImageFile != null)
            //{
            //    vmCommonProduct.Image = _service.UploadFile(vmCommonProduct.ImageFile, "Product", _webHostEnvironment.WebRootPath);
            //}
            if (vmCommonProduct.ActionEum == ActionEnum.Add)
            {
                //Add 
                vmCommonProduct.ProductType = "F";

                await _service.ProductAdd(vmCommonProduct);
            }
            else if (vmCommonProduct.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.ProductEdit(vmCommonProduct);
            }
            else if (vmCommonProduct.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.ProductDelete(vmCommonProduct.ID);
            }
            else
            {
                return RedirectToAction("Error");
            }

            if (vmCommonProduct.CompanyFK == (int)CompanyName.GloriousCropCareLimited)
            {
                return RedirectToAction(nameof(GCCLCommonFinishProduct), new
                {
                    companyId = vmCommonProduct.CompanyFK,
                    categoryId = 0,//vmCommonProduct.Common_ProductCategoryFk, 
                    subCategoryId = 0// vmCommonProduct.Common_ProductSubCategoryFk
                });
            }


            return RedirectToAction(nameof(CommonFinishProduct), new { companyId = vmCommonProduct.CompanyFK, categoryId = vmCommonProduct.Common_ProductCategoryFk, subCategoryId = vmCommonProduct.Common_ProductSubCategoryFk });
        }

        public async Task<ActionResult> CommonFinishProductBOM(int companyId, int productId = 0)
        {

            var vmFinishProductBOM = await Task.Run(() => _service.GetCommonProductByID(companyId, productId));

            return View(vmFinishProductBOM);
        }

        [HttpPost]
        public async Task<ActionResult> CommonFinishProductBOM(VMFinishProductBOM vmFinishProductBOM)
        {

            if (vmFinishProductBOM.ActionEum == ActionEnum.Add)
            {
                //Add 

                await _service.FinishProductBOMAdd(vmFinishProductBOM);
            }
            else if (vmFinishProductBOM.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.FinishProductBOMEdit(vmFinishProductBOM);
            }
            else if (vmFinishProductBOM.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.FinishProductBOMDelete(vmFinishProductBOM.ID);
            }
            else
            {
                return RedirectToAction("Error");
            }
            return RedirectToAction(nameof(CommonFinishProductBOM), new { companyId = vmFinishProductBOM.CompanyFK, productId = vmFinishProductBOM.FProductFK });
        }

        #endregion


        #region Common Raw Product Category

        [HttpGet]
        public async Task<ActionResult> CommonRawProductCategory(int companyId)
        {

            VMCommonProductCategory vmCommonProductCategory = new VMCommonProductCategory();
            vmCommonProductCategory = await Task.Run(() => _service.GetFinishProductCategory(companyId, "R"));
            return View(vmCommonProductCategory);
        }

        [HttpPost]
        public async Task<ActionResult> CommonRawProductCategory(VMCommonProductCategory vmCommonProductCategory)
        {

            if (vmCommonProductCategory.ActionEum == ActionEnum.Add)
            {
                //Add 
                vmCommonProductCategory.ProductType = "R";

                await _service.ProductFinishCategoryAdd(vmCommonProductCategory);
            }
            else if (vmCommonProductCategory.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.ProductFinishCategoryEdit(vmCommonProductCategory);
            }
            else if (vmCommonProductCategory.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.ProductFinishCategoryDelete(vmCommonProductCategory.ID);
            }
            else
            {
                return View("Error");
            }
            return RedirectToAction(nameof(CommonRawProductCategory), new { companyId = vmCommonProductCategory.CompanyFK });
        }

        #endregion

        #region Common Raw Product SubCategory

        [HttpGet]
        public async Task<ActionResult> CommonRawProductSubCategory(int companyId, int categoryId = 0)
        {

            VMCommonProductSubCategory vmCommonProductSubCategory = new VMCommonProductSubCategory();
            vmCommonProductSubCategory = await Task.Run(() => _service.GetProductSubCategory(companyId, categoryId, "R"));
            vmCommonProductSubCategory.ProductCategory = new SelectList(await _service.GetProductCategory(companyId, "R"), "Value", "Text");



            return View(vmCommonProductSubCategory);
        }

        [HttpPost]
        public async Task<ActionResult> CommonRawProductSubCategory(VMCommonProductSubCategory vmCommonProductSubCategory)
        {

            if (vmCommonProductSubCategory.ActionEum == ActionEnum.Add)
            {
                //Add 
                vmCommonProductSubCategory.ProductType = "R";

                await _service.ProductSubCategoryAdd(vmCommonProductSubCategory);
            }
            else if (vmCommonProductSubCategory.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.ProductSubCategoryEdit(vmCommonProductSubCategory);
            }
            else if (vmCommonProductSubCategory.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.ProductSubCategoryDelete(vmCommonProductSubCategory.ID);
            }
            else
            {
                return View("Error");
            }
            return RedirectToAction(nameof(CommonRawProductSubCategory), new { companyId = vmCommonProductSubCategory.CompanyFK, categoryId = vmCommonProductSubCategory.Common_ProductCategoryFk });
        }

        #endregion

        #region Common Raw Product
        [HttpGet]
        public async Task<ActionResult> CommonRawProduct(int companyId, int categoryId = 0, int subCategoryId = 0)
        {

            VMCommonProduct vmCommonProduct = new VMCommonProduct();
            vmCommonProduct = await _service.GetProduct(companyId, categoryId, subCategoryId, "R");

            vmCommonProduct.UnitList = new SelectList(_service.UnitDropDownList(companyId), "Value", "Text");
            vmCommonProduct.ProductCategoryList = new SelectList(await _service.GetProductCategory(companyId, "R"), "Value", "Text");
            return View(vmCommonProduct);
        }

        [HttpPost]
        public async Task<ActionResult> CommonRawProduct(VMCommonProduct vmCommonProduct)
        {

            //if (vmCommonProduct.ImageFile != null)
            //{
            //    vmCommonProduct.Image = _service.UploadFile(vmCommonProduct.ImageFile, "Product", _webHostEnvironment.WebRootPath);
            //}
            if (vmCommonProduct.ActionEum == ActionEnum.Add)
            {
                //Add 
                vmCommonProduct.ProductType = "R";

                await _service.ProductAdd(vmCommonProduct);
            }
            else if (vmCommonProduct.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.ProductEdit(vmCommonProduct);
            }
            else if (vmCommonProduct.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.ProductDelete(vmCommonProduct.ID);
            }
            else
            {
                return View("Error");
            }
            return RedirectToAction(nameof(CommonRawProduct), new { companyId = vmCommonProduct.CompanyFK, categoryId = vmCommonProduct.Common_ProductCategoryFk, subCategoryId = vmCommonProduct.Common_ProductSubCategoryFk });
        }

        #endregion

        #region Packing Category
        [HttpGet]
        public async Task<ActionResult> CommonPackingCategory(int companyId)
        {

            VMCommonProductCategory vmCommonProductCategory = new VMCommonProductCategory();
            vmCommonProductCategory = await Task.Run(() => _service.GetFinishProductCategory(companyId, "P"));
            return View(vmCommonProductCategory);
        }

        [HttpPost]
        public async Task<ActionResult> CommonPackingCategory(VMCommonProductCategory vmCommonProductCategory)
        {

            if (vmCommonProductCategory.ActionEum == ActionEnum.Add)
            {
                //Add 
                vmCommonProductCategory.ProductType = "P";

                await _service.ProductFinishCategoryAdd(vmCommonProductCategory);
            }
            else if (vmCommonProductCategory.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.ProductFinishCategoryEdit(vmCommonProductCategory);
            }
            else if (vmCommonProductCategory.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.ProductFinishCategoryDelete(vmCommonProductCategory.ID);
            }
            else
            {
                return RedirectToAction("Error");
            }
            return RedirectToAction(nameof(CommonPackingCategory), new { companyId = vmCommonProductCategory.CompanyFK });
        }

        #endregion

        #region Common Raw Product SubCategory

        public async Task<ActionResult> CommonPackingSubCategory(int companyId, int categoryId = 0)
        {

            VMCommonProductSubCategory vmCommonProductSubCategory = new VMCommonProductSubCategory();
            vmCommonProductSubCategory = await Task.Run(() => _service.GetProductSubCategory(companyId, categoryId, "P"));
            return View(vmCommonProductSubCategory);
        }

        [HttpPost]
        public async Task<ActionResult> CommonPackingSubCategory(VMCommonProductSubCategory vmCommonProductSubCategory)
        {

            if (vmCommonProductSubCategory.ActionEum == ActionEnum.Add)
            {
                //Add 
                vmCommonProductSubCategory.ProductType = "P";

                await _service.ProductSubCategoryAdd(vmCommonProductSubCategory);
            }
            else if (vmCommonProductSubCategory.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.ProductSubCategoryEdit(vmCommonProductSubCategory);
            }
            else if (vmCommonProductSubCategory.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.ProductSubCategoryDelete(vmCommonProductSubCategory.ID);
            }
            else
            {
                return RedirectToAction("Error");
            }
            return RedirectToAction(nameof(CommonPackingSubCategory), new { companyId = vmCommonProductSubCategory.CompanyFK, categoryId = vmCommonProductSubCategory.Common_ProductCategoryFk });
        }

        #endregion

        #region Common Raw Product
        public async Task<ActionResult> CommonPackingMaterials(int companyId, int categoryId = 0, int subCategoryId = 0)
        {

            VMCommonProduct vmCommonProduct = new VMCommonProduct();
            vmCommonProduct = await Task.Run(() => _service.GetProduct(companyId, categoryId, subCategoryId, "P"));

            vmCommonProduct.UnitList = new SelectList(_service.UnitDropDownList(companyId), "Value", "Text");


            return View(vmCommonProduct);
        }

        [HttpPost]
        public async Task<ActionResult> CommonPackingMaterials(VMCommonProduct vmCommonProduct)
        {

            //if (vmCommonProduct.ImageFile != null)
            //{
            //    vmCommonProduct.Image = _service.UploadFile(vmCommonProduct.ImageFile, "Product", _webHostEnvironment.WebRootPath);
            //}
            if (vmCommonProduct.ActionEum == ActionEnum.Add)
            {
                //Add 
                vmCommonProduct.ProductType = "P";

                await _service.ProductAdd(vmCommonProduct);
            }
            else if (vmCommonProduct.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.ProductEdit(vmCommonProduct);
            }
            else if (vmCommonProduct.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.ProductDelete(vmCommonProduct.ID);
            }
            else
            {
                return RedirectToAction("Error");
            }
            return RedirectToAction(nameof(CommonPackingMaterials), new { companyId = vmCommonProduct.CompanyFK, categoryId = vmCommonProduct.Common_ProductCategoryFk, subCategoryId = vmCommonProduct.Common_ProductSubCategoryFk });
        }

        #endregion

        #region Common Trems And Conditions
        [HttpGet]
        public async Task<ActionResult> POTremsAndConditions(int companyId)
        {
            VMPOTremsAndConditions vmTremsAndConditions = new VMPOTremsAndConditions();
            vmTremsAndConditions = await Task.Run(() => _service.GetPOTremsAndConditions(companyId));
            return View(vmTremsAndConditions);
        }

        [HttpPost]
        public async Task<ActionResult> POTremsAndConditions(VMPOTremsAndConditions vmpoTremsAndConditions)
        {

            if (vmpoTremsAndConditions.ActionEum == ActionEnum.Add)
            {
                //Add 
                await _service.POTremsAndConditionAdd(vmpoTremsAndConditions);
            }
            else if (vmpoTremsAndConditions.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.POTremsAndConditionEdit(vmpoTremsAndConditions);
            }
            else if (vmpoTremsAndConditions.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.POTremsAndConditionDelete(vmpoTremsAndConditions.ID);
            }
            else
            {
                return RedirectToAction("Error");
            }
            return RedirectToAction(nameof(POTremsAndConditions), new { companyId = vmpoTremsAndConditions.CompanyFK });
        }

        #endregion

        public JsonResult CommonProductByIDGet(int id)
        {
            var model = _service.GetCommonProductByID(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public JsonResult FinishProductBOMsByIDGet(int id)
        {
            var model = _service.GetFinishProductBOMsByID(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> ChangeProductMRPPrice(int id = 0)
        {

            VMCommonProduct vmCommonProduct = new VMCommonProduct();
            if (id > 0)
            {
                vmCommonProduct = _service.GetCommonProductByID(id);
            }


            return View(vmCommonProduct);
        }

        [HttpPost]
        public async Task<ActionResult> ChangeProductMRPPrice(VMCommonProduct vmCommonProduct)
        {

            if (vmCommonProduct.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.ProductMRPPriceEdit(vmCommonProduct);
            }
            else
            {
                return View("Error");
            }
            return RedirectToAction(nameof(ChangeProductMRPPrice), new { id = vmCommonProduct.ID });
        }

        //public async Task<ActionResult> MakeCommonProductGRN(int id = 0)
        //{
        //    if (_vmLogin == null)
        //    {
        //        return RedirectToAction("Login", "Auth");
        //    }
        //    VMCommonProduct vmCommonProduct = new VMCommonProduct();
        //    if (id > 0)
        //    {
        //        vmCommonProduct = _service.GetCommonProductByID(id);
        //    }

        //    return View(vmCommonProduct);
        //}


        //[HttpPost]
        //public async Task<ActionResult> MakeCommonProductGRN(VMCommonProduct vmCommonProduct)
        //{

        //    if (vmCommonProduct.ActionEum == ActionEnum.Edit)
        //    {
        //        //Edit
        //        await _service.ProductGRNEdit(vmCommonProduct);
        //    }
        //    else
        //    {
        //        return RedirectToAction("Error");
        //    }
        //    return RedirectToAction(nameof(MakeCommonProductGRN), new { id = vmCommonProduct.ID });
        //}

        public JsonResult CommonProductByID(int id)
        {

            var model = _service.GetCommonProductByID(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteSupplier(int companyId, string prefix)
        {
            var products = _service.GetAutoCompleteSupplier(companyId, prefix);
            return Json(products, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteSupplierGet(string id)
        {
            var products = _service.GetAutoCompleteSupplier(id);
            return Json(products, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteProductCategoryGet(int companyId, string prefix, string productType)
        {
            var products = _service.GetAutoCompleteProductCategory(companyId, prefix, productType);
            return Json(products, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteProductGet(int companyId, string prefix, string productType = "")
        {
            var products = _service.GetAutoCompleteProduct(companyId, prefix, productType);
            return Json(products, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteAdministrativeExpenseHeadGlGet(int companyId, string prefix)
        {
            var products = _service.AutoCompleteAdministrativeExpenseHeadGlGet(companyId, prefix);
            return Json(products, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GCCLAutoCompleteRawPackingMaterialsGet(int companyId, string prefix)
        {
            var products = _service.GCCLGetAutoCompleteRawPackingMaterials(companyId, prefix);
            return Json(products, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getallEmployee(string prefix)
        {
            var products = _service.AllEmployee(prefix);
            return Json(products, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getallEmployeeforMenu(string prefix)
        {
            var products = _service.AllEmployeeForMenu(prefix);
            return Json(products, JsonRequestBehavior.AllowGet);
        }

        #region Common Supplier


        public JsonResult CommonSupplierByIDGet(int id)
        {
            var model = _service.GetCommonSupplierByID(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> CommonSupplier(int companyId)
        {

            VMCommonSupplier vmCommonSupplier = new VMCommonSupplier();
            vmCommonSupplier = await Task.Run(() => _service.GetSupplier(companyId));
            vmCommonSupplier.CountryList = new SelectList(_service.CommonCountriesDropDownList(), "Value", "Text");

            return View(vmCommonSupplier);
        }

        [HttpPost]
        public async Task<ActionResult> CommonSupplier(VMCommonSupplier vmCommonSupplier)
        {

            if (vmCommonSupplier.ActionEum == ActionEnum.Add)
            {
                //Add 
                await _service.SupplierAdd(vmCommonSupplier);
            }
            else if (vmCommonSupplier.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.SupplierEdit(vmCommonSupplier);
            }
            else if (vmCommonSupplier.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.SupplierDelete(vmCommonSupplier.ID);
            }
            else
            {
                return View("Error");
            }
            return RedirectToAction(nameof(CommonSupplier), new { companyId = vmCommonSupplier.CompanyFK });
        }

        #endregion

        #region Common Customer
        public JsonResult CommonCustomerByIDGet(int id)
        {
            var model = _service.GetCommonCustomerByID(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RSCustomerByIDGet(int id)
        {
            var model = _service.GetRSCustomerByID(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public JsonResult FeedCommonCustomerByIDGet(int id)
        {
            var model = _service.GetCommonCustomerByIDFeed(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> RSCommonCustomer(int companyId)
        {

            VMCommonSupplier vmCommonCustomer = new VMCommonSupplier();
            vmCommonCustomer = await Task.Run(() => _service.RSGetCustomer(companyId));
            vmCommonCustomer.DivisionList = new SelectList(_service.CommonDivisionsDropDownList(), "Value", "Text");
            vmCommonCustomer.DistrictList = new SelectList(_service.CommonDistrictsDropDownList(), "Value", "Text");
            vmCommonCustomer.UpazilasList = new SelectList(_service.CommonUpazilasDropDownList(), "Value", "Text");
            vmCommonCustomer.PaymentTypeList = new SelectList(_service.CommonCustomerPaymentType(), "Value", "Text");

            return View(vmCommonCustomer);
        }

        [HttpPost]
        public async Task<ActionResult> RSCommonCustomer(VMCommonSupplier vmCommonCustomer, HttpPostedFileBase file)
        {

            List<FileItem> itemlist = new List<FileItem>();
            if (file != null)
            {
                FileItem item = new FileItem();
                item.file = file;
                item.docdesc = "Vendor Photo";
                item.docfilename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                item.docid = 0;
                item.FileCatagoryId = 4;
                item.fileext = Path.GetExtension(file.FileName);
                item.isactive = true;
                item.RecDate = DateTime.Now;
                item.SortOrder = 1;
                item.userid = Convert.ToInt32(Session["Id"]);
                itemlist.Add(item);
                itemlist = await _ftpService.UploadFileBulk(itemlist, "VendorImage");
                if (file != null)
                {
                    var ress = itemlist.FirstOrDefault(f => f.SortOrder == 1);
                    vmCommonCustomer.ImageDocId = ress.docid;
                }
            }
            if (vmCommonCustomer.ActionEum == ActionEnum.Add)
            {
                //Add 
                await _service.RSCustomerAdd(vmCommonCustomer);
            }
            else if (vmCommonCustomer.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.RSCustomerEdit(vmCommonCustomer);
            }
            else if (vmCommonCustomer.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.CustomerDelete(vmCommonCustomer.ID);
            }
            else
            {
                return RedirectToAction("Error");
            }
            return RedirectToAction(nameof(RSCommonCustomer), new { companyId = vmCommonCustomer.CompanyFK });
        }


        [HttpGet]
        public async Task<ActionResult> RSCommonCustomerBooking(int companyId, int vendorId = 0, int productSubCategoryId = 0)
        {
            VMCommonSupplier vmCommonCustomer = new VMCommonSupplier();
            if (vendorId > 0)
            {
                vmCommonCustomer = await Task.Run(() => _service.RSGetCustomerBooking(companyId, vendorId));
            }
            else
            {
                vmCommonCustomer = await Task.Run(() => _service.RSGetCustomerBookingProductCategories(companyId, productSubCategoryId));

            }
            return View(vmCommonCustomer);
        }

        [HttpGet]
        public async Task<ActionResult> RSCommonCustomerGroup(int companyId, int vendorId)
        {
            VMCommonSupplier vmCommonCustomer = new VMCommonSupplier();
            vmCommonCustomer = await Task.Run(() => _service.RSGetCustomerGroup(companyId, vendorId));
            vmCommonCustomer.DivisionList = new SelectList(_service.CommonDivisionsDropDownList(), "Value", "Text");
            vmCommonCustomer.DistrictList = new SelectList(_service.CommonDistrictsDropDownList(), "Value", "Text");
            vmCommonCustomer.UpazilasList = new SelectList(_service.CommonUpazilasDropDownList(), "Value", "Text");
            vmCommonCustomer.PaymentTypeList = new SelectList(_service.CommonCustomerPaymentType(), "Value", "Text");

            return View(vmCommonCustomer);
        }

        [HttpPost]
        public async Task<ActionResult> RSCommonCustomerGroup(VMCommonSupplier vmCommonCustomer, HttpPostedFileBase file)
        {

            List<FileItem> itemList = new List<FileItem>();
            if (file != null)
            {
                FileItem item = new FileItem();
                item.file = file;
                item.docdesc = "Vendor Photo";
                item.docfilename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                item.docid = 0;
                item.FileCatagoryId = 4;
                item.fileext = Path.GetExtension(file.FileName);
                item.isactive = true;
                item.RecDate = DateTime.Now;
                item.SortOrder = 1;
                item.userid = Convert.ToInt32(Session["Id"]);
                itemList.Add(item);
                itemList = await _ftpService.UploadFileBulk(itemList, "VendorImage");
                if (file != null)
                {
                    var res = itemList.FirstOrDefault(f => f.SortOrder == 1);
                    vmCommonCustomer.ImageDocId = res.docid;
                }
            }

            if (vmCommonCustomer.ActionEum == ActionEnum.Add)
            {
                //Add 
                var res = await _service.RSCustomerGroupAdd(vmCommonCustomer);
            }
            else if (vmCommonCustomer.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.RSCustomerGroupEdit(vmCommonCustomer);
            }
            else if (vmCommonCustomer.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.CustomerDelete(vmCommonCustomer.ID);
            }
            else
            {
                return View("Error");
            }
            return RedirectToAction(nameof(RSCommonCustomerGroup), new { companyId = vmCommonCustomer.CompanyFK, vendorId = vmCommonCustomer.VendorReferenceId });
        }

        [HttpGet]
        public async Task<ActionResult> CommonFeedCustomer(int companyId, int zoneId = 0, int? subZoneId = 0)
        {

            VMCommonSupplier vmCommonCustomer = new VMCommonSupplier();
            vmCommonCustomer = await Task.Run(() => _service.GetFeedCustomer(companyId, zoneId));
            vmCommonCustomer.DivisionList = new SelectList(_service.CommonDivisionsDropDownList(), "Value", "Text");
            vmCommonCustomer.DistrictList = new SelectList(_service.CommonDistrictsDropDownList(), "Value", "Text");
            vmCommonCustomer.UpazilasList = new SelectList(_service.CommonUpazilasDropDownList(), "Value", "Text");
            vmCommonCustomer.PaymentTypeList = new SelectList(_service.CommonCustomerPaymentType(), "Value", "Text");
            vmCommonCustomer.NomineeRelationList = new SelectList(_service.CommonRelationList(), "Value", "Text");
            vmCommonCustomer.ZoneList = new SelectList(_service.CommonZonesDropDownList(companyId), "Value", "Text");
            vmCommonCustomer.TerritoryList = new SelectList(_service.CommonSubZonesDropDownList(companyId), "Value", "Text");


            return View(vmCommonCustomer);
        }

        [HttpGet]
        public async Task<ActionResult> CommonCustomer(int companyId, int zoneId = 0, int regionId = 0, int areaId = 0, int subZoneId = 0)
        {
            VMCommonSupplier vmCommonCustomer = new VMCommonSupplier();
            vmCommonCustomer = await Task.Run(() => _service.GetCustomer(companyId, zoneId, subZoneId));
            vmCommonCustomer.DivisionList = new SelectList(_service.CommonDivisionsDropDownList(), "Value", "Text");
            vmCommonCustomer.DistrictList = new SelectList(_service.CommonDistrictsDropDownList(), "Value", "Text");
            vmCommonCustomer.UpazilasList = new SelectList(_service.CommonUpazilasDropDownList(), "Value", "Text");
            vmCommonCustomer.PaymentTypeList = new SelectList(_service.CommonCustomerPaymentType(), "Value", "Text");
            vmCommonCustomer.ZoneList = new SelectList(_service.CommonZonesDropDownList(companyId), "Value", "Text");
            vmCommonCustomer.RegionList = new SelectList(_service.CommonRegionDropDownList(companyId, zoneId), "Value", "Text");
            vmCommonCustomer.AreaList = new SelectList(_service.CommonAreaDropDownList(companyId, zoneId, regionId), "Value", "Text");
            vmCommonCustomer.TerritoryList = new SelectList(_service.CommonSubZonesDropDownList(companyId), "Value", "Text");
            return View(vmCommonCustomer);
        }

        [HttpPost]
        public async Task<ActionResult> CommonCustomer(VMCommonSupplier vmCommonCustomer)
        {

            if (vmCommonCustomer.ActionEum == ActionEnum.Add)
            {
                //Add 
                await _service.CustomerAdd(vmCommonCustomer);
            }
            else if (vmCommonCustomer.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.CustomerEdit(vmCommonCustomer);
            }
            else if (vmCommonCustomer.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.CustomerDelete(vmCommonCustomer.ID);
            }
            else
            {
                return View("Error");
            }

            if (vmCommonCustomer.CompanyFK == (int)CompanyName.GloriousLandsAndDevelopmentsLimited || vmCommonCustomer.CompanyFK == (int)CompanyName.KrishibidPropertiesLimited)
            {
                return RedirectToAction(nameof(RSCommonCustomer), new { companyId = vmCommonCustomer.CompanyFK });
            }
            if (vmCommonCustomer.CompanyFK == (int)CompanyName.KrishibidFeedLimited)
            {
                return RedirectToAction(nameof(CommonFeedCustomer), new { companyId = vmCommonCustomer.CompanyFK });
            }
            return RedirectToAction(nameof(CommonCustomer), new { companyId = vmCommonCustomer.CompanyFK });
        }

        [HttpGet]
        public async Task<ActionResult> CommonCustomerByID(int customerId)
        {

            VMCommonSupplier vmCommonCustomer = new VMCommonSupplier();
            vmCommonCustomer = await Task.Run(() => _service.GetCustomerById(customerId));
            return View(vmCommonCustomer);
        }

        [HttpPost]
        public async Task<ActionResult> CommonCustomerDelete(VMCommonCustomer vmCommonCustomer)
        {

            if (vmCommonCustomer.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.CustomerDelete(vmCommonCustomer.ID);
            }
            else
            {
                return RedirectToAction("Error");
            }
            return RedirectToAction(nameof(CommonCustomer));
        }

        [HttpPost]
        public async Task<JsonResult> DeleteVendorImageFile(long docId)
        {
            var result = await _ftpService.DeletePermanentlyVendor(docId);
            return Json(result);
        }

        #endregion

        #region Common Deport

        [HttpGet]
        public async Task<ActionResult> CommonDeport(int companyId, int zoneId = 0, int regionId = 0, int areaId = 0, int subZoneId = 0)
        {
            VMCommonSupplier vmCommonDeport = new VMCommonSupplier();
            vmCommonDeport = await Task.Run(() => _service.GetDeport(companyId, zoneId, subZoneId));
            vmCommonDeport.DivisionList = new SelectList(_service.CommonDivisionsDropDownList(), "Value", "Text");
            vmCommonDeport.DistrictList = new SelectList(_service.CommonDistrictsDropDownList(), "Value", "Text");
            vmCommonDeport.UpazilasList = new SelectList(_service.CommonUpazilasDropDownList(), "Value", "Text");
            vmCommonDeport.PaymentTypeList = new SelectList(_service.CommonCustomerPaymentType(), "Value", "Text");
            vmCommonDeport.ZoneList = new SelectList(_service.CommonZonesDropDownList(companyId), "Value", "Text");
            vmCommonDeport.RegionList = new SelectList(_service.CommonRegionDropDownList(companyId, zoneId), "Value", "Text");
            vmCommonDeport.AreaList = new SelectList(_service.CommonAreaDropDownList(companyId, zoneId, regionId), "Value", "Text");
            vmCommonDeport.TerritoryList = new SelectList(_service.CommonSubZonesDropDownList(companyId), "Value", "Text");
            return View(vmCommonDeport);
        }

        [HttpPost]
        public async Task<ActionResult> CommonDeport(VMCommonSupplier vmCommonDeport)
        {

            if (vmCommonDeport.ActionEum == ActionEnum.Add)
            {
                //Add 
                await _service.DeportAdd(vmCommonDeport);
            }
            else if (vmCommonDeport.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.DeportEdit(vmCommonDeport);
            }
            else if (vmCommonDeport.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.DeportDelete(vmCommonDeport.ID);
            }
            else
            {
                return View("Error");
            }

            return RedirectToAction(nameof(CommonDeport), new { companyId = vmCommonDeport.CompanyFK });
        }

        [HttpGet]
        public async Task<ActionResult> CommonDeportById(int deportId)
        {

            VMCommonSupplier vmCommonDeport = new VMCommonSupplier();
            vmCommonDeport = await Task.Run(() => _service.GetDeportById(deportId));
            return View(vmCommonDeport);
        }

        [HttpGet]
        public JsonResult CommonDeportByIdGet(int id)
        {
            var model = _service.GetCommonDeportById(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Common Dealer

        [HttpGet]
        public async Task<ActionResult> CommonDealer(int companyId, int zoneId = 0, int regionId = 0, int areaId = 0, int subZoneId = 0)
        {
            VMCommonSupplier vmCommonDealer = new VMCommonSupplier();
            vmCommonDealer = await Task.Run(() => _service.GetDealer(companyId, zoneId, subZoneId));
            vmCommonDealer.DeportList = new SelectList(_service.CommonDeportDropDownList(), "Value", "Text");
            vmCommonDealer.DivisionList = new SelectList(_service.CommonDivisionsDropDownList(), "Value", "Text");
            vmCommonDealer.DistrictList = new SelectList(_service.CommonDistrictsDropDownList(), "Value", "Text");
            vmCommonDealer.UpazilasList = new SelectList(_service.CommonUpazilasDropDownList(), "Value", "Text");
            vmCommonDealer.PaymentTypeList = new SelectList(_service.CommonCustomerPaymentType(), "Value", "Text");
            vmCommonDealer.ZoneList = new SelectList(_service.CommonZonesDropDownList(companyId), "Value", "Text");
            vmCommonDealer.RegionList = new SelectList(_service.CommonRegionDropDownList(companyId, zoneId), "Value", "Text");
            vmCommonDealer.AreaList = new SelectList(_service.CommonAreaDropDownList(companyId, zoneId, regionId), "Value", "Text");
            vmCommonDealer.TerritoryList = new SelectList(_service.CommonSubZonesDropDownList(companyId), "Value", "Text");
            return View(vmCommonDealer);
        }

        [HttpPost]
        public async Task<ActionResult> CommonDealer(VMCommonSupplier vmCommonDealer)
        {

            if (vmCommonDealer.ActionEum == ActionEnum.Add)
            {
                //Add 
                await _service.DealerAdd(vmCommonDealer);
            }
            else if (vmCommonDealer.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.DealerEdit(vmCommonDealer);
            }
            else if (vmCommonDealer.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.DealerDelete(vmCommonDealer.ID);
            }
            else
            {
                return View("Error");
            }

            return RedirectToAction(nameof(CommonDealer), new { companyId = vmCommonDealer.CompanyFK });
        }

        [HttpGet]
        public async Task<ActionResult> CommonDealerById(int deportId)
        {

            VMCommonSupplier vmCommonDealer = new VMCommonSupplier();
            vmCommonDealer = await Task.Run(() => _service.GetDealerById(deportId));
            return View(vmCommonDealer);
        }

        [HttpGet]
        public JsonResult CommonDealerByIdGet(int id)
        {
            var model = _service.GetCommonDealerById(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Geolocation
        [HttpGet]
        public async Task<ActionResult> CommonDivisions()
        {
            VMCommonDivisions vmCommonDivisions = new VMCommonDivisions();
            vmCommonDivisions = await Task.Run(() => _service.GetDivisions());
            return View(vmCommonDivisions);
        }

        [HttpGet]
        public async Task<ActionResult> CommonDistricts(int divisionsId = 0)
        {

            VMCommonDistricts vmCommonDistricts = new VMCommonDistricts();
            vmCommonDistricts = await Task.Run(() => _service.GetDistricts(divisionsId));
            vmCommonDistricts.DivisionList = new SelectList(_service.CommonDivisionsDropDownList(), "Value", "Text");
            return View(vmCommonDistricts);
        }

        [HttpGet]
        public async Task<ActionResult> CommonUpazilas(int divisionsId = 0, int districtsId = 0)
        {


            VMCommonThana vmCommonThana = new VMCommonThana();
            vmCommonThana = await Task.Run(() => _service.GetUpazilas(divisionsId, districtsId));
            vmCommonThana.DistrictList = await Task.Run(() => _service.GetDistrictsDropDownList());
            return View(vmCommonThana);
        }

        [HttpGet]
        public async Task<ActionResult> CommonDistrictsGet(int id)
        {

            var vmCDistricts = await Task.Run(() => _service.CommonDistrictsGet(id));
            var list = vmCDistricts.Select(x => new { Value = x.ID, Text = x.Name }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> CommonSubZonesGet(int id)
        {

            var vmCDistricts = await Task.Run(() => _service.CommonSubZonesGet(id));
            var list = vmCDistricts.Select(x => new { Value = x.ID, Text = x.Name }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> CommonRegionGet(int companyId, int zoneId)
        {

            var dts = await Task.Run(() => _service.CommonRegionGet(companyId, zoneId));
            var list = dts.Select(x => new { Value = x.ID, Text = x.Name }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> AllRegionGet(int companyId)
        {

            var dts = await Task.Run(() => _service.AllRegionGet(companyId));
            var list = dts.Select(x => new { Value = x.ID, Text = x.Name }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }



        [HttpGet]
        public async Task<ActionResult> CommonUpazilasGet(int id)
        {

            var vmPoliceStations = await Task.Run(() => _service.DDLUpazilasListByDistrictID(id));
            return Json(vmPoliceStations, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public async Task<ActionResult> AccountingSignatory(int companyId)
        {
            VMAccountingSignatory vmAccountingSignatory = new VMAccountingSignatory();
            vmAccountingSignatory = await Task.Run(() => _service.GetAccountingSignatory(companyId));
            vmAccountingSignatory.CompanyList = new SelectList(_service.CompaniesDropDownList(), "Value", "Text");
            return View(vmAccountingSignatory);
        }

        [HttpPost]
        public async Task<ActionResult> AccountingSignatory(VMAccountingSignatory vmAccountingSignatory)
        {

            if (vmAccountingSignatory.ActionEum == ActionEnum.Add)
            {


                await _service.AccountingSignatoryAdd(vmAccountingSignatory);
            }
            else if (vmAccountingSignatory.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.AccountingSignatoryEdit(vmAccountingSignatory);
            }
            else if (vmAccountingSignatory.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.AccountingSignatoryDelete(vmAccountingSignatory.SignatoryId);
            }
            else
            {
                return RedirectToAction("Error");
            }
            return RedirectToAction(nameof(AccountingSignatory), new { companyId = vmAccountingSignatory.CompanyFK });
        }

        [HttpGet]
        public async Task<ActionResult> Company()
        {

            VMCompany VMCompany = new VMCompany();
            VMCompany = await Task.Run(() => _service.GetCompany());
            //VMCompany.CompanyList = new SelectList(_service.CompaniesDropDownList(), "Value", "Text");

            return View(VMCompany);
        }

        [HttpPost]
        public async Task<ActionResult> Company(VMCompany VMCompany)
        {

            if (VMCompany.ActionEum == ActionEnum.Add)
            {


                await _service.CompanyAdd(VMCompany);
            }
            //else if (VMCompany.ActionEum == ActionEnum.Edit)
            //{
            //    //Edit
            //    await _service.AccountingSignatoryEdit(VMCompany);
            //}
            //else if (VMCompany.ActionEum == ActionEnum.Delete)
            //{
            //    //Delete
            //    await _service.AccountingSignatoryDelete(VMCompany.SignatoryId);
            //}
            else
            {
                return RedirectToAction("Error");
            }
            return RedirectToAction(nameof(Company));
        }

        #region Common Bank

        [HttpGet]
        public async Task<ActionResult> CommonBank(int companyId)
        {
            VMCommonBank vmCommonBank = new VMCommonBank();
            vmCommonBank = await Task.Run(() => _service.GetBanks(companyId));
            return View(vmCommonBank);
        }

        [HttpPost]
        public async Task<ActionResult> CommonBank(VMCommonBank vMCommonBank)
        {

            if (vMCommonBank.ActionEum == ActionEnum.Add)
            {
                //Add 
                await _service.BankAdd(vMCommonBank);
            }
            else if (vMCommonBank.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.BankEdit(vMCommonBank);
            }
            else if (vMCommonBank.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.BankDelete(vMCommonBank.ID);
            }
            else
            {
                return RedirectToAction("Error");
            }
            return RedirectToAction(nameof(CommonBank), new { companyId = vMCommonBank.CompanyFK });
        }

        #endregion


        #region Bank Branch
        [HttpGet]
        public async Task<ActionResult> CommonBankBranchGet(int companyId, int bankId)
        {

            var vmCommonProductSubCategory = await Task.Run(() => _service.CommonBankGet(companyId, bankId));
            var list = vmCommonProductSubCategory.Select(x => new { Value = x.ID, Text = x.Name }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> CommonBankBranch(int companyId, int bankId = 0)
        {

            VMCommonBankBranch vMCommonBankBranch = new VMCommonBankBranch();
            vMCommonBankBranch = await Task.Run(() => _service.GetBankBranches(companyId, bankId));
            vMCommonBankBranch.BankList = new SelectList(_service.CommonBanksDropDownList(companyId), "Value", "Text");

            return View(vMCommonBankBranch);
        }

        [HttpPost]
        public async Task<ActionResult> CommonBankBranch(VMCommonBankBranch vMCommonBankBranch)
        {

            if (vMCommonBankBranch.ActionEum == ActionEnum.Add)
            {
                //Add 

                await _service.BankBranchAdd(vMCommonBankBranch);
            }
            else if (vMCommonBankBranch.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.BankBranchEdit(vMCommonBankBranch);
            }
            else if (vMCommonBankBranch.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.BankBranchDelete(vMCommonBankBranch.ID);
            }
            else
            {
                return RedirectToAction("Error");
            }
            return RedirectToAction(nameof(CommonBankBranch), new { companyId = vMCommonBankBranch.CompanyFK, bankId = vMCommonBankBranch.BankId });
        }

        #endregion

        #region Common Department

        [HttpGet]
        public async Task<ActionResult> CommonDepartment(int companyId)
        {
            VMCommonDepartment vmCommonDepartment = new VMCommonDepartment();
            vmCommonDepartment = await _departmentService.GetDepartments(companyId);
            return View(vmCommonDepartment);
        }

        [HttpPost]
        public async Task<ActionResult> CommonDepartment(VMCommonDepartment vmCommonDepartment)
        {

            if (vmCommonDepartment.ActionEum == ActionEnum.Add)
            {
                //Add 
                await _departmentService.DepartmentAdd(vmCommonDepartment);
            }
            else if (vmCommonDepartment.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _departmentService.DepartmentEdit(vmCommonDepartment);
            }
            else if (vmCommonDepartment.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _departmentService.DepartmentDelete(vmCommonDepartment.ID);
            }
            else
            {
                return RedirectToAction("Error");
            }
            return RedirectToAction(nameof(CommonDepartment), new { companyId = vmCommonDepartment.CompanyFK });
        }

        #endregion

        #region Common Designation

        [HttpGet]
        public async Task<ActionResult> CommonHrDesignation(int companyId)
        {
            VMCommonHrDesignation vmCommonDesignation = new VMCommonHrDesignation();
            vmCommonDesignation = await _hrDesignationService.GetHrDesignations(companyId);
            return View(vmCommonDesignation);
        }

        [HttpPost]
        public async Task<ActionResult> CommonHrDesignation(VMCommonHrDesignation vmCommonDesignation)
        {

            if (vmCommonDesignation.ActionEum == ActionEnum.Add)
            {
                //Add 
                await _hrDesignationService.HrDesignationAdd(vmCommonDesignation);
            }
            else if (vmCommonDesignation.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _hrDesignationService.HrDesignationEdit(vmCommonDesignation);
            }
            else if (vmCommonDesignation.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _hrDesignationService.HrDesignationDelete(vmCommonDesignation.ID);
            }
            else
            {
                return RedirectToAction("Error");
            }
            return RedirectToAction(nameof(CommonHrDesignation), new { companyId = vmCommonDesignation.CompanyFK });
        }

        #endregion

        #region Account Cheque Info

        public JsonResult CommonActChequeInfoByIDGet(int id)
        {
            var model = _service.GetActChequeInfoByID(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> CommonActChequeInfo(int companyId)
        {

            VMCommonActChequeInfo vMCommonActChequeInfo = new VMCommonActChequeInfo();
            vMCommonActChequeInfo = await Task.Run(() => _service.GetActChequeInfos(companyId));
            vMCommonActChequeInfo.BankList =
                new SelectList(_service.CommonBanksDropDownList(companyId), "Value", "Text");

            vMCommonActChequeInfo.BankBranchList =
               new SelectList(_service.CommonBankBranchesDropDownList(companyId), "Value", "Text");


            vMCommonActChequeInfo.SignatoryList =
              new SelectList(_service.CommonActSignatoryDropdownList(companyId), "Value", "Text");

            return View(vMCommonActChequeInfo);
        }

        [HttpPost]
        public async Task<ActionResult> CommonActChequeInfo(VMCommonActChequeInfo vMCommonActChequeInfo)
        {

            if (vMCommonActChequeInfo.ActionEum == ActionEnum.Add)
            {
                //Add 
                await _service.ActChequeInfoAdd(vMCommonActChequeInfo);
            }
            else if (vMCommonActChequeInfo.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.ActChequeInfoEdit(vMCommonActChequeInfo);
            }
            else if (vMCommonActChequeInfo.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.ActChequeInfoDelete(vMCommonActChequeInfo.ID);
            }
            else
            {
                return RedirectToAction("Error");
            }
            return RedirectToAction(nameof(CommonActChequeInfo), new { companyId = vMCommonActChequeInfo.CompanyFK });
        }

        #endregion


        #region Common Raw Product Category

        public async Task<ActionResult> ProductCategoryProject(int companyId, string productType)
        {

            VMCommonProductCategory vmCommonProductCategory = new VMCommonProductCategory();
            vmCommonProductCategory = await Task.Run(() => _service.GetProductCategoryProject(companyId, productType));
            return View(vmCommonProductCategory);
        }

        [HttpPost]
        public async Task<ActionResult> ProductCategoryProject(VMCommonProductCategory vmProductCategoryProject)
        {

            if (vmProductCategoryProject.ActionEum == ActionEnum.Add)
            {
                //Add                

                await _service.ProductCategoryProjectAdd(vmProductCategoryProject);
            }
            else if (vmProductCategoryProject.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.ProductCategoryProjectEdit(vmProductCategoryProject);
            }
            else if (vmProductCategoryProject.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.ProductCategoryProjectDelete(vmProductCategoryProject.ID);
            }
            else
            {
                return RedirectToAction("Error");
            }
            return RedirectToAction(nameof(ProductCategoryProject), new { companyId = vmProductCategoryProject.CompanyFK, productType = vmProductCategoryProject.ProductType });
        }

        #endregion

        #region Common Raw Product SubCategory

        [HttpGet]
        public async Task<ActionResult> ProductSubCategoryBlock(int companyId, int categoryId = 0, string productType = "")
        {

            VMCommonProductSubCategory vmCommonProductSubCategory = new VMCommonProductSubCategory();

            vmCommonProductSubCategory = await Task.Run(() => _service.GetProductSubCategory(companyId, categoryId, productType));
            vmCommonProductSubCategory.ProductCategoryList = await _service.GetProductCategory(companyId, productType);
            return View(vmCommonProductSubCategory);
        }

        [HttpPost]
        public async Task<ActionResult> ProductSubCategoryBlock(VMCommonProductSubCategory vmCommonProductSubCategory)
        {

            if (vmCommonProductSubCategory.ActionEum == ActionEnum.Add)
            {
                //Add               

                await _service.ProductSubCategoryAdd(vmCommonProductSubCategory);
            }
            else if (vmCommonProductSubCategory.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.ProductSubCategoryEdit(vmCommonProductSubCategory);
            }
            else if (vmCommonProductSubCategory.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.ProductSubCategoryDelete(vmCommonProductSubCategory.ID);
            }
            else
            {
                return RedirectToAction("Error");
            }
            return RedirectToAction(nameof(ProductSubCategoryBlock), new { companyId = vmCommonProductSubCategory.CompanyFK, categoryId = vmCommonProductSubCategory.Common_ProductCategoryFk, productType = vmCommonProductSubCategory.ProductType });
        }

        #endregion

        #region Common Raw Product
        public async Task<ActionResult> ProductPlotOrFlat(int companyId, int categoryId = 0, int subCategoryId = 0, string productType = "")
        {

            VMrealStateProductsForList vm = new VMrealStateProductsForList();
            vm = await _service.GetPlotOrFlat(companyId, categoryId, subCategoryId);
            vm.ProductType = productType;
            return View(vm);
        }

        [HttpPost]
        public async Task<ActionResult> ProductPlotOrFlat(VMRealStateProduct vmCommonProduct)
        {
            if (vmCommonProduct.ActionEum == ActionEnum.Add)
            {
                //Add                

                await _service.RealStateProductAdd(vmCommonProduct);
            }
            else if (vmCommonProduct.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.RealStateProductEdit(vmCommonProduct);
            }
            else if (vmCommonProduct.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.ProductDelete(vmCommonProduct.ID);
            }
            else
            {
                return RedirectToAction("Error");
            }
            return RedirectToAction(nameof(ProductPlotOrFlat), new { companyId = vmCommonProduct.CompanyFK, productType = vmCommonProduct.ProductType });
        }

        public async Task<ActionResult> ProductPlotOrFlatEdit(int ProductId, string productType = "")
        {
            VMRealStateProduct model = new VMRealStateProduct();
            model = await _service.GetRealStateProductForEdit(ProductId);
            model.StatusList = new SelectList(_service.PlotOrPlatStatusList(model.CompanyFK.Value), "Value", "Text");

            var company = _companyService.GetCompany((int)model.CompanyFK);
            model.ActionId = (int)ActionEnum.Edit;
            model.CompanyName = company.Name;
            model.ProductType = productType;
            return View(model);
        }

        public async Task<ActionResult> ProductPlotOrFlatView(int companyId, int ProductId)
        {
            VMrealStateProductsForList model = new VMrealStateProductsForList();
            model = await _service.GetPlotOrFlatView(companyId, ProductId);
            if (companyId == 9)
            {
                var lid = model.realStateProducts.FlatProp.LandFacing;
                model.realStateProducts.LandFacingTitle = await _service.FacingName(lid);
            }

            var company = _companyService.GetCompany((int)model.CompanyId);
            model.ActionId = (int)ActionEnum.Edit;
            model.CompanyName = company.Name;

            return View(model);
        }

        //public async Task<ActionResult> ProductPlotOrFlatEdit(int companyId , int id)
        //{
        //    VMRealStateProduct vmCommonProduct = new VMRealStateProduct();
        //    vmCommonProduct =  _service.GetCommonProductByID(id);
        //    vmCommonProduct.UnitList = new SelectList(_service.UnitDropDownList(companyId), "Value", "Text");
        //    vmCommonProduct.FacingDropDown = await _service.GetFacingDropDown();
        //    return View(vmCommonProduct);
        //}

        public async Task<ActionResult> ProductPlotOrFlatCreate(int companyId, string productType = "")
        {
            VMRealStateProduct vmCommonProduct = new VMRealStateProduct();
            var company = _companyService.GetCompany(companyId);
            vmCommonProduct.CompanyFK = companyId;
            vmCommonProduct.CompanyName = company.Name;
            vmCommonProduct.ProductType = productType;
            vmCommonProduct.StatusList = new SelectList(_service.PlotOrPlatStatusList(companyId), "Value", "Text");
            vmCommonProduct.UnitList = new SelectList(_service.UnitDropDownList(companyId), "Value", "Text");
            vmCommonProduct.FacingDropDown = await _service.GetFacingDropDown();
            vmCommonProduct.GetProductCategoryList = await _service.GetProductCategory(companyId, productType);
            return View(vmCommonProduct);
        }

        #endregion

        [HttpGet]
        public async Task<ActionResult> CommonClient(int companyId)
        {

            VMCommonSupplier vmCommonCustomer = new VMCommonSupplier();
            vmCommonCustomer = await Task.Run(() => _service.GetClient(companyId));
            vmCommonCustomer.DivisionList = new SelectList(_service.CommonDivisionsDropDownList(), "Value", "Text");
            vmCommonCustomer.DistrictList = new SelectList(_service.CommonDistrictsDropDownList(), "Value", "Text");
            vmCommonCustomer.UpazilasList = new SelectList(_service.CommonUpazilasDropDownList(), "Value", "Text");
            vmCommonCustomer.PaymentTypeList = new SelectList(_service.CommonCustomerPaymentType(), "Value", "Text");
            return View(vmCommonCustomer);
        }


        [HttpPost]
        public async Task<ActionResult> CommonClient(VMCommonSupplier vmCommonCustomer)
        {

            if (vmCommonCustomer.ActionEum == ActionEnum.Add)
            {
                //Add 
                await _service.CustomerAdd(vmCommonCustomer);
            }
            else if (vmCommonCustomer.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.CustomerEdit(vmCommonCustomer);
            }
            else if (vmCommonCustomer.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.CustomerDelete(vmCommonCustomer.ID);
            }
            else
            {
                return View("Error");
            }
            if (vmCommonCustomer.CompanyFK == (int)CompanyName.KrishibidFeedLimited)
            {
                return RedirectToAction(nameof(CommonFeedCustomer), new { companyId = vmCommonCustomer.CompanyFK });
            }
            return RedirectToAction(nameof(CommonCustomer), new { companyId = vmCommonCustomer.CompanyFK });
        }

        [HttpPost]
        public async Task<ActionResult> SaveDivision(Division Model)
        {
            var vmPoliceStations = await Task.Run(() => _service.SaveDivision(Model));
            return Json(vmPoliceStations, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        [HttpGet]
        public async Task<JsonResult> GetDivisionById(int id)
        {
            var obj = await _service.GetDivisionById(id);
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> SaveUpazila(Upazila Model)
        {
            var vmPoliceStations = await Task.Run(() => _service.SaveUpazila(Model));
            return Json(vmPoliceStations, JsonRequestBehavior.AllowGet);
        }


        [SessionExpire]
        [HttpGet]
        public async Task<JsonResult> GetDistrictById(int id)
        {
            var obj = await _service.GetDistrictById(id);
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> SaveDistrict(District model)
        {
            var vmPoliceStations = await Task.Run(() => _service.SaveDistrict(model));
            return Json(vmPoliceStations, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        [HttpPost]
        public async Task<JsonResult> DeleteDistrict(int id)
        {
            var obj = await _service.DeleteDistrict(id);
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        [HttpPost]
        public async Task<JsonResult> DeleteUpazila(int id)
        {

            var obj = await _service.DeleteUpazila(id);
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        [HttpGet]
        public async Task<JsonResult> GetUpazilaById(int id)
        {
            var obj = await _service.GetUpazilaById(id);
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        #region Incentive

        public async Task<ActionResult> Incentives(int companyId)
        {

            VMIncentive vmIncentive = new VMIncentive();
            vmIncentive = await Task.Run(() => _service.GetIncentives(companyId));
            return View(vmIncentive);
        }

        [HttpPost]
        public async Task<ActionResult> Incentives(VMIncentive vmIncentive)
        {

            if (vmIncentive.ActionEum == ActionEnum.Add)
            {
                //Add 

                await _service.IncentiveAdd(vmIncentive);
            }
            else if (vmIncentive.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.IncentiveEdit(vmIncentive);
            }
            else if (vmIncentive.ActionEum == ActionEnum.Delete)
            {
                //Delete
                await _service.IncentiveDelete(vmIncentive.ID);
            }
            else
            {
                return RedirectToAction("Error");
            }
            return RedirectToAction(nameof(Incentives), new { companyId = vmIncentive.CompanyId });
        }

        public async Task<ActionResult> IncentiveDetails(int companyId, int incentiveId = 0)
        {

            VMIncentiveDetails vmIncentiveDetails = new VMIncentiveDetails();
            vmIncentiveDetails = await Task.Run(() => _service.GetIncentiveDetails(companyId, incentiveId));
            return View(vmIncentiveDetails);
        }

        [HttpPost]
        public async Task<ActionResult> IncentiveDetails(VMIncentiveDetails vmIncentiveDetails)
        {

            if (vmIncentiveDetails.ActionEum == ActionEnum.Add)
            {
                //Add                

                await _service.IncentiveDetailsAdd(vmIncentiveDetails);
            }
            else if (vmIncentiveDetails.ActionEum == ActionEnum.Edit)
            {
                //Edit
                await _service.IncentiveDetailsEdit(vmIncentiveDetails);
            }
            else if (vmIncentiveDetails.ActionEum == ActionEnum.Delete)
            {
                await _service.IncentiveDetailsDelete(vmIncentiveDetails.ID);
            }

            else
            {
                return RedirectToAction("Error");
            }
            return RedirectToAction(nameof(IncentiveDetails), new { companyId = vmIncentiveDetails.CompanyId, incentiveId = vmIncentiveDetails.IncentiveId });
        }

        #endregion

    }
}