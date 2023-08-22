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
    public class SeedService : ISeedService
    {
        private bool disposed = false;

        BSAEntities context = new BSAEntities();
        public List<SeedModel> GetAllSeed(string searchText)
        {
            IQueryable<Seed> seeds = context.Seeds.Where(x => x.ProductName.Contains(searchText)).OrderBy(x => x.Id);
            return ObjectConverter<Seed, SeedModel>.ConvertList(seeds.ToList()).ToList();
        }

        public List<SeedModel> GetSeeds(string searchText, string memberId)
        {
            dynamic result = context.Database.SqlQuery<SeedModel>("exec sp_GetSeedList {0}, {1} ", searchText, memberId).ToList();
            return result;
        }

        public List<SeedModel> SeedVerification(string searchText)
        {
            dynamic result = context.Database.SqlQuery<SeedModel>("exec sp_GetSeedVarification {0} ", searchText).ToList();
            return result;
        }

        public SeedModel GetSeed(int id)
        {
            if (id == 0)
            {
                return new SeedModel() { Id = id };
            }
            Seed Seed = context.Seeds.Find(id);
            return ObjectConverter<Seed, SeedModel>.Convert(Seed);
        }

        public bool SaveSeed(int id, SeedModel model)
        {
            bool result = false;
            try
            {
                Seed Seed = ObjectConverter<SeedModel, Seed>.Convert(model);
                if (id > 0)
                {
                    Seed.ModifiedDate = DateTime.Now;
                    Seed.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                }
                else
                {
                    Seed.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
                    Seed.CreatedDate = DateTime.Now;
                }
                Seed.ProductName = model.ProductName;
                Seed.ProductNameBN = model.ProductNameBN;
                Seed.SeedCategoryId = model.SeedCategoryId;


                Seed.Remarks = model.Remarks;
                context.Entry(Seed).State = Seed.Id == 0 ? EntityState.Added : EntityState.Modified;
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

        public bool DeleteSeed(int id)
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
