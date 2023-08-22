using KGERP.Data.Models;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace KGERP.Service.Implementation
{
    public class CompanyService : ICompanyService
    {
        private readonly ERPEntities context;
        public CompanyService(ERPEntities context)
        {
            this.context = context;
        }
        public List<CompanyModel> GetCompanies(string searchText)
        {
            IQueryable<Company> companies = context.Companies.Where(x => (x.Name.ToLower().Contains(searchText.ToLower()) || string.IsNullOrEmpty(searchText)) || (x.ShortName.ToLower().Contains(searchText.ToLower())
            || string.IsNullOrEmpty(searchText))).OrderBy(x => x.OrderNo);
            List<CompanyModel> models = ObjectConverter<Company, CompanyModel>.ConvertList(companies.ToList()).ToList();
            return models;
        }

        public CompanyModel GetCompany(int id)
        {
            if (id <= 0)
            {
                return new CompanyModel()
                {
                    IsCompany = true,
                    IsActive = true,
                    CompanyLogo = "~/Images/Logo/logo.png"
                };
            }
            Company company = context.Companies.FirstOrDefault(x => x.CompanyId == id);
            return ObjectConverter<Company, CompanyModel>.Convert(company);
        }

        public List<SelectModel> GetCompanySelectModels()
        {
            return context.Companies.Where(x => x.IsCompany && x.IsActive).OrderBy(x => x.Name).ToList().Select(x => new SelectModel()
            {
                Text = x.Name.ToString(),
                Value = x.CompanyId
            }).ToList();
        }

        public List<SelectModel> GetAllCompanySelectModels()
        {
            return context.Companies.OrderBy(x => x.Name).ToList().Select(x => new SelectModel()
            {
                Text = x.Name.ToString(),
                Value = x.CompanyId
            }).ToList();
        }

        public List<SelectModel> GetAllCompanySelectModels2()
        {
            return context.Companies.Where(f => f.IsDepartment == true).OrderBy(x => x.Name).ToList().Select(x => new SelectModel()
            {
                Text = x.Name.ToString(),
                Value = x.CompanyId
            }).ToList();
        }

        public List<SelectModel> GetKGRECompnay()
        {

            return context.Database.SqlQuery<Company>("Select * from Company  where CompanyId  in (7,9) order by OrderNo").ToList().Select(x => new SelectModel()
            {
                Text = x.Name,
                Value = x.CompanyId
            }).ToList();
        }

        public List<SelectModel> GetFilterCompanySelectModels()
        {
            return context.Database.SqlQuery<Company>("Select * from Company  where CompanyId not in (1,26,27,30,31,227) order by OrderNo").ToList().Select(x => new SelectModel()
            {
                Text = x.Name,
                Value = x.CompanyId
            }).ToList();
        }

        public bool SaveCompany(int id, CompanyModel model)
        {
            if (model == null)
            {
                throw new Exception("Company data missing!");
            }

            Company company = ObjectConverter<CompanyModel, Company>.Convert(model);
            if (id > 0)
            {
                company = context.Companies.FirstOrDefault(x => x.CompanyId == id);
                if (company == null)
                {
                    throw new Exception("Company Menu not found!");
                }
                company.ModifiedDate = DateTime.Now;
                company.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;

                company.CompanyId = model.CompanyId;
                company.Name = model.Name;
                company.ShortName = model.ShortName;
                company.OrderNo = model.OrderNo;
                company.MushokNo = model.MushokNo;
                company.Address = model.Address;
                company.Phone = model.Phone;
                company.Fax = model.Fax;
                company.Email = model.Email;

                if (!string.IsNullOrEmpty(model.CompanyLogo))
                {
                    company.CompanyLogo = model.CompanyLogo;
                }
                company.IsCompany = model.IsCompany;
                company.IsActive = model.IsActive;
                return context.SaveChanges() > 0;
            }

            else
            {
                company.CompanyId = context.Database.SqlQuery<int>("exec spGetNewCompanyId").FirstOrDefault();
                company.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
                company.CreatedDate = DateTime.Now;

                company.Name = model.Name;
                company.ShortName = model.ShortName;
                company.OrderNo = model.OrderNo;
                company.MushokNo = model.MushokNo;
                company.Address = model.Address;
                company.Phone = model.Phone;
                company.Fax = model.Fax;
                company.Email = model.Email;
                if (!string.IsNullOrEmpty(model.CompanyLogo))
                {
                    company.CompanyLogo = model.CompanyLogo;
                }
                company.IsCompany = model.IsCompany;
                company.IsActive = model.IsActive;

                context.Companies.Add(company);
                return context.SaveChanges() > 0;
            }
        }

        public List<SelectModel> GetCompanySelectModelById(int companyId)
        {
            return context.Companies.Where(x => x.CompanyId == companyId).ToList().Select(x => new SelectModel()
            {
                Text = x.Name.ToString(),
                Value = x.CompanyId
            }).ToList();
        }

        public List<SelectModel> GetSaleYearSelectModel()
        {
            List<SelectModel> years = new List<SelectModel>();
            int beginYear = Convert.ToInt32(ConfigurationManager.AppSettings["SalesBeginYear"]);
            int endYear = DateTime.Today.Year;
            for (int i = beginYear; i <= endYear; i++)
            {
                years.Add(new SelectModel { Text = i.ToString(), Value = i.ToString() });
            }
            return years;
        }
    }
}

