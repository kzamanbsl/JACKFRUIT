using KGERP.Data.Models;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace KGERP.Service.Implementation
{
    public class DropDownItemService : IDropDownItemService
    {
        private readonly ERPEntities context;
        public DropDownItemService(ERPEntities context)
        {
            this.context = context;
        }



        public List<DropDownItemModel> GetDropDownItems(string searchText)
        {
            IQueryable<DropDownItem> queryable = context.DropDownItems.Include(x => x.DropDownType).Where(x => (x.Name.ToLower().Contains(searchText.ToLower()) || string.IsNullOrEmpty(searchText)) ||
                                                                                                               (x.DropDownType.Name.ToLower().Contains(searchText.ToLower()) || string.IsNullOrEmpty(searchText))).OrderBy(x => x.Name);
            return ObjectConverter<DropDownItem, DropDownItemModel>.ConvertList(queryable.ToList()).ToList();
        }

        public DropDownItemModel GetDropDownItem(int id)
        {
            if (id <= 0)
            {
                return new DropDownItemModel()
                {
                    CompanyId = (int)System.Web.HttpContext.Current.Session["CompanyId"],
                    IsActive = true
                };
            }
            DropDownItem dropDownItem = context.DropDownItems.FirstOrDefault(x => x.DropDownItemId == id);

            return ObjectConverter<DropDownItem, DropDownItemModel>.Convert(dropDownItem);
        }

        public List<SelectModel> GetDropDownItemSelectModels(int id)
        {
            if (id == 16)
            {
                return context.DropDownItems.ToList().Where(x => x.DropDownTypeId == id).OrderBy(x => x.OrderNo).Select(x => new SelectModel()
                {
                    Text = x.Name,
                    Value = x.DropDownItemId
                }).ToList();

            }
            else
            {
                return context.DropDownItems.ToList().Where(x => x.DropDownTypeId == id).OrderBy(x => x.Name).Select(x => new SelectModel()
                {
                    Text = x.Name,
                    Value = x.DropDownItemId
                }).ToList();
            }
        }

        public bool SaveDropDownItem(int id, DropDownItemModel model, out string message)
        {
            message = string.Empty;
            if (model == null)
            {
                throw new Exception("Dropdown data missing!");
            }

            bool exist = context.DropDownItems.Where(x => x.DropDownTypeId == model.DropDownTypeId && x.Name.ToLower().Equals(model.Name.ToLower()) && x.DropDownItemId != id).Any();

            if (exist)
            {
                message = "Dropdown Item data already exist";
                return false;
            }
            DropDownItem dropDownItem = ObjectConverter<DropDownItemModel, DropDownItem>.Convert(model);
            if (id > 0)
            {
                dropDownItem = context.DropDownItems.FirstOrDefault(x => x.DropDownItemId == id);
                if (dropDownItem == null)
                {
                    throw new Exception("Dropdown Item not found!");
                }
                dropDownItem.ModifiedDate = DateTime.Now;
                dropDownItem.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            }

            else
            {
                dropDownItem.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
                dropDownItem.CreatedDate = DateTime.Now;

            }

            dropDownItem.DropDownTypeId = model.DropDownTypeId;
            dropDownItem.CompanyId = model.CompanyId;
            dropDownItem.ItemValue = model.ItemValue;
            dropDownItem.Name = model.Name;
            dropDownItem.Remarks = model.Remarks;
            dropDownItem.OrderNo = model.OrderNo;
            dropDownItem.IsActive = model.IsActive;
            context.Entry(dropDownItem).State = dropDownItem.DropDownItemId == 0 ? EntityState.Added : EntityState.Modified;
            return context.SaveChanges() > 0;
        }

        public bool DeleteDropDownItem(int id)
        {
            DropDownItem dropDownItem = context.DropDownItems.Find(id);
            if (dropDownItem == null)
            {
                throw new Exception("Data not found");
            }
            context.DropDownItems.Remove(dropDownItem);
            return context.SaveChanges() > 0;
        }
        public async Task<DropDownTypeModel> GetDropDownTypes(int companyId)
        {
            DropDownTypeModel dropDownTypeModel = new DropDownTypeModel();
            dropDownTypeModel.CompanyId = companyId;
            dropDownTypeModel.DataList = await Task.Run(() => (from t1 in context.DropDownTypes
                                                               where t1.CompanyId == companyId
                                                               select new DropDownTypeModel
                                                               {
                                                                   DropDownTypeId = t1.DropDownTypeId,
                                                                   Name = t1.Name,
                                                                   Remarks = t1.Remarks
                                                               }).OrderBy(o => o.Name).AsEnumerable());
            return dropDownTypeModel;
        }
        public async Task<DropDownItemModel> GetDropDownItems(int companyId)
        {
            DropDownItemModel dropDownItemModel = new DropDownItemModel();
            dropDownItemModel.CompanyId = companyId;
            dropDownItemModel.DataList = await Task.Run(() =>(from t1 in context.DropDownItems
                                                              join t2 in context.DropDownTypes on t1.DropDownTypeId equals t2.DropDownTypeId
                                                              where t1.CompanyId == companyId
                                                              select new DropDownItemModel
                                                              {
                                                                  DropDownItemId = t1.DropDownItemId,
                                                                  TypeName = t2.Name,
                                                                  Name= t1.Name,
                                                                  ItemValue= t1.ItemValue ??0,
                                                                  Remarks= t1.Remarks,
                                                                  OrderNo= t1.OrderNo
                                                              }).AsEnumerable());

            return dropDownItemModel;
        }
    }
}
