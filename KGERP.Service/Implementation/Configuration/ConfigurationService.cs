using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Dynamic;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Web.Mvc;
using KGERP.Data.Models;
using KGERP.Service.Implementation.Accounting;
using KGERP.Service.ServiceModel;
using KGERP.Service.ServiceModel.RealState;
using KGERP.Utility;
using Newtonsoft.Json;

namespace KGERP.Service.Implementation.Configuration

{
    public class ConfigurationService
    {
        private readonly ERPEntities _db;

        public ConfigurationService(ERPEntities db)
        {
            _db = db;
        }

        #region User Role Menu Item

        #region ClientUserMenuAssignment

        public ClientMenu ClientUserMenuAssignment(ClientMenu model)
        {

            ClientMenu clientMenu = new ClientMenu()
            {
                CompanyUserMenus = _db.CompanyUserMenus.Where(x => x.UserId == model.UserId).ToList(),
            };

            //Super Admin Allowed Menus
            var baseUserMenus = _db.CompanyUserMenus.Where(x => x.UserId == CompanyInfo.CompanyAdminUserId && x.IsActive == true).ToList();

            var baseMenuIds = baseUserMenus.Select(c => c.CompanyMenuId).Distinct().ToList();
            clientMenu.CompanyMenus = _db.CompanyMenus.Where(x => baseMenuIds.Contains(x.CompanyMenuId)).ToList();
            var subMenus = clientMenu.CompanyMenus.SelectMany(c => c.CompanySubMenus).ToList();

            //Assigned Submenu
            var userSubMenuIds = clientMenu?.CompanyUserMenus?.Select(c => c.CompanySubMenuId).Distinct().ToList();
            if (userSubMenuIds?.Count() > 0)
            {
                foreach (var item in clientMenu.CompanyUserMenus)
                {
                    var subMenu = subMenus.FirstOrDefault(c => c.CompanySubMenuId == item.CompanySubMenuId);
                    ClientUserMenu data = new ClientUserMenu()
                    {
                        CompanyUserMenuId = item.CompanyUserMenuId,
                        IsActive = item.IsActive,
                        UserId = item.UserId,
                        MenuId = item.CompanyMenuId,
                        MenuName = clientMenu.CompanyMenus.FirstOrDefault(c => c.CompanyMenuId == item.CompanyMenuId)?.Name,
                        SubMenuId = item.CompanySubMenuId,
                        SubMenuName = subMenu?.Name,
                        SubMenuController = subMenu?.Controller,
                        SubMenuAction = subMenu?.Action
                    };

                    clientMenu.ClientUserMenus.Add(data);
                }
            }

            //Un Assigned Submenu
            var unAssignedSubMenus = baseUserMenus?.Count() > 0 && userSubMenuIds?.Count() > 0
                ? baseUserMenus.Where(c => !userSubMenuIds.Contains(c.CompanySubMenuId))
                : baseUserMenus;

            foreach (var item in unAssignedSubMenus)
            {
                var subMenu = subMenus.FirstOrDefault(c => c.CompanySubMenuId == item.CompanySubMenuId);
                ClientUserMenu data = new ClientUserMenu()
                {
                    CompanyUserMenuId = 0,
                    IsActive = false,
                    UserId = model.UserId,
                    MenuId = item.CompanyMenuId,
                    MenuName = clientMenu.CompanyMenus.FirstOrDefault(c => c.CompanyMenuId == item.CompanyMenuId)?.Name,
                    SubMenuId = item.CompanySubMenuId,
                    SubMenuName = subMenu?.Name,
                    SubMenuController = subMenu?.Controller,
                    SubMenuAction = subMenu?.Action
                };

                clientMenu.ClientUserMenus.Add(data);
            }


            return clientMenu;
        }

        public object ClientCompanyUserMenuUpdate(int index, string userId, bool isActive, int companyId, int menuId, int subMenuId)
        {

            var existPermission = _db.CompanyUserMenus.FirstOrDefault(c => c.UserId == userId && c.CompanyId == companyId && c.CompanyMenuId == menuId && c.CompanySubMenuId == subMenuId);
            if (existPermission?.CompanyUserMenuId > 0)
            {
                existPermission.IsActive = isActive;
            }
            else
            {
                CompanyUserMenu userMenu = new CompanyUserMenu
                {
                    CompanyMenuId = menuId,
                    CompanySubMenuId = subMenuId,
                    IsActive = true,
                    IsView = true,
                    CompanyId = companyId,
                    UserId = userId,
                    CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                    CreatedDate = DateTime.Now
                };
                _db.CompanyUserMenus.Add(userMenu);
            }

            if (_db.SaveChanges() > 0)
            {
                return new { indexNo = index, isSuccess = isActive };
            }
            return new { indexNo = index, isSuccess = false };

        }
        public object GetUserClientMenuAssign(string prefix)
        {
            var v = (from t1 in _db.Users.Where(q => q.Active)
                     join t2 in _db.Employees on t1.UserName equals t2.EmployeeId into t2_Join
                     from t2 in t2_Join.DefaultIfEmpty()
                     where (t1.UserName.Contains(prefix) || t1.Email.Contains(prefix))

                     select new
                     {
                         label = (t1.UserName + " ( " + t2.Name + " )"),
                         val = t1.UserName
                     }).OrderBy(x => x.label).Take(100).ToList();
            var result = v.Where(c => c.val != CompanyInfo.ProjectAdminUserId && c.val != CompanyInfo.CompanyAdminUserId);
            return result;
        }
        #endregion

        public async Task<UserDataAccessModel> GetUserDataAccessModelByEmployeeId(long id)
        {
            var model = new UserDataAccessModel();

            if (id <= 0) { return model; }

            Employee employee = await _db.Employees.FirstOrDefaultAsync(c => c.Id == id);
            User user = await _db.Users.FirstOrDefaultAsync(c => c.UserName == employee.EmployeeId);
            model.EmployeeId = id;
            model.EmployeeName = employee.Name;
            model.UserId = user.UserName;
            model.UserTypeId = user.UserTypeId ?? 0;

            List<EmployeeServicePointMap> employeeServicePointMaps = _db.EmployeeServicePointMaps.Where(c => c.EmployeeId == id && c.IsActive)
                    .Include(c => c.SubZone).Include(c => c.Area).Include(c => c.Region).Include(c => c.ZoneDivision).Include(c => c.Zone).AsNoTracking().ToList();

            var territories = employeeServicePointMaps.Where(c => c.TerritoryId > 0).Select(s => s.SubZone).ToList();
            var areas = employeeServicePointMaps.Where(c => c.AreaId > 0).Select(s => s.Area).ToList();
            var regions = employeeServicePointMaps.Where(c => c.RegionId > 0).Select(s => s.Region).ToList();
            var zoneDivisions = employeeServicePointMaps.Where(c => c.ZoneDivisionId > 0).Select(s => s.ZoneDivision).ToList();
            var zones = employeeServicePointMaps.Where(c => c.ZoneId > 0).Select(s => s.Zone).ToList();

            if (territories?.Count() > 0)
            {
                var subZoneIds = territories.Select(c => c.SubZoneId).ToArray();
                model.SubZoneIds = subZoneIds;

                var areaId = territories.FirstOrDefault().AreaId ?? 0;
                model.AreaIds = new int[] { areaId };

                var regionId = territories.FirstOrDefault().RegionId ?? 0;
                model.RegionIds = new int[] { regionId };

                var zoneDivisionId = territories.FirstOrDefault().ZoneDivisionId ?? 0;
                model.ZoneDivisionIds = new int[] { zoneDivisionId };

                var zoneId = territories.FirstOrDefault().ZoneId;
                model.ZoneIds = new int[] { zoneId };
            }
            else if (areas?.Count() > 0)
            {
                var areaIds = areas.Select(c => c.AreaId).ToArray();
                model.AreaIds = areaIds;

                var regionId = areas.FirstOrDefault().RegionId ?? 0;
                model.RegionIds = new int[] { regionId };

                var zoneDivisionId = areas.FirstOrDefault().ZoneDivisionId;
                model.ZoneDivisionIds = new int[] { (int)zoneDivisionId };

                var zoneId = areas.FirstOrDefault().ZoneId;
                model.ZoneIds = new int[] { (int)zoneId };
            }
            else if (regions?.Count() > 0)
            {
                var regionIds = regions.Select(c => c.RegionId).ToArray();
                model.RegionIds = regionIds;

                var zoneDivisionId = regions.FirstOrDefault().ZoneDivisionId;
                model.ZoneDivisionIds = new int[] { (int)zoneDivisionId };

                var zoneId = regions.FirstOrDefault().ZoneId;
                model.ZoneIds = new int[] { (int)zoneId };
            }
            else if (zoneDivisions?.Count() > 0)
            {
                var zoneDivisionIds = zoneDivisions.Select(c => c.ZoneDivisionId).ToArray();
                model.ZoneDivisionIds = zoneDivisionIds;

                var zoneId = zoneDivisions.FirstOrDefault().ZoneId;
                model.ZoneIds = new int[] { zoneId };
            }
            else if (zones?.Count() > 0)
            {
                var zoneIds = zones.Select(c => c.ZoneId).ToArray();
                model.ZoneIds = zoneIds;
            }

            if (model.UserTypeId == (int)EnumUserType.Deport)
            {
                var deports = _db.Vendors.Where(c => c.EmployeeId == model.UserId).Select(s => s.VendorId).ToArray();
                model.DeportIds = deports;
            }

            if (model.UserTypeId == (int)EnumUserType.Dealer)
            {
                var dealers = _db.Vendors.Where(c => c.EmployeeId == model.UserId).Select(s => s.VendorId).ToArray();
                model.DealerIds = dealers;
            }

            return model;
        }

        public async Task<VMUserMenuAssignment> UserMenuAssignmentGet(VMUserMenuAssignment vmUserMenuAssignment)
        {
            VMUserMenuAssignment vmMenuAssignment = new VMUserMenuAssignment();
            vmMenuAssignment.CompanyFK = vmUserMenuAssignment.CompanyFK;
            var companySubMenus = await _db.CompanySubMenus.Where(x => x.CompanyId == vmUserMenuAssignment.CompanyFK && x.IsActive == true).ToListAsync();
            var companySubMenusId = companySubMenus.Select(x => x.CompanySubMenuId).ToList();

            var companyUserMenus = await _db.CompanyUserMenus.Where(x => x.CompanyId == vmUserMenuAssignment.CompanyFK && x.UserId == vmUserMenuAssignment.UserId).ToListAsync();
            var companyUserMenus_SubMenuId = companyUserMenus.Select(x => x.CompanySubMenuId).ToList();

            var companySubMenusNotExistsOnUserMenus = companySubMenusId.Where(companySubMenuId => !companyUserMenus_SubMenuId.Contains(companySubMenuId)).ToList();

            var filteredCompanySubMenus = companySubMenus.Where(x => companySubMenusNotExistsOnUserMenus.Contains(x.CompanySubMenuId)).ToList();
            if (filteredCompanySubMenus.Any())
            {
                List<CompanyUserMenu> userMenuList = new List<CompanyUserMenu>();
                foreach (var subMenus in filteredCompanySubMenus)
                {
                    CompanyUserMenu userMenu = new CompanyUserMenu
                    {
                        CompanyMenuId = subMenus.CompanyMenuId.Value,
                        CompanySubMenuId = subMenus.CompanySubMenuId,
                        IsActive = false,
                        IsView = true,
                        CompanyId = vmUserMenuAssignment.CompanyFK,
                        UserId = vmUserMenuAssignment.UserId,
                        CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                        CreatedDate = DateTime.Now
                    };

                    userMenuList.Add(userMenu);
                }

                _db.CompanyUserMenus.AddRange(userMenuList);
                try
                {
                    await _db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    var x = ex.Message;
                }

            }
            vmMenuAssignment.DataList = await Task.Run(() => CompanyUserMenuDataLoad(vmUserMenuAssignment));
            vmMenuAssignment.CompanyFK = vmUserMenuAssignment.CompanyFK;
            vmMenuAssignment.UserId = vmUserMenuAssignment.UserId;
            vmMenuAssignment.CompanyList = new SelectList(CompaniesDropDownList(), "Value", "Text");

            return vmMenuAssignment;
        }

        public IEnumerable<VMUserMenuAssignment> CompanyUserMenuDataLoad(VMUserMenuAssignment vmMenuAssignment)
        {
            var v = (from t1 in _db.CompanyUserMenus
                     join t2 in _db.CompanySubMenus on t1.CompanySubMenuId equals t2.CompanySubMenuId
                     join t3 in _db.CompanyMenus on t1.CompanyMenuId equals t3.CompanyMenuId
                     join t4 in _db.Companies on t2.CompanyId equals t4.CompanyId
                     where t1.UserId == vmMenuAssignment.UserId && t1.CompanyId == vmMenuAssignment.CompanyFK
                     select new VMUserMenuAssignment
                     {
                         CompanyName = t4.Name,
                         MenuName = t3.Name,
                         SubmenuName = t2.Name,
                         Method = t2.Action + "/" + t2.Controller,

                         SubMenuID = t2.CompanySubMenuId,
                         IsActive = t1.IsActive,
                         MenuPriority = t2.OrderNo,


                         MenuID = t3.CompanyMenuId,
                         CompanyUserMenusId = t1.CompanyUserMenuId,
                         UserId = t1.UserId,
                         CompanyFK = t1.CompanyId,
                     }).OrderBy(x => x.MenuPriority).AsEnumerable();
            return v;
        }

        public CompanyUserMenu CompanyUserMenuEdit(VMUserMenuAssignment vmUserMenuAssignment)
        {
            long result = -1;
            //to select Accounting_Chart_Two data.....
            CompanyUserMenu companyUserMenus = _db.CompanyUserMenus.Find(vmUserMenuAssignment.CompanyUserMenusId);
            companyUserMenus.IsActive = vmUserMenuAssignment.IsActive;
            companyUserMenus.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            companyUserMenus.ModifiedDate = DateTime.Now;

            if (_db.SaveChanges() > 0)
            {
                result = companyUserMenus.CompanyUserMenuId;
            }
            return companyUserMenus;
        }

        #endregion

        public async Task<VMUserMenu> AccountingCostCenterGet(int companyId)
        {
            VMUserMenu vmUserMenu = new VMUserMenu();
            vmUserMenu.CompanyFK = companyId;
            vmUserMenu.DataList = (from t1 in _db.Accounting_CostCenter
                                   join t2 in _db.Companies on t1.CompanyId equals t2.CompanyId
                                   where t1.CompanyId == companyId && t1.IsActive == true
                                   select new VMUserMenu
                                   {
                                       ID = t1.CostCenterId,
                                       Name = t1.Name,
                                       CompanyName = t2.Name,
                                       CompanyFK = t1.CompanyId
                                   }).OrderByDescending(x => x.ID).AsEnumerable();
            return vmUserMenu;
        }

        public async Task<int> AccountingCostCenterAdd(VMUserMenu vmUserMenu)
        {
            var result = -1;


            Accounting_CostCenter costCenter = new Accounting_CostCenter
            {

                Name = vmUserMenu.Name,

                CompanyId = vmUserMenu.CompanyFK.Value,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _db.Accounting_CostCenter.Add(costCenter);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = costCenter.CostCenterId;
            }
            return result;
        }

        public async Task<int> AccountingCostCenterEdit(VMUserMenu vmUserMenu)
        {
            var result = -1;
            Accounting_CostCenter costCenter = _db.Accounting_CostCenter.Find(vmUserMenu.ID);
            costCenter.Name = vmUserMenu.Name;

            costCenter.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;


            if (await _db.SaveChangesAsync() > 0)
            {
                result = costCenter.CostCenterId;
            }
            return result;
        }

        public async Task<int> AccountingCostCenterDelete(int id)
        {
            int result = -1;
            if (id != 0)
            {
                Accounting_CostCenter costCenter = await _db.Accounting_CostCenter.FindAsync(id);
                costCenter.IsActive = false;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = costCenter.CostCenterId;
                }
            }
            return result;
        }

        #region User Menu
        public async Task<VMUserMenu> UserMenuGet()
        {
            VMUserMenu vmUserMenu = new VMUserMenu();
            vmUserMenu.DataList = await Task.Run(() => UserMenuDataLoad());
            return vmUserMenu;
        }

        public IEnumerable<VMUserMenu> UserMenuDataLoad()
        {
            var v = (from t1 in _db.CompanyMenus
                     join t2 in _db.Companies on t1.CompanyId equals t2.CompanyId
                     where t1.IsActive == true
                     select new VMUserMenu
                     {
                         ID = t1.CompanyMenuId,
                         Name = t1.Name,
                         CompanyName = t2.Name,
                         LayerNo = t1.LayerNo,
                         ShortName = t1.ShortName,
                         Priority = t1.OrderNo,
                         IsActive = t1.IsActive,
                         CompanyFK = t1.CompanyId
                     }).OrderByDescending(x => x.ID).AsEnumerable();
            return v;
        }

        public async Task<int> UserMenuAdd(VMUserMenu vmUserMenu)
        {
            var result = -1;


            CompanyMenu userMenu = new CompanyMenu
            {
                CompanyMenuId = _db.Database.SqlQuery<int>("exec spGetNewCompanyId").FirstOrDefault(),
                Name = vmUserMenu.Name,
                OrderNo = vmUserMenu.Priority,
                LayerNo = vmUserMenu.LayerNo,
                ShortName = vmUserMenu.Name,

                CompanyId = vmUserMenu.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _db.CompanyMenus.Add(userMenu);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = userMenu.CompanyMenuId;
            }
            return result;
        }

        public async Task<int> UserMenuEdit(VMUserMenu vmUserMenu)
        {
            var result = -1;
            using (DbContextTransaction dbTran = _db.Database.BeginTransaction())
            {
                try
                {
                    CompanyMenu userMenu = _db.CompanyMenus.Find(vmUserMenu.ID);
                    var CompanyUserMenuList = await _db.CompanyUserMenus
                       .Where(e => e.CompanyMenuId == vmUserMenu.ID
                       && e.CompanyId == userMenu.CompanyId && e.IsActive == true).ToListAsync();
                    CompanyUserMenuList.ForEach(e => e.CompanyId = vmUserMenu.CompanyFK);

                    userMenu.Name = vmUserMenu.Name;
                    userMenu.CompanyId = vmUserMenu.CompanyFK;

                    userMenu.OrderNo = vmUserMenu.Priority;
                    userMenu.LayerNo = vmUserMenu.LayerNo;
                    userMenu.ShortName = vmUserMenu.ShortName;
                    userMenu.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                    userMenu.ModifiedDate = DateTime.Now;
                    await _db.SaveChangesAsync();
                    result = userMenu.CompanyMenuId;
                    dbTran.Commit();
                }
                catch (DbEntityValidationException)
                {
                    dbTran.Rollback();
                    //throw;
                }
            }
            return result;
        }

        public async Task<int> UserMenuDelete(int id)
        {
            int result = -1;

            if (id != 0)
            {
                using (DbContextTransaction dbTran = _db.Database.BeginTransaction())
                {
                    try
                    {
                        CompanyMenu userMenu = _db.CompanyMenus.Find(id);
                        var CompanyUserMenuList = await _db.CompanyUserMenus
                           .Where(e => e.CompanyMenuId == userMenu.CompanyMenuId
                           && e.CompanyId == userMenu.CompanyId && e.IsActive == true).ToListAsync();
                        CompanyUserMenuList.ForEach(e => e.IsActive = false);

                        userMenu.IsActive = false;
                        userMenu.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                        userMenu.ModifiedDate = DateTime.Now;
                        await _db.SaveChangesAsync();
                        result = userMenu.CompanyMenuId;
                        dbTran.Commit();
                    }
                    catch (DbEntityValidationException)
                    {
                        dbTran.Rollback();
                        //throw;
                    }
                }
            }
            return result;
        }
        #endregion

        #region User Submenu
        public async Task<VMUserSubMenu> UserSubMenuGet()
        {
            VMUserSubMenu vmUserSubMenu = new VMUserSubMenu();

            vmUserSubMenu.DataList = await Task.Run(() => UserSubMenuDataLoad());

            return vmUserSubMenu;
        }

        public IEnumerable<VMUserSubMenu> UserSubMenuDataLoad()
        {
            var v = (from t1 in _db.CompanySubMenus
                     join t2 in _db.CompanyMenus on t1.CompanyMenuId equals t2.CompanyMenuId
                     join t3 in _db.Companies on t2.CompanyId equals t3.CompanyId

                     where t1.IsActive == true
                     select new VMUserSubMenu
                     {
                         CompanyName = t3.Name,
                         ID = t1.CompanySubMenuId,
                         Name = t1.Name,
                         Param = t1.Param,
                         CompanyFK = t1.CompanyId,
                         Controller = t1.Controller,
                         IsActive = t1.IsActive,
                         LayerNo = t1.LayerNo,
                         ShortName = t1.ShortName,
                         Action = t1.Action,
                         UserMenuName = t2.Name,
                         User_MenuFk = t2.CompanyMenuId,
                         Priority = t1.OrderNo

                     }).OrderByDescending(x => x.ID).AsEnumerable();
            return v;
        }

