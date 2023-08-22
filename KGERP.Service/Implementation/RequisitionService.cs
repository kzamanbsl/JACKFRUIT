using KGERP.Data.Models;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGERP.Service.Implementation
{
    public class RequisitionService : IRequisitionService
    {
        private readonly ERPEntities context;
        public RequisitionService(ERPEntities context)
        {
            this.context = context;
        }
        public int GetRequisitionNo()
        {
            int requisitionId = 0;
            var value = context.Requisitions.OrderByDescending(x => x.RequisitionId).FirstOrDefault();
            if (value != null)
            {
                requisitionId = value.RequisitionId + 1;
            }
            else
            {
                requisitionId = requisitionId + 1;
            }
            return requisitionId;
        }

        public async Task<int> CreateProductionRequisition(RequisitionModel model)
        {
            int requisitionItemId = -0;

            int result = -1;
            #region Check Packing Materials
            List<Product> products = new List<Product>();
            Product product = context.Products.Where(x => x.ProductId == model.ProductId && x.PackId == null).FirstOrDefault();
            if (product != null)
            {
                products.Add(product);
            }

            if (products.Any())
            {
                var s = string.Join(",", products.Where(p => p.PackId == null)
                                    .Select(p => p.ProductName.ToString()));
                StringBuilder sb = new StringBuilder();
                sb.Append("Bag is not selected for product " + s + ". Please select bag first !");
                //message = sb.ToString();
                return result;
            }
            #endregion

            //Requisition requisition = ObjectConverter<RequisitionModel, Requisition>.Convert(model);
            #region Requisition No
            var lastReqNo = context.Requisitions.Where(q => q.CompanyId == (int)CompanyName.KrishibidFeedLimited)
               .OrderByDescending(o => o.RequisitionId)
               .Select(s => s.RequisitionNo).FirstOrDefault();


            string reNo = "";
            if (lastReqNo == null)
            {
                reNo = "R000001";
            }
            else
            {
                reNo = GenerateRequisitionNo(lastReqNo);
            }
            #endregion

            Requisition requisition = new Requisition
            {
                RequisitionDate = model.RequisitionDate,
                Description = model.Description,
                RequisitionNo = reNo,
                IsActive = true,
                RequisitionBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                RequisitionStatus = "N",
                CompanyId = model.CompanyId
            };
            context.Requisitions.Add(requisition);
            if (context.SaveChanges() > 0)
            {
                result = requisition.RequisitionId;
            }



            return result;
        }
        public async Task<int> CreateProductionRequisitionItem(RequisitionModel model)
        {
            int requisitionItemId = -0;

            int result = -1;
            #region Check Packing Materials
            //List<Product> products = new List<Product>();
            Product packingItemCheck = context.Products.Where(x => x.ProductId == model.ProductId && x.PackId == null).FirstOrDefault();          

            if (packingItemCheck != null)
            {
               
                StringBuilder sb = new StringBuilder();
                sb.Append("Bag is not selected for product " + packingItemCheck.ProductName + ". Please select bag first !");
                
                return result;
            }
            #endregion

            //Requisition requisition = ObjectConverter<RequisitionModel, Requisition>.Convert(model);

            Product product = context.Products.Find(model.ProductId);


            RequisitionItem requisitionItem = new RequisitionItem
            {
                RequisitionId = model.RequisitionId,
                ProductId = model.ProductId,
                RequisitionItemStatus = "N",
                Qty = model.Qty,
                InputQty = model.InputQty,
                OutputQty = model.OutputQty,
                ActualProcessLoss = model.ActualProcessLoss,
                OverHead = model.OverHead,
                ProcessLoss = model.ProcessLoss,
                BagId= context.Products.SingleOrDefault(s=>s.ProductId == model.ProductId)?.PackId,

              //  BagQty = Convert.ToInt32(Math.Ceiling(model.Qty/ Convert.ToDecimal(product.PackSize))),
                IsActive = true
            };
            context.RequisitionItems.Add(requisitionItem);
            if (context.SaveChanges() > 0)
            {
                result = requisitionItem.RequisitionItemId;
            }

            return result;
        }



        public List<RequisitionItemModel> GetRequisitionItemIssueStatus(int requisitionId)
        {
            IQueryable<RequisitionItem> requisitionItems = context.RequisitionItems.Include(x => x.Product).Where(x => x.RequisitionId == requisitionId);

            return ObjectConverter<RequisitionItem, RequisitionItemModel>.ConvertList(requisitionItems.ToList()).ToList();
        }

        public ProductModel GetProcessLossAmount(int productId)
        {
            ProductModel data = ObjectConverter<Product, ProductModel>.Convert(context.Products.Where(x => x.ProductId == productId).FirstOrDefault());


            return data;
        }

        public bool DeleteRequisition(int requisitionId)
        {
            return context.Database.ExecuteSqlCommand("sp_Feed_DeleteRequisition {0}", requisitionId) > 0;
        }
        public async Task<RequisitionModel> GetRequisition(int companyId, int requisitionId)
        {
            RequisitionModel model = new RequisitionModel();
            model = await Task.Run(() => (from t1 in context.Requisitions
                                          .Where(x => x.IsActive == true && x.RequisitionId == requisitionId && x.CompanyId == companyId)
                                          select new RequisitionModel
                                          {
                                              RequisitionNo = t1.RequisitionNo,
                                              RequisitionId = t1.RequisitionId,
                                              Description = t1.Description,
                                              CompanyId = t1.CompanyId,
                                              RequisitionDate = t1.RequisitionDate,
                                              IsSubmitted = t1.IsSubmitted,
                                              RequisitionStatus = t1.RequisitionStatus
                                          }).FirstOrDefaultAsync());

            model.Items = await Task.Run(() => (from t1 in context.RequisitionItems
                                                join t3 in context.Products.Where(x => x.IsActive) on t1.ProductId equals t3.ProductId
                                                where t1.IsActive == true && t1.RequisitionId == model.RequisitionId
                                                select new RequisitionItemModel
                                                {
                                                    RequisitionId = t1.RequisitionId,
                                                    ProductName = t3.ProductName,
                                                    ProductId = t3.ProductId,
                                                    InputQty = t1.InputQty,
                                                    ProcessLoss = t1.ProcessLoss,
                                                    RequisitionItemId = t1.RequisitionItemId,
                                                    Qty = t1.Qty
                                                }).OrderByDescending(x => x.RequisitionId)
                                                .ToListAsync());

            return model;
        }

        //public async Task<RequisitionModel> GetRequisition2( int companyId,int id)
        //{
        //    var model = new RequisitionModel();

        //    string requisitionNo = string.Empty;
        //    if (id <= 0)
        //    {
        //        IQueryable<Requisition> requisitions = context.Requisitions.Where(x => x.CompanyId == companyId);
        //        int count = requisitions.Count();
        //        if (count == 0)
        //        {
        //            return new RequisitionModel()
        //            {
        //                RequisitionNo = "R000001",
        //                CompanyId = companyId
        //            };
        //        }

        //        requisitions = requisitions
        //            .Where(x => x.CompanyId == companyId)
        //            .OrderByDescending(x => x.RequisitionId)
        //            .Take(1);

        //        requisitionNo = requisitions
        //            .ToList()
        //            .FirstOrDefault().RequisitionNo;

        //        requisitionNo = GenerateRequisitionNo(requisitionNo);
        //        return new RequisitionModel()
        //        {
        //            RequisitionNo = requisitionNo,
        //            CompanyId = companyId
        //        };


        //    }


        //        var requisition = context.Requisitions
        //            .Include(x => x.RequisitionItems)
        //            .Where(x => x.RequisitionId == id)
        //            .FirstOrDefault();


        //    model.RequisitionNo = requisition.RequisitionNo;
        //    model.RequisitionDate = requisition.RequisitionDate;
        //    model.Description = requisition.Description;
        //    model.CompanyId = requisition.CompanyId;
        //    model.IsSubmitted = requisition.IsSubmitted;
        //    model.RequisitionId = requisition.RequisitionId;
        //    model.RequisitionStatus = requisition.RequisitionStatus;
        //    foreach (var item in requisition.RequisitionItems)
        //    {
        //        var reqItem = new RequisitionItemModel()
        //        {
        //            ProductName = context.Products.SingleOrDefault(q => q.ProductId == item.ProductId)?.ProductName,
        //            InputQty = item.InputQty,
        //            Qty= item.Qty,
        //            ProcessLoss= item.ProcessLoss

        //        };
        //        model.Items.Add(reqItem);

        //    }

        //    // model = ObjectConverter<Requisition, RequisitionModel>.Convert(requisition);

        //    if (requisition == null)
        //    {
        //        throw new Exception("Data not found");
        //    }

        //   // model = ObjectConverter<Requisition, RequisitionModel>.Convert(requisition);
        //    return model;
        //}

        public List<RequisitionItemModel> GetRequisitionItems(int requisitionId)
        {
            List<RequisitionItemModel> requisitionItems = context.Database.SqlQuery<RequisitionItemModel>("exec sp_Feed_GetRequisitionItems {0}", requisitionId).ToList();
            return requisitionItems;
        }
        private string GenerateRequisitionNo(string lastRequisitionNo)
        {
            string numberPortion = lastRequisitionNo.Substring(1, 6);
            int num = Convert.ToInt32(numberPortion);
            num = ++num;
            return "R" + num.ToString().PadLeft(6, '0');
        }


        public async Task<RequisitionModel> RequisitionList(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            RequisitionModel requisitionModel = new RequisitionModel();
            requisitionModel.CompanyId = companyId;
            requisitionModel.DataList = await Task.Run(() => (from t1 in context.Requisitions
                                                              where t1.CompanyId == companyId
                                                              && t1.RequisitionDate >= fromDate
                                                              && t1.RequisitionDate <= toDate
                                                              select new RequisitionModel
                                                              {
                                                                  CompanyId = t1.CompanyId,
                                                                  RequisitionId = t1.RequisitionId,
                                                                  RequisitionDate = t1.RequisitionDate,
                                                                  RequisitionNo = t1.RequisitionNo,
                                                                  RequisitionBy = t1.RequisitionBy,
                                                                  Description = t1.Description,
                                                                  RequisitionStatus = t1.RequisitionStatus,
                                                                  IsSubmitted = t1.IsSubmitted                                                                   
                                                              }).OrderByDescending(x => x.RequisitionDate).AsEnumerable());

           


            return requisitionModel;
        }
        public async Task<RequisitionModel> RequisitionIssuePendingList(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            RequisitionModel requisitionModel = new RequisitionModel();
            requisitionModel.CompanyId = companyId;
            requisitionModel.DataList = await Task.Run(() => (from t1 in context.Requisitions
                                                              where t1.CompanyId == companyId
                                                              && t1.RequisitionStatus == "D"
                                                              && t1.RequisitionDate >= fromDate
                                                              && t1.RequisitionDate <= toDate
                                                              && t1.IsSubmitted == false
                                                              select new RequisitionModel
                                                              {
                                                                  CompanyId = t1.CompanyId,
                                                                  RequisitionId = t1.RequisitionId,
                                                                  RequisitionDate = t1.RequisitionDate,
                                                                  RequisitionNo = t1.RequisitionNo,
                                                                  RequisitionBy = t1.RequisitionBy,
                                                                  Description = t1.Description,
                                                                  RequisitionStatus = t1.RequisitionStatus,
                                                                  IsSubmitted = t1.IsSubmitted
                                                              }).OrderByDescending(x => x.RequisitionDate).AsEnumerable());
            return requisitionModel;
        }

        public async Task<RequisitionModel> RequisitionDeliveryPendingList(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            RequisitionModel requisitionModel = new RequisitionModel();
            requisitionModel.CompanyId = companyId;
            requisitionModel.DataList = await Task.Run(() => (from t1 in context.Requisitions
                                                              where t1.CompanyId == companyId
                                                              && t1.RequisitionStatus == "N"
                                                              && t1.RequisitionDate >= fromDate
                                                              && t1.RequisitionDate <= toDate
                                                              select new RequisitionModel
                                                              {
                                                                  CompanyId = t1.CompanyId,
                                                                  RequisitionId = t1.RequisitionId,
                                                                  RequisitionDate = t1.RequisitionDate,
                                                                  RequisitionNo = t1.RequisitionNo,
                                                                  RequisitionBy = t1.RequisitionBy,
                                                                  Description = t1.Description,
                                                                  RequisitionStatus = t1.RequisitionStatus

                                                              }).OrderByDescending(x => x.RequisitionDate).AsEnumerable());

            return requisitionModel;
        }

        public RequisitionModel GetRequisitionById(int requisitionId)
        {
            Requisition requisition = context.Requisitions.Where(x => x.RequisitionId == requisitionId).FirstOrDefault();
            return ObjectConverter<Requisition, RequisitionModel>.Convert(requisition);
        }

        /*need to update */
        public List<RequisitionItemDetailModel> GetRequisitionItemDetails(int requisitionId)
        {
            var deliveryDate = context.Requisitions.SingleOrDefault(q => q.RequisitionId == requisitionId)?.DeliveredDate;
            List<RequisitionItem> requisitionItems = context.RequisitionItems.Where(x => x.RequisitionId == requisitionId).ToList();
            foreach (var requisitionItem in requisitionItems)
            {
                context.Database.ExecuteSqlCommand("sp_Feed_GetRM_AccordingtoFM {0}", requisitionItem.RequisitionItemId);
            }

            var data = context.Database.SqlQuery<RequisitionItemDetailModel>("sp_GetRmFormulaWithAvailableQty {0},{1}", requisitionId, deliveryDate).ToList();
            return data;
        }

        public List<RequisitionItemDetailModel> GetRequisitionItemDetails(int requisitionId, DateTime deliveryDate)
        {
            List<RequisitionItem> requisitionItems = context.RequisitionItems.Where(x => x.RequisitionId == requisitionId).ToList();
            foreach (var requisitionItem in requisitionItems)
            {
                context.Database.ExecuteSqlCommand("sp_Feed_GetRM_AccordingtoFM {0}", requisitionItem.RequisitionItemId);
            }

            var data = context.Database.SqlQuery<RequisitionItemDetailModel>("sp_GetRmFormulaWithAvailableQty {0},{1}", requisitionId, deliveryDate).ToList();
            return data;
        }

        public int CreateOrEdit(RequisitionModel model)
        {
            int noOfRowsAffected = 0;
            Requisition requisition = ObjectConverter<RequisitionModel, Requisition>.Convert(model);

            requisition = context.Requisitions.Find(model.RequisitionId);
            if (requisition != null)
            {
                requisition.RequisitionStatus = "D";
                requisition.DeliveredBy = System.Web.HttpContext.Current.User.Identity.Name;
                requisition.DeliveredDate = model.DeliveredDate;
                requisition.DeliveryNo = model.DeliveryNo;

                context.Entry(requisition).State = requisition.RequisitionId == 0 ? EntityState.Added : EntityState.Modified;
                noOfRowsAffected = context.SaveChanges();
            }

            //int result = context.Database.ExecuteSqlCommand("EXEC sp_RequisitionDelivery {0},{1},{2},{3},{4}", model.RequisitionId, model.DeliveredDate, model.DeliveryNo, model.DeliveredBy, model.CompanyId);


            if (noOfRowsAffected > 0)
            {
                List<RequisitionItem> requisitionItems = context.RequisitionItems.Where(x => x.RequisitionId == model.RequisitionId).ToList();
                if (requisitionItems.Any())
                {
                    foreach (var item in requisitionItems)
                    {
                        noOfRowsAffected = context.Database.ExecuteSqlCommand("EXEC spCreateFormulaHistory {0},{1},{2},{3},{4},{5}", item.RequisitionId, item.RequisitionItemId, item.ProductFormulaId, item.ProductId, requisition.DeliveredBy, requisition.CompanyId);
                    }
                }
            }
            return noOfRowsAffected > 0 ? requisition.RequisitionId : 0;


        }

        public string GetFormulaMessage(int requisitionId)
        {
            return context.Database.SqlQuery<string>(@"exec spProductFormulaNotExistForProducts {0}", requisitionId).FirstOrDefault();
        }

        public List<RequisitionItemModel> GetProductionItems(int companyId, int requisitionId, DateTime issueDate)
        {
            return context.Database.SqlQuery<RequisitionItemModel>("exec GetProductionItems {0},{1},{2}", companyId, requisitionId, issueDate).ToList();
        }


        public async Task<long> EditProductionReqisitionDetail(RequisitionModel model)
        {
            long result = -1;
            RequisitionItem models = await context.RequisitionItems
                .SingleOrDefaultAsync(s => s.RequisitionItemId == model.RequisitionItemId);

            models.RequisitionId = model.RequisitionId;
            models.ProductId = model.ProductId;
            models.Qty = model.Qty;
            models.InputQty = model.InputQty;
            models.OutputQty = model.OutputQty;
            models.ActualProcessLoss = model.ActualProcessLoss;
            models.ProcessLoss = model.ProcessLoss;


            context.RequisitionItems.Add(models);

            if (await context.SaveChangesAsync() > 0)
            {
                result = model.RequisitionId;
            }
            return result;
        }
    }
}
