using KGERP.Data.Models;
using KGERP.Service.Implementation.Configuration;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace KGERP.Service.Implementation
{
    public class HrDesignationService
    {
        private readonly ERPEntities _context = new ERPEntities();

        public async Task<VMCommonHrDesignation> GetHrDesignations(int companyId)
        {
            VMCommonHrDesignation vmCommonDesignation = new VMCommonHrDesignation();

            vmCommonDesignation.CompanyFK = companyId;

            vmCommonDesignation.DataList = await Task.Run(() => (from t1 in _context.Designations
                                                                where t1.IsActive == true && t1.CompanyId == companyId
                                                                select new VMCommonHrDesignation
                                                                {
                                                                    ID = t1.DesignationId,
                                                                    Name = t1.Name,
                                                                    CompanyFK = t1.CompanyId
                                                                }).OrderByDescending(x => x.ID).AsEnumerable());

            return vmCommonDesignation;
        }


        public async Task<int> HrDesignationAdd(VMCommonHrDesignation vmCommonDesignation)
        {
            var result = -1;

            #region IsExist
            var isExist = vmCommonDesignation.ID > 0 ? _context.Designations.FirstOrDefault(c => c.Name.ToLower() == vmCommonDesignation.Name.ToLower() && c.DesignationId != vmCommonDesignation.ID && c.IsActive == true) : _context.Designations.FirstOrDefault(c => c.Name.ToLower() == vmCommonDesignation.Name.ToLower() && c.IsActive == true);
            if (isExist?.DesignationId > 0)
            {
                throw new Exception($"Sorry! This Name {vmCommonDesignation.Name} already Exist!");
            }
            #endregion

            Designation department = new Designation
            {
                Name = vmCommonDesignation.Name,
                CompanyId = vmCommonDesignation.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true

            };
            _context.Designations.Add(department);
            if (await _context.SaveChangesAsync() > 0)
            {
                result = department.DesignationId;
            }
            return result;
        }

        public async Task<int> HrDesignationEdit(VMCommonHrDesignation vmCommonDesignation)
        {
            var result = -1;
            Designation department = _context.Designations.Find(vmCommonDesignation.ID);
            department.Name = vmCommonDesignation.Name;


            department.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            department.ModifiedDate = DateTime.Now;

            if (await _context.SaveChangesAsync() > 0)
            {
                result = department.DesignationId;
            }
            return result;
        }


        public async Task<int> HrDesignationDelete(int id)
        {
            int result = -1;
            if (id != 0)
            {
                Designation department = await _context.Designations.FindAsync(id);
                department.IsActive = false;

                if (await _context.SaveChangesAsync() > 0)
                {
                    result = department.DesignationId;
                }
            }
            return result;
        }

        public bool ChecckName(VMCommonHrDesignation designation)
        {
            var exit = _context.Designations.FirstOrDefault(f => f.Name.Trim() == designation.Name.Trim());
            if (exit == null)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> CheckDesignationName(string name, int id)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            bool isExist = false;
            if (id > 0)
            {
                isExist = await _context.Designations.AnyAsync(u => u.Name.ToLower() == name.ToLower() && u.DesignationId != id && u.IsActive == true);
            }
            else
            {
                isExist = await _context.Designations.AnyAsync(u => u.Name.ToLower() == name.ToLower() && u.IsActive == true);
            }
            return isExist;
        }

    }
}
