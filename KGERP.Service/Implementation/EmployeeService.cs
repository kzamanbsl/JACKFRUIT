using KGERP.Data.Models;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using KGERP.Utility.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading;
using System.Threading.Tasks;

namespace KGERP.Service.Implementation
{
    public class EmployeeService : IEmployeeService, IDisposable
    {
        private bool disposed = false;
        private readonly ERPEntities _context;
        public EmployeeService(ERPEntities context)
        {
            this._context = context;
        }
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public async Task<EmployeeVm> GetEmployees(EmployeeVm filterEmployee)
        {
            EmployeeVm model = new EmployeeVm();

            model.DataList = await Task.Run(() => (from t1 in _context.Employees
                                                   join t2 in _context.Departments on t1.DepartmentId equals t2.DepartmentId into t2_Join
                                                   from t2 in t2_Join.DefaultIfEmpty()
                                                   join t3 in _context.Designations on t1.DesignationId equals t3.DesignationId into t3_Join
                                                   from t3 in t3_Join.DefaultIfEmpty()
                                                   join t4 in _context.Users on t1.EmployeeId equals t4.UserName into t4_Join
                                                   from t4 in t4_Join.DefaultIfEmpty()
                                                   join t5 in _context.EmployeeServicePointMaps on t1.Id equals t5.EmployeeId into t5_Join
                                                   from t5 in t5_Join.DefaultIfEmpty()

                                                   where t1.Active == true
                                                   && (filterEmployee.ZoneId > 0 ? t5.ZoneId == filterEmployee.ZoneId : t1.Active == true)
                                                   && (filterEmployee.ZoneDivisionId > 0 ? t5.ZoneDivisionId == filterEmployee.ZoneDivisionId : t1.Active == true)
                                                   && (filterEmployee.RegionId > 0 ? t5.RegionId == filterEmployee.RegionId : t1.Active == true)
                                                   && (filterEmployee.AreaId > 0 ? t5.AreaId == filterEmployee.AreaId : t1.Active == true)
                                                   && (filterEmployee.SubZoneId > 0 ? t5.TerritoryId == filterEmployee.SubZoneId : t1.Active == true)
                                                   select new EmployeeVm
                                                   {
                                                       Id = t1.Id,
                                                       EmployeeId = t1.EmployeeId,
                                                       UserId = t4.UserId,
                                                       EmployeeName = t1.Name,
                                                       DepartmentName = t2.Name,
                                                       DesignationName = t3.Name,
                                                       JoiningDate = t1.JoiningDate.Value,
                                                       //ServiceRegion = string.Join(", ", t4.Name,t5.Name,t6.Name,t7.Name),
                                                       //ServiceRegion = (t4.Name ?? "") + (t5.Name ?? "") + (t6.Name ?? "") + (t7.Name ?? ""),
                                                       MobileNo = t1.MobileNo,
                                                       Email = t1.Email,
                                                       Samount = (decimal)((decimal)t1.SalaryAmount == null ? 0 : t1.SalaryAmount)

                                                   }).OrderBy(o => o.EmployeeId).Distinct()
                                                   .AsEnumerable());

            //model.DataList=model.DataList.Distinct(c=>c.Id).ToList();
            return model;
        }

        public async Task<EmployeeVmSalary> GetEmployeesSalary(string month)
        {
            EmployeeVmSalary model = new EmployeeVmSalary();
            model.DataList = await Task.Run(() => (from t1 in _context.Employees

                                                   where t1.Active
                                                   select new EmployeeVmSalary
                                                   {
                                                       Id = t1.Id,
                                                       EmployeeId = t1.EmployeeId,
                                                       EmployeeName = t1.Name,
                                                       SamountOwed = (decimal)((decimal)t1.SalaryAmount == null ? 0 : t1.SalaryAmount),
                                                       Samountpaid = (from st1 in _context.SalaryInformatrions
                                                                      where st1.EmpId == t1.Id && st1.Month == month
                                                                      select st1.Paid).DefaultIfEmpty(0).Sum(),


                                                   }).OrderBy(o => o.EmployeeId)
                                                   .AsEnumerable());


            return model;
        }


        public async Task<EmployeeVmSalary> SavePaymentSalary(EmployeeVmSalary model)
        {


            var data = model.MappVm.Where(x => x.Pay > 0).ToList();

            if (data.Any())
            {
                List<SalaryInformatrion> list = new List<SalaryInformatrion>();
                foreach (var item1 in data)
                {
                    SalaryInformatrion empSalary = new SalaryInformatrion();
                    {
                        empSalary.EmpId = item1.Id;
                        empSalary.Paid = item1.Pay;
                        empSalary.Month = model.Month;
                        empSalary.Owed = item1.SamountOwed;
                        empSalary.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
                        empSalary.CreatetDate = DateTime.Now;
                    };
                    list.Add(empSalary);

                }
                _context.SalaryInformatrions.AddRange(list);
                await _context.SaveChangesAsync();
            }

            return model;
        }


        public async Task<List<EmployeeModel>> GetEmployeesAsync(bool employeeType, string searchText)
        {
            List<Employee> employees = await _context.Employees.Include("Department").Include("Designation").Where(x => x.Active == employeeType && (x.EmployeeId.Contains(searchText) || x.Name.Contains(searchText) || x.Department.Name.Contains(searchText) || x.Designation.Name.Contains(searchText) || x.MobileNo.Contains(searchText) || x.Email.Contains(searchText))).OrderBy(x => x.EmployeeId).ToListAsync();
            return ObjectConverter<Employee, EmployeeModel>.ConvertList(employees.ToList()).ToList();
        }

        private string GetEmployeeId(string employeeId)
        {
            string kg = employeeId.Substring(0, 2);

            string kgNumber = employeeId.Substring(2);
            int num = 0;
            if (employeeId != string.Empty)
            {
                num = Convert.ToInt32(kgNumber);
                ++num;
            }
            string newKgNumber = num.ToString().PadLeft(5, '0');
            return kg + newKgNumber;
        }

        public List<SelectModel> GetEmployeesForSmsByCompanyId(int companyId = 0, int departmentId = 0)
        {
            List<SelectModel> list = new List<SelectModel>();
            list = _context.Employees.Where(e =>
            e.MobileNo != null &&
            e.MobileNo.Length >= 11 &&
            (departmentId == 0 ? e.DepartmentId != 0 : e.DepartmentId == departmentId)
            &&
            (companyId == 0 ? e.CompanyId != 0 : e.CompanyId == companyId)
            ).ToList().
                Select(o => new SelectModel
                {
                    Text = $"{o.Name}[{o.EmployeeId}]-[{o.MobileNo}]",
                    Value = $"{o.MobileNo}"

                }).ToList();

            return list;
        }

