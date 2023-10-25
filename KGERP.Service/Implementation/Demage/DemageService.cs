using KGERP.Data.Models;
using KGERP.Service.Implementation.Accounting;
using KGERP.Service.Implementation.Configuration;
using KGERP.Service.Implementation.Procurement;
using KGERP.Service.Implementation.Warehouse;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace KGERP.Service.Implementation.ProdMaster
{
    public class DemageService : IDemageService
    {

        private readonly ERPEntities context;
        private readonly ConfigurationService configurationService;
        public DemageService(ERPEntities context, ConfigurationService configurationService)
        {
            this.context = context;
            this.configurationService = configurationService;
        }
      
        public async Task<int> DemageMasterAdd(DemageMasterModel model)
        {
            int result = -1;
            DemageMaster demageMaster = new DemageMaster
            {
                DemageMasterId = model.DemageMasterId,
                OperationDate = model.OperationDate,
                
                DemageFromId = model.DemageFromId,
                FromDeportId = model.FromDeportId,
                FromDealerId = model.FromDealerId,
                FromCustomerId = model.FromCustomerId,
                ToStockInfoId = model.ToStockInfoId,
                ToDeportId = model.ToDeportId,
                ToDealerId = model.ToDealerId,
                Remarks = model.Remarks,
                StatusId = model.StatusId,
                CompanyId = (int)model.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),
                CreateDate = DateTime.Now,
                IsActive = true
            };
            context.DemageMasters.Add(demageMaster);
            if (await context.SaveChangesAsync() > 0)
            {
                result = demageMaster.DemageMasterId;
            }
            return result;
        }
        public async Task<int> DemageDetailAdd(DemageMasterModel model)
        {
            int result = -1;
            DemageDetail demageDetail = new DemageDetail
            {
                DemageMasterId = model.DemageMasterId,
                DemageDetailId = model.DetailModel.DemageDetailId,
                DemageTypeId = model.DetailModel.DemageTypeId,
                ProductId = model.DetailModel.ProductId,
                DemageQty = model.DetailModel.DemageQty,
                UnitPrice = model.DetailModel.UnitPrice,
                TotalPrice = model.DetailModel.TotalPrice,
                Remarks = model.DetailModel.Remarks,
                CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),
                CreateDate = DateTime.Now,
                IsActive = true,

            };
            context.DemageDetails.Add(demageDetail);

            if (await context.SaveChangesAsync() > 0)
            {
                result = demageDetail.DemageMasterId;
            }

            return result;
        }
        public async Task<int> DemageDetailEdit(DemageMasterModel model)
        {
            int result = -1;
            DemageDetail demageDetail = await context.DemageDetails.FindAsync(model.DetailModel.DemageDetailId);
            if (demageDetail == null) throw new Exception("Sorry! item not found!");

            demageDetail.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            demageDetail.ModifiedDate = DateTime.Now;
            demageDetail.DemageTypeId = model.DetailModel.DemageTypeId;
            demageDetail.ProductId = model.DetailModel.ProductId;
            demageDetail.DemageQty = model.DetailModel.DemageQty;
            demageDetail.UnitPrice = model.DetailModel.UnitPrice;
            demageDetail.TotalPrice = model.DetailModel.TotalPrice;
            demageDetail.Remarks = model.DetailModel.Remarks;
            demageDetail.IsActive = true;
            if (await context.SaveChangesAsync() > 0)
            {
                result = demageDetail.DemageDetailId;
            }

            return result;
        }


    }
}
