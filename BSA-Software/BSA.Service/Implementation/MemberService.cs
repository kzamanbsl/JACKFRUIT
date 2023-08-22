using BSA.Service.Interface;
using BSA.Service.ServiceModel;
using BSA.Data.Models;
using BSA.Utility;
using BSA.Utility.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;

namespace BSA.Service.Implementation
{
    public class MemberService : IMemberService
    {
        private bool disposed = false;
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        BSAEntities memberRepository = new BSAEntities();

        public List<MemberModel> GetEmployees(string searchText)
        {
            IQueryable<BSAMember> employees = memberRepository.BSAMembers.Where(x => x.Id != 1 && x.Id != 2 && x.Active && (x.MemberId.Contains(searchText) || x.MemberNameBn.Contains(searchText) || x.MobileNo.Contains(searchText) || x.Email.Contains(searchText))).OrderBy(x => x.MemberOrder).ThenBy(x =>x.Id);
            return ObjectConverter<BSAMember, MemberModel>.ConvertList(employees.ToList()).ToList();
        }

        public async Task<List<MemberModel>> GetPreviousEmployeesAsync(string searchText)
        {
            List<BSAMember> employees = await memberRepository.BSAMembers.Where(x => !x.Active && (x.MemberId.Contains(searchText) || x.MemberNameBn.Contains(searchText) || x.MobileNo.Contains(searchText) || x.Email.Contains(searchText))).OrderBy(x => x.MemberId).ToListAsync();
            return ObjectConverter<BSAMember, MemberModel>.ConvertList(employees.ToList()).ToList();
        }

        private string GetMemberId(string employeeId)
        {
            string bsa = employeeId.Substring(0, 2);

            string bsaNumber = employeeId.Substring(2);
            int num = 0;
            if (employeeId != string.Empty)
            {
                num = Convert.ToInt32(bsaNumber);
                ++num;
            }
            string newbsaNumber = num.ToString().PadLeft(3, '0');
            return bsa + newbsaNumber;
        }

        public MemberModel GetEmployee(long id)
        {
            if (id <= 0)
            {
                BSAMember lastEmployee = memberRepository.BSAMembers.OrderByDescending(x => x.Id).FirstOrDefault();

                if (lastEmployee == null)
                {
                    return new MemberModel();
                    //{ EmployeeId = "001" };
                }
                return new MemberModel()
                {
                    //EmployeeId = GetEmployeeId(lastEmployee.EmployeeId)
                };
            }

            BSAMember employee = memberRepository.BSAMembers.OrderByDescending(x => x.Id == id).FirstOrDefault();
            return ObjectConverter<BSAMember, MemberModel>.Convert(employee);
        }

        public bool SaveEmployee(long id, MemberModel model)
        {
            if (model == null)
            {
                throw new Exception(Constants.DATA_NOT_FOUND);
            }
            if (model == null)
            {
                throw new Exception(Constants.DATA_NOT_FOUND);
            }
            BSAMember bSAMember = ObjectConverter<MemberModel, BSAMember>.Convert(model);


            if (id > 0)
            {
                bSAMember = memberRepository.BSAMembers.FirstOrDefault(x => x.Id == id);
                if (bSAMember == null)
                {
                    throw new Exception(Constants.DATA_NOT_FOUND);
                }

                bSAMember.ModifiedDate = DateTime.Now;
                bSAMember.ModifedBy = "";
                if (!string.IsNullOrEmpty(model.LogoUrl))
                {
                    bSAMember.LogoUrl = model.LogoUrl;
                }
            }
            else
            {
                UserModel userModel = new UserModel();
                userModel.UserName = model.MemberId;
                userModel.Email = model.Email;
                userModel.IsEmailVerified = true;
                userModel.Active = true;
                userModel.Password = Crypto.Hash(userModel.UserName.ToUpper());
                userModel.ConfirmPassword = userModel.Password;

                User user = ObjectConverter<UserModel, User>.Convert(userModel);

                memberRepository.Users.Add(user);
                int isUserSaved = memberRepository.SaveChanges();
                if (isUserSaved <= 0)
                    throw new Exception(Constants.OPERATION_FAILE);

                bSAMember.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
                bSAMember.CreatedDate = DateTime.Now;
                bSAMember.Active = true;

                memberRepository.BSAMembers.Add(bSAMember);
                try
                {
                    int isEmployeeSaved = memberRepository.SaveChanges();
                    if (isEmployeeSaved > 0)
                    {
                        try
                        {
                            int noOfRowsAffected = memberRepository.Database.ExecuteSqlCommand("spAssignDefaultMenu {0},{1}", bSAMember.MemberId, bSAMember.CreatedBy);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex);
                        }
                    }

                }
                catch (Exception)
                {
                    memberRepository.Users.Remove(user);
                    return memberRepository.SaveChanges() > 0;
                }
            }

