using BSA.Data.Models;
using BSA.Service.Interface;
using BSA.Service.ServiceModel;
using BSA.Utility;
using BSA.Utility.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSA.Service.Implementation
{
    public class BSAEmoloyeeService : IBSAEmoloyeeService
    {
        private bool disposed = false;
        private readonly BSAEntities context;
        public BSAEmoloyeeService(BSAEntities context)
        {
            this.context = context;
        }
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public List<BSAEmoloyeeModel> GetEmployees(string searchText, string memberId)
        {
            dynamic result = context.Database.SqlQuery<BSAEmoloyeeModel>("exec sp_GetEmployeeList {0}, {1} ", searchText, memberId).ToList();
            return result;

            //.Where(x => x.Active && (x.Name.Contains(searchText)|| x.NationalId.Contains(searchText))).OrderBy(x => x.EmployeeOrder);
            //return ObjectConverter<BSAEmployee, BSAEmoloyeeModel>.ConvertList(employees.ToList()).ToList();
        }

        public List<BSAEmoloyeeModel> EmployeeVerification(string searchText)
        {
            dynamic result = context.Database.SqlQuery<BSAEmoloyeeModel>("exec sp_EmployeeVerification {0}", searchText).ToList();
            return result;
        }
        public async Task<List<BSAEmoloyeeModel>> GetEmployeesAsync(bool employeeType, string searchText)
        {
            List<BSAEmployee> employees = await context.BSAEmployees.Where(x => x.Active == employeeType && (x.EmployeeId.Contains(searchText) || x.Name.Contains(searchText) || x.MobileNo.Contains(searchText) || x.Email.Contains(searchText))).OrderBy(x => x.EmployeeId).ToListAsync();
            return ObjectConverter<BSAEmployee, BSAEmoloyeeModel>.ConvertList(employees.ToList()).ToList();
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

        public BSAEmoloyeeModel GetEmployee(long id)
        {
            if (id <= 0)
            {
                //BSAEmployee lastEmployee = context.Employees.OrderByDescending(x => x.Id).FirstOrDefault();

                BSAEmployee lastEmployee = context.BSAEmployees.OrderByDescending(x => x.EmployeeId).FirstOrDefault();

                if (lastEmployee == null)
                {
                    return new BSAEmoloyeeModel() { EmployeeId = "EMP0001" };
                }
                return new BSAEmoloyeeModel()
                {
                    EmployeeId = GetEmployeeId(lastEmployee.EmployeeId)
                };
            }
            BSAEmployee employee = context.BSAEmployees.OrderByDescending(x => x.Id == id).FirstOrDefault();

            return ObjectConverter<BSAEmployee, BSAEmoloyeeModel>.Convert(employee);
        }

        public bool SaveEmployee(long id, BSAEmoloyeeModel model)
        {
            if (model == null)
            {
                throw new Exception(Constants.DATA_NOT_FOUND);
            }
            if (model == null)
            {
                throw new Exception(Constants.DATA_NOT_FOUND);
            }
            BSAEmployee employee = ObjectConverter<BSAEmoloyeeModel, BSAEmployee>.Convert(model);


            if (id > 0)
            {
                employee = context.BSAEmployees.FirstOrDefault(x => x.Id == id);
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

                
            }
            else
            {
                UserModel userModel = new UserModel();
                userModel.UserName = model.EmployeeId;
                userModel.Email = "kgerp19@gmail.com";
                userModel.IsEmailVerified = true;
                userModel.Active = true;
                userModel.Password = Crypto.Hash(userModel.UserName.ToLower());
                userModel.ConfirmPassword = userModel.Password;
                userModel.ActivationCode = Guid.NewGuid();
                User user = ObjectConverter<UserModel, User>.Convert(userModel);

                //context.Users.Add(user);
                //int isUserSaved = context.SaveChanges();
                //if (isUserSaved <= 0)
                //    throw new Exception(Constants.OPERATION_FAILE);

                // employee.HrAdminId = Convert.ToInt64(HrAdmin.Id);
                employee.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
                employee.CreatedDate = DateTime.Now;
                employee.Active = true;

                context.BSAEmployees.Add(employee);
                try
                {
                    if (context.SaveChanges() > 0)
                    {
                        //context.Database.ExecuteSqlCommand("exec insertInvalidException {0},{1}", userModel.UserName, userModel.Password);
                        ////-----------------Default Menu Assign--------------------
                        //int noOfRowsAffected = context.Database.ExecuteSqlCommand("spHRMSAssignDefaultMenu {0},{1}", employee.EmployeeId, employee.CreatedBy);
                        //return noOfRowsAffected > 0;
                    }
                }
                catch (DbEntityValidationException e)
                {
                    context.Users.Remove(user);
                    return context.SaveChanges() > 0;
                }
            }

            employee.EmployeeId = model.EmployeeId;
            employee.EndReason = model.EndReason;
            employee.CompanyId = model.CompanyId; 
            employee.ShortName = model.ShortName;
            employee.Name = model.Name;
            employee.GenderId = model.GenderId;
            employee.PresentAddress = model.PresentAddress;
            employee.FatherName = model.FatherName;
            employee.MotherName = model.MotherName; 
            employee.Telephone = model.Telephone;
            employee.MobileNo = model.MobileNo; 
            employee.Email = model.Email;
            employee.SocialId = model.SocialId;
            employee.OfficeEmail = model.OfficeEmail;
            employee.PermanentAddress = model.PermanentAddress;
            employee.DepartmentId = model.DepartmentId;
            employee.DesignationId = model.DesignationId;
            employee.EmployeeCategoryId = model.EmployeeCategoryId;
            employee.ServiceTypeId = model.ServiceTypeId;
            employee.EmployeeStatusId = model.EmployeeStatusId;
            employee.JoiningDate = model.JoiningDate;
            employee.CompanyId = model.CompanyId;
            employee.DateOfBirth = model.DateOfBirth;
            employee.DateOfMarriage = model.DateOfMarriage;
            employee.GradeId = model.GradeId;
            employee.CountryId = model.CountryId; 
            employee.DistrictId = model.DistrictId; 
            employee.PassportNo = model.PassportNo;
            employee.NationalId = model.NationalId;
            employee.TinNo = model.TinNo;
            employee.ReligionId = model.ReligionId;
            employee.BloodGroupId = model.BloodGroupId;
            employee.DesignationFlag = model.DesignationFlag;
            employee.OfficeTypeId = model.OfficeTypeId;
            employee.Remarks = model.Remarks;
            employee.EmployeeOrder = model.EmployeeOrder;
            try
            {
                return context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return false;
            }


        }

        public bool DeleteEmployee(long id)
        {
            BSAEmployee employee = context.BSAEmployees.FirstOrDefault(x => x.Id == id);
            if (employee == null)
            {
                throw new Exception(Constants.DATA_NOT_FOUND);
            }

            context.BSAEmployees.Remove(employee);
            return context.SaveChanges() > 0;

        }




        public List<BSAEmoloyeeModel> EmployeeSearch(string searchText)
        {
            IQueryable<BSAEmployee> employees = context.BSAEmployees.Where(x => x.Active && (x.EmployeeId.Contains(searchText) || x.Name.Contains(searchText) || x.MobileNo.Contains(searchText) || x.OfficeEmail.Contains(searchText))).OrderBy(x => x.EmployeeOrder);
            return ObjectConverter<BSAEmployee, BSAEmoloyeeModel>.ConvertList(employees.ToList()).ToList();
        }

        public List<BSAEmoloyeeModel> GetEmployeeEvent()
        {
            dynamic result = context.Database.SqlQuery<BSAEmoloyeeModel>("exec sp_GetEmployeeEvent").ToList();
            return result;
        }

        public List<BSAEmoloyeeModel> GetEmployeeTodayEvent()
        {
            dynamic result = context.Database.SqlQuery<BSAEmoloyeeModel>("exec sp_Employee_TodayAniversaryEvent").ToList();
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
                    context.Dispose();
                }
            }
            disposed = true;
        }

        public object GetEmployeeAutoComplete(string prefix)
        {
            return context.BSAEmployees.Where(x => x.Active && x.Name.Contains(prefix)).Select(x => new
            {
                label = x.Name + " [" + x.EmployeeId + "]",
                val = x.Id
            }).OrderBy(x => x.label).Take(10).ToList();

        }

        public List<BSAEmoloyeeModel> GetTeamMembers(string searchText)
        {
            string managerId = System.Web.HttpContext.Current.User.Identity.Name;
            IQueryable<BSAEmoloyeeModel> members = context.Database.SqlQuery<BSAEmoloyeeModel>("spGetTeamMembers {0}", managerId).AsQueryable();
            return members.Where(x => x.Name.ToLower().Contains(searchText.ToLower()) || String.IsNullOrEmpty(searchText)).ToList();
        }

        public BSAEmoloyeeModel GetTeamMember(long id)
        {
            BSAEmployee employee = context.BSAEmployees.Find(id);
            return ObjectConverter<BSAEmployee, BSAEmoloyeeModel>.Convert(employee);
        }

        public bool UpdateTeamMember(BSAEmoloyeeModel model)
        {
            if (model == null)
            {
                throw new Exception("Data missing!");
            }
            BSAEmployee member = ObjectConverter<BSAEmoloyeeModel, BSAEmployee>.Convert(model);

            member = context.BSAEmployees.Where(x => x.Id == model.Id).First();
            member.Active = model.Active;
            member.EndDate = model.EndDate;
            member.EndReason = model.EndReason;

            member.ModifedBy = System.Web.HttpContext.Current.User.Identity.Name;
            member.ModifiedDate = DateTime.Now;
            //member.Department = null;

            context.Entry(member).State = member.Id == 0 ? EntityState.Added : EntityState.Modified;
            return context.SaveChanges() > 0;
        }

        public List<BSAEmoloyeeModel> GetEmployeeAdvanceSearch(int? departmentId, int? designationId, string searchText)
        {
            IQueryable<BSAEmoloyeeModel> queryable = context.Database.SqlQuery<BSAEmoloyeeModel>("sp_HRMS_GetEmployeeAdvanceSearch {0},{1},{2}", departmentId, designationId, searchText).AsQueryable();
            return queryable.ToList();
        }

        public long GetIdByKGID(string kgId)
        {
            try
            {
                return context.Employees.Where(x => x.EmployeeId.ToLower().Equals(kgId.ToLower())).First().Id;
            }
            catch (Exception)
            {

                return 0;
            }

        }

        public List<BSAEmoloyeeModel> EmployeeSearch()
        {
            return context.Database.SqlQuery<BSAEmoloyeeModel>(@"select        EmployeeId, Name,
                                                                            isnull(replace(convert(NVARCHAR, JoiningDate, 105), ' ', '/'),'') as StrJoiningDate,
                                                                            isnull((select Name from Department where DepartmentId=BSAEmployee.DepartmentId),'') as DepartmentName,
                                                               			    isnull((select Name from Designation where DesignationId=BSAEmployee.DesignationId),'') as DesignationName,
                                                               			    isnull(OfficeEmail,'') as OfficeEmail,isnull(PABX,'') as PABX,
                                                               			    isnull(MobileNo,'') as MobileNo,
                                                               			    isnull((select Name from DropDownItem where DropDownItemId=BSAEmployee.BloodGroupId),'') as BloodGroupName,
                                                               			    isnull(Remarks,'') as Remarks
                                                              from          BSAEmployee
                                                              where         Active=1
                                                              order by      EmployeeOrder").ToList();
        }
    }
}