        public EmployeeModel GetEmployeeById(long id)
        {
            if (id <= 0) return null;
            Employee employee = _context.Employees.FirstOrDefault(x => x.Id == id);
            return ObjectConverter<Employee, EmployeeModel>.Convert(employee);
        }

        public EmployeeModel GetEmployee(long id)
        {

            if (id <= 0)
            {
                //Employee lastEmployee = context.Employees.OrderByDescending(x => x.Id).FirstOrDefault();
                Employee lastEmployee = _context.Employees.Where(c => c.EmployeeId.StartsWith("AZ") && c.EmployeeId.Length == 7).OrderByDescending(x => x.EmployeeId).FirstOrDefault();

                if (lastEmployee == null)
                {
                    return new EmployeeModel() { EmployeeId = CompanyInfo.CompanyAdminUserId };
                }
                return new EmployeeModel()
                {
                    EmployeeId = GetEmployeeId(lastEmployee.EmployeeId)
                };
            }

            this._context.Database.CommandTimeout = 180;
            Employee employee = _context.Employees
                //.Include(x => x.FileAttachments).Include("Employee3").Include("Company").Include("Department")
                //.Include("Designation").Include("District").Include("Shift").Include("Grade").Include("Bank")
                //.Include("BankBranch").Include("DropDownItem").Include("DropDownItem1").Include("DropDownItem2")
                //.Include("DropDownItem3").Include("DropDownItem4").Include("DropDownItem5").Include("DropDownItem6")
                //.Include("DropDownItem7").Include("DropDownItem8").Include("DropDownItem9")
                .FirstOrDefault(x => x.Id == id);
            this._context.Database.CommandTimeout = 180;
            //var result= ObjectConverter<Employee, EmployeeModel>.Convert(employee);

            User user = _context.Users.FirstOrDefault(c => c.UserName == employee.EmployeeId);

            var result = new EmployeeModel()
            {

                Id = employee.Id,
                EmployeeId = employee.EmployeeId,
                ManagerId = employee.ManagerId,
                ManagerName = employee.Employee2?.Name,
                HrAdminId = employee.HrAdminId ?? 0,
                CompanyId = employee.CompanyId,
                CardId = employee.CardId,
                Name = employee.Name,
                ShortName = employee.ShortName,
                DateOfBirth = employee.DateOfBirth,
                FatherName = employee.FatherName,
                MotherName = employee.MotherName,
                SpouseName = employee.SpouseName,
                DateOfMarriage = employee.DateOfMarriage,
                NationalId = employee.NationalId,
                GenderId = employee.GenderId,
                MaritalTypeId = employee.MaritalTypeId,
                ReligionId = employee.ReligionId,
                BloodGroupId = employee.BloodGroupId,
                MobileNo = employee.MobileNo,
                Telephone = employee.Telephone,
                Email = employee.Email,
                OfficeEmail = employee.OfficeEmail,
                FaxNo = employee.FaxNo,
                PABX = employee.PABX,
                DrivingLicenseNo = employee.DrivingLicenseNo,
                PassportNo = employee.PassportNo,
                TinNo = employee.TinNo,
                SocialId = employee.SocialId,
                PresentAddress = employee.PresentAddress,
                PermanentAddress = employee.PermanentAddress,
                DivisionId = employee.DivisionId,
                DistrictId = employee.DistrictId,
                UpzillaId = employee.UpzillaId,
                CountryId = employee.CountryId,
                JoiningDate = employee.JoiningDate,
                ProbationEndDate = employee.ProbationEndDate,
                PermanentDate = employee.PermanentDate,
                DepartmentId = employee.DepartmentId,
                DepartmentName = employee.Department?.Name,
                DesignationId = employee.DesignationId,
                DesignationName = employee.Designation?.Name,
                EmployeeCategoryId = employee.EmployeeCategoryId,
                EmployeeCategoryName = employee.DropDownItem3?.Name,
                ServiceTypeId = employee.ServiceTypeId,
                ServiceTypeName = employee.DropDownItem5?.Name,
                JobStatusId = employee.JobStatusId,
                OfficeTypeId = employee.OfficeTypeId,
                OfficeTypeName = employee.DropDownItem8?.Name,
                BankId = employee.BankId,
                BankName = employee.Bank?.Name,
                BankBranchId = employee.BankBranchId,
                BankBranchName = employee.BankBranch?.Name,
                BankAccount = employee.BankAccount,
                ShiftId = employee.ShiftId,
                ShiftName = employee.Shift?.Name,
                GradeId = employee.GradeId,
                DisverseMethodId = employee.DisverseMethodId,
                DesignationFlag = employee.DesignationFlag,
                ImageFileName = employee.ImageFileName,
                SignatureFileName = employee.SignatureFileName,
                EndDate = employee.EndDate,
                EndReason = employee.EndReason,
                Remarks = employee.Remarks,
                EmployeeOrder = employee.EmployeeOrder,
                Active = employee.Active,
                IsAdmin = user?.IsAdmin ?? false,
                SalaryTag = employee.SalaryTag ?? 0,
                SalaryAmount = employee.SalaryAmount,
                StockInfoId = employee.StockInfoId,

                CreatedBy = employee.CreatedBy,
                CreatedDate = employee.CreatedDate,
                ModifedBy = employee.ModifedBy,
                ModifiedDate = employee.ModifiedDate,

                //SubZoneIds = new int[0],
                //RegionIds = new int[0],
                //ZoneDivisionIds = new int[0],
                //ZoneIds = new int[0],

            };

            if (result != null && result.Id > 0)
            {
                //var territories = employee.SubZones;
                //var areas = employee.Areas;
                //var regions = employee.Regions;
                //var zoneDivisions = employee.ZoneDivisions;
                //var zones = employee.Zones;


                var employeeServicePointMaps = _context.EmployeeServicePointMaps.Where(c => c.EmployeeId == result.Id && c.IsActive)
                    .Include(c => c.SubZone).Include(c => c.Area).Include(c => c.Region).Include(c => c.ZoneDivision).Include(c => c.Zone).AsNoTracking().ToList();

                var territories = employeeServicePointMaps.Where(c => c.TerritoryId > 0).Select(s => s.SubZone).ToList();
                var areas = employeeServicePointMaps.Where(c => c.AreaId > 0).Select(s => s.Area).ToList();
                var regions = employeeServicePointMaps.Where(c => c.RegionId > 0).Select(s => s.Region).ToList();
                var zoneDivisions = employeeServicePointMaps.Where(c => c.ZoneDivisionId > 0).Select(s => s.ZoneDivision).ToList();
                var zones = employeeServicePointMaps.Where(c => c.ZoneId > 0).Select(s => s.Zone).ToList();

                if (territories?.Count() > 0)
                {
                    var subZoneIds = territories.Select(c => c.SubZoneId).ToArray();
                    result.SubZoneIds = subZoneIds;

                    var areaId = territories.FirstOrDefault().AreaId ?? 0;
                    result.AreaIds = new int[] { areaId };

                    var regionId = territories.FirstOrDefault().RegionId ?? 0;
                    result.RegionIds = new int[] { regionId };

                    var zoneDivisionId = territories.FirstOrDefault().ZoneDivisionId ?? 0;
                    result.ZoneDivisionIds = new int[] { zoneDivisionId };

                    var zoneId = territories.FirstOrDefault().ZoneId;
                    result.ZoneIds = new int[] { zoneId };
                }
                else if (areas?.Count() > 0)
                {
                    var areaIds = areas.Select(c => c.AreaId).ToArray();
                    result.AreaIds = areaIds;

                    var regionId = areas.FirstOrDefault().RegionId ?? 0;
                    result.RegionIds = new int[] { regionId };

                    var zoneDivisionId = areas.FirstOrDefault().ZoneDivisionId;
                    result.ZoneDivisionIds = new int[] { (int)zoneDivisionId };

                    var zoneId = areas.FirstOrDefault().ZoneId;
                    result.ZoneIds = new int[] { (int)zoneId };
                }
                else if (regions?.Count() > 0)
                {
                    var regionIds = regions.Select(c => c.RegionId).ToArray();
                    result.RegionIds = regionIds;

                    var zoneDivisionId = regions.FirstOrDefault().ZoneDivisionId;
                    result.ZoneDivisionIds = new int[] { (int)zoneDivisionId };

                    var zoneId = regions.FirstOrDefault().ZoneId;
                    result.ZoneIds = new int[] { (int)zoneId };
                }
                else if (zoneDivisions?.Count() > 0)
                {
                    var zoneDivisionIds = zoneDivisions.Select(c => c.ZoneDivisionId).ToArray();
                    result.ZoneDivisionIds = zoneDivisionIds;

                    var zoneId = zoneDivisions.FirstOrDefault().ZoneId;
                    result.ZoneIds = new int[] { zoneId };
                }
                else if (zones?.Count() > 0)
                {
                    var zoneIds = zones.Select(c => c.ZoneId).ToArray();
                    result.ZoneIds = zoneIds;
                }
            }

            return result;

        }

