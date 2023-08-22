using BSA.Service.Interface;
using BSA.Service.ServiceModel;
using BSA.Data.Models;
using BSA.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Validation;

namespace BSA.Service.Implementation
{
    public class SeedCategoryService : ISeedCategoryService
    {
        private bool disposed = false;

        BSAEntities context = new BSAEntities();
        public List<SeedCategoryModel> GetAllSeedCategory(string searchText)
        {
            IQueryable<SeedCategory> seedCategorys = context.SeedCategories.Where(x => x.Name.Contains(searchText)).OrderBy(x => x.Id);
            return ObjectConverter<SeedCategory, SeedCategoryModel>.ConvertList(seedCategorys.ToList()).ToList();
        }

        public List<SeedCategoryModel> GetSeedCategorys(string searchText,string memberId)
        {
            dynamic result = context.Database.SqlQuery<SeedCategoryModel>("exec sp_GetSeedCategories {0}, {1} ", searchText, memberId).ToList();
            return result;
            //IQueryable<SeedCategory> seedCategorys = context.SeedCategories.Where(x => x.Name.Contains(searchText)).OrderBy(x => x.Id);
            //return ObjectConverter<SeedCategory, SeedCategoryModel>.ConvertList(seedCategorys.ToList()).ToList();
        }
        public List<SelectModel> GetAllSeedCategory()
        {
            return context.SeedCategories.ToList().Select(x => new SelectModel()
            {
                Text = x.Name,
                Value = x.Id
            }).ToList();
        }
        public SeedCategoryModel GetSeedCategory(int id)
        {
            if (id == 0)
            {
                return new SeedCategoryModel() { Id = id };
            }
            SeedCategory seedCategory = context.SeedCategories.Find(id);
            return ObjectConverter<SeedCategory, SeedCategoryModel>.Convert(seedCategory);
        }

        public bool SaveSeedCategory(int id, SeedCategoryModel model)
        {
            bool result = false;
            try
            {
                SeedCategory seedCategory = ObjectConverter<SeedCategoryModel, SeedCategory>.Convert(model);
                if (id > 0)
                {
                    seedCategory.ModifiedDate = DateTime.Now;
                    seedCategory.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                }
                else
                {
                    seedCategory.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
                    seedCategory.CreatedDate = DateTime.Now;
                }
                seedCategory.Name = model.Name;
                seedCategory.NameBN = model.NameBN;

                seedCategory.Remarks = model.Remarks;
                context.Entry(seedCategory).State = seedCategory.Id == 0 ? EntityState.Added : EntityState.Modified;
                result = context.SaveChanges() > 0;
            }
            catch (DbEntityValidationException ex)
            {
                foreach (DbEntityValidationResult errors in ex.EntityValidationErrors)
                {
                    foreach (DbValidationError validationError in errors.ValidationErrors)
                    {
                        // get the error message 
                        string errorMessage = validationError.ErrorMessage;
                        result = false;
                    }
                }
            }
            return result;
        }

        public bool DeleteSeedCategory(int id)
        {
            throw new NotImplementedException();
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

    }
}
