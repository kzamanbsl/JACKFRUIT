using KGERP.Data.Models;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using KGERP.Utility.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Drawing;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;

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

        public async Task<EmployeeVm> GetEmployees()
        {
            EmployeeVm model = new EmployeeVm();

            model.DataList = await Task.Run(() => (from t1 in _context.Employees
                                                   join t2 in _context.Departments on t1.DepartmentId equals t2.DepartmentId into t2_Join
                                                   from t2 in t2_Join.DefaultIfEmpty()
                                                   join t3 in _context.Designations on t1.DesignationId equals t3.DesignationId into t3_Join
                                                   from t3 in t3_Join.DefaultIfEmpty()
                                                   //join t4 in _context.Zones on t1.Id equals t4.EmployeeId into t4_Join
                                                   //from t4 in t4_Join.DefaultIfEmpty()
                                                   //join t5 in _context.Regions on t1.Id equals t5.EmployeeId into t5_Join
                                                   //from t5 in t5_Join.DefaultIfEmpty()
                                                   //join t6 in _context.Areas on t1.Id equals t6.EmployeeId into t6_Join
                                                   //from t6 in t6_Join.DefaultIfEmpty()
                                                   //join t7 in _context.SubZones on t1.Id equals t7.EmployeeId into t7_Join
                                                   //from t7 in t7_Join.DefaultIfEmpty()


                                                   where t1.Active
                                                   select new EmployeeVm
                                                   {
                                                       Id = t1.Id,
                                                       EmployeeId = t1.EmployeeId,
                                                       EmployeeName = t1.Name,
                                                       DepartmentName = t2.Name,
                                                       DesignationName = t3.Name,
                                                       JoiningDate= t1.JoiningDate.Value,
                                                       //ServiceArea = string.Join(", ", t4.Name,t5.Name,t6.Name,t7.Name),
                                                       //ServiceArea = (t4.Name ?? "") + (t5.Name ?? "") + (t6.Name ?? "") + (t7.Name ?? ""),
                                                       MobileNo = t1.MobileNo,
                                                       Email = t1.Email,
                                                       Samount = (decimal)((decimal)t1.SalaryAmount == null ? 0 : t1.SalaryAmount)

                                                   }).OrderBy(o => o.EmployeeId)
                                                   .AsEnumerable());

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

                Employee lastEmployee = _context.Employees.Where(c=>c.EmployeeId.StartsWith("AZ")).OrderByDescending(x => x.EmployeeId).FirstOrDefault();

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

            var result = new EmployeeModel()
            {

                Id = employee.Id,
                EmployeeId = employee.EmployeeId,
                ManagerId = employee.ManagerId,
                ManagerName = employee.Employee3?.Name,
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
                BankAccount = employee.EmployeeId,
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
                SalaryTag = employee.SalaryTag ?? 0,
                SalaryAmount = employee.SalaryAmount,
                StockInfoId = employee.StockInfoId,

                CreatedBy = employee.CreatedBy,
                CreatedDate = employee.CreatedDate,
                ModifedBy = employee.ModifedBy,
                ModifiedDate = employee.ModifiedDate,

                //SubZoneIds = new int[0],
                //AreaIds = new int[0],
                //RegionIds = new int[0],
                //ZoneIds = new int[0],

            };

            if (id > 0)
            {
                var territorys=employee.SubZones;
                var areas=employee.Areas;
                var reagions=employee.Regions;
                var zones=employee.Zones;

                if(territorys?.Count()>0)
                {
                    var subZoneIds= territorys.Select(c => c.SubZoneId).ToArray();
                    result.SubZoneIds= subZoneIds;

                    var areaId = territorys.FirstOrDefault().AreaId ?? 0;
                    result.AreaIds = new int[] {areaId};

                    var regionId= territorys.FirstOrDefault().RegionId ?? 0;
                    result.RegionIds= new int[] { regionId };

                    var zoneId= territorys.FirstOrDefault().ZoneId;
                    result.ZoneIds = new int[] { zoneId };
                }
                else if (areas?.Count() > 0)
                {
                    var areaids= areas.Select(c => c.AreaId).ToArray();
                    result.AreaIds = areaids;

                    var regionId= areas.FirstOrDefault().RegionId;
                    result.RegionIds = new int[] { regionId };

                    var zoneId= areas.FirstOrDefault().ZoneId;
                    result.ZoneIds = new int[] { zoneId };
                }
                else if (reagions?.Count() > 0)
                {
                    var regionIds= reagions.Select(c => c.RegionId).ToArray();
                    result.RegionIds = regionIds;

                    var zoneId = reagions.FirstOrDefault().ZoneId;
                    result.ZoneIds = new int[] { zoneId };
                }
                else if (zones?.Count() > 0)
                {
                    result.ZoneIds = (int[])zones.Select(c => c.ZoneId);
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

                #region UserUpdate
               
                if (model.Active == false)
                {
                    User user = _context.Users.FirstOrDefault(d => d.UserName == model.EmployeeId);
                    if (user != null)
                    {
                        user.Active = false;
                        user.IsEmailVerified = false;
                        user.UserTypeId = (int)EnumUserType.Employee;

                        _context.Users.Add(user);
                        _context.Entry(user).State = EntityState.Modified;
                        result = _context.SaveChanges() > 0;
                    }
                }
                else
                {
                    User user = _context.Users.FirstOrDefault(d => d.UserName == model.EmployeeId);
                    if (user != null)
                    {
                        user.Active = true;
                        user.IsEmailVerified = true;
                        user.UserTypeId = (int)EnumUserType.Employee;

                        _context.Users.Add(user);
                        _context.Entry(user).State = EntityState.Modified;
                        result = _context.SaveChanges()>0;
                    }
                }
                #endregion

                model.Id = updateEmployee.Id;
            }
            else
            {
                #region UserCreate
                UserModel userModel = new UserModel();
                userModel.UserName = model.EmployeeId;
                userModel.Email = CompanyInfo.CompanyShortName + model.EmployeeId + "@gmail.com";
                userModel.Active = true;
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
                    if (result==true)
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
                    //context.AttendenceApproveApplications.Where(w => w.EmployeeId == employeeId && w.ManagerStatus == 0).ToList().ForEach(i => i.ManagerId = model.ManagerId);

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

                    #region Zone Region Area and Territory Update
                    if (model.SubZoneIds?.Length > 0)
                    {
                        var subZoneIds = model.SubZoneIds.Distinct();
                        var subZones = _context.SubZones.Where(c => subZoneIds.Contains(c.SubZoneId));
                        foreach (var subZone in subZones)
                        {
                            var name= model.Name != null ? model.Name : subZone.SalesOfficerName;
                            var designation = model.Designation?.Name != null ? model.Designation?.Name : subZone.Designation;
                            var email = model.Email != null ? model.Email : subZone.Email;
                            var mobileOffice= model.MobileNo != null ? model.MobileNo : subZone.MobileOffice;
                            var mobilePersonal= model.Telephone != null ?  model.Telephone : subZone.MobilePersonal;

                            subZone.EmployeeId = model.Id;
                            subZone.SalesOfficerName = name;
                            subZone.Designation = designation;
                            subZone.Email = email;
                            subZone.MobileOffice = mobileOffice;
                            subZone.MobilePersonal = mobilePersonal;
                        }
                        
                    }
                    else if (model.AreaIds?.Length > 0)
                    {
                        var areaIds = model.AreaIds.Distinct();
                        var areas = _context.Areas.Where(c => areaIds.Contains(c.AreaId));
                        foreach (var area in areas)
                        {
                            var areaIncharge = model.Name != null ? model.Name : area.AreaIncharge;
                            var designation = model.Designation?.Name != null ? model.Designation?.Name : area.Designation;
                            var email = model.Email != null ? model.Email : area.Email;
                            var mobileOffice = model.MobileNo != null ? model.MobileNo : area.MobileOffice;
                            var mobilePersonal = model.Telephone != null ? model.Telephone : area.MobilePersonal;

                            area.EmployeeId = model.Id;
                            area.AreaIncharge = areaIncharge;
                            area.Designation = designation;
                            area.Email = email;
                            area.MobileOffice = mobileOffice;
                            area.MobilePersonal = mobilePersonal;
                        }
                    }
                    else if (model.RegionIds?.Length > 0)
                    {
                        var regionIds = model.RegionIds.Distinct();
                        var regions = _context.Regions.Where(c => regionIds.Contains(c.RegionId));
                        foreach (var region in regions)
                        {
                            var regionIncharge = model.Name != null ? model.Name : region.RegionIncharge;
                            var designation = model.Designation?.Name != null ? model.Designation?.Name : region.Designation;
                            var email = model.Email != null ? model.Email : region.Email;
                            var mobileOffice = model.MobileNo != null ? model.MobileNo : region.MobileOffice;
                            var mobilePersonal = model.Telephone != null ? model.Telephone : region.MobilePersonal;

                            region.EmployeeId = model.Id;

                            region.RegionIncharge = regionIncharge;
                            region.Designation = designation;
                            region.Email = email;
                            region.MobileOffice = mobileOffice;
                            region.MobilePersonal = mobilePersonal;
                        }
                    }
                    else if (model.ZoneIds?.Length > 0)
                    {
                        var zoneIds = model.ZoneIds.Distinct();
                        var zones = _context.Zones.Where(c => zoneIds.Contains(c.ZoneId));
                        foreach (var zone in zones)
                        {
                            var zoneIncharge = model.Name != null ? model.Name : zone.ZoneIncharge;
                            var designation = model.Designation?.Name != null ? model.Designation?.Name : zone.Designation;
                            var email = model.Email != null ? model.Email : zone.Email;
                            var mobileOffice = model.MobileNo != null ? model.MobileNo : zone.MobileOffice;
                            var mobilePersonal = model.Telephone != null ? model.Telephone : zone.MobilePersonal;

                            zone.EmployeeId = model.Id;
                            zone.ZoneIncharge = zoneIncharge;
                            zone.Designation = designation;
                            zone.Email = email;
                            zone.MobileOffice = mobileOffice;
                            zone.MobilePersonal = mobilePersonal;
                        }
                    }
                    return _context.SaveChanges() > 0;
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


    }
}
