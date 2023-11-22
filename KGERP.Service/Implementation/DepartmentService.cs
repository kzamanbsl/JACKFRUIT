using KGERP.Data.Models;
using KGERP.Service.Implementation.Configuration;
using KGERP.Service.Interface;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace KGERP.Service.Implementation
{
    public class DepartmentService : IDepartmentService
    {
        ERPEntities _db = new ERPEntities();

        public async Task<VMCommonDepartment> GetDepartments(int companyId)
        {
            VMCommonDepartment vmCommonDepartment = new VMCommonDepartment();

            vmCommonDepartment.CompanyFK = companyId;

            vmCommonDepartment.DataList = await Task.Run(() => (from t1 in _db.Departments
                                                                where t1.IsActive == true && t1.CompanyId == companyId
                                                                select new VMCommonDepartment
                                                                {
                                                                    ID = t1.DepartmentId,
                                                                    Name = t1.Name,
                                                                    CompanyFK = t1.CompanyId
                                                                }).OrderByDescending(x => x.ID).AsEnumerable());

            return vmCommonDepartment;
        }


        public async Task<int> DepartmentAdd(VMCommonDepartment vmCommonDepartment)
        {
            var result = -1;

            #region IsExist
            var isExist = await _db.Departments.FirstOrDefaultAsync(c => c.Name.Equals(vmCommonDepartment.Name) && c.IsActive == true);
            if (isExist?.DepartmentId > 0)
            {
                throw new Exception($"Sorry! This Name {vmCommonDepartment.Name} already Exists!");
            }
            #endregion

            Department department = new Department
            {
                Name = vmCommonDepartment.Name,
                CompanyId = vmCommonDepartment.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true

            };
            _db.Departments.Add(department);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = department.DepartmentId;
            }
            return result;
        }

        public async Task<int> DepartmentEdit(VMCommonDepartment vmCommonDepartment)
        {
            var result = -1;
            #region IsExist
            var isExist = await _db.Departments.FirstOrDefaultAsync(c => c.Name.Equals(vmCommonDepartment.Name) && c.DepartmentId != vmCommonDepartment.ID && c.IsActive == true);
            if (isExist?.DepartmentId > 0)
            {
                throw new Exception($"Sorry! This Name {vmCommonDepartment.Name} already Exists!");
            }
            #endregion
            Department department = _db.Departments.Find(vmCommonDepartment.ID);
            department.Name = vmCommonDepartment.Name;


            department.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            department.ModifiedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = department.DepartmentId;
            }
            return result;
        }


        public async Task<int> DepartmentDelete(int id)
        {
            int result = -1;
            if (id != 0)
            {
                Department department = await _db.Departments.FindAsync(id);
                department.IsActive = false;

                if (await _db.SaveChangesAsync() > 0)
                {
                    result = department.DepartmentId;
                }
            }
            return result;
        }

        public async Task<bool> CheckDepartmentName(string name, int id)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            bool isExist = false;
            if (id > 0)
            {
                isExist = await _db.Departments.AnyAsync(u => u.Name.Equals(name) && u.DepartmentId != id && u.IsActive == true);
            }
            else
            {
                isExist = await _db.Departments.AnyAsync(u => u.Name.Equals(name) && u.IsActive == true);
            }
            return isExist;
        }



        public List<SelectModel> GetDepartmentSelectModels()
        {
            return _db.Departments.Where(x => x.IsActive == true && x.CompanyId == CompanyInfo.CompanyId).OrderBy(x => x.Name).ToList().Select(x => new SelectModel()
            {
                Text = x.Name.ToString(),
                Value = x.DepartmentId.ToString()
            }).ToList();
        }

        public List<SelectListItem> GetDepartmentSelectListModels()
        {
            return _db.Departments.OrderBy(x => x.Name).ToList().Select(x => new SelectListItem()
            {
                Text = x.Name.ToString(),
                Value = x.DepartmentId.ToString()
            }).ToList();
        }
    }
}
