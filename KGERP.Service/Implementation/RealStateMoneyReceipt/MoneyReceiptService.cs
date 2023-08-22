using KGERP.Data.Models;
using KGERP.Service.ServiceModel;
using KGERP.Service.ServiceModel.RealState;
using KGERP.Utility;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace KGERP.Service.Implementation.RealStateMoneyReceipt
{
    public class MoneyReceiptService
    {
        private ERPEntities context;
        // private readonly ERPLogServices _eRPLogServices;
        public MoneyReceiptService(ERPEntities db)
        {
            context = db;
            ///_eRPLogServices = eRPLogServices;
        }
        public async Task<List<SelectModelType>> MoneyReceiptType(int companyId)
        {
            try
            {
                List<SelectModelType> selectModelList = new List<SelectModelType>();
                var y = await context.MoneyReceiptTypes.Where(d => d.CompanyId == companyId).ToListAsync();
                selectModelList = y.Select(o => new SelectModelType()
                {
                    Text = o.Title,
                    Value = o.ReceiptTypeId
                }).ToList();
                return selectModelList;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        public async Task<List<SelectModelType>> BankList()
        {
            try
            {
                List<SelectModelType> selectModelList = new List<SelectModelType>();
                var y = await context.Banks.Where(d => d.IsActive == true).ToListAsync();
                selectModelList = y.Select(o => new SelectModelType()
                {
                    Text = o.Name,
                    Value = o.BankId
                }).ToList();
                return selectModelList;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<List<SelectModelType>> ProjectList(int companyId)
        {
            try
            {
                List<SelectModelType> selectModelList = new List<SelectModelType>();
                var y = await context.ProductCategories.Where(d => d.IsActive && d.CompanyId == companyId).ToListAsync();
                selectModelList = y.Select(o => new SelectModelType()
                {
                    Text = o.Name,
                    Value = o.ProductCategoryId
                }).ToList();
                return selectModelList;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public List<SelectModel> GetPaymentMethodSelectModels()
        {
            List<SelectModel> paymentMethods = Enum.GetValues(typeof(RealStatePaymentMethod)).Cast<RealStatePaymentMethod>().Select(v => new SelectModel { Text = v.ToString(), Value = Convert.ToInt32(v) }).ToList();
            return paymentMethods;
        }
        public async Task<MoneyReceiptViewModel> AddReceipt(MoneyReceiptViewModel model)
        {
            using (var scope = context.Database.BeginTransaction())
            {
                try
                {
                    string agent = "";
                    // string agent2 = "";

                    List<MoneyReceiptDetail> moneyReceiptDetailList = new List<MoneyReceiptDetail>();
                    if (model.IsBookingAmount)
                    {
                        var productBookingInfoes = context.ProductBookingInfoes.FirstOrDefault(f => f.BookingId == model.BookingId);

                        agent = agent + "+" + "Booking Money";
                        // agent2 = agent2+ "+" + "Booking Money";
                        MoneyReceiptDetail against = new MoneyReceiptDetail
                        {
                            Amount = productBookingInfoes.BookingAmt - productBookingInfoes.PaidBookingAmt,
                            CollectionFromId = model.BookingId,
                            IsActive = true,
                            MoneyReceiptId = model.MoneyReceiptId,
                            CreateBy = System.Web.HttpContext.Current.User.Identity.Name,
                            CreateDate = DateTime.Now,
                            CollectionFromName = "Booking Money",
                            Indecator = (int)IndicatorEnum.BookingMoney
                        };
                        moneyReceiptDetailList.Add(against);
                    }
                    if (model.InstallmentId != null)
                    {
                        foreach (var item in model.InstallmentId)
                        {
                            long id = Convert.ToInt64(item);
                            var installmentSchedules = context.BookingInstallmentSchedules.FirstOrDefault(f => f.InstallmentId == id);
                            agent = agent + "+" + installmentSchedules.InstallmentTitle;
                            // agent2 = agent2 + "+" + installmentSchedules.InstallmentTitle.Substring(0, installmentSchedules.InstallmentTitle.IndexOf(" "));
                            MoneyReceiptDetail installment = new MoneyReceiptDetail
                            {
                                Amount = installmentSchedules.Amount - installmentSchedules.PaidAmount,
                                CollectionFromId = Convert.ToInt64(item),
                                IsActive = true,
                                MoneyReceiptId = model.MoneyReceiptId,
                                CreateBy = System.Web.HttpContext.Current.User.Identity.Name,
                                CreateDate = DateTime.Now,
                                CollectionFromName = installmentSchedules.InstallmentTitle,
                                Indecator = (int)IndicatorEnum.Installment
                            };

                            moneyReceiptDetailList.Add(installment);
                        }
                    }
                    if (model.Against != null)
                    {
                        foreach (var item in model.Against)
                        {
                            long id = Convert.ToInt64(item);
                            var vm = context.BookingCostMappings.FirstOrDefault(f => f.CostsMappingId == id);

                            var name = context.BookingCostHeads.SingleOrDefault(f => f.CostId == vm.CostId).CostName;
                            agent = agent + "+" + name;
                            // agent2 = agent2 + "+" + name;
                            MoneyReceiptDetail against = new MoneyReceiptDetail
                            {
                                Amount = vm.Amount - vm.PaidAmount,
                                CollectionFromId = id,
                                IsActive = true,
                                MoneyReceiptId = model.MoneyReceiptId,
                                CreateBy = System.Web.HttpContext.Current.User.Identity.Name,
                                CreateDate = DateTime.Now,
                                CollectionFromName = name,
                                Indecator = (int)IndicatorEnum.CostHead
                            };
                            moneyReceiptDetailList.Add(against);

                        }

                    }

                    var moneyReceiptNo = 0;
                    var count = await context.MoneyReceipts.CountAsync();
                    if (count == 0) { moneyReceiptNo = 1; } else { moneyReceiptNo = count; }
                    var extation = model.CompanyId == (int)CompanyName.GloriousLandsAndDevelopmentsLimited ? "GLDL" : "KPL";
                    string poCid = extation + "-" +
                      DateTime.Now.ToString("yy") +
                      DateTime.Now.ToString("MM") +
                      DateTime.Now.ToString("dd") + "-" +
                      count.ToString().PadLeft(2, '0');

                    MoneyReceipt receipt = new MoneyReceipt();
                    //var groupinfo = context.CustomerGroupInfoes.SingleOrDefault(f => f.GroupName == model.ClientName.Trim());
                    receipt.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
                    receipt.CreatedDate = DateTime.Now;
                    receipt.MoneyReceiptDate = Convert.ToDateTime(model.ReceiptDateString);
                    receipt.ChequeDate = Convert.ToDateTime(model.ChequeDateString);
                    receipt.MoneyReceiptNo = poCid;
                    receipt.CompanyId = model.CompanyId;
                    receipt.ClientName = model.ClientName;
                    receipt.CGId = model.CGId;
                    receipt.ClientId = model.ClientId;
                    receipt.BankName = model.BankName;
                    receipt.Amount = model.Amount;
                    receipt.Particular = model.Particular;
                    receipt.ReceivedBy = model.ReceivedBy;
                    receipt.ReceivedTypeId = model.PayTypeId;
                    receipt.IsActive = true;
                    receipt.IsSubmitted = false;
                    receipt.AccountNo = model.AccountNo;
                    receipt.ChequeNo = model.ChequeNo;
                    receipt.JsonData = "";
                    receipt.MoneyReceiptSLNumber = model.SerialNumber;
                    var stringvalue = agent.Remove(0, 1);
                    if (stringvalue.Count() > 200)
                    {
                        receipt.IsShort = true;
                    }
                    receipt.Against = stringvalue;
                    receipt.ShortAgainst = stringvalue;
                    context.MoneyReceipts.Add(receipt);
                    context.SaveChanges();
                    model.MoneyReceiptId = receipt.MoneyReceiptId;
                    model.MoneyReceiptNo = receipt.MoneyReceiptNo;
                    foreach (var item in moneyReceiptDetailList)
                    {
                        item.MoneyReceiptId = receipt.MoneyReceiptId;
                    }
                    context.MoneyReceiptDetails.AddRange(moneyReceiptDetailList);
                    context.SaveChanges();
                    scope.Commit();
                    return model;
                }
                catch (Exception ex)
                {
                    scope.Rollback();
                    return model;
                }
            }
        }
        public async Task<MoneyReceiptViewModel> MonyeReceiptList(int companyId)
        {
            MoneyReceiptViewModel model = new MoneyReceiptViewModel();
            model.CompanyId = companyId;
            model.MoneyReceiptList = (from t1 in context.MoneyReceipts.Where(d => d.IsActive && d.CompanyId == companyId)
                                      join t2 in context.CustomerGroupInfoes on t1.CGId equals t2.CGId
                                      join t3 in context.ProductBookingInfoes on t1.CGId equals t3.CGId
                                      select new MoneyReceiptViewModel
                                      {
                                          MoneyReceiptId = t1.MoneyReceiptId,
                                          MoneyReceiptNo = t1.MoneyReceiptNo,
                                          ClientName = t2.GroupName,
                                          AccountNo = t1.AccountNo,
                                          ChequeNo = t1.ChequeNo,
                                          Amount = t1.Amount,
                                          MoneyReceiptDate = t1.MoneyReceiptDate,
                                          IsSubmitted = t1.IsSubmitted,
                                          AgainstString = t1.Against,
                                          PayTypeId = t1.ReceivedTypeId,
                                          CompanyId = t1.CompanyId,
                                          SerialNumber = t1.MoneyReceiptSLNumber,
                                          FileNo = t3.FileNo,
                                          Postingdate = t1.CreatedDate,
                                      }).OrderByDescending(x => x.MoneyReceiptId).ToList();
            return model;
        }
        public async Task<MoneyReceiptViewModel> MoneyReceiptIntegratedList(int companyId)
        {
            MoneyReceiptViewModel model = new MoneyReceiptViewModel();
            model.CompanyId = companyId;
            model.MoneyReceiptList = (from t1 in context.MoneyReceipts.Where(d => d.IsActive && d.CompanyId == companyId && d.IsExisting == false)
                                      join t2 in context.CustomerGroupInfoes on t1.CGId equals t2.CGId
                                      join t3 in context.ProductBookingInfoes on t1.CGId equals t3.CGId
                                      select new MoneyReceiptViewModel
                                      {
                                          MoneyReceiptId = t1.MoneyReceiptId,
                                          MoneyReceiptNo = t1.MoneyReceiptNo,
                                          ClientName = t2.GroupName,
                                          AccountNo = t1.AccountNo,
                                          ChequeNo = t1.ChequeNo,
                                          Amount = t1.Amount,
                                          MoneyReceiptDate = t1.MoneyReceiptDate,
                                          IsSubmitted = t1.IsSubmitted,
                                          AgainstString = t1.Against,
                                          PayTypeId = t1.ReceivedTypeId,
                                          CompanyId = t1.CompanyId,
                                          SerialNumber = t1.MoneyReceiptSLNumber,
                                          FileNo = t3.FileNo,
                                          Postingdate = t1.CreatedDate,
                                      }).OrderByDescending(x => x.MoneyReceiptId).ToList();
            return model;
        }
        public async Task<MoneyReceiptViewModel> MoneyReceiptDetails(long id)
        {
            MoneyReceiptViewModel model = new MoneyReceiptViewModel();
            model = (from t1 in context.MoneyReceipts.Where(d => d.IsActive && d.MoneyReceiptId == id)
                     join t2 in context.CustomerGroupInfoes on t1.CGId equals t2.CGId
                     join t3 in context.ProductBookingInfoes on t2.CGId equals t3.CGId

                     join t4 in context.Products on t3.ProductId equals t4.ProductId
                     join t5 in context.ProductSubCategories on t4.ProductSubCategoryId equals t5.ProductSubCategoryId
                     join t6 in context.ProductCategories on t5.ProductCategoryId equals t6.ProductCategoryId
                     where t1.IsActive

                     select new MoneyReceiptViewModel
                     {
                         BankCharge = t1.BankCharge,
                         HeadGLId = t2.HeadGLId,
                         MoneyReceiptId = t1.MoneyReceiptId,
                         MoneyReceiptNo = t1.MoneyReceiptNo,
                         ClientName = t1.ReceivedBy,
                         AccountNo = t1.AccountNo,
                         ChequeNo = t1.ChequeNo,
                         Amount = t1.Amount,
                         MoneyReceiptDate = t1.MoneyReceiptDate,
                         IsSubmitted = t1.IsSubmitted,
                         AgainstString = t1.Against,
                         PayTypeId = t1.ReceivedTypeId,
                         BankName = t1.BankName,
                         PlotName = t4.ProductName,
                         ProjectName = t6.Name,
                         BlockName = t5.Name,
                         BookingNo = t3.BookingNo,
                         ChequeDate = t1.ChequeDate,
                         CGId = t1.CGId,
                         BookingId = t3.BookingId,
                         CompanyId = t1.CompanyId,
                         FileNo = t3.FileNo,
                         SerialNumber = t1.MoneyReceiptSLNumber,
                         Particular = t1.Particular,
                         IsShort = t1.IsShort,
                     }).FirstOrDefault();

            model.MoneyReceiptList = (from t1 in context.MoneyReceiptDetails.Where(x => x.MoneyReceiptId == id)
                                      select new MoneyReceiptViewModel
                                      {
                                          MoneyReceiptId = t1.MoneyReceiptId,
                                          MoneyReceiptDetailId = t1.MoneyReceiptDetailId,
                                          CollectionFrom = t1.CollectionFromName,
                                          Amount = t1.Amount,

                                          PaidAmount = t1.PaidAmount
                                      }).ToList();
            decimal total = 0;
            foreach (var item in model.MoneyReceiptList)
            {
                total = (decimal)(total + item.PaidAmount);
            }
            model.TotalAmount = total;
            return model;


        }
        public async Task<MoneyReceiptViewModel> MonyeReceiptDetailsForAccountingPush(MoneyReceiptViewModel mrviewModel)
        {
            MoneyReceiptViewModel model = new MoneyReceiptViewModel();
            model = (from t1 in context.MoneyReceipts.Where(d => d.IsActive && d.MoneyReceiptId == mrviewModel.MoneyReceiptId)
                     join t2 in context.CustomerGroupInfoes on t1.CGId equals t2.CGId
                     join t3 in context.ProductBookingInfoes on t2.CGId equals t3.CGId
                     join t4 in context.Products on t3.ProductId equals t4.ProductId
                     join t5 in context.ProductSubCategories on t4.ProductSubCategoryId equals t5.ProductSubCategoryId
                     join t6 in context.ProductCategories on t5.ProductCategoryId equals t6.ProductCategoryId
                     where t1.IsActive
                     select new MoneyReceiptViewModel
                     {
                         Accounting_BankOrCashId = mrviewModel.Accounting_BankOrCashId,
                         Accounting_BankOrCashParantId = mrviewModel.Accounting_BankOrCashParantId,
                         FileNo = t3.FileNo,
                         CompanyId = t1.CompanyId,
                         IntegratedFrom = "MoneyReceipt",
                         BankCharge = mrviewModel.BankCharge,
                         HeadGLId = t2.HeadGLId,
                         CGId = t1.CGId,
                         MoneyReceiptId = t1.MoneyReceiptId,
                         MoneyReceiptNo = t1.MoneyReceiptNo,
                         ClientName = t1.ReceivedBy,
                         AccountNo = t1.AccountNo,
                         ChequeNo = t1.ChequeNo,
                         ChequeDate = t1.ChequeDate,
                         BookingNo = t3.BookingNo,
                         Amount = t1.Amount,
                         MoneyReceiptDate = t1.MoneyReceiptDate,
                         IsSubmitted = t1.IsSubmitted,
                         AgainstString = t1.Against,
                         PayTypeId = t1.ReceivedTypeId,
                         BankName = t1.BankName,
                         PlotName = t4.ProductName,
                         ProjectName = t6.Name,
                         BlockName = t5.Name,
                         BookingId = t3.BookingId,
                         SerialNumber = t1.MoneyReceiptSLNumber,
                         VoucherTypeId = (mrviewModel.CompanyId == (int)CompanyName.GloriousLandsAndDevelopmentsLimited) ? 90 : (mrviewModel.CompanyId == (int)CompanyName.KrishibidPropertiesLimited) ? 44 : 0,
                         BankChargeAccHeahId = (mrviewModel.CompanyId == (int)CompanyName.GloriousLandsAndDevelopmentsLimited) ? 50602580 : (mrviewModel.CompanyId == (int)CompanyName.KrishibidPropertiesLimited) ? 43242 : 0
                     }).FirstOrDefault();

            model.MoneyReceiptList = (from t1 in context.MoneyReceiptDetails.Where(x => x.MoneyReceiptId == mrviewModel.MoneyReceiptId)
                                      select new MoneyReceiptViewModel
                                      {
                                          MoneyReceiptId = t1.MoneyReceiptId,
                                          MoneyReceiptDetailId = t1.MoneyReceiptDetailId,
                                          CollectionFrom = t1.CollectionFromName,
                                          Indecator = t1.Indecator,
                                          CollectionFromId = t1.CollectionFromId,
                                          Amount = t1.Amount - t1.PaidAmount,
                                          PaidAmount = t1.PaidAmount
                                      }).ToList();
            decimal total = 0;
            foreach (var item in model.MoneyReceiptList)
            {
                total = (decimal)(total + item.PaidAmount);
            }
            model.TotalAmount = total;
            return model;
        }
        public GetClientGroupInfoViewModel GetCline(long CGId)
        {
            GetClientGroupInfoViewModel model = new GetClientGroupInfoViewModel();
            model = (from t1 in context.CustomerGroupInfoes.Where(d => d.CGId == CGId)
                     join t2 in context.Vendors on t1.PrimaryClientId equals t2.VendorId
                     join t3 in context.ProductBookingInfoes on t1.CGId equals t3.CGId
                     join t4 in context.Products on t3.ProductId equals t4.ProductId into t4_join
                     from t4 in t4_join.DefaultIfEmpty()
                     join t5 in context.ProductSubCategories on t4.ProductSubCategoryId equals t5.ProductSubCategoryId into t5_join
                     from t5 in t5_join.DefaultIfEmpty()
                     join t6 in context.ProductCategories on t5.ProductCategoryId equals t6.ProductCategoryId into t6_join
                     from t6 in t6_join.DefaultIfEmpty()
                     select new GetClientGroupInfoViewModel
                     {
                         CGId = t1.CGId,
                         ClientId = t1.PrimaryClientId,
                         BookingId = t3.BookingId,
                         BookingNo = t3.BookingNo,
                         ClientName = t2.Name,
                         PlotName = t4.ProductName,
                         ProjectName = t6.Name,
                         BlockName = t5.Name,
                         BookingMoney = t3.BookingAmt - t3.PaidBookingAmt
                     }).FirstOrDefault();


            model.Schedule = (from t1 in context.CustomerGroupInfoes.Where(d => d.CGId == CGId)
                              join t2 in context.BookingInstallmentSchedules on t1.CGId equals t2.CGID
                              where !t2.IsPaid && t2.IsActive
                              select new InstallmentScheduleShortModel
                              {
                                  InstallmentId = t2.InstallmentId,
                                  Title = t2.InstallmentTitle + " - " + (t2.Amount - t2.PaidAmount) + "TK",
                                  InstallmentDate = t2.Date
                              }).ToList();

            model.costHead = (from t1 in context.CustomerGroupInfoes.Where(d => d.CGId == CGId)
                              join t2 in context.ProductBookingInfoes on t1.CGId equals t2.CGId
                              join t3 in context.BookingCostMappings.Where(x => x.Amount > 0 && x.Amount != x.PaidAmount && x.IsSnstallmentInclude == false) on t2.BookingId equals t3.BookingId
                              join t4 in context.BookingCostHeads on t3.CostId equals t4.CostId

                              select new SelectModelType
                              {
                                  Value = (int)t3.CostsMappingId,
                                  Text = t4.CostName + " - " + (t3.Amount - t3.PaidAmount) + "TK",
                              }).ToList();
            return model;
        }
        public List<SelectDDLModel> GetGroupByProjectId(int companyId, long projectId)
        {
            List<SelectDDLModel> list = new List<SelectDDLModel>();

            list = (from t1 in context.ProductBookingInfoes
                    join t2 in context.Products on t1.ProductId equals t2.ProductId
                    join t3 in context.ProductSubCategories on t2.ProductSubCategoryId equals t3.ProductSubCategoryId
                    join t4 in context.CustomerGroupInfoes on t1.CGId equals t4.CGId
                    where t3.ProductCategoryId == projectId
                    && t4.CompanyId == companyId
                    && t1.IsActive == true && t4.IsActive == true
                    select new SelectDDLModel
                    {
                        Value = t1.CGId,
                        Text = t4.GroupName + " - " + t1.FileNo + " - " + t4.PrimaryContactNo
                    }).ToList();
            return list;
        }
        public async Task<int> UpdateItem(MoneyReceiptViewModel model)
        {
            MoneyReceiptDetail detail = new MoneyReceiptDetail();
            detail = await context.MoneyReceiptDetails.FirstOrDefaultAsync(f => f.MoneyReceiptDetailId == model.MoneyReceiptDetailId);
            detail.PaidAmount = (decimal)model.PaidAmount;
            detail.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            detail.ModifiedDate = DateTime.Now;
            context.Entry(detail).State = EntityState.Modified;
            if (await context.SaveChangesAsync() > 0)
            {
                return 1;
            }
            return 0;

        }
        public async Task<MoneyReceiptViewModel> PurposeUpdate(MoneyReceiptViewModel model)
        {
            MoneyReceipt detail = new MoneyReceipt();
            detail = await context.MoneyReceipts.FirstOrDefaultAsync(f => f.MoneyReceiptId == model.MoneyReceiptId);
            detail.Against = model.AgainstString;
            if (model.AgainstString.Count() > 200)
            {
                detail.IsShort = true;
            }
            else
            {
                detail.IsShort = false;
            }
            detail.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            detail.ModifiedDate = DateTime.Now;
            context.Entry(detail).State = EntityState.Modified;
            if (await context.SaveChangesAsync() > 0)
            {
                model.Message = "Purpose update successfully";
                return model;
            }
            model.Message = "Purpose update Not successfully";
            return model;

        }
        public async Task<MoneyReceiptViewModel> MoneyReceiptItemDelete(MoneyReceiptViewModel model)
        {
            using (var scope = context.Database.BeginTransaction())
            {
                try
                {
                    var receipt = await context.MoneyReceipts.FirstOrDefaultAsync(f => f.MoneyReceiptId == model.MoneyReceiptId);
                    receipt.IsActive = false;
                    receipt.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                    receipt.ModifiedDate = DateTime.Now;
                    context.Entry(receipt).State = EntityState.Modified;
                    context.SaveChanges();
                    var detail = await context.MoneyReceiptDetails.Where(f => f.MoneyReceiptDetailId == model.MoneyReceiptDetailId).ToListAsync();
                    foreach (var item in detail)
                    {
                        item.IsActive = false;
                        context.Entry(receipt).State = EntityState.Modified;
                        item.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                        item.ModifiedDate = DateTime.Now;
                        context.SaveChanges();
                    }
                    model.MoneyReceiptNo = receipt.MoneyReceiptNo;
                    scope.Commit();
                    return model;
                }
                catch (Exception ex)
                {
                    scope.Rollback();
                    return model;
                }
            }
        }
        public async Task<int> MoneyReceiptStatusUpdate(MoneyReceiptViewModel moneyReceiptViewModel)
        {

            var installments = moneyReceiptViewModel.MoneyReceiptList.Where(x => x.Indecator == (int)IndicatorEnum.Installment).ToList();
            if (installments != null)
            {
                foreach (var item in installments)
                {
                    BookingInstallmentSchedule schedule = context.BookingInstallmentSchedules.FirstOrDefault(h => h.InstallmentId == item.CollectionFromId);
                    if (item.PaidAmount + schedule.PaidAmount == schedule.Amount)
                    {
                        schedule.PaidAmount = schedule.PaidAmount + item.PaidAmount ?? 0;
                        schedule.IsPaid = true;
                        schedule.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                        schedule.ModifiedDate = DateTime.Now;
                    }
                    else
                    {
                        schedule.PaidAmount = schedule.PaidAmount + item.PaidAmount ?? 0;
                        schedule.IsPaid = false;
                        schedule.IsPartlyPaid = true;
                        schedule.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                        schedule.ModifiedDate = DateTime.Now;
                    }

                    context.Entry(schedule).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }
            var costCollection = moneyReceiptViewModel.MoneyReceiptList.Where(x => x.Indecator == (int)IndicatorEnum.CostHead).ToList();
            if (costCollection != null)
            {
                foreach (var item in costCollection)
                {
                    BookingCostMapping bookingCostMapping = context.BookingCostMappings.FirstOrDefault(h => h.CostsMappingId == item.CollectionFromId);
                    if (item.PaidAmount + bookingCostMapping.PaidAmount == bookingCostMapping.Amount)
                    {
                        bookingCostMapping.PaidAmount = bookingCostMapping.PaidAmount + item.PaidAmount ?? 0;
                        bookingCostMapping.IsPaid = true;
                        bookingCostMapping.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                        bookingCostMapping.ModifiedDate = DateTime.Now;
                    }
                    else
                    {
                        bookingCostMapping.PaidAmount = bookingCostMapping.PaidAmount + item.PaidAmount ?? 0;
                        bookingCostMapping.IsPaid = false;
                        bookingCostMapping.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                        bookingCostMapping.ModifiedDate = DateTime.Now;
                    }
                    context.Entry(bookingCostMapping).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }

            var bookingMoney = moneyReceiptViewModel.MoneyReceiptList.FirstOrDefault(x => x.Indecator == (int)IndicatorEnum.BookingMoney);
            if (bookingMoney != null)
            {
                ProductBookingInfo productBookingInfoes = context.ProductBookingInfoes.FirstOrDefault(h => h.BookingId == bookingMoney.CollectionFromId);
                productBookingInfoes.PaidBookingAmt = productBookingInfoes.PaidBookingAmt + bookingMoney.PaidAmount ?? 0;
                context.Entry(productBookingInfoes).State = EntityState.Modified;
                context.SaveChanges();
            }
            MoneyReceipt moneyReceipt = context.MoneyReceipts.Find(moneyReceiptViewModel.MoneyReceiptId);
            moneyReceipt.IsSubmitted = true;
            moneyReceipt.IsExisting = true;
            moneyReceipt.SubmittedDate = moneyReceiptViewModel.Submitdate;
            moneyReceipt.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            moneyReceipt.ModifiedDate = DateTime.Now;
            context.Entry(moneyReceipt).State = EntityState.Modified;
            context.SaveChanges();

            return 0;
        }

    }
}
