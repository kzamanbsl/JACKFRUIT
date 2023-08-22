using KGERP.Data.Models;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System.Collections.Generic;
using System.Linq;

namespace KGERP.Service.Implementation
{
    public class VoucherTypeService : IVoucherTypeService
    {
        private readonly ERPEntities _context;
        public VoucherTypeService(ERPEntities context)
        {
            this._context = context;
        }
        public List<VoucherTypeModel> GetVoucherTypes()
        {
            IQueryable<VoucherType> vendors = _context.VoucherTypes.AsQueryable();
            return ObjectConverter<VoucherType, VoucherTypeModel>.ConvertList(vendors.ToList()).ToList();

        }

        public List<SelectModel> GetVoucherTypeSelectModels(int companyId = 0)
        {
            if (companyId == (int)CompanyName.GloriousCropCareLimited || companyId == (int)CompanyName.KrishibidSeedLimited)
            {
                return _context.VoucherTypes.Where(x => x.CompanyId == companyId && x.IsActive == true).ToList().Select(x => new SelectModel()
                {
                    Text = x.Name,
                    Value = x.VoucherTypeId
                }).ToList();
            }
            else if (companyId > 0)
            {
                return _context.VoucherTypes.Where(x => x.CompanyId == companyId && x.IsActive == true).ToList().Select(x => new SelectModel()
                {
                    Text = x.Name,
                    Value = x.VoucherTypeId
                }).ToList();
            }
            else
            {
                return _context.VoucherTypes.Where(x => x.CompanyId == null && x.IsActive == true).ToList().Select(x => new SelectModel()
                {
                    Text = x.Name,
                    Value = x.VoucherTypeId
                }).ToList();
            }

        }

        public List<SelectModel> GetAccountingCostCenter(int companyId)
        {
            List<SelectModel> selectModelList = new List<SelectModel>();
            SelectModel selectModel = new SelectModel
            {
                Text = "== Select Cost Center ==",
                Value = 0,
            };
            selectModelList.Add(selectModel);

            var v = _context.Accounting_CostCenter.Where(x => x.CompanyId == companyId && x.IsActive == true).ToList()
               .Select(x => new SelectModel()
               {
                   Text = x.Name,
                   Value = x.CostCenterId
               }).ToList();

            selectModelList.AddRange(v);

            //if (companyId == 7 || companyId == 9)
            //{
            //    var v = context.KGREProjects.Where(x => x.CompanyId == companyId && x.IsAccounting==1).ToList()
            //    .Select(x => new SelectModel()
            //    {
            //        Text = x.ProjectName,
            //        Value = x.ProjectId
            //    }).ToList();
            //    selectModelLiat.AddRange(v);
            //}
            //else
            //{

            //}

            return selectModelList;
        }

        public List<SelectModel> GetProductCategory(int companyId)
        {
            List<SelectModel> selectModelList = new List<SelectModel>();
            SelectModel selectModel = new SelectModel
            {
                Text = "==Select Product Category==",
                Value = 0,
            };

            selectModelList.Add(selectModel);

            var v = _context.ProductCategories.Where(x => x.CompanyId == companyId && x.IsActive==true).ToList()
                .Select(x => new SelectModel()
                {
                    Text = x.Name,
                    Value = x.ProductCategoryId
                }).ToList();

            selectModelList.AddRange(v);
            return selectModelList;
        }

        public List<SelectModel> GetProductCategoryGldl(int companyId)
        {
            List<SelectModel> selectModelList = new List<SelectModel>();
            var v = _context.ProductCategories.Where(x => x.CompanyId == companyId && x.IsActive == true).ToList()
                .Select(x => new SelectModel()
                {
                    Text = x.Name,
                    Value = x.ProductCategoryId
                }).ToList();

            selectModelList.AddRange(v);

            return selectModelList;
        }

    }
}