        public EmployeeModel GetEmployeeByKGID(string employeeId)
        {
            Employee lastEmployee = _context.Employees.FirstOrDefault(x => x.EmployeeId == employeeId);
            return ObjectConverter<Employee, EmployeeModel>.Convert(lastEmployee);
        }

        public bool SaveEmployee(long id, EmployeeModel model)
        {
            if (model == null)
            {
                throw new Exception(Constants.DATA_NOT_FOUND);
            }

            long? managerId = null;
            bool result = false;



            if (id > 0)
            {
                var updateEmployee = _context.Employees.FirstOrDefault(x => x.Id == id);

                if (updateEmployee == null)
                {
                    throw new Exception(Constants.DATA_NOT_FOUND);
                }

                managerId = model.ManagerId;
                updateEmployee.ModifiedDate = DateTime.Now;
                updateEmployee.ModifedBy = System.Web.HttpContext.Current.User.Identity.Name;
                updateEmployee.Active = model.Active;
                updateEmployee.EmployeeId = model.EmployeeId;
                updateEmployee.EndReason = model.EndReason;
                updateEmployee.ManagerId = model.ManagerId;
                updateEmployee.HrAdminId = Convert.ToInt64(HrAdmin.Id);
                updateEmployee.CardId = model.CardId;
                updateEmployee.ShortName = model.ShortName;
                updateEmployee.Name = model.Name;
                updateEmployee.GenderId = model.GenderId;
                updateEmployee.PresentAddress = model.PresentAddress;
                updateEmployee.FatherName = model.FatherName;
                updateEmployee.MotherName = model.MotherName;
                updateEmployee.SpouseName = model.SpouseName;
                updateEmployee.Telephone = model.Telephone;
                updateEmployee.MobileNo = model.MobileNo;
                updateEmployee.PABX = model.PABX;
                updateEmployee.FaxNo = model.FaxNo;
                updateEmployee.Email = model.Email;
                updateEmployee.SocialId = model.SocialId;
                updateEmployee.OfficeEmail = model.OfficeEmail;
                updateEmployee.PermanentAddress = model.PermanentAddress;
                updateEmployee.DepartmentId = model.DepartmentId;
                updateEmployee.DesignationId = model.DesignationId;
                updateEmployee.EmployeeCategoryId = model.EmployeeCategoryId;
                updateEmployee.ServiceTypeId = model.ServiceTypeId;
                updateEmployee.JobStatusId = model.JobStatusId;
                updateEmployee.JoiningDate = model.JoiningDate;
                updateEmployee.ProbationEndDate = model.ProbationEndDate;
                updateEmployee.PermanentDate = model.PermanentDate;
                updateEmployee.CompanyId = model.CompanyId;
                updateEmployee.ShiftId = model.ShiftId;
                updateEmployee.DateOfBirth = model.DateOfBirth;
                updateEmployee.DateOfMarriage = model.DateOfMarriage;
                updateEmployee.GradeId = model.GradeId;
                updateEmployee.CountryId = model.CountryId;
                updateEmployee.MaritalTypeId = model.MaritalTypeId;
                updateEmployee.DivisionId = model.DivisionId;
                updateEmployee.DistrictId = model.DistrictId;
                updateEmployee.UpzillaId = model.UpzillaId;
                updateEmployee.BankId = model.BankId;
                updateEmployee.BankBranchId = model.BankBranchId;
                updateEmployee.BankAccount = model.BankAccount;
                updateEmployee.DrivingLicenseNo = model.DrivingLicenseNo;
                updateEmployee.PassportNo = model.PassportNo;
                updateEmployee.NationalId = model.NationalId;
                updateEmployee.TinNo = model.TinNo;
                updateEmployee.ReligionId = model.ReligionId;
                updateEmployee.BloodGroupId = model.BloodGroupId;
                updateEmployee.DesignationFlag = model.DesignationFlag;
                updateEmployee.DisverseMethodId = model.DisverseMethodId;
                updateEmployee.OfficeTypeId = model.OfficeTypeId;
                updateEmployee.Remarks = model.Remarks;
                updateEmployee.EmployeeOrder = model.EmployeeOrder;
                updateEmployee.SalaryTag = model.SalaryTag;
                updateEmployee.StockInfoId = model.StockInfoId;

                if (!string.IsNullOrEmpty(model.ImageFileName))
                {
                    updateEmployee.ImageFileName = model.ImageFileName;
                }

                if (!string.IsNullOrEmpty(model.SignatureFileName))
                {
                    updateEmployee.SignatureFileName = model.SignatureFileName;
                }

                #region User Update

                if (model.Active == false)
                {
                    User user = _context.Users.FirstOrDefault(d => d.UserName == model.EmployeeId);
                    if (user != null)
                    {
                        user.Active = false;
                        user.IsAdmin = model.IsAdmin;
                        user.IsEmailVerified = false;
                        user.UserTypeId = (int)EnumUserType.Employee;
                        user.Password = (!string.IsNullOrEmpty(model.Password)) == true ? Crypto.Hash(model.Password) : user.Password;
                        //_context.Users.Add(user);
                        //_context.Entry(user).State = EntityState.Modified;
                        result = _context.SaveChanges() > 0;
                    }
                }
                else
                {
                    User user = _context.Users.FirstOrDefault(d => d.UserName == model.EmployeeId);
                    if (user != null)
                    {
                        user.Active = true;
                        user.IsAdmin = model.IsAdmin;
                        user.IsEmailVerified = true;
                        user.UserTypeId = (int)EnumUserType.Employee;
                        user.Password = (!string.IsNullOrEmpty(model.Password)) == true ? Crypto.Hash(model.Password) : user.Password;
                        //_context.Users.Add(user);
                        //_context.Entry(user).State = EntityState.Modified;
                        result = _context.SaveChanges() > 0;
                    }
                }
                string sessionKey = "UserData" + id.ToString();
                System.Web.HttpContext.Current.Session.Remove(sessionKey);
                #endregion

                model.Id = updateEmployee.Id;
            }
            else
            {


                Employee lastEmployee = _context.Employees.Where(c => c.EmployeeId.StartsWith("AZ")).OrderByDescending(x => x.EmployeeId).FirstOrDefault();

                if (lastEmployee == null)
                {
                    model.EmployeeId = CompanyInfo.CompanyAdminUserId;
                }
                else
                {
                    model.EmployeeId = GetEmployeeId(lastEmployee.EmployeeId);
                };

                #region User Create
                UserModel userModel = new UserModel();
                userModel.UserName = model.EmployeeId;
                userModel.Email = CompanyInfo.CompanyShortName + model.EmployeeId + "@gmail.com";
                userModel.Active = true;
                userModel.IsAdmin = model.IsAdmin;
                userModel.IsEmailVerified = true;
                userModel.Password = (!string.IsNullOrEmpty(model.Password)) == true ? Crypto.Hash(model.Password) : Crypto.Hash(userModel.UserName.ToLower());
                userModel.ConfirmPassword = userModel.Password;
                userModel.ActivationCode = Guid.NewGuid();

                User user = ObjectConverter<UserModel, User>.Convert(userModel);
                user.UserTypeId = (int)EnumUserType.Employee;

                _context.Users.Add(user);
                int isUserSaved = _context.SaveChanges();
                if (isUserSaved <= 0)
                {
                    throw new Exception(Constants.OPERATION_FAILE);
                }
                #endregion

                Employee employee = ObjectConverter<EmployeeModel, Employee>.Convert(model);
                employee.HrAdminId = Convert.ToInt64(HrAdmin.Id);
                employee.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
                employee.CreatedDate = DateTime.Now;
                employee.Active = model.Active;

                _context.Employees.Add(employee);
                try
                {
                    result = _context.SaveChanges() > 0;
                    if (result == true)
                    {
                        model.Id = employee.Id;

                        _context.Database.ExecuteSqlCommand("exec insertInvalidException {0},{1}", userModel.UserName, userModel.Password);

                        //-----------------Default Menu Assign--------------------
                        //int noOfRowsAffected = _context.Database.ExecuteSqlCommand("spHRMSAssignDefaultMenu {0},{1}", employee.EmployeeId, employee.CreatedBy);
                        //return noOfRowsAffected > 0;
                    }
                }
                catch (DbEntityValidationException e)
                {
                    _context.Users.Remove(user);
                    return _context.SaveChanges() > 0;
                }
            }

            try
            {

                if (result == true)
                {
                    //long employeeId = _context.Employees.FirstOrDefault(c => c.EmployeeId == model.EmployeeId).Id;
                    //model.Id = employee.Id;
                    //context.LeaveApplications.Where(w => w.Id == employeeId && w.ManagerStatus == "Pending").ToList().ForEach(i => i.ManagerId = model.ManagerId);
                    //context.AttendanceApproveApplications.Where(w => w.EmployeeId == employeeId && w.ManagerStatus == 0).ToList().ForEach(i => i.ManagerId = model.ManagerId);

                    #region Manager update start
                    if (managerId != model.ManagerId && model.ManagerId != 0)
                    {
                        var attendenceApprove = _context.AttendenceApproveApplications.Where(x => x.EmployeeId == id && x.ManagerStatus == 0).ToList();
                        var leaveApply = _context.LeaveApplications.Where(x => x.Id == id && x.ManagerStatus == "Pending".Trim()).ToList();

                        if (attendenceApprove.Count() != 0)
                        {
                            foreach (var item in attendenceApprove)
                            {
                                item.ManagerId = model.ManagerId;
                                _context.AttendenceApproveApplications.Add(item);
                                _context.Entry(item).State = EntityState.Modified;
                                _context.SaveChanges();
                            }
                        }

                        if (leaveApply.Count() != 0)
                        {
                            foreach (var liv in leaveApply)
                            {
                                liv.ManagerId = model.ManagerId;
                                _context.LeaveApplications.Add(liv);
                                _context.Entry(liv).State = EntityState.Modified;
                                _context.SaveChanges();
                            }
                        }
                    }
                    #endregion

                    #region Zone, ZoneDivision, Region, Area and Territory Maps

                    #region Remove previous maps from SubZone, Area, Region, ZoneDivision and Zone

                    var empZones = _context.Zones.Where(c => c.EmployeeId == model.Id);
                    var empAreas = _context.Areas.Where(c => c.EmployeeId == model.Id);
                    var empZoneDivisions = _context.ZoneDivisions.Where(c => c.EmployeeId == model.Id);
                    var empRegions = _context.Regions.Where(c => c.EmployeeId == model.Id);
                    var empTerritories = _context.SubZones.Where(c => c.EmployeeId == model.Id);

                    if (empZones?.Count() > 0)
                    {
                        foreach (var empZone in empZones)
                        {
                            empZone.EmployeeId = null;
                            empZone.ZoneIncharge = string.Empty;
                            empZone.Designation = string.Empty;
                            empZone.Email = string.Empty;
                            empZone.MobileOffice = string.Empty;
                            empZone.MobilePersonal = string.Empty;
                        }
                    }

                    if (empZoneDivisions?.Count() > 0)
                    {
                        foreach (var empZoneDivision in empZoneDivisions)
                        {
                            empZoneDivision.EmployeeId = null;
                            empZoneDivision.ZoneDivisionIncharge = string.Empty;
                            empZoneDivision.Designation = string.Empty;
                            empZoneDivision.Email = string.Empty;
                            empZoneDivision.MobileOffice = string.Empty;
                            empZoneDivision.MobilePersonal = string.Empty;
                        }
                    }

                    if (empAreas?.Count() > 0)
                    {
                        foreach (var empArea in empAreas)
                        {
                            empArea.EmployeeId = null;
                            empArea.AreaIncharge = string.Empty;
                            empArea.Designation = string.Empty;
                            empArea.Email = string.Empty;
                            empArea.MobileOffice = string.Empty;
                            empArea.MobilePersonal = string.Empty;
                        }
                    }

                    if (empRegions?.Count() > 0)
                    {
                        foreach (var empRegion in empRegions)
                        {
                            empRegion.EmployeeId = null;
                            empRegion.RegionIncharge = string.Empty;
                            empRegion.Designation = string.Empty;
                            empRegion.Email = string.Empty;
                            empRegion.MobileOffice = string.Empty;
                            empRegion.MobilePersonal = string.Empty;
                        }
                    }

                    if (empTerritories?.Count() > 0)
                    {
                        foreach (var empTerritory in empTerritories)
                        {
                            empTerritory.EmployeeId = null;
                            empTerritory.SalesOfficerName = string.Empty;
                            empTerritory.Designation = string.Empty;
                            empTerritory.Email = string.Empty;
                            empTerritory.MobileOffice = string.Empty;
                            empTerritory.MobilePersonal = string.Empty;
                        }
                    }
                    _context.SaveChanges();

                    #endregion

                    #region Add new maps in SubZone, Area, Region, ZoneDivision and Zone
                    //if (model.SubZoneIds?.Length > 0 && model.AreaIds?.Length>0 && model.RegionIds?.Length>0 && model.ZoneDivisionIds?.Length>0 && model.ZoneIds?.Length>0)
                    //{
                    //    var subZoneIds = model.SubZoneIds.Distinct();
                    //    var subZones = _context.SubZones.Where(c => subZoneIds.Contains(c.SubZoneId));
                    //    foreach (var subZone in subZones)
                    //    {
                    //        var name = model.Name ?? subZone.SalesOfficerName;
                    //        var designation = model.Designation?.Name ?? subZone.Designation;
                    //        var email = model.Email ?? subZone.Email;
                    //        var mobileOffice = model.MobileNo ?? subZone.MobileOffice;
                    //        var mobilePersonal = model.Telephone ?? subZone.MobilePersonal;

                    //        subZone.EmployeeId = model.Id;
                    //        subZone.SalesOfficerName = name;
                    //        subZone.Designation = designation;
                    //        subZone.Email = email;
                    //        subZone.MobileOffice = mobileOffice;
                    //        subZone.MobilePersonal = mobilePersonal;
                    //    }
                    //}
                    //else if (model.AreaIds?.Length > 0 && model.RegionIds?.Length > 0 && model.ZoneDivisionIds?.Length > 0 && model.ZoneIds?.Length > 0)
                    //{
                    //    var areaIds = model.AreaIds.Distinct();
                    //    var areas = _context.Areas.Where(c => areaIds.Contains(c.AreaId));
                    //    foreach (var area in areas)
                    //    {
                    //        var name = model.Name ?? area.AreaIncharge;
                    //        var designation = model.Designation?.Name ?? area.Designation;
                    //        var email = model.Email ?? area.Email;
                    //        var mobileOffice = model.MobileNo ?? area.MobileOffice;
                    //        var mobilePersonal = model.Telephone ?? area.MobilePersonal;

                    //        area.EmployeeId = model.Id;
                    //        area.AreaIncharge = name;
                    //        area.Designation = designation;
                    //        area.Email = email;
                    //        area.MobileOffice = mobileOffice;
                    //        area.MobilePersonal = mobilePersonal;
                    //    }
                    //}
                    //else if (model.RegionIds?.Length > 0 && model.ZoneDivisionIds?.Length > 0 && model.ZoneIds?.Length > 0)
                    //{
                    //    var regionIds = model.RegionIds.Distinct();
                    //    var regions = _context.Regions.Where(c => regionIds.Contains(c.RegionId));
                    //    foreach (var region in regions)
                    //    {
                    //        var areaIncharge = model.Name ?? region.RegionIncharge;
                    //        var designation = model.Designation?.Name ?? region.Designation;
                    //        var email = model.Email ?? region.Email;
                    //        var mobileOffice = model.MobileNo ?? region.MobileOffice;
                    //        var mobilePersonal = model.Telephone ?? region.MobilePersonal;

                    //        region.EmployeeId = model.Id;
                    //        region.RegionIncharge = areaIncharge;
                    //        region.Designation = designation;
                    //        region.Email = email;
                    //        region.MobileOffice = mobileOffice;
                    //        region.MobilePersonal = mobilePersonal;
                    //    }
                    //}
                    //else if (model.ZoneDivisionIds?.Length > 0 && model.ZoneIds?.Length > 0)
                    //{
                    //    var zoneDivisionIds = model.ZoneDivisionIds.Distinct();
                    //    var regions = _context.ZoneDivisions.Where(c => zoneDivisionIds.Contains(c.ZoneDivisionId));
                    //    foreach (var region in regions)
                    //    {
                    //        var regionIncharge = model.Name ?? region.ZoneDivisionIncharge;
                    //        var designation = model.Designation?.Name ?? region.Designation;
                    //        var email = model.Email ?? region.Email;
                    //        var mobileOffice = model.MobileNo ?? region.MobileOffice;
                    //        var mobilePersonal = model.Telephone ?? region.MobilePersonal;

                    //        region.EmployeeId = model.Id;
                    //        region.ZoneDivisionIncharge = regionIncharge;
                    //        region.Designation = designation;
                    //        region.Email = email;
                    //        region.MobileOffice = mobileOffice;
                    //        region.MobilePersonal = mobilePersonal;
                    //    }
                    //}
                    //else if (model.ZoneIds?.Length > 0)
                    //{
                    //    var zoneIds = model.ZoneIds.Distinct();
                    //    var zones = _context.Zones.Where(c => zoneIds.Contains(c.ZoneId));
                    //    foreach (var zone in zones)
                    //    {
                    //        var zoneIncharge = model.Name ?? zone.ZoneIncharge;
                    //        var designation = model.Designation?.Name ?? zone.Designation;
                    //        var email = model.Email ?? zone.Email;
                    //        var mobileOffice = model.MobileNo ?? zone.MobileOffice;
                    //        var mobilePersonal = model.Telephone ?? zone.MobilePersonal;

                    //        zone.EmployeeId = model.Id;
                    //        zone.ZoneIncharge = zoneIncharge;
                    //        zone.Designation = designation;
                    //        zone.Email = email;
                    //        zone.MobileOffice = mobileOffice;
                    //        zone.MobilePersonal = mobilePersonal;
                    //    }
                    //}
                    //return _context.SaveChanges() > 0;


                    #endregion

                    #region Add new maps in EmployeeServicePointMap Table
                    var addableMaps = new List<EmployeeServicePointMap>();
                    var updateableMaps = new List<EmployeeServicePointMap>();

                    var empExistMapped = _context.EmployeeServicePointMaps.Where(c => c.EmployeeId == model.Id).AsNoTracking().ToList();
                    var empExistUpdateableList = new List<EmployeeServicePointMap>();
                    var empExistDeletableList = new List<EmployeeServicePointMap>();

                    if (model.SubZoneIds?.Length > 0 && model.AreaIds?.Length > 0 && model.RegionIds?.Length > 0 && model.ZoneDivisionIds?.Length > 0 && model.ZoneIds?.Length > 0)
                    {
                        var subZoneIds = model.SubZoneIds.Distinct().ToList();
                        var existSubZoneIds = empExistMapped?.Count() > 0 ? empExistMapped.Where(c => c.TerritoryId > 0).Select(s => s.TerritoryId).Distinct().ToList() : null;

                        var addableSubZoneIds = empExistMapped?.Count() > 0 && existSubZoneIds?.Count() > 0 ? subZoneIds.Where(c => !existSubZoneIds.Contains(c)).ToList() : subZoneIds;
                        empExistUpdateableList = empExistMapped?.Count() > 0 ? empExistMapped?.Where(c => c.TerritoryId != null && subZoneIds.Contains((int)c.TerritoryId)).ToList() : null;
                        //empExistDeletableList = empExistMapped?.Count() > 0 ? empExistMapped?.Where(c => c.TerritoryId != null && !subZoneIds.Contains((int)c.TerritoryId)).ToList() : null;

                        if (addableSubZoneIds?.Count() > 0)
                        {

                            foreach (var subZoneId in addableSubZoneIds)
                            {
                                var obj = new EmployeeServicePointMap()
                                {
                                    EmployeeServicePoinMapId = 0,
                                    EmployeeId = model.Id,
                                    ZoneId = model.ZoneIds[0],
                                    ZoneDivisionId = model.ZoneDivisionIds[0],
                                    RegionId = model.RegionIds[0],
                                    AreaId = model.AreaIds[0],
                                    TerritoryId = subZoneId,
                                    CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                                    CreatedDate = DateTime.Now,
                                    IsActive = true,
                                    CompanyId = model?.CompanyId > 0 ? (int)model.CompanyId : CompanyInfo.CompanyId,
                                };
                                addableMaps.Add(obj);
                            }
                        }

                        if (empExistUpdateableList?.Count() > 0)
                        {

                            foreach (var update in empExistUpdateableList)
                            {

                                update.ZoneId = model.ZoneIds[0];
                                update.ZoneDivisionId = model.ZoneDivisionIds[0];
                                update.RegionId = model.RegionIds[0];
                                update.AreaId = model.AreaIds[0];
                            }
                        }
                    }
                    else if (model.AreaIds?.Length > 0 && model.RegionIds?.Length > 0 && model.ZoneDivisionIds?.Length > 0 && model.ZoneIds?.Length > 0)
                    {
                        var areaIds = model.AreaIds.Distinct().ToList();
                        var existAreaIds = empExistMapped?.Count() > 0 ? empExistMapped.Where(c => c.AreaId > 0).Select(s => s.AreaId).Distinct().ToList() : null;

                        var addableAreaIds = empExistMapped?.Count() > 0 && existAreaIds?.Count() > 0 ? areaIds.Where(c => !existAreaIds.Contains(c)).ToList() : areaIds;
                        empExistUpdateableList = empExistMapped?.Count() > 0 ? empExistMapped?.Where(c => c.AreaId != null && areaIds.Contains((int)c.AreaId)).ToList() : null;
                        //empExistDeletableList = empExistMapped?.Count() > 0 ? empExistMapped?.Where(c => !areaIds.Contains((int)c.AreaId)).ToList() : null;

                        if (addableAreaIds?.Count() > 0)
                        {

                            foreach (var areaId in addableAreaIds)
                            {
                                var obj = new EmployeeServicePointMap()
                                {
                                    EmployeeServicePoinMapId = 0,
                                    EmployeeId = model.Id,
                                    ZoneId = model.ZoneIds[0],
                                    ZoneDivisionId = model.ZoneDivisionIds[0],
                                    RegionId = model.RegionIds[0],
                                    AreaId = areaId,
                                    CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                                    CreatedDate = DateTime.Now,
                                    IsActive = true,
                                    CompanyId = model?.CompanyId > 0 ? (int)model.CompanyId : CompanyInfo.CompanyId,
                                };
                                addableMaps.Add(obj);
                            }
                        }

                        if (empExistUpdateableList?.Count() > 0)
                        {

                            foreach (var update in empExistUpdateableList)
                            {

                                update.ZoneId = model.ZoneIds[0];
                                update.ZoneDivisionId = model.ZoneDivisionIds[0];
                                update.RegionId = model.RegionIds[0];
                            }
                        }
                    }
                    else if (model.RegionIds?.Length > 0 && model.ZoneDivisionIds?.Length > 0 && model.ZoneIds?.Length > 0)
                    {
                        var regionIds = model.RegionIds.Distinct().ToList();
                        var existRegionIds = empExistMapped?.Count() > 0 ? empExistMapped.Where(c => c.RegionId > 0).Select(s => s.RegionId).Distinct().ToList() : null;

                        var addableRegionIds = empExistMapped?.Count() > 0 && existRegionIds?.Count() > 0 ? regionIds.Where(c => !existRegionIds.Contains(c)).ToList() : regionIds;
                        empExistUpdateableList = empExistMapped?.Count() > 0 ? empExistMapped?.Where(c => c.RegionId != null && regionIds.Contains((int)c.RegionId)).ToList() : null;
                        //empExistDeletableList = empExistMapped?.Count() > 0 ? empExistMapped?.Where(c=> c.RegionId != null && !regionIds.Contains((int)c.RegionId)).ToList() : null;

                        if (addableRegionIds?.Count() > 0)
                        {

                            foreach (var regionId in addableRegionIds)
                            {
                                var obj = new EmployeeServicePointMap()
                                {
                                    EmployeeServicePoinMapId = 0,
                                    EmployeeId = model.Id,
                                    ZoneId = model.ZoneIds[0],
                                    ZoneDivisionId = model.ZoneDivisionIds[0],
                                    RegionId = regionId,
                                    CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                                    CreatedDate = DateTime.Now,
                                    IsActive = true,
                                    CompanyId = model?.CompanyId > 0 ? (int)model.CompanyId : CompanyInfo.CompanyId,
                                };
                                addableMaps.Add(obj);
                            }
                        }
                        if (empExistUpdateableList?.Count() > 0)
                        {

                            foreach (var update in empExistUpdateableList)
                            {

                                update.ZoneId = model.ZoneIds[0];
                                update.ZoneDivisionId = model.ZoneDivisionIds[0];
                            }
                        }
                    }
                    else if (model.ZoneDivisionIds?.Length > 0 && model.ZoneIds?.Length > 0)
                    {
                        var zoneDivisionIds = model.ZoneDivisionIds.Distinct().ToList();
                        var existZoneDivisionIds = empExistMapped?.Count() > 0 ? empExistMapped.Where(c => c.ZoneDivisionId > 0).Select(s => s.ZoneDivisionId).Distinct().ToList() : null;

                        var addableZoneDivisionIds = empExistMapped?.Count() > 0 && existZoneDivisionIds?.Count() > 0 ? zoneDivisionIds.Where(c => !existZoneDivisionIds.Contains(c)).ToList() : zoneDivisionIds;
                        empExistUpdateableList = empExistMapped?.Count() > 0 ? empExistMapped?.Where(c => c.ZoneDivisionId != null && zoneDivisionIds.Contains((int)c.ZoneDivisionId)).ToList() : null;
                        //empExistDeletableList = empExistMapped?.Count() > 0 ? empExistMapped?.Where(c => c.ZoneDivisionId != null && !zoneDivisionIds.Contains((int)c.ZoneDivisionId)).ToList() : null;

                        if (addableZoneDivisionIds?.Count() > 0)
                        {

                            foreach (var zoneDivisionId in addableZoneDivisionIds)
                            {
                                var obj = new EmployeeServicePointMap()
                                {
                                    EmployeeServicePoinMapId = 0,
                                    EmployeeId = model.Id,
                                    ZoneId = model.ZoneIds[0],
                                    ZoneDivisionId = zoneDivisionId,
                                    CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                                    CreatedDate = DateTime.Now,
                                    IsActive = true,
                                    CompanyId = model?.CompanyId > 0 ? (int)model.CompanyId : CompanyInfo.CompanyId,
                                };
                                addableMaps.Add(obj);
                            }
                        }
                        if (empExistUpdateableList?.Count() > 0)
                        {

                            foreach (var update in empExistUpdateableList)
                            {

                                update.ZoneId = model.ZoneIds[0];
                            }
                        }
                    }
                    else if (model.ZoneIds?.Length > 0)
                    {
                        var zoneIds = model.ZoneIds.Distinct().ToList();
                        var existZoneIds = empExistMapped?.Count() > 0 ? empExistMapped.Where(c => c.ZoneId > 0).Select(s => s.ZoneId).Distinct().ToList() : null;

                        var addableZoneIds = empExistMapped?.Count() > 0 && existZoneIds?.Count() > 0 ? zoneIds.Where(c => !existZoneIds.Contains(c)).ToList() : zoneIds;
                        empExistUpdateableList = empExistMapped?.Count() > 0 ? empExistMapped?.Where(c => c.ZoneId != null && zoneIds.Contains((int)c.ZoneId)).ToList() : null;
                        //empExistDeletableList = empExistMapped?.Count() > 0 ? empExistMapped?.Where(c => c.ZoneId != null && !zoneIds.Contains((int)c.ZoneId)).ToList() : null;

                        if (addableZoneIds?.Count() > 0)
                        {

                            foreach (var zoneId in addableZoneIds)
                            {
                                var obj = new EmployeeServicePointMap()
                                {
                                    //EmployeeServicePoinMapId = 0,
                                    EmployeeId = model.Id,
                                    ZoneId = zoneId,
                                    CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                                    CreatedDate = DateTime.Now,
                                    IsActive = true,
                                    CompanyId = model?.CompanyId > 0 ? (int)model.CompanyId : CompanyInfo.CompanyId,
                                };
                                addableMaps.Add(obj);
                            }
                        }
                    }

                    if (empExistUpdateableList?.Count() > 0)
                    {

                        foreach (var updateable in empExistUpdateableList)
                        {
                            updateable.IsActive = true;
                            updateable.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                            updateable.ModifiedDate = DateTime.Now;
                            updateableMaps.Add(updateable);
                        }

                    }

                    var existIds = empExistUpdateableList?.Count() > 0 ? empExistUpdateableList.Select(s => s.EmployeeServicePoinMapId).Distinct().ToList() : null;
                    empExistDeletableList = empExistMapped?.Count() > 0 && existIds?.Count() > 0 ? empExistMapped?.Where(c => !existIds.Contains(c.EmployeeServicePoinMapId)).ToList() : empExistMapped;

                    if (empExistDeletableList?.Count() > 0)
                    {

                        foreach (var deletable in empExistDeletableList)
                        {
                            deletable.IsActive = false;
                            deletable.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                            deletable.ModifiedDate = DateTime.Now;
                            updateableMaps.Add(deletable);
                        }
                    }

                    if (addableMaps?.Count() > 0)
                    {
                        _context.EmployeeServicePointMaps.AddRange(addableMaps);
                        result = _context.SaveChanges() > 0;
                    }

                    if (updateableMaps?.Count() > 0)
                    {
                        // _context.Entry(updateableMaps).State = EntityState.Modified; //work for single only

                        foreach (var entity in updateableMaps)
                        {
                            _context.Entry(entity).State = EntityState.Modified;
                        }
                        result = _context.SaveChanges() > 0;
                    }

                    return result;


                    #endregion

                    #endregion
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return false;
            }
        }

        public bool DeleteEmployee(long id)
        {
            Employee employee = _context.Employees.FirstOrDefault(x => x.Id == id);
            if (employee == null)
            {
                throw new Exception(Constants.DATA_NOT_FOUND);
            }

            _context.Employees.Remove(employee);
            return _context.SaveChanges() > 0;

        }

        public List<SelectModel> GetEmployeeSelectModels()
        {
            return _context.Employees.Where(c => c.Active).ToList().OrderBy(x => x.EmployeeId).Select(x => new SelectModel()
            {
                Text = "[" + x.EmployeeId.ToString() + "] " + x.Name,
                Value = x.Id.ToString()
            }).ToList();
        }

        public List<EmployeeModel> EmployeeSearch(string searchText)
        {
            IQueryable<Employee> employees = _context.Employees.Include("Department").Include("Designation").Include("DropDownItem").Where(x => x.Active && (x.EmployeeId.Contains(searchText) || x.Name.Contains(searchText) || x.Department.Name.Contains(searchText) || x.Designation.Name.Contains(searchText) || x.PABX.Contains(searchText) || x.MobileNo.Contains(searchText) || x.OfficeEmail.Contains(searchText) || x.EndReason.Contains(searchText) || x.DropDownItem.Name.Contains(searchText))).OrderBy(x => x.EmployeeOrder);
            return ObjectConverter<Employee, EmployeeModel>.ConvertList(employees.ToList()).ToList();
        }

        public List<EmployeeModel> GetBirthday()
        {
            var b = ObjectConverter<Employee, EmployeeModel>.ConvertList(
                _context.Employees.Include("Department").Include("Designation").Where(
                e => e.DateOfBirth.Value.Day == DateTime.Now.Day
                && e.DateOfBirth.Value.Month == DateTime.Now.Month).OrderBy(x => x.Id).ToList())
                .ToList();

            var bw = ObjectConverter<Employee, EmployeeModel>.ConvertList(
                _context.Employees.Include("Department").Include("Designation").Where(
                e => e.DateOfBirth.Value.Day == DateTime.Now.Day
                && e.DateOfBirth.Value.Month == DateTime.Now.Month).OrderBy(x => x.Id).ToList())

                .ToList();
            return b;
        }

        public List<EmployeeModel> GetEmployeeEvent()
        {
            dynamic result = _context.Database.SqlQuery<EmployeeModel>("exec sp_GetEmployeeEvent").ToList();
            return result;
        }

        public List<EmployeeModel> GetEmployeeTodayEvent()
        {
            dynamic result = _context.Database.SqlQuery<EmployeeModel>("exec sp_Employee_TodayAniversaryEvent").ToList();
            return result;
        }

        public List<EmployeeModel> GetProbitionPreiodEmployeeList()
        {
            dynamic result = _context.Database.SqlQuery<EmployeeModel>("exec sp_HRMS_GetProbitionPreiodEmployeeList").ToList();
            return result;
        }

        public object GetEmployeeAutoComplete(string prefix)
        {
            return _context.Employees.Where(x => x.Active && x.Name.Contains(prefix)).Select(x => new
            {
                label = x.Name + " [" + x.EmployeeId + "]",
                val = x.Id
            }).OrderBy(x => x.label).Take(10).ToList();

        }

        public List<EmployeeModel> GetTeamMembers(string searchText)
        {
            string managerId = System.Web.HttpContext.Current.User.Identity.Name;
            IQueryable<EmployeeModel> members = _context.Database.SqlQuery<EmployeeModel>("spGetTeamMembers {0}", managerId).AsQueryable();
            return members.Where(x => x.Name.ToLower().Contains(searchText.ToLower()) || String.IsNullOrEmpty(searchText)).ToList();
        }

        public EmployeeModel GetTeamMember(long id)
        {
            Employee employee = _context.Employees.Find(id);
            return ObjectConverter<Employee, EmployeeModel>.Convert(employee);
        }

        public bool UpdateTeamMember(EmployeeModel model)
        {
            if (model == null)
            {
                throw new Exception("Data missing!");
            }
            Employee member = ObjectConverter<EmployeeModel, Employee>.Convert(model);

            member = _context.Employees.FirstOrDefault(x => x.Id == model.Id);
            member.Active = model.Active;
            member.EndDate = model.EndDate;
            member.EndReason = model.EndReason;

            member.ModifedBy = System.Web.HttpContext.Current.User.Identity.Name;
            member.ModifiedDate = DateTime.Now;
            member.Department = null;

            _context.Entry(member).State = member.Id == 0 ? EntityState.Added : EntityState.Modified;
            return _context.SaveChanges() > 0;
        }

        public List<EmployeeModel> GetEmployeeAdvanceSearch(int? departmentId, int? designationId, string searchText)
        {
            IQueryable<EmployeeModel> queryable = _context.Database.SqlQuery<EmployeeModel>("sp_HRMS_GetEmployeeAdvanceSearch {0},{1},{2}", departmentId, designationId, searchText).AsQueryable();
            return queryable.ToList();
        }

        public long GetIdByKGID(string kgId)
        {
            try
            {
                return _context.Employees.First(x => x.EmployeeId.ToLower().Equals(kgId.ToLower())).Id;
            }
            catch (Exception)
            {

                return 0;
            }

        }

        public List<EmployeeModel> EmployeeSearch()
        {
            return _context.Database.SqlQuery<EmployeeModel>(@"select        EmployeeId, Name,
                                                                            isnull(replace(convert(NVARCHAR, JoiningDate, 105), ' ', '/'),'') as StrJoiningDate,
                                                                            isnull((select Name from Department where DepartmentId=Employee.DepartmentId),'') as DepartmentName,
                                                               			    isnull((select Name from Designation where DesignationId=Employee.DesignationId),'') as DesignationName,
                                                               			    isnull(OfficeEmail,'') as OfficeEmail,isnull(PABX,'') as PABX,
                                                               			    isnull(MobileNo,'') as MobileNo,
                                                               			    isnull((select Name from DropDownItem where DropDownItemId=Employee.BloodGroupId),'') as BloodGroupName,
                                                               			    isnull(Remarks,'') as Remarks
                                                              from          Employee
                                                              where         Active=1
                                                              order by      EmployeeOrder").ToList();
        }

        public object GetEmployeeDesignationAutoComplete(string prefix)
        {
            return _context.Employees.Include(x => x.Designation).Where(x => x.Active && x.Name.Contains(prefix)).Select(x => new
            {
                label = x.Name + " [" + x.Designation.Name + "]",
                val = x.Id
            }).OrderBy(x => x.label).Take(10).ToList();
        }

        public async Task<int> AddSalary(EmployeeVm model)
        {
            var obj = await _context.Employees.SingleOrDefaultAsync(x => x.Id == model.Id);
            if (obj != null)
            {
                obj.SalaryAmount = model.Samount;
                var res = await _context.SaveChangesAsync();
                return res;
            }
            return 0;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            disposed = true;
        }

    }
}
