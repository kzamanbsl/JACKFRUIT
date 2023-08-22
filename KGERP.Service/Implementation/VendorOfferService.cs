using KGERP.Data.Models;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace KGERP.Service.Implementation
{
    public class VendorOfferService : IVendorOfferService
    {
        private readonly ERPEntities context;
        public VendorOfferService(ERPEntities context)
        {
            this.context = context;
        }

        public VendorOfferModel GetVendorOffer(int id)
        {
            if (id == 0)
            {
                return new VendorOfferModel() { VendorId = id };
            }
            return context.Database.SqlQuery<VendorOfferModel>(@"select VendorOfferId,
               VendorId,ProductId,ProductType,
               (select ProductName from Erp.Product where ProductId=Erp.VendorOffer.ProductId) as ProductName,
               (select ProductCode from Erp.Product where ProductId=Erp.VendorOffer.ProductId) as ProductCode,
               BaseCommission,CashCommission,CarryingCommission,FactoryCarryingCommission,SpecialCommission,AdditionPrice,IsActive
               from Erp.VendorOffer
               where VendorOfferId={0}", id).FirstOrDefault();

        }

        public List<VendorOfferModel> GetVendorOffers(int vendorId, string productType, string searchText)
        {
            IQueryable<VendorOfferModel> models = context.Database.SqlQuery<VendorOfferModel>("exec spGetProductOffers {0},{1}", vendorId, productType).AsQueryable();
            return models.Where(x => x.ProductName.ToLower().Contains(searchText.ToLower()) || string.IsNullOrEmpty(searchText)).OrderBy(x => x.ProductCategory).ThenBy(x => x.ProductCode).ToList();
        }



        public int InsertCustomerOffer(int companyId)
        {
            int count = 0;
            List<Vendor> customers = context.Vendors.Where(x => x.CompanyId == companyId && x.VendorTypeId == (int)Provider.Customer && x.IsActive).ToList();
            foreach (var customer in customers)
            {
                List<VendorOffer> vendorOffers = new List<VendorOffer>();
                List<Product> products = context.Products.Include(x => x.ProductCategory).Include(x => x.ProductSubCategory).Where(x => x.ProductCategory.ProductType == "F" && x.ProductCategory.CompanyId == 8).ToList();
                foreach (var product in products)
                {
                    decimal cashCommission = 0;
                    decimal carryingCommission = 0;
                    if (customer.CustomerType == "Cash")
                    {
                        cashCommission = product.ProductCategory.CashCustomerRate ?? 0;
                    }

                    VendorOffer vendorOffer = new VendorOffer
                    {
                        VendorId = customer.VendorId,
                        ProductId = product.ProductId,
                        BaseCommission = product.ProductSubCategory.BaseCommissionRate,
                        CashCommission = cashCommission,
                        CarryingCommission = carryingCommission,
                        SpecialCommission = 0,
                        CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    };
                    vendorOffers.Add(vendorOffer);
                    count++;
                }
                context.VendorOffers.AddRange(vendorOffers);
                context.SaveChanges();
            }
            return count;
        }

        public bool SaveVendorOffer(long id, VendorOfferModel model)
        {
            int noOfRowsAffected = 0;
            if (model == null)
            {
                throw new Exception("Data missing!");
            }

            //bool exist = context.VendorOffers.Where(x => x.VendorId == model.VendorId && x.ProductId == model.ProductId && x.VendorOfferId != id).Any();

            //if (exist)
            //{
            //    throw new Exception("Already exist!");
            //}
            VendorOffer vendorOffer = ObjectConverter<VendorOfferModel, VendorOffer>.Convert(model);
            if (id > 0)
            {
                vendorOffer = context.VendorOffers.Find(id);
                if (vendorOffer == null)
                {
                    throw new Exception("Data not found!");
                }
                vendorOffer.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                vendorOffer.ModifiedDate = DateTime.Now;
            }

            else
            {
                vendorOffer.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
                vendorOffer.CreatedDate = DateTime.Now;
            }


            vendorOffer.VendorId = model.VendorId;
            vendorOffer.ProductId = model.ProductId;
            vendorOffer.BaseCommission = model.BaseCommission;
            vendorOffer.CashCommission = model.CashCommission;

            vendorOffer.CarryingCommission = model.CarryingCommission;
            vendorOffer.FactoryCarryingCommission = model.FactoryCarryingCommission;
            vendorOffer.SpecialCommission = model.SpecialCommission;
            vendorOffer.AdditionPrice = model.AdditionPrice;


            vendorOffer.IsActive = model.IsActive;

            context.Entry(vendorOffer).State = vendorOffer.VendorOfferId == 0 ? EntityState.Added : EntityState.Modified;
            noOfRowsAffected = context.SaveChanges();
            if (noOfRowsAffected > 0)
            {
                if (model.IsAllBase)
                {
                    context.Database.ExecuteSqlCommand("update Erp.VendorOffer set BaseCommission={0} where VendorId={1}", model.BaseCommission, model.VendorId);
                }
                if (model.IsAllCash)
                {
                    context.Database.ExecuteSqlCommand("update Erp.VendorOffer set CashCommission={0} where VendorId={1}", model.CashCommission, model.VendorId);
                }

                if (model.IsAllCarrying)
                {
                    context.Database.ExecuteSqlCommand("update Erp.VendorOffer set CarryingCommission={0} where VendorId={1}", model.CarryingCommission, model.VendorId);
                }
                if (model.IsAllFactoryCarrying)
                {
                    context.Database.ExecuteSqlCommand("update Erp.VendorOffer set FactoryCarryingCommission={0} where VendorId={1}", model.FactoryCarryingCommission, model.VendorId);
                }
                if (model.IsAllSpecial)
                {
                    context.Database.ExecuteSqlCommand("update Erp.VendorOffer set SpecialCommission={0} where VendorId={1}", model.SpecialCommission, model.VendorId);
                }

                if (model.IsAllAdditionPrice)
                {
                    context.Database.ExecuteSqlCommand("update Erp.VendorOffer set AdditionPrice={0} where VendorId={1}", model.AdditionPrice, model.VendorId);
                }

            }
            return noOfRowsAffected > 0;
        }
    }
}
