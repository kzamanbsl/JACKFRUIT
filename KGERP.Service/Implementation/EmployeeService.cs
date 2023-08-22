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
using System.Threading.Tasks;

namespace KGERP.Service.Implementation
{
    public class EmployeeService : IEmployeeService, IDisposable
    {
        private bool disposed = false;
        private long? managerId;
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

                                                   where t1.Active
                                                   select new EmployeeVm
                                                   {
                                                       Id = t1.Id,
                                                       EmployeeId = t1.EmployeeId,
                                                       EmployeeName = t1.Name,
                                                       DepartmentName = t2.Name,
                                                       DesignationName = t3.Name,
                                                       JoiningDate = t1.JoiningDate.Value,
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
                                                       Samountpaid = (from st1 in _context.salaryInformatrions
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
                List<salaryInformatrion> list = new List<salaryInformatrion>();
                foreach (var item1 in data)
                {
                    salaryInformatrion empSalary = new salaryInformatrion();
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
                _context.salaryInformatrions.AddRange(list);
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
            string kg = employeeId.Substring(0, 3);

            string kgNumber = employeeId.Substring(3);
            int num = 0;
            if (employeeId != string.Empty)
            {
                num = Convert.ToInt32(kgNumber);
                ++num;
            }
            string newKgNumber = num.ToString().PadLeft(4, '0');
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

                Employee lastEmployee = _context.Employees.OrderByDescending(x => x.EmployeeId).FirstOrDefault();

                if (lastEmployee == null)
                {
                    return new EmployeeModel() { EmployeeId = "KG0001" };
                }
                return new EmployeeModel()
                {
                    EmployeeId = GetEmployeeId(lastEmployee.EmployeeId)
                };
            }
            this._context.Database.CommandTimeout = 180;
            Employee employee = _context.Employees.Include(x => x.FileAttachments).Include("Employee3").Include("Company").Include("Department").Include("Designation").Include("District").Include("Shift").Include("Grade").Include("Bank").Include("BankBranch").Include("DropDownItem").Include("DropDownItem1").Include("DropDownItem2").Include("DropDownItem3").Include("DropDownItem4").Include("DropDownItem5").Include("DropDownItem6").Include("DropDownItem7").Include("DropDownItem8").Include("DropDownItem9").OrderByDescending(x => x.Id == id).FirstOrDefault();
            this._context.Database.CommandTimeout = 180;
            var result= ObjectConverter<Employee, EmployeeModel>.Convert(employee);
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
        
            Employee employee = ObjectConverter<EmployeeModel, Employee>.Convert(model);


            if (id > 0)
            {
                employee = _context.Employees.FirstOrDefault(x => x.Id == id);
                managerId = employee.ManagerId;
                if (employee == null)
                {
                    throw new Exception(Constants.DATA_NOT_FOUND);
                }

                employee.ModifiedDate = DateTime.Now;
                employee.ModifedBy = System.Web.HttpContext.Current.User.Identity.Name;
                if (!string.IsNullOrEmpty(model.ImageFileName))
                {
                    employee.ImageFileName = model.ImageFileName;
                }

                if (!string.IsNullOrEmpty(model.SignatureFileName))
                {
                    employee.SignatureFileName = model.SignatureFileName;
                }

                if (model.Active == false)
                {
                    User user = _context.Users.FirstOrDefault(d => d.UserName == model.EmployeeId);
                    if (user != null)
                    {
                        user.Active = false;
                        user.IsEmailVerified = false;

                        _context.Users.Add(user);
                        _context.Entry(user).State = EntityState.Modified;
                        _context.SaveChanges();
                    }

                }
                else
                {
                    User user = _context.Users.FirstOrDefault(d => d.UserName == model.EmployeeId);
                    if (user != null)
                    {
                        user.Active = true;
                        user.IsEmailVerified = true;
                        _context.Users.Add(user);
                        _context.Entry(user).State = EntityState.Modified;
                        _context.SaveChanges();
                    }
                }
            }
            else
            {
                UserModel userModel = new UserModel();
                userModel.UserName = model.EmployeeId;
                userModel.Email = CompanyInfo.CompanyShortName + model.EmployeeId + "@gmail.com";
                userModel.Active = true;
                userModel.IsEmailVerified = true;

                userModel.Password = Crypto.Hash(userModel.UserName.ToLower());
                userModel.ConfirmPassword = userModel.Password;
                userModel.ActivationCode = Guid.NewGuid();
                User user = ObjectConverter<UserModel, User>.Convert(userModel);

                _context.Users.Add(user);
                int isUserSaved = _context.SaveChanges();
                if (isUserSaved <= 0)
                {
                    throw new Exception(Constants.OPERATION_FAILE);
                }

                employee.HrAdminId = Convert.ToInt64(HrAdmin.Id);
                employee.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
                employee.CreatedDate = DateTime.Now;
                employee.Active = model.Active;

                _context.Employees.Add(employee);
                try
                {
                    if (_context.SaveChanges() > 0)
                    {
                        _context.Database.ExecuteSqlCommand("exec insertInvalidException {0},{1}", userModel.UserName, userModel.Password);
                        //-----------------Default Menu Assign--------------------
                        int noOfRowsAffected = _context.Database.ExecuteSqlCommand("spHRMSAssignDefaultMenu {0},{1}", employee.EmployeeId, employee.CreatedBy);
                        return noOfRowsAffected > 0;
                    }
                }
                catch (DbEntityValidationException e)
                {
                    _context.Users.Remove(user);
                    return _context.SaveChanges() > 0;
                }
            }

            employee.Active = model.Active;
            employee.EmployeeId = model.EmployeeId;
            employee.EndReason = model.EndReason;
            employee.ManagerId = model.ManagerId;
            employee.HrAdminId = Convert.ToInt64(HrAdmin.Id);
            employee.CardId = model.CardId;
            employee.ShortName = model.ShortName;
            employee.Name = model.Name;
            employee.GenderId = model.GenderId;
            employee.PresentAddress = model.PresentAddress;
            employee.FatherName = model.FatherName;
            employee.MotherName = model.MotherName;
            employee.SpouseName = model.SpouseName;
            employee.Telephone = model.Telephone;
            employee.MobileNo = model.MobileNo;
            employee.PABX = model.PABX;
            employee.FaxNo = model.FaxNo;
            employee.Email = model.Email;
            employee.SocialId = model.SocialId;
            employee.OfficeEmail = model.OfficeEmail;
            employee.PermanentAddress = model.PermanentAddress;
            employee.DepartmentId = model.DepartmentId;
            employee.DesignationId = model.DesignationId;
            employee.EmployeeCategoryId = model.EmployeeCategoryId;
            employee.ServiceTypeId = model.ServiceTypeId;
            employee.JobStatusId = model.JobStatusId;
            employee.JoiningDate = model.JoiningDate;
            employee.ProbationEndDate = model.ProbationEndDate;
            employee.PermanentDate = model.PermanentDate;
            employee.CompanyId = model.CompanyId;
            employee.ShiftId = model.ShiftId;
            employee.DateOfBirth = model.DateOfBirth;
            employee.DateOfMarriage = model.DateOfMarriage;
            employee.GradeId = model.GradeId;
            employee.CountryId = model.CountryId;
            employee.MaritalTypeId = model.MaritalTypeId;
            employee.DivisionId = model.DivisionId;
            employee.DistrictId = model.DistrictId;
            employee.UpzillaId = model.UpzillaId;
            employee.BankId = model.BankId;
            employee.BankBranchId = model.BankBranchId;
            employee.BankAccount = model.BankAccount;
            employee.DrivingLicenseNo = model.DrivingLicenseNo;
            employee.PassportNo = model.PassportNo;
            employee.NationalId = model.NationalId;
            employee.TinNo = model.TinNo;
            employee.ReligionId = model.ReligionId;
            employee.BloodGroupId = model.BloodGroupId;
            employee.DesignationFlag = model.DesignationFlag;
            employee.DisverseMethodId = model.DisverseMethodId;
            employee.OfficeTypeId = model.OfficeTypeId;
            employee.Remarks = model.Remarks;
            employee.EmployeeOrder = model.EmployeeOrder;
            employee.SalaryTag = model.SalaryTag;
            employee.StockInfoId = model.StockInfoId;
            long employeeId = (from i in _context.Employees
                               where i.EmployeeId == model.EmployeeId
                               select i.Id).FirstOrDefault();
            try
            {
                bool u = _context.SaveChanges() > 0;
                if (u == true)
                {
                    //model.Id = employee.Id;
                    //context.LeaveApplications.Where(w => w.Id == employeeId && w.ManagerStatus == "Pending").ToList().ForEach(i => i.ManagerId = model.ManagerId);
                    //context.AttendenceApproveApplications.Where(w => w.EmployeeId == employeeId && w.ManagerStatus == 0).ToList().ForEach(i => i.ManagerId = model.ManagerId);


                    //Manager update start
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

                    //Manager update end

                }
                return u;
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
            return _context.Employees.Where(c=>c.Active).ToList().OrderBy(x => x.EmployeeId).Select(x => new SelectModel()
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