        public async Task<int> UserSubMenuAdd(VMUserSubMenu vmUserSubMenu)
        {
            var result = -1;

            var objectToSave = await _db.CompanySubMenus
                .SingleOrDefaultAsync(q => q.Name == vmUserSubMenu.Name
                && q.CompanyId == vmUserSubMenu.CompanyFK
                && q.CompanyMenuId == vmUserSubMenu.User_MenuFk
                );

            if (objectToSave != null)
            {

                return result = objectToSave.CompanySubMenuId;
            }


            CompanySubMenu userSubMenu = new CompanySubMenu
            {
                CompanySubMenuId = _db.Database.SqlQuery<int>("exec spGetNewCompanyId").FirstOrDefault(),
                Name = vmUserSubMenu.Name,
                CompanyId = vmUserSubMenu.CompanyFK,
                CompanyMenuId = vmUserSubMenu.User_MenuFk,
                OrderNo = vmUserSubMenu.Priority,
                Controller = vmUserSubMenu.Controller,
                Action = vmUserSubMenu.Action,
                LayerNo = vmUserSubMenu.LayerNo,
                IsActive = true,
                IsSideMenu = true,
                ShortName = vmUserSubMenu.ShortName,
                Param = vmUserSubMenu.Param,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now



            };
            _db.CompanySubMenus.Add(userSubMenu);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = userSubMenu.CompanySubMenuId;
            }
            return result;
        }

        public async Task<int> UserSubMenuEdit(VMUserSubMenu vmUserSubMenu)
        {
            var result = -1;
            using (DbContextTransaction dbTran = _db.Database.BeginTransaction())
            {
                try
                {
                    CompanySubMenu userSubMenu = _db.CompanySubMenus.Find(vmUserSubMenu.ID);

                    var CompanyUserMenuList = await _db.CompanyUserMenus
                        .Where(e => e.CompanyMenuId == userSubMenu.CompanyMenuId
                        && e.CompanySubMenuId == userSubMenu.CompanySubMenuId).ToListAsync();
                    CompanyUserMenuList.ForEach(e => e.CompanyMenuId = vmUserSubMenu.User_MenuFk);

                    userSubMenu.CompanyMenuId = vmUserSubMenu.User_MenuFk;
                    userSubMenu.Name = vmUserSubMenu.Name;
                    userSubMenu.OrderNo = vmUserSubMenu.Priority;
                    userSubMenu.Controller = vmUserSubMenu.Controller;
                    userSubMenu.Action = vmUserSubMenu.Action;
                    userSubMenu.LayerNo = vmUserSubMenu.LayerNo;
                    userSubMenu.ShortName = vmUserSubMenu.ShortName;
                    userSubMenu.Param = vmUserSubMenu.Param;
                    userSubMenu.CompanyId = vmUserSubMenu.CompanyFK;
                    userSubMenu.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                    userSubMenu.ModifiedDate = DateTime.Now;
                    _db.Entry(userSubMenu).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                    result = userSubMenu.CompanySubMenuId;



                    dbTran.Commit();
                }
                catch (DbEntityValidationException ex)
                {
                    dbTran.Rollback();
                    //throw;
                }
            }
            return result;
        }

        public async Task<int> UserSubMenuDelete(int id)
        {
            int result = -1;

            if (id != 0)
            {
                using (DbContextTransaction dbTran = _db.Database.BeginTransaction())
                {
                    try
                    {
                        CompanySubMenu userSubMenu = _db.CompanySubMenus.Find(id);

                        var CompanyUserMenuList = await _db.CompanyUserMenus
                            .Where(e => e.CompanyMenuId == userSubMenu.CompanyMenuId
                            && e.CompanySubMenuId == userSubMenu.CompanySubMenuId && e.IsActive == true).ToListAsync();
                        CompanyUserMenuList.ForEach(e => e.IsActive = false);

                        userSubMenu.IsActive = false;
                        userSubMenu.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                        userSubMenu.ModifiedDate = DateTime.Now;
                        _db.Entry(userSubMenu).State = EntityState.Modified;
                        result = await _db.SaveChangesAsync();
                        dbTran.Commit();
                    }
                    catch (DbEntityValidationException ex)
                    {
                        dbTran.Rollback();
                        //throw;
                    }
                }
            }
            return result;
        }
        public CompanyModel UserAssignedMenuGet()
        {
            string userId = System.Web.HttpContext.Current.User.Identity.Name;
            CompanyModel companyModel = new CompanyModel();
            companyModel.DataList = (from t1 in _db.CompanyUserMenus
                                     join t2 in _db.CompanySubMenus on t1.CompanySubMenuId equals t2.CompanySubMenuId
                                     join t3 in _db.CompanyMenus on t2.CompanyMenuId equals t3.CompanyMenuId
                                     where t1.IsActive == true && t1.UserId == userId
                                     select new CompanyModel
                                     {
                                         Id = t2.CompanySubMenuId,
                                         CompanyId = t2.CompanyId.Value,
                                         ParentId = t3.CompanyMenuId,
                                         Name = t2.Name,
                                         ShortName = t2.ShortName



                                     }).OrderByDescending(x => x.Id).AsEnumerable();
            return companyModel;
        }
        #endregion

        #region Common Unit
        public async Task<VMCommonUnit> GetUnit(int companyId)
        {
            VMCommonUnit vmCommonUnit = new VMCommonUnit();
            vmCommonUnit.CompanyFK = companyId;
            vmCommonUnit.DataList = await Task.Run(() => (from t1 in _db.Units
                                                          where t1.IsActive == true
                                                          && t1.CompanyId == companyId
                                                          select new VMCommonUnit
                                                          {
                                                              ID = t1.UnitId,
                                                              Name = t1.Name,
                                                              CompanyFK = t1.CompanyId,
                                                              CreatedBy = t1.CreatedBy

                                                          }).OrderByDescending(x => x.ID).AsEnumerable());
            return vmCommonUnit;
        }
        public async Task<VMCommonUnit> GetSingleCommonUnit(int id)
        {

            var v = await Task.Run(() => (from t1 in _db.Units
                                          where t1.UnitId == id && t1.IsActive == true
                                          select new VMCommonUnit
                                          {
                                              ID = t1.UnitId,
                                              Name = t1.Name,
                                              CompanyFK = t1.CompanyId
                                          }).FirstOrDefault());
            return v;
        }
        public async Task<int> UnitAdd(VMCommonUnit vmCommonUnit)
        {
            var result = -1;

            #region IsExist
            var isExist = await _db.Units.FirstOrDefaultAsync(u => u.Name.Equals(vmCommonUnit.Name) && u.IsActive == true);
            if (isExist?.UnitId > 0)
            {
                throw new Exception($"Sorry! This Name {vmCommonUnit.Name} already Exists!");
            }
            #endregion

            Unit commonUnit = new Unit
            {
                Name = vmCommonUnit.Name,
                CompanyId = vmCommonUnit.CompanyFK,
                ShortName = vmCommonUnit.Name,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _db.Units.Add(commonUnit);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = commonUnit.UnitId;
            }
            return result;
        }

        public async Task<int> UnitEdit(VMCommonUnit vmCommonUnit)
        {
            var result = -1;

            #region IsExist
            var isExist = await _db.Units.FirstOrDefaultAsync(u => u.Name.Equals(vmCommonUnit.Name) && u.UnitId != vmCommonUnit.ID && u.IsActive == true);
            if (isExist?.UnitId > 0)
            {
                throw new Exception($"Sorry! This Name {vmCommonUnit.Name} already Exists!");
            }
            #endregion

            Unit commonUnit = await _db.Units.FindAsync(vmCommonUnit.ID);
            commonUnit.Name = vmCommonUnit.Name;
            commonUnit.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            commonUnit.ModifiedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = commonUnit.UnitId;
            }
            return result;
        }
        public async Task<int> UnitDelete(int id)
        {
            int result = -1;
            if (id != 0)
            {
                Unit commonUnit = await _db.Units.FindAsync(id);
                commonUnit.IsActive = false;
                commonUnit.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                commonUnit.ModifiedDate = DateTime.Now;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = commonUnit.UnitId;
                }
            }
            return result;
        }

        public async Task<bool> CheckDuplicateUnitName(string name, int id)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            bool isExist = false;
            if (id > 0)
            {
                isExist = await _db.Units.AnyAsync(u => u.Name.Equals(name) && u.UnitId != id && u.IsActive == true);
            }
            else
            {
                isExist = await _db.Units.AnyAsync(u => u.Name.Equals(name) && u.IsActive == true);
            }
            return isExist;
        }
        #endregion

        #region Common DamageType
        public object GetAutoCompleteDamageType(int companyId, string prefix)
        {
            var v = (from t1 in _db.DamageTypes

                     where t1.IsActive == true && t1.CompanyId == companyId
                     && ((t1.Name.StartsWith(prefix)))

                     select new
                     {
                         label = t1.Name ?? "",
                         val = t1.DamageTypeId
                     }).OrderBy(x => x.label).Take(10).ToList();
            return v;
        }

        public List<object> DamageTypeDropDownList(int companyId = 0)
        {
            var list = new List<object>();
            _db.DamageTypes.Where(x => x.IsActive && x.CompanyId == companyId).Select(x => x).ToList()
            .ForEach(x => list.Add(new
            {
                Value = x.DamageTypeId,
                Text = x.Name
            }));
            return list;

        }

        public async Task<VMCommonDamageType> GetDamageType(int companyId)
        {
            VMCommonDamageType vmCommonDamageType = new VMCommonDamageType();
            vmCommonDamageType.CompanyFK = companyId;
            vmCommonDamageType.DataList = await Task.Run(() => (from t1 in _db.DamageTypes
                                                                where t1.IsActive == true
                                                                && t1.CompanyId == companyId
                                                                select new VMCommonDamageType
                                                                {
                                                                    ID = t1.DamageTypeId,
                                                                    Name = t1.Name,
                                                                    DamageTypeForId = t1.DamageTypeForId,
                                                                    CompanyFK = t1.CompanyId,
                                                                    CreatedBy = t1.CreatedBy

                                                                }).OrderByDescending(x => x.ID).AsEnumerable());
            return vmCommonDamageType;
        }
        public async Task<VMCommonDamageType> GetSingleCommonDamageType(int id)
        {

            var v = await Task.Run(() => (from t1 in _db.DamageTypes
                                          where t1.DamageTypeId == id && t1.IsActive == true
                                          select new VMCommonDamageType
                                          {
                                              ID = t1.DamageTypeId,
                                              Name = t1.Name,
                                              DamageTypeForId = t1.DamageTypeForId,
                                              CompanyFK = t1.CompanyId
                                          }).FirstOrDefault());
            return v;
        }
        public async Task<int> DamageTypeAdd(VMCommonDamageType vmCommonDamageType)
        {
            var result = -1;

            #region IsExist
            var isExist = await _db.DamageTypes.FirstOrDefaultAsync(u => u.Name.Equals(vmCommonDamageType.Name) && u.DamageTypeForId == vmCommonDamageType.DamageTypeForId && u.IsActive == true);
            if (isExist?.DamageTypeId > 0)
            {
                throw new Exception($"Sorry! This Name {vmCommonDamageType.Name} already Exists!");
            }
            #endregion

            DamageType commonDamageType = new DamageType
            {
                Name = vmCommonDamageType.Name,
                CompanyId = vmCommonDamageType.CompanyFK,
                DamageTypeForId = (int)vmCommonDamageType.DamageTypeForId,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _db.DamageTypes.Add(commonDamageType);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = commonDamageType.DamageTypeId;
            }
            return result;
        }
        public async Task<int> DamageTypeEdit(VMCommonDamageType vmCommonDamageType)
        {
            var result = -1;

            #region IsExist
            var isExist = await _db.DamageTypes.FirstOrDefaultAsync(u => u.Name.Equals(vmCommonDamageType.Name) && u.DamageTypeForId == vmCommonDamageType.DamageTypeForId && u.DamageTypeId != vmCommonDamageType.ID && u.IsActive == true);
            if (isExist?.DamageTypeId > 0)
            {
                throw new Exception($"Sorry! This Name {vmCommonDamageType.Name} already Exists!");
            }
            #endregion

            DamageType commonDamageType = await _db.DamageTypes.FindAsync(vmCommonDamageType.ID);
            commonDamageType.Name = vmCommonDamageType.Name;
            commonDamageType.DamageTypeForId = (int)vmCommonDamageType.DamageTypeForId;
            commonDamageType.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            commonDamageType.ModifiedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = commonDamageType.DamageTypeId;
            }
            return result;
        }
        public async Task<int> DamageTypeDelete(int id)
        {
            int result = -1;
            if (id != 0)
            {
                DamageType commonDamageType = await _db.DamageTypes.FindAsync(id);
                commonDamageType.IsActive = false;
                commonDamageType.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                commonDamageType.ModifiedDate = DateTime.Now;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = commonDamageType.DamageTypeId;
                }
            }
            return result;
        }

        public async Task<bool> CheckDamageTypeName(string name, int damageTypeForId, int id)
        {
            if (string.IsNullOrEmpty(name) && damageTypeForId == 0)
            {
                return false;
            }
            bool isExist = false;
            if (id > 0)
            {
                isExist = await _db.DamageTypes.AnyAsync(u => u.Name.Equals(name) && u.DamageTypeForId == damageTypeForId && u.DamageTypeId != id && u.IsActive == true);
            }
            else
            {
                isExist = await _db.DamageTypes.AnyAsync(u => u.Name.Equals(name) && u.DamageTypeForId == damageTypeForId && u.IsActive == true);
            }
            return isExist;
        }

        #endregion

        public object GetAutoCompleteSupplier(int companyId, string prefix)
        {
            var v = (from t1 in _db.Vendors
                     where t1.CompanyId == companyId
                     && t1.IsActive == true
                     && t1.VendorTypeId == (int)Provider.Supplier
                     && ((t1.Name.StartsWith(prefix)))

                     select new
                     {
                         label = t1.Name ?? "",
                         val = t1.VendorId
                     }).OrderBy(x => x.label).Take(10).ToList();

            return v;
        }
        public object GetAutoCompleteSupplier(string prefix)
        {
            var v = (from t1 in _db.Vendors

                     where t1.IsActive == true
                     && t1.VendorTypeId == (int)Provider.Supplier
                     && ((t1.Name.StartsWith(prefix)))

                     select new
                     {
                         label = t1.Name ?? "",
                         val = t1.VendorId
                     }).OrderBy(x => x.label).Take(10).ToList();

            return v;
        }
        public object GetAutoCompleteProductCategory(int companyId, string prefix, string productType)
        {
            var v = (from t1 in _db.ProductCategories

                     where t1.CompanyId == companyId
                     && t1.IsActive == true
                     && (productType == "" ? t1.ProductType != productType : t1.ProductType == productType)
                     && t1.Name.StartsWith(prefix)

                     select new
                     {
                         label = t1.Name ?? "",
                         val = t1.ProductCategoryId
                     }).OrderBy(x => x.label).Take(10).ToList();

            return v;
        }

        public async Task<List<SelectModelType>> GetProductCategory(int companyId, string productType)
        {

            var result = await _db.ProductCategories.AsNoTracking()
                       .Where(c => c.IsActive == true && c.CompanyId == companyId/* && productType == productType*/)
                       .Select(c => new SelectModelType
                       {
                           Text = c.Name,
                           Value = c.ProductCategoryId
                       }).ToListAsync();

            return result;
        }

        public List<SelectModel> GetProductCategory(int companyId)
        {
            List<SelectModel> selectModelList = new List<SelectModel>();
            SelectModel selectModel = new SelectModel
            {
                Text = "==Select Product Category==",
                Value = 0,
            };

            selectModelList.Add(selectModel);

            var v = _db.ProductCategories.Where(x => x.CompanyId == companyId && x.IsActive == true).ToList()
                .Select(x => new SelectModel()
                {
                    Text = x.Name,
                    Value = x.ProductCategoryId
                }).ToList();

            selectModelList.AddRange(v);
            return selectModelList;
        }

        public object GetAutoCompleteProduct(int companyId, string prefix, string productType)
        {

            var v = (from t1 in _db.Products
                     join t2 in _db.ProductSubCategories on t1.ProductSubCategoryId equals t2.ProductSubCategoryId
                     join t3 in _db.ProductCategories on t2.ProductCategoryId equals t3.ProductCategoryId
                     join t4 in _db.Units on t1.UnitId equals t4.UnitId

                     where t1.CompanyId == companyId && t1.IsActive == true && t1.ProductType == productType &&
                     ((t1.ProductName.StartsWith(prefix)) || (t2.Name.StartsWith(prefix)) || (t3.Name.StartsWith(prefix)) || (t1.ShortName.StartsWith(prefix)))

                     select new
                     {
                         label = t3.Name + " " + t2.Name + " " + t1.ProductName,
                         val = t1.ProductId,
                         unit = t4.Name,
                         price = t1.UnitPrice ?? 0,
                         tpPrice = t1.TPPrice,
                         qty = t1.Qty ?? 0
                     }).OrderBy(x => x.label).Take(20).ToList();

            return v;
        }

        public object AutoCompleteAdministrativeExpenseHeadGlGet(int companyId, string prefix)
        {

            var v = (from t1 in _db.HeadGLs
                     join t2 in _db.Head5 on t1.ParentId equals t2.Id
                     join t3 in _db.Head4 on t2.ParentId equals t3.Id
                     join t4 in _db.Head3 on t3.ParentId equals t4.Id

                     where t1.CompanyId == companyId && t1.IsActive == true && t1.AccCode.StartsWith("440") &&
                     ((t1.AccName.StartsWith(prefix)) || (t2.AccName.StartsWith(prefix)) || (t3.AccName.StartsWith(prefix)) || (t1.AccCode.StartsWith(prefix)))

                     select new
                     {
                         label = t2.AccCode + " " + t1.AccName,
                         val = t1.Id,
                         code = t4.AccCode
                     }).OrderBy(x => x.label).Take(20).ToList();

            return v;
        }

        public object GCCLGetAutoCompleteRawPackingMaterials(int companyId, string prefix)
        {
            var v = (from t1 in _db.Products
                     join t2 in _db.ProductSubCategories on t1.ProductSubCategoryId equals t2.ProductSubCategoryId
                     join t3 in _db.ProductCategories on t2.ProductCategoryId equals t3.ProductCategoryId

                     where t1.CompanyId == companyId && t1.IsActive == true && t2.IsActive == true && t3.IsActive == true &&
                     //((t1.ProductType == "R") || (t1.ProductType == "P")) &&
                     ((t1.ProductName.Contains(prefix)) || (t2.Name.Contains(prefix)) || (t3.Name.Contains(prefix)) || (t1.ShortName.Contains(prefix)))

                     select new
                     {
                         label = ((t1.ProductType == "R") ? "Raw Materials: " : (t1.ProductType == "P") ? "Packaging Materials: " : (t1.ProductType == "F") ? "Finished Goods: " : "") + t3.Name + " " + t2.Name + " " + t1.ProductName,
                         val = t1.ProductId
                     }).OrderBy(x => x.label).Take(100).ToList();

            return v;
        }


        public object AllEmployee(string prefix)
        {
            var v = (from t1 in _db.Employees
                     join t2 in _db.Designations on t1.DesignationId equals t2.DesignationId into t2_Join
                     from t2 in t2_Join.DefaultIfEmpty()
                     where (t1.EmployeeId.Contains(prefix) || t1.Name.Contains(prefix) || t1.ShortName.Contains(prefix) || t2.Name.Contains(prefix))

                     select new
                     {
                         label = (t1.Name + "-" + t2.Name + "( " + t1.EmployeeId + " )"),
                         val = t1.EmployeeId
                     }).OrderBy(x => x.label).Take(100).ToList();

            return v;
        }

        public object AllEmployeeForMenu(string prefix)
        {
            var v = (from t1 in _db.Employees.Where(q => q.Active)
                     join t2 in _db.Designations on t1.DesignationId equals t2.DesignationId into t2_Join
                     from t2 in t2_Join.DefaultIfEmpty()
                     where (t1.EmployeeId.Contains(prefix) || t1.Name.Contains(prefix) || t1.ShortName.Contains(prefix) || t2.Name.Contains(prefix))

                     select new
                     {
                         label = (t1.Name + "-" + t2.Name + "( " + t1.EmployeeId + " )"),
                         val = t1.EmployeeId
                     }).OrderBy(x => x.label).Take(100).ToList();

            return v;
        }

        public List<SelectModel> GetEmployeeSelectModels(int companyId)
        {
            return _db.Employees.Where(x => x.CompanyId == companyId && x.Active == true).ToList().Select(x => new SelectModel()
            {
                Text = x.Name,
                Value = x.Id

            }).ToList();
        }

        public async Task<VMCommonProductCategory> GetSingleProductCategory(int id)
        {
            var v = await Task.Run(() => (from t1 in _db.ProductCategories
                                          where t1.ProductCategoryId == id && t1.IsActive == true
                                          select new VMCommonProductCategory
                                          {
                                              ID = t1.ProductCategoryId,
                                              Name = t1.Name

                                          }).FirstOrDefault());
            return v;
        }
        public async Task<VMCommonProductSubCategory> GetSingleProductSubCategory(int id)
        {
            var v = await Task.Run(() => (from t1 in _db.ProductCategories
                                          where t1.ProductCategoryId == id && t1.IsActive == true
                                          select new VMCommonProductSubCategory
                                          {
                                              ID = t1.ProductCategoryId,
                                              Name = t1.Name
                                          }).FirstOrDefault());
            return v;
        }
        public async Task<List<VMCommonProductSubCategory>> CommonProductSubCategoryGet(int companyId, int categoryId)
        {

            List<VMCommonProductSubCategory> vmCommonProductSubCategoryList =
                await Task.Run(() => (_db.ProductSubCategories.AsNoTracking()
                .Where(x => x.IsActive == true && x.ProductCategoryId == categoryId && x.CompanyId == companyId)
                .Join(_db.ProductCategories.AsNoTracking().Where(c => c.IsActive == true),
                t1 => t1.ProductCategoryId,
                t2 => t2.ProductCategoryId,
                (t1, t2) => new VMCommonProductSubCategory
                {
                    ID = t1.ProductSubCategoryId,
                    Name = t1.Name
                })
                .ToListAsync()));


            return vmCommonProductSubCategoryList;
        }
        public async Task<List<VMCommonUnit>> CompanyMenusGet(int companyId)
        {
            List<VMCommonUnit> vmCommonUnit =
                await Task.Run(() => (_db.CompanyMenus
                .Where(x => x.IsActive == true && x.CompanyId == companyId))
                .Select(x => new VMCommonUnit() { ID = x.CompanyMenuId, Name = x.Name })
                .ToListAsync());
            return vmCommonUnit;
        }
        public async Task<List<VMCommonProduct>> CommonProductGet(int? companyId, int productSubCategoryId)
        {
            List<VMCommonProduct> vmCommonProductList = new List<VMCommonProduct>();

            if (productSubCategoryId > 0)
            {


                //vmCommonProductList =
                //await Task.Run(() => (_db.Products
                //.Where(x => x.IsActive == true && x.ProductSubCategoryId == productSubCategoryId && x.CompanyId == companyId))
                //.Select(x => new VMCommonProduct() { ID = x.ProductId, Name = x.ProductName })
                //.ToListAsync());

                vmCommonProductList = (from t1 in _db.Products
                                       join t2 in _db.ProductSubCategories on t1.ProductSubCategoryId equals t2.ProductSubCategoryId
                                       join t3 in _db.ProductCategories on t2.ProductCategoryId equals t3.ProductCategoryId

                                       where t1.CompanyId == companyId && t1.IsActive == true && t2.IsActive == true && t3.IsActive == true &&
                                       t2.ProductSubCategoryId == productSubCategoryId

                                       select new VMCommonProduct
                                       {
                                           ID = t1.ProductId,
                                           Name = t1.ProductName
                                       }).OrderBy(x => x.ID).ToList();
            }

            if (productSubCategoryId is 0)
            {
                vmCommonProductList = (from t1 in _db.Products
                                       join t2 in _db.ProductSubCategories on t1.ProductSubCategoryId equals t2.ProductSubCategoryId
                                       join t3 in _db.ProductCategories on t2.ProductCategoryId equals t3.ProductCategoryId

                                       where t1.CompanyId == companyId && t1.IsActive == true && t2.IsActive == true && t3.IsActive == true
                                       //&& t2.ProductSubCategoryId == productSubCategoryId

                                       select new VMCommonProduct
                                       {
                                           ID = t1.ProductId,
                                           Name = t1.ProductName
                                       }).OrderBy(x => x.ID).ToList();

            }

            return vmCommonProductList;
        }

        public async Task<List<VMCommonDistricts>> CommonDistrictsGet(int id)
        {

            List<VMCommonDistricts> vmCommonDistricts = await Task.Run(() => (_db.Districts.Where(x => x.IsActive == true && x.DivisionId == id)).Select(x => new VMCommonDistricts() { ID = x.DistrictId, Name = x.Name }).ToListAsync());

            return vmCommonDistricts;
        }
        public async Task<List<VMCommonDistricts>> CommonSubZonesGet(int id)
        {
            List<VMCommonDistricts> vmCommonDistricts = await Task.Run(() => (_db.SubZones.Where(x => x.IsActive == true && x.AreaId == id)).Select(x => new VMCommonDistricts() { ID = x.SubZoneId, Name = x.Name }).ToListAsync());
            return vmCommonDistricts;
        }


        public async Task<List<VMCommonDistricts>> CommonZoneDivisionGet(int companyId, int zoneId)
        {
            List<VMCommonDistricts> vmZoneDivisions = await Task.Run(() => (_db.ZoneDivisions.Where(x => x.IsActive == true && x.ZoneId == zoneId && x.CompanyId == companyId)).Select(x => new VMCommonDistricts() { ID = x.ZoneDivisionId, Name = x.Name }).ToListAsync());
            return vmZoneDivisions;
        }
        public async Task<List<VMCommonDistricts>> AllZoneDivisionGet(int companyId)
        {
            List<VMCommonDistricts> vmZoneDivisions = await Task.Run(() => (_db.ZoneDivisions.Where(x => x.IsActive == true && x.CompanyId == companyId)).Select(x => new VMCommonDistricts() { ID = x.ZoneDivisionId, Name = x.Name }).ToListAsync());
            return vmZoneDivisions;
        }



        #region Supplier
        public async Task<VMCommonSupplier> GetSupplier(int companyId)
        {
            VMCommonSupplier vmCommonSupplier = new VMCommonSupplier();
            vmCommonSupplier.CompanyFK = companyId;
            vmCommonSupplier.DataList = await Task.Run(() => (from t1 in _db.Vendors.Where(x => x.VendorTypeId == (int)Provider.Supplier && x.CompanyId == companyId && x.IsActive == true)
                                                              join t2 in _db.Countries on t1.CountryId equals t2.CountryId
                                                              where t1.VendorId > 0
                                                              select new VMCommonSupplier
                                                              {
                                                                  ID = t1.VendorId,
                                                                  Name = t1.Name,
                                                                  Email = t1.Email,
                                                                  Phone = t1.Phone,
                                                                  Country = t2.CountryName,
                                                                  CompanyFK = t1.CompanyId,
                                                                  Common_CountriesFk = t1.CountryId.Value,
                                                                  HeadGLId = t1.HeadGLId,
                                                                  ContactPerson = t1.ContactName,
                                                                  Address = t1.Address,
                                                                  Code = t1.Code,
                                                                  CreatedBy = t1.CreatedBy,
                                                                  Remarks = t1.Remarks,
                                                                  IsForeign = t1.IsForeign,
                                                                  BranchName = t1.BranchName,
                                                                  ACName = t1.ACName,
                                                                  ACNo = t1.ACNo,
                                                                  BankName = t1.BankName
                                                              }).OrderByDescending(x => x.ID).AsEnumerable());


            return vmCommonSupplier;
        }
        public async Task<int> SupplierAdd(VMCommonSupplier vmCommonSupplier)
        {
            var result = -1;

            #region Genarate Supplier code
            int totalSupplier = _db.Vendors.Count(x => x.VendorTypeId == (int)Provider.Supplier && x.CompanyId == vmCommonSupplier.CompanyFK);


            if (totalSupplier == 0)
            {
                totalSupplier = 1;
            }
            else
            {
                totalSupplier++;
            }

            var newString = totalSupplier.ToString().PadLeft(4, '0');
            #endregion

            Vendor commonSupplier = new Vendor
            {
                Code = newString,
                Name = vmCommonSupplier.Name,
                Phone = vmCommonSupplier.Phone,
                ContactName = vmCommonSupplier.ContactPerson,
                Email = vmCommonSupplier.Email,
                Address = vmCommonSupplier.Address,
                VendorTypeId = (int)Provider.Supplier,
                IsForeign = vmCommonSupplier.IsForeign,
                CompanyId = vmCommonSupplier.CompanyFK.Value,
                CountryId = vmCommonSupplier.Common_CountriesFk,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                BranchName = vmCommonSupplier.BranchName,
                ACName = vmCommonSupplier.ACName,
                ACNo = vmCommonSupplier.ACNo,
                BankName = vmCommonSupplier.BankName,
                IsActive = true,
                CreditRatioFrom = 0,
                CreditRatioTo = 0,

            };
            _db.Vendors.Add(commonSupplier);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = commonSupplier.VendorId;

                //31016 Account Payable Head for Seed Company

            }
            if (result > 0)
            {
                VMHeadIntegration vmHeadIntegration = new VMHeadIntegration();

                if (commonSupplier.CompanyId == (int)CompanyName.KrishibidSeedLimited)
                {
                    vmHeadIntegration = new VMHeadIntegration
                    {
                        AccName = commonSupplier.Name,
                        LayerNo = 6,
                        Remarks = "GL Layer",
                        IsIncomeHead = false,
                        ParentId = 31016,
                        IsActive = true,

                        CompanyFK = commonSupplier.CompanyId,
                        CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                        CreatedDate = DateTime.Now,

                    };
                }
                if (commonSupplier.CompanyId == (int)CompanyName.GloriousCropCareLimited)
                {
                    vmHeadIntegration = new VMHeadIntegration
                    {
                        AccName = commonSupplier.Name,
                        LayerNo = 6,
                        Remarks = "GL Layer",
                        IsIncomeHead = false,
                        ParentId = 39195,
                        IsActive = true,

                        CompanyFK = commonSupplier.CompanyId,
                        CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                        CreatedDate = DateTime.Now,
                    };
                }

                if (commonSupplier.CompanyId == (int)CompanyName.KrishibidFeedLimited)
                {
                    vmHeadIntegration = new VMHeadIntegration
                    {
                        AccName = commonSupplier.Name,
                        LayerNo = 6,
                        Remarks = "GL Layer",
                        IsIncomeHead = false,
                        ParentId = 5283,

                        CompanyFK = commonSupplier.CompanyId,
                        CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                        CreatedDate = DateTime.Now,
                    };
                }

                HeadGL headGl = await PayableHeadIntegrationAdd(vmHeadIntegration);
                if (headGl != null)
                {
                    await VendorsCodeAndHeadGLIdEdit(commonSupplier.VendorId, headGl);
                }
            }


            return result;
        }

        public async Task<int> SupplierEdit(VMCommonSupplier vmCommonSupplier)
        {
            var result = -1;
            Vendor commonSupplier = await _db.Vendors.FindAsync(vmCommonSupplier.ID);

            commonSupplier.Name = vmCommonSupplier.Name;
            commonSupplier.Phone = vmCommonSupplier.Phone;
            commonSupplier.ContactName = vmCommonSupplier.ContactPerson;
            commonSupplier.Email = vmCommonSupplier.Email;
            commonSupplier.Address = vmCommonSupplier.Address;
            commonSupplier.IsForeign = vmCommonSupplier.IsForeign;

            commonSupplier.CountryId = vmCommonSupplier.Common_CountriesFk;
            commonSupplier.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            commonSupplier.ModifiedDate = DateTime.Now;
            commonSupplier.BranchName = vmCommonSupplier.BranchName;
            commonSupplier.ACName = vmCommonSupplier.ACName;
            commonSupplier.ACNo = vmCommonSupplier.ACNo;
            commonSupplier.BankName = vmCommonSupplier.BankName;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = commonSupplier.VendorId;
            }
            await IntegratedAccountsHeadEdit(commonSupplier.Name, commonSupplier.HeadGLId.Value);
            return result;
        }

        public async Task<HeadGL> IntegratedAccountsHeadEdit(string accName, int headGlId)
        {
            long result = -1;

            HeadGL headGL = await _db.HeadGLs.FindAsync(headGlId);
            headGL.AccName = accName;

            headGL.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            headGL.ModifiedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = headGL.Id;
            }
            return headGL;
        }

        public async Task<int> SupplierDelete(int id)
        {
            int result = -1;
            if (id != 0)
            {
                Vendor commonSupplier = await _db.Vendors.FindAsync(id);
                commonSupplier.IsActive = false;
                commonSupplier.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                commonSupplier.ModifiedDate = DateTime.Now;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = commonSupplier.VendorId;
                }
            }
            return result;
        }

        #endregion


        public async Task<VMPOTremsAndConditions> GetPOTremsAndConditions(int companyId)
        {
            VMPOTremsAndConditions vmCommonZone = new VMPOTremsAndConditions();
            vmCommonZone.CompanyFK = companyId;
            vmCommonZone.DataList = await Task.Run(() => (from t1 in _db.POTremsAndConditions
                                                          where t1.IsActive == true && t1.CompanyId == companyId
                                                          select new VMPOTremsAndConditions
                                                          {
                                                              ID = t1.ID,
                                                              Key = t1.Key,
                                                              Value = t1.Value,
                                                              CompanyFK = t1.CompanyId
                                                          }).OrderByDescending(x => x.ID).AsEnumerable());
            return vmCommonZone;
        }

        public async Task<int> POTremsAndConditionAdd(VMPOTremsAndConditions vmPOTremsAndConditions)
        {
            var result = -1;
            POTremsAndCondition poTremsAndConditions = new POTremsAndCondition
            {
                Key = vmPOTremsAndConditions.Key,
                Value = vmPOTremsAndConditions.Value,


                CompanyId = vmPOTremsAndConditions.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true

            };
            _db.POTremsAndConditions.Add(poTremsAndConditions);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = poTremsAndConditions.ID;
            }
            return result;
        }

        public async Task<int> POTremsAndConditionEdit(VMPOTremsAndConditions vmPOTremsAndConditions)
        {
            var result = -1;
            POTremsAndCondition poTremsAndCondition = _db.POTremsAndConditions.Find(vmPOTremsAndConditions.ID);

            poTremsAndCondition.Key = vmPOTremsAndConditions.Key;
            poTremsAndCondition.Value = vmPOTremsAndConditions.Value;

            poTremsAndCondition.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            poTremsAndCondition.ModifiedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = poTremsAndCondition.ID;
            }
            return result;
        }

        public async Task<int> POTremsAndConditionDelete(int id)
        {
            int result = -1;
            if (id != 0)
            {
                POTremsAndCondition poTremsAndCondition = await _db.POTremsAndConditions.FindAsync(id);
                poTremsAndCondition.IsActive = false;

                if (await _db.SaveChangesAsync() > 0)
                {
                    result = poTremsAndCondition.ID;
                }
            }
            return result;
        }

        #region HeadGL


        public async Task<int> GLDLBlockCodeAndHeadGLIdEdit(int supplierId, HeadGL headGl, int head5Id)
        {
            var result = -1;

            ProductSubCategory productCategories = await _db.ProductSubCategories.FindAsync(supplierId);
            productCategories.AccountingIncomeHeadId = headGl.Id;
            productCategories.AccountingHeadId = head5Id;

            productCategories.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            productCategories.ModifiedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = productCategories.ProductSubCategoryId;
            }
            return result;
        }

        public async Task<int> VendorsCodeAndHeadGLIdEdit(int supplierId, HeadGL headGl)
        {
            var result = -1;

            Vendor commonSupplier = await _db.Vendors.FindAsync(supplierId);
            commonSupplier.HeadGLId = headGl.Id;
            commonSupplier.Code = headGl.AccCode;

            commonSupplier.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            commonSupplier.ModifiedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = commonSupplier.VendorId;
            }
            return result;
        }

        public async Task<HeadGL> PayableHeadIntegrationAdd(VMHeadIntegration vmHeadIntegration)
        {
            long result = -1;

            string newAccountCode = "";
            int orderNo = 0;

            Head5 parentHead = _db.Head5.FirstOrDefault(x => x.Id == vmHeadIntegration.ParentId);

            IQueryable<HeadGL> childHeads = _db.HeadGLs.Where(x => x.ParentId == vmHeadIntegration.ParentId);

            if (childHeads.Count() > 0)
            {
                string lastAccCode = childHeads.OrderByDescending(x => x.AccCode).FirstOrDefault()?.AccCode;
                string parentPart = lastAccCode?.Substring(0, 10);
                string childPart = lastAccCode?.Substring(10, 3);
                newAccountCode = parentPart + (Convert.ToInt32(childPart) + 1).ToString().PadLeft(3, '0');
                orderNo = childHeads.Count();
            }
            else
            {
                newAccountCode = parentHead?.AccCode + "001";
                orderNo += 1;
            }


            HeadGL headGL = new HeadGL
            {
                Id = _db.Database.SqlQuery<int>("spGetNewId").FirstOrDefault(),
                AccCode = newAccountCode,
                LayerNo = vmHeadIntegration.LayerNo,
                IsIncomeHead = vmHeadIntegration.IsIncomeHead,
                CompanyId = vmHeadIntegration.CompanyFK,
                CreateDate = vmHeadIntegration.CreatedDate,
                CreatedBy = vmHeadIntegration.CreatedBy,
                AccName = vmHeadIntegration.AccName,
                ParentId = vmHeadIntegration.ParentId,
                OrderNo = orderNo,
                Remarks = vmHeadIntegration.Remarks,
                IsActive = true

            };
            _db.HeadGLs.Add(headGL);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = headGL.Id;
            }
            return headGL;
        }


        #endregion

        #region Zone

        public List<SelectModel> GetAllZoneSelectModels(int companyId)
        {
            List<SelectModel> stocks = _db.Zones.Where(x => x.CompanyId == companyId && x.IsActive).ToList().Select(x => new SelectModel()
            {
                Text = x.Name,
                Value = x.ZoneId
            }).OrderBy(x => x.Text).ToList();
            stocks.Add(new SelectModel { Text = "All", Value = "0" });
            return stocks.OrderBy(x => Convert.ToInt32(x.Value)).ToList();
        }

        public List<SelectModel> GetZoneSelectList(int companyId)
        {
            List<SelectModel> selectModelList = new List<SelectModel>();
            SelectModel selectModel = new SelectModel();


            var v = _db.Zones.Where(x => x.CompanyId == companyId && x.IsActive == true).ToList()
                .Select(x => new SelectModel()
                {
                    Text = x.Name,
                    Value = x.ZoneId
                }).ToList();

            selectModelList.AddRange(v);
            return selectModelList;
        }

        public async Task<VMCommonZone> GetZones(int companyId)
        {
            VMCommonZone vmCommonZone = new VMCommonZone();
            vmCommonZone.CompanyFK = companyId;
            vmCommonZone.DataList = await Task.Run(() => (from t1 in _db.Zones
                                                          where t1.IsActive == true
                                                          && t1.CompanyId == companyId
                                                          select new VMCommonZone
                                                          {
                                                              ID = t1.ZoneId,
                                                              Name = t1.Name,
                                                              Code = t1.Code,
                                                              EmployeeId = t1.EmployeeId,
                                                              CompanyFK = t1.CompanyId,
                                                              ZoneIncharge = t1.ZoneIncharge,
                                                              Designation = t1.Designation,
                                                              Email = t1.Email,
                                                              MobileOffice = t1.MobileOffice,
                                                              MobilePersonal = t1.MobilePersonal,

                                                          }).OrderByDescending(x => x.ID).AsEnumerable());
            return vmCommonZone;
        }

        public async Task<int> ZoneAdd(VMCommonZone vmCommonZone)
        {
            var result = -1;
            #region check Zone Duplicate
            var isExist = await _db.Zones.FirstOrDefaultAsync(u => u.Name.Equals(vmCommonZone.Name) && u.IsActive == true);
            if (isExist?.ZoneId > 0)
            {
                throw new Exception($"Sorry! This Name {vmCommonZone.Name} already Exists!");
            }
            #endregion


            Zone zone = new Zone
            {
                Name = vmCommonZone.Name,
                ZoneIncharge = vmCommonZone.ZoneIncharge,
                //Code = vmCommonZone.Code, //Don't use Code it will add from Head4
                EmployeeId = vmCommonZone.EmployeeId,
                Designation = vmCommonZone.Designation,
                Email = vmCommonZone.Email,
                MobileOffice = vmCommonZone.MobileOffice,
                MobilePersonal = vmCommonZone.MobilePersonal,

                CompanyId = vmCommonZone.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true

            };
            _db.Zones.Add(zone);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = zone.ZoneId;


                VMHeadIntegration integration = new VMHeadIntegration
                {
                    AccName = zone.Name,
                    LayerNo = 4,
                    Remarks = "4th Layer",
                    IsIncomeHead = false,
                    ParentId = 29631, // See Account Receivable Id
                    IsActive = true,

                    CompanyFK = zone.CompanyId,
                    CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                    CreatedDate = DateTime.Now,
                };
                if (zone.CompanyId == (int)CompanyName.KrishibidSeedLimited)
                {
                    Head4 head4 = await Head4IntegrationAdd(integration);
                    if (head4 != null)
                    {
                        await ZoneCodeAndHeadIdEdit(zone.ZoneId, head4);
                    }
                }
            }
            return result;
        }

        public async Task<Head4> Head4IntegrationAdd(VMHeadIntegration vmHeadIntegration)
        {
            long result = -1;

            string newAccountCode = "";
            int orderNo = 0;


            Head3 parentHead = _db.Head3.FirstOrDefault(x => x.Id == vmHeadIntegration.ParentId);

            IQueryable<Head4> childHeads = _db.Head4.Where(x => x.ParentId == vmHeadIntegration.ParentId);

            if (childHeads.Count() > 0)
            {
                string lastAccCode = childHeads.OrderByDescending(x => x.AccCode).FirstOrDefault()?.AccCode;
                string parentPart = lastAccCode?.Substring(0, 4);
                string childPart = lastAccCode?.Substring(4, 3);
                newAccountCode = parentPart + (Convert.ToInt32(childPart) + 1).ToString().PadLeft(3, '0');
                orderNo = childHeads.Count();
            }
            else
            {
                newAccountCode = parentHead?.AccCode + "001";
                orderNo += 1;
            }


            Head4 head4 = new Head4
            {
                Id = _db.Database.SqlQuery<int>("spGetNewId").FirstOrDefault(),
                AccCode = newAccountCode,
                LayerNo = vmHeadIntegration.LayerNo,

                CompanyId = vmHeadIntegration.CompanyFK,
                CreateDate = vmHeadIntegration.CreatedDate,
                CreatedBy = vmHeadIntegration.CreatedBy,
                AccName = vmHeadIntegration.AccName,
                ParentId = vmHeadIntegration.ParentId,
                OrderNo = orderNo,
                Remarks = vmHeadIntegration.Remarks,
                IsActive = true
            };
            _db.Head4.Add(head4);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = head4.Id;
            }
            return head4;
        }

        public async Task<int> ZoneCodeAndHeadIdEdit(int zoneId, Head4 head4)
        {
            var result = -1;

            Zone zone = await _db.Zones.FindAsync(zoneId);
            zone.HeadGLId = head4.Id;
            zone.Code = head4.AccCode;
            zone.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            zone.ModifiedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = zone.ZoneId;
            }
            return result;
        }

        public async Task<int> ZonesEdit(VMCommonZone vmCommonZone)
        {
            var result = -1;

            #region check Zone Duplicate
            var isExist = await _db.Zones.FirstOrDefaultAsync(u => u.Name.Equals(vmCommonZone.Name) && u.ZoneId != vmCommonZone.ID && u.IsActive == true);
            if (isExist?.ZoneId > 0)
            {
                throw new Exception($"Sorry! This Name {vmCommonZone.Name} already Exists!");
            }
            #endregion

            Zone zone = await _db.Zones.FindAsync(vmCommonZone.ID);
            zone.Name = vmCommonZone.Name;
            zone.EmployeeId = vmCommonZone.EmployeeId;
            //zone.Code = vmCommonZone.Code; //Don't use Code it will add from Head4
            //zone.ZoneIncharge = vmCommonZone.ZoneIncharge;
            //zone.Designation = vmCommonZone.Designation;
            //zone.Email = vmCommonZone.Email;
            //zone.MobileOffice = vmCommonZone.MobileOffice;
            //zone.MobilePersonal = vmCommonZone.MobilePersonal;

            zone.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            zone.ModifiedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = zone.ZoneId;

                UpdateZoneForSeed(zone);

            }
            return result;
        }

        private void UpdateZoneForSeed(Zone zone)
        {
            Head4 head4 = _db.Head4.Find(zone.HeadGLId);
            head4.AccName = zone.Name;
            head4.AccCode = zone.Code;

            head4.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            head4.ModifiedDate = DateTime.Now;

            _db.SaveChanges();
        }

        public async Task<int> ZonesDelete(int id)
        {
            int result = -1;
            if (id != 0)
            {
                Zone zone = await _db.Zones.FindAsync(id);
                zone.IsActive = false;
                zone.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                zone.ModifiedDate = DateTime.Now;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = zone.ZoneId;
                    DeleteZoneForSeed(zone);
                }
            }
            return result;
        }

        private void DeleteZoneForSeed(Zone zone)
        {
            Head4 head4 = _db.Head4.Find(zone.HeadGLId);
            head4.IsActive = false;
            head4.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            head4.ModifiedDate = DateTime.Now;
            _db.SaveChanges();
        }

        public async Task<bool> CheckDuplicateZoneName(string zoneName, int id)
        {
            bool isExist = false;
            zoneName = zoneName.Trim();
            if (string.IsNullOrEmpty(zoneName))
            {
                return isExist;
            }
            if (id > 0)
            {
                isExist = await _db.Zones.AnyAsync(u => u.Name.Equals(zoneName) && u.ZoneId != id && u.IsActive == true);

            }
            else
            {
                isExist = await _db.Zones.AnyAsync(u => u.Name.Equals(zoneName) && u.IsActive == true);
            }

            return isExist;
        }

        #endregion

        #region ZoneDivision
        public List<object> CommonZoneDivisionDropDownList(int companyId, int zoneId = 0)
        {
            var list = new List<object>();
            var v = _db.ZoneDivisions.Where(x => x.IsActive == true && x.CompanyId == companyId && (zoneId > 0 ? x.ZoneId == zoneId : x.ZoneDivisionId > 0)).ToList();
            foreach (var x in v)
            {
                list.Add(new { Text = x.Name, Value = x.ZoneDivisionId });
            }
            return list;
        }

        public List<SelectModel> GetZoneDivisionSelectList(int companyId, int? zoneId)
        {
            List<SelectModel> selectModelList = new List<SelectModel>();
            SelectModel selectModel = new SelectModel
            {
                Text = "==Select ZoneDivision==",
                Value = 0,
            };
            selectModelList.Add(selectModel);

            if (zoneId.HasValue && zoneId > 0)
            {
                var v = _db.ZoneDivisions.Where(x => x.CompanyId == companyId && x.ZoneId == zoneId && x.IsActive == true).ToList()
                    .Select(x => new SelectModel()
                    {
                        Text = x.Name,
                        Value = x.ZoneDivisionId
                    }).ToList();
                selectModelList.AddRange(v);
            }
            else
            {
                var v = _db.ZoneDivisions.Where(x => x.CompanyId == companyId && x.IsActive == true).ToList()
                    .Select(x => new SelectModel()
                    {
                        Text = x.Name,
                        Value = x.ZoneDivisionId
                    }).ToList();
                selectModelList.AddRange(v);
            }

            return selectModelList;
        }

        public async Task<VMCommonZoneDivision> GetZoneDivisions(int companyId, int zoneId = 0)
        {
            VMCommonZoneDivision vmCommonZoneDivision = new VMCommonZoneDivision();
            vmCommonZoneDivision.CompanyFK = companyId;
            vmCommonZoneDivision.DataList = await Task.Run(() => (from t1 in _db.ZoneDivisions
                                                                  join t2 in _db.Zones on t1.ZoneId equals t2.ZoneId into t2_Zones
                                                                  from t2 in t2_Zones.DefaultIfEmpty()
                                                                  where t1.IsActive == true && t1.CompanyId == companyId
                                                                  && (zoneId > 0 ? t1.ZoneId == zoneId : t1.ZoneDivisionId > 0)
                                                                  select new VMCommonZoneDivision
                                                                  {
                                                                      ID = t1.ZoneDivisionId,
                                                                      ZoneId = t1.ZoneId,
                                                                      ZoneName = t2.Name,
                                                                      Name = t1.Name,
                                                                      Code = t1.Code,
                                                                      ZoneDivisionIncharge = t1.ZoneDivisionIncharge,
                                                                      SalesOfficerName = t1.SalesOfficerName,
                                                                      Designation = t1.Designation,
                                                                      Email = t1.Email,
                                                                      MobileOffice = t1.MobileOffice,
                                                                      MobilePersonal = t1.MobilePersonal,
                                                                      CompanyFK = t1.CompanyId,
                                                                      EmployeeId = t1.EmployeeId
                                                                  }).OrderByDescending(x => x.ID).AsEnumerable());
            return vmCommonZoneDivision;
        }

        public async Task<int> ZoneDivisionAdd(VMCommonZoneDivision vmCommonZoneDivision)
        {
            var result = -1;

            #region check ZoneDivision Duplicate
            var isExist = await _db.ZoneDivisions.FirstOrDefaultAsync(u => u.Name.Equals(vmCommonZoneDivision.Name) && u.ZoneId == vmCommonZoneDivision.ZoneId && u.IsActive == true);
            if (isExist?.ZoneDivisionId > 0)
            {
                throw new Exception($"Sorry! This Name {vmCommonZoneDivision.Name} already Exists!");
            }
            #endregion

            ZoneDivision zoneDivision = new ZoneDivision
            {
                Name = vmCommonZoneDivision.Name,
                // Code = vmCommonZoneDivision.Code, // Don't use zoneDivision it will add form Head5
                ZoneDivisionIncharge = vmCommonZoneDivision.ZoneDivisionIncharge,
                SalesOfficerName = vmCommonZoneDivision.SalesOfficerName,
                Designation = vmCommonZoneDivision.Designation,
                Email = vmCommonZoneDivision.Email,
                MobileOffice = vmCommonZoneDivision.MobileOffice,
                MobilePersonal = vmCommonZoneDivision.MobilePersonal,
                ZoneId = vmCommonZoneDivision.ZoneId,
                EmployeeId = vmCommonZoneDivision.EmployeeId,
                CompanyId = vmCommonZoneDivision.CompanyFK.Value,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true

            };
            _db.ZoneDivisions.Add(zoneDivision);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = zoneDivision.ZoneDivisionId;
            }

            return result;
        }

        public async Task<int> ZoneDivisionEdit(VMCommonZoneDivision vmCommonZoneDivision)
        {
            var result = -1;

            #region check ZoneDivision Duplicate
            var isExist = await _db.ZoneDivisions.FirstOrDefaultAsync(u => u.Name.Equals(vmCommonZoneDivision.Name) && u.ZoneId == vmCommonZoneDivision.ZoneId && u.ZoneDivisionId != vmCommonZoneDivision.ID && u.IsActive == true);
            if (isExist?.ZoneDivisionId > 0)
            {
                throw new Exception($"Sorry! This Name {vmCommonZoneDivision.Name} already Exists!");
            }
            #endregion

            ZoneDivision zoneDivision = await _db.ZoneDivisions.FindAsync(vmCommonZoneDivision.ID);
            zoneDivision.ZoneId = vmCommonZoneDivision.ZoneId;
            zoneDivision.Name = vmCommonZoneDivision.Name;
            // zoneDivision.Code = vmCommonZoneDivision.Code; // Don't use zoneDivision it will add form Head5
            //zoneDivision.ZoneDivisionIncharge = vmCommonZoneDivision.ZoneDivisionIncharge;
            //zoneDivision.SalesOfficerName = vmCommonZoneDivision.SalesOfficerName;
            //zoneDivision.Designation = vmCommonZoneDivision.Designation;
            //zoneDivision.Email = vmCommonZoneDivision.Email;
            //zoneDivision.MobilePersonal = vmCommonZoneDivision.MobilePersonal;
            //zoneDivision.MobileOffice = vmCommonZoneDivision.MobileOffice;
            zoneDivision.EmployeeId = vmCommonZoneDivision.EmployeeId;
            zoneDivision.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            zoneDivision.ModifiedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = zoneDivision.ZoneDivisionId;
            }
            return result;
        }

        public async Task<int> ZoneDivisionDelete(int id)
        {
            int result = -1;
            if (id != 0)
            {
                ZoneDivision zoneDivision = await _db.ZoneDivisions.FindAsync(id);
                zoneDivision.IsActive = false;
                zoneDivision.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                zoneDivision.ModifiedDate = DateTime.Now;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = zoneDivision.ZoneId;
                }
            }
            return result;
        }

        public async Task<bool> CheckDuplicateZoneDivisionName(int zoneId, string zoneDivisionName, int id)
        {
            bool isExist = false;
            if (string.IsNullOrEmpty(zoneDivisionName))
            {
                return isExist;
            }
            if (id > 0)
            {
                isExist = await _db.ZoneDivisions.AnyAsync(u => u.Name.Equals(zoneDivisionName) && u.ZoneId == zoneId && u.ZoneDivisionId != id && u.IsActive == true);
            }
            else
            {
                isExist = await _db.ZoneDivisions.AnyAsync(u => u.Name.Equals(zoneDivisionName) && u.ZoneId == zoneId && u.IsActive == true);
            }

            return isExist;
        }

        #endregion

        #region Region
        public List<object> CommonRegionDropDownList(int companyId, int zoneId = 0, int zoneDivisionId = 0)
        {
            var list = new List<object>();
            var v = _db.Regions.Where(x => x.IsActive == true && x.CompanyId == companyId && (zoneId > 0 && zoneDivisionId > 0 ? x.ZoneId == zoneId && x.ZoneDivisionId == zoneDivisionId : x.RegionId > 0)).ToList();

            foreach (var x in v)
            {
                list.Add(new { Text = x.Name, Value = x.RegionId });
            }

            return list;
        }

        public List<SelectModel> GetRegionSelectList(int companyId, int? zoneId, int? zoneDivisionId)
        {
            List<SelectModel> selectModelList = new List<SelectModel>();
            SelectModel selectModel = new SelectModel
            {
                Text = "==Select Region==",
                Value = 0,
            };
            selectModelList.Add(selectModel);

            if (zoneId.HasValue && zoneId > 0 && zoneDivisionId > 0)
            {
                var v = _db.Regions.Where(x => x.CompanyId == companyId && x.ZoneId == zoneId && x.ZoneDivisionId == zoneDivisionId && x.IsActive == true).ToList()
                    .Select(x => new SelectModel()
                    {
                        Text = x.Name,
                        Value = x.RegionId
                    }).ToList();
                selectModelList.AddRange(v);
            }
            else if (zoneId.HasValue && zoneId > 0)
            {
                var v = _db.Regions.Where(x => x.CompanyId == companyId && x.ZoneId == zoneId && x.IsActive == true).ToList()
                    .Select(x => new SelectModel()
                    {
                        Text = x.Name,
                        Value = x.RegionId
                    }).ToList();
                selectModelList.AddRange(v);
            }
            else
            {
                var v = _db.Regions.Where(x => x.CompanyId == companyId && x.IsActive == true).ToList()
                    .Select(x => new SelectModel()
                    {
                        Text = x.Name,
                        Value = x.RegionId
                    }).ToList();
                selectModelList.AddRange(v);
            }

            return selectModelList;
        }

        public async Task<VMCommonRegion> GetRegions(int companyId, int zoneId = 0, int zoneDivisionId = 0)
        {
            VMCommonRegion vmCommonRegion = new VMCommonRegion();
            vmCommonRegion.CompanyFK = companyId;
            vmCommonRegion.DataList = await Task.Run(() => (from t1 in _db.Regions
                                                            join t2 in _db.Zones on t1.ZoneId equals t2.ZoneId into t2_Zones
                                                            from t2 in t2_Zones.DefaultIfEmpty()
                                                            join t3 in _db.ZoneDivisions on t1.ZoneDivisionId equals t3.ZoneDivisionId into t3_ZoneDivision
                                                            from t3 in t3_ZoneDivision.DefaultIfEmpty()
                                                            where t1.IsActive == true && t1.CompanyId == companyId
                                                            && (zoneId > 0 && zoneDivisionId > 0 ? t1.ZoneId == zoneId && t1.ZoneDivisionId == zoneDivisionId : t1.RegionId > 0)
                                                            select new VMCommonRegion
                                                            {
                                                                ID = t1.RegionId,
                                                                ZoneId = t1.ZoneId,
                                                                ZoneName = t2.Name,
                                                                ZoneDivisionId = t1.ZoneDivisionId,
                                                                ZoneDivisionName = t3.Name,
                                                                Name = t1.Name,
                                                                Code = t1.Code,
                                                                RegionIncharge = t1.RegionIncharge,
                                                                SalesOfficerName = t1.SalesOfficerName,
                                                                Designation = t1.Designation,
                                                                Email = t1.Email,
                                                                MobileOffice = t1.MobileOffice,
                                                                MobilePersonal = t1.MobilePersonal,
                                                                CompanyFK = t1.CompanyId,
                                                                EmployeeId = t1.EmployeeId
                                                            }).OrderByDescending(x => x.ID).AsEnumerable());
            return vmCommonRegion;
        }

        public async Task<int> RegionAdd(VMCommonRegion vmCommonRegion)
        {
            var result = -1;

            #region check Region Duplicate
            var isExist = await _db.Regions.FirstOrDefaultAsync(u => u.Name.Equals(vmCommonRegion.Name) && u.ZoneId == vmCommonRegion.ZoneId && u.ZoneDivisionId == vmCommonRegion.ZoneDivisionId && u.IsActive == true);
            if (isExist?.RegionId > 0)
            {
                throw new Exception($"Sorry! This Name {vmCommonRegion.Name} already Exists!");
            }
            #endregion

            Region region = new Region
            {
                Name = vmCommonRegion.Name,
                // Code = vmCommonZoneDivision.Code, // Don't use zoneDivision it will add form Head5
                RegionIncharge = vmCommonRegion.RegionIncharge,
                SalesOfficerName = vmCommonRegion.SalesOfficerName,
                Designation = vmCommonRegion.Designation,
                Email = vmCommonRegion.Email,
                MobileOffice = vmCommonRegion.MobileOffice,
                MobilePersonal = vmCommonRegion.MobilePersonal,
                ZoneId = vmCommonRegion.ZoneId,
                ZoneDivisionId = vmCommonRegion.ZoneDivisionId,
                EmployeeId = vmCommonRegion.EmployeeId,
                CompanyId = vmCommonRegion.CompanyFK.Value,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true

            };
            _db.Regions.Add(region);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = region.RegionId;
            }

            return result;
        }

        public async Task<int> RegionEdit(VMCommonRegion vmCommonRegion)
        {
            var result = -1;

            #region check Region Duplicate
            var isExist = await _db.Regions.FirstOrDefaultAsync(u => u.Name.Equals(vmCommonRegion.Name) && u.ZoneId == vmCommonRegion.ZoneId && u.ZoneDivisionId == vmCommonRegion.ZoneDivisionId && u.RegionId != vmCommonRegion.ID && u.IsActive == true);
            if (isExist?.RegionId > 0)
            {
                throw new Exception($"Sorry! This Name {vmCommonRegion.Name} already Exists!");
            }
            #endregion

            Region region = await _db.Regions.FindAsync(vmCommonRegion.ID);
            region.ZoneId = vmCommonRegion.ZoneId;
            region.ZoneDivisionId = vmCommonRegion.ZoneDivisionId;
            region.Name = vmCommonRegion.Name;
            // zoneDivision.Code = vmCommonZoneDivision.Code; // Don't use zoneDivision it will add form Head5
            //region.RegionIncharge = vmCommonRegion.RegionIncharge;
            //region.SalesOfficerName = vmCommonRegion.SalesOfficerName;
            //region.Designation = vmCommonRegion.Designation;
            //region.Email = vmCommonRegion.Email;
            //region.MobilePersonal = vmCommonRegion.MobilePersonal;
            //region.MobileOffice = vmCommonRegion.MobileOffice;
            region.EmployeeId = vmCommonRegion.EmployeeId;
            region.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            region.ModifiedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = region.RegionId;
            }
            return result;
        }

        public async Task<int> RegionDelete(int id)
        {
            int result = -1;
            if (id != 0)
            {
                Region region = await _db.Regions.FindAsync(id);
                region.IsActive = false;
                region.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                region.ModifiedDate = DateTime.Now;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = region.RegionId;
                }
            }
            return result;
        }
        public async Task<bool> CheckDuplicateRegionName(int zoneId, int zoneDivisionId, string regionName, int id)
        {
            bool isExist = false;
            if (string.IsNullOrEmpty(regionName))
            {
                return isExist;
            }
            if (id > 0)
            {
                isExist = await _db.Regions.AnyAsync(u => u.Name.Equals(regionName) && u.ZoneId == zoneId && u.ZoneDivisionId == zoneDivisionId && u.RegionId != id && u.IsActive == true);

            }
            else
            {
                isExist = await _db.Regions.AnyAsync(u => u.Name.Equals(regionName) && u.ZoneId == zoneId && u.ZoneDivisionId == zoneDivisionId && u.IsActive == true);
            }

            return isExist;
        }
        #endregion

        #region Area
        public List<object> CommonAreaDropDownList(int companyId, int zoneId = 0, int zoneDivisionId = 0, int regionId = 0)
        {
            var list = new List<object>();
            var v = _db.Areas.Where(x =>
            x.IsActive == true && x.CompanyId == companyId
            && (zoneId > 0 && zoneDivisionId > 0 && regionId > 0 ? x.ZoneId == zoneId && x.ZoneDivisionId == zoneDivisionId && x.RegionId == regionId
            : x.AreaId > 0)).ToList();

            foreach (var x in v)
            {
                list.Add(new { Text = x.Name, Value = x.AreaId });
            }

            return list;
        }

        public List<SelectModel> GetAreaSelectList(int companyId, int? zoneId, int? zoneDivisionId, int? regionId)
        {
            List<SelectModel> selectModelList = new List<SelectModel>();
            SelectModel selectModel = new SelectModel
            {
                Text = "==Select Area==",
                Value = 0,
            };
            selectModelList.Add(selectModel);

            if (zoneId.HasValue && zoneId > 0 && zoneDivisionId > 0 && regionId > 0)
            {
                var v = _db.Areas.Where(x => x.CompanyId == companyId && x.ZoneId == zoneId && x.ZoneDivisionId == zoneDivisionId && x.RegionId == regionId && x.IsActive == true).ToList()
                    .Select(x => new SelectModel()
                    {
                        Text = x.Name,
                        Value = x.AreaId
                    }).ToList();
                selectModelList.AddRange(v);
            }
            else if (zoneId.HasValue && zoneId > 0 && zoneDivisionId > 0)
            {
                var v = _db.Areas.Where(x => x.CompanyId == companyId && x.ZoneId == zoneId && x.ZoneDivisionId == zoneDivisionId && x.IsActive == true).ToList()
                    .Select(x => new SelectModel()
                    {
                        Text = x.Name,
                        Value = x.AreaId
                    }).ToList();
                selectModelList.AddRange(v);
            }
            else if (zoneId.HasValue && zoneId > 0)
            {
                var v = _db.Areas.Where(x => x.CompanyId == companyId && x.ZoneId == zoneId && x.IsActive == true).ToList()
                    .Select(x => new SelectModel()
                    {
                        Text = x.Name,
                        Value = x.AreaId
                    }).ToList();
                selectModelList.AddRange(v);
            }
            else
            {
                var v = _db.Areas.Where(x => x.CompanyId == companyId && x.IsActive == true).ToList()
                    .Select(x => new SelectModel()
                    {
                        Text = x.Name,
                        Value = x.AreaId
                    }).ToList();
                selectModelList.AddRange(v);
            }

            return selectModelList;
        }

        public async Task<VMCommonArea> GetAreas(int companyId, int zoneId = 0, int zoneDivisionId = 0, int regionId = 0)
        {
            VMCommonArea vmCommonArea = new VMCommonArea();
            vmCommonArea.CompanyFK = companyId;
            vmCommonArea.DataList = await Task.Run(() => (from t1 in _db.Areas
                                                          join t2 in _db.Zones on t1.ZoneId equals t2.ZoneId into t2_Zones
                                                          from t2 in t2_Zones.DefaultIfEmpty()
                                                          join t3 in _db.ZoneDivisions on t1.ZoneDivisionId equals t3.ZoneDivisionId into t3_ZoneDivision
                                                          from t3 in t3_ZoneDivision.DefaultIfEmpty()
                                                          join t4 in _db.Regions on t1.RegionId equals t4.RegionId into t4_Region
                                                          from t4 in t4_Region.DefaultIfEmpty()
                                                          where t1.IsActive == true && t1.CompanyId == companyId
                                                          && (zoneId > 0 && zoneDivisionId > 0 && regionId > 0 ? t1.ZoneId == zoneId && t1.ZoneDivisionId == zoneDivisionId && t1.RegionId == regionId : t1.AreaId > 0)
                                                          select new VMCommonArea
                                                          {
                                                              ID = t1.AreaId,
                                                              ZoneId = t1.ZoneId ?? 0,
                                                              ZoneName = t2.Name,
                                                              ZoneDivisionId = t1.ZoneDivisionId ?? 0,
                                                              ZoneDivisionName = t3.Name,
                                                              RegionId = t1.RegionId ?? 0,
                                                              RegionName = t4.Name,
                                                              Name = t1.Name,
                                                              Code = t1.Code,
                                                              AreaIncharge = t1.AreaIncharge,
                                                              Designation = t1.Designation,
                                                              Email = t1.Email,
                                                              MobileOffice = t1.MobileOffice,
                                                              MobilePersonal = t1.MobilePersonal,
                                                              CompanyFK = t1.CompanyId,
                                                              EmployeeId = t1.EmployeeId
                                                          }).OrderByDescending(x => x.ID).AsEnumerable());
            return vmCommonArea;
        }

        public async Task<int> AreaAdd(VMCommonArea vmCommonArea)
        {
            var result = -1;

            #region check Area Duplicate
            var isExist = await _db.Areas.FirstOrDefaultAsync(u => u.Name.Equals(vmCommonArea.Name) && u.ZoneId == vmCommonArea.ZoneId && u.ZoneDivisionId == vmCommonArea.ZoneDivisionId && u.RegionId == vmCommonArea.RegionId && u.IsActive == true);
            if (isExist?.AreaId > 0)
            {
                throw new Exception($"Sorry! This Name {vmCommonArea.Name} already Exists!");
            }
            #endregion

            Area area = new Area
            {
                Name = vmCommonArea.Name,
                // Code = vmCommonZoneDivision.Code, // Don't use zoneDivision it will add form Head5
                AreaIncharge = vmCommonArea.AreaIncharge,
                Designation = vmCommonArea.Designation,
                Email = vmCommonArea.Email,
                MobileOffice = vmCommonArea.MobileOffice,
                MobilePersonal = vmCommonArea.MobilePersonal,
                ZoneId = vmCommonArea.ZoneId,
                ZoneDivisionId = vmCommonArea.ZoneDivisionId,
                RegionId = vmCommonArea.RegionId,
                EmployeeId = vmCommonArea.EmployeeId,
                CompanyId = vmCommonArea.CompanyFK.Value,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true

            };
            _db.Areas.Add(area);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = area.AreaId;
            }

            return result;
        }

        public async Task<int> AreaEdit(VMCommonArea vmCommonArea)
        {
            var result = -1;

            #region check Area Duplicate
            var isExist = await _db.Areas.FirstOrDefaultAsync(u => u.Name.Equals(vmCommonArea.Name) && u.ZoneId == vmCommonArea.ZoneId && u.ZoneDivisionId == vmCommonArea.ZoneDivisionId && u.RegionId == vmCommonArea.RegionId && u.AreaId != vmCommonArea.ID && u.IsActive == true);
            if (isExist?.AreaId > 0)
            {
                throw new Exception($"Sorry! This Name {vmCommonArea.Name} already Exists!");
            }
            #endregion

            Area area = await _db.Areas.FindAsync(vmCommonArea.ID);
            area.ZoneId = vmCommonArea.ZoneId;
            area.ZoneDivisionId = vmCommonArea.ZoneDivisionId;
            area.RegionId = vmCommonArea.RegionId;
            area.Name = vmCommonArea.Name;
            // zoneDivision.Code = vmCommonZoneDivision.Code; // Don't use zoneDivision it will add form Head5
            //area.RegionIncharge = vmCommonRegion.RegionIncharge;
            //area.SalesOfficerName = vmCommonRegion.SalesOfficerName;
            //area.Designation = vmCommonRegion.Designation;
            //area.Email = vmCommonRegion.Email;
            //area.MobilePersonal = vmCommonRegion.MobilePersonal;
            //area.MobileOffice = vmCommonRegion.MobileOffice;
            area.EmployeeId = vmCommonArea.EmployeeId;
            area.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            area.ModifiedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = area.AreaId;
            }
            return result;
        }

        public async Task<int> AreaDelete(int id)
        {
            int result = -1;
            if (id != 0)
            {
                Area area = await _db.Areas.FindAsync(id);
                area.IsActive = false;
                area.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                area.ModifiedDate = DateTime.Now;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = area.AreaId;
                }
            }
            return result;
        }
        public async Task<bool> CheckDuplicateAreaName(int zoneId, int zoneDivisionId, int regionId, string areaName, int id)
        {
            bool isExist = false;
            if (string.IsNullOrEmpty(areaName))
            {
                return isExist;
            }
            if (id > 0)
            {
                isExist = await _db.Areas.AnyAsync(u => u.Name.Equals(areaName) && u.ZoneId == zoneId && u.ZoneDivisionId == zoneDivisionId && u.RegionId == regionId && u.AreaId != id && u.IsActive == true);

            }
            else
            {
                isExist = await _db.Areas.AnyAsync(u => u.Name.Equals(areaName) && u.ZoneId == zoneId && u.ZoneDivisionId == zoneDivisionId && u.RegionId == regionId && u.IsActive == true);
            }

            return isExist;
        }
        #endregion

        #region SubZone/Territory

        public List<SelectModel> GetSubZoneSelectList(int companyId, int? zoneId, int? zoneDivisionId, int? regionId = 0, int? areaId = 0)
        {
            List<SelectModel> selectModelList = new List<SelectModel>();
            SelectModel selectModel = new SelectModel
            {
                Text = "==Select Territory==",
                Value = 0,
            };
            selectModelList.Add(selectModel);

            if (zoneId.HasValue && zoneId > 0 && zoneDivisionId <= 0)
            {
                var v = _db.SubZones.Where(x => x.CompanyId == companyId && x.ZoneId == zoneId && x.IsActive == true).ToList()
                    .Select(x => new SelectModel()
                    {
                        Text = x.Name,
                        Value = x.SubZoneId
                    }).ToList();
                selectModelList.AddRange(v);
            }
            else if (zoneId > 0 && zoneDivisionId > 0)
            {
                var v = _db.SubZones.Where(x => x.CompanyId == companyId && x.ZoneId == zoneId && x.ZoneDivisionId == zoneDivisionId && x.IsActive == true).ToList()
                    .Select(x => new SelectModel()
                    {
                        Text = x.Name,
                        Value = x.SubZoneId
                    }).ToList();
                selectModelList.AddRange(v);
            }
            else if (zoneId > 0 && zoneDivisionId > 0 && regionId > 0)
            {
                var v = _db.SubZones.Where(x => x.CompanyId == companyId && x.ZoneId == zoneId && x.ZoneDivisionId == zoneDivisionId && x.RegionId == regionId && x.IsActive == true).ToList()
                    .Select(x => new SelectModel()
                    {
                        Text = x.Name,
                        Value = x.SubZoneId
                    }).ToList();
                selectModelList.AddRange(v);
            }
            else if (zoneId > 0 && zoneDivisionId > 0 && regionId > 0 && areaId > 0)
            {
                var v = _db.SubZones.Where(x => x.CompanyId == companyId && x.ZoneId == zoneId && x.ZoneDivisionId == zoneDivisionId && x.RegionId == regionId && x.AreaId == areaId && x.IsActive == true).ToList()
                    .Select(x => new SelectModel()
                    {
                        Text = x.Name,
                        Value = x.SubZoneId
                    }).ToList();
                selectModelList.AddRange(v);
            }
            else
            {
                var v = _db.SubZones.Where(x => x.CompanyId == companyId && x.IsActive == true).ToList()
                    .Select(x => new SelectModel()
                    {
                        Text = x.Name,
                        Value = x.SubZoneId
                    }).ToList();
                selectModelList.AddRange(v);
            }

            return selectModelList;
        }

        public async Task<VMCommonSubZone> GetSubZones(int companyId, int zoneId = 0, int zoneDivisionId = 0, int areaId = 0)
        {
            VMCommonSubZone vmCommonSubZone = new VMCommonSubZone();
            vmCommonSubZone.CompanyFK = companyId;
            vmCommonSubZone.DataList = await Task.Run(() => (from t1 in _db.SubZones
                                                             join t2 in _db.Zones on t1.ZoneId equals t2.ZoneId into t2_Join
                                                             from t2 in t2_Join.DefaultIfEmpty()
                                                             join t3 in _db.ZoneDivisions on t1.ZoneDivisionId equals t3.ZoneDivisionId into t3_Join
                                                             from t3 in t3_Join.DefaultIfEmpty()
                                                             join t4 in _db.Regions on t1.RegionId equals t4.RegionId into t4_Join
                                                             from t4 in t4_Join.DefaultIfEmpty()
                                                             join t5 in _db.Areas on t1.AreaId equals t5.AreaId into t5_Join
                                                             from t5 in t5_Join.DefaultIfEmpty()
                                                             where t1.IsActive == true && t1.CompanyId == companyId
                                                             && (zoneId > 0 && zoneDivisionId > 0 && areaId > 0 ? t1.ZoneId == zoneId && t1.ZoneDivisionId == zoneDivisionId && t1.RegionId == areaId : t1.SubZoneId > 0)
                                                             select new VMCommonSubZone
                                                             {
                                                                 ID = t1.SubZoneId,
                                                                 ZoneId = t1.ZoneId,
                                                                 ZoneName = t2.Name,
                                                                 ZoneDivisionId = t1.ZoneDivisionId,
                                                                 ZoneDivisionName = t3.Name,
                                                                 RegionId = t1.RegionId,
                                                                 RegionName = t4.Name,
                                                                 AreaId = t5.AreaId,
                                                                 AreaName = t5.Name,
                                                                 Name = t1.Name,
                                                                 Code = t1.Code,
                                                                 SalesOfficerName = t1.SalesOfficerName,
                                                                 Designation = t1.Designation,
                                                                 Email = t1.Email,
                                                                 MobileOffice = t1.MobileOffice,
                                                                 MobilePersonal = t1.MobilePersonal,
                                                                 CompanyFK = t1.CompanyId,
                                                                 EmployeeId = t1.EmployeeId
                                                             }).OrderByDescending(x => x.ID).AsEnumerable());
            return vmCommonSubZone;
        }

        public async Task<int> SubZoneAdd(VMCommonSubZone vmCommonSubZone)
        {
            var result = -1;

            #region check SubZone Duplicate
            var isExist = await _db.SubZones.FirstOrDefaultAsync(u => u.Name.Equals(vmCommonSubZone.Name) && u.ZoneId == vmCommonSubZone.ZoneId && u.ZoneDivisionId == vmCommonSubZone.ZoneDivisionId && u.RegionId == vmCommonSubZone.RegionId && u.AreaId == vmCommonSubZone.AreaId && u.IsActive == true);
            if (isExist?.SubZoneId > 0)
            {
                throw new Exception($"Sorry! This Name {vmCommonSubZone.Name} already Exists!");
            }
            #endregion


            SubZone subZone = new SubZone
            {
                Name = vmCommonSubZone.Name,
                // Code = vmCommonSubZone.Code, // Don't use code, it will add form Head5
                SalesOfficerName = vmCommonSubZone.SalesOfficerName,
                Designation = vmCommonSubZone.Designation,
                Email = vmCommonSubZone.Email,
                MobileOffice = vmCommonSubZone.MobileOffice,
                MobilePersonal = vmCommonSubZone.MobilePersonal,
                ZoneId = vmCommonSubZone.ZoneId,
                ZoneDivisionId = vmCommonSubZone.ZoneDivisionId,
                RegionId = vmCommonSubZone.RegionId,
                AreaId = vmCommonSubZone.AreaId,
                EmployeeId = vmCommonSubZone.EmployeeId,
                CompanyId = vmCommonSubZone.CompanyFK.Value,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true

            };
            _db.SubZones.Add(subZone);
            try
            {
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = subZone.SubZoneId;

                    Zone zone = await _db.Zones.FindAsync(subZone.ZoneId);

                    VMHeadIntegration integration = new VMHeadIntegration
                    {
                        AccName = subZone.Name,
                        LayerNo = 5,
                        Remarks = "5th Layer",
                        IsIncomeHead = false,
                        ParentId = zone.HeadGLId, // See Account Receivable Id
                        IsActive = true,

                        CompanyFK = subZone.CompanyId,
                        CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                        CreatedDate = DateTime.Now,
                    };
                    if (zone.CompanyId == (int)CompanyName.KrishibidSeedLimited)
                    {
                        Head5 head5 = await Head5IntegrationAdd(integration);
                        if (head5 != null)
                        {
                            await SubZoneCodeAndHead5IdEdit(subZone.SubZoneId, head5);
                        }
                    }
                }
            }
            catch
            {

            }

            return result;
        }
        public async Task<Head5> Head5IntegrationAdd(VMHeadIntegration vmHeadIntegration)
        {
            long result = -1;

            string newAccountCode = "";
            int orderNo = 0;
            Head4 parentHead = _db.Head4.FirstOrDefault(x => x.Id == vmHeadIntegration.ParentId);

            IQueryable<Head5> childHeads = _db.Head5.Where(x => x.ParentId == vmHeadIntegration.ParentId);

            if (childHeads.Count() > 0)
            {
                string lastAccCode = childHeads.OrderByDescending(x => x.AccCode).FirstOrDefault()?.AccCode;
                string parentPart = lastAccCode?.Substring(0, 7);
                string childPart = lastAccCode?.Substring(7, 3);
                newAccountCode = parentPart + (Convert.ToInt32(childPart) + 1).ToString().PadLeft(3, '0');
                orderNo = childHeads.Count();
            }
            else
            {
                newAccountCode = parentHead?.AccCode + "001";
                orderNo += 1;
            }


            Head5 head5 = new Head5
            {
                Id = _db.Database.SqlQuery<int>("spGetNewId").FirstOrDefault(),
                AccCode = newAccountCode,
                LayerNo = vmHeadIntegration.LayerNo,

                CompanyId = vmHeadIntegration.CompanyFK,
                CreateDate = vmHeadIntegration.CreatedDate,
                CreatedBy = vmHeadIntegration.CreatedBy,
                AccName = vmHeadIntegration.AccName,
                ParentId = vmHeadIntegration.ParentId,
                OrderNo = orderNo,
                Remarks = vmHeadIntegration.Remarks,
                IsActive = true

            };
            _db.Head5.Add(head5);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = head5.Id;
            }
            return head5;
        }

        public async Task<int> SubZoneCodeAndHead5IdEdit(int subZoneId, Head5 head5)
        {
            var result = -1;

            SubZone subZone = await _db.SubZones.FindAsync(subZoneId);
            subZone.AccountHeadId = head5.Id;
            subZone.Code = head5.AccCode;

            subZone.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            subZone.ModifiedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = subZone.ZoneId;
            }
            return result;
        }

        public async Task<int> SubZonesEdit(VMCommonSubZone vmCommonSubZone)
        {
            var result = -1;

            #region check SubZone Duplicate
            var isExist = await _db.SubZones.FirstOrDefaultAsync(u => u.Name.Equals(vmCommonSubZone.Name) && u.ZoneId == vmCommonSubZone.ZoneId && u.ZoneDivisionId == vmCommonSubZone.ZoneDivisionId && u.RegionId == vmCommonSubZone.RegionId && u.AreaId == vmCommonSubZone.AreaId && u.SubZoneId != vmCommonSubZone.ID && u.IsActive == true);
            if (isExist?.SubZoneId > 0)
            {
                throw new Exception($"Sorry! This Name {vmCommonSubZone.Name} already Exists!");
            }
            #endregion

            SubZone subZone = await _db.SubZones.FindAsync(vmCommonSubZone.ID);
            subZone.ZoneId = vmCommonSubZone.ZoneId;
            subZone.ZoneDivisionId = vmCommonSubZone.ZoneDivisionId;
            subZone.RegionId = vmCommonSubZone.RegionId;
            subZone.AreaId = vmCommonSubZone.AreaId;
            subZone.Name = vmCommonSubZone.Name;
            //subZone.Code = vmCommonSubZone.Code; // Don't use code, it will add form Head5
            //subZone.SalesOfficerName = vmCommonSubZone.SalesOfficerName;
            //subZone.Designation = vmCommonSubZone.Designation;
            //subZone.Email = vmCommonSubZone.Email;
            //subZone.MobilePersonal = vmCommonSubZone.MobilePersonal;
            //subZone.MobileOffice = vmCommonSubZone.MobileOffice;
            subZone.EmployeeId = vmCommonSubZone.EmployeeId;
            subZone.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            subZone.ModifiedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = subZone.SubZoneId;
                UpdateSubZoneForSeed(subZone);
            }
            return result;
        }

        private void UpdateSubZoneForSeed(SubZone subZone)
        {
            Head5 head5 = _db.Head5.Find(subZone.AccountHeadId);
            head5.AccName = subZone.Name;
            head5.AccCode = subZone.Code;
            head5.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            head5.ModifiedDate = DateTime.Now;
            _db.SaveChanges();
        }

        private void DeleteSubZoneForSeed(SubZone subZone)
        {
            Head5 head5 = _db.Head5.Find(subZone.AccountHeadId);
            head5.IsActive = false;
            head5.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            head5.ModifiedDate = DateTime.Now;
            _db.SaveChanges();
        }

        public async Task<int> SubZonesDelete(int id)
        {
            int result = -1;
            if (id != 0)
            {
                SubZone subZone = await _db.SubZones.FindAsync(id);
                subZone.IsActive = false;
                subZone.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                subZone.ModifiedDate = DateTime.Now;

                if (await _db.SaveChangesAsync() > 0)
                {
                    result = subZone.ZoneId;

                    DeleteSubZoneForSeed(subZone);
                }
            }
            return result;
        }
        public async Task<bool> CheckDuplicateSubZoneName(int zoneId, int zoneDivisionId, int regionId, int areaId, string subZoneName, int id)
        {
            bool isExist = false;
            if (string.IsNullOrEmpty(subZoneName))
            {
                return isExist;
            }
            if (id > 0)
            {
                isExist = await _db.SubZones.AnyAsync(u => u.Name.Equals(subZoneName) && u.ZoneId == zoneId && u.ZoneDivisionId == zoneDivisionId && u.RegionId == regionId && u.AreaId == areaId && u.SubZoneId != id && u.IsActive == true);
            }
            else
            {
                isExist = await _db.SubZones.AnyAsync(u => u.Name.Equals(subZoneName) && u.ZoneId == zoneId && u.ZoneDivisionId == zoneDivisionId && u.RegionId == regionId && u.AreaId == areaId && u.IsActive == true);
            }

            return isExist;
        }

        #endregion

        #region Product Category 
        public async Task<VMCommonProductCategory> GetFinishProductCategory(int companyId, string productType)
        {
            VMCommonProductCategory vmCommonProductCategory = new VMCommonProductCategory();
            vmCommonProductCategory.CompanyFK = companyId;
            vmCommonProductCategory.DataList = await Task.Run(() => (from t1 in _db.ProductCategories
                                                                     where t1.ProductType == productType &&
                                                                     t1.IsActive == true && t1.CompanyId == companyId
                                                                     // productCategoryId > 0 ? t1.ProductCategoryId == productCategoryId: t1.ProductCategoryId > 0
                                                                     select new VMCommonProductCategory
                                                                     {
                                                                         ID = t1.ProductCategoryId,
                                                                         Name = t1.Name,
                                                                         ProductType = t1.ProductType,
                                                                         CompanyFK = t1.CompanyId,
                                                                         CashCommission = t1.CashCustomerRate,
                                                                         Remarks = t1.Remarks
                                                                     }).OrderByDescending(x => x.ID).AsEnumerable());
            return vmCommonProductCategory;
        }
        public async Task<int> ProductFinishCategoryAdd(VMCommonProductCategory productCategoryModel)
        {
            var result = -1;

            #region IsExist
            var isExist = _db.ProductCategories.FirstOrDefault(c => c.Name.Equals(productCategoryModel.Name) && c.IsActive == true);
            if (isExist?.ProductCategoryId > 0)
            {
                throw new Exception($"Sorry! This Name {productCategoryModel.Name} already Exists!");
            }
            #endregion

            ProductCategory productCategory = new ProductCategory
            {
                Name = productCategoryModel.Name,
                ProductType = productCategoryModel.ProductType,
                CashCustomerRate = productCategoryModel.CashCommission,

                Remarks = productCategoryModel.Remarks,
                CompanyId = productCategoryModel.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true

            };
            _db.ProductCategories.Add(productCategory);
            if (await _db.SaveChangesAsync() > 0)
            {
                if (productCategory.CompanyId == (int)CompanyName.GloriousCropCareLimited)
                {

                    var category = await _db.ProductCategories.FindAsync(productCategory.ProductCategoryId);
                    VMHeadIntegration integration = new VMHeadIntegration
                    {
                        AccName = category.Name,
                        LayerNo = 5,
                        Remarks = "5 Layer",
                        IsIncomeHead = false,
                        ProductType = category.ProductType,
                        CompanyFK = productCategory.CompanyId,
                        CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                        CreatedDate = DateTime.Now,
                    };


                    int head5Id = AccHead5Push(integration, productCategory.ProductCategoryId);

                    //if (headGlId != null)
                    //{
                    //    await GLDLBlockCodeAndHeadGLIdEdit(commonProductSubCategory.ProductSubCategoryId, headGlId, head5Id);
                    //}
                }

                if (productCategory.CompanyId == (int)CompanyName.KrishibidFeedLimited)
                {

                    var category = await _db.ProductCategories.FindAsync(productCategory.ProductCategoryId);
                    VMHeadIntegration integration = new VMHeadIntegration
                    {
                        AccName = category.Name,
                        LayerNo = 5,
                        Remarks = "5 Layer",
                        IsIncomeHead = false,
                        ProductType = category.ProductType,
                        CompanyFK = productCategory.CompanyId,
                        CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                        CreatedDate = DateTime.Now,
                    };


                    int head5Id = AccHead5Push(integration, productCategory.ProductCategoryId);

                    //if (headGlId != null)
                    //{
                    //    await GLDLBlockCodeAndHeadGLIdEdit(commonProductSubCategory.ProductSubCategoryId, headGlId, head5Id);
                    //}
                }
                if (productCategory.CompanyId == (int)CompanyName.KrishibidSeedLimited)
                {

                    var category = await _db.ProductCategories.FindAsync(productCategory.ProductCategoryId);
                    VMHeadIntegration integration = new VMHeadIntegration
                    {
                        ID = productCategory.ProductCategoryId,
                        AccName = category.Name,
                        LayerNo = 6,
                        Remarks = "GL Layer",
                        IsIncomeHead = false,
                        ProductType = category.ProductType,
                        CompanyFK = productCategory.CompanyId,
                        CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                        CreatedDate = DateTime.Now,
                    };

                    int head5Id = SeedAccHeadGlPush(integration);

                    //if (headGlId != null)
                    //{
                    //    await GLDLBlockCodeAndHeadGLIdEdit(commonProductSubCategory.ProductSubCategoryId, headGlId, head5Id);
                    //}
                }

                result = productCategory.ProductCategoryId;
            }
            return result;
        }
        public async Task<int> ProductFinishCategoryEdit(VMCommonProductCategory vmCommonProductCategory)
        {
            var result = -1;

            #region IsExist
            var isExist = _db.ProductCategories.FirstOrDefault(c => c.Name.Equals(vmCommonProductCategory.Name) && c.ProductCategoryId != vmCommonProductCategory.ID && c.IsActive == true);
            if (isExist?.ProductCategoryId > 0)
            {
                throw new Exception($"Sorry! This Name {vmCommonProductCategory.Name} already Exists!");
            }
            #endregion

            ProductCategory productCategory = await _db.ProductCategories.FindAsync(vmCommonProductCategory.ID);
            productCategory.Name = vmCommonProductCategory.Name;
            productCategory.CashCustomerRate = vmCommonProductCategory.CashCommission;

            productCategory.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            productCategory.ModifiedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = productCategory.ProductCategoryId;
            }
            return result;
        }
        public async Task<int> ProductFinishCategoryDelete(int id)
        {
            int result = -1;
            if (id != 0)
            {
                ProductCategory productCategory = await _db.ProductCategories.FindAsync(id);
                productCategory.IsActive = false;
                productCategory.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                productCategory.ModifiedDate = DateTime.Now;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = productCategory.ProductCategoryId;
                }
            }
            return result;
        }

        public async Task<bool> CheckProductCategoryName(string name, int id)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            bool isExist = false;
            if (id > 0)
            {
                isExist = await _db.ProductCategories.AnyAsync(u => u.Name.Equals(name) && u.ProductCategoryId != id && u.IsActive == true);
            }
            else
            {
                isExist = await _db.ProductCategories.AnyAsync(u => u.Name.Equals(name) && u.IsActive == true);
            }
            return isExist;
        }

        #endregion

        #region Common Product Subcategory
        public async Task<VMCommonProductSubCategory> GetProductSubCategory(int companyId, int categoryId, string productType)
        {
            VMCommonProductSubCategory vmCommonProductSubCategory = new VMCommonProductSubCategory();
            if (categoryId > 0)
            {
                vmCommonProductSubCategory = await Task.Run(() => (from t1 in _db.ProductCategories.Where(x => x.ProductCategoryId == categoryId && x.IsActive == true)

                                                                   select new VMCommonProductSubCategory
                                                                   {

                                                                       Common_ProductCategoryFk = t1.ProductCategoryId,
                                                                       CategoryName = t1.Name,
                                                                       BaseCommissionRate = t1.CashCustomerRate,
                                                                       CompanyFK = t1.CompanyId,
                                                                       ProductType = t1.ProductType,
                                                                       Description = t1.Address,
                                                                       Code = t1.Code
                                                                   }).FirstOrDefault());
            }
            else if (categoryId is 0)
            {
                vmCommonProductSubCategory = await Task.Run(() => (from t1 in _db.ProductCategories.Where(x => x.IsActive == true)

                                                                   select new VMCommonProductSubCategory
                                                                   {

                                                                       Common_ProductCategoryFk = t1.ProductCategoryId,
                                                                       CategoryName = t1.Name,
                                                                       BaseCommissionRate = t1.CashCustomerRate,
                                                                       CompanyFK = t1.CompanyId,
                                                                       ProductType = t1.ProductType,
                                                                       Description = t1.Address,
                                                                       Code = t1.Code
                                                                   }).FirstOrDefault());
            }

            else
            {
                vmCommonProductSubCategory.CompanyFK = companyId;
                vmCommonProductSubCategory.ProductType = productType;

            }
            vmCommonProductSubCategory.DataList = await Task.Run(() => (from t1 in _db.ProductSubCategories
                                                                        join t2 in _db.ProductCategories on t1.ProductCategoryId equals t2.ProductCategoryId
                                                                        where t1.IsActive == true && t2.IsActive == true && t1.CompanyId == companyId
                                                                        && t1.ProductType == productType
                                                                        && ((categoryId > 0) ? t1.ProductCategoryId == categoryId : t1.ProductCategoryId > 0)
                                                                        select new VMCommonProductSubCategory
                                                                        {
                                                                            Common_ProductCategoryFk = t2.ProductCategoryId,
                                                                            CategoryName = t2.Name,
                                                                            ProductType = t1.ProductType,
                                                                            CompanyFK = t1.CompanyId,
                                                                            ID = t1.ProductSubCategoryId,
                                                                            Name = t1.Name,
                                                                            Code = t1.Code,
                                                                            BaseCommissionRate = t1.BaseCommissionRate,
                                                                        }).OrderByDescending(x => x.ID).AsEnumerable());
            return vmCommonProductSubCategory;
        }
        public async Task<int> ProductSubCategoryAdd(VMCommonProductSubCategory vmCommonProductSubCategory)
        {
            var result = -1;
            #region IsExist
            var isExist = _db.ProductSubCategories.FirstOrDefault(c => c.Name.Equals(vmCommonProductSubCategory.Name) && c.ProductCategoryId == vmCommonProductSubCategory.CategoryId && c.IsActive == true);

            if (isExist?.ProductSubCategoryId > 0)
            {
                throw new Exception($"Sorry! This Name {vmCommonProductSubCategory.Name} already Exists!");
            }
            #endregion

            ProductSubCategory commonProductSubCategory = new ProductSubCategory
            {
                Name = vmCommonProductSubCategory.Name,
                Code = vmCommonProductSubCategory.Code,
                ProductCategoryId = vmCommonProductSubCategory.CategoryId,
                BaseCommissionRate = vmCommonProductSubCategory.BaseCommissionRate,
                ProductType = vmCommonProductSubCategory.ProductType,
                CompanyId = vmCommonProductSubCategory.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _db.ProductSubCategories.Add(commonProductSubCategory);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = commonProductSubCategory.ProductSubCategoryId;

                if (commonProductSubCategory.CompanyId == (int)CompanyName.GloriousCropCareLimited)
                {

                    ProductSubCategory subCategory = await _db.ProductSubCategories.FindAsync(commonProductSubCategory.ProductSubCategoryId);

                    VMHeadIntegration integration = new VMHeadIntegration
                    {
                        AccName = subCategory.Name,
                        LayerNo = 6,
                        Remarks = "6 Layer",
                        IsIncomeHead = false,
                        ProductType = subCategory.ProductType,
                        CompanyFK = commonProductSubCategory.CompanyId,
                        CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                        CreatedDate = DateTime.Now
                    };


                    int headGl = AccHeadGlPush(integration, commonProductSubCategory);

                    //if (headGlId != null)
                    //{
                    //    await GLDLBlockCodeAndHeadGLIdEdit(commonProductSubCategory.ProductSubCategoryId, headGlId, head5Id);
                    //}
                }


                if (commonProductSubCategory.CompanyId == (int)CompanyName.GloriousLandsAndDevelopmentsLimited)
                {
                    int head5Id = BlockHead5Push(commonProductSubCategory);
                    var catetegory = await _db.ProductCategories.FindAsync(commonProductSubCategory.ProductCategoryId);
                    VMHeadIntegration integration = new VMHeadIntegration
                    {
                        AccName = catetegory.Name + " - " + commonProductSubCategory.Name,
                        LayerNo = 6,
                        Remarks = "GL Layer",
                        IsIncomeHead = false,
                        ParentId = 50602122,

                        CompanyFK = commonProductSubCategory.CompanyId,
                        CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                        CreatedDate = DateTime.Now,
                    };
                    HeadGL headGlId = await PayableHeadIntegrationAdd(integration);
                    if (headGlId != null)
                    {
                        await GLDLBlockCodeAndHeadGLIdEdit(commonProductSubCategory.ProductSubCategoryId, headGlId, head5Id);
                    }
                }
            }



            return result;
        }
        public async Task<int> ProductSubCategoryEdit(VMCommonProductSubCategory vmCommonProductSubCategory)
        {

            var result = -1;

            var isExist = await _db.ProductSubCategories.FirstOrDefaultAsync(c => c.Name.Equals(vmCommonProductSubCategory.Name) && c.ProductCategoryId == vmCommonProductSubCategory.Common_ProductCategoryFk && c.ProductSubCategoryId != vmCommonProductSubCategory.CategoryId && c.IsActive == true);

            if (isExist?.ProductSubCategoryId > 0)
            {
                throw new Exception($"Sorry! This Name {vmCommonProductSubCategory.Name} already Exists!");
            }

            ProductSubCategory commonProductSubCategory = await _db.ProductSubCategories.FindAsync(vmCommonProductSubCategory.ID);
            commonProductSubCategory.Name = vmCommonProductSubCategory.Name;
            commonProductSubCategory.Code = vmCommonProductSubCategory.Code;
            commonProductSubCategory.ProductCategoryId = vmCommonProductSubCategory.Common_ProductCategoryFk;
            commonProductSubCategory.BaseCommissionRate = vmCommonProductSubCategory.BaseCommissionRate;

            commonProductSubCategory.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            commonProductSubCategory.ModifiedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = commonProductSubCategory.ProductSubCategoryId;
            }
            return result;
        }
        public async Task<int> ProductSubCategoryDelete(int id)
        {
            int result = -1;

            if (id != 0)
            {
                ProductSubCategory commonProductSubCategory = await _db.ProductSubCategories.FindAsync(id);
                commonProductSubCategory.IsActive = false;
                commonProductSubCategory.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                commonProductSubCategory.ModifiedDate = DateTime.Now;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = commonProductSubCategory.ProductSubCategoryId;
                }
            }
            return result;
        }
        public async Task<bool> IsSubCategoryExits(string name, int categoryId, int id)
        {
            var isExits = false;

            if (id > 0)
            {
                isExits = await _db.ProductSubCategories.AnyAsync(c => c.Name.Equals(name) && c.ProductCategoryId == categoryId && c.ProductSubCategoryId != id && c.IsActive == true);

            }
            else if (categoryId > 0)
            {
                isExits = await _db.ProductSubCategories.AnyAsync(c => c.Name.Equals(name) && c.ProductCategoryId == categoryId && c.IsActive == true);

            }


            return isExits;

        }
        #endregion

        public class CustomerPaymentType
        {
            public string Text { get; set; }
            public string Value { get; set; }

        }
        public class EnumModel
        {
            public int Value { get; set; }
            public string Text { get; set; }
        }
        public List<object> CommonCountriesDropDownList()
        {
            var list = new List<object>();
            var v = _db.Countries.ToList();
            foreach (var x in v)
            {
                list.Add(new { Text = x.CountryName, Value = x.CountryId });
            }
            return list;
        }
        public List<object> CommonDistrictsDropDownList()
        {
            var list = new List<object>();
            var v = _db.Districts.Where(a => a.IsActive == true).ToList();
            foreach (var x in v)
            {
                list.Add(new { Text = x.Name, Value = x.DistrictId });
            }
            return list;
        }
        public List<object> CommonUpazilasDropDownList()
        {
            var list = new List<object>();
            var v = _db.Upazilas.Where(a => a.IsActive == true).ToList();
            foreach (var x in v)
            {
                list.Add(new { Text = x.Name, Value = x.UpazilaId });
            }
            return list;
        }
        public List<object> CommonDivisionsDropDownList()
        {
            var list = new List<object>();
            var v = _db.Divisions.ToList();
            foreach (var x in v)
            {
                list.Add(new { Text = x.Name, Value = x.DivisionId });
            }
            return list;
        }
        public List<object> CommonDeportDropDownList()
        {
            var list = new List<object>();
            var v = _db.Vendors.Where(c => c.VendorTypeId == (int)Provider.Deport && c.IsActive == true).ToList();
            foreach (var x in v)
            {
                list.Add(new { Text = x.Name, Value = x.VendorId });
            }
            return list;
        }
        public List<object> CommonDealerDropDownList()
        {
            var list = new List<object>();
            var v = _db.Vendors.Where(c => c.VendorTypeId == (int)Provider.Dealer && c.IsActive == true).ToList();
            foreach (var x in v)
            {
                list.Add(new { Text = x.Name, Value = x.VendorId });
            }
            return list;
        }
        public List<object> CommonCustomerDropDownList()
        {
            var list = new List<object>();
            var v = _db.Vendors.Where(c => c.VendorTypeId == (int)Provider.Customer && c.IsActive == true).ToList();
            foreach (var x in v)
            {
                list.Add(new { Text = x.Name, Value = x.VendorId });
            }
            return list;
        }
        public List<object> CommonCustomerPaymentType()
        {
            var list = new List<object>();

            var students = new List<CustomerPaymentType>() {
                new CustomerPaymentType(){ Text = "Credit", Value="Credit"},
                new CustomerPaymentType(){ Text = "Cash", Value="Cash"},
                new CustomerPaymentType(){ Text = "Special (Credit & Cash)", Value="Special"}
            };

            foreach (var x in students)
            {
                list.Add(new { Text = x.Text, Value = x.Value });
            }
            return list;
        }

        public List<object> CommonRelationList()
        {
            var list = new List<object>();
            list.Add(new { Text = "Wife", Value = "Wife" });
            list.Add(new { Text = "Husband", Value = "Husband" });
            list.Add(new { Text = "Son", Value = "Son" });
            list.Add(new { Text = "Doughter", Value = "Doughter" });
            list.Add(new { Text = "Brother", Value = "Brother" });
            list.Add(new { Text = "Sister", Value = "Sister" });
            list.Add(new { Text = "Father", Value = "Father" });
            list.Add(new { Text = "Mother", Value = "Mother" });

            return list;
        }
        public List<object> CommonZonesDropDownList(int companyId)
        {
            var list = new List<object>();
            var v = _db.Zones.Where(x => x.CompanyId == companyId && x.IsActive == true).ToList();
            foreach (var x in v)
            {
                list.Add(new { Text = x.Name, Value = x.ZoneId });
            }
            return list;
        }
        public List<object> CompaniesDropDownList()
        {
            var list = new List<object>();
            var v = _db.Companies.Where(x => x.IsActive == true && x.IsCompany).ToList();
            foreach (var x in v)
            {
                list.Add(new { Text = x.Name, Value = x.CompanyId });
            }
            return list;
        }
        public List<object> CompanyMenusDropDownList()
        {
            var list = new List<object>();
            var v = _db.CompanyMenus.ToList();
            foreach (var x in v)
            {
                list.Add(new { Text = x.Name, Value = x.CompanyMenuId });
            }
            return list;
        }
        public List<object> CommonSubZonesDropDownList(int companyId, int zoneId = 0)
        {
            var list = new List<object>();
            var v = _db.SubZones.Where(x => x.IsActive == true && x.CompanyId == companyId && (zoneId > 0 ? x.ZoneId == zoneId : x.SubZoneId > 0)).ToList();
            foreach (var x in v)
            {
                list.Add(new { Text = x.Name, Value = x.SubZoneId });
            }
            return list;
        }
        public List<SelectModelType> ZoneDropDownList(int companyId)
        {
            var list = new List<SelectModelType>();
            list = _db.Zones.Where(x => x.IsActive == true && x.CompanyId == companyId && x.ZoneId > 0)
                .Select(s => new SelectModelType
                {
                    Value = s.HeadGLId,
                    Text = s.Name
                }
                ).ToList();
            return list;
        }
        public List<object> CommonThanaDropDownList()
        {
            var list = new List<object>();
            var v = _db.Upazilas.Where(a => a.IsActive == true).ToList();
            foreach (var x in v)
            {
                list.Add(new { Text = x.Name, Value = x.UpazilaId });
            }
            return list;
        }

        //public string UploadFile(IFormFile file, string folderName, string webRootPath)
        //{
        //    string fName = "";
        //    if (file != null)
        //    {
        //        //string folderName = "Product";
        //        //string webRootPath = _webHostEnvironment.WebRootPath;
        //        string newPath = Path.Combine(webRootPath, folderName);

        //        if (!Directory.Exists(newPath))
        //        {
        //            Directory.CreateDirectory(newPath);
        //        }
        //        if (file.Length > 0)
        //        {
        //            string exten = Path.GetFileName(file.FileName);
        //            fName = Guid.NewGuid() + exten.Substring(exten.IndexOf("."), exten.Length - exten.IndexOf("."));
        //            string sFileExtension = Path.GetExtension(file.FileName).ToLower();
        //            string fullPath = Path.Combine(newPath, fName);
        //            FileStream stream;
        //            using (stream = new FileStream(fullPath, FileMode.OpenOrCreate))
        //            {
        //                file.CopyTo(stream);
        //                stream.Position = 0;
        //            }


        //        }
        //    }
        //    return fName;
        //}

        #region Common Product
        public List<SelectDDLModel> GetProductSelectionList(int companyId, int categoryId = 0, int subCategoryId = 0, string productType = "")
        {
            List<SelectDDLModel> list;
            if (subCategoryId > 0)
            {
                list = (from t1 in _db.Products
                        where t1.ProductSubCategoryId == subCategoryId
                              && t1.CompanyId == companyId
                              && t1.IsActive == true
                        // && !string.IsNullOrEmpty(productType) ? t1.ProductType.Equals(productType)

                        select new SelectDDLModel()
                        {
                            Text = t1.ProductName,
                            Value = t1.ProductId
                        }).ToList();

            }

            else if (categoryId > 0)
            {
                list = (from t1 in _db.Products
                        where t1.ProductCategoryId == categoryId
                              && t1.CompanyId == companyId
                              && t1.IsActive == true
                        // && !string.IsNullOrEmpty(productType) ? t1.ProductType.Equals(productType)

                        select new SelectDDLModel()
                        {
                            Text = t1.ProductName,
                            Value = t1.ProductId
                        }).ToList();
            }
            else
            {
                list = (from t1 in _db.Products
                        where t1.CompanyId == companyId
                              && t1.IsActive == true
                        // && !string.IsNullOrEmpty(productType) ? t1.ProductType.Equals(productType)

                        select new SelectDDLModel()
                        {
                            Text = t1.ProductName,
                            Value = t1.ProductId
                        }).ToList();

            }

            return list;
        }
        public async Task<VMCommonProduct> GetProduct(int companyId, int categoryId = 0, int subCategoryId = 0, string productType = "")
        {
            VMCommonProduct vmCommonProduct = new VMCommonProduct();
            if ((categoryId == 0 && subCategoryId > 0) || (categoryId > 0 && subCategoryId > 0))
            {
                vmCommonProduct = await (from t1 in _db.ProductSubCategories.Where(x => x.ProductSubCategoryId == subCategoryId && x.IsActive == true)
                                         join t2 in _db.ProductCategories on t1.ProductCategoryId equals t2.ProductCategoryId
                                         select new VMCommonProduct
                                         {
                                             Common_ProductSubCategoryFk = t1.ProductSubCategoryId,
                                             Common_ProductCategoryFk = t1.ProductCategoryId,
                                             CategoryName = t2.Name,
                                             SubCategoryName = t1.Name,
                                             CompanyFK = t1.CompanyId,

                                         }).FirstOrDefaultAsync();
            }
            else if (categoryId > 0 && subCategoryId == 0)
            {
                vmCommonProduct = await (from t1 in _db.ProductCategories.Where(x => x.ProductCategoryId == categoryId && x.IsActive == true)
                                         select new VMCommonProduct
                                         {

                                             Common_ProductCategoryFk = t1.ProductCategoryId,
                                             CategoryName = t1.Name,
                                             CompanyFK = t1.CompanyId
                                         }).FirstOrDefaultAsync();
            }
            else
            {
                vmCommonProduct.CompanyFK = companyId;

            }
            vmCommonProduct.DataList = await (from t1 in _db.Products.Where(x => x.CompanyId == companyId && x.ProductType == productType)
                                              join t2 in _db.ProductSubCategories on t1.ProductSubCategoryId equals t2.ProductSubCategoryId into t2_Join
                                              from t2 in t2_Join.DefaultIfEmpty()
                                              join t3 in _db.ProductCategories on t2.ProductCategoryId equals t3.ProductCategoryId into t3_Join
                                              from t3 in t3_Join.DefaultIfEmpty()
                                              join t4 in _db.Units on t1.UnitId equals t4.UnitId into t4_Join
                                              from t4 in t4_Join.DefaultIfEmpty()
                                              join t5 in _db.Products on t1.PackId equals t5.ProductId into t5_Join
                                              from t5 in t5_Join.DefaultIfEmpty()


                                              where t1.IsActive == true && t2.IsActive == true && t3.IsActive == true &&
                                              ((categoryId > 0 && subCategoryId == 0) ? t2.ProductCategoryId == categoryId
                                              : (categoryId == 0 && subCategoryId > 0) ? t1.ProductSubCategoryId == subCategoryId
                                              : t1.ProductId > 0)
                                              select new VMCommonProduct
                                              {
                                                  ID = t1.ProductId,
                                                  Name = t1.ProductName,
                                                  ShortName = t1.ShortName,
                                                  //MRPPrice = t1.UnitPrice ?? 0,
                                                  DeportSalePrice = t1.DeportPrice,
                                                  DealerSalePrice = t1.DealerPrice,
                                                  CustomerSalePrice = t1.CustomerPrice,
                                                  TPPrice = t1.TPPrice,
                                                  Qty = t1.Qty,
                                                  Consumption = t1.Consumption,
                                                  CreditSalePrice = t1.CreditSalePrice,
                                                  SubCategoryName = t2.Name,
                                                  CategoryName = t3.Name,
                                                  UnitName = t4.Name,
                                                  ProductType = t1.ProductType,
                                                  Code = t1.ProductCode,
                                                  PackId = t1.PackId,
                                                  PackName = t5 != null ? t2.Name + " " + t5.ProductName : "",
                                                  DieSize = t1.DieSize,
                                                  PackSize = t1.PackSize,
                                                  ProcessLoss = t1.ProcessLoss,
                                                  FormulaQty = t1.FormulaQty
                                              }).OrderByDescending(x => x.ID).ToListAsync();//.AsEnumerable()

            return vmCommonProduct;
        }
        public async Task<VMCommonProduct> GetProductByBinSlaveId(int binSlaveID)
        {
            VMCommonProduct vmCommonProduct = new VMCommonProduct();
            vmCommonProduct.DataList = await Task.Run(() => (from t1 in _db.Products
                                                             join t2 in _db.ProductSubCategories on t1.ProductSubCategoryId equals t2.ProductSubCategoryId
                                                             join t3 in _db.ProductCategories on t2.ProductCategoryId equals t3.ProductCategoryId
                                                             join t4 in _db.Units on t1.UnitId equals t4.UnitId

                                                             select new VMCommonProduct
                                                             {
                                                                 ID = t1.ProductId,
                                                                 Name = t1.ProductName,
                                                                 MRPPrice = t1.UnitPrice.Value,
                                                                 SubCategoryName = t2.Name,
                                                                 CategoryName = t3.Name,
                                                                 UnitName = t4.Name
                                                             }).OrderByDescending(x => x.ID).AsEnumerable());
            return vmCommonProduct;
        }

        #region RealState Product Services
        public async Task<List<SelectModelType>> GetFacingDropDown()
        {
            var list = await _db.FacingInfoes.Where(x => x.IsActive == true).Select(e => new SelectModelType()
            {
                Text = e.Title,
                Value = e.FacingId
            }).ToListAsync();

            return list;
        }
        public async Task<int> RealStateProductAdd(VMRealStateProduct vmCommonProduct)
        {
            var result = -1;
            #region Genarate Product No





            var JsonData = vmCommonProduct.CompanyFK == (int)CompanyName.GloriousLandsAndDevelopmentsLimited ? JsonConvert.SerializeObject(vmCommonProduct.PlotProp) :
                 vmCommonProduct.CompanyFK == (int)CompanyName.KrishibidPropertiesLimited ? JsonConvert.SerializeObject(vmCommonProduct.FlatProp) : "";
            #endregion

            Product commonProduct = new Product
            {
                Status = vmCommonProduct.Status,
                ProductCode = vmCommonProduct.ProductCode,
                ShortName = vmCommonProduct.ShortName,
                ProductName = vmCommonProduct.Name,
                UnitPrice = vmCommonProduct.MRPPrice,
                TPPrice = vmCommonProduct.TPPrice,
                CreditSalePrice = vmCommonProduct.CreditSalePrice,

                ProductCategoryId = vmCommonProduct.Common_ProductCategoryFk,
                ProductSubCategoryId = vmCommonProduct.Common_ProductSubCategoryFk,
                UnitId = vmCommonProduct.Common_UnitFk,
                Remarks = vmCommonProduct.Remarks,
                CompanyId = vmCommonProduct.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                DieSize = vmCommonProduct.DieSize,
                PackId = vmCommonProduct.PackId,
                PackSize = vmCommonProduct.PackSize,
                ProcessLoss = vmCommonProduct.ProcessLoss,

                IsActive = true,
                ProductType = vmCommonProduct.ProductType,

                OrderNo = 0,
                FacingId = vmCommonProduct.FacingId,
                JsonData = JsonData

            };
            _db.Products.Add(commonProduct);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = commonProduct.ProductId;
            }
            return result;
        }
        public async Task<int> RealStateProductEdit(VMRealStateProduct vmCommonProduct)
        {
            var result = -1;
            Product commonProduct = _db.Products.Find(vmCommonProduct.ID);

            var JsonData = vmCommonProduct.CompanyFK == (int)CompanyName.GloriousLandsAndDevelopmentsLimited ? JsonConvert.SerializeObject(vmCommonProduct.PlotProp) :
                 vmCommonProduct.CompanyFK == (int)CompanyName.KrishibidPropertiesLimited ? JsonConvert.SerializeObject(vmCommonProduct.FlatProp) : "";
            commonProduct.Status = vmCommonProduct.Status;

            commonProduct.ProductName = vmCommonProduct.Name;
            commonProduct.UnitPrice = vmCommonProduct.MRPPrice;
            commonProduct.TPPrice = vmCommonProduct.TPPrice;
            commonProduct.ShortName = vmCommonProduct.ShortName;
            commonProduct.CreditSalePrice = vmCommonProduct.CreditSalePrice;
            commonProduct.ProductSubCategoryId = vmCommonProduct.Common_ProductSubCategoryFk;
            commonProduct.UnitId = vmCommonProduct.Common_UnitFk;
            commonProduct.CompanyId = vmCommonProduct.CompanyFK.Value;
            commonProduct.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            commonProduct.ModifiedDate = DateTime.Now;
            commonProduct.Remarks = vmCommonProduct.Remarks;
            commonProduct.DieSize = vmCommonProduct.DieSize;
            commonProduct.PackId = vmCommonProduct.PackId;
            commonProduct.PackSize = vmCommonProduct.PackSize;
            commonProduct.ProcessLoss = vmCommonProduct.ProcessLoss;
            commonProduct.FacingId = vmCommonProduct.FacingId;
            commonProduct.JsonData = JsonData;
            commonProduct.ProductCode = vmCommonProduct.ProductCode;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = commonProduct.ProductId;
            }
            return result;
        }

        public async Task<VMRealStateProduct> GetRealStateProductForEdit(int productId)
        {
            VMRealStateProduct model = new VMRealStateProduct();
            Product product = await _db.Products.Include(e => e.ProductCategory).SingleOrDefaultAsync(o => o.ProductId == productId);
            if (product != null)
            {
                if (product.CompanyId == (int)CompanyName.GloriousLandsAndDevelopmentsLimited)
                {
                    model.PlotProp = JsonConvert.DeserializeObject<PlotProperties>(product.JsonData);
                    model.PlotProp = model.PlotProp == null ? new PlotProperties() : model.PlotProp;
                }
                if (product.CompanyId == (int)CompanyName.KrishibidPropertiesLimited)
                {
                    model.FlatProp = JsonConvert.DeserializeObject<FlatProperties>(product.JsonData);
                    model.FlatProp = model.FlatProp == null ? new FlatProperties() : model.FlatProp;
                }
                model.FacingDropDown = await GetFacingDropDown();
                model.UnitList = new SelectList(this.UnitDropDownList(product.CompanyId.Value), "Value", "Text", product.UnitId);
                model.ProductCategoryList = new SelectList(_db.ProductCategories.Where(e => e.CompanyId.Value == product.CompanyId.Value && e.IsActive == true)
                    .Select(o => new SelectModelType
                    {
                        Text = o.Name,
                        Value = o.ProductCategoryId
                    }).ToList(), "Value", "Text", product.ProductCategoryId);
                model.ProductSubCategoryList = new SelectList(_db.ProductSubCategories.Where(e => e.ProductCategoryId == product.ProductCategoryId && e.CompanyId.Value == product.CompanyId.Value && e.IsActive)
                    .Select(o => new SelectModelType
                    {
                        Text = o.Name,
                        Value = o.ProductSubCategoryId
                    }).ToList(), "Value", "Text", product.ProductSubCategoryId);
                model.Name = product.ProductName;
                model.MRPPrice = product.UnitPrice.HasValue ? product.UnitPrice.Value : 0;
                model.TPPrice = product.TPPrice;
                model.ShortName = product.ShortName;
                model.CreditSalePrice = product.CreditSalePrice;
                model.Common_ProductSubCategoryFk = product.ProductSubCategoryId;
                model.Common_UnitFk = product.UnitId;
                model.Common_ProductCategoryFk = product.ProductCategoryId;
                model.CategoryName = product.ProductCategory.Name;
                model.CompanyFK = product.CompanyId;
                model.Remarks = product.Remarks;
                model.DieSize = product.DieSize;
                model.PackId = product.PackId;
                model.PackSize = product.PackSize;
                model.ProcessLoss = product.ProcessLoss;
                model.FacingId = product.FacingId;
                model.ProductType = product.ProductType;
                model.ID = product.ProductId;
                model.ProductCode = product.ProductCode;
                model.Status = product.Status;

            }
            return model;
        }
        #endregion RealState Product Services


        public async Task<int> ProductAdd(VMCommonProduct vmCommonProduct)
        {
            var result = -1;


            #region check Duplicate

            var isExist = await _db.Products.FirstOrDefaultAsync(u => u.ProductName.Equals(vmCommonProduct.Name) && u.ProductCategoryId == vmCommonProduct.Common_ProductCategoryFk && u.ProductSubCategoryId == vmCommonProduct.Common_ProductSubCategoryFk && u.IsActive == true);

            if (isExist?.ProductCategoryId > 0)
            {
                throw new Exception($"Sorry! This Name {vmCommonProduct.Name} already Exists!");
            }

            #endregion



            #region Genarate Product No
            int lsatProduct = _db.Products.Select(x => x.ProductId).OrderByDescending(ID => ID).FirstOrDefault();
            if (lsatProduct == 0)
            {
                lsatProduct = 1;
            }
            else
            {
                lsatProduct++;
            }

            var productId = vmCommonProduct.ProductType + lsatProduct.ToString().PadLeft(6, '0');
            #endregion

            Product commonProduct = new Product
            {
                ProductCode = productId,
                ShortName = vmCommonProduct.ShortName,
                ProductName = vmCommonProduct.Name,
                UnitPrice = vmCommonProduct.MRPPrice,
                DeportPrice = vmCommonProduct.DeportSalePrice,
                DealerPrice = vmCommonProduct.DealerSalePrice,
                CustomerPrice = vmCommonProduct.CustomerSalePrice,

                TPPrice = vmCommonProduct.TPPrice,
                Qty = vmCommonProduct.Qty,
                Consumption = vmCommonProduct.Consumption,
                CreditSalePrice = vmCommonProduct.CreditSalePrice,

                ProductCategoryId = vmCommonProduct.Common_ProductCategoryFk,
                ProductSubCategoryId = vmCommonProduct.Common_ProductSubCategoryFk,
                UnitId = vmCommonProduct.Common_UnitFk,

                Remarks = vmCommonProduct.Remarks,
                CompanyId = vmCommonProduct.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                DieSize = vmCommonProduct.DieSize,
                PackId = vmCommonProduct.PackId,
                PackSize = vmCommonProduct.PackSize,
                ProcessLoss = vmCommonProduct.ProcessLoss,
                FormulaQty = vmCommonProduct.FormulaQty,

                IsActive = true,
                ProductType = vmCommonProduct.ProductType,
                OrderNo = 0

            };
            _db.Products.Add(commonProduct);
            if (await _db.SaveChangesAsync() > 0)
            {


                if (commonProduct.CompanyId == (int)CompanyName.KrishibidFeedLimited)
                {
                    result = commonProduct.ProductId;

                    Product product = await _db.Products.FindAsync(commonProduct.ProductId);

                    VMHeadIntegration integration = new VMHeadIntegration
                    {
                        AccName = product.ProductName,
                        LayerNo = 6,
                        Remarks = "6 Layer",
                        IsIncomeHead = false,
                        ProductType = product.ProductType,
                        CompanyFK = commonProduct.CompanyId,
                        CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                        CreatedDate = DateTime.Now
                    };


                    int headGl = ProductHeadGlPush(integration, commonProduct);

                    //if (headGlId != null)
                    //{
                    //    await GLDLBlockCodeAndHeadGLIdEdit(commonProductSubCategory.ProductSubCategoryId, headGlId, head5Id);
                    //}
                }


                result = commonProduct.ProductId;
            }
            return result;
        }
        public async Task<int> ProductEdit(VMCommonProduct vmCommonProduct)
        {
            var result = -1;

            #region check Duplicate

            var isExist = await _db.Products.FirstOrDefaultAsync(u => u.ProductName.Equals(vmCommonProduct.Name) && u.ProductCategoryId == vmCommonProduct.Common_ProductCategoryFk && u.ProductSubCategoryId == vmCommonProduct.Common_ProductSubCategoryFk && u.ProductId != vmCommonProduct.ID && u.IsActive == true);

            if (isExist?.ProductCategoryId > 0)
            {
                throw new Exception($"Sorry! This Name {vmCommonProduct.Name} already Exists!");
            }

            #endregion

            Product commonProduct = await _db.Products.FindAsync(vmCommonProduct.ID);

            commonProduct.ProductName = vmCommonProduct.Name;
            commonProduct.UnitPrice = vmCommonProduct.MRPPrice;
            commonProduct.DeportPrice = vmCommonProduct.DeportSalePrice;
            commonProduct.DealerPrice = vmCommonProduct.DealerSalePrice;
            commonProduct.CustomerPrice = vmCommonProduct.CustomerSalePrice;

            commonProduct.TPPrice = vmCommonProduct.TPPrice;
            commonProduct.Qty = vmCommonProduct.Qty;
            commonProduct.Consumption = vmCommonProduct.Consumption;
            commonProduct.ShortName = vmCommonProduct.ShortName;
            commonProduct.CreditSalePrice = vmCommonProduct.CreditSalePrice;
            commonProduct.ProductCategoryId = vmCommonProduct.Common_ProductCategoryFk;
            commonProduct.ProductSubCategoryId = vmCommonProduct.Common_ProductSubCategoryFk;
            commonProduct.UnitId = vmCommonProduct.Common_UnitFk;
            commonProduct.CompanyId = vmCommonProduct.CompanyFK.Value;
            commonProduct.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            commonProduct.ModifiedDate = DateTime.Now;
            commonProduct.Remarks = vmCommonProduct.Remarks;
            commonProduct.DieSize = vmCommonProduct.DieSize;
            commonProduct.PackId = vmCommonProduct.PackId;
            commonProduct.PackSize = vmCommonProduct.PackSize;
            commonProduct.ProcessLoss = vmCommonProduct.ProcessLoss;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = commonProduct.ProductId;
            }
            return result;
        }
        public async Task<int> ProductDelete(int id)
        {
            int result = -1;

            if (id != 0)
            {
                Product commonProduct = await _db.Products.FindAsync(id);
                commonProduct.IsActive = false;
                commonProduct.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                commonProduct.ModifiedDate = DateTime.Now;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = commonProduct.ProductId;
                }
            }
            return result;
        }

        public async Task<bool> CheckDuplicateProductName(int categoryId, int subCategoryId, string productName, int id)
        {
            bool isExist = false;

            if (string.IsNullOrEmpty(productName))
            {
                return isExist;
            }
            if (id > 0)
            {
                isExist = await _db.Products.AnyAsync(u => u.ProductName.Equals(productName) && u.ProductCategoryId == categoryId && u.ProductSubCategoryId == subCategoryId && u.ProductId != id && u.IsActive == true);
            }
            else
            {
                isExist = await _db.Products.AnyAsync(u => u.ProductName.Equals(productName) && u.ProductCategoryId == categoryId && u.ProductSubCategoryId == subCategoryId && u.IsActive == true);
            }

            return isExist;

        }
        public List<object> ProductSubCategoryDropDownList()
        {
            var productSubCategoryList = new List<object>();
            var productSubCategory = _db.ProductSubCategories.Where(a => a.IsActive == true).ToList();
            foreach (var x in productSubCategory)
            {
                productSubCategoryList.Add(new { Text = x.Name, Value = x.ProductSubCategoryId });
            }
            return productSubCategoryList;
        }
        public List<object> UnitDropDownList(int companyId)
        {
            var unitList = new List<object>();
            var units = _db.Units.Where(a => a.IsActive == true && a.CompanyId == companyId).ToList();
            foreach (var x in units)
            {
                unitList.Add(new { Text = x.Name, Value = x.UnitId });
            }
            return unitList;
        }
        public List<object> PlotOrPlatStatusList(int companyId)
        {
            var UnitList = new List<object>();


            var Units = _db.DropDownItems.Where(a => a.DropDownTypeId == 62 && a.CompanyId == companyId).ToList();
            foreach (var x in Units)
            {
                UnitList.Add(new { Text = x.Name, Value = x.DropDownItemId });
            }
            return UnitList;
        }

        public VMRealStateProduct GetCommonProductByID(int id)
        {
            var v = (from t1 in _db.Products.Where(x => x.ProductId == id)
                     join t2 in _db.ProductSubCategories on t1.ProductSubCategoryId equals t2.ProductSubCategoryId into t2_Join
                     from t2 in t2_Join.DefaultIfEmpty()
                     join t3 in _db.ProductCategories on t2.ProductCategoryId equals t3.ProductCategoryId into t3_Join
                     from t3 in t3_Join.DefaultIfEmpty()
                     join t4 in _db.Units on t1.UnitId equals t4.UnitId into t4_Join
                     from t4 in t4_Join.DefaultIfEmpty()
                     join t5 in _db.Products on t1.PackId equals t5.ProductId into t5_Join
                     from t5 in t5_Join.DefaultIfEmpty()

                     select new VMRealStateProduct
                     {
                         ID = t1.ProductId,
                         Name = t1.ProductName,
                         MRPPrice = t1.UnitPrice ?? 0,
                         TPPrice = t1.TPPrice,
                         Qty = t1.Qty,
                         ShortName = t1.ShortName,
                         SubCategoryName = t2.Name,
                         CategoryId = t2.ProductCategoryId ?? 0,
                         UnitName = t4.Name,
                         UnitPrice = t1.UnitPrice,
                         DeportSalePrice = t1.DeportPrice,
                         DealerSalePrice = t1.DealerPrice,
                         CustomerSalePrice = t1.CustomerPrice,
                         Common_ProductSubCategoryFk = t1.ProductSubCategoryId,
                         Common_UnitFk = t1.UnitId,
                         Common_ProductCategoryFk = t2.ProductCategoryId,
                         CompanyFK = t1.CompanyId,
                         CostingPrice = t1.CostingPrice,
                         PackId = t1.PackId,
                         PackName = t3 != null ? t3.Name + " " + t5.ProductName : "",
                         DieSize = t1.DieSize,
                         PackSize = t1.PackSize,
                         ProcessLoss = t1.ProcessLoss,
                         FormulaQty = t1.FormulaQty,
                         CreditSalePrice = t1.CreditSalePrice
                     }).FirstOrDefault();
            return v;
        }
        public VMFinishProductBOM GetFinishProductBOMsByID(int id)
        {
            var v = (from t1 in _db.FinishProductBOMs.Where(x => x.ID == id)
                     join t2 in _db.Products on t1.RProductFK equals t2.ProductId
                     join t4 in _db.Units on t2.UnitId equals t4.UnitId

                     select new VMFinishProductBOM
                     {
                         ID = t1.ID,
                         RequiredQuantity = t1.RequiredQuantity,
                         RProductFK = t1.RProductFK,
                         FProductFK = t1.FProductFK,
                         RawProductName = t2.ProductName,
                         UnitName = t4.Name,
                         CompanyFK = t1.CompanyId

                     }).FirstOrDefault();
            return v;
        }
        public async Task<VMFinishProductBOM> GetCommonProductByID(int companyId, int productId)
        {
            VMFinishProductBOM vmFinishProductBOM = new VMFinishProductBOM();

            vmFinishProductBOM = await Task.Run(() => (from t1 in _db.Products.Where(x => x.CompanyId == companyId && x.ProductId == productId)
                                                       join t2 in _db.ProductSubCategories on t1.ProductSubCategoryId equals t2.ProductSubCategoryId
                                                       join t3 in _db.ProductCategories on t2.ProductCategoryId equals t3.ProductCategoryId
                                                       join t4 in _db.Units on t1.UnitId equals t4.UnitId

                                                       select new VMFinishProductBOM
                                                       {
                                                           FProductFK = t1.ProductId,
                                                           Name = t1.ProductName,
                                                           MRPPrice = t1.UnitPrice.Value,
                                                           SubCategoryName = t2.Name,
                                                           CategoryName = t3.Name,
                                                           UnitName = t4.Name,
                                                           Common_ProductSubCategoryFk = t1.ProductSubCategoryId,
                                                           Common_UnitFk = t1.UnitId,
                                                           Common_ProductCategoryFk = t2.ProductCategoryId,
                                                           CompanyFK = t1.CompanyId,
                                                           CostingPrice = t1.CostingPrice

                                                       }).FirstOrDefault());

            vmFinishProductBOM.DataListProductBOM = await Task.Run(() => (from t1 in _db.FinishProductBOMs.Where(x => x.CompanyId == companyId && x.FProductFK == productId)
                                                                          join t2 in _db.Products on t1.RProductFK equals t2.ProductId
                                                                          join t3 in _db.ProductSubCategories on t2.ProductSubCategoryId equals t3.ProductSubCategoryId
                                                                          join t4 in _db.ProductCategories on t3.ProductCategoryId equals t4.ProductCategoryId
                                                                          join t5 in _db.Units on t2.UnitId equals t5.UnitId
                                                                          where t1.IsActive == true && t2.IsActive == true && t3.IsActive == true && t4.IsActive == true
                                                                          select new VMFinishProductBOM
                                                                          {
                                                                              ID = t1.ID,
                                                                              RProductFK = t1.RProductFK,
                                                                              FProductFK = t1.FProductFK,

                                                                              CategoryName = t4.Name,
                                                                              SubCategoryName = t3.Name,
                                                                              Name = t2.ProductName,
                                                                              UnitName = t5.Name,
                                                                              RequiredQuantity = t1.RequiredQuantity,
                                                                              //RProcessLoss = t1.RProcessLoss,


                                                                              Common_ProductSubCategoryFk = t2.ProductSubCategoryId,
                                                                              Common_UnitFk = t2.UnitId,
                                                                              Common_ProductCategoryFk = t2.ProductCategoryId,
                                                                              CompanyFK = t1.CompanyId

                                                                          }).AsEnumerable());



            return vmFinishProductBOM;
        }

        public VMCommonSupplier GetCommonSupplierByID(int id)
        {
            var v = (from t1 in _db.Vendors.Where(x => x.VendorTypeId == (int)Provider.Supplier && x.VendorId == id)
                     join t2 in _db.Countries on t1.CountryId equals t2.CountryId

                     select new VMCommonSupplier
                     {
                         ID = t1.VendorId,
                         Name = t1.Name,
                         Email = t1.Email,
                         Phone = t1.Phone,
                         Country = t2.CountryName,
                         CompanyFK = t1.CompanyId,
                         Common_CountriesFk = t1.CountryId.Value,
                         BranchName = t1.BranchName,
                         ACName = t1.ACName,
                         ACNo = t1.ACNo,
                         BankName = t1.BankName,
                         ContactPerson = t1.ContactName,
                         Address = t1.Address,
                         Code = t1.Code,
                         CreatedBy = t1.CreatedBy,
                         Remarks = t1.Remarks,
                         IsForeign = t1.IsForeign
                     }).FirstOrDefault();
            return v;
        }

        public async Task<int> ProductMRPPriceEdit(VMCommonProduct vmCommonProduct)
        {
            var result = -1;
            Product commonProduct = _db.Products.Find(vmCommonProduct.ID);
            commonProduct.UnitPrice = vmCommonProduct.MRPPrice;

            commonProduct.CompanyId = vmCommonProduct.CompanyFK;
            commonProduct.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            commonProduct.ModifiedDate = DateTime.Now;
            commonProduct.Remarks = vmCommonProduct.Remarks;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = commonProduct.ProductId;
            }
            return result;
        }

        #endregion


        #region Finish Product BOM
        public async Task<int> FinishProductBOMAdd(VMFinishProductBOM vmFinishProductBOM)
        {
            var result = -1;
            FinishProductBOM finishProductBOM = new FinishProductBOM
            {
                FProductFK = vmFinishProductBOM.FProductFK,
                RequiredQuantity = vmFinishProductBOM.RequiredQuantity,
                RProductFK = vmFinishProductBOM.RProductFK,
                RProcessLoss = vmFinishProductBOM.RProcessLoss,


                CompanyId = vmFinishProductBOM.CompanyFK.Value,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true

            };
            _db.FinishProductBOMs.Add(finishProductBOM);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = finishProductBOM.ID;
            }
            return result;
        }

        public async Task<int> FinishProductBOMEdit(VMFinishProductBOM vmFinishProductBOM)
        {
            var result = -1;
            FinishProductBOM finishProductBOM = _db.FinishProductBOMs.Find(vmFinishProductBOM.ID);
            finishProductBOM.FProductFK = vmFinishProductBOM.FProductFK;
            finishProductBOM.RequiredQuantity = vmFinishProductBOM.RequiredQuantity;
            finishProductBOM.RProductFK = vmFinishProductBOM.RProductFK;
            finishProductBOM.RProcessLoss = vmFinishProductBOM.RProcessLoss;

            finishProductBOM.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            finishProductBOM.ModifiedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = finishProductBOM.ID;
            }
            return result;
        }
        public async Task<int> FinishProductBOMDelete(int id)
        {
            int result = -1;
            if (id != 0)
            {
                FinishProductBOM finishProductBOM = await _db.FinishProductBOMs.FindAsync(id);

                finishProductBOM.IsActive = false;

                if (await _db.SaveChangesAsync() > 0)
                {
                    result = finishProductBOM.ID;
                }
            }
            return result;
        }
        #endregion

        #region Common Customer
        public VMCommonSupplier GetCommonCustomerByIDFeed(int id)
        {
            var v = (from t1 in _db.Vendors.Where(x => x.VendorTypeId == (int)Provider.Customer && x.VendorId == id)
                     join t2 in _db.Zones on t1.ZoneId equals t2.ZoneId
                     join t3 in _db.Upazilas on t1.UpazilaId equals t3.UpazilaId
                     join t4 in _db.Districts on t3.DistrictId equals t4.DistrictId

                     select new VMCommonSupplier
                     {
                         ID = t1.VendorId,
                         Name = t1.Name,
                         Email = t1.Email,
                         Phone = t1.Phone,
                         CompanyFK = t1.CompanyId,
                         CustomerTypeFk = t1.CustomerTypeFK,
                         ZoneId = t2.ZoneId,
                         Common_DivisionFk = t4.DivisionId,
                         Common_DistrictsFk = t3.DistrictId,
                         Common_UpazilasFk = t3.UpazilaId,
                         ContactPerson = t1.ContactName,
                         Address = t1.Address,
                         Code = t1.Code,
                         CreatedBy = t1.CreatedBy,
                         Remarks = t1.Remarks,
                         IsForeign = t1.IsForeign,
                         CreditLimit = t1.CreditLimit,
                         NID = t1.NID,
                         SecurityAmount = t1.SecurityAmount,
                         CustomerStatus = t1.CustomerStatus ?? 1,
                         Propietor = t1.Propietor,
                         PaymentType = t1.CustomerType,
                         NomineeName = t1.NomineeName,
                         NomineePhone = t1.NomineePhone,
                         BusinessAddress = t1.BusinessAddress,
                         NomineeNID = t1.NomineeNID,
                         NomineeRelation = t1.NomineeRelation


                     }).FirstOrDefault();
            return v;
        }
        public VMCommonSupplier GetCommonCustomerByID(int id)
        {
            var v = (from t1 in _db.Vendors.Where(x => x.VendorTypeId == (int)Provider.Customer && x.VendorId == id)
                     join t2 in _db.SubZones on t1.SubZoneId equals t2.SubZoneId into t2_def
                     from t2 in t2_def.DefaultIfEmpty()
                     join t3 in _db.Upazilas on t1.UpazilaId equals t3.UpazilaId into t3_def
                     from t3 in t3_def.DefaultIfEmpty()
                     join t4 in _db.Districts on t1.DistrictId equals t4.DistrictId into t4_def
                     from t4 in t4_def.DefaultIfEmpty()
                     join t5 in _db.ZoneDivisions on t1.ZoneDivisionId equals t5.ZoneDivisionId into t5_def
                     from t5 in t5_def.DefaultIfEmpty()

                     select new VMCommonSupplier
                     {
                         ID = t1.VendorId,
                         Name = t1.Name,
                         Email = t1.Email,
                         Phone = t1.Phone,
                         CompanyFK = t1.CompanyId,
                         SubZoneId = t1.SubZoneId.Value,
                         CustomerTypeFk = t1.CustomerTypeFK,
                         ZoneId = t2.ZoneId,
                         RegionId = t1.RegionId.Value,
                         AreaId = t1.AreaId.Value,
                         ZoneDivisionId = t1.ZoneDivisionId,
                         Common_DivisionFk = t4.DivisionId > 0 ? t4.DivisionId : 0,
                         Common_DistrictsFk = t3.DistrictId > 0 ? t3.DistrictId : 0,
                         Common_UpazilasFk = t3.UpazilaId > 0 ? t3.UpazilaId : 0,
                         ContactPerson = t1.ContactName,
                         Address = t1.Address,
                         Code = t1.Code,
                         CreatedBy = t1.CreatedBy,
                         Remarks = t1.Remarks,
                         IsForeign = t1.IsForeign,
                         CreditLimit = t1.CreditLimit,
                         NID = t1.NID,
                         SecurityAmount = t1.SecurityAmount,
                         CustomerStatus = t1.CustomerStatus ?? 1,
                         Propietor = t1.Propietor,
                         PaymentType = t1.CustomerType

                     }).FirstOrDefault();
            return v;
        }

        public VMCommonSupplier GetRSCustomerByID(int id)
        {
            var v = (from t1 in _db.Vendors.Where(x => x.VendorId == id)
                     join t2 in _db.Upazilas on t1.UpazilaId equals t2.UpazilaId into t2_Join
                     from t2 in t2_Join.DefaultIfEmpty()
                     join t3 in _db.Districts on t2.DistrictId equals t3.DistrictId into t3_Join
                     from t3 in t3_Join.DefaultIfEmpty()
                     join t4 in _db.Divisions on t3.DivisionId equals t4.DivisionId into t4_Join
                     from t4 in t4_Join.DefaultIfEmpty()

                     select new VMCommonSupplier
                     {
                         ID = t1.VendorId,
                         Name = t1.Name,
                         Email = t1.Email,
                         Phone = t1.Phone,
                         CompanyFK = t1.CompanyId,
                         Common_DistrictsFk = t3 != null ? t3.DistrictId : 0,
                         Common_UpazilasFk = t2 != null ? t2.UpazilaId : 0,
                         Common_DivisionFk = t4 != null ? t4.DivisionId : 0,
                         ImageFileUrl = t1.ImageUrl,
                         ContactPerson = t1.ContactName,
                         Address = t1.Address,
                         Code = t1.Code,
                         CreatedBy = t1.CreatedBy,
                         Remarks = t1.Remarks,
                         VendorReferenceId = t1.VendorReferenceId,
                         NID = t1.NID,
                         ImageDocId = t1.DocId
                     }).FirstOrDefault();
            return v;
        }
        public async Task<VMCommonSupplier> GetCustomer(int companyId, int zoneId, int subZoneId)
        {
            VMCommonSupplier vmCommonCustomer = new VMCommonSupplier();
            vmCommonCustomer.CompanyFK = companyId;
            vmCommonCustomer.DataList = await Task.Run(() => (from t1 in _db.Vendors.Where(x => x.IsActive == true && x.VendorTypeId == (int)Provider.Customer && x.CompanyId == companyId)

                                                              join t2 in _db.Upazilas on t1.UpazilaId equals t2.UpazilaId into t2_def
                                                              from t2 in t2_def.DefaultIfEmpty()
                                                              join t3 in _db.Districts on t1.DistrictId equals t3.DistrictId into t3_def
                                                              from t3 in t3_def.DefaultIfEmpty()
                                                              join t4 in _db.Divisions on t3.DivisionId equals t4.DivisionId into t4_def
                                                              from t4 in t4_def.DefaultIfEmpty()
                                                              join t8 in _db.Countries on t1.CountryId equals t8.CountryId into t8_def
                                                              from t8 in t8_def.DefaultIfEmpty()


                                                              join t6 in _db.Zones on t1.ZoneId equals t6.ZoneId into t6_def
                                                              from t6 in t6_def.DefaultIfEmpty()
                                                              join t7 in _db.ZoneDivisions on t1.ZoneDivisionId equals t7.ZoneDivisionId into t7_def
                                                              from t7 in t7_def.DefaultIfEmpty()
                                                              join t9 in _db.Regions on t1.RegionId equals t9.RegionId into t9_def
                                                              from t9 in t9_def.DefaultIfEmpty()
                                                              join t10 in _db.Areas on t1.AreaId equals t10.AreaId into t10_def
                                                              from t10 in t10_def.DefaultIfEmpty()
                                                              join t5 in _db.SubZones on t1.SubZoneId equals t5.SubZoneId into t5_def
                                                              from t5 in t5_def.DefaultIfEmpty()

                                                              where ((zoneId > 0) && (subZoneId == 0) ? t1.ZoneId == zoneId :
                                                                     (zoneId > 0) && (subZoneId > 0) ? t1.SubZoneId == subZoneId :
                                                              t1.VendorId > 0)
                                                              select new VMCommonSupplier
                                                              {
                                                                  ID = t1.VendorId,
                                                                  Name = t1.Name,
                                                                  Email = t1.Email,
                                                                  ContactPerson = t1.ContactName,
                                                                  Address = t1.Address,
                                                                  Code = t1.Code,
                                                                  Common_DistrictsFk = t2.DistrictId > 0 ? t2.DistrictId : 0,
                                                                  Common_UpazilasFk = t1.UpazilaId.Value > 0 ? t1.UpazilaId.Value : 0,
                                                                  District = t3.Name,
                                                                  Upazila = t2.Name,
                                                                  Division = t4.Name,
                                                                  Country = t8.CountryName,
                                                                  CreatedBy = t1.CreatedBy,
                                                                  Remarks = t1.Remarks,
                                                                  CompanyFK = t1.CompanyId,
                                                                  Phone = t1.Phone,
                                                                  ZoneId = t1.ZoneId ?? 0,
                                                                  ZoneName = t6.Name,
                                                                  ZoneDivisionId = t1.ZoneDivisionId,
                                                                  ZoneDivisionName = t7.Name,
                                                                  RegionId = t1.RegionId,
                                                                  RegionName = t9.Name,
                                                                  AreaId = t1.AreaId,
                                                                  AreaName = t10.Name,
                                                                  SubZoneId = t1.SubZoneId ?? 0,
                                                                  SubZoneName = t5.Name,

                                                                  ZoneIncharge = t6.ZoneIncharge,
                                                                  CreditLimit = t1.CreditLimit,
                                                                  NID = t1.NID,
                                                                  CustomerTypeFk = t1.CustomerTypeFK,
                                                                  SecurityAmount = t1.SecurityAmount,
                                                                  CustomerStatus = t1.CustomerStatus ?? 1,
                                                                  PaymentType = t1.CustomerType,
                                                                  Propietor = t1.Propietor
                                                              }).OrderByDescending(x => x.ID).AsEnumerable());



            return vmCommonCustomer;
        }


        public async Task<VMCommonSupplier> RSGetCustomer(int companyId)
        {
            VMCommonSupplier vmCommonCustomer = new VMCommonSupplier();
            vmCommonCustomer.CompanyFK = companyId;
            vmCommonCustomer.DataList = await Task.Run(() => (from t1 in _db.Vendors.Where(x => x.IsActive == true && x.VendorTypeId == (int)Provider.Customer && x.CompanyId == companyId)
                                                              join t2 in _db.Upazilas on t1.UpazilaId equals t2.UpazilaId into t2_Join
                                                              from t2 in t2_Join.DefaultIfEmpty()
                                                              join t3 in _db.Districts on t2.DistrictId equals t3.DistrictId into t3_Join
                                                              from t3 in t3_Join.DefaultIfEmpty()
                                                              join t4 in _db.Divisions on t3.DivisionId equals t4.DivisionId into t4_Join
                                                              from t4 in t4_Join.DefaultIfEmpty()

                                                              select new VMCommonSupplier
                                                              {
                                                                  ID = t1.VendorId,
                                                                  Name = t1.Name,
                                                                  Email = t1.Email,
                                                                  ContactPerson = t1.ContactName,
                                                                  Address = t1.Address,
                                                                  Code = t1.Code,
                                                                  District = t3.Name,
                                                                  BusinessAddress = t1.BusinessAddress,
                                                                  Country = t4.Name,
                                                                  CreatedBy = t1.CreatedBy,
                                                                  Remarks = t1.Remarks,
                                                                  CompanyFK = t1.CompanyId,
                                                                  Phone = t1.Phone,

                                                                  CreditLimit = t1.CreditLimit,
                                                                  NID = t1.NID,
                                                                  CustomerTypeFk = t1.CustomerTypeFK,
                                                                  SecurityAmount = t1.SecurityAmount,
                                                                  CustomerStatus = t1.CustomerStatus ?? 1,
                                                                  PaymentType = t1.CustomerType,
                                                                  Propietor = t1.Propietor,
                                                                  ImageDocId = t1.DocId
                                                              }).OrderByDescending(x => x.ID).AsEnumerable());



            return vmCommonCustomer;
        }

        public async Task<VMCommonSupplier> RSGetCustomerBooking(int companyId, int vendorId)
        {
            VMCommonSupplier vmCommonCustomer = new VMCommonSupplier();
            vmCommonCustomer.CompanyFK = companyId;
            vmCommonCustomer.DataList = await Task.Run(() => (from t1 in _db.CustomerGroupInfoes
                                                              join t2 in _db.Vendors on t1.PrimaryClientId equals t2.VendorId
                                                              join t3 in _db.HeadGLs on t1.HeadGLId equals t3.Id
                                                              join t4 in _db.ProductBookingInfoes on t1.CGId equals t4.CGId
                                                              join t5 in _db.Products on t4.ProductId equals t5.ProductId
                                                              join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                                                              join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                                                              where t1.PrimaryClientId == vendorId && t1.CompanyId == companyId
                                                              select new VMCommonSupplier
                                                              {
                                                                  CompanyFK = t1.CompanyId,
                                                                  CGId = t1.CGId,
                                                                  ID = t1.PrimaryClientId.Value,
                                                                  Name = t1.GroupName,
                                                                  ACName = t3.AccName,
                                                                  Code = t3.AccCode,
                                                                  FileNo = t4.FileNo,
                                                                  BookingNo = t4.BookingNo,
                                                                  ProductName = t7.Name + " " + t6.Name + " " + t5.ProductName
                                                              }).OrderByDescending(x => x.ID).AsEnumerable());



            return vmCommonCustomer;
        }
        public async Task<VMCommonSupplier> RSGetCustomerBookingProductCategories(int companyId, int productSubCategoryId)
        {
            VMCommonSupplier vmCommonCustomer = new VMCommonSupplier();
            vmCommonCustomer.CompanyFK = companyId;
            vmCommonCustomer.DataList = await Task.Run(() => (from t4 in _db.ProductBookingInfoes.Where(f => f.IsActive == true)
                                                              join t5 in _db.Products on t4.ProductId equals t5.ProductId
                                                              join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                                                              join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                                                              join t1 in _db.CustomerGroupInfoes on t4.CGId equals t1.CGId
                                                              join t2 in _db.Vendors on t1.PrimaryClientId equals t2.VendorId
                                                              join t3 in _db.HeadGLs on t1.HeadGLId equals t3.Id
                                                              where t6.ProductSubCategoryId == productSubCategoryId && t1.CompanyId == companyId && t1.IsActive == true
                                                              select new VMCommonSupplier
                                                              {
                                                                  CompanyFK = t1.CompanyId,
                                                                  CGId = t1.CGId,
                                                                  ID = t1.PrimaryClientId.Value,
                                                                  Name = t1.GroupName,
                                                                  ACName = t3.AccName,
                                                                  Code = t3.AccCode,
                                                                  FileNo = t4.FileNo,
                                                                  BookingNo = t4.BookingNo,
                                                                  ProductName = t7.Name + " " + t6.Name + " " + t5.ProductName
                                                              }).OrderByDescending(x => x.ID).AsEnumerable());



            return vmCommonCustomer;
        }
        public async Task<VMCommonSupplier> RSGetCustomerGroup(int companyId, int vendorId)
        {
            VMCommonSupplier vmCommonCustomer = new VMCommonSupplier();
            vmCommonCustomer.CompanyFK = companyId;
            vmCommonCustomer.VendorReferenceId = vendorId;

            vmCommonCustomer.DataList = await Task.Run(() => (from t1 in _db.Vendors
                                                              join t2 in _db.Upazilas on t1.UpazilaId equals t2.UpazilaId into t2_Join
                                                              from t2 in t2_Join.DefaultIfEmpty()
                                                              join t3 in _db.Districts on t2.DistrictId equals t3.DistrictId into t3_Join
                                                              from t3 in t3_Join.DefaultIfEmpty()
                                                              join t4 in _db.Divisions on t3.DivisionId equals t4.DivisionId into t4_Join
                                                              from t4 in t4_Join.DefaultIfEmpty()
                                                              where t1.CompanyId == companyId && t1.VendorReferenceId == vendorId && t1.IsActive == true
                                                              select new VMCommonSupplier
                                                              {
                                                                  ID = t1.VendorId,
                                                                  Name = t1.Name,
                                                                  Email = t1.Email,
                                                                  ContactPerson = t1.ContactName,
                                                                  Address = t1.Address,
                                                                  Code = t1.Code,
                                                                  District = t3.Name,
                                                                  BusinessAddress = t1.BusinessAddress,
                                                                  Country = t4.Name,
                                                                  CreatedBy = t1.CreatedBy,
                                                                  Remarks = t1.Remarks,
                                                                  CompanyFK = t1.CompanyId,
                                                                  Phone = t1.Phone,
                                                                  ImageFileUrl = t1.ImageUrl,
                                                                  ImageDocId = t1.DocId,
                                                                  CreditLimit = t1.CreditLimit,
                                                                  NID = t1.NID,
                                                                  CustomerTypeFk = t1.CustomerTypeFK,
                                                                  SecurityAmount = t1.SecurityAmount,
                                                                  CustomerStatus = t1.CustomerStatus ?? 1,
                                                                  PaymentType = t1.CustomerType,
                                                                  Propietor = t1.Propietor
                                                              }).OrderByDescending(x => x.ID).AsEnumerable());



            return vmCommonCustomer;
        }
        public async Task<VMCommonSupplier> GetClient(int companyId)
        {
            VMCommonSupplier vmCommonCustomer = new VMCommonSupplier();
            vmCommonCustomer.CompanyFK = companyId;
            vmCommonCustomer.DataList = await Task.Run(() => (from t1 in _db.Vendors.Where(x => x.IsActive == true && x.VendorTypeId == (int)Provider.Customer && x.CompanyId == companyId)
                                                              join t2 in _db.Upazilas on t1.UpazilaId equals t2.UpazilaId
                                                              join t3 in _db.Districts on t2.DistrictId equals t3.DistrictId
                                                              join t4 in _db.Divisions on t3.DivisionId equals t4.DivisionId
                                                              join t5 in _db.SubZones on t1.SubZoneId equals t5.SubZoneId
                                                              join t6 in _db.Zones on t5.ZoneId equals t6.ZoneId

                                                              select new VMCommonSupplier
                                                              {
                                                                  ID = t1.VendorId,
                                                                  Name = t1.Name,
                                                                  Email = t1.Email,
                                                                  ContactPerson = t1.ContactName,
                                                                  Address = t1.Address,
                                                                  Code = t1.Code,
                                                                  Common_DistrictsFk = t2.DistrictId,
                                                                  Common_UpazilasFk = t1.UpazilaId.Value,
                                                                  District = t3.Name,
                                                                  Upazila = t2.Name,
                                                                  Country = t4.Name,
                                                                  CreatedBy = t1.CreatedBy,
                                                                  Remarks = t1.Remarks,
                                                                  CompanyFK = t1.CompanyId,
                                                                  Phone = t1.Phone,
                                                                  ZoneName = t6.Name + " " + t5.Name,
                                                                  ZoneIncharge = t6.ZoneIncharge,
                                                                  CreditLimit = t1.CreditLimit,
                                                                  NID = t1.NID,
                                                                  CustomerTypeFk = t1.CustomerTypeFK,
                                                                  SecurityAmount = t1.SecurityAmount,
                                                                  CustomerStatus = t1.CustomerStatus ?? 1,
                                                                  Propietor = t1.Propietor
                                                              }).OrderByDescending(x => x.ID).AsEnumerable());



            return vmCommonCustomer;
        }

        public async Task<VMCommonSupplier> GetFeedCustomer(int companyId, int zoneId)
        {
            VMCommonSupplier vmCommonCustomer = new VMCommonSupplier();
            vmCommonCustomer.CompanyFK = companyId;
            vmCommonCustomer.DataList = await Task.Run(() => (from t1 in _db.Vendors.Where(x => x.IsActive == true && x.VendorTypeId == (int)Provider.Customer && x.CompanyId == companyId)
                                                              join t2 in _db.Upazilas on t1.UpazilaId equals t2.UpazilaId
                                                              join t3 in _db.Districts on t2.DistrictId equals t3.DistrictId
                                                              join t4 in _db.Divisions on t3.DivisionId equals t4.DivisionId
                                                              join t6 in _db.Zones on t1.ZoneId equals t6.ZoneId
                                                              join t7 in _db.HeadGLs on t1.HeadGLId equals t7.Id
                                                              join t8 in _db.Head5 on t7.ParentId equals t8.Id
                                                              join t9 in _db.Head4 on t8.ParentId equals t9.Id


                                                              where (zoneId > 0 ? t6.ZoneId == zoneId : zoneId == 0)

                                                              select new VMCommonSupplier
                                                              {
                                                                  Common_CountriesFk = t8.Id,
                                                                  NomineeName = t8.AccName + " " + t9.AccName,
                                                                  ID = t1.VendorId,
                                                                  Name = t1.Name,
                                                                  Email = t1.Email,
                                                                  ContactPerson = t1.ContactName,
                                                                  Address = t1.Address,
                                                                  Code = t1.Code,
                                                                  Common_DistrictsFk = t2.DistrictId,
                                                                  Common_UpazilasFk = t1.UpazilaId.Value,
                                                                  District = t3.Name,
                                                                  Upazila = t2.Name,
                                                                  Country = t4.Name,
                                                                  CreatedBy = t1.CreatedBy,
                                                                  Remarks = t1.Remarks,
                                                                  CompanyFK = t1.CompanyId,
                                                                  Phone = t1.Phone,
                                                                  ZoneName = t6.Name,
                                                                  ZoneIncharge = t6.ZoneIncharge,
                                                                  CreditLimit = t1.CreditLimit,
                                                                  NID = t1.NID,
                                                                  CustomerTypeFk = t1.CustomerTypeFK,
                                                                  SecurityAmount = t1.SecurityAmount,
                                                                  CustomerStatus = t1.CustomerStatus ?? 1,
                                                                  Propietor = t1.Propietor,
                                                                  HeadGLId = t1.HeadGLId,
                                                                  VendorTypeId = t1.VendorTypeId,
                                                                  ACName = t1.ACName,
                                                                  ACNo = t1.ACNo,
                                                                  BankName = t1.BankName,
                                                                  BranchName = t1.BranchName
                                                              }).OrderByDescending(x => x.ID).AsEnumerable());



            return vmCommonCustomer;
        }

        public async Task<VMCommonSupplier> GetCustomerById(int customerId)
        {
            VMCommonSupplier vmCommonCustomer = new VMCommonSupplier();
            vmCommonCustomer = await Task.Run(() => (from t1 in _db.Vendors.Where(x => x.IsActive == true && x.VendorTypeId == (int)Provider.Customer && x.VendorId == customerId)
                                                     join t2 in _db.Upazilas on t1.UpazilaId equals t2.UpazilaId
                                                     join t3 in _db.Districts on t2.DistrictId equals t3.DistrictId
                                                     join t4 in _db.Divisions on t3.DivisionId equals t4.DivisionId
                                                     join t5 in _db.SubZones on t1.SubZoneId equals t5.SubZoneId
                                                     join t6 in _db.Zones on t5.ZoneId equals t6.ZoneId
                                                     select new VMCommonSupplier
                                                     {
                                                         ID = t1.VendorId,
                                                         Name = t1.Name,
                                                         Email = t1.Email,
                                                         ContactPerson = t1.ContactName,
                                                         Address = t1.Address,
                                                         Code = t1.Code,
                                                         Common_DistrictsFk = t2.DistrictId,
                                                         Common_UpazilasFk = t1.UpazilaId.Value,
                                                         District = t3.Name,
                                                         Upazila = t2.Name,
                                                         Country = t4.Name,
                                                         CreatedBy = t1.CreatedBy,
                                                         Division = t4.Name,
                                                         Remarks = t1.Remarks,
                                                         CompanyFK = t1.CompanyId,
                                                         Phone = t1.Phone,
                                                         ZoneName = t5.Name,
                                                         ZoneIncharge = t6.ZoneIncharge,
                                                         CreditLimit = t1.CreditLimit,
                                                         NID = t1.NID,
                                                         CustomerTypeFk = t1.CustomerTypeFK,
                                                         VendorTypeId = t1.VendorTypeId
                                                     }).FirstOrDefault());



            return vmCommonCustomer;
        }
        public async Task<int> CustomerAdd(VMCommonSupplier vmCommonCustomer)
        {
            var result = -1;
            #region Genarate Supplier code
            int totalSupplier = _db.Vendors.Count(x => x.VendorTypeId == (int)Provider.Customer && x.CompanyId == vmCommonCustomer.CompanyFK);

            if (totalSupplier == 0)
            {
                totalSupplier = 1;
            }
            else
            {
                totalSupplier++;
            }

            var newString = totalSupplier.ToString().PadLeft(4, '0');
            #endregion



            Vendor commonCustomer = new Vendor
            {
                Name = vmCommonCustomer.Name,
                Phone = vmCommonCustomer.Phone,
                Email = vmCommonCustomer.Email,
                //DistrictId = vmCommonCustomer.Common_DistrictsFk,
                //UpazilaId = vmCommonCustomer.Common_UpazilasFk,
                ContactName = vmCommonCustomer.ContactPerson,
                VendorTypeId = (int)Provider.Customer,
                Address = vmCommonCustomer.Address,
                NID = vmCommonCustomer.NID,
                CreditLimit = vmCommonCustomer.CreditLimit,
                CustomerTypeFK = vmCommonCustomer.CustomerTypeFk,
                CompanyId = vmCommonCustomer.CompanyFK.Value,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                SecurityAmount = vmCommonCustomer.SecurityAmount,
                CustomerStatus = vmCommonCustomer.CustomerStatus,
                Propietor = vmCommonCustomer.Propietor,
                CustomerType = vmCommonCustomer.PaymentType,
                IsActive = true,
                CreditRatioFrom = 0,
                CreditRatioTo = 0,
                IsForeign = false,
                NomineeName = vmCommonCustomer.NomineeName,
                NomineePhone = vmCommonCustomer.NomineePhone,
                ZoneId = vmCommonCustomer.ZoneId,
                ZoneDivisionId = vmCommonCustomer.ZoneDivisionId,
                RegionId = vmCommonCustomer.RegionId,
                AreaId = vmCommonCustomer.AreaId,
                SubZoneId = vmCommonCustomer.SubZoneId,
                NomineeRelation = vmCommonCustomer.NomineeRelation,
                NomineeNID = vmCommonCustomer.NomineeNID,
                BusinessAddress = vmCommonCustomer.BusinessAddress,

            };
            _db.Vendors.Add(commonCustomer);

            if (await _db.SaveChangesAsync() > 0)
            {
                result = commonCustomer.VendorId;

                if (commonCustomer.CompanyId == (int)CompanyName.KrishibidFeedLimited && commonCustomer.VendorTypeId == (int)Provider.Customer)
                {
                    //Setting up Customer Offer
                    _db.Database.ExecuteSqlCommand("exec spSetCustomerCommission {0},{1},{2}", commonCustomer.CompanyId, commonCustomer.VendorId, commonCustomer.CreatedBy);
                }

                //Accounts Receivable Seed Head4 Id = 38116
                int parentId = 0;

                if (commonCustomer.CompanyId == (int)CompanyName.KrishibidSeedLimited || commonCustomer.CompanyId == (int)CompanyName.GloriousCropCareLimited)
                {
                    var subZones = await _db.SubZones.FindAsync(commonCustomer.SubZoneId);
                    parentId = subZones.AccountHeadId;

                }

                //else if (commonCustomer.CompanyId == (int)CompanyName.KrishibidFeedLimited)
                //{
                //    var zone = _db.Zones.Find(commonCustomer.ZoneId);
                //    ParentId = zone.HeadGLId;

                //}

                VMHeadIntegration integration = new VMHeadIntegration
                {
                    AccName = commonCustomer.Name,
                    LayerNo = 6,
                    Remarks = "GL Layer",
                    IsIncomeHead = false,
                    ParentId = parentId,
                    IsActive = true,

                    CompanyFK = commonCustomer.CompanyId,
                    CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                    CreatedDate = DateTime.Now,
                };

                if (commonCustomer.CompanyId == (int)CompanyName.KrishibidSeedLimited
                                || commonCustomer.CompanyId == (int)CompanyName.GloriousCropCareLimited
                                || commonCustomer.CompanyId == (int)CompanyName.KrishibidFeedLimited)
                {
                    HeadGL headGlId = await PayableHeadIntegrationAdd(integration);
                    if (headGlId != null)
                    {
                        await VendorsCodeAndHeadGLIdEdit(commonCustomer.VendorId, headGlId);
                    }
                }

            }

            return result;
        }

        public async Task<int> RSCustomerGroupAdd(VMCommonSupplier vmCommonCustomer)
        {
            var result = -1;
            #region Genarate Supplier code
            int totalSupplier = _db.Vendors.Count(x => x.VendorTypeId == (int)Provider.CustomerAssociates && x.CompanyId == vmCommonCustomer.CompanyFK);


            if (totalSupplier == 0)
            {
                totalSupplier = 1;
            }
            else
            {
                totalSupplier++;
            }

            var newString = totalSupplier.ToString().PadLeft(4, '0');
            #endregion
            Vendor commonCustomer = new Vendor
            {
                Name = vmCommonCustomer.Name,
                Phone = vmCommonCustomer.Phone,
                Email = vmCommonCustomer.Email,
                UpazilaId = vmCommonCustomer.Common_UpazilasFk,
                ContactName = vmCommonCustomer.ContactPerson,
                VendorTypeId = (int)Provider.CustomerAssociates,
                Address = vmCommonCustomer.Address,
                SubZoneId = vmCommonCustomer.SubZoneId,
                NID = vmCommonCustomer.NID,
                CreditLimit = vmCommonCustomer.CreditLimit,
                CustomerTypeFK = vmCommonCustomer.CustomerTypeFk,
                CompanyId = vmCommonCustomer.CompanyFK.Value,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                SecurityAmount = vmCommonCustomer.SecurityAmount,
                CustomerStatus = vmCommonCustomer.CustomerStatus,
                Propietor = vmCommonCustomer.Propietor,
                CustomerType = vmCommonCustomer.PaymentType,
                IsActive = true,
                CreditRatioFrom = 0,
                CreditRatioTo = 0,
                IsForeign = false,
                NomineeName = vmCommonCustomer.NomineeName,
                NomineePhone = vmCommonCustomer.NomineePhone,
                ZoneId = vmCommonCustomer.ZoneId,
                NomineeRelation = vmCommonCustomer.NomineeRelation,
                NomineeNID = vmCommonCustomer.NomineeNID,
                BusinessAddress = vmCommonCustomer.BusinessAddress,
                VendorReferenceId = vmCommonCustomer.VendorReferenceId,
                DocId = vmCommonCustomer.ImageDocId
            };
            _db.Vendors.Add(commonCustomer);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = commonCustomer.VendorId;
            }
            return result;
        }

        public async Task<int> RSCustomerGroupEdit(VMCommonSupplier vmCommonCustomer)
        {
            var result = -1;
            Vendor commonCustomer = _db.Vendors.Find(vmCommonCustomer.ID);
            commonCustomer.Name = vmCommonCustomer.Name;
            commonCustomer.UpazilaId = vmCommonCustomer.Common_UpazilasFk;
            commonCustomer.DistrictId = vmCommonCustomer.Common_DistrictsFk;
            commonCustomer.Address = vmCommonCustomer.Address;
            commonCustomer.Phone = vmCommonCustomer.Phone;
            commonCustomer.SubZoneId = vmCommonCustomer.SubZoneId;
            commonCustomer.NID = vmCommonCustomer.NID;
            commonCustomer.CreditLimit = vmCommonCustomer.CreditLimit;
            commonCustomer.Email = vmCommonCustomer.Email;
            commonCustomer.Remarks = vmCommonCustomer.Remarks;
            commonCustomer.CompanyId = vmCommonCustomer.CompanyFK.Value;
            commonCustomer.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            commonCustomer.ModifiedDate = DateTime.Now;
            commonCustomer.ContactName = vmCommonCustomer.ContactPerson;
            commonCustomer.CustomerTypeFK = vmCommonCustomer.CustomerTypeFk;
            commonCustomer.SecurityAmount = vmCommonCustomer.SecurityAmount;
            commonCustomer.CustomerStatus = vmCommonCustomer.CustomerStatus;
            commonCustomer.Propietor = vmCommonCustomer.Propietor;
            commonCustomer.NomineeName = vmCommonCustomer.NomineeName;
            commonCustomer.NomineePhone = vmCommonCustomer.NomineePhone;
            commonCustomer.ZoneId = vmCommonCustomer.ZoneId;
            commonCustomer.BusinessAddress = vmCommonCustomer.BusinessAddress;
            commonCustomer.NomineeNID = vmCommonCustomer.NomineeNID;
            commonCustomer.NomineeRelation = vmCommonCustomer.NomineeRelation;
            commonCustomer.VendorReferenceId = vmCommonCustomer.VendorReferenceId;
            commonCustomer.DocId = vmCommonCustomer.ImageDocId;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = commonCustomer.VendorId;
            }

            return result;
        }
        public async Task<int> RSCustomerAdd(VMCommonSupplier vmCommonCustomer)
        {
            var result = -1;
            #region Genarate Supplier code
            int totalSupplier = _db.Vendors
                .Where(x => x.VendorTypeId == (int)Provider.Customer && x.CompanyId == vmCommonCustomer.CompanyFK).Count();


            if (totalSupplier == 0)
            {
                totalSupplier = 1;
            }
            else
            {
                totalSupplier++;
            }

            var newString = totalSupplier.ToString().PadLeft(4, '0');
            #endregion
            Vendor commonCustomer = new Vendor
            {
                Name = vmCommonCustomer.Name,
                Phone = vmCommonCustomer.Phone,
                Email = vmCommonCustomer.Email,
                UpazilaId = vmCommonCustomer.Common_UpazilasFk,
                ContactName = vmCommonCustomer.ContactPerson,
                VendorTypeId = (int)Provider.Customer,
                Address = vmCommonCustomer.Address,
                SubZoneId = vmCommonCustomer.SubZoneId,
                NID = vmCommonCustomer.NID,
                CreditLimit = vmCommonCustomer.CreditLimit,
                CustomerTypeFK = vmCommonCustomer.CustomerTypeFk,
                CompanyId = vmCommonCustomer.CompanyFK.Value,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                SecurityAmount = vmCommonCustomer.SecurityAmount,
                CustomerStatus = vmCommonCustomer.CustomerStatus,
                Propietor = vmCommonCustomer.Propietor,
                CustomerType = vmCommonCustomer.PaymentType,
                IsActive = true,
                CreditRatioFrom = 0,
                CreditRatioTo = 0,
                IsForeign = false,
                NomineeName = vmCommonCustomer.NomineeName,
                NomineePhone = vmCommonCustomer.NomineePhone,
                ZoneId = vmCommonCustomer.ZoneId,
                NomineeRelation = vmCommonCustomer.NomineeRelation,
                NomineeNID = vmCommonCustomer.NomineeNID,
                BusinessAddress = vmCommonCustomer.BusinessAddress,
                DocId = vmCommonCustomer.ImageDocId

            };
            _db.Vendors.Add(commonCustomer);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = commonCustomer.VendorId;


            }
            return result;
        }
        public async Task<int> RSCustomerEdit(VMCommonSupplier vmCommonCustomer)
        {
            var result = -1;
            Vendor commonCustomer = _db.Vendors.Find(vmCommonCustomer.ID);
            commonCustomer.Name = vmCommonCustomer.Name;
            commonCustomer.UpazilaId = vmCommonCustomer.Common_UpazilasFk;
            commonCustomer.Address = vmCommonCustomer.Address;
            commonCustomer.Phone = vmCommonCustomer.Phone;
            commonCustomer.SubZoneId = vmCommonCustomer.SubZoneId;
            commonCustomer.NID = vmCommonCustomer.NID;
            commonCustomer.CreditLimit = vmCommonCustomer.CreditLimit;
            commonCustomer.Email = vmCommonCustomer.Email;
            commonCustomer.Remarks = vmCommonCustomer.Remarks;
            commonCustomer.CompanyId = vmCommonCustomer.CompanyFK.Value;
            commonCustomer.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            commonCustomer.ModifiedDate = DateTime.Now;
            commonCustomer.ContactName = vmCommonCustomer.ContactPerson;
            commonCustomer.CustomerTypeFK = vmCommonCustomer.CustomerTypeFk;
            commonCustomer.SecurityAmount = vmCommonCustomer.SecurityAmount;
            commonCustomer.CustomerStatus = vmCommonCustomer.CustomerStatus;
            commonCustomer.Propietor = vmCommonCustomer.Propietor;
            commonCustomer.NomineeName = vmCommonCustomer.NomineeName;
            commonCustomer.NomineePhone = vmCommonCustomer.NomineePhone;
            commonCustomer.ZoneId = vmCommonCustomer.ZoneId;
            commonCustomer.BusinessAddress = vmCommonCustomer.BusinessAddress;
            commonCustomer.NomineeNID = vmCommonCustomer.NomineeNID;
            commonCustomer.NomineeRelation = vmCommonCustomer.NomineeRelation;
            commonCustomer.DocId = vmCommonCustomer.ImageDocId;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = commonCustomer.VendorId;
            }

            return result;
        }
        public async Task<int> CustomerEdit(VMCommonSupplier vmCommonCustomer)
        {
            var result = -1;
            Vendor commonCustomer = await _db.Vendors.FindAsync(vmCommonCustomer.ID);
            commonCustomer.Name = vmCommonCustomer.Name;
            //commonCustomer.DistrictId = vmCommonCustomer.Common_DistrictsFk;
            //commonCustomer.UpazilaId = vmCommonCustomer.Common_UpazilasFk;
            commonCustomer.Address = vmCommonCustomer.Address;
            commonCustomer.Phone = vmCommonCustomer.Phone;
            commonCustomer.NID = vmCommonCustomer.NID;
            commonCustomer.CreditLimit = vmCommonCustomer.CreditLimit;
            commonCustomer.Email = vmCommonCustomer.Email;
            commonCustomer.Remarks = vmCommonCustomer.Remarks;
            commonCustomer.CompanyId = vmCommonCustomer.CompanyFK.Value;
            commonCustomer.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            commonCustomer.ModifiedDate = DateTime.Now;
            commonCustomer.ContactName = vmCommonCustomer.ContactPerson;
            commonCustomer.CustomerTypeFK = vmCommonCustomer.CustomerTypeFk;
            commonCustomer.SecurityAmount = vmCommonCustomer.SecurityAmount;
            commonCustomer.CustomerStatus = vmCommonCustomer.CustomerStatus;
            commonCustomer.Propietor = vmCommonCustomer.Propietor;
            commonCustomer.NomineeName = vmCommonCustomer.NomineeName;
            commonCustomer.NomineePhone = vmCommonCustomer.NomineePhone;
            commonCustomer.ZoneId = vmCommonCustomer.ZoneId;
            commonCustomer.ZoneDivisionId = vmCommonCustomer.ZoneDivisionId;
            commonCustomer.RegionId = vmCommonCustomer.RegionId;
            commonCustomer.AreaId = vmCommonCustomer.AreaId;
            commonCustomer.SubZoneId = vmCommonCustomer.SubZoneId;
            commonCustomer.BusinessAddress = vmCommonCustomer.BusinessAddress;
            commonCustomer.NomineeNID = vmCommonCustomer.NomineeNID;
            commonCustomer.NomineeRelation = vmCommonCustomer.NomineeRelation;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = commonCustomer.VendorId;
            }
            await IntegratedAccountsHeadEdit(commonCustomer.Name, commonCustomer.HeadGLId.Value);

            return result;
        }
        public async Task<int> CustomerDelete(int id)
        {
            int result = -1;
            if (id != 0)
            {
                Vendor commonCustomer = await _db.Vendors.FindAsync(id);
                commonCustomer.IsActive = false;
                commonCustomer.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                commonCustomer.ModifiedDate = DateTime.Now;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = commonCustomer.VendorId;
                }
            }
            return result;
        }

        #endregion

        #region Common Deport

        public async Task<VMCommonSupplier> GetDeportById(int deportId)
        {
            VMCommonSupplier vmCommonDeport = new VMCommonSupplier();
            vmCommonDeport = await Task.Run(() => (from t1 in _db.Vendors.Where(x => x.IsActive == true && x.VendorTypeId == (int)Provider.Deport && x.VendorId == deportId)
                                                       //join t2 in _db.Upazilas on t1.UpazilaId equals t2.UpazilaId
                                                       //join t3 in _db.Districts on t2.DistrictId equals t3.DistrictId
                                                       //join t4 in _db.Divisions on t3.DivisionId equals t4.DivisionId
                                                   join t5 in _db.SubZones on t1.SubZoneId equals t5.SubZoneId
                                                   join t6 in _db.Zones on t5.ZoneId equals t6.ZoneId
                                                   select new VMCommonSupplier
                                                   {
                                                       ID = t1.VendorId,
                                                       Name = t1.Name,
                                                       Email = t1.Email,
                                                       ContactPerson = t1.ContactName,
                                                       Address = t1.Address,
                                                       Code = t1.Code,
                                                       //Common_DistrictsFk = t2.DistrictId,
                                                       //Common_UpazilasFk = t1.UpazilaId.Value,
                                                       //District = t3.Name,
                                                       //Upazila = t2.Name,
                                                       //Country = t4.Name,
                                                       CreatedBy = t1.CreatedBy,
                                                       // Division = t4.Name,
                                                       Remarks = t1.Remarks,
                                                       CompanyFK = t1.CompanyId,
                                                       Phone = t1.Phone,
                                                       ZoneName = t5.Name,
                                                       ZoneIncharge = t6.ZoneIncharge,
                                                       CreditLimit = t1.CreditLimit,
                                                       NID = t1.NID,
                                                       CustomerTypeFk = t1.CustomerTypeFK,
                                                       VendorTypeId = t1.VendorTypeId
                                                   }).FirstOrDefault());



            return vmCommonDeport;
        }
        public VMCommonSupplier GetCommonDeportById(int id)
        {
            var v = (from t1 in _db.Vendors.Where(x => x.VendorTypeId == (int)Provider.Deport && x.VendorId == id)
                     join t2 in _db.SubZones on t1.SubZoneId equals t2.SubZoneId into t2_def
                     from t2 in t2_def.DefaultIfEmpty()
                     join t3 in _db.Upazilas on t1.UpazilaId equals t3.UpazilaId into t3_def
                     from t3 in t3_def.DefaultIfEmpty()
                     join t4 in _db.Districts on t3.DistrictId equals t4.DistrictId into t4_def
                     from t4 in t4_def.DefaultIfEmpty()
                     join t5 in _db.ZoneDivisions on t1.ZoneDivisionId equals t5.ZoneDivisionId into t5_def
                     from t5 in t5_def.DefaultIfEmpty()

                     select new VMCommonSupplier
                     {
                         ID = t1.VendorId,
                         Name = t1.Name,
                         Email = t1.Email,
                         Phone = t1.Phone,
                         CompanyFK = t1.CompanyId,
                         ZoneId = t1.ZoneId.Value,
                         ZoneDivisionId = t1.ZoneDivisionId.Value,
                         SubZoneId = t1.SubZoneId.Value,
                         RegionId = t1.RegionId.Value,
                         CustomerTypeFk = t1.CustomerTypeFK,
                         Common_DivisionFk = t4.DivisionId > 0 ? t4.DivisionId : 0,
                         Common_DistrictsFk = t3.DistrictId > 0 ? t3.DistrictId : 0,
                         Common_UpazilasFk = t3.UpazilaId > 0 ? t3.UpazilaId : 0,
                         ContactPerson = t1.ContactName,
                         Address = t1.Address,
                         Code = t1.Code,
                         CreatedBy = t1.CreatedBy,
                         Remarks = t1.Remarks,
                         IsForeign = t1.IsForeign,
                         CreditLimit = t1.CreditLimit,
                         NID = t1.NID,
                         SecurityAmount = t1.SecurityAmount,
                         CustomerStatus = t1.CustomerStatus ?? 1,
                         Propietor = t1.Propietor,
                         PaymentType = t1.CustomerType

                     }).FirstOrDefault();
            return v;
        }
        public async Task<VMCommonSupplier> GetDeport(int companyId, int zoneId, int subZoneId)
        {
            VMCommonSupplier vmCommonDeport = new VMCommonSupplier();
            vmCommonDeport.CompanyFK = companyId;
            vmCommonDeport.DataList = await Task.Run(() => (from t1 in _db.Vendors.Where(x => x.IsActive == true && x.VendorTypeId == (int)Provider.Deport && x.CompanyId == companyId)

                                                            join t2 in _db.Upazilas on t1.UpazilaId equals t2.UpazilaId into t2_def
                                                            from t2 in t2_def.DefaultIfEmpty()
                                                            join t3 in _db.Districts on t1.DistrictId equals t3.DistrictId into t3_def
                                                            from t3 in t3_def.DefaultIfEmpty()
                                                            join t4 in _db.Divisions on t3.DivisionId equals t4.DivisionId into t4_def
                                                            from t4 in t4_def.DefaultIfEmpty()

                                                            join t6 in _db.Zones on t1.ZoneId equals t6.ZoneId into t6_def
                                                            from t6 in t6_def.DefaultIfEmpty()
                                                            join t7 in _db.ZoneDivisions on t1.ZoneDivisionId equals t7.ZoneDivisionId into t7_def
                                                            from t7 in t7_def.DefaultIfEmpty()
                                                            join t9 in _db.Regions on t1.RegionId equals t9.RegionId into t9_def
                                                            from t9 in t9_def.DefaultIfEmpty()
                                                            join t10 in _db.Areas on t1.AreaId equals t10.AreaId into t10_def
                                                            from t10 in t10_def.DefaultIfEmpty()
                                                            join t5 in _db.SubZones on t1.SubZoneId equals t5.SubZoneId into t5_def
                                                            from t5 in t5_def.DefaultIfEmpty()

                                                            join t8 in _db.Countries on t1.CountryId equals t8.CountryId into t8_def
                                                            from t8 in t8_def.DefaultIfEmpty()

                                                            where ((zoneId > 0) && (subZoneId == 0) ? t1.ZoneId == zoneId :
                                                                     (zoneId > 0) && (subZoneId > 0) ? t1.SubZoneId == subZoneId :
                                                              t1.VendorId > 0)
                                                            select new VMCommonSupplier
                                                            {
                                                                ID = t1.VendorId,
                                                                Name = t1.Name,
                                                                Email = t1.Email,
                                                                ContactPerson = t1.ContactName,
                                                                Address = t1.Address,
                                                                Code = t1.Code,
                                                                EmployeeId = t1.EmployeeId,
                                                                Common_DistrictsFk = t2.DistrictId > 0 ? t2.DistrictId : 0,
                                                                Common_UpazilasFk = t1.UpazilaId.Value > 0 ? t1.UpazilaId.Value : 0,
                                                                District = t3.Name,
                                                                Upazila = t2.Name,
                                                                Division = t4.Name,
                                                                Country = t8.CountryName,
                                                                CreatedBy = t1.CreatedBy,
                                                                Remarks = t1.Remarks,
                                                                CompanyFK = t1.CompanyId,
                                                                Phone = t1.Phone,
                                                                ZoneId = t1.ZoneId ?? 0,
                                                                ZoneDivisionId = t1.ZoneDivisionId,
                                                                ZoneDivisionName = t7.Name,
                                                                SubZoneId = t1.SubZoneId ?? 0,
                                                                SubZoneName = t5.Name,
                                                                ZoneName = t6.Name + " " + t5.Name,
                                                                ZoneIncharge = t6.ZoneIncharge,
                                                                CreditLimit = t1.CreditLimit,
                                                                NID = t1.NID,
                                                                CustomerTypeFk = t1.CustomerTypeFK,
                                                                SecurityAmount = t1.SecurityAmount,
                                                                CustomerStatus = t1.CustomerStatus ?? 1,
                                                                PaymentType = t1.CustomerType,
                                                                Propietor = t1.Propietor
                                                            }).OrderByDescending(x => x.ID).AsEnumerable());



            return vmCommonDeport;
        }
        public async Task<int> DeportAdd(VMCommonSupplier vmCommonDeport)
        {
            var result = -1;
            #region Genarate Deport code
            int totalDeport = _db.Vendors.Count(x => x.VendorTypeId == (int)Provider.Deport && x.CompanyId == vmCommonDeport.CompanyFK);

            if (totalDeport == 0)
            {
                totalDeport = 1;
            }
            else
            {
                totalDeport++;
            }

            var newString = totalDeport.ToString().PadLeft(4, '0');
            #endregion

            Vendor commonDeport = new Vendor
            {
                Name = vmCommonDeport.Name,
                Phone = vmCommonDeport.Phone,
                Email = vmCommonDeport.Email,

                //DistrictId = vmCommonDeport.Common_DistrictsFk,
                //UpazilaId = vmCommonDeport.Common_UpazilasFk,
                ZoneId = vmCommonDeport.ZoneId,
                ZoneDivisionId = vmCommonDeport.ZoneDivisionId,
                RegionId = vmCommonDeport.RegionId,
                SubZoneId = vmCommonDeport.SubZoneId,
                Address = vmCommonDeport.Address,

                ContactName = vmCommonDeport.ContactPerson,
                VendorTypeId = (int)Provider.Deport,

                CreditLimit = vmCommonDeport.CreditLimit,
                SecurityAmount = vmCommonDeport.SecurityAmount,
                CustomerTypeFK = vmCommonDeport.CustomerTypeFk,
                CompanyId = vmCommonDeport.CompanyFK.Value,

                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,


                CustomerStatus = vmCommonDeport.CustomerStatus,
                Propietor = vmCommonDeport.Propietor,
                NID = vmCommonDeport.NID,
                BusinessAddress = vmCommonDeport.BusinessAddress,
                CustomerType = vmCommonDeport.PaymentType,
                IsActive = true,
                CreditRatioFrom = 0,
                CreditRatioTo = 0,
                IsForeign = false,

                NomineeName = vmCommonDeport.NomineeName,
                NomineePhone = vmCommonDeport.NomineePhone,
                NomineeRelation = vmCommonDeport.NomineeRelation,
                NomineeNID = vmCommonDeport.NomineeNID,


            };
            _db.Vendors.Add(commonDeport);

            if (await _db.SaveChangesAsync() > 0)
            {
                result = commonDeport.VendorId;
            }

            return result;
        }

        public async Task<int> DeportEdit(VMCommonSupplier vmCommonDeport)
        {
            var result = -1;

            Vendor commonDeport = await _db.Vendors.FindAsync(vmCommonDeport.ID);

            commonDeport.Name = vmCommonDeport.Name;
            commonDeport.Phone = vmCommonDeport.Phone;
            commonDeport.Email = vmCommonDeport.Email;
            commonDeport.NID = vmCommonDeport.NID;

            //commonDeport.DistrictId = vmCommonDeport.Common_DistrictsFk;
            //commonDeport.UpazilaId = vmCommonDeport.Common_UpazilasFk;
            commonDeport.ZoneId = vmCommonDeport.ZoneId;
            commonDeport.ZoneDivisionId = vmCommonDeport.ZoneDivisionId;
            commonDeport.RegionId = vmCommonDeport.RegionId;
            commonDeport.SubZoneId = vmCommonDeport.SubZoneId;
            commonDeport.Address = vmCommonDeport.Address;

            commonDeport.SecurityAmount = vmCommonDeport.SecurityAmount;
            commonDeport.CreditLimit = vmCommonDeport.CreditLimit;

            commonDeport.Remarks = vmCommonDeport.Remarks;
            commonDeport.CompanyId = vmCommonDeport.CompanyFK.Value;
            commonDeport.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            commonDeport.ModifiedDate = DateTime.Now;
            commonDeport.ContactName = vmCommonDeport.ContactPerson;
            commonDeport.CustomerTypeFK = vmCommonDeport.CustomerTypeFk;

            commonDeport.CustomerStatus = vmCommonDeport.CustomerStatus;
            commonDeport.Propietor = vmCommonDeport.Propietor;
            commonDeport.NomineeName = vmCommonDeport.NomineeName;
            commonDeport.NomineePhone = vmCommonDeport.NomineePhone;

            commonDeport.BusinessAddress = vmCommonDeport.BusinessAddress;
            commonDeport.NomineeNID = vmCommonDeport.NomineeNID;
            commonDeport.NomineeRelation = vmCommonDeport.NomineeRelation;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = commonDeport.VendorId;
            }

            return result;
        }

        public async Task<int> DeportDelete(int id)
        {
            int result = -1;
            if (id != 0)
            {
                Vendor commonDeport = await _db.Vendors.FindAsync(id);
                commonDeport.IsActive = false;
                commonDeport.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                commonDeport.ModifiedDate = DateTime.Now;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = commonDeport.VendorId;
                }
            }
            return result;
        }
        #endregion

        #region Common Dealer

        public async Task<VMCommonSupplier> GetDealerById(int deportId)
        {
            VMCommonSupplier vmCommonDealer = new VMCommonSupplier();
            vmCommonDealer = await Task.Run(() => (from t1 in _db.Vendors.Where(x => x.IsActive == true && x.VendorTypeId == (int)Provider.Dealer && x.VendorId == deportId)
                                                   join t2 in _db.Upazilas on t1.UpazilaId equals t2.UpazilaId
                                                   join t3 in _db.Districts on t2.DistrictId equals t3.DistrictId
                                                   //join t4 in _db.Divisions on t3.DivisionId equals t4.DivisionId
                                                   join t5 in _db.SubZones on t1.SubZoneId equals t5.SubZoneId
                                                   join t6 in _db.Zones on t5.ZoneId equals t6.ZoneId
                                                   select new VMCommonSupplier
                                                   {
                                                       ID = t1.VendorId,
                                                       Name = t1.Name,
                                                       Email = t1.Email,
                                                       ContactPerson = t1.ContactName,
                                                       Address = t1.Address,
                                                       Code = t1.Code,
                                                       Common_DistrictsFk = t2.DistrictId,
                                                       Common_UpazilasFk = t1.UpazilaId.Value,
                                                       District = t3.Name,
                                                       Upazila = t2.Name,
                                                       //Country = t4.Name,
                                                       CreatedBy = t1.CreatedBy,
                                                       // Division = t4.Name,
                                                       Remarks = t1.Remarks,
                                                       CompanyFK = t1.CompanyId,
                                                       Phone = t1.Phone,
                                                       ZoneName = t5.Name,
                                                       ZoneIncharge = t6.ZoneIncharge,
                                                       CreditLimit = t1.CreditLimit,
                                                       NID = t1.NID,
                                                       CustomerTypeFk = t1.CustomerTypeFK,
                                                       VendorTypeId = t1.VendorTypeId,
                                                       ParentId = t1.ParentId
                                                   }).FirstOrDefault());



            return vmCommonDealer;
        }
        public VMCommonSupplier GetCommonDealerById(int id)
        {
            var v = (from t1 in _db.Vendors.Where(x => x.VendorTypeId == (int)Provider.Dealer && x.VendorId == id)
                     join t2 in _db.SubZones on t1.SubZoneId equals t2.SubZoneId into t2_def
                     from t2 in t2_def.DefaultIfEmpty()
                     join t3 in _db.Upazilas on t1.UpazilaId equals t3.UpazilaId into t3_def
                     from t3 in t3_def.DefaultIfEmpty()
                     join t4 in _db.Districts on t1.DistrictId equals t4.DistrictId into t4_def
                     from t4 in t4_def.DefaultIfEmpty()
                     join t5 in _db.ZoneDivisions on t1.ZoneDivisionId equals t5.ZoneDivisionId into t5_def
                     from t5 in t5_def.DefaultIfEmpty()

                     select new VMCommonSupplier
                     {
                         ID = t1.VendorId,
                         Name = t1.Name,
                         Email = t1.Email,
                         Phone = t1.Phone,
                         CompanyFK = t1.CompanyId,

                         CustomerTypeFk = t1.CustomerTypeFK,
                         ZoneId = t2.ZoneId,
                         ZoneDivisionId = t1.ZoneDivisionId,
                         RegionId = t1.RegionId,
                         AreaId = t1.AreaId,
                         SubZoneId = t1.SubZoneId.Value,
                         //Common_DivisionFk = t4.DivisionId > 0 ? t4.DivisionId : 0,
                         //Common_DistrictsFk = t3.DistrictId > 0 ? t3.DistrictId : 0,
                         //Common_UpazilasFk = t3.UpazilaId > 0 ? t3.UpazilaId : 0,
                         ContactPerson = t1.ContactName,
                         Address = t1.Address,
                         Code = t1.Code,
                         CreatedBy = t1.CreatedBy,
                         Remarks = t1.Remarks,
                         IsForeign = t1.IsForeign,
                         CreditLimit = t1.CreditLimit,
                         NID = t1.NID,
                         SecurityAmount = t1.SecurityAmount,
                         CustomerStatus = t1.CustomerStatus ?? 1,
                         Propietor = t1.Propietor,
                         PaymentType = t1.CustomerType,
                         ParentId = t1.ParentId

                     }).FirstOrDefault();
            return v;
        }
        public async Task<VMCommonSupplier> GetDealer(int companyId, int zoneId, int subZoneId)
        {
            VMCommonSupplier vmCommonDealer = new VMCommonSupplier();
            vmCommonDealer.CompanyFK = companyId;
            vmCommonDealer.DataList = await Task.Run(() => (from t1 in _db.Vendors.Where(x => x.IsActive == true && x.VendorTypeId == (int)Provider.Dealer && x.CompanyId == companyId)
                                                            join t2 in _db.Upazilas on t1.UpazilaId equals t2.UpazilaId into t2_def
                                                            from t2 in t2_def.DefaultIfEmpty()
                                                            join t3 in _db.Districts on t1.DistrictId equals t3.DistrictId into t3_def
                                                            from t3 in t3_def.DefaultIfEmpty()
                                                            join t4 in _db.Divisions on t3.DivisionId equals t4.DivisionId into t4_def
                                                            from t4 in t4_def.DefaultIfEmpty()
                                                            join t5 in _db.SubZones on t1.SubZoneId equals t5.SubZoneId into t5_def
                                                            from t5 in t5_def.DefaultIfEmpty()
                                                            join t6 in _db.Zones on t1.ZoneId equals t6.ZoneId into t6_def
                                                            from t6 in t6_def.DefaultIfEmpty()
                                                            join t7 in _db.ZoneDivisions on t1.ZoneDivisionId equals t7.ZoneDivisionId into t7_def
                                                            from t7 in t7_def.DefaultIfEmpty()
                                                            join t8 in _db.Countries on t1.CountryId equals t8.CountryId into t8_def
                                                            from t8 in t8_def.DefaultIfEmpty()
                                                            join t9 in _db.Regions on t1.RegionId equals t9.RegionId into t9_def
                                                            from t9 in t9_def.DefaultIfEmpty()
                                                            join t10 in _db.Areas on t1.AreaId equals t10.AreaId into t10_def
                                                            from t10 in t10_def.DefaultIfEmpty()

                                                            where ((zoneId > 0) && (subZoneId == 0) ? t1.ZoneId == zoneId :
                                                                     (zoneId > 0) && (subZoneId > 0) ? t1.SubZoneId == subZoneId :
                                                              t1.VendorId > 0)
                                                            select new VMCommonSupplier
                                                            {
                                                                ID = t1.VendorId,
                                                                Name = t1.Name,
                                                                Email = t1.Email,
                                                                ContactPerson = t1.ContactName,
                                                                Address = t1.Address,
                                                                Code = t1.Code,
                                                                EmployeeId = t1.EmployeeId,
                                                                Common_DistrictsFk = t2.DistrictId > 0 ? t2.DistrictId : 0,
                                                                Common_UpazilasFk = t1.UpazilaId.Value > 0 ? t1.UpazilaId.Value : 0,
                                                                District = t3.Name,
                                                                Upazila = t2.Name,
                                                                Division = t4.Name,
                                                                Country = t8.CountryName,
                                                                CreatedBy = t1.CreatedBy,
                                                                Remarks = t1.Remarks,
                                                                CompanyFK = t1.CompanyId,
                                                                Phone = t1.Phone,
                                                                ZoneId = t1.ZoneId ?? 0,
                                                                ZoneDivisionId = t1.ZoneDivisionId,
                                                                ZoneDivisionName = t7.Name,
                                                                RegionId = t1.RegionId,
                                                                RegionName = t9.Name,
                                                                AreaId = t1.AreaId,
                                                                AreaName = t10.Name,
                                                                SubZoneId = t1.SubZoneId ?? 0,
                                                                SubZoneName = t5.Name,
                                                                ZoneName = t6.Name,
                                                                ZoneIncharge = t6.ZoneIncharge,
                                                                CreditLimit = t1.CreditLimit,
                                                                NID = t1.NID,
                                                                CustomerTypeFk = t1.CustomerTypeFK,
                                                                SecurityAmount = t1.SecurityAmount,
                                                                CustomerStatus = t1.CustomerStatus ?? 1,
                                                                PaymentType = t1.CustomerType,
                                                                Propietor = t1.Propietor,
                                                                ParentId = t1.ParentId
                                                            }).OrderByDescending(x => x.ID).AsEnumerable());



            return vmCommonDealer;
        }
        public async Task<int> DealerAdd(VMCommonSupplier vmCommonDealer)
        {
            var result = -1;
            #region Genarate Dealer code
            int totalDealer = _db.Vendors.Count(x => x.VendorTypeId == (int)Provider.Dealer && x.CompanyId == vmCommonDealer.CompanyFK);

            if (totalDealer == 0)
            {
                totalDealer = 1;
            }
            else
            {
                totalDealer++;
            }

            var newString = totalDealer.ToString().PadLeft(4, '0');
            #endregion

            Vendor commonDealer = new Vendor
            {
                Name = vmCommonDealer.Name,
                Phone = vmCommonDealer.Phone,
                Email = vmCommonDealer.Email,

                //DistrictId = vmCommonDealer.Common_DistrictsFk,
                //UpazilaId = vmCommonDealer.Common_UpazilasFk,
                ZoneId = vmCommonDealer.ZoneId,
                ZoneDivisionId = vmCommonDealer.ZoneDivisionId,
                RegionId = vmCommonDealer.RegionId,
                AreaId = vmCommonDealer.AreaId,
                SubZoneId = vmCommonDealer.SubZoneId,
                Address = vmCommonDealer.Address,

                ContactName = vmCommonDealer.ContactPerson,
                VendorTypeId = (int)Provider.Dealer,

                CreditLimit = vmCommonDealer.CreditLimit,
                SecurityAmount = vmCommonDealer.SecurityAmount,
                CustomerTypeFK = vmCommonDealer.CustomerTypeFk,
                CompanyId = vmCommonDealer.CompanyFK.Value,

                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,


                CustomerStatus = vmCommonDealer.CustomerStatus,
                Propietor = vmCommonDealer.Propietor,
                NID = vmCommonDealer.NID,
                BusinessAddress = vmCommonDealer.BusinessAddress,
                CustomerType = vmCommonDealer.PaymentType,
                IsActive = true,
                CreditRatioFrom = 0,
                CreditRatioTo = 0,
                IsForeign = false,

                NomineeName = vmCommonDealer.NomineeName,
                NomineePhone = vmCommonDealer.NomineePhone,
                NomineeRelation = vmCommonDealer.NomineeRelation,
                NomineeNID = vmCommonDealer.NomineeNID,
                ParentId = vmCommonDealer.ParentId

            };
            _db.Vendors.Add(commonDealer);

            if (await _db.SaveChangesAsync() > 0)
            {
                result = commonDealer.VendorId;
            }

            return result;
        }

        public async Task<int> DealerEdit(VMCommonSupplier vmCommonDealer)
        {
            var result = -1;

            Vendor commonDealer = await _db.Vendors.FindAsync(vmCommonDealer.ID);

            commonDealer.Name = vmCommonDealer.Name;
            commonDealer.Phone = vmCommonDealer.Phone;
            commonDealer.Email = vmCommonDealer.Email;
            commonDealer.NID = vmCommonDealer.NID;

            //commonDealer.DistrictId = vmCommonDealer.Common_DistrictsFk;
            //commonDealer.UpazilaId = vmCommonDealer.Common_UpazilasFk;
            commonDealer.ZoneId = vmCommonDealer.ZoneId;
            commonDealer.ZoneDivisionId = vmCommonDealer.ZoneDivisionId;
            commonDealer.RegionId = vmCommonDealer.RegionId;
            commonDealer.AreaId = vmCommonDealer.AreaId;
            commonDealer.SubZoneId = vmCommonDealer.SubZoneId;
            commonDealer.Address = vmCommonDealer.Address;

            commonDealer.SecurityAmount = vmCommonDealer.SecurityAmount;
            commonDealer.CreditLimit = vmCommonDealer.CreditLimit;

            commonDealer.Remarks = vmCommonDealer.Remarks;
            commonDealer.CompanyId = vmCommonDealer.CompanyFK.Value;
            commonDealer.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            commonDealer.ModifiedDate = DateTime.Now;
            commonDealer.ContactName = vmCommonDealer.ContactPerson;
            commonDealer.CustomerTypeFK = vmCommonDealer.CustomerTypeFk;

            commonDealer.CustomerStatus = vmCommonDealer.CustomerStatus;
            commonDealer.Propietor = vmCommonDealer.Propietor;
            commonDealer.NomineeName = vmCommonDealer.NomineeName;
            commonDealer.NomineePhone = vmCommonDealer.NomineePhone;

            commonDealer.BusinessAddress = vmCommonDealer.BusinessAddress;
            commonDealer.NomineeNID = vmCommonDealer.NomineeNID;
            commonDealer.NomineeRelation = vmCommonDealer.NomineeRelation;
            commonDealer.ParentId = vmCommonDealer.ParentId;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = commonDealer.VendorId;
            }

            return result;
        }

        public async Task<int> DealerDelete(int id)
        {
            int result = -1;
            if (id != 0)
            {
                Vendor commonDealer = await _db.Vendors.FindAsync(id);
                commonDealer.IsActive = false;
                commonDealer.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                commonDealer.ModifiedDate = DateTime.Now;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = commonDealer.VendorId;
                }
            }
            return result;
        }
        #endregion

        #region Division District Upazila
        public async Task<VMCommonDivisions> GetDivisions()
        {
            VMCommonDivisions vmCommonDivisions = new VMCommonDivisions();

            vmCommonDivisions.DataList = await Task.Run(() => (from t1 in _db.Divisions
                                                               select new VMCommonDivisions
                                                               {
                                                                   ID = t1.DivisionId,
                                                                   Name = t1.Name
                                                               }).OrderByDescending(x => x.ID).AsEnumerable());
            return vmCommonDivisions;
        }
        public async Task<VMCommonDistricts> GetDistricts(int divisionsId = 0)
        {
            VMCommonDistricts vmCommonDistricts = new VMCommonDistricts();

            vmCommonDistricts.DataList = await Task.Run(() => (from t1 in _db.Districts
                                                               join t2 in _db.Divisions on t1.DivisionId equals t2.DivisionId
                                                               where t1.IsActive == true
                                                               && ((divisionsId > 0) ? t1.DivisionId == divisionsId : t1.DistrictId > 0)
                                                               select new VMCommonDistricts
                                                               {
                                                                   Common_DivisionsFk = t1.DivisionId,
                                                                   ID = t1.DistrictId,
                                                                   DivisionsName = t2.Name,
                                                                   Name = t1.Name,
                                                                   ShortName = t1.ShortName
                                                               }).OrderByDescending(x => x.ID).AsEnumerable());
            return vmCommonDistricts;
        }

        public List<District> GetDistrictsDropDownList()
        {

            var list = _db.Districts.Where(a => a.IsActive == true).ToList();

            return list;
        }
        public async Task<VMCommonThana> GetUpazilas(int divisionsId = 0, int districtsId = 0)
        {
            VMCommonThana vmCommonThana = new VMCommonThana();

            vmCommonThana.DataList = await Task.Run(() => (from t1 in _db.Upazilas
                                                           join t2 in _db.Districts on t1.DistrictId equals t2.DistrictId
                                                           join t3 in _db.Divisions on t2.DivisionId equals t3.DivisionId

                                                           where t1.IsActive == true && t2.IsActive == true &&
                                                           ((divisionsId > 0 && districtsId == 0) ? t2.DivisionId == divisionsId
                                                           : (divisionsId == 0 && districtsId > 0) ? t1.DistrictId == districtsId
                                                           : t1.UpazilaId > 0)
                                                           select new VMCommonThana
                                                           {
                                                               ID = t1.UpazilaId,
                                                               Name = t1.Name,
                                                               ShortName = t1.ShortName,
                                                               Common_DistrictsFk = t2.DistrictId,
                                                               Common_DivisionsFk = t3.DivisionId,
                                                               DistictName = t2.Name,

                                                               DivisionsName = t3.Name

                                                           }).OrderByDescending(x => x.ID).AsEnumerable());
            return vmCommonThana;
        }
        #endregion

        public List<object> DDLDistrictListByDivisionID(int divisionId)
        {
            var list = new List<object>();
            var vData = _db.Districts.Where(a => a.IsActive == true && a.DivisionId == divisionId).ToList();
            foreach (var x in vData)
            {
                list.Add(new { Text = x.Name, Value = x.DistrictId });
            }
            return list;
        }

        public List<object> DDLUpazilasListByDistrictID(int districtId)
        {
            var list = new List<object>();
            var vData = _db.Upazilas.Where(a => a.IsActive == true && a.DistrictId == districtId).ToList();
            foreach (var x in vData)
            {
                list.Add(new { Text = x.Name, Value = x.UpazilaId });
            }
            return list;
        }

        public List<object> GetVendorsPaymentMethodEnum()
        {
            var PaymentTypeList = new List<object>();

            foreach (var eVal in Enum.GetValues(typeof(VendorsPaymentMethodEnum)))
            {
                PaymentTypeList.Add(new SelectListItem { Text = Enum.GetName(typeof(VendorsPaymentMethodEnum), eVal), Value = (Convert.ToInt32(eVal)).ToString() });
            }
            return PaymentTypeList;
        }


        public List<object> CommonActSignatoryDropdownList(int companyId)
        {
            var list = new List<object>();
            var v = _db.Accounting_Signatory.Where(x => x.CompanyId == companyId).ToList();
            foreach (var x in v)
            {
                list.Add(new { Text = x.SignatoryName, Value = x.SignatoryId });
            }
            return list;
        }
        public async Task<VMAccountingSignatory> GetAccountingSignatory(int companyId)
        {
            VMAccountingSignatory vmAccountingSignatory = new VMAccountingSignatory();
            vmAccountingSignatory.CompanyFK = companyId;
            vmAccountingSignatory.DataList = await Task.Run(() => (from t1 in _db.Accounting_Signatory
                                                                   join t2 in _db.Companies on t1.CompanyId equals t2.CompanyId
                                                                   where t1.CompanyId == companyId && t1.IsActive == true

                                                                   select new VMAccountingSignatory
                                                                   {
                                                                       SignatoryId = t1.SignatoryId,
                                                                       SignatoryType = t1.SignatoryType,
                                                                       SignatoryName = t1.SignatoryName,
                                                                       CompanyName = t2.Name,
                                                                       OrderBy = t1.OrderBy,
                                                                       CompanyFK = t1.CompanyId
                                                                   }).OrderByDescending(x => x.SignatoryId).AsEnumerable());
            return vmAccountingSignatory;
        }

        #region Accounting Signatory

        public async Task<int> AccountingSignatoryAdd(VMAccountingSignatory vmAccountingSignatory)
        {
            var result = -1;
            Accounting_Signatory commo = new Accounting_Signatory
            {

                SignatoryName = vmAccountingSignatory.SignatoryName,
                SignatoryType = vmAccountingSignatory.SignatoryType,
                OrderBy = vmAccountingSignatory.OrderBy,
                CompanyId = vmAccountingSignatory.CompanyFK.Value,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true,
                Priority = 1
            };
            _db.Accounting_Signatory.Add(commo);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = commo.SignatoryId;
            }
            return result;
        }
        public async Task<int> AccountingSignatoryEdit(VMAccountingSignatory vmAccountingSignatory)
        {

            var result = -1;
            Accounting_Signatory accountingSignatory = _db.Accounting_Signatory.Find(vmAccountingSignatory.SignatoryId);
            accountingSignatory.CompanyId = vmAccountingSignatory.CompanyFK.Value;
            accountingSignatory.SignatoryType = vmAccountingSignatory.SignatoryType;
            accountingSignatory.OrderBy = vmAccountingSignatory.OrderBy;

            accountingSignatory.SignatoryName = vmAccountingSignatory.SignatoryName;
            accountingSignatory.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            accountingSignatory.ModifiedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = accountingSignatory.SignatoryId;
            }
            return result;
        }
        public async Task<int> AccountingSignatoryDelete(int id)
        {
            int result = -1;

            if (id != 0)
            {
                Accounting_Signatory accountingSignatory = _db.Accounting_Signatory.Find(id);
                accountingSignatory.IsActive = false;

                if (await _db.SaveChangesAsync() > 0)
                {
                    result = accountingSignatory.SignatoryId;
                }
            }
            return result;
        }

        #endregion


        public async Task<VMCompany> GetCompany()
        {
            VMCompany VMCompany = new VMCompany();

            VMCompany.DataList = await Task.Run(() => (from x in _db.Companies


                                                       select new VMCompany
                                                       {
                                                           ID = x.CompanyId,
                                                           Name = x.Name,
                                                           ShortName = x.ShortName,
                                                           OrderNo = x.OrderNo,
                                                           MushokNo = x.MushokNo,
                                                           IsCompany = x.IsCompany
                                                           //CompanyLogo = string.Format("{0}://{1}", HttpContext.Request.Url.Scheme, HttpContext.Request.Url.Authority) + "/Images/Logo/" + (string.IsNullOrEmpty(x.CompanyLogo) ? "logo.png" : x.CompanyLogo),
                                                       }));
            return VMCompany;
        }

        public async Task<int> CompanyAdd(VMCompany vmCompany)
        {

            var result = -1;
            //if (!string.IsNullOrEmpty(VMCompany.CompanyLogo))
            //{
            //   commoLogo = VMCompany.CompanyLogo;
            //}
            Company commo = new Company
            {
                CompanyId = _db.Database.SqlQuery<int>("exec spGetNewCompanyId").FirstOrDefault(),
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,

                Name = vmCompany.Name,
                ShortName = vmCompany.ShortName,
                OrderNo = vmCompany.OrderNo,
                MushokNo = vmCompany.MushokNo,
                Address = vmCompany.Address,
                Phone = vmCompany.Phone,
                Fax = vmCompany.Fax,
                Email = vmCompany.Email,
                //CompanyLogo = commoLogo,
                IsCompany = vmCompany.IsCompany,
                IsActive = vmCompany.IsActive,

            };
            //SignatoryName = VMCompany.SignatoryName,
            //    SignatoryType = VMCompany.SignatoryType,
            //    CompanyId = VMCompany.CompanyFK.Value,
            //    CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
            //    CreatedDate = DateTime.Now,
            //    IsActive = true,
            //    Priority = 1

            _db.Companies.Add(commo);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = commo.CompanyId;
            }
            return result;
        }

        #region Bank

        public List<object> CommonBanksDropDownList(int companyId)
        {
            var list = new List<object>();
            var v = _db.Banks.Where(x => x.CompanyId == companyId && x.IsActive == true).ToList();
            foreach (var x in v)
            {
                list.Add(new { Text = x.Name, Value = x.BankId });
            }
            return list;
        }

        public async Task<VMCommonBank> GetBanks(int companyId)
        {
            VMCommonBank vMCommonBank = new VMCommonBank();

            vMCommonBank.CompanyFK = companyId;

            vMCommonBank.DataList = await Task.Run(() => (from t1 in _db.Banks
                                                          where t1.IsActive == true && t1.CompanyId == companyId
                                                          select new VMCommonBank
                                                          {
                                                              ID = t1.BankId,
                                                              Name = t1.Name,
                                                              CompanyFK = t1.CompanyId
                                                          }).OrderByDescending(x => x.ID).AsEnumerable());

            return vMCommonBank;
        }


        public async Task<int> BankAdd(VMCommonBank vMCommonBank)
        {
            var result = -1;

            #region IsExist
            var isExist = await _db.Banks.FirstOrDefaultAsync(u => u.Name.Equals(vMCommonBank.Name) && u.IsActive == true);

            if (isExist?.BankId > 0)
            {
                throw new Exception($"Sorry! This Name {vMCommonBank.Name} already Exists!");
            }
            #endregion

            Bank bank = new Bank
            {
                Name = vMCommonBank.Name,
                CompanyId = vMCommonBank.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true

            };
            _db.Banks.Add(bank);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = bank.BankId;
            }
            return result;
        }

        public async Task<int> BankEdit(VMCommonBank vMCommonBank)
        {
            var result = -1;

            #region IsExist
            var isExist = await _db.Banks.FirstOrDefaultAsync(u => u.Name.Equals(vMCommonBank.Name) && u.BankId != vMCommonBank.ID && u.IsActive == true);

            if (isExist?.BankId > 0)
            {
                throw new Exception($"Sorry! This Name {vMCommonBank.Name} already Exists!");
            }
            #endregion

            Bank bank = _db.Banks.Find(vMCommonBank.ID);
            bank.Name = vMCommonBank.Name;


            bank.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            bank.ModifiedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = bank.BankId;
            }
            return result;
        }


        public async Task<int> BankDelete(int id)
        {
            int result = -1;
            if (id != 0)
            {
                Bank bank = await _db.Banks.FindAsync(id);
                bank.IsActive = false;

                if (await _db.SaveChangesAsync() > 0)
                {
                    result = bank.BankId;
                }
            }
            return result;
        }
        #endregion

        #region Bank Branch

        public List<object> CommonBankBranchesDropDownList(int companyId)
        {
            var list = new List<object>();
            var v = _db.BankBranches.Where(x => x.CompanyId == companyId && x.IsActive == true).ToList();
            foreach (var x in v)
            {
                list.Add(new { Text = x.Name, Value = x.BankId });
            }
            return list;
        }

        public async Task<List<VMCommonBankBranch>> CommonBankGet(int companyId, int bankId)
        {

            List<VMCommonBankBranch> vMCommonBankBranches =
                await Task.Run(() => (_db.BankBranches
                .Where(x => x.IsActive == true && x.BankId == bankId && x.CompanyId == companyId))
                .Select(x => new VMCommonBankBranch() { ID = x.BankBranchId, Name = x.Name })
                .ToListAsync());


            return vMCommonBankBranches;
        }

        public async Task<VMCommonBankBranch> GetBankBranches(int companyId, int bankId = 0)
        {
            VMCommonBankBranch vmCommonBankBranch = new VMCommonBankBranch();
            vmCommonBankBranch.CompanyFK = companyId;
            vmCommonBankBranch.DataList = await Task.Run(() => (from t1 in _db.BankBranches
                                                                join t2 in _db.Banks on t1.BankId equals t2.BankId
                                                                where t1.IsActive == true && t1.CompanyId == companyId
                                                                && (bankId > 0 ? t1.BankId == bankId : t1.BankBranchId > 0)
                                                                select new VMCommonBankBranch
                                                                {
                                                                    ID = t1.BankBranchId,
                                                                    BankId = t2.BankId,
                                                                    Name = t1.Name,
                                                                    BankName = t2.Name,
                                                                    Address = t1.Address,
                                                                    CompanyFK = t1.CompanyId
                                                                }).OrderByDescending(x => x.ID).AsEnumerable());
            return vmCommonBankBranch;
        }

        public async Task<int> BankBranchAdd(VMCommonBankBranch vMCommonBankBranch)
        {
            var result = -1;

            #region IsExist
            var isExist = await _db.BankBranches.FirstOrDefaultAsync(u => u.Name.Equals(vMCommonBankBranch.BankName) && u.BankId == vMCommonBankBranch.BankId && u.IsActive == true);

            if (isExist?.BankId > 0)
            {
                throw new Exception($"Sorry! This Name {vMCommonBankBranch.BankName} already Exists!");
            }
            #endregion

            BankBranch bankBranch = new BankBranch
            {
                Name = vMCommonBankBranch.Name,
                Address = vMCommonBankBranch.Address,
                BankId = vMCommonBankBranch.BankId,
                CompanyId = vMCommonBankBranch.CompanyFK.Value,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _db.BankBranches.Add(bankBranch);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = bankBranch.BankBranchId;
            }
            return result;
        }
        public async Task<int> BankBranchEdit(VMCommonBankBranch vMCommonBankBranch)
        {
            var result = -1;

            #region IsExist
            var isExist = await _db.BankBranches.FirstOrDefaultAsync(u => u.Name.Equals(vMCommonBankBranch.BankName) && u.BankId == vMCommonBankBranch.BankId && u.BankBranchId != vMCommonBankBranch.ID && u.IsActive == true);

            if (isExist?.BankId > 0)
            {
                throw new Exception($"Sorry! This Name {vMCommonBankBranch.BankName} already Exists!");
            }
            #endregion

            BankBranch bankBranch = _db.BankBranches.Find(vMCommonBankBranch.ID);
            bankBranch.BankId = vMCommonBankBranch.BankId;
            bankBranch.Name = vMCommonBankBranch.Name;
            bankBranch.Address = vMCommonBankBranch.Address;
            bankBranch.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            bankBranch.ModifiedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = bankBranch.BankBranchId;
            }
            return result;
        }
        public async Task<int> BankBranchDelete(int id)
        {
            int result = -1;
            if (id != 0)
            {
                BankBranch bankBranch = await _db.BankBranches.FindAsync(id);
                bankBranch.IsActive = false;

                if (await _db.SaveChangesAsync() > 0)
                {
                    result = bankBranch.BankBranchId;
                }
            }
            return result;
        }

        #endregion

        #region Account Cheque Info
        public VMCommonActChequeInfo GetActChequeInfoByID(int id)
        {
            var v = (from t1 in _db.Accounting_ChequeInfo.Where(x => x.ActChequeInfoId == id)
                     join t2 in _db.BankBranches on t1.BankBranchId equals t2.BankBranchId
                     join t3 in _db.Accounting_Signatory on t1.SignatoryId equals t3.SignatoryId
                     select new VMCommonActChequeInfo
                     {
                         ID = t1.ActChequeInfoId,
                         BankBranchId = t1.BankBranchId,
                         BankId = t2.BankId ?? 0,
                         AccountNo = t1.AccountNo,
                         ChequeDate = t1.ChequeDate,
                         PayTo = t1.PayTo,
                         Amount = t1.Amount,
                         SignatoryId = t1.SignatoryId ?? 0,
                         SignatoryName = t3.SignatoryName,
                         BankName = t2.Bank.Name,
                         BankBranchName = t2.Name,
                         CompanyFK = t1.CompanyId
                     }).FirstOrDefault();
            return v;
        }

        public async Task<VMCommonActChequeInfo> GetActChequeInfos(int companyId)
        {
            VMCommonActChequeInfo vMCommonActChequeInfo = new VMCommonActChequeInfo();
            vMCommonActChequeInfo.CompanyFK = companyId;
            vMCommonActChequeInfo.DataList = await Task.Run(() => (from t1 in _db.Accounting_ChequeInfo
                                                                   join t2 in _db.BankBranches on t1.BankBranchId equals t2.BankBranchId
                                                                   join t3 in _db.Accounting_Signatory on t1.SignatoryId equals t3.SignatoryId
                                                                   where t1.IsActive == true && t1.CompanyId == companyId
                                                                   select new VMCommonActChequeInfo
                                                                   {
                                                                       ID = t1.ActChequeInfoId,
                                                                       BankBranchId = t2.BankBranchId,
                                                                       PayTo = t1.PayTo,
                                                                       AccountNo = t1.AccountNo,
                                                                       Amount = t1.Amount,
                                                                       CompanyFK = t1.CompanyId,
                                                                       BankBranchName = t2.Name,
                                                                       BankName = t2.Bank.Name,
                                                                       SignatoryName = t3.SignatoryName
                                                                   }).OrderByDescending(x => x.ID).AsEnumerable());
            return vMCommonActChequeInfo;
        }

        public async Task<int> ActChequeInfoAdd(VMCommonActChequeInfo vMCommonActChequeInfo)
        {
            var result = -1;
            Accounting_ChequeInfo accounting_ChequeInfo = new Accounting_ChequeInfo
            {
                AccountNo = vMCommonActChequeInfo.AccountNo,
                PayTo = vMCommonActChequeInfo.PayTo,
                Amount = vMCommonActChequeInfo.Amount,
                ChequeDate = vMCommonActChequeInfo.ChequeDate,
                BankBranchId = vMCommonActChequeInfo.BankBranchId,
                SignatoryId = vMCommonActChequeInfo.SignatoryId,
                CompanyId = vMCommonActChequeInfo.CompanyFK.Value,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _db.Accounting_ChequeInfo.Add(accounting_ChequeInfo);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = accounting_ChequeInfo.ActChequeInfoId;
            }
            return result;
        }
        public async Task<int> ActChequeInfoEdit(VMCommonActChequeInfo vMCommonActChequeInfo)
        {
            var result = -1;
            Accounting_ChequeInfo accounting_ChequeInfo = _db.Accounting_ChequeInfo
                .Find(vMCommonActChequeInfo.ID);

            accounting_ChequeInfo.BankBranchId = vMCommonActChequeInfo.BankBranchId;
            accounting_ChequeInfo.SignatoryId = vMCommonActChequeInfo.SignatoryId;
            accounting_ChequeInfo.PayTo = vMCommonActChequeInfo.PayTo;
            accounting_ChequeInfo.Amount = vMCommonActChequeInfo.Amount;

            accounting_ChequeInfo.AccountNo = vMCommonActChequeInfo.AccountNo;
            accounting_ChequeInfo.ChequeDate = vMCommonActChequeInfo.ChequeDate;

            accounting_ChequeInfo.CompanyId = vMCommonActChequeInfo.CompanyFK.Value;
            accounting_ChequeInfo.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            accounting_ChequeInfo.ModifiedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = accounting_ChequeInfo.ActChequeInfoId;
            }
            return result;
        }
        public async Task<int> ActChequeInfoDelete(int id)
        {
            int result = -1;
            if (id != 0)
            {
                Accounting_ChequeInfo accounting_ChequeInfo = await _db.Accounting_ChequeInfo.FindAsync(id);
                accounting_ChequeInfo.IsActive = false;

                if (await _db.SaveChangesAsync() > 0)
                {
                    result = accounting_ChequeInfo.ActChequeInfoId;
                }
            }
            return result;
        }

        #endregion


        public async Task<VMCommonProductCategory> GetProductCategoryProject(int companyId, string productType)
        {
            VMCommonProductCategory vmCommonProductCategory = new VMCommonProductCategory();
            vmCommonProductCategory.CompanyFK = companyId;
            vmCommonProductCategory.ProductType = productType;

            vmCommonProductCategory.DataList = await Task.Run(() => (from t1 in _db.ProductCategories
                                                                     where t1.ProductType == productType &&
                                                                     t1.IsActive == true && t1.CompanyId == companyId
                                                                     // productCategoryId > 0 ? t1.ProductCategoryId == productCategoryId: t1.ProductCategoryId > 0
                                                                     select new VMCommonProductCategory
                                                                     {
                                                                         ID = t1.ProductCategoryId,
                                                                         Name = t1.Name,
                                                                         ProductType = t1.ProductType,
                                                                         CompanyFK = t1.CompanyId,
                                                                         CashCommission = t1.CashCustomerRate,
                                                                         Remarks = t1.Remarks,
                                                                         Code = t1.Code,
                                                                         Description = t1.Address,
                                                                         IsCrm = t1.IsCrm

                                                                     }).OrderByDescending(x => x.ID).AsEnumerable());
            return vmCommonProductCategory;
        }

        public async Task<int> ProductCategoryProjectAdd(VMCommonProductCategory productCategoryModel)
        {
            var result = -1;
            //  int totalProject = _db.ProductCategories.Count(x => x.ProductType == productCategoryModel.ProductType && x.CompanyId == productCategoryModel.CompanyFK);

            ProductCategory productCategory = new ProductCategory
            {
                Code = productCategoryModel.Code,
                Name = productCategoryModel.Name,
                ProductType = productCategoryModel.ProductType,
                CashCustomerRate = productCategoryModel.CashCommission,
                Address = productCategoryModel.Description,
                Remarks = productCategoryModel.Remarks,
                CompanyId = productCategoryModel.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true

            };
            _db.ProductCategories.Add(productCategory);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = productCategory.ProductCategoryId;
                if (productCategory.CompanyId == (int)CompanyName.KrishibidPropertiesLimited)
                {
                    int head5Id = ProjectHead5Push(productCategory);

                    VMHeadIntegration integration = new VMHeadIntegration
                    {
                        AccName = productCategory.Name,
                        LayerNo = 6,
                        Remarks = "GL Layer",
                        IsIncomeHead = false,
                        ParentId = 42868,

                        CompanyFK = productCategory.CompanyId,
                        CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                        CreatedDate = DateTime.Now,
                    };
                    HeadGL headGlId = await PayableHeadIntegrationAdd(integration);
                    if (headGlId != null)
                    {
                        await KPLProjectCodeAndHeadGLIdEdit(productCategory.ProductCategoryId, headGlId, head5Id);
                    }

                }

            }
            return result;
        }

        public async Task<int> KPLProjectCodeAndHeadGLIdEdit(int supplierId, HeadGL headGl, int head5Id)
        {
            var result = -1;

            ProductCategory productCategories = await _db.ProductCategories.FindAsync(supplierId);
            productCategories.AccountingIncomeHeadId = headGl.Id;
            productCategories.AccountingHeadId = head5Id;

            productCategories.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            productCategories.ModifiedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = productCategories.ProductCategoryId;
            }
            return result;
        }


        private int ProjectHead5Push(ProductCategory productCategory)
        {
            int result = -1;
            string newAccountCode = "";
            int orderNo = 0;
            Head4 parentHead = _db.Head4.FirstOrDefault(x => x.Id == 41304);
            IQueryable<Head5> childHeads = _db.Head5.Where(x => x.ParentId == 41304);

            if (childHeads.Count() > 0)
            {
                string lastAccCode = childHeads.OrderByDescending(x => x.AccCode).FirstOrDefault()?.AccCode;
                string parentPart = lastAccCode.Substring(0, 7);
                string childPart = lastAccCode.Substring(7, 3);
                newAccountCode = parentPart + (Convert.ToInt32(childPart) + 1).ToString().PadLeft(3, '0');
                orderNo = childHeads.Count();
            }
            else
            {
                newAccountCode = parentHead.AccCode + "001";
                orderNo = orderNo + 1;
            }

            Head5 head5 = new Head5
            {
                Id = _db.Database.SqlQuery<int>("spGetNewId").FirstOrDefault(),
                AccCode = newAccountCode,
                AccName = productCategory.Name,
                ParentId = 41304, // Properties Accounts Receivable Head4 Id
                CompanyId = productCategory.CompanyId,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                LayerNo = 5,
                CreateDate = DateTime.Now,

                OrderNo = orderNo,
                Remarks = ""
            };
            _db.Head5.Add(head5);
            if (_db.SaveChanges() > 0)
            {
                result = head5.Id;
            }
            return result;

        }

        private int BlockHead5Push(ProductSubCategory productSubCategory)
        {
            int result = -1;
            string newAccountCode = "";
            int orderNo = 0;
            Head4 parentHead = _db.Head4.FirstOrDefault(x => x.Id == 50598716);
            IQueryable<Head5> childHeads = _db.Head5.Where(x => x.ParentId == 50598716);

            if (childHeads.Count() > 0)
            {
                string lastAccCode = childHeads.OrderByDescending(x => x.AccCode).FirstOrDefault()?.AccCode;
                string parentPart = lastAccCode.Substring(0, 7);
                string childPart = lastAccCode.Substring(7, 3);
                newAccountCode = parentPart + (Convert.ToInt32(childPart) + 1).ToString().PadLeft(3, '0');
                orderNo = childHeads.Count();
            }
            else
            {
                newAccountCode = parentHead.AccCode + "001";
                orderNo = orderNo + 1;
            }

            Head5 head5 = new Head5
            {
                Id = _db.Database.SqlQuery<int>("spGetNewId").FirstOrDefault(),
                AccCode = newAccountCode,
                AccName = productSubCategory.Name,
                ParentId = 41304, // Properties Accounts Receivable Head4 Id
                CompanyId = productSubCategory.CompanyId,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                LayerNo = 5,
                CreateDate = DateTime.Now,

                OrderNo = orderNo,
                Remarks = ""
            };
            _db.Head5.Add(head5);
            if (_db.SaveChanges() > 0)
            {
                result = head5.Id;
            }
            return result;

        }


        private int AccHead5Push(VMHeadIntegration vmModel, int id)
        {
            int result = -1;

            //List<Head5> head5s = new List<Head5>();
            if (vmModel.ProductType == "R")
            {

                Head5 head5_1 = new Head5
                {
                    Id = _db.Database.SqlQuery<int>("spGetNewId").FirstOrDefault(),
                    AccCode = GenerateHead5AccCode(30802),

                    AccName = vmModel.AccName,
                    ParentId = 30802, // Properties Accounts Receivable Head4 Id
                    CompanyId = vmModel.CompanyFK,
                    LayerNo = 5,
                    CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                    CreateDate = DateTime.Now,
                    IsActive = true,

                    OrderNo = 0,
                    Remarks = ""
                };



                _db.Head5.Add(head5_1);
                var categoryForAssets = _db.ProductCategories.SingleOrDefault(x => x.ProductCategoryId == id);
                categoryForAssets.AccountingHeadId = head5_1.Id;
                categoryForAssets.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                categoryForAssets.ModifiedDate = DateTime.Now;
                _db.SaveChanges();

                Head5 head5_2 = new Head5
                {
                    Id = _db.Database.SqlQuery<int>("spGetNewId").FirstOrDefault(),
                    AccCode = GenerateHead5AccCode(38017), // 
                    AccName = vmModel.AccName,
                    ParentId = 38017,// Properties Accounts Receivable Head4 Id
                    CompanyId = vmModel.CompanyFK,
                    LayerNo = 5,
                    IsActive = true,
                    OrderNo = 0,
                    Remarks = "",
                    CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                    CreateDate = DateTime.Now,
                };
                _db.Head5.Add(head5_2);
                var categoryForIncome = _db.ProductCategories.SingleOrDefault(x => x.ProductCategoryId == id);
                categoryForIncome.AccountingIncomeHeadId = head5_2.Id;
                categoryForIncome.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                categoryForIncome.ModifiedDate = DateTime.Now;
                _db.SaveChanges();


                Head5 head5_3 = new Head5
                {
                    Id = _db.Database.SqlQuery<int>("spGetNewId").FirstOrDefault(),
                    AccCode = GenerateHead5AccCode(31302), // 
                    AccName = vmModel.AccName,
                    ParentId = 31302,// Properties Accounts Receivable Head4 Id
                    CompanyId = vmModel.CompanyFK,
                    LayerNo = 5,
                    IsActive = true,
                    OrderNo = 0,
                    Remarks = "",
                    CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                    CreateDate = DateTime.Now,
                };
                _db.Head5.Add(head5_3);
                var categoryForExpanse = _db.ProductCategories.SingleOrDefault(x => x.ProductCategoryId == id);
                categoryForExpanse.AccountingExpenseHeadId = head5_3.Id;
                categoryForExpanse.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                categoryForExpanse.ModifiedDate = DateTime.Now;
                _db.SaveChanges();

            }

            if (vmModel.ProductType == "F")
            {

                Head5 head5_1 = new Head5
                {
                    Id = _db.Database.SqlQuery<int>("spGetNewId").FirstOrDefault(),
                    AccCode = GenerateHead5AccCode(4579), //Stock And Store

                    AccName = vmModel.AccName,
                    ParentId = 4579,// Properties Accounts Receivable Head4 Id
                    CompanyId = vmModel.CompanyFK,
                    LayerNo = 5,
                    IsActive = true,
                    OrderNo = 0,
                    Remarks = "",
                    CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                    CreateDate = DateTime.Now,
                };


                _db.Head5.Add(head5_1);
                var categoryForAssets = _db.ProductCategories.SingleOrDefault(x => x.ProductCategoryId == id);
                categoryForAssets.AccountingHeadId = head5_1.Id;
                categoryForAssets.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                categoryForAssets.ModifiedDate = DateTime.Now;
                _db.SaveChanges();

                Head5 head5_2 = new Head5
                {
                    Id = _db.Database.SqlQuery<int>("spGetNewId").FirstOrDefault(),
                    AccCode = GenerateHead5AccCode(50609029), // Income
                    AccName = vmModel.AccName,
                    ParentId = 50609029,// Properties Accounts Receivable Head4 Id
                    CompanyId = vmModel.CompanyFK,
                    LayerNo = 5,
                    IsActive = true,
                    OrderNo = 0,
                    Remarks = "",
                    CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                    CreateDate = DateTime.Now,
                };


                _db.Head5.Add(head5_2);
                var categoryForIncome = _db.ProductCategories.SingleOrDefault(x => x.ProductCategoryId == id);
                categoryForIncome.AccountingIncomeHeadId = head5_2.Id;
                categoryForIncome.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                categoryForIncome.ModifiedDate = DateTime.Now;
                _db.SaveChanges();


                Head5 head5_3 = new Head5
                {
                    Id = _db.Database.SqlQuery<int>("spGetNewId").FirstOrDefault(),
                    AccCode = GenerateHead5AccCode(50609623), // Expenses
                    AccName = vmModel.AccName,
                    ParentId = 50609623,// Properties Accounts Receivable Head4 Id
                    CompanyId = vmModel.CompanyFK,
                    LayerNo = 5,
                    IsActive = true,
                    OrderNo = 0,
                    Remarks = "",
                    CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                    CreateDate = DateTime.Now,
                };
                _db.Head5.Add(head5_3);
                var categoryForExpanse = _db.ProductCategories.SingleOrDefault(x => x.ProductCategoryId == id);
                categoryForExpanse.AccountingExpenseHeadId = head5_3.Id;
                categoryForExpanse.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                categoryForExpanse.ModifiedDate = DateTime.Now;
                _db.SaveChanges();

            }

            if (vmModel.ProductType == "P")
            {

                Head5 head5_1 = new Head5
                {
                    Id = _db.Database.SqlQuery<int>("spGetNewId").FirstOrDefault(),
                    AccCode = GenerateHead5AccCode(50606085), //Stock And Store

                    AccName = vmModel.AccName,
                    ParentId = 50606085,// Properties Accounts Receivable Head4 Id
                    CompanyId = vmModel.CompanyFK,
                    LayerNo = 5,
                    IsActive = true,
                    OrderNo = 0,
                    Remarks = "",
                    CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                    CreateDate = DateTime.Now,
                };


                _db.Head5.Add(head5_1);
                var categoryForAssets = _db.ProductCategories.SingleOrDefault(x => x.ProductCategoryId == id);
                categoryForAssets.AccountingHeadId = head5_1.Id;
                categoryForAssets.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                categoryForAssets.ModifiedDate = DateTime.Now;
                _db.SaveChanges();

                Head5 head5_2 = new Head5
                {
                    Id = _db.Database.SqlQuery<int>("spGetNewId").FirstOrDefault(),
                    AccCode = GenerateHead5AccCode(50613003), // Income
                    AccName = vmModel.AccName,
                    ParentId = 50613003, // Properties Accounts Receivable Head4 Id
                    CompanyId = vmModel.CompanyFK,
                    LayerNo = 5,
                    CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                    CreateDate = DateTime.Now,
                    IsActive = true,
                    OrderNo = 0,
                    Remarks = ""
                };

                _db.Head5.Add(head5_2);
                var categoryForIncome = _db.ProductCategories.SingleOrDefault(x => x.ProductCategoryId == id);
                categoryForIncome.AccountingIncomeHeadId = head5_2.Id;
                categoryForIncome.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                categoryForIncome.ModifiedDate = DateTime.Now;
                _db.SaveChanges();


                Head5 head5_3 = new Head5
                {
                    Id = _db.Database.SqlQuery<int>("spGetNewId").FirstOrDefault(),
                    AccCode = GenerateHead5AccCode(50606189), // Expenses
                    AccName = vmModel.AccName,
                    ParentId = 50606189, // Properties Accounts Receivable Head4 Id
                    CompanyId = vmModel.CompanyFK,
                    CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                    CreateDate = DateTime.Now,
                    LayerNo = 5,
                    IsActive = true,
                    OrderNo = 0,
                    Remarks = ""
                };

                _db.Head5.Add(head5_3);
                var categoryForExpanse = _db.ProductCategories.SingleOrDefault(x => x.ProductCategoryId == id);
                categoryForExpanse.AccountingExpenseHeadId = head5_3.Id;
                categoryForExpanse.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                categoryForExpanse.ModifiedDate = DateTime.Now;
                _db.SaveChanges();

            }

            return result;

        }

        private int AccHeadGlPush(VMHeadIntegration vmModel, ProductSubCategory productSubCategory)
        {
            int result = -1;
            ProductCategory productCategory = _db.ProductCategories.Find(productSubCategory.ProductCategoryId);
            //List<Head5> head5s = new List<Head5>();
            HeadGL headGl_1 = new HeadGL
            {
                Id = _db.Database.SqlQuery<int>("spGetNewId").FirstOrDefault(),
                AccCode = GenerateHeadGlAccCode(productCategory.AccountingHeadId.Value),

                AccName = vmModel.AccName,
                ParentId = productCategory.AccountingHeadId.Value, // Properties Accounts Receivable Head4 Id
                CompanyId = vmModel.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreateDate = DateTime.Now,
                LayerNo = 6,
                IsActive = true,
                OrderNo = 0,
                Remarks = "GL Layer"
            };


            _db.HeadGLs.Add(headGl_1);
            var categoryForAssets = _db.ProductSubCategories.SingleOrDefault(x => x.ProductSubCategoryId == productSubCategory.ProductSubCategoryId);
            categoryForAssets.AccountingHeadId = headGl_1.Id;
            categoryForAssets.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            categoryForAssets.ModifiedDate = DateTime.Now;
            _db.SaveChanges();

            HeadGL headGl_2 = new HeadGL
            {
                Id = _db.Database.SqlQuery<int>("spGetNewId").FirstOrDefault(),
                AccCode = GenerateHeadGlAccCode(productCategory.AccountingIncomeHeadId.Value),
                AccName = vmModel.AccName,
                ParentId = productCategory.AccountingIncomeHeadId.Value, // Properties Accounts Receivable Head4 Id
                CompanyId = vmModel.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreateDate = DateTime.Now,
                LayerNo = 6,
                IsActive = true,
                OrderNo = 0,
                Remarks = "GL Layer"
            };


            _db.HeadGLs.Add(headGl_2);
            var categoryForIncome = _db.ProductSubCategories.SingleOrDefault(x => x.ProductSubCategoryId == productSubCategory.ProductSubCategoryId);
            categoryForIncome.AccountingIncomeHeadId = headGl_2.Id;
            categoryForIncome.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            categoryForIncome.ModifiedDate = DateTime.Now;
            _db.SaveChanges();


            HeadGL headGl_3 = new HeadGL
            {
                Id = _db.Database.SqlQuery<int>("spGetNewId").FirstOrDefault(),
                AccCode = GenerateHeadGlAccCode(productCategory.AccountingExpenseHeadId.Value),
                AccName = vmModel.AccName,
                ParentId = productCategory.AccountingExpenseHeadId.Value, // Properties Accounts Receivable Head4 Id
                CompanyId = vmModel.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreateDate = DateTime.Now,
                LayerNo = 6,
                IsActive = true,
                OrderNo = 0,
                Remarks = "GL Layer"
            };


            _db.HeadGLs.Add(headGl_3);
            var categoryForExpanse = _db.ProductSubCategories.SingleOrDefault(x => x.ProductSubCategoryId == productSubCategory.ProductSubCategoryId);
            categoryForExpanse.AccountingExpenseHeadId = headGl_3.Id;
            categoryForExpanse.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            categoryForExpanse.ModifiedDate = DateTime.Now;
            _db.SaveChanges();

            return result;

        }


        private int SeedAccHeadGlPush(VMHeadIntegration vmModel)
        {
            int result = -1;

            //List<Head5> head5s = new List<Head5>();
            HeadGL headGl_1 = new HeadGL
            {
                Id = _db.Database.SqlQuery<int>("spGetNewId").FirstOrDefault(),
                AccCode = GenerateHeadGlAccCode(38059),

                AccName = vmModel.AccName,
                ParentId = 38059,
                CompanyId = vmModel.CompanyFK,
                LayerNo = 6,
                IsActive = true,
                OrderNo = 0,
                Remarks = "GL Layer",
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreateDate = DateTime.Now,
            };


            _db.HeadGLs.Add(headGl_1);
            var categoryForAssets = _db.ProductCategories.SingleOrDefault(x => x.ProductCategoryId == vmModel.ID);
            categoryForAssets.AccountingHeadId = headGl_1.Id;
            categoryForAssets.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            categoryForAssets.ModifiedDate = DateTime.Now;
            _db.SaveChanges();

            HeadGL headGl_2 = new HeadGL
            {
                Id = _db.Database.SqlQuery<int>("spGetNewId").FirstOrDefault(),
                AccCode = GenerateHeadGlAccCode(38018), // 
                AccName = vmModel.AccName,
                ParentId = 38018,
                CompanyId = vmModel.CompanyFK,
                LayerNo = 6,
                IsActive = true,
                OrderNo = 0,
                Remarks = "GL Layer",
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreateDate = DateTime.Now,
            };


            _db.HeadGLs.Add(headGl_2);
            var categoryForIncome = _db.ProductCategories.SingleOrDefault(x => x.ProductCategoryId == vmModel.ID);
            categoryForIncome.AccountingIncomeHeadId = headGl_2.Id;
            categoryForIncome.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            categoryForIncome.ModifiedDate = DateTime.Now;
            _db.SaveChanges();


            HeadGL headGl_3 = new HeadGL
            {
                Id = _db.Database.SqlQuery<int>("spGetNewId").FirstOrDefault(),
                AccCode = GenerateHeadGlAccCode(38015), // 
                AccName = vmModel.AccName,
                ParentId = 38015,
                CompanyId = vmModel.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                LayerNo = 6,
                CreateDate = DateTime.Now,
                IsActive = true,
                OrderNo = 0,
                Remarks = "GL Layer"
            };


            _db.HeadGLs.Add(headGl_3);
            var categoryForExpanse = _db.ProductCategories.SingleOrDefault(x => x.ProductCategoryId == vmModel.ID);
            categoryForExpanse.AccountingExpenseHeadId = headGl_3.Id;
            categoryForExpanse.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            categoryForExpanse.ModifiedDate = DateTime.Now;
            _db.SaveChanges();

            return result;

        }

        public int ProductHeadGlPush(VMHeadIntegration vmModel, Product product)
        {
            int result = -1;
            ProductCategory productCategory = _db.ProductCategories.Find(product.ProductCategoryId);
            //List<Head5> head5s = new List<Head5>();
            HeadGL headGl_1 = new HeadGL
            {
                Id = _db.Database.SqlQuery<int>("spGetNewId").FirstOrDefault(),
                AccCode = GenerateHeadGlAccCode(productCategory.AccountingHeadId.Value),

                AccName = vmModel.AccName,
                ParentId = productCategory.AccountingHeadId.Value, // Properties Accounts Receivable Head4 Id
                CompanyId = vmModel.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreateDate = DateTime.Now,
                LayerNo = 6,
                IsActive = true,
                OrderNo = 0,
                Remarks = "GL Layer"
            };


            _db.HeadGLs.Add(headGl_1);
            var categoryForAssets = _db.Products.SingleOrDefault(x => x.ProductId == product.ProductId);
            categoryForAssets.AccountingHeadId = headGl_1.Id;
            categoryForAssets.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            categoryForAssets.ModifiedDate = DateTime.Now;
            _db.SaveChanges();

            HeadGL headGl_2 = new HeadGL
            {
                Id = _db.Database.SqlQuery<int>("spGetNewId").FirstOrDefault(),
                AccCode = GenerateHeadGlAccCode(productCategory.AccountingIncomeHeadId.Value),
                AccName = vmModel.AccName,
                ParentId = productCategory.AccountingIncomeHeadId.Value,// Properties Accounts Receivable Head4 Id
                CompanyId = vmModel.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                LayerNo = 6,
                CreateDate = DateTime.Now,
                IsActive = true,

                OrderNo = 0,
                Remarks = "GL Layer"
            };


            _db.HeadGLs.Add(headGl_2);
            var categoryForIncome = _db.Products.SingleOrDefault(x => x.ProductId == product.ProductId);
            categoryForIncome.AccountingIncomeHeadId = headGl_2.Id;
            categoryForIncome.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            categoryForIncome.ModifiedDate = DateTime.Now;
            _db.SaveChanges();


            HeadGL headGl_3 = new HeadGL
            {
                Id = _db.Database.SqlQuery<int>("spGetNewId").FirstOrDefault(),
                AccCode = GenerateHeadGlAccCode(productCategory.AccountingExpenseHeadId.Value),
                AccName = vmModel.AccName,
                ParentId = productCategory.AccountingExpenseHeadId.Value, // Properties Accounts Receivable Head4 Id
                CompanyId = vmModel.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreateDate = DateTime.Now,
                LayerNo = 6,
                IsActive = true,
                OrderNo = 0,
                Remarks = "GL Layer"
            };


            _db.HeadGLs.Add(headGl_3);
            var categoryForExpanse = _db.Products.SingleOrDefault(x => x.ProductId == product.ProductId);
            categoryForExpanse.AccountingExpenseHeadId = headGl_3.Id;
            categoryForExpanse.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            categoryForExpanse.ModifiedDate = DateTime.Now;
            _db.SaveChanges();

            return result;

        }


        private string GenerateHead5AccCode(int head4Id)
        {
            var head4 = _db.Head4.FirstOrDefault(x => x.Id == head4Id);


            var head5DataList = _db.Head5.Where(x => x.ParentId == head4Id).AsEnumerable();

            string newAccountCode = "";
            if (head5DataList != null)
            {
                string lastAccCode = head5DataList.OrderByDescending(x => x.AccCode).FirstOrDefault()?.AccCode;
                string parentPart = lastAccCode.Substring(0, 7);
                string childPart = lastAccCode.Substring(7, 3);
                newAccountCode = parentPart + (Convert.ToInt32(childPart) + 1).ToString().PadLeft(3, '0');

            }
            else
            {
                newAccountCode = head4.AccCode + "001";

            }
            return newAccountCode;
        }
        private string GenerateHeadGlAccCode(int head5Id)
        {
            var head5 = _db.Head5.FirstOrDefault(x => x.Id == head5Id);


            var headGlDataList = _db.HeadGLs.Where(x => x.ParentId == head5Id).AsEnumerable();

            string newAccountCode = "";
            if (headGlDataList.Any())
            {
                string lastAccCode = headGlDataList.OrderByDescending(x => x.AccCode).FirstOrDefault().AccCode;
                string parentPart = lastAccCode.Substring(0, 10);
                string childPart = lastAccCode.Substring(10, 3);
                newAccountCode = parentPart + (Convert.ToInt32(childPart) + 1).ToString().PadLeft(3, '0');

            }
            else
            {
                newAccountCode = head5.AccCode + "001";

            }
            return newAccountCode;
        }

        public async Task<int> ProductCategoryProjectEdit(VMCommonProductCategory vmCommonProductCategory)
        {
            var result = -1;
            using (var scope = _db.Database.BeginTransaction())
            {
                try
                {
                    ProductCategory productCategory = await _db.ProductCategories.FindAsync(vmCommonProductCategory.ID);
                    productCategory.Name = vmCommonProductCategory.Name;
                    productCategory.Code = vmCommonProductCategory.Code;
                    productCategory.CashCustomerRate = vmCommonProductCategory.CashCommission;
                    productCategory.Address = vmCommonProductCategory.Description;

                    productCategory.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                    productCategory.ModifiedDate = DateTime.Now;

                    _db.ProductCategories.Add(productCategory);
                    _db.SaveChanges();

                    result = productCategory.ProductCategoryId;
                    var h5 = _db.Head5.FirstOrDefault(f => f.Id == productCategory.AccountingHeadId);
                    h5.AccName = vmCommonProductCategory.Name;

                    _db.Entry(h5).State = EntityState.Modified;
                    _db.SaveChanges();

                    var hgl = _db.HeadGLs.FirstOrDefault(f => f.Id == productCategory.AccountingIncomeHeadId);
                    hgl.AccName = vmCommonProductCategory.Name;

                    _db.Entry(hgl).State = EntityState.Modified;
                    _db.SaveChanges();

                    scope.Commit();
                    return result;
                }
                catch (Exception ex)
                {
                    scope.Rollback();
                    return result;
                }
            }
        }

        public async Task<int> ProductCategoryProjectDelete(int id)
        {
            int result = -1;
            if (id != 0)
            {
                ProductCategory productCategory = await _db.ProductCategories.FindAsync(id);
                productCategory.IsActive = false;

                if (await _db.SaveChangesAsync() > 0)
                {
                    result = productCategory.ProductCategoryId;
                }
            }
            return result;
        }


        public async Task<VMrealStateProductsForList> GetPlotOrFlat(int companyId, int categoryId = 0, int subCategoryId = 0)
        {
            VMrealStateProductsForList model = new VMrealStateProductsForList() { CompanyId = companyId };

            model.DataList = await (from t1 in _db.Products.Where(x => x.CompanyId == companyId)
                                    join t2 in _db.ProductSubCategories on t1.ProductSubCategoryId equals t2.ProductSubCategoryId
                                    join t3 in _db.ProductCategories on t2.ProductCategoryId equals t3.ProductCategoryId
                                    join t4 in _db.Units on t1.UnitId equals t4.UnitId
                                    join t5 in _db.FacingInfoes on t1.FacingId equals t5.FacingId
                                    where t1.IsActive == true && t2.IsActive == true && t3.IsActive == true &&
                                    ((categoryId > 0 && subCategoryId == 0) ? t2.ProductCategoryId == categoryId
                                    : (categoryId == 0 && subCategoryId > 0) ? t1.ProductSubCategoryId == subCategoryId
                                    : t1.ProductId > 0)
                                    select new realStateProducts
                                    {

                                        ID = t1.ProductId,
                                        Name = t1.ProductName,
                                        ShortName = t1.ShortName,
                                        MRPPrice = t1.UnitPrice == null ? 0 : t1.UnitPrice.Value,
                                        TPPrice = t1.TPPrice,
                                        SubCategoryName = t2.Name,
                                        CategoryName = t3.Name,
                                        UnitName = t4.Name,
                                        ProductType = t1.ProductType,
                                        Code = t1.ProductCode,
                                        ProcessLoss = t1.ProcessLoss,
                                        Status = t1.Status,
                                        FacingId = t1.FacingId,
                                        JsonData = t1.JsonData,
                                        FacingTitle = t5.Title,
                                        PackSize = t1.PackSize,
                                        Common_ProductCategoryFk = t3.ProductCategoryId,
                                        Remarks = t1.Remarks
                                    }).OrderByDescending(x => x.ID).ToListAsync();

            //foreach (var item in model.DataList)
            //{
            //    if (companyId==9)
            //    {
            //        item.GetList = _db.ProductSubCategories.Where(e => e.ProductCategoryId == item.Common_ProductCategoryFk && e.CompanyId.Value == companyId && e.IsActive).
            //               Select(o => new SelectModelType
            //               {
            //                   Text = o.Name,
            //                   Value = o.ProductSubCategoryId
            //               }).ToList();
            //    }
            //}

            return model;
        }

        public async Task<VMrealStateProductsForList> GetPlotOrFlatView(int companyId, int productId = 0)
        {
            VMrealStateProductsForList model = new VMrealStateProductsForList() { CompanyId = companyId };

            model.realStateProducts = await (from t1 in _db.Products.Where(x => x.CompanyId == companyId && x.ProductId == productId)
                                             join t2 in _db.ProductSubCategories on t1.ProductSubCategoryId equals t2.ProductSubCategoryId
                                             join t3 in _db.ProductCategories on t2.ProductCategoryId equals t3.ProductCategoryId
                                             join t4 in _db.Units on t1.UnitId equals t4.UnitId
                                             join t5 in _db.FacingInfoes on t1.FacingId equals t5.FacingId

                                             select new realStateProducts
                                             {
                                                 ID = t1.ProductId,
                                                 Name = t1.ProductName,
                                                 ShortName = t1.ShortName,
                                                 MRPPrice = t1.UnitPrice == null ? 0 : t1.UnitPrice.Value,
                                                 TPPrice = t1.TPPrice,
                                                 SubCategoryName = t2.Name,
                                                 CategoryName = t3.Name,
                                                 UnitName = t4.Name,
                                                 ProductType = t1.ProductType,
                                                 Code = t1.ProductCode,
                                                 ProcessLoss = t1.ProcessLoss,
                                                 Status = t1.Status,
                                                 FacingId = t1.FacingId,
                                                 JsonData = t1.JsonData,
                                                 FacingTitle = t5.Title,
                                                 PackSize = t1.PackSize,
                                                 Common_ProductCategoryFk = t3.ProductCategoryId
                                             }).FirstOrDefaultAsync();

            model.GetList = _db.ProductSubCategories.Where(e => e.ProductCategoryId == model.realStateProducts.Common_ProductCategoryFk && e.CompanyId.Value == companyId && e.IsActive == true).
               Select(o => new SelectModelType
               {
                   Text = o.Name,
                   Value = o.ProductSubCategoryId
               }).ToList();

            return model;
        }

        public async Task<string> FacingName(int id)
        {
            var name = await _db.FacingInfoes.FirstOrDefaultAsync(r => r.FacingId == id);
            return name?.Title;
        }

        public async Task<Division> SaveDivision(Division model)
        {


            var objToSave = _db.Divisions.Where(e => e.DivisionId == model.DivisionId).FirstOrDefault();
            try
            {

                if (objToSave == null)
                {

                    Division commonDivisin = new Division
                    {
                        Name = model.Name,

                    };

                    _db.Divisions.Add(commonDivisin);
                    await _db.SaveChangesAsync();

                }
                else
                {
                    objToSave.Name = model.Name;

                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                var mess = ex;
            }

            return model;
        }
        public async Task<Division> GetDivisionById(int id)
        {

            var obj = await _db.Divisions.SingleOrDefaultAsync(e => e.DivisionId == id);
            return new Division() { DivisionId = obj.DivisionId, Name = obj.Name };
        }


        public async Task<District> SaveDistrict(District model)
        {

            var obj = _db.Districts.OrderByDescending(x => x.Code).FirstOrDefault();
            var objToSave = _db.Districts.FirstOrDefault(e => e.IsActive == true && e.DistrictId == model.DistrictId);
            try
            {

                var codeee = Convert.ToInt32(obj.Code);
                var codee = codeee + 1;
                if (objToSave == null)
                {
                    District commonDistrict = new District
                    {
                        Name = model.Name,
                        ShortName = model.ShortName,
                        DivisionId = model.DivisionId,
                        Code = Convert.ToString(codee),
                        IsActive = true
                    };
                    _db.Districts.Add(commonDistrict);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    objToSave.Name = model.Name;
                    objToSave.ShortName = model.ShortName;
                    objToSave.DivisionId = model.DivisionId;
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                var mess = ex;
            }

            return model;
        }
        public async Task<District> GetDistrictById(int id)
        {

            var obj = await _db.Districts.SingleOrDefaultAsync(e => e.DistrictId == id);
            return new District() { DistrictId = obj.DistrictId, Name = obj.Name, ShortName = obj.ShortName, DivisionId = obj.DivisionId };
        }
        public async Task<bool> DeleteDistrict(int id)
        {
            var model = new District();
            var obj = await _db.Districts.SingleOrDefaultAsync(e => e.DistrictId == id);
            if (obj == null)
            {
                return false;
            }
            obj.IsActive = false;
            await _db.SaveChangesAsync();

            return true;

        }

        public async Task<Upazila> GetUpazilaById(int id)
        {

            var obj = await _db.Upazilas.SingleOrDefaultAsync(e => e.UpazilaId == id);
            return new Upazila() { UpazilaId = obj.UpazilaId, Name = obj.Name, ShortName = obj.ShortName, DistrictId = obj.DistrictId };
        }
        public async Task<bool> DeleteUpazila(int id)
        {
            var model = new Upazila();
            var obj = await _db.Upazilas.SingleOrDefaultAsync(e => e.UpazilaId == id);
            if (obj == null)
            {
                return false;
            }
            obj.IsActive = false;
            await _db.SaveChangesAsync();

            return true;
        }
        public async Task<Upazila> SaveUpazila(Upazila model)
        {

            var obj = _db.Upazilas.OrderByDescending(x => x.Code).FirstOrDefault();
            var objToSave = _db.Upazilas.FirstOrDefault(e => e.IsActive == true && e.UpazilaId == model.UpazilaId);
            try
            {

                var codeee = Convert.ToInt32(obj.Code);
                var codee = codeee + 1;
                if (objToSave == null)
                {

                    Upazila commonCustomer = new Upazila
                    {
                        Name = model.Name,
                        ShortName = model.ShortName,
                        DistrictId = model.DistrictId,
                        Code = Convert.ToString(codee),
                        FacCarryingCommission = Convert.ToDecimal(1.20),
                        DepoCarryingCommission = Convert.ToDecimal(1.20),
                        IsActive = true
                    };

                    _db.Upazilas.Add(commonCustomer);
                    await _db.SaveChangesAsync();

                }
                else
                {
                    objToSave.Name = model.Name;
                    objToSave.ShortName = model.ShortName;
                    objToSave.DistrictId = model.DistrictId;
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                var mess = ex;
            }

            return model;
        }

        #region Incentives
        public async Task<VMIncentive> GetIncentives(int companyId)
        {
            VMIncentive vmIncentive = new VMIncentive();
            vmIncentive.CompanyFK = companyId;
            vmIncentive.DataList = await Task.Run(() => (from t1 in _db.Incentives
                                                         where t1.IsActive == true && t1.CompanyId == companyId
                                                         // productCategoryId > 0 ? t1.ProductCategoryId == productCategoryId: t1.ProductCategoryId > 0
                                                         select new VMIncentive
                                                         {
                                                             CompanyId = t1.CompanyId,
                                                             IncentiveId = t1.IncentiveId,
                                                             IncentiveType = t1.IncentiveType,
                                                             IsActive = t1.IsActive

                                                         }).OrderByDescending(x => x.IncentiveId).AsEnumerable());
            return vmIncentive;
        }
        public async Task<int> IncentiveAdd(VMIncentive vmIncentive)
        {
            var result = -1;
            Incentive incentive = new Incentive
            {
                IncentiveType = vmIncentive.IncentiveType,
                CompanyId = vmIncentive.CompanyId,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true

            };
            _db.Incentives.Add(incentive);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = incentive.IncentiveId;
            }
            return result;
        }
        public async Task<int> IncentiveEdit(VMIncentive vmIncentive)
        {
            var result = -1;
            Incentive incentive = _db.Incentives.Find(vmIncentive.IncentiveId);
            incentive.IncentiveType = vmIncentive.IncentiveType;

            vmIncentive.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            vmIncentive.ModifiedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = vmIncentive.IncentiveId;
            }
            return result;
        }
        public async Task<int> IncentiveDelete(int id)
        {
            int result = -1;
            if (id != 0)
            {
                Incentive incentive = await _db.Incentives.FindAsync(id);
                incentive.IsActive = false;

                if (await _db.SaveChangesAsync() > 0)
                {
                    result = incentive.IncentiveId;
                }
            }
            return result;
        }

        public async Task<VMIncentiveDetails> GetIncentiveDetails(int companyId, int incentiveId)
        {
            VMIncentiveDetails vmIncentiveDetails = new VMIncentiveDetails();
            vmIncentiveDetails = await Task.Run(() => (from t1 in _db.Incentives
                                                       where t1.IsActive == true && t1.CompanyId == companyId
                                                        && t1.IncentiveId == incentiveId
                                                       select new VMIncentiveDetails
                                                       {

                                                           CompanyId = t1.CompanyId,
                                                           IncentiveId = t1.IncentiveId,
                                                           IncentiveType = t1.IncentiveType,
                                                           IsActive = t1.IsActive
                                                       }).FirstOrDefault()); ;
            vmIncentiveDetails.DataListDetails = await Task.Run(() => (from t1 in _db.IncentiveDetails.Where(h => h.IsActive == true
                                                                       )
                                                                       join t2 in _db.Incentives on t1.IncentiveId equals t2.IncentiveId
                                                                       where t1.IsActive == true && t2.IsActive == true && t2.CompanyId == companyId
                                                                        && incentiveId > 0 ? t1.IncentiveId == incentiveId : t1.IncentiveId > 0
                                                                       select new VMIncentiveDetails
                                                                       {
                                                                           MinQty = t1.MinQty,
                                                                           MaxQty = t1.MaxQty,
                                                                           Rate = t1.Rate,
                                                                           IncentiveDetailId = t1.IncentiveDetailId,
                                                                           CompanyId = t2.CompanyId,
                                                                           IncentiveId = t1.IncentiveId,
                                                                           IncentiveType = t2.IncentiveType,
                                                                           IsActive = t2.IsActive
                                                                       }).OrderByDescending(x => x.IncentiveDetailId).AsEnumerable());
            return vmIncentiveDetails;
        }
        public async Task<int> IncentiveDetailsAdd(VMIncentiveDetails vmIncentiveDetails)
        {
            var result = -1;
            IncentiveDetail incentiveDetail = new IncentiveDetail
            {
                MinQty = vmIncentiveDetails.MinQty,
                MaxQty = vmIncentiveDetails.MaxQty,
                Rate = vmIncentiveDetails.Rate,
                IncentiveId = vmIncentiveDetails.IncentiveId,
                IsActive = true

            };
            _db.IncentiveDetails.Add(incentiveDetail);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = incentiveDetail.IncentiveDetailId;
            }
            return result;
        }
        public async Task<int> IncentiveDetailsEdit(VMIncentiveDetails vmIncentiveDetails)
        {
            var result = -1;
            IncentiveDetail incentiveDetail = _db.IncentiveDetails.Find(vmIncentiveDetails.IncentiveDetailId);
            incentiveDetail.MinQty = vmIncentiveDetails.MinQty;
            incentiveDetail.MaxQty = vmIncentiveDetails.MaxQty;
            incentiveDetail.Rate = vmIncentiveDetails.Rate;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = incentiveDetail.IncentiveId;
            }
            return result;
        }
        public async Task<int> IncentiveDetailsDelete(int id)
        {
            int result = -1;
            if (id != 0)
            {
                IncentiveDetail incentiveDetail = await _db.IncentiveDetails.FindAsync(id);
                incentiveDetail.IsActive = false;

                if (await _db.SaveChangesAsync() > 0)
                {
                    result = incentiveDetail.IncentiveId;
                }
            }
            return result;
        }
        #endregion

    }



}