            bSAMember.MemberId = model.MemberId;
            bSAMember.MemberType = model.MemberType;
            bSAMember.MemberNameBn = model.MemberName;
      
            if (model.MemberType == "Seed Industry")
            {
                bSAMember.MemberOrder = 1;
            }
            else if (model.MemberType == "Seed Deller")
            {
                bSAMember.MemberOrder = 2;
            }
            else if (model.MemberType == "Associate")
            {
                bSAMember.MemberOrder = 3;
            }
            else
            {
                bSAMember.MemberOrder = 4;
            }
            bSAMember.Delegate = model.Delegate;
            bSAMember.DelegateEmail = model.DelegateEmail;
            bSAMember.AddressOne = model.AddressOne;
            bSAMember.AddressTwo = model.AddressTwo;
            bSAMember.Telephone = model.Telephone;
            bSAMember.MobileNo = model.MobileNo;
            bSAMember.Email = model.Email;
            bSAMember.TIN = model.TIN;
            bSAMember.Website = model.Website;
            bSAMember.OfficeEmail = model.OfficeEmail;


            try
            {
                bool result = memberRepository.SaveChanges() > 0;
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
            BSAMember employee = memberRepository.BSAMembers.FirstOrDefault(x => x.Id == id);
            if (employee == null)
            {
                throw new Exception(Constants.DATA_NOT_FOUND);
            }
            memberRepository.BSAMembers.Remove(employee);
            return memberRepository.SaveChanges() > 0;
        }

        public List<SelectModel> GetEmployeeSelectModels()
        {
            return memberRepository.BSAMembers.ToList().OrderBy(x => x.MemberId).Select(x => new SelectModel()
            {
                Text = "[" + x.MemberId.ToString() + "] " + x.MemberNameBn,
                Value = x.Id.ToString()
            }).ToList();
        }

        public List<MemberModel> EmployeeSearch(string searchText)
        {
            IQueryable<BSAMember> employees = memberRepository.BSAMembers.Where(x => x.Active && (x.MemberId.Contains(searchText) || x.MemberNameBn.Contains(searchText) || x.MobileNo.Contains(searchText) || x.OfficeEmail.Contains(searchText))).OrderBy(x => x.Id);
            return ObjectConverter<BSAMember, MemberModel>.ConvertList(employees.ToList()).ToList();
        }

        public List<MemberModel> GetEmployeeEvent()
        {
            dynamic result = memberRepository.Database.SqlQuery<MemberModel>("exec sp_GetEmployeeEvent").ToList();
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
                    memberRepository.Dispose();
                }
            }
            disposed = true;
        }

        public object GetEmployeeAutoComplete(string prefix)
        {
            return memberRepository.BSAMembers.Where(x => x.MemberNameBn.Contains(prefix)).Select(x => new
            {
                label = x.MemberNameBn + " [" + x.MemberId + "]",
                val = x.Id
            }).OrderBy(x => x.label).Take(10).ToList();

        }

        public string GetNewMemberId(string memberType)
        {
            string memberId = string.Empty;
            if (!string.IsNullOrEmpty(memberType))
            {
                BSAMember lastMember = memberRepository.BSAMembers.Where(n => n.MemberType == memberType).OrderByDescending(x => x.Id).FirstOrDefault();

                if (lastMember == null)
                {
                    return memberId = "001";
                }
                else
                {
                    return memberId = GenerateNewMemberId(lastMember.MemberId);
                }
            }
            else
            {
                return memberId = "";
            }
        }

        private string GenerateNewMemberId(string employeeId)
        {
            string kg = employeeId.Substring(0, 2);

            string kgNumber = employeeId.Substring(2);
            int num = 0;
            if (employeeId != string.Empty)
            {
                num = Convert.ToInt32(kgNumber);
                ++num;
            }
            string newKgNumber = num.ToString().PadLeft(3, '0');
            return kg + newKgNumber;
        }

    }
}
