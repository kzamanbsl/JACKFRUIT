using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using KGERP.Data.Models;
using KGERP.Service.Implementation.Production;
using KGERP.Service.Implementation.Warehouse;
using KGERP.Service.ServiceModel;
using KGERP.Service.ServiceModel.RealState;
using KGERP.Utility;

namespace KGERP.Service.Implementation.Accounting

{
    public class AccountingService
    {
        private readonly ERPEntities _db;
        string _urlInfo = "";

        public AccountingService(ERPEntities db)
        {
            _db = db;
            _urlInfo = GetErpUrlInfo();
        }

        public string GetErpUrlInfo()
        {
            return _db.UrlInfoes.Where(x => x.UrlType == 1).Select(x => x.Url).FirstOrDefault();
        }

        public async Task<long> AccountingJournalPush(DateTime journalDate, int companyFk, int drHeadId, long? crHeadId, decimal amount, string title, string description, int journalType)
        {
            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,
                Title = title,
                Narration = description,
                CompanyFK = companyFk,
                Date = journalDate,
                IsSubmit = true
            };

            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();
            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = description,
                Debit = Convert.ToDouble(amount),
                Credit = 0,
                Accounting_HeadFK = drHeadId
            });

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = description,
                Debit = 0,
                Credit = Convert.ToDouble(amount),
                Accounting_HeadFK = Convert.ToInt32(crHeadId)
            });

            var resultData = await AccountingJournalMasterPush(vMJournalSlave);
            return resultData.VoucherId;
        }

        public async Task<long> AccountingInventoryPush(DateTime journalDate, int companyFk, int adjustHeadDr, int adjustHeadCr, decimal adjustValue, string title, string description, int journalType)
        {
            long result = -1;
            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,
                Title = title,
                Narration = description,
                CompanyFK = companyFk,
                Date = journalDate,

                IsSubmit = true
            };


            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = description,
                Debit = 0,
                Credit = Convert.ToDouble(adjustValue),
                Accounting_HeadFK = adjustHeadCr
            });

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = description,
                Debit = Convert.ToDouble(adjustValue),
                Credit = 0,
                Accounting_HeadFK = adjustHeadDr
            });

            var resultData = await AccountingJournalMasterPush(vMJournalSlave);
            return resultData.VoucherId;

        }

        public async Task<Voucher> AccountingJournalMasterPush(VMJournalSlave vmJournalSlave)
        {
            Accounting_CostCenter costCenter = new Accounting_CostCenter();
            if (vmJournalSlave.CompanyFK == 7 || vmJournalSlave.CompanyFK == 9)
            {
                if (vmJournalSlave.Accounting_CostCenterFK != null)
                {
                    costCenter.CostCenterId = (int)vmJournalSlave.Accounting_CostCenterFK;
                }
                else
                {
                    costCenter = _db.Accounting_CostCenter.FirstOrDefault(x => x.CompanyId == vmJournalSlave.CompanyFK);
                }
            }
            else
            {
                costCenter = _db.Accounting_CostCenter.FirstOrDefault(x => x.CompanyId == vmJournalSlave.CompanyFK);
            }

            string voucherNo = GetNewVoucherNo(vmJournalSlave.JournalType, vmJournalSlave.CompanyFK.Value, vmJournalSlave.Date.Value);

            Voucher voucher = new Voucher
            {
                VoucherTypeId = vmJournalSlave.JournalType,
                VoucherNo = voucherNo,
                Accounting_CostCenterFk = costCenter.CostCenterId,
                VoucherStatus = "A",
                VoucherDate = vmJournalSlave.Date,
                Narration = vmJournalSlave.Title + " " + vmJournalSlave.Narration,
                ChqDate = vmJournalSlave.ChqDate,
                ChqName = vmJournalSlave.ChqName,
                ChqNo = vmJournalSlave.ChqNo,
                CompanyId = vmJournalSlave.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreateDate = DateTime.Now,
                IsActive = true,
                IsSubmit = vmJournalSlave.IsSubmit,
                IsIntegrated = true
            };

            using (var scope = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.Vouchers.Add(voucher);
                    _db.SaveChanges();

                    List<VoucherDetail> voucherDetailList = new List<VoucherDetail>();
                    voucherDetailList = vmJournalSlave.DataListSlave.Select(x => new VoucherDetail
                    {
                        Particular = x.Particular,
                        DebitAmount = Convert.ToDouble(x.Debit),
                        CreditAmount = Convert.ToDouble(x.Credit),
                        AccountHeadId = x.Accounting_HeadFK,
                        IsActive = true,
                        VoucherId = voucher.VoucherId,
                        TransactionDate = voucher.VoucherDate,
                        IsVirtual = x.IsVirtual
                    }).ToList();

                    _db.VoucherDetails.AddRange(voucherDetailList);
                    _db.SaveChanges();

                    scope.Commit();

                    return voucher;
                }
                catch (Exception ex)
                {
                    scope.Rollback();
                    return voucher;
                }
            }
        }

        public string GetNewVoucherNo(int voucherTypeId, int companyId, DateTime voucherDate)
        {
            VoucherType voucherType = _db.VoucherTypes.FirstOrDefault(x => x.VoucherTypeId == voucherTypeId);
            string voucherNo = string.Empty;
            int vouchersCount = _db.Vouchers.Count(x => x.VoucherTypeId == voucherTypeId && x.CompanyId == companyId
                && x.VoucherDate.Value.Month == voucherDate.Month
                && x.VoucherDate.Value.Year == voucherDate.Year);

            vouchersCount++;
            voucherNo = voucherType?.Code + "-" + vouchersCount.ToString().PadLeft(6, '0');

            //if (vouchersCount  == 0)
            //{               
            //    voucherNo = voucherType.Code + "-" + "000001";                
            //}
            //else
            //{

            //    //Voucher voucher = context.Vouchers.Where(x => x.VoucherTypeId == voucherTypeId && x.CompanyId == companyId).OrderByDescending(x => x.VoucherNo).FirstOrDefault();
            //    //voucherNo = GenerateVoucherNo(voucher.VoucherNo);
            //}           
            return voucherNo;
        }

        public string GetVoucherNo(int voucherTypeId, int companyId)
        {
            string voucherNo = string.Empty;
            var vouchers = _db.Vouchers.Where(x => x.VoucherTypeId == voucherTypeId && x.CompanyId == companyId);

            if (!vouchers.Any())
            {
                VoucherType voucherType = _db.VoucherTypes.FirstOrDefault(x => x.VoucherTypeId == voucherTypeId);
                voucherNo = voucherType.Code + "-" + "000001";
                return voucherNo;
            }

            Voucher voucher = _db.Vouchers.Where(x => x.VoucherTypeId == voucherTypeId && x.CompanyId == companyId).OrderByDescending(x => x.VoucherNo).FirstOrDefault();
            voucherNo = GenerateVoucherNo(voucher.VoucherNo);
            return voucherNo;

        }

        private string GenerateVoucherNo(string lastVoucherNo)
        {
            string prefix = lastVoucherNo.Substring(0, 4);
            int code = Convert.ToInt32(lastVoucherNo.Substring(4, 6));
            int newCode = code + 1;
            return prefix + newCode.ToString().PadLeft(6, '0');
        }

        public async Task<VMJournalSlave> GetCompaniesDetails(int companyId)
        {
            VMJournalSlave vmJournalSlave = new VMJournalSlave();
            vmJournalSlave = await Task.Run(() => (from t1 in _db.Companies.Where(x => x.IsActive && x.CompanyId == companyId)

                                                   select new VMJournalSlave
                                                   {
                                                       CompanyFK = t1.CompanyId,
                                                       CompanyName = t1.Name
                                                   }).FirstOrDefault());


            return vmJournalSlave;
        }

        public async Task<VMJournalSlave> GetVoucherDetails(int companyId, int voucherId)
        {
            VMJournalSlave vmJournalSlave = new VMJournalSlave();
            vmJournalSlave = await Task.Run(() => (from t1 in _db.Vouchers.Where(x => x.IsActive && x.VoucherId == voucherId && x.CompanyId == companyId)
                                                   join t4 in _db.VoucherTypes on t1.VoucherTypeId equals t4.VoucherTypeId
                                                   join t2 in _db.Companies on t1.CompanyId equals t2.CompanyId
                                                   join t3 in _db.Accounting_CostCenter on t1.Accounting_CostCenterFk equals t3.CostCenterId
                                                   //  join t5 in _db.HeadGLs on t1.VirtualHeadId equals t5.Id

                                                   select new VMJournalSlave
                                                   {
                                                       VoucherId = t1.VoucherId,
                                                       Accounting_CostCenterName = t3.Name,
                                                       VoucherNo = t1.VoucherNo,
                                                       Date = t1.VoucherDate,
                                                       Narration = t1.Narration,
                                                       CompanyFK = t1.CompanyId,
                                                       Status = t1.VoucherStatus,
                                                       ChqDate = t1.ChqDate,
                                                       ChqName = t1.ChqName,
                                                       ChqNo = t1.ChqNo,
                                                       Accounting_CostCenterFK = t1.Accounting_CostCenterFk,
                                                       Accounting_BankOrCashId = t1.VirtualHeadId,
                                                       //BankOrCashNane = "[" + t5.AccCode + "] " + t5.AccName,
                                                       CompanyName = t2.Name,
                                                       IsSubmit = t1.IsSubmit
                                                   }).FirstOrDefault());

            vmJournalSlave.DataListDetails = await Task.Run(() => (from t1 in _db.VoucherDetails.Where(x => x.IsActive && x.VoucherId == voucherId && !x.IsVirtual)
                                                                   join t2 in _db.HeadGLs on t1.AccountHeadId equals t2.Id
                                                                   select new VMJournalSlave
                                                                   {
                                                                       VoucherDetailId = t1.VoucherDetailId,
                                                                       AccountingHeadName = t2.AccName,
                                                                       Code = t2.AccCode,
                                                                       Credit = t1.CreditAmount,
                                                                       Debit = t1.DebitAmount,
                                                                       Particular = t1.Particular
                                                                   }).OrderByDescending(x => x.VoucherDetailId).AsEnumerable());
            if ((vmJournalSlave.DataListDetails?.Count() ?? 0) > 0)
            {
                vmJournalSlave.Particular = vmJournalSlave.DataListDetails.OrderByDescending(x => x.VoucherDetailId).Select(x => x.Particular).FirstOrDefault();
            }
            return vmJournalSlave;
        }

        public async Task<VMJournalSlave> GetStockVoucherDetails(int companyId, int voucherId)
        {
            VMJournalSlave vmJournalSlave = new VMJournalSlave();
            vmJournalSlave = await Task.Run(() => (from t1 in _db.Vouchers.Where(x => x.IsActive && x.VoucherId == voucherId && x.CompanyId == companyId)
                                                   join t4 in _db.VoucherTypes on t1.VoucherTypeId equals t4.VoucherTypeId
                                                   join t2 in _db.Companies on t1.CompanyId equals t2.CompanyId
                                                   join t3 in _db.Accounting_CostCenter on t1.Accounting_CostCenterFk equals t3.CostCenterId

                                                   select new VMJournalSlave
                                                   {
                                                       VoucherId = t1.VoucherId,
                                                       Accounting_CostCenterName = t3.Name,
                                                       VoucherNo = t1.VoucherNo,
                                                       Date = t1.VoucherDate,
                                                       Narration = t1.Narration,
                                                       CompanyFK = t1.CompanyId,
                                                       Status = t1.VoucherStatus,
                                                       ChqDate = t1.ChqDate,
                                                       ChqName = t1.ChqName,
                                                       ChqNo = t1.ChqNo,
                                                       Accounting_CostCenterFK = t1.Accounting_CostCenterFk,
                                                       Accounting_BankOrCashId = t1.VirtualHeadId,
                                                       IsSubmit = t1.IsSubmit

                                                   }).FirstOrDefault());

            vmJournalSlave.DataListDetails = await Task.Run(() => (from t1 in _db.VoucherDetails.Where(x => x.IsActive && x.VoucherId == voucherId && !x.IsVirtual)
                                                                   join t2 in _db.HeadGLs on t1.AccountHeadId equals t2.Id
                                                                   select new VMJournalSlave
                                                                   {
                                                                       VoucherDetailId = t1.VoucherDetailId,
                                                                       AccountingHeadName = t2.AccName,
                                                                       Code = t2.AccCode,
                                                                       Credit = t1.CreditAmount,
                                                                       Debit = t1.DebitAmount,
                                                                       Particular = t1.Particular
                                                                   }).OrderByDescending(x => x.VoucherDetailId).AsEnumerable());
            return vmJournalSlave;
        }

        public async Task<long> VoucherAdd(VMJournalSlave vmJournalSlave)
        {
            long result = -1;
            //GetVoucherNo

            Voucher voucher = new Voucher
            {
                Narration = vmJournalSlave.Narration,
                VoucherNo = vmJournalSlave.VoucherNo,
                VoucherStatus = vmJournalSlave.Status,
                VoucherTypeId = vmJournalSlave.VoucherTypeId,
                ChqDate = vmJournalSlave.ChqDate,
                VirtualHeadId = vmJournalSlave.Accounting_BankOrCashId,
                ChqNo = vmJournalSlave.ChqNo,

                Accounting_CostCenterFk = vmJournalSlave.Accounting_CostCenterFK,
                ChqName = vmJournalSlave.ChqName,
                VoucherDate = vmJournalSlave.Date,
                CompanyId = vmJournalSlave.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreateDate = DateTime.Now,
                IsActive = true,
                IsStock = vmJournalSlave.IsStock
            };
            _db.Vouchers.Add(voucher);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = voucher.VoucherId;
            }
            return result;
        }

        public async Task<long> VoucherDetailAdd(VMJournalSlave vmJournalSlave)
        {
            long result = -1;
            if ((vmJournalSlave.Accounting_HeadFK > 0) && (vmJournalSlave.Debit > 0 || vmJournalSlave.Credit > 0))
            {
                VoucherDetail voucherDetail = new VoucherDetail
                {
                    AccountHeadId = vmJournalSlave.Accounting_HeadFK,
                    CreditAmount = vmJournalSlave.Credit,
                    DebitAmount = vmJournalSlave.Debit,
                    Particular = vmJournalSlave.Particular,
                    TransactionDate = DateTime.Now,
                    VoucherId = vmJournalSlave.VoucherId,

                    IsActive = true
                };
                _db.VoucherDetails.Add(voucherDetail);
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = voucherDetail.VoucherDetailId;
                }
            }


            return result;
        }

        public async Task<long> VoucherDetailsEdit(VMJournalSlave vmJournalSlave)
        {
            long result = -1;
            VoucherDetail model = await _db.VoucherDetails.FindAsync(vmJournalSlave.VoucherDetailId);

            model.AccountHeadId = vmJournalSlave.Accounting_HeadFK;
            model.CreditAmount = vmJournalSlave.Credit;
            model.DebitAmount = vmJournalSlave.Debit;
            model.Particular = vmJournalSlave.Particular;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = model.VoucherDetailId;
            }

            return result;
        }

        public async Task<long> VoucherDelete(VoucherModel voucherModel)
        {
            long result = -1;
            Voucher model = await _db.Vouchers.FindAsync(voucherModel.VoucherId);

            model.IsActive = false;

            List<VoucherDetail> voucherDetailList = _db.VoucherDetails.Where(x => x.VoucherId == voucherModel.VoucherId).ToList();
            voucherDetailList.ForEach(x => x.IsActive = false);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = model.VoucherId;
            }

            return result;
        }

        public async Task<long> VoucherUndoSubmit(VoucherModel voucherModel)
        {
            long result = -1;
            Voucher model = await _db.Vouchers.FindAsync(voucherModel.VoucherId);
            model.IsSubmit = false;
            model.VoucherStatus = null;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = model.VoucherId;
            }
            return result;
        }

        public async Task<long> VoucherDetailsDelete(long voucherDetailId)
        {
            long result = -1;
            VoucherDetail model = await _db.VoucherDetails.FindAsync(voucherDetailId);

            model.IsActive = false;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = model.VoucherDetailId;
            }

            return result;
        }

        public List<object> CostCenterDropDownList(int companyId)
        {
            var list = new List<object>();
            _db.Accounting_CostCenter
        .Where(x => x.IsActive && x.CompanyId == companyId).Select(x => x).ToList()
        .ForEach(x => list.Add(new
        {
            Value = x.CostCenterId,
            Text = x.Name
        }));
            return list;

        }

        public List<object> VoucherTypesCashAndBankDropDownList()
        {
            var list = new List<object>();
            _db.VoucherTypes.Where(x => x.IsActive && x.VoucherTypeId <= (int)JournalEnum.CashReceive).Select(x => x).ToList()
        .ForEach(x => list.Add(new
        {
            Value = x.VoucherTypeId,
            Text = x.Name
        }));
            return list;

        }

        public List<object> VoucherTypesDownList(int companyId)
        {
            var list = new List<object>();
            _db.VoucherTypes
        .Where(x => x.IsActive && x.CompanyId == companyId).Select(x => x).ToList()
        .ForEach(x => list.Add(new
        {
            Value = x.VoucherTypeId,
            Text = x.Name
        }));
            return list;

        }

        public List<object> VoucherTypesJournalVoucherDropDownList()
        {
            var list = new List<object>();
            _db.VoucherTypes
        .Where(x => x.IsActive && x.VoucherTypeId == (int)JournalEnum.JournalVoucher).Select(x => x).ToList()
        .ForEach(x => list.Add(new
        {
            Value = x.VoucherTypeId,
            Text = x.Name
        }));
            return list;

        }

        public List<object> SeedCashAndBankDropDownList(int companyId)
        {
            var list = new List<object>();
            var v = (from t1 in _db.Head5
                     join t2 in _db.Head4 on t1.ParentId equals t2.Id
                     where t2.AccCode == "1301001" && t1.CompanyId == companyId
                     select new
                     {
                         Value = t1.Id,
                         Text = t1.AccName
                     }).ToList();

            foreach (var item in v)
            {
                list.Add(new { Text = item.Text, Value = item.Value });
            }

            return list;

        }

        public List<object> GCCLLCFactoryExpanceHeadGLList(int companyId)
        {
            var list = new List<object>();
            var v = (from t1 in _db.HeadGLs
                     join t2 in _db.Head5 on t1.ParentId equals t2.Id
                     join t3 in _db.Head4 on t2.ParentId equals t3.Id
                     join t4 in _db.Head3 on t3.ParentId equals t4.Id

                     where (t4.AccCode == "4103" || t4.AccCode == "4104" || t4.AccCode == "4105"
                     || t4.AccCode == "4106" || t2.AccCode == "1301002002" || t3.AccCode == "2401013")
                     && t1.CompanyId == companyId
                     select new
                     {
                         Value = t1.Id,
                         Text = t4.AccCode + " -" + t4.AccName + " " + t1.AccCode + " -" + t1.AccName
                     }).ToList();
            foreach (var item in v)
            {
                list.Add(new { Text = item.Text, Value = item.Value });
            }
            return list;

        }

        public List<object> ExpanceHeadGLList(int companyId)
        {
            var list = new List<object>();
            var v = (from t1 in _db.HeadGLs
                     join t2 in _db.Head5 on t1.ParentId equals t2.Id
                     join t3 in _db.Head4 on t2.ParentId equals t3.Id
                     join t4 in _db.Head3 on t3.ParentId equals t4.Id

                     where (t4.AccCode == "4501" || t3.AccCode == "4501001" || t2.AccCode == "4501001001")
                     && t1.CompanyId == companyId
                     select new
                     {
                         Value = t1.Id,
                         Text = t4.AccCode + " -" + t4.AccName + " " + t1.AccCode + " -" + t1.AccName
                     }).ToList();
            foreach (var item in v)
            {
                list.Add(new { Text = item.Text, Value = item.Value });
            }
            return list;

        }

        public List<object> GCCLOtherIncomeHeadGLList(int companyId)
        {
            var list = new List<object>();
            var v = (from t1 in _db.HeadGLs
                     join t2 in _db.Head5 on t1.ParentId equals t2.Id

                     where t2.AccCode == "3101002001"
                     && t1.CompanyId == companyId
                     select new
                     {
                         Value = t1.Id,
                         Text = t2.AccCode + " -" + t2.AccName + " " + t1.AccCode + " -" + t1.AccName
                     }).ToList();
            foreach (var item in v)
            {
                list.Add(new { Text = item.Text, Value = item.Value });
            }
            return list;

        }

        public List<object> OtherIncomeHeadGLList(int companyId)
        {
            var list = new List<object>();
            var v = (from t1 in _db.HeadGLs
                     join t2 in _db.Head5 on t1.ParentId equals t2.Id

                     where t2.AccCode == "3201001001"
                     && t1.CompanyId == companyId
                     select new
                     {
                         Value = t1.Id,
                         Text = t2.AccCode + " -" + t2.AccName + " " + t1.AccCode + " -" + t1.AccName
                     }).ToList();
            foreach (var item in v)
            {
                list.Add(new { Text = item.Text, Value = item.Value });
            }
            return list;

        }

        public List<object> GCCLCashAndBankDropDownList(int companyId)
        {
            var list = new List<object>();
            var v = (from t1 in _db.Head5
                     join t2 in _db.Head4 on t1.ParentId equals t2.Id
                     where (t2.AccCode == "1301001" || t1.AccCode == "1301002002") && t1.CompanyId == companyId
                     select new
                     {
                         Value = t1.Id,
                         Text = t1.AccCode + " -" + t1.AccName
                     }).ToList();

            foreach (var item in v)
            {
                list.Add(new { Text = item.Text, Value = item.Value });
            }

            return list;

        }
        public List<object> CashAndBankDropDownList(int companyId)
        {
            var list = new List<object>();
            var v = (from t1 in _db.Head5
                     join t2 in _db.Head4 on t1.ParentId equals t2.Id
                     where (t2.AccCode == "1301001") && t1.CompanyId == companyId
                     select new
                     {
                         Value = t1.Id,
                         Text = t1.AccCode + " -" + t1.AccName
                     }).ToList();

            foreach (var item in v)
            {
                list.Add(new { Text = item.Text, Value = item.Value });
            }
            return list;
        }

        public Company GetCompanyById(int companyId)
        {
            var company = _db.Companies.FirstOrDefault(x => x.CompanyId == companyId);
            return company;
        }

        public List<object> KPLCashAndBankDropDownList(int companyId)
        {
            var list = new List<object>();
            var v = (from t1 in _db.Head5
                     join t2 in _db.Head4 on t1.ParentId equals t2.Id
                     where (t2.AccCode == "1301001") && t1.CompanyId == companyId
                     select new
                     {
                         Value = t1.Id,
                         Text = t1.AccCode + " -" + t1.AccName
                     }).ToList();

            foreach (var item in v)
            {
                list.Add(new { Text = item.Text, Value = item.Value });
            }

            return list;

        }

        public List<object> StockDropDownList(int companyId)
        {
            var list = new List<object>();

            if (companyId == (int)CompanyName.NaturalFishFarmingLimited)
            {
                var v = (from t1 in _db.Head5
                         where t1.AccCode == "1301004001" && t1.CompanyId == companyId
                         select new
                         {
                             Value = t1.Id,
                             Text = t1.AccCode + " -" + t1.AccName
                         }).ToList();

                foreach (var item in v)
                {
                    list.Add(new { Text = item.Text, Value = item.Value });
                }
            }
            if (companyId == (int)CompanyName.KrishibidBazaarLimited)
            {
                var v = (from t1 in _db.Head5
                         where t1.AccCode == "1301005001" && t1.CompanyId == companyId
                         select new
                         {
                             Value = t1.Id,
                             Text = t1.AccCode + " -" + t1.AccName
                         }).ToList();

                foreach (var item in v)
                {
                    list.Add(new { Text = item.Text, Value = item.Value });
                }
            }
            if (companyId == (int)CompanyName.OrganicPoultryLimited || companyId == (int)CompanyName.SonaliOrganicDairyLimited)
            {
                var v = (from t1 in _db.Head5
                         join t2 in _db.Head4 on t1.ParentId equals t2.Id
                         where t2.AccCode == "1301004" && t1.CompanyId == companyId
                         select new
                         {
                             Value = t1.Id,
                             Text = t1.AccCode + " -" + t1.AccName
                         }).ToList();

                foreach (var item in v)
                {
                    list.Add(new { Text = item.Text, Value = item.Value });
                }
            }
            if (companyId == (int)CompanyName.KrishibidPrintingAndPublicationLimited)
            {
                var v = (from t1 in _db.Head5
                         join t2 in _db.Head4 on t1.ParentId equals t2.Id
                         where t2.AccCode == "1305001" && t1.CompanyId == companyId
                         select new
                         {
                             Value = t1.Id,
                             Text = t1.AccCode + " -" + t1.AccName
                         }).ToList();

                foreach (var item in v)
                {
                    list.Add(new { Text = item.Text, Value = item.Value });
                }
            }

            if (companyId == (int)CompanyName.KrishibidFoodAndBeverageLimited)
            {
                var v = (from t1 in _db.Head5
                         join t2 in _db.Head4 on t1.ParentId equals t2.Id
                         join t3 in _db.Head3 on t2.ParentId equals t3.Id

                         where t3.AccCode == "1305" && t1.CompanyId == companyId
                         select new
                         {
                             Value = t1.Id,
                             Text = t1.AccCode + " -" + t1.AccName
                         }).ToList();

                foreach (var item in v)
                {
                    list.Add(new { Text = item.Text, Value = item.Value });
                }
            }
            if (companyId == (int)CompanyName.KrishibidPackagingLimited)
            {
                var v = (
                    from t1 in _db.Head5
                    join t2 in _db.Head4 on t1.ParentId equals t2.Id
                    where t2.AccCode == "1301005" && t1.CompanyId == companyId
                    select new
                    {
                        Value = t1.Id,
                        Text = t1.AccCode + " -" + t1.AccName
                    }).ToList();

                foreach (var item in v)
                {
                    list.Add(new { Text = item.Text, Value = item.Value });
                }
            }
            if (companyId == (int)CompanyName.KrishibidFisheriesLimited)
            {
                var v = (
                    from t1 in _db.Head5
                    where t1.AccCode == "1301005001" && t1.CompanyId == companyId
                    select new
                    {
                        Value = t1.Id,
                        Text = t1.AccCode + " -" + t1.AccName
                    }).ToList();

                foreach (var item in v)
                {
                    list.Add(new { Text = item.Text, Value = item.Value });
                }
            }
            if (companyId == (int)CompanyName.KrishibidPoultryLimited)
            {
                var v = (
                    from t1 in _db.Head5
                    where t1.AccCode == "1301005001" && t1.CompanyId == companyId
                    select new
                    {
                        Value = t1.Id,
                        Text = t1.AccCode + " -" + t1.AccName
                    }).ToList();

                foreach (var item in v)
                {
                    list.Add(new { Text = item.Text, Value = item.Value });
                }
            }
            if (companyId == (int)CompanyName.KrishibidTradingLimited)
            {
                var v = (
                    from t1 in _db.Head5
                    where t1.AccCode == "1305001001" && t1.CompanyId == companyId
                    select new
                    {
                        Value = t1.Id,
                        Text = t1.AccCode + " -" + t1.AccName
                    }).ToList();

                foreach (var item in v)
                {
                    list.Add(new { Text = item.Text, Value = item.Value });
                }
            }
            if (companyId == (int)CompanyName.KrishibidSafeFood)
            {
                var v = (
                    from t1 in _db.Head5

                    where t1.ParentId == 50611881 && t1.CompanyId == companyId
                    select new
                    {
                        Value = t1.Id,
                        Text = t1.AccCode + " -" + t1.AccName
                    }).ToList();

                foreach (var item in v)
                {
                    list.Add(new { Text = item.Text, Value = item.Value });
                }
            }

            return list;
        }

        public async Task<long> FishariseAutoInsertStockVoucherDetails(int companyId, int voucherId)
        {
            long result = -1;

            var voucher = await _db.Vouchers.FindAsync(voucherId);

            var fromDate = voucher.VoucherDate.Value.AddDays(0 - voucher.VoucherDate.Value.Day).AddDays(-10);
            var toDate = voucher.VoucherDate.Value.AddDays(0 - voucher.VoucherDate.Value.Day);
            var previousStock = (from t1 in _db.VoucherDetails
                                 join t2 in _db.Vouchers on t1.VoucherId equals t2.VoucherId
                                 join headGL in _db.HeadGLs on t1.AccountHeadId equals headGL.Id
                                 join head5 in _db.Head5 on headGL.ParentId equals head5.Id
                                 join head4 in _db.Head4 on head5.ParentId equals head4.Id
                                 where t2.CompanyId == companyId && head5.AccCode == "1301005001"
                                 && t1.IsActive && t2.IsActive && !t1.IsVirtual &&
                                 t2.Accounting_CostCenterFk == voucher.Accounting_CostCenterFk &&
                                     t2.VoucherDate >= fromDate
                                     && t2.VoucherDate <= toDate

                                 select new
                                 {
                                     AccountHeadId = t1.AccountHeadId,
                                     DebitAmount = t1.DebitAmount
                                 }).ToList();
            var fromExistDate = voucher.VoucherDate.Value.AddDays(-10);
            var currentExistStock = (from t1 in _db.VoucherDetails
                                     join t2 in _db.Vouchers on t1.VoucherId equals t2.VoucherId
                                     join headGL in _db.HeadGLs on t1.AccountHeadId equals headGL.Id
                                     join head5 in _db.Head5 on headGL.ParentId equals head5.Id
                                     join head4 in _db.Head4 on head5.ParentId equals head4.Id
                                     where t2.CompanyId == companyId && head5.AccCode == "1301005001"
                                     && t1.IsActive && t2.IsActive && !t1.IsVirtual &&

    t2.VoucherDate >= fromExistDate

                                                  && t2.VoucherDate <= voucher.VoucherDate.Value
                                                  && t1.VoucherId != voucher.VoucherId

                                     select new
                                     {
                                         AccountHeadId = t1.AccountHeadId,
                                         DebitAmount = t1.DebitAmount
                                     }).AsEnumerable();

            var currentStock = (from t1 in _db.VoucherDetails
                                where t1.VoucherId == voucher.VoucherId && t1.IsActive
                                select new
                                {
                                    DebitAmount = t1.DebitAmount,
                                    AccountHeadId = t1.AccountHeadId

                                }).ToList();

            List<VoucherDetail> voucherDetailList = new List<VoucherDetail>();
            if (!currentExistStock.Any())
            {
                foreach (var item in previousStock.Where(x => x.DebitAmount > 0))
                {
                    VoucherDetail voucherDetail = new VoucherDetail
                    {
                        VoucherId = voucher.VoucherId,

                        AccountHeadId = item.AccountHeadId,
                        CreditAmount = item.DebitAmount,
                        DebitAmount = 0,
                        TransactionDate = DateTime.Now,
                        IsActive = true,
                        IsVirtual = true


                    };
                    voucherDetailList.Add(voucherDetail);
                    VoucherDetail voucherDetail1 = new VoucherDetail
                    {
                        VoucherId = voucher.VoucherId,

                        AccountHeadId = 50611909,
                        CreditAmount = 0,
                        DebitAmount = item.DebitAmount,
                        TransactionDate = DateTime.Now,
                        IsActive = true,
                        IsVirtual = true


                    };
                    voucherDetailList.Add(voucherDetail1);

                }
            }

            foreach (var item in currentStock)
            {


                VoucherDetail voucherDetail1 = new VoucherDetail
                {
                    VoucherId = voucher.VoucherId,

                    AccountHeadId = 50611909,
                    CreditAmount = item.DebitAmount,
                    DebitAmount = 0,
                    TransactionDate = DateTime.Now,
                    IsActive = true
                };
                voucherDetailList.Add(voucherDetail1);

            }

            _db.VoucherDetails.AddRange(voucherDetailList);

            if (await _db.SaveChangesAsync() > 0)
            {
                result = voucher.VoucherId;
            }

            return result;

        }

        public async Task<long> PoultryAutoInsertStockVoucherDetails(int companyId, int voucherId)
        {
            long result = -1;

            var voucher = await _db.Vouchers.FindAsync(voucherId);

            var fromDate = voucher.VoucherDate.Value.AddDays(0 - voucher.VoucherDate.Value.Day).AddDays(-10);
            var toDate = voucher.VoucherDate.Value.AddDays(0 - voucher.VoucherDate.Value.Day);
            var previousStock = (from t1 in _db.VoucherDetails
                                 join t2 in _db.Vouchers on t1.VoucherId equals t2.VoucherId
                                 join headGL in _db.HeadGLs on t1.AccountHeadId equals headGL.Id
                                 join head5 in _db.Head5 on headGL.ParentId equals head5.Id
                                 join head4 in _db.Head4 on head5.ParentId equals head4.Id
                                 where t2.CompanyId == companyId && head5.AccCode == "1301005001"
                                 && t1.IsActive && t2.IsActive && !t1.IsVirtual &&
                                 t2.Accounting_CostCenterFk == voucher.Accounting_CostCenterFk &&

                                     t2.VoucherDate >= fromDate
                                     && t2.VoucherDate <= toDate

                                 select new
                                 {
                                     AccountHeadId = t1.AccountHeadId,
                                     DebitAmount = t1.DebitAmount
                                 }).ToList();
            var fromExistDate = voucher.VoucherDate.Value.AddDays(-10);
            var currentExistStock = (from t1 in _db.VoucherDetails
                                     join t2 in _db.Vouchers on t1.VoucherId equals t2.VoucherId
                                     join headGL in _db.HeadGLs on t1.AccountHeadId equals headGL.Id
                                     join head5 in _db.Head5 on headGL.ParentId equals head5.Id
                                     join head4 in _db.Head4 on head5.ParentId equals head4.Id
                                     where t2.CompanyId == companyId && head5.AccCode == "1301005001"
                                     && t1.IsActive && t2.IsActive && !t1.IsVirtual &&

    t2.VoucherDate >= fromExistDate

                                                  && t2.VoucherDate <= voucher.VoucherDate.Value
                                                  && t1.VoucherId != voucher.VoucherId

                                     select new
                                     {
                                         AccountHeadId = t1.AccountHeadId,
                                         DebitAmount = t1.DebitAmount
                                     }).AsEnumerable();

            var currentStock = (from t1 in _db.VoucherDetails
                                where t1.VoucherId == voucher.VoucherId && t1.IsActive
                                select new
                                {
                                    DebitAmount = t1.DebitAmount,
                                    AccountHeadId = t1.AccountHeadId

                                }).ToList();

            List<VoucherDetail> voucherDetailList = new List<VoucherDetail>();
            if (!currentExistStock.Any())
            {
                foreach (var item in previousStock.Where(x => x.DebitAmount > 0))
                {
                    VoucherDetail voucherDetail = new VoucherDetail
                    {
                        VoucherId = voucher.VoucherId,

                        AccountHeadId = item.AccountHeadId,
                        CreditAmount = item.DebitAmount,
                        DebitAmount = 0,
                        TransactionDate = DateTime.Now,
                        IsActive = true,
                        IsVirtual = true


                    };
                    voucherDetailList.Add(voucherDetail);
                    VoucherDetail voucherDetail1 = new VoucherDetail
                    {
                        VoucherId = voucher.VoucherId,

                        AccountHeadId = 50611914,
                        CreditAmount = 0,
                        DebitAmount = item.DebitAmount,
                        TransactionDate = DateTime.Now,
                        IsActive = true,
                        IsVirtual = true


                    };
                    voucherDetailList.Add(voucherDetail1);

                }
            }

            foreach (var item in currentStock)
            {


                VoucherDetail voucherDetail1 = new VoucherDetail
                {
                    VoucherId = voucher.VoucherId,

                    AccountHeadId = 50611914,
                    CreditAmount = item.DebitAmount,
                    DebitAmount = 0,
                    TransactionDate = DateTime.Now,
                    IsActive = true
                };
                voucherDetailList.Add(voucherDetail1);

            }

            _db.VoucherDetails.AddRange(voucherDetailList);

            if (await _db.SaveChangesAsync() > 0)
            {
                result = voucher.VoucherId;
            }

            return result;

        }

        public async Task<long> TradingAutoInsertStockVoucherDetails(int companyId, int voucherId)
        {
            long result = -1;

            var voucher = await _db.Vouchers.FindAsync(voucherId);

            var fromDate = voucher.VoucherDate.Value.AddDays(0 - voucher.VoucherDate.Value.Day).AddDays(-10);
            var toDate = voucher.VoucherDate.Value.AddDays(0 - voucher.VoucherDate.Value.Day);
            var previousStock = (from t1 in _db.VoucherDetails
                                 join t2 in _db.Vouchers on t1.VoucherId equals t2.VoucherId
                                 join headGL in _db.HeadGLs on t1.AccountHeadId equals headGL.Id
                                 join head5 in _db.Head5 on headGL.ParentId equals head5.Id
                                 join head4 in _db.Head4 on head5.ParentId equals head4.Id
                                 where t2.CompanyId == companyId && head5.AccCode == "1301005001"
                                 && t1.IsActive && t2.IsActive && !t1.IsVirtual &&
                                 t2.Accounting_CostCenterFk == voucher.Accounting_CostCenterFk &&
                                     t2.VoucherDate >= fromDate
                                     && t2.VoucherDate <= toDate

                                 select new
                                 {
                                     AccountHeadId = t1.AccountHeadId,
                                     DebitAmount = t1.DebitAmount
                                 }).ToList();
            var fromExistDate = voucher.VoucherDate.Value.AddDays(-10);
            var currentExistStock = (from t1 in _db.VoucherDetails
                                     join t2 in _db.Vouchers on t1.VoucherId equals t2.VoucherId
                                     join headGL in _db.HeadGLs on t1.AccountHeadId equals headGL.Id
                                     join head5 in _db.Head5 on headGL.ParentId equals head5.Id
                                     join head4 in _db.Head4 on head5.ParentId equals head4.Id
                                     where t2.CompanyId == companyId && head5.AccCode == "1301005001"
                                     && t1.IsActive && t2.IsActive && !t1.IsVirtual &&

    t2.VoucherDate >= fromExistDate

                                                  && t2.VoucherDate <= voucher.VoucherDate.Value
                                                  && t1.VoucherId != voucher.VoucherId

                                     select new
                                     {
                                         AccountHeadId = t1.AccountHeadId,
                                         DebitAmount = t1.DebitAmount
                                     }).AsEnumerable();

            var currentStock = (from t1 in _db.VoucherDetails
                                where t1.VoucherId == voucher.VoucherId && t1.IsActive
                                select new
                                {
                                    DebitAmount = t1.DebitAmount,
                                    AccountHeadId = t1.AccountHeadId

                                }).ToList();

            List<VoucherDetail> voucherDetailList = new List<VoucherDetail>();
            if (!currentExistStock.Any())
            {
                foreach (var item in previousStock.Where(x => x.DebitAmount > 0))
                {
                    VoucherDetail voucherDetail = new VoucherDetail
                    {
                        VoucherId = voucher.VoucherId,

                        AccountHeadId = item.AccountHeadId,
                        CreditAmount = item.DebitAmount,
                        DebitAmount = 0,
                        TransactionDate = DateTime.Now,
                        IsActive = true,
                        IsVirtual = true


                    };
                    voucherDetailList.Add(voucherDetail);
                    VoucherDetail voucherDetail1 = new VoucherDetail
                    {
                        VoucherId = voucher.VoucherId,

                        AccountHeadId = 50611914,
                        CreditAmount = 0,
                        DebitAmount = item.DebitAmount,
                        TransactionDate = DateTime.Now,
                        IsActive = true,
                        IsVirtual = true


                    };
                    voucherDetailList.Add(voucherDetail1);

                }
            }

            foreach (var item in currentStock)
            {


                VoucherDetail voucherDetail1 = new VoucherDetail
                {
                    VoucherId = voucher.VoucherId,

                    AccountHeadId = 50611914,
                    CreditAmount = item.DebitAmount,
                    DebitAmount = 0,
                    TransactionDate = DateTime.Now,
                    IsActive = true
                };
                voucherDetailList.Add(voucherDetail1);

            }

            _db.VoucherDetails.AddRange(voucherDetailList);

            if (await _db.SaveChangesAsync() > 0)
            {
                result = voucher.VoucherId;
            }

            return result;

        }

        public async Task<long> SafeFoodAutoInsertStockVoucherDetails(int companyId, int voucherId)
        {
            long result = -1;

            var voucher = await _db.Vouchers.FindAsync(voucherId);

            var fromDate = voucher.VoucherDate.Value.AddDays(0 - voucher.VoucherDate.Value.Day).AddDays(-10);
            var toDate = voucher.VoucherDate.Value.AddDays(0 - voucher.VoucherDate.Value.Day);
            var previousStock = (from t1 in _db.VoucherDetails
                                 join t2 in _db.Vouchers on t1.VoucherId equals t2.VoucherId
                                 join headGL in _db.HeadGLs on t1.AccountHeadId equals headGL.Id
                                 join head5 in _db.Head5 on headGL.ParentId equals head5.Id
                                 join head4 in _db.Head4 on head5.ParentId equals head4.Id
                                 where t2.CompanyId == companyId && head5.ParentId == 50611881
                                 && t1.IsActive && t2.IsActive && !t1.IsVirtual &&
                                 t2.Accounting_CostCenterFk == voucher.Accounting_CostCenterFk &&
                                     t2.VoucherDate >= fromDate
                                     && t2.VoucherDate <= toDate

                                 select new
                                 {
                                     AccountHeadId = t1.AccountHeadId,
                                     DebitAmount = t1.DebitAmount
                                 }).ToList();
            var fromExistDate = voucher.VoucherDate.Value.AddDays(-10);
            var currentExistStock = (from t1 in _db.VoucherDetails
                                     join t2 in _db.Vouchers on t1.VoucherId equals t2.VoucherId
                                     join headGL in _db.HeadGLs on t1.AccountHeadId equals headGL.Id
                                     join head5 in _db.Head5 on headGL.ParentId equals head5.Id
                                     join head4 in _db.Head4 on head5.ParentId equals head4.Id
                                     where t2.CompanyId == companyId && head5.ParentId == 50611881
                                     && t1.IsActive && t2.IsActive && !t1.IsVirtual &&

    t2.VoucherDate >= fromExistDate

                                                  && t2.VoucherDate <= voucher.VoucherDate.Value
                                                  && t1.VoucherId != voucher.VoucherId

                                     select new
                                     {
                                         AccountHeadId = t1.AccountHeadId,
                                         DebitAmount = t1.DebitAmount
                                     }).AsEnumerable();

            var currentStock = (from t1 in _db.VoucherDetails
                                where t1.VoucherId == voucher.VoucherId && t1.IsActive
                                select new
                                {
                                    DebitAmount = t1.DebitAmount,
                                    AccountHeadId = t1.AccountHeadId

                                }).ToList();

            List<VoucherDetail> voucherDetailList = new List<VoucherDetail>();
            if (!currentExistStock.Any())
            {
                foreach (var item in previousStock.Where(x => x.DebitAmount > 0))
                {
                    VoucherDetail voucherDetail = new VoucherDetail
                    {
                        VoucherId = voucher.VoucherId,

                        AccountHeadId = item.AccountHeadId,
                        CreditAmount = item.DebitAmount,
                        DebitAmount = 0,
                        TransactionDate = DateTime.Now,
                        IsActive = true,
                        IsVirtual = true


                    };
                    voucherDetailList.Add(voucherDetail);
                    VoucherDetail voucherDetail1 = new VoucherDetail
                    {
                        VoucherId = voucher.VoucherId,

                        AccountHeadId = 50612090,
                        CreditAmount = 0,
                        DebitAmount = item.DebitAmount,
                        TransactionDate = DateTime.Now,
                        IsActive = true,
                        IsVirtual = true


                    };
                    voucherDetailList.Add(voucherDetail1);

                }
            }

            foreach (var item in currentStock)
            {


                VoucherDetail voucherDetail1 = new VoucherDetail
                {
                    VoucherId = voucher.VoucherId,

                    AccountHeadId = 50612090,
                    CreditAmount = item.DebitAmount,
                    DebitAmount = 0,
                    TransactionDate = DateTime.Now,
                    IsActive = true
                };
                voucherDetailList.Add(voucherDetail1);

            }

            _db.VoucherDetails.AddRange(voucherDetailList);

            if (await _db.SaveChangesAsync() > 0)
            {
                result = voucher.VoucherId;
            }

            return result;

        }

        public async Task<List<HeadGLModel>> Head5Get(int companyId, int parentId)
        {

            List<HeadGLModel> head5List =
               await Task.Run(() => (from t1 in _db.Head5
                                     where t1.ParentId == parentId && t1.CompanyId == companyId
                                     select new HeadGLModel
                                     {
                                         Id = t1.Id,
                                         AccName = t1.AccCode + " -" + t1.AccName
                                     }).ToList());
            return head5List;
        }

        public async Task<List<HeadGLModel>> HeadGLGet(int companyId, int parentId)
        {

            List<HeadGLModel> headGLList =
               await Task.Run(() => (from t1 in _db.HeadGLs
                                     where t1.ParentId == parentId && t1.CompanyId == companyId
                                     select new HeadGLModel
                                     {
                                         Id = t1.Id,
                                         AccName = t1.AccCode + " -" + t1.AccName
                                     }).ToList());
            return headGLList;
        }

        public async Task<List<HeadGLModel>> HeadGLByHead5ParentIdGet(int companyId, int parentId)
        {

            List<HeadGLModel> headGLModelList =
               await Task.Run(() => (from t1 in _db.HeadGLs
                                     join t2 in _db.Head5 on t1.ParentId equals t2.Id
                                     where t2.ParentId == parentId && t1.CompanyId == companyId
                                     select new HeadGLModel
                                     {
                                         Id = t1.Id,
                                         AccName = t1.AccCode + " -" + t1.AccName
                                     }).ToList());
            return headGLModelList;
        }

        public async Task<List<HeadGLModel>> HeadGLByHeadGLParentIdGet(int companyId, int parentId)
        {

            List<HeadGLModel> headGLModelList =
               await Task.Run(() => (from t1 in _db.HeadGLs
                                     where t1.ParentId == parentId && t1.CompanyId == companyId
                                     select new HeadGLModel
                                     {
                                         Id = t1.Id,
                                         AccName = t1.AccName
                                     }).ToList());
            return headGLModelList;
        }

        public object GetAutoCompleteHeadGL(string prefix, int companyId)
        {
            var v = (from t1 in _db.HeadGLs
                     join t2 in _db.Head5 on t1.ParentId equals t2.Id
                     join t3 in _db.Head4 on t2.ParentId equals t3.Id

                     where t1.CompanyId == companyId && t1.IsActive && t2.IsActive && t3.IsActive
                     && ((t1.AccName.Contains(prefix)) || (t1.AccCode.Contains(prefix)))
                     select new
                     {
                         label = "[" + t1.AccCode + "] " + (t3.AccName != t1.AccName ? t3.AccName + " " + t1.AccName : t1.AccName),
                         val = t1.Id
                     }).OrderBy(x => x.label).Take(150).ToList();

            return v;
        }

        public object GetAutoCompleteExpenseHeadGL(string prefix, int companyId)
        {
            var v = (from t1 in _db.HeadGLs
                     join t2 in _db.Head5 on t1.ParentId equals t2.Id
                     join t3 in _db.Head4 on t2.ParentId equals t3.Id

                     where t1.CompanyId == companyId && t3.AccCode.StartsWith("440") && t1.IsActive && t2.IsActive && t3.IsActive
                           && ((t1.AccName.Contains(prefix)) || (t1.AccCode.Contains(prefix)))
                     select new
                     {
                         label = "[" + t1.AccCode + "] " + (t3.AccName != t1.AccName ? t3.AccName + " " + t1.AccName : t1.AccName),
                         val = t1.Id
                     }).OrderBy(x => x.label).Take(150).ToList();

            return v;
        }

        public async Task<long> AutoInsertVoucherDetails(int voucherId, int virtualHeadId, string virtualHeadParticular)
        {
            long result = -1;

            var voucher = await _db.Vouchers.FindAsync(voucherId);
            double totalDebitAmount = _db.VoucherDetails.Where(x => x.VoucherId == voucherId && x.IsActive == true).Select(x => x.DebitAmount).DefaultIfEmpty(0).Sum();
            double totalCreditAmount = _db.VoucherDetails.Where(x => x.VoucherId == voucherId && x.IsActive == true).Select(x => x.CreditAmount).DefaultIfEmpty(0).Sum();
            double newAmount = 0;
            if (totalDebitAmount > totalCreditAmount)
            {
                newAmount = totalDebitAmount - totalCreditAmount;
            }
            else
            {
                newAmount = totalCreditAmount - totalDebitAmount;
            }
            if (newAmount > 0 && virtualHeadId > 0)
            {
                VoucherDetail voucherDetail = new VoucherDetail
                {
                    VoucherId = voucher.VoucherId,

                    AccountHeadId = virtualHeadId,
                    CreditAmount = totalDebitAmount > totalCreditAmount ? newAmount : 0,
                    DebitAmount = totalCreditAmount > totalDebitAmount ? newAmount : 0,
                    TransactionDate = DateTime.Now,
                    IsActive = true,
                    Particular = virtualHeadParticular
                };
                _db.VoucherDetails.Add(voucherDetail);
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = voucherDetail.VoucherDetailId;
                }
            }

            return result;

        }

        public async Task<long> NFFLAutoInsertStockVoucherDetails(int companyId, int voucherId)
        {
            long result = -1;

            var voucher = await _db.Vouchers.FindAsync(voucherId);

            var fromDate = voucher.VoucherDate.Value.AddDays(0 - voucher.VoucherDate.Value.Day).AddDays(-10);
            var toDate = voucher.VoucherDate.Value.AddDays(0 - voucher.VoucherDate.Value.Day);
            var previousStock = (from t1 in _db.VoucherDetails
                                 join t2 in _db.Vouchers on t1.VoucherId equals t2.VoucherId
                                 join t3 in _db.HeadGLs on t1.AccountHeadId equals t3.Id
                                 join t4 in _db.Head5 on t3.ParentId equals t4.Id
                                 where t2.CompanyId == companyId && t4.AccCode == "1301004001"
                                 && t1.IsActive && t2.IsActive && !t1.IsVirtual
                                 && t2.Accounting_CostCenterFk == voucher.Accounting_CostCenterFk &&
                                     t2.VoucherDate >= fromDate
                                     && t2.VoucherDate <= toDate


                                 select new
                                 {
                                     AccountHeadId = t1.AccountHeadId,
                                     DebitAmount = t1.DebitAmount
                                 }).ToList();
            var fromExistDate = voucher.VoucherDate.Value.AddDays(-10);
            var currentStockExist = (from t1 in _db.VoucherDetails
                                     join t2 in _db.Vouchers on t1.VoucherId equals t2.VoucherId
                                     join t3 in _db.HeadGLs on t1.AccountHeadId equals t3.Id
                                     join t4 in _db.Head5 on t3.ParentId equals t4.Id
                                     where t2.CompanyId == companyId && t4.AccCode == "1301004001"
                                     && t1.IsActive && t2.IsActive && !t1.IsVirtual &&
                                         t2.VoucherDate >= fromExistDate
                                         && t2.VoucherDate <= voucher.VoucherDate.Value
                                                  && t1.VoucherId != voucher.VoucherId

                                     select new
                                     {
                                         AccountHeadId = t1.AccountHeadId,
                                         DebitAmount = t1.DebitAmount
                                     }).AsEnumerable();

            var currentStock = (from t1 in _db.VoucherDetails
                                where t1.VoucherId == voucher.VoucherId && t1.IsActive
                                select new
                                {
                                    DebitAmount = t1.DebitAmount,
                                    AccountHeadId = t1.AccountHeadId
                                }).ToList();

            List<VoucherDetail> voucherDetailList = new List<VoucherDetail>();
            if (!currentStockExist.Any())
            {
                foreach (var item in previousStock.Where(x => x.DebitAmount > 0))
                {
                    VoucherDetail voucherDetail = new VoucherDetail
                    {
                        VoucherId = voucher.VoucherId,

                        AccountHeadId = item.AccountHeadId,
                        CreditAmount = item.DebitAmount,
                        DebitAmount = 0,
                        TransactionDate = DateTime.Now,
                        IsActive = true,
                        IsVirtual = true
                    };
                    voucherDetailList.Add(voucherDetail);
                    VoucherDetail voucherDetail1 = new VoucherDetail
                    {
                        VoucherId = voucher.VoucherId,

                        AccountHeadId = 50601365,
                        CreditAmount = 0,
                        DebitAmount = item.DebitAmount,
                        TransactionDate = DateTime.Now,
                        IsActive = true,
                        IsVirtual = true


                    };
                    voucherDetailList.Add(voucherDetail1);

                }
            }

            foreach (var item in currentStock)
            {


                VoucherDetail voucherDetail1 = new VoucherDetail
                {
                    VoucherId = voucher.VoucherId,

                    AccountHeadId = 50601365,
                    CreditAmount = item.DebitAmount,
                    DebitAmount = 0,
                    TransactionDate = DateTime.Now,
                    IsActive = true
                };
                voucherDetailList.Add(voucherDetail1);

            }

            _db.VoucherDetails.AddRange(voucherDetailList);

            if (await _db.SaveChangesAsync() > 0)
            {
                result = voucher.VoucherId;
            }

            return result;

        }

        public async Task<long> OPLAutoInsertStockVoucherDetails(int companyId, int voucherId)
        {
            long result = -1;

            var voucher = await _db.Vouchers.FindAsync(voucherId);

            var fromDate = voucher.VoucherDate.Value.AddDays(0 - voucher.VoucherDate.Value.Day).AddDays(-10);
            var toDate = voucher.VoucherDate.Value.AddDays(0 - voucher.VoucherDate.Value.Day);
            var previousStock = (from t1 in _db.VoucherDetails
                                 join t2 in _db.Vouchers on t1.VoucherId equals t2.VoucherId
                                 join t3 in _db.HeadGLs on t1.AccountHeadId equals t3.Id
                                 join t4 in _db.Head5 on t3.ParentId equals t4.Id
                                 join t5 in _db.Head4 on t4.ParentId equals t5.Id
                                 where t2.CompanyId == companyId && t5.AccCode == "1301004"
                                 && t1.IsActive && t2.IsActive && !t1.IsVirtual &&
                                 t2.Accounting_CostCenterFk == voucher.Accounting_CostCenterFk &&
                                     t2.VoucherDate >= fromDate
                                     && t2.VoucherDate <= toDate

                                 select new
                                 {
                                     AccountHeadId = t1.AccountHeadId,
                                     DebitAmount = t1.DebitAmount
                                 }).ToList();

            var fromExistDate = voucher.VoucherDate.Value.AddDays(-10);

            var currentExistStock = (from t1 in _db.VoucherDetails
                                     join t2 in _db.Vouchers on t1.VoucherId equals t2.VoucherId
                                     join t3 in _db.HeadGLs on t1.AccountHeadId equals t3.Id
                                     join t4 in _db.Head5 on t3.ParentId equals t4.Id
                                     join t5 in _db.Head4 on t4.ParentId equals t5.Id
                                     where t2.CompanyId == companyId && t5.AccCode == "1301004"
                                     && t1.IsActive && t2.IsActive && !t1.IsVirtual &&
                                         t2.VoucherDate >= fromExistDate
                                         && t2.VoucherDate <= voucher.VoucherDate.Value
                                                  && t1.VoucherId != voucher.VoucherId

                                     select new
                                     {
                                         AccountHeadId = t1.AccountHeadId,
                                         DebitAmount = t1.DebitAmount
                                     }).AsEnumerable();
            var currentStock = (from t1 in _db.VoucherDetails
                                where t1.VoucherId == voucher.VoucherId && t1.IsActive
                                select new
                                {
                                    DebitAmount = t1.DebitAmount,
                                    AccountHeadId = t1.AccountHeadId

                                }).ToList();

            List<VoucherDetail> voucherDetailList = new List<VoucherDetail>();
            if (!currentExistStock.Any())
            {
                foreach (var item in previousStock.Where(x => x.DebitAmount > 0))
                {
                    VoucherDetail voucherDetail = new VoucherDetail
                    {
                        VoucherId = voucher.VoucherId,

                        AccountHeadId = item.AccountHeadId,
                        CreditAmount = item.DebitAmount,
                        DebitAmount = 0,
                        TransactionDate = DateTime.Now,
                        IsActive = true,
                        IsVirtual = true


                    };
                    voucherDetailList.Add(voucherDetail);
                    VoucherDetail voucherDetail1 = new VoucherDetail
                    {
                        VoucherId = voucher.VoucherId,

                        AccountHeadId = 50607042,
                        CreditAmount = 0,
                        DebitAmount = item.DebitAmount,
                        TransactionDate = DateTime.Now,
                        IsActive = true,
                        IsVirtual = true


                    };
                    voucherDetailList.Add(voucherDetail1);

                }
            }

            foreach (var item in currentStock)
            {


                VoucherDetail voucherDetail1 = new VoucherDetail
                {
                    VoucherId = voucher.VoucherId,

                    AccountHeadId = 50607042,
                    CreditAmount = item.DebitAmount,
                    DebitAmount = 0,
                    TransactionDate = DateTime.Now,
                    IsActive = true
                };
                voucherDetailList.Add(voucherDetail1);

            }

            _db.VoucherDetails.AddRange(voucherDetailList);

            if (await _db.SaveChangesAsync() > 0)
            {
                result = voucher.VoucherId;
            }

            return result;

        }

        public async Task<long> SODLAutoInsertStockVoucherDetails(int companyId, int voucherId)
        {
            long result = -1;

            var voucher = await _db.Vouchers.FindAsync(voucherId);

            var fromDate = voucher.VoucherDate.Value.AddDays(0 - voucher.VoucherDate.Value.Day).AddDays(-10);
            var toDate = voucher.VoucherDate.Value.AddDays(0 - voucher.VoucherDate.Value.Day);

            var previousStock = (from t1 in _db.VoucherDetails
                                 join t2 in _db.Vouchers on t1.VoucherId equals t2.VoucherId
                                 join t3 in _db.HeadGLs on t1.AccountHeadId equals t3.Id
                                 join t4 in _db.Head5 on t3.ParentId equals t4.Id
                                 join t5 in _db.Head4 on t4.ParentId equals t5.Id
                                 where t2.CompanyId == companyId && t5.AccCode == "1301004"
                                 && t1.IsActive && t2.IsActive && !t1.IsVirtual &&
                                 t2.Accounting_CostCenterFk == voucher.Accounting_CostCenterFk &&
                                     t2.VoucherDate >= fromDate
                                     && t2.VoucherDate <= toDate

                                 select new
                                 {
                                     AccountHeadId = t1.AccountHeadId,
                                     DebitAmount = t1.DebitAmount
                                 }).ToList();
            var fromExistDate = voucher.VoucherDate.Value.AddDays(-10);

            var existCurrentMonthStock = (from t1 in _db.VoucherDetails
                                          join t2 in _db.Vouchers on t1.VoucherId equals t2.VoucherId
                                          join t3 in _db.HeadGLs on t1.AccountHeadId equals t3.Id
                                          join t4 in _db.Head5 on t3.ParentId equals t4.Id
                                          join t5 in _db.Head4 on t4.ParentId equals t5.Id
                                          where t2.CompanyId == companyId && t5.AccCode == "1301004"
                                          && t1.IsActive && t2.IsActive && !t1.IsVirtual &&
                                              t2.VoucherDate >= fromExistDate
                                              && t2.VoucherDate <= voucher.VoucherDate.Value
                                              && t1.VoucherId != voucher.VoucherId
                                          select new
                                          {
                                              AccountHeadId = t1.AccountHeadId,
                                              DebitAmount = t1.DebitAmount
                                          }).AsEnumerable();

            var currentStock = (from t1 in _db.VoucherDetails
                                where t1.VoucherId == voucher.VoucherId && t1.IsActive
                                select new
                                {
                                    DebitAmount = t1.DebitAmount,
                                    AccountHeadId = t1.AccountHeadId

                                }).ToList();

            List<VoucherDetail> voucherDetailList = new List<VoucherDetail>();

            if (!existCurrentMonthStock.Any())
            {
                foreach (var item in previousStock.Where(x => x.DebitAmount > 0))
                {
                    VoucherDetail voucherDetail = new VoucherDetail
                    {
                        VoucherId = voucher.VoucherId,
                        AccountHeadId = item.AccountHeadId,
                        CreditAmount = item.DebitAmount,
                        DebitAmount = 0,
                        TransactionDate = DateTime.Now,
                        IsActive = true,
                        IsVirtual = true


                    };
                    voucherDetailList.Add(voucherDetail);
                    VoucherDetail voucherDetail1 = new VoucherDetail
                    {
                        VoucherId = voucher.VoucherId,

                        AccountHeadId = 50607044,
                        CreditAmount = 0,
                        DebitAmount = item.DebitAmount,
                        TransactionDate = DateTime.Now,
                        IsActive = true,
                        IsVirtual = true


                    };
                    voucherDetailList.Add(voucherDetail1);

                }
            }

            foreach (var item in currentStock)
            {


                VoucherDetail voucherDetail1 = new VoucherDetail
                {
                    VoucherId = voucher.VoucherId,

                    AccountHeadId = 50607044,
                    CreditAmount = item.DebitAmount,
                    DebitAmount = 0,
                    TransactionDate = DateTime.Now,
                    IsActive = true
                };
                voucherDetailList.Add(voucherDetail1);

            }

            _db.VoucherDetails.AddRange(voucherDetailList);

            if (await _db.SaveChangesAsync() > 0)
            {
                result = voucher.VoucherId;
            }

            return result;

        }

        public async Task<long> PrintingAutoInsertStockVoucherDetails(int companyId, int voucherId)
        {
            long result = -1;

            var voucher = await _db.Vouchers.FindAsync(voucherId);

            var fromDate = voucher.VoucherDate.Value.AddDays(0 - voucher.VoucherDate.Value.Day).AddDays(-10);
            var toDate = voucher.VoucherDate.Value.AddDays(0 - voucher.VoucherDate.Value.Day);
            var previousStock = (from t1 in _db.VoucherDetails
                                 join t2 in _db.Vouchers on t1.VoucherId equals t2.VoucherId
                                 join headGL in _db.HeadGLs on t1.AccountHeadId equals headGL.Id
                                 join head5 in _db.Head5 on headGL.ParentId equals head5.Id
                                 join head4 in _db.Head4 on head5.ParentId equals head4.Id
                                 where t2.CompanyId == companyId && head4.AccCode == "1305001"
                                 && t1.IsActive && t2.IsActive && !t1.IsVirtual &&
                                 t2.Accounting_CostCenterFk == voucher.Accounting_CostCenterFk &&
                                     t2.VoucherDate >= fromDate
                                     && t2.VoucherDate <= toDate

                                 select new
                                 {
                                     AccountHeadId = t1.AccountHeadId,
                                     DebitAmount = t1.DebitAmount
                                 }).ToList();

            var fromExistDate = voucher.VoucherDate.Value.AddDays(-10);

            var currentExistStock = (from t1 in _db.VoucherDetails
                                     join t2 in _db.Vouchers on t1.VoucherId equals t2.VoucherId
                                     join headGL in _db.HeadGLs on t1.AccountHeadId equals headGL.Id
                                     join head5 in _db.Head5 on headGL.ParentId equals head5.Id
                                     join head4 in _db.Head4 on head5.ParentId equals head4.Id
                                     where t2.CompanyId == companyId && head4.AccCode == "1305001"
                                     && t1.IsActive && t2.IsActive && !t1.IsVirtual &&
                                          t2.VoucherDate >= fromExistDate
                                                  && t2.VoucherDate <= voucher.VoucherDate.Value
                                                  && t1.VoucherId != voucher.VoucherId

                                     select new
                                     {
                                         AccountHeadId = t1.AccountHeadId,
                                         DebitAmount = t1.DebitAmount
                                     }).AsEnumerable();

            var currentStock = (from t1 in _db.VoucherDetails
                                where t1.VoucherId == voucher.VoucherId && t1.IsActive
                                select new
                                {
                                    DebitAmount = t1.DebitAmount,
                                    AccountHeadId = t1.AccountHeadId

                                }).ToList();

            List<VoucherDetail> voucherDetailList = new List<VoucherDetail>();
            if (!currentExistStock.Any())
            {
                foreach (var item in previousStock.Where(x => x.DebitAmount > 0))
                {
                    VoucherDetail voucherDetail = new VoucherDetail
                    {
                        VoucherId = voucher.VoucherId,

                        AccountHeadId = item.AccountHeadId,
                        CreditAmount = item.DebitAmount,
                        DebitAmount = 0,
                        TransactionDate = DateTime.Now,
                        IsActive = true,
                        IsVirtual = true


                    };
                    voucherDetailList.Add(voucherDetail);
                    VoucherDetail voucherDetail1 = new VoucherDetail
                    {
                        VoucherId = voucher.VoucherId,

                        AccountHeadId = 50607050,
                        CreditAmount = 0,
                        DebitAmount = item.DebitAmount,
                        TransactionDate = DateTime.Now,
                        IsActive = true,
                        IsVirtual = true


                    };
                    voucherDetailList.Add(voucherDetail1);

                }
            }

            foreach (var item in currentStock)
            {


                VoucherDetail voucherDetail1 = new VoucherDetail
                {
                    VoucherId = voucher.VoucherId,

                    AccountHeadId = 50607050,
                    CreditAmount = item.DebitAmount,
                    DebitAmount = 0,
                    TransactionDate = DateTime.Now,
                    IsActive = true
                };
                voucherDetailList.Add(voucherDetail1);
            }

            _db.VoucherDetails.AddRange(voucherDetailList);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = voucher.VoucherId;
            }
            return result;

        }

        public async Task<long> FBLAutoInsertStockVoucherDetails(int companyId, int voucherId)
        {
            long result = -1;

            var voucher = await _db.Vouchers.FindAsync(voucherId);

            var fromDate = voucher.VoucherDate.Value.AddDays(0 - voucher.VoucherDate.Value.Day).AddDays(-10);
            var toDate = voucher.VoucherDate.Value.AddDays(0 - voucher.VoucherDate.Value.Day);
            var previousStock = (from t1 in _db.VoucherDetails
                                 join t2 in _db.Vouchers on t1.VoucherId equals t2.VoucherId
                                 join headGL in _db.HeadGLs on t1.AccountHeadId equals headGL.Id
                                 join head5 in _db.Head5 on headGL.ParentId equals head5.Id
                                 join head4 in _db.Head4 on head5.ParentId equals head4.Id
                                 join head3 in _db.Head3 on head4.ParentId equals head3.Id

                                 where t2.CompanyId == companyId && head3.AccCode == "1305"
                                 && t1.IsActive && t2.IsActive && !t1.IsVirtual &&
                                 t2.Accounting_CostCenterFk == voucher.Accounting_CostCenterFk &&
                                     t2.VoucherDate >= fromDate
                                     && t2.VoucherDate <= toDate

                                 select new
                                 {
                                     AccountHeadId = t1.AccountHeadId,
                                     DebitAmount = t1.DebitAmount
                                 }).ToList();

            var fromExistDate = voucher.VoucherDate.Value.AddDays(-10);
            var currentExistStock = (from t1 in _db.VoucherDetails
                                     join t2 in _db.Vouchers on t1.VoucherId equals t2.VoucherId
                                     join headGL in _db.HeadGLs on t1.AccountHeadId equals headGL.Id
                                     join head5 in _db.Head5 on headGL.ParentId equals head5.Id
                                     join head4 in _db.Head4 on head5.ParentId equals head4.Id
                                     join head3 in _db.Head3 on head4.ParentId equals head3.Id

                                     where t2.CompanyId == companyId && head3.AccCode == "1305"
                                     && t1.IsActive && t2.IsActive && !t1.IsVirtual &&
                                         t2.VoucherDate >= fromExistDate
                                                  && t2.VoucherDate <= voucher.VoucherDate.Value
                                                  && t1.VoucherId != voucher.VoucherId

                                     select new
                                     {
                                         AccountHeadId = t1.AccountHeadId,
                                         DebitAmount = t1.DebitAmount
                                     }).AsEnumerable();

            var currentStock = (from t1 in _db.VoucherDetails
                                where t1.VoucherId == voucher.VoucherId && t1.IsActive
                                select new
                                {
                                    DebitAmount = t1.DebitAmount,
                                    AccountHeadId = t1.AccountHeadId
                                }).ToList();

            List<VoucherDetail> voucherDetailList = new List<VoucherDetail>();
            if (!currentExistStock.Any())
            {
                foreach (var item in previousStock.Where(x => x.DebitAmount > 0))
                {
                    VoucherDetail voucherDetail = new VoucherDetail
                    {
                        VoucherId = voucher.VoucherId,
                        AccountHeadId = item.AccountHeadId,
                        CreditAmount = item.DebitAmount,
                        DebitAmount = 0,
                        TransactionDate = DateTime.Now,
                        IsActive = true,
                        IsVirtual = true
                    };
                    voucherDetailList.Add(voucherDetail);
                    VoucherDetail voucherDetail1 = new VoucherDetail
                    {
                        VoucherId = voucher.VoucherId,
                        AccountHeadId = 50607519,
                        CreditAmount = 0,
                        DebitAmount = item.DebitAmount,
                        TransactionDate = DateTime.Now,
                        IsActive = true,
                        IsVirtual = true
                    };
                    voucherDetailList.Add(voucherDetail1);
                }
            }

            foreach (var item in currentStock)
            {


                VoucherDetail voucherDetail1 = new VoucherDetail
                {
                    VoucherId = voucher.VoucherId,

                    AccountHeadId = 50607519,
                    CreditAmount = item.DebitAmount,
                    DebitAmount = 0,
                    TransactionDate = DateTime.Now,
                    IsActive = true
                };
                voucherDetailList.Add(voucherDetail1);

            }

            _db.VoucherDetails.AddRange(voucherDetailList);

            if (await _db.SaveChangesAsync() > 0)
            {
                result = voucher.VoucherId;
            }

            return result;

        }

        public async Task<long> PackagingAutoInsertStockVoucherDetails(int companyId, int voucherId)
        {
            long result = -1;

            var voucher = await _db.Vouchers.FindAsync(voucherId);

            var fromDate = voucher.VoucherDate.Value.AddDays(0 - voucher.VoucherDate.Value.Day).AddDays(-10);
            var toDate = voucher.VoucherDate.Value.AddDays(0 - voucher.VoucherDate.Value.Day);
            var previousStock = (from t1 in _db.VoucherDetails
                                 join t2 in _db.Vouchers on t1.VoucherId equals t2.VoucherId
                                 join headGL in _db.HeadGLs on t1.AccountHeadId equals headGL.Id
                                 join head5 in _db.Head5 on headGL.ParentId equals head5.Id
                                 join head4 in _db.Head4 on head5.ParentId equals head4.Id

                                 where t2.CompanyId == companyId && head4.AccCode == "1301005"
                                 && t1.IsActive && t2.IsActive && !t1.IsVirtual &&
                                 t2.Accounting_CostCenterFk == voucher.Accounting_CostCenterFk &&
                                     t2.VoucherDate >= fromDate
                                     && t2.VoucherDate <= toDate

                                 select new
                                 {
                                     AccountHeadId = t1.AccountHeadId,
                                     DebitAmount = t1.DebitAmount
                                 }).ToList();

            var fromExistDate = voucher.VoucherDate.Value.AddDays(-10);

            var currentExistStock = (from t1 in _db.VoucherDetails
                                     join t2 in _db.Vouchers on t1.VoucherId equals t2.VoucherId
                                     join headGL in _db.HeadGLs on t1.AccountHeadId equals headGL.Id
                                     join head5 in _db.Head5 on headGL.ParentId equals head5.Id
                                     join head4 in _db.Head4 on head5.ParentId equals head4.Id

                                     where t2.CompanyId == companyId && head4.AccCode == "1301005"
                                     && t1.IsActive && t2.IsActive && !t1.IsVirtual &&
                                         t2.VoucherDate >= fromExistDate
                                                  && t2.VoucherDate <= voucher.VoucherDate.Value
                                                  && t1.VoucherId != voucher.VoucherId

                                     select new
                                     {
                                         AccountHeadId = t1.AccountHeadId,
                                         DebitAmount = t1.DebitAmount
                                     }).AsEnumerable();
            var currentStock = (from t1 in _db.VoucherDetails
                                where t1.VoucherId == voucher.VoucherId && t1.IsActive
                                select new
                                {
                                    DebitAmount = t1.DebitAmount,
                                    AccountHeadId = t1.AccountHeadId

                                }).ToList();

            List<VoucherDetail> voucherDetailList = new List<VoucherDetail>();
            if (!currentExistStock.Any())
            {
                foreach (var item in previousStock.Where(x => x.DebitAmount > 0))
                {
                    VoucherDetail voucherDetail = new VoucherDetail
                    {
                        VoucherId = voucher.VoucherId,

                        AccountHeadId = item.AccountHeadId,
                        CreditAmount = item.DebitAmount,
                        DebitAmount = 0,
                        TransactionDate = DateTime.Now,
                        IsActive = true,
                        IsVirtual = true


                    };
                    voucherDetailList.Add(voucherDetail);
                    VoucherDetail voucherDetail1 = new VoucherDetail
                    {
                        VoucherId = voucher.VoucherId,

                        AccountHeadId = 50605003,
                        CreditAmount = 0,
                        DebitAmount = item.DebitAmount,
                        TransactionDate = DateTime.Now,
                        IsActive = true,
                        IsVirtual = true


                    };
                    voucherDetailList.Add(voucherDetail1);

                }
            }

            foreach (var item in currentStock)
            {
                VoucherDetail voucherDetail1 = new VoucherDetail
                {
                    VoucherId = voucher.VoucherId,

                    AccountHeadId = 50605003,
                    CreditAmount = item.DebitAmount,
                    DebitAmount = 0,
                    TransactionDate = DateTime.Now,
                    IsActive = true
                };
                voucherDetailList.Add(voucherDetail1);

            }

            _db.VoucherDetails.AddRange(voucherDetailList);

            if (await _db.SaveChangesAsync() > 0)
            {
                result = voucher.VoucherId;
            }

            return result;

        }

        public async Task<long> KBLAutoInsertStockVoucherDetails(int companyId, int voucherId)
        {
            long result = -1;

            var voucher = await _db.Vouchers.FindAsync(voucherId);

            var fromDate = voucher.VoucherDate.Value.AddDays(0 - voucher.VoucherDate.Value.Day).AddDays(-10);
            var toDate = voucher.VoucherDate.Value.AddDays(0 - voucher.VoucherDate.Value.Day);
            var previousStock = (from t1 in _db.VoucherDetails
                                 join t2 in _db.Vouchers on t1.VoucherId equals t2.VoucherId
                                 join headGL in _db.HeadGLs on t1.AccountHeadId equals headGL.Id
                                 join head5 in _db.Head5 on headGL.ParentId equals head5.Id
                                 join head4 in _db.Head4 on head5.ParentId equals head4.Id
                                 where t2.CompanyId == companyId && head5.AccCode == "1301005001"
                                 && t1.IsActive && t2.IsActive && !t1.IsVirtual &&
                                 t2.Accounting_CostCenterFk == voucher.Accounting_CostCenterFk &&
                                     t2.VoucherDate >= fromDate
                                     && t2.VoucherDate <= toDate

                                 select new
                                 {
                                     AccountHeadId = t1.AccountHeadId,
                                     DebitAmount = t1.DebitAmount
                                 }).ToList();
            var fromExistDate = voucher.VoucherDate.Value.AddDays(-10);
            var currentExistStock = (from t1 in _db.VoucherDetails
                                     join t2 in _db.Vouchers on t1.VoucherId equals t2.VoucherId
                                     join headGL in _db.HeadGLs on t1.AccountHeadId equals headGL.Id
                                     join head5 in _db.Head5 on headGL.ParentId equals head5.Id
                                     join head4 in _db.Head4 on head5.ParentId equals head4.Id
                                     where t2.CompanyId == companyId && head5.AccCode == "1301005001"
                                     && t1.IsActive && t2.IsActive && !t1.IsVirtual &&

    t2.VoucherDate >= fromExistDate

                                                  && t2.VoucherDate <= voucher.VoucherDate.Value
                                                  && t1.VoucherId != voucher.VoucherId

                                     select new
                                     {
                                         AccountHeadId = t1.AccountHeadId,
                                         DebitAmount = t1.DebitAmount
                                     }).AsEnumerable();

            var currentStock = (from t1 in _db.VoucherDetails
                                where t1.VoucherId == voucher.VoucherId && t1.IsActive
                                select new
                                {
                                    DebitAmount = t1.DebitAmount,
                                    AccountHeadId = t1.AccountHeadId

                                }).ToList();

            List<VoucherDetail> voucherDetailList = new List<VoucherDetail>();
            if (!currentExistStock.Any())
            {
                foreach (var item in previousStock.Where(x => x.DebitAmount > 0))
                {
                    VoucherDetail voucherDetail = new VoucherDetail
                    {
                        VoucherId = voucher.VoucherId,

                        AccountHeadId = item.AccountHeadId,
                        CreditAmount = item.DebitAmount,
                        DebitAmount = 0,
                        TransactionDate = DateTime.Now,
                        IsActive = true,
                        IsVirtual = true


                    };
                    voucherDetailList.Add(voucherDetail);
                    VoucherDetail voucherDetail1 = new VoucherDetail
                    {
                        VoucherId = voucher.VoucherId,

                        AccountHeadId = 50608410,
                        CreditAmount = 0,
                        DebitAmount = item.DebitAmount,
                        TransactionDate = DateTime.Now,
                        IsActive = true,
                        IsVirtual = true


                    };
                    voucherDetailList.Add(voucherDetail1);

                }
            }

            foreach (var item in currentStock)
            {


                VoucherDetail voucherDetail1 = new VoucherDetail
                {
                    VoucherId = voucher.VoucherId,

                    AccountHeadId = 50608410,
                    CreditAmount = item.DebitAmount,
                    DebitAmount = 0,
                    TransactionDate = DateTime.Now,
                    IsActive = true
                };
                voucherDetailList.Add(voucherDetail1);

            }

            _db.VoucherDetails.AddRange(voucherDetailList);

            if (await _db.SaveChangesAsync() > 0)
            {
                result = voucher.VoucherId;
            }

            return result;

        }

        public async Task<long> UpdateVoucherStatus(int voucherId)
        {
            long result = -1;
            Voucher voucher = await _db.Vouchers.FindAsync(voucherId);
            voucher.VoucherStatus = "A";
            voucher.IsSubmit = true;


            if (await _db.SaveChangesAsync() > 0)
            {
                result = voucher.VoucherId;

            }
            int erpSM = await SMSPush(voucher);
            return result;

        }

        private async Task<int> SMSPush(Voucher voucher)
        {
            if (voucher.CompanyId == (int)CompanyName.KrishibidSeedLimited)
            {
                string bankOrCashName = (from t1 in _db.VoucherDetails
                                         join t2 in _db.HeadGLs on t1.AccountHeadId equals t2.Id
                                         join t3 in _db.Head5 on t2.ParentId equals t3.Id
                                         where t1.VoucherId == voucher.VoucherId
                                         && t3.ParentId == 29395
                                         select t2.AccName).FirstOrDefault();

                var dataList = (from t1 in _db.VoucherDetails
                                join t2 in _db.Vendors on t1.AccountHeadId equals t2.HeadGLId
                                join t3 in _db.Vouchers on t1.VoucherId equals t3.VoucherId
                                where t1.VoucherId == voucher.VoucherId
                                && t2.VendorTypeId == 2 // vendorTypeId
                                && t3.VoucherTypeId == 19// voucherTypeId                                
                                && t2.Phone != null
                                select new
                                {
                                    VoucherNo = t3.VoucherNo,
                                    CompanyId = t3.CompanyId,
                                    VoucherDate = t3.VoucherDate,
                                    PhoneNo = t2.Phone,
                                    CreditAmount = t1.CreditAmount,
                                    Particular = t1.Particular,
                                }).AsEnumerable();
                List<ErpSMS> erpSmsList = new List<ErpSMS>();
                foreach (var item in dataList)
                {
                    ErpSMS erpSms = new ErpSMS()
                    {

                        CompanyId = item.CompanyId.Value,
                        Subject = "Collection",
                        Date = item.VoucherDate.Value,
                        PhoneNo = item.PhoneNo.Replace(" ", String.Empty).Replace("-", String.Empty).Replace("+88", String.Empty),
                        Message = "Dear customer, We have received from you Tk. " + item.CreditAmount + " /= Via " + bankOrCashName + " On " + item.Particular + " Thank you staying with Krishibid seed Ltd. If need, call: 01700729665",
                        SmsType = 1,
                        Status = (int)EnumSmSStatus.Pending,
                        Remarks = item.VoucherNo,
                        RowTime = DateTime.Now

                    };
                    erpSmsList.Add(erpSms);

                    //ErpSMS erpSms2 = new ErpSMS()
                    //{
                    //    CompanyId = item.CompanyId.Value,
                    //    Subject = "Collection",
                    //    Date = item.VoucherDate.Value,
                    //    PhoneNo = "01700729665",
                    //    Message = "Dear customer, We have received from you Tk. " + item.CreditAmount + " /= Via " + bankOrCashName + " On " + item.Particular + " Thank you staying with Krishibid seed Ltd. If need, call: 01700729665",
                    //    SmsType = 1,
                    //    Status = (int)EnumSmSStatus.Pending,
                    //    Remarks = item.VoucherNo,
                    //    RowTime = DateTime.Now

                    //};
                    //erpSmsList.Add(erpSms2);
                }
                try
                {
                    _db.ErpSMS.AddRange(erpSmsList);
                    await _db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    var x = ex.Message;
                }


            }

            if (voucher.CompanyId == (int)CompanyName.GloriousCropCareLimited)
            {
                string bankOrCashName = (from t1 in _db.VoucherDetails
                                         join t2 in _db.HeadGLs on t1.AccountHeadId equals t2.Id
                                         join t3 in _db.Head5 on t2.ParentId equals t3.Id
                                         where t1.VoucherId == voucher.VoucherId
                                         && t3.ParentId == 43904
                                         select t2.AccName).FirstOrDefault();

                var dataList = (from t1 in _db.VoucherDetails
                                join t2 in _db.Vendors on t1.AccountHeadId equals t2.HeadGLId
                                join t3 in _db.Vouchers on t1.VoucherId equals t3.VoucherId
                                where t1.VoucherId == voucher.VoucherId
                                && t2.VendorTypeId == 2 // vendorTypeId
                                //&& t3.VoucherTypeId == 10 // voucherTypeId                                
                                && t2.Phone != null
                                select new
                                {
                                    VoucherNo = t3.VoucherNo,
                                    CompanyId = t3.CompanyId,
                                    VoucherDate = t3.VoucherDate,
                                    PhoneNo = t2.Phone,
                                    CreditAmount = t1.CreditAmount,
                                    Particular = t1.Particular,
                                }).AsEnumerable();
                List<ErpSMS> erpSmsList = new List<ErpSMS>();
                foreach (var item in dataList)
                {
                    ErpSMS erpSms = new ErpSMS()
                    {

                        CompanyId = item.CompanyId.Value,
                        Subject = "Collection",
                        Date = item.VoucherDate.Value,
                        PhoneNo = item.PhoneNo.Replace(" ", String.Empty).Replace("-", String.Empty).Replace("+88", String.Empty),
                        Message = "Dear customer, We have received from you Tk. " + item.CreditAmount + " /= Via " + bankOrCashName + " On " + item.Particular + " Thank you staying with Glorious Crop Care Limited. If need, call: 01700729903",
                        SmsType = 1,
                        Status = (int)EnumSmSStatus.Pending,
                        Remarks = item.VoucherNo,
                        RowTime = DateTime.Now

                    };
                    erpSmsList.Add(erpSms);

                }
                try
                {
                    _db.ErpSMS.AddRange(erpSmsList);
                    await _db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    var x = ex.Message;
                }

            }
            return 1;
        }

        public async Task<VMJournalSlave> GetSingleVoucherDetails(int voucherDetailId)
        {
            var v = await Task.Run(() => (from t1 in _db.VoucherDetails
                                          join t2 in _db.HeadGLs on t1.AccountHeadId equals t2.Id


                                          where t1.VoucherDetailId == voucherDetailId
                                          select new VMJournalSlave
                                          {
                                              VoucherId = t1.VoucherId.Value,
                                              VoucherDetailId = t1.VoucherDetailId,
                                              Accounting_HeadFK = t1.AccountHeadId.Value,
                                              AccountingHeadName = "[" + t2.AccCode + "] " + t2.AccName,
                                              Debit = t1.DebitAmount,
                                              Credit = t1.CreditAmount,
                                              Particular = t1.Particular
                                          }).FirstOrDefault());
            return v;
        }

        public async Task<VMJournalSlave> GetSingleVoucher(int voucherId)
        {
            var v = await Task.Run(() => (from t1 in _db.Vouchers.Where(f => f.VoucherId == voucherId)
                                          select new VMJournalSlave
                                          {
                                              VoucherId = t1.VoucherId,
                                              VoucherNo = t1.VoucherNo,
                                              VoucherTypeId = t1.VoucherTypeId.Value,
                                              ChqDate = t1.ChqDate,
                                              ChqName = t1.ChqName,
                                              Narration = t1.Narration
                                          }).FirstOrDefault());
            return v;
        }

        public async Task<VoucherTypeModel> GetSingleVoucherTypes(int voucherTypesId)
        {
            var v = await Task.Run(() => (from t1 in _db.VoucherTypes
                                          where t1.VoucherTypeId == voucherTypesId
                                          select new VoucherTypeModel
                                          {
                                              Code = t1.Code,
                                              IsBankOrCash = t1.IsBankOrCash,
                                              VoucherTypeId = t1.VoucherTypeId,
                                              Name = t1.Name,
                                              IsActive = t1.IsActive
                                          }).FirstOrDefault());
            return v;
        }

        public async Task<long> AccountingJournalPushGCCL(DateTime journalDate, int companyFk, int drHeadId, List<AccountList> crHead, decimal amount, string title, string description, int journalType)
        {
            long result = -1;
            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,
                Title = title,
                Narration = description,
                CompanyFK = companyFk,
                Date = journalDate,
                IsSubmit = true
            };

            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();
            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = description,
                Debit = Convert.ToDouble(amount),
                Credit = 0,
                Accounting_HeadFK = drHeadId
            });
            foreach (var item in crHead)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = description,
                    Debit = 0,
                    Credit = Convert.ToDouble(item.Value),
                    Accounting_HeadFK = Convert.ToInt32(item.AccountingHeadId)
                });
            }
            var resultData = await AccountingJournalMasterPush(vMJournalSlave);
            return resultData.VoucherId;
        }

        public async Task<long> AccountingProductionPushGCCL(DateTime journalDate, int companyFk, VMProdReferenceSlave vmProdReferenceSlave, string title, string description, int journalType)
        {
            long result = -1;
            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,
                Title = title,
                Narration = description,
                CompanyFK = companyFk,
                Date = journalDate,
                IsSubmit = true
            };

            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();

            #region Raw Item Cr Integration Dr
            foreach (var item in vmProdReferenceSlave.RawDataListSlave)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.ProductName,
                    Debit = 0,
                    Credit = Convert.ToDouble(item.TotalPrice),
                    Accounting_HeadFK = item.AccountingHeadId.Value
                });
            }
            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = description,
                Debit = vmProdReferenceSlave.RawDataListSlave.Any() ? Convert.ToDouble(vmProdReferenceSlave.RawDataListSlave.Sum(x => x.TotalPrice)) : 0,
                Credit = 0,
                Accounting_HeadFK = 50606113 // ERP Raw Dr Integration
            });
            #endregion

            #region Production Manager Cr Factory Expenses Dr
            foreach (var item in vmProdReferenceSlave.DataListSlave)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.FactoryExpecsesHeadName,
                    Debit = Convert.ToDouble(item.FectoryExpensesAmount),
                    Credit = 0,
                    Accounting_HeadFK = item.FactoryExpensesHeadGLId.Value
                });
            }
            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = description,
                Debit = 0,
                Credit = vmProdReferenceSlave.DataListSlave.Any() ? Convert.ToDouble(vmProdReferenceSlave.DataListSlave.Sum(x => x.FectoryExpensesAmount)) : 0,
                Accounting_HeadFK = vmProdReferenceSlave.AdvanceHeadGLId.Value
            });


            #endregion

            #region Integration Cr Finish Item Dr
            foreach (var item in vmProdReferenceSlave.FinishDataListSlave)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.ProductName,
                    Debit = Convert.ToDouble(((item.Quantity + item.QuantityOver) - item.QuantityLess) * item.CostingPrice),
                    Credit = 0,
                    Accounting_HeadFK = item.AccountingHeadId.Value
                });
            }
            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = description,
                Debit = 0,
                Credit = vmProdReferenceSlave.FinishDataListSlave.Any() ? Convert.ToDouble(vmProdReferenceSlave.FinishDataListSlave.Sum(x => ((x.Quantity + x.QuantityOver) - x.QuantityLess) * x.CostingPrice)) : 0,
                Accounting_HeadFK = 50606113 // ERP Integration
            });


            #endregion

            var resultData = await AccountingJournalMasterPush(vMJournalSlave);
            return resultData.VoucherId;
        }

        public async Task<long> AccountingPurchasePushFeed(int companyFk, VMWarehousePOReceivingSlave vmWarehousePoReceivingSlave, int journalType)
        {
            long result = -1;


            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {

                JournalType = journalType,
                Title = "<a href='" + _urlInfo + "Report/GetPurchaseOrderTemplateReport?purchaseOrderId=" + vmWarehousePoReceivingSlave.Procurement_PurchaseOrderFk + "&EXPORT=EXPORT&reportType=PDF'>" + vmWarehousePoReceivingSlave.POCID + "</a>" + " Date: " + vmWarehousePoReceivingSlave.PODate.Value.ToShortDateString(),
                Narration = " MRR No: " + vmWarehousePoReceivingSlave.ChallanCID + " Challan No: " + vmWarehousePoReceivingSlave.Challan + " Date: " + vmWarehousePoReceivingSlave.ReceivedDate.ToShortDateString() + " Received By: " + vmWarehousePoReceivingSlave.EmployeeName,
                CompanyFK = companyFk,
                Date = vmWarehousePoReceivingSlave.ReceivedDate,
                IsSubmit = true
            };
            List<string> strList = new List<string>();
            foreach (var item in vmWarehousePoReceivingSlave.DataListSlave)
            {
                strList.Add(item.ProductCategory + " " + item.ProductSubCategory + " " + item.ProductName + " " + item.StockInQty + " " + item.StockInRate);
            }
            string particular = String.Join(", ", strList.ToArray());

            double totalAmount = vmWarehousePoReceivingSlave.DataListSlave.Any() ? Convert.ToDouble(vmWarehousePoReceivingSlave.DataListSlave.Sum(x => x.StockInQty * x.StockInRate)) : 0;
            double totalDeduction = vmWarehousePoReceivingSlave.DataListSlave.Any() ? Convert.ToDouble(vmWarehousePoReceivingSlave.DataListSlave.Sum(x => x.StockInQty * x.StockInRate * (x.Deduction / 100))) : 0;
            decimal truckFare = vmWarehousePoReceivingSlave.TruckFare;
            decimal labourBill = vmWarehousePoReceivingSlave.LabourBill;

            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();
            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = particular + " Amount: " + totalAmount + " Deduction: " + totalDeduction + " Truck Fare: " + truckFare + " labour Bill: " + labourBill,
                Debit = 0,
                Credit = totalAmount - (Convert.ToDouble(truckFare) + Convert.ToDouble(labourBill) + totalDeduction),
                Accounting_HeadFK = vmWarehousePoReceivingSlave.AccountingHeadId.Value //Supplier/ LC

            });
            foreach (var item in vmWarehousePoReceivingSlave.DataListSlave)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.ProductCategory + " " + item.ProductSubCategory + " " + item.ProductName + " " + item.StockInQty + " " + item.StockInRate + " Amount: " + totalAmount + " Deduction: " + totalDeduction + " Truck Fare: " + truckFare + " labour Bill: " + labourBill,
                    Debit = Convert.ToDouble(item.StockInQty * item.StockInRate),
                    Credit = 0,
                    Accounting_HeadFK = item.AccountingExpenseHeadId.Value
                });
            }
            foreach (var item in vmWarehousePoReceivingSlave.DataListSlave)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.ProductCategory + " " + item.ProductSubCategory + " " + item.ProductName + " " + item.StockInQty + " " + item.StockInRate + " Amount: " + totalAmount + " Deduction: " + totalDeduction + " Truck Fare: " + truckFare + " labour Bill: " + labourBill,
                    Debit = Convert.ToDouble(item.StockInQty * item.StockInRate),
                    Credit = 0,
                    Accounting_HeadFK = item.AccountingHeadId.Value,
                    IsVirtual = true

                });
            }
            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Adjust",
                Debit = 0,
                Credit = Convert.ToDouble(labourBill),
                Accounting_HeadFK = 50613304, //Labour Bill Payable (RM)
                IsVirtual = false

            });
            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Adjust",
                Debit = 0,
                Credit = Convert.ToDouble(truckFare),
                Accounting_HeadFK = 50613302, //Truck Fare Payable (RM)
                IsVirtual = false

            });

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Adjust",
                Debit = 0,
                Credit = totalAmount + totalDeduction,
                Accounting_HeadFK = 50609522, //Feed Stock Adjust With Erp Cr
                IsVirtual = true

            });
            var resultData = await AccountingJournalMasterPush(vMJournalSlave);
            var voucherMap = VoucherMapping(resultData.VoucherId, vmWarehousePoReceivingSlave.CompanyFK.Value, vmWarehousePoReceivingSlave.MaterialReceiveId, vmWarehousePoReceivingSlave.IntegratedFrom);

            return resultData.VoucherId;
        }

        public async Task<long> AccountingProductTpPricePushFeed(WareHouseProductPriceVm model, int journalType)
        {

            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,
                Title = "Product : " + model.ProductName + " Current Stock : " + model.StockQuantity,
                Narration = "Previous Date :" + model.PreviousPriceDate.ToShortDateString() + "[" + model.PreviousPrice + "] to Update Date: " + model.PriceUpdateDate.ToShortDateString() + "[" + model.UpdatePrice + "]",
                CompanyFK = model.CompanyId,
                Date = model.PriceUpdateDate,
                IsSubmit = true
            };

            double amount = Convert.ToDouble((model.StockQuantity * model.UpdatePrice) - (model.StockQuantity * model.PreviousPrice));

            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = model.ProductName,
                Debit = amount > 0 ? amount : 0,
                Credit = amount > 0 ? 0 : Math.Abs(amount),
                Accounting_HeadFK = model.AccountingHeadId.Value
            });

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Adjust",
                Debit = amount > 0 ? 0 : Math.Abs(amount),
                Credit = amount > 0 ? amount : 0,
                Accounting_HeadFK = 50609522 //Feed Stock Adjust With Erp Cr

            });

            var resultData = await AccountingJournalMasterPush(vMJournalSlave);
            var voucherMap = VoucherMapping(model.PriceId, model.CompanyId, model.PriceId, model.IntegratedFrom);

            return resultData.VoucherId;
        }

        public async Task<long> AccountingIssuePushFeed(int companyFk, WarehouseIssueSlaveVm wareHouseIssueSlave, int journalType)
        {
            long result = -1;


            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,
                Title = "<a href='" + _urlInfo + "Report/GetIssueReport?issueId=" + wareHouseIssueSlave.IssueId + "&EXPORT=EXPORT&reportType=PDF'>" + wareHouseIssueSlave.IssueNo + "</a>" + " Date: " + wareHouseIssueSlave.IssueDate.ToShortDateString(),
                Narration = wareHouseIssueSlave.IssueNo + " Date: " + wareHouseIssueSlave.IssueDate.ToShortDateString(),
                CompanyFK = companyFk,
                Date = wareHouseIssueSlave.IssueDate,
                IsSubmit = true
            };
            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();
            foreach (var item in wareHouseIssueSlave.DataListSlave)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.ProductDescription,
                    Debit = 0,
                    Credit = Convert.ToDouble(item.Quantity * item.UnitPrice),
                    Accounting_HeadFK = item.AccountingHeadId.Value
                });
            }

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Adjust",
                Debit = (wareHouseIssueSlave.DataListSlave?.Count() ?? 0) > 0 ? Convert.ToDouble(wareHouseIssueSlave.DataListSlave.Sum(x => x.Quantity * x.UnitPrice)) : 0,
                Credit = 0,
                Accounting_HeadFK = 50609522, //Feed Stock Adjust With Erp Cr
                IsVirtual = true

            });
            var resultData = await AccountingJournalMasterPush(vMJournalSlave);
            var voucherMap = VoucherMapping(result, wareHouseIssueSlave.CompanyId, wareHouseIssueSlave.IssueId, wareHouseIssueSlave.IntegratedFrom);

            return resultData.VoucherId;
        }

        public async Task<long> AccountingPurchasePushGCCL(int companyFk, VMWarehousePOReceivingSlave vmWarehousePoReceivingSlave, int journalType)
        {
            long result = -1;
            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,
                Title = "<a href='" + _urlInfo + "Report/GCCLPurchseInvoiceReport?companyId=" + companyFk + "&materialReceiveId=" + vmWarehousePoReceivingSlave.MaterialReceiveId + "&reportName=GCCLPurchaseInvoiceReports'>" + vmWarehousePoReceivingSlave.POCID + "</a>" + " Date: " + vmWarehousePoReceivingSlave.PODate.ToString(),
                Narration = vmWarehousePoReceivingSlave.Challan + " Date: " + vmWarehousePoReceivingSlave.ChallanDate.ToString(),
                CompanyFK = companyFk,
                Date = vmWarehousePoReceivingSlave.ChallanDate,
                IsSubmit = true
            };
            List<string> strList = new List<string>();
            foreach (var item in vmWarehousePoReceivingSlave.DataListSlave)
            {
                strList.Add(item.ProductDescription);
            }
            string particular = String.Join(", ", strList.ToArray());
            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();
            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = particular,
                Debit = 0,
                Credit = vmWarehousePoReceivingSlave.DataListSlave.Any() ? Convert.ToDouble(vmWarehousePoReceivingSlave.DataListSlave.Sum(x => x.ReceivedQuantity * x.PurchasingPrice)) : 0,
                Accounting_HeadFK = vmWarehousePoReceivingSlave.AccountingHeadId.Value //Supplier/ LC
            });
            foreach (var item in vmWarehousePoReceivingSlave.DataListSlave)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.ProductDescription,
                    Debit = Convert.ToDouble(item.ReceivedQuantity * item.PurchasingPrice),
                    Credit = 0,
                    Accounting_HeadFK = item.AccountingExpenseHeadId.Value
                });
            }
            foreach (var item in vmWarehousePoReceivingSlave.DataListSlave)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.ProductDescription,
                    Debit = Convert.ToDouble(item.ReceivedQuantity * item.PurchasingPrice),
                    Credit = 0,
                    Accounting_HeadFK = item.AccountingHeadId.Value
                });
            }

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Adjust",
                Debit = 0,
                Credit = vmWarehousePoReceivingSlave.DataListSlave.Any() ? Convert.ToDouble(vmWarehousePoReceivingSlave.DataListSlave.Sum(x => x.ReceivedQuantity * x.PurchasingPrice)) : 0,
                Accounting_HeadFK = 50606113 //GCCL Stock Adjust With Erp Cr
            });
            var resultData = await AccountingJournalMasterPush(vMJournalSlave);
            var voucherMap = VoucherMapping(result, vmWarehousePoReceivingSlave.CompanyFK.Value, vmWarehousePoReceivingSlave.MaterialReceiveId, vmWarehousePoReceivingSlave.IntegratedFrom);

            return resultData.VoucherId;
        }

        public async Task<long> AccountingPurchaseReturnPushGCCL(int companyFk, VMWarehousePOReceivingSlave vmWarehousePoReceivingSlave, int journalType)
        {
            long result = -1;
            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,
                Title = "<a href='" + _urlInfo + "Report/GCCLPurchseInvoiceReport?companyId=" + companyFk + "&materialReceiveId=" + vmWarehousePoReceivingSlave.MaterialReceiveId + "&reportName=GCCLPurchaseInvoiceReports'>" + vmWarehousePoReceivingSlave.POCID + "</a>" + " Date: " + vmWarehousePoReceivingSlave.PODate.ToString(),
                Narration = vmWarehousePoReceivingSlave.Challan + " Date: " + vmWarehousePoReceivingSlave.ChallanDate.ToString(),
                CompanyFK = companyFk,
                Date = vmWarehousePoReceivingSlave.ChallanDate,
                IsSubmit = true
            };
            List<string> strList = new List<string>();
            foreach (var item in vmWarehousePoReceivingSlave.DataListSlave)
            {
                strList.Add(item.ProductDescription);
            }
            string particular = String.Join(", ", strList.ToArray());
            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();
            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = particular,
                Debit = (vmWarehousePoReceivingSlave.DataListSlave?.Count() ?? 0) > 0 ? Convert.ToDouble(vmWarehousePoReceivingSlave.DataListSlave.Sum(x => x.ReturnQuantity * x.PurchasingPrice)) : 0,
                Credit = 0,
                Accounting_HeadFK = vmWarehousePoReceivingSlave.AccountingHeadId.Value //Supplier/ LC
            });
            foreach (var item in vmWarehousePoReceivingSlave.DataListSlave)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.ProductDescription,
                    Debit = 0,
                    Credit = Convert.ToDouble(item.ReturnQuantity * item.PurchasingPrice),
                    Accounting_HeadFK = item.AccountingExpenseHeadId.Value
                });
            }
            foreach (var item in vmWarehousePoReceivingSlave.DataListSlave)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.ProductDescription,
                    Debit = 0,
                    Credit = Convert.ToDouble(item.ReturnQuantity * item.PurchasingPrice),
                    Accounting_HeadFK = item.AccountingHeadId.Value
                });
            }

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Adjust",
                Debit = (vmWarehousePoReceivingSlave.DataListSlave?.Count() ?? 0) > 0 ? Convert
                .ToDouble(vmWarehousePoReceivingSlave.DataListSlave
                .Sum(x => x.ReturnQuantity * x.PurchasingPrice)) : 0,
                Credit = 0,
                Accounting_HeadFK = 50606113 //GCCL Stock Adjust With Erp Cr
            });
            var resultData = await AccountingJournalMasterPush(vMJournalSlave);
            var voucherMap = VoucherMapping(result, vmWarehousePoReceivingSlave.CompanyFK.Value, vmWarehousePoReceivingSlave.MaterialReceiveId, vmWarehousePoReceivingSlave.IntegratedFrom);

            return resultData.VoucherId;
        }

        public async Task<long> AccountingMaterialIssuePushFeed(int companyFk, VMWarehousePOReceivingSlave vmWarehousePoReceivingSlave, int journalType)
        {
            long result = -1;


            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,
                Title = vmWarehousePoReceivingSlave.POCID + " Date: " + vmWarehousePoReceivingSlave.PODate.ToString(),
                Narration = vmWarehousePoReceivingSlave.Challan + " Date: " + vmWarehousePoReceivingSlave.ChallanDate.ToString(),
                CompanyFK = companyFk,
                Date = vmWarehousePoReceivingSlave.ChallanDate,
                IsSubmit = true
            };
            List<string> strList = new List<string>();
            foreach (var item in vmWarehousePoReceivingSlave.DataListSlave)
            {
                strList.Add(item.ProductDescription);
            }
            string particular = String.Join(", ", strList.ToArray());
            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();
            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = particular,
                Debit = 0,
                Credit = (vmWarehousePoReceivingSlave.DataListSlave?.Count() ?? 0) > 0 ? Convert.ToDouble(vmWarehousePoReceivingSlave.DataListSlave.Sum(x => x.ReceivedQuantity * x.PurchasingPrice)) : 0,
                Accounting_HeadFK = vmWarehousePoReceivingSlave.AccountingHeadId.Value //Supplier/ LC
            });
            foreach (var item in vmWarehousePoReceivingSlave.DataListSlave)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.ProductDescription,
                    Debit = Convert.ToDouble(item.ReceivedQuantity * item.PurchasingPrice),
                    Credit = 0,
                    Accounting_HeadFK = item.AccountingExpenseHeadId.Value
                });
            }
            foreach (var item in vmWarehousePoReceivingSlave.DataListSlave)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.ProductDescription,
                    Debit = Convert.ToDouble(item.ReceivedQuantity * item.PurchasingPrice),
                    Credit = 0,
                    Accounting_HeadFK = item.AccountingHeadId.Value
                });
            }

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Adjust",
                Debit = 0,
                Credit = (vmWarehousePoReceivingSlave.DataListSlave?.Count() ?? 0) > 0 ? Convert.ToDouble(vmWarehousePoReceivingSlave.DataListSlave.Sum(x => x.ReceivedQuantity * x.PurchasingPrice)) : 0,
                Accounting_HeadFK = 43576 //Seed Stock Adjust With Erp Cr
            });
            var resultData = await AccountingJournalMasterPush(vMJournalSlave);
            return resultData.VoucherId;
        }

        public async Task<long> AccountingPurchasePushSEED(int companyFk, VMWarehousePOReceivingSlave vmWarehousePoReceivingSlave, int journalType)
        {
            long result = -1;


            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,
                Title = "<a href='" + _urlInfo + "Report/GCCLPurchseInvoiceReport?companyId=" + companyFk + "&materialReceiveId=" + vmWarehousePoReceivingSlave.MaterialReceiveId + "&reportName=GCCLPurchaseInvoiceReports'>" + vmWarehousePoReceivingSlave.POCID + "</a>" + " Date: " + vmWarehousePoReceivingSlave.PODate.ToString(),
                Narration = vmWarehousePoReceivingSlave.Challan + " Date: " + vmWarehousePoReceivingSlave.ChallanDate.ToString(),
                CompanyFK = companyFk,
                Date = vmWarehousePoReceivingSlave.ChallanDate,
                IsSubmit = true
            };

            List<string> strList = new List<string>();
            foreach (var item in vmWarehousePoReceivingSlave.DataListSlave)
            {
                strList.Add(item.ProductDescription);
            }

            string particular = String.Join(", ", strList.ToArray());
            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();
            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = particular,
                Debit = 0,
                Credit = (vmWarehousePoReceivingSlave.DataListSlave?.Count() ?? 0) > 0 ? Convert.ToDouble(vmWarehousePoReceivingSlave.DataListSlave.Sum(x => x.ReceivedQuantity * x.PurchasingPrice)) : 0,
                Accounting_HeadFK = vmWarehousePoReceivingSlave.AccountingHeadId.Value //Supplier/ LC
            });

            foreach (var item in vmWarehousePoReceivingSlave.DataListSlave)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.ProductDescription,
                    Debit = Convert.ToDouble(item.ReceivedQuantity * item.PurchasingPrice),
                    Credit = 0,
                    Accounting_HeadFK = item.AccountingExpenseHeadId.Value
                });
            }

            foreach (var item in vmWarehousePoReceivingSlave.DataListSlave)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.ProductDescription,
                    Debit = Convert.ToDouble(item.ReceivedQuantity * item.PurchasingPrice),
                    Credit = 0,
                    Accounting_HeadFK = item.AccountingHeadId.Value
                });
            }

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Adjust",
                Debit = 0,
                Credit = (vmWarehousePoReceivingSlave.DataListSlave?.Count() ?? 0) > 0 ? Convert.ToDouble(vmWarehousePoReceivingSlave.DataListSlave.Sum(x => x.ReceivedQuantity * x.PurchasingPrice)) : 0,
                Accounting_HeadFK = 43576 //Seed Stock Adjust With Erp Cr
            });

            var resultData = await AccountingJournalMasterPush(vMJournalSlave);
            var voucherMap = VoucherMapping(resultData.VoucherId, vmWarehousePoReceivingSlave.CompanyFK.Value, vmWarehousePoReceivingSlave.MaterialReceiveId, vmWarehousePoReceivingSlave.IntegratedFrom);

            return resultData.VoucherId;
        }

        public async Task<long> AccountingSalesPushGCCL(int companyFk, VMOrderDeliverDetail vmOrderDeliverDetail, int journalType)
        {
            long result = -1;

            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,
                Title = vmOrderDeliverDetail.OrderNo + " Date: " + vmOrderDeliverDetail.OrderDate.ToString("MM/dd/yyyy"),
                Narration = vmOrderDeliverDetail.ChallanNo + " Date: " + vmOrderDeliverDetail.DeliveryDate.Value.ToString("MM/dd/yyyy"),
                CompanyFK = companyFk,
                Date = vmOrderDeliverDetail.DeliveryDate,
                IsSubmit = true,

            };

            List<string> strList = new List<string>();
            foreach (var item in vmOrderDeliverDetail.DataListDetail)
            {
                string s = "Product: " + item.ProductCategory + " " + item.ProductSubCategory + " " + item.ProductName + "Delivered Qty: " + item.DeliveredQty + " Unit Price: " + item.UnitPrice + " Discount: " + item.Discount + " Special Discount " + vmOrderDeliverDetail.SpecialDiscount + " Payment Method: " + ((VendorsPaymentMethodEnum)vmOrderDeliverDetail.PaymentMethod).ToString();
                strList.Add(s);
            }

            string particular = String.Join(", ", strList.ToArray());
            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();
            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = particular,
                Debit = (vmOrderDeliverDetail.DataListDetail?.Count() ?? 0) > 0 ? Convert.ToDouble(vmOrderDeliverDetail.DataListDetail.Sum(x => (x.DeliveredQty * x.UnitPrice - Convert.ToDouble(x.Discount ?? 0))) - Convert.ToDouble(vmOrderDeliverDetail.SpecialDiscount ?? 0)) : 0,
                Credit = 0,
                Accounting_HeadFK = vmOrderDeliverDetail.AccountingHeadId.Value //Customer/ LC
            });

            int count = 1;
            foreach (var item in vmOrderDeliverDetail.DataListDetail)
            {

                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = "Delivered Qty: " + item.DeliveredQty + " Unit Price: " + item.UnitPrice + " Discount: " + item.Discount + " Special Discount " + vmOrderDeliverDetail.SpecialDiscount,
                    Debit = 0,
                    Credit = (item.DeliveredQty * item.UnitPrice),
                    Accounting_HeadFK = item.AccountingIncomeHeadId.Value
                });
                count++;
            }

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Sales Commission & Discount",
                Debit = (vmOrderDeliverDetail.DataListDetail?.Count() ?? 0) > 0 ? (vmOrderDeliverDetail.DataListDetail.Sum(x => Convert.ToDouble(x.Discount))) + (Convert.ToDouble(vmOrderDeliverDetail.SpecialDiscount ?? 0)) : 0,
                Credit = 0,
                Accounting_HeadFK = 39513 //Sales Commission & Discount

            });

            foreach (var item in vmOrderDeliverDetail.DataListDetail)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = "Delivered Qty: " + item.DeliveredQty + " Costing Price: " + item.COGSPrice,
                    Debit = 0,
                    Credit = item.DeliveredQty * Convert.ToDouble(item.COGSPrice),
                    Accounting_HeadFK = item.AccountingHeadId.Value,
                    IsVirtual = true

                });
            }

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Adjust",
                Debit = (vmOrderDeliverDetail.DataListDetail?.Count() ?? 0) > 0 ? vmOrderDeliverDetail.DataListDetail.Sum(x => x.DeliveredQty * Convert.ToDouble(x.COGSPrice)) : 0,
                Credit = 0,
                Accounting_HeadFK = 50606113, //GCCL Stock Adjust With Erp Cr
                IsVirtual = true

            });

            var resultData = await AccountingJournalMasterPush(vMJournalSlave);

            var voucherMap = VoucherMapping(result, vmOrderDeliverDetail.CompanyFK.Value, vmOrderDeliverDetail.OrderDeliverId, vmOrderDeliverDetail.IntegratedFrom);

            return resultData.VoucherId;
        }

        public async Task<long> AccountingSalesPushSEED(int companyFk, VMOrderDeliverDetail vmOrderDeliverDetail, int journalType)
        {
            long result = -1;

            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,

                Title = vmOrderDeliverDetail.OrderNo + " Date: " + vmOrderDeliverDetail.OrderDate.ToString(),
                Narration = vmOrderDeliverDetail.ChallanNo + " Date: " + vmOrderDeliverDetail.DeliveryDate.ToString() +
                "Crops Group: " + vmOrderDeliverDetail.DataListDetail.Select(x => x.ProductCategory).FirstOrDefault(),
                CompanyFK = companyFk,
                Date = vmOrderDeliverDetail.DeliveryDate,
                IsSubmit = true
            };

            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();
            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Customer",
                Debit = (vmOrderDeliverDetail.DataListDetail?.Count() ?? 0) > 0 ? Convert.ToDouble(vmOrderDeliverDetail.DataListDetail.Sum(x => (x.DeliveredQty * x.UnitPrice))) - Convert.ToDouble(vmOrderDeliverDetail.DataListDetail.Sum(x => x.Discount)) : 0,
                Credit = 0,
                Accounting_HeadFK = vmOrderDeliverDetail.AccountingHeadId.Value //Customer/ LC 
            });

            foreach (var item in vmOrderDeliverDetail.DataListDetail)
            {

                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.ProductCategory + " " + item.ProductSubCategory + " " + item.ProductName + " " + " Delivered Qty " + item.DeliveredQty + " Unit Price: " + item.UnitPrice + " Total Price" + item.DeliveredQty * item.UnitPrice,
                    Debit = 0,
                    Credit = (item.DeliveredQty * item.UnitPrice),
                    Accounting_HeadFK = item.AccountingIncomeHeadId.Value
                });

            }
            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Sales Commission",
                Debit = (vmOrderDeliverDetail.DataListDetail?.Count() ?? 0) > 0 ? Convert.ToDouble(vmOrderDeliverDetail.DataListDetail.Sum(x => x.Discount)) : 0,
                Credit = 0,
                Accounting_HeadFK = 34430
            });

            foreach (var item in vmOrderDeliverDetail.DataListDetail)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.ProductCategory + " " + item.ProductSubCategory + " " + item.ProductName + " " + "Delivered Qty: " + item.DeliveredQty + " COGS Price: " + item.COGSPrice + " Total Costing" + Convert.ToDecimal(item.DeliveredQty) * item.COGSPrice,
                    Debit = 0,
                    Credit = item.DeliveredQty * Convert.ToDouble(item.COGSPrice),
                    Accounting_HeadFK = item.AccountingHeadId.Value
                });
            }


            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Adjust",
                Debit = (vmOrderDeliverDetail.DataListDetail?.Count() ?? 0) > 0 ? vmOrderDeliverDetail.DataListDetail.Sum(x => x.DeliveredQty * Convert.ToDouble(x.COGSPrice)) : 0,
                Credit = 0,
                Accounting_HeadFK = 43576 //SEED Stock Adjust With Erp Dr
            });

            var resultData = await AccountingJournalMasterPush(vMJournalSlave);
            var voucherMap = VoucherMapping(result, vmOrderDeliverDetail.CompanyFK.Value, vmOrderDeliverDetail.MaterialReceiveId, vmOrderDeliverDetail.IntegratedFrom);

            return resultData.VoucherId;
        }

        public async Task<long> AccountingSalesPushFeed(int companyFk, VMOrderDeliverDetail vmOrderDeliverDetail, int journalType)
        {


            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,

                Title = vmOrderDeliverDetail.OrderNo + " Date: " + vmOrderDeliverDetail.OrderDate.ToString(),
                Narration = vmOrderDeliverDetail.ChallanNo + " Date: " + vmOrderDeliverDetail.DeliveryDate.ToString(),
                CompanyFK = companyFk,
                Date = vmOrderDeliverDetail.DeliveryDate,
                IsSubmit = true
            };

            List<string> strList = new List<string>();
            if ((vmOrderDeliverDetail.DataListDetail?.Count() ?? 0) > 0)
            {
                foreach (var item in vmOrderDeliverDetail.DataListDetail)
                {
                    string s = item.ProductCategory + " " + (item.ProductCategory != item.ProductSubCategory ? item.ProductSubCategory : "") + item.ProductName + " Quantity " + item.DeliveredQty + " Unit Price: " + item.UnitPrice + " Total Price" + item.DeliveredQty * item.UnitPrice;
                    strList.Add(s);
                }
            }

            string particular = String.Join(", ", strList.ToArray());
            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();
            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = particular,
                Debit = (vmOrderDeliverDetail.DataListDetail?.Count() ?? 0) > 0 ? Convert.ToDouble(vmOrderDeliverDetail.DataListDetail.Sum(item => (item.DeliveredQty * (item.UnitPrice + Convert.ToDouble(item.AdditionPrice))) - (item.DeliveredQty * Convert.ToDouble(item.EBaseCommission) + item.DeliveredQty * Convert.ToDouble(item.ECarryingCommission) + item.DeliveredQty * Convert.ToDouble(item.ECashCommission) + item.DeliveredQty * Convert.ToDouble(item.SpecialDiscount)))) : 0,
                Credit = 0,
                Accounting_HeadFK = vmOrderDeliverDetail.AccountingHeadId.Value //Customer/ LC
            });

            //vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            //{
            //    Particular = particular,
            //    Debit = (vmOrderDeliverDetail.DataListDetail?.Count() ?? 0) > ? Convert.ToDouble(vmOrderDeliverDetail.DataListDetail.Sum(item => item.DeliveredQty * Convert.ToDouble(item.EBaseCommission))) : 0,
            //    Credit = 0,
            //    Accounting_HeadFK = 50612377 //Feed Base Commission
            //});

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = particular,
                Debit = (vmOrderDeliverDetail.DataListDetail?.Count() ?? 0) > 0 ? Convert.ToDouble(vmOrderDeliverDetail.DataListDetail.Sum(item => item.DeliveredQty * Convert.ToDouble(item.ECarryingCommission))) : 0,
                Credit = 0,
                Accounting_HeadFK = 50610267 //Feed Carrying Commission 
            });

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = particular,
                Debit = (vmOrderDeliverDetail.DataListDetail?.Count() ?? 0) > 0 ? Convert.ToDouble(vmOrderDeliverDetail.DataListDetail.Sum(item => item.DeliveredQty * Convert.ToDouble(item.ECashCommission))) : 0,
                Credit = 0,
                Accounting_HeadFK = 50612378 //Feed Cash Commission
            });

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = particular,
                Debit = (vmOrderDeliverDetail.DataListDetail?.Count() ?? 0) > 0 ? Convert.ToDouble(vmOrderDeliverDetail.DataListDetail.Sum(item => item.DeliveredQty * Convert.ToDouble(item.SpecialDiscount))) : 0,
                Credit = 0,
                Accounting_HeadFK = 50612379 // Special Discount

            });


            foreach (var item in vmOrderDeliverDetail.DataListDetail)
            {

                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.ProductCategory + " " + item.ProductSubCategory + " " + item.ProductName + " " + " Delivered Qty " + item.DeliveredQty + " Unit Price: " + item.UnitPrice + " Total Price" + item.DeliveredQty * item.UnitPrice + " Base Commission: " + item.DeliveredQty * Convert.ToDouble(item.EBaseCommission) + " Carrying Commission: " + item.DeliveredQty * Convert.ToDouble(item.ECarryingCommission) + " Cash Commission: " + item.DeliveredQty * Convert.ToDouble(item.ECashCommission) + " Special Discount: " + item.DeliveredQty * Convert.ToDouble(item.SpecialDiscount),
                    Debit = 0,
                    Credit = (item.DeliveredQty * (item.UnitPrice + Convert.ToDouble(item.AdditionPrice))) - (item.DeliveredQty * Convert.ToDouble(item.EBaseCommission)),
                    Accounting_HeadFK = item.AccountingIncomeHeadId.Value
                });

            }

            foreach (var item in vmOrderDeliverDetail.DataListDetail)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.ProductCategory + " " + item.ProductSubCategory + " " + item.ProductName + " " + " Delivered Qty " + item.DeliveredQty + " Unit Price: " + item.UnitPrice + " Total Price" + item.DeliveredQty * item.UnitPrice + " Base Commission: " + item.DeliveredQty * Convert.ToDouble(item.EBaseCommission) + " Carrying Commission: " + item.DeliveredQty * Convert.ToDouble(item.ECarryingCommission) + " Cash Commission: " + item.DeliveredQty * Convert.ToDouble(item.ECashCommission) + " Special Discount: " + item.DeliveredQty * Convert.ToDouble(item.SpecialDiscount),
                    Debit = 0,
                    Credit = item.DeliveredQty * Convert.ToDouble(item.COGSPrice),
                    Accounting_HeadFK = item.AccountingHeadId.Value,
                    IsVirtual = true
                });
            }

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Adjust",
                Debit = (vmOrderDeliverDetail.DataListDetail?.Count() ?? 0) > 0 ? vmOrderDeliverDetail.DataListDetail.Sum(x => x.DeliveredQty * Convert.ToDouble(x.COGSPrice)) : 0,
                Credit = 0,
                Accounting_HeadFK = 50609522, //Feed Stock Adjust With Erp Dr
                IsVirtual = true
            });

            var resultData = await AccountingJournalMasterPush(vMJournalSlave);
            var voucherMap = VoucherMapping(resultData.VoucherId, vmOrderDeliverDetail.CompanyFK.Value, vmOrderDeliverDetail.OrderDeliverId, vmOrderDeliverDetail.IntegratedFrom);

            return resultData.VoucherId;
        }

        private bool VoucherMapping(long voucherId, int companyId, long integratedId, string integratedFrom)
        {
            var objectToSave = _db.VoucherMaps
               .SingleOrDefault(q => q.VoucherId == voucherId
               && q.IntegratedId == integratedId
               && q.CompanyId == companyId
               && q.IntegratedFrom == integratedFrom);


            if (objectToSave != null)
            {
                return false;
            }
            else
            {
                VoucherMap voucherMap = new VoucherMap();
                voucherMap.VoucherId = voucherId;
                voucherMap.IntegratedId = integratedId;
                voucherMap.CompanyId = companyId;
                voucherMap.IntegratedFrom = integratedFrom;

                _db.VoucherMaps.Add(voucherMap);

                return _db.SaveChanges() > 0;

            }

        }

        public async Task<long> AccountingStockAdjustPushSEED(int companyFk, VMStockAdjustDetail vmStockAdjust, int journalType)
        {
            long result = -1;


            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,
                Title = vmStockAdjust.InvoiceNo + " Date: " + vmStockAdjust.AdjustDate.ToString(),
                Narration = vmStockAdjust.InvoiceNo + " Date: " + vmStockAdjust.AdjustDate.ToString(),
                CompanyFK = companyFk,
                Date = vmStockAdjust.AdjustDate,
                IsSubmit = true
            };


            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();

            foreach (var item in vmStockAdjust.DataListSlave)
            {

                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.LessQty > 0 ? "Less Qty " + item.LessQty + " Unit Price: " + item.UnitPrice + " Total Price: " + item.LessQty * item.UnitPrice : item.ExcessQty > 0 ? "Excess Qty " + item.ExcessQty + " Unit Price: " + item.UnitPrice + " Total Price: " + item.ExcessQty * item.UnitPrice : "",
                    Debit = Convert.ToDouble(item.ExcessQty > 0 ? item.ExcessQty * item.UnitPrice : 0),
                    Credit = Convert.ToDouble(item.LessQty > 0 ? item.LessQty * item.UnitPrice : 0),
                    Accounting_HeadFK = item.AccountingHeadId.Value
                });

            }
            foreach (var item in vmStockAdjust.DataListSlave)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.LessQty > 0 ? "Less Qty " + item.LessQty + " Unit Price: " + item.UnitPrice + " Total Price: " + item.LessQty * item.UnitPrice : item.ExcessQty > 0 ? "Excess Qty " + item.ExcessQty + " Unit Price: " + item.UnitPrice + " Total Price: " + item.ExcessQty * item.UnitPrice : "",
                    Debit = Convert.ToDouble(item.LessQty > 0 ? item.LessQty * item.UnitPrice : 0),
                    Credit = Convert.ToDouble(item.ExcessQty > 0 ? item.ExcessQty * item.UnitPrice : 0),
                    Accounting_HeadFK = 43576
                });
            }

            var voucherMap = VoucherMapping(result, vmStockAdjust.CompanyFK.Value, vmStockAdjust.StockAdjustId, vmStockAdjust.IntegratedFrom);
            var resultData = await AccountingJournalMasterPush(vMJournalSlave);
            return resultData.VoucherId;
        }

        public async Task<long> AccountingStockAdjustPushFeed(int companyFk, VMStockAdjustDetail vmStockAdjust, int journalType)
        {
            long result = -1;


            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,
                Title = vmStockAdjust.InvoiceNo,
                Narration = " Date: " + vmStockAdjust.AdjustDate.ToString(),
                CompanyFK = companyFk,
                Date = vmStockAdjust.AdjustDate,
                IsSubmit = true
            };


            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();

            foreach (var item in vmStockAdjust.DataListSlave)
            {

                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.LessQty > 0 ? "Less Qty " + item.LessQty + " Unit Price: " + item.UnitPrice + " Total Price: " + item.LessQty * item.UnitPrice : item.ExcessQty > 0 ? "Excess Qty " + item.ExcessQty + " Unit Price: " + item.UnitPrice + " Total Price: " + item.ExcessQty * item.UnitPrice : "",
                    Debit = Convert.ToDouble(item.ExcessQty > 0 ? item.ExcessQty * item.UnitPrice : 0),
                    Credit = Convert.ToDouble(item.LessQty > 0 ? item.LessQty * item.UnitPrice : 0),
                    Accounting_HeadFK = item.AccountingHeadId.Value
                });

            }
            foreach (var item in vmStockAdjust.DataListSlave)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.LessQty > 0 ? "Less Qty " + item.LessQty + " Unit Price: " + item.UnitPrice + " Total Price: " + item.LessQty * item.UnitPrice : item.ExcessQty > 0 ? "Excess Qty " + item.ExcessQty + " Unit Price: " + item.UnitPrice + " Total Price: " + item.ExcessQty * item.UnitPrice : "",
                    Debit = Convert.ToDouble(item.LessQty > 0 ? item.LessQty * item.UnitPrice : 0),
                    Credit = Convert.ToDouble(item.ExcessQty > 0 ? item.ExcessQty * item.UnitPrice : 0),
                    Accounting_HeadFK = 50609522
                });
            }

            var voucherMap = VoucherMapping(result, vmStockAdjust.CompanyFK.Value, vmStockAdjust.StockAdjustId, vmStockAdjust.IntegratedFrom);
            var resultData = await AccountingJournalMasterPush(vMJournalSlave);
            return resultData.VoucherId;
        }

        public async Task<long> AccountingStockAdjustPushGCCL(int companyFk, VMStockAdjustDetail vmStockAdjust, int journalType)
        {
            long result = -1;


            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,
                Title = vmStockAdjust.InvoiceNo + " Date: " + vmStockAdjust.AdjustDate.ToString(),
                Narration = vmStockAdjust.InvoiceNo + " Date: " + vmStockAdjust.AdjustDate.ToString(),
                CompanyFK = companyFk,
                Date = vmStockAdjust.AdjustDate,
                IsSubmit = true
            };


            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();

            foreach (var item in vmStockAdjust.DataListSlave)
            {

                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.LessQty > 0 ? "Less Qty " + item.LessQty + " Unit Price: " + item.UnitPrice + " Total Price: " + item.LessQty * item.UnitPrice : item.ExcessQty > 0 ? "Excess Qty " + item.ExcessQty + " Unit Price: " + item.UnitPrice + " Total Price: " + item.ExcessQty * item.UnitPrice : "",
                    Debit = Convert.ToDouble(item.ExcessQty > 0 ? item.ExcessQty * item.UnitPrice : 0),
                    Credit = Convert.ToDouble(item.LessQty > 0 ? item.LessQty * item.UnitPrice : 0),
                    Accounting_HeadFK = item.AccountingHeadId.Value
                });

            }
            foreach (var item in vmStockAdjust.DataListSlave)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.LessQty > 0 ? "Less Qty " + item.LessQty + " Unit Price: " + item.UnitPrice + " Total Price: " + item.LessQty * item.UnitPrice : item.ExcessQty > 0 ? "Excess Qty " + item.ExcessQty + " Unit Price: " + item.UnitPrice + " Total Price: " + item.ExcessQty * item.UnitPrice : "",
                    Debit = Convert.ToDouble(item.LessQty > 0 ? item.LessQty * item.UnitPrice : 0),
                    Credit = Convert.ToDouble(item.ExcessQty > 0 ? item.ExcessQty * item.UnitPrice : 0),
                    Accounting_HeadFK = 50606113
                });
            }


            var voucherMap = VoucherMapping(result, vmStockAdjust.CompanyFK.Value, vmStockAdjust.StockAdjustId, vmStockAdjust.IntegratedFrom);

            var resultData = await AccountingJournalMasterPush(vMJournalSlave);
            return resultData.VoucherId;
        }


        public async Task<long> CullectionPushGCCL(int companyFk, VMPayment vmPayment, int journalType)
        {
            long result = -1;
            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,
                Title = "Collection No: " + vmPayment.PaymentNo,
                Narration = "Date: " + vmPayment.TransactionDate.ToShortDateString(),
                CompanyFK = companyFk,
                Date = vmPayment.TransactionDate,
                IsSubmit = true
            };

            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();

            #region Raw Item Cr Integration Dr
            List<string> strList = new List<string>();
            foreach (var item in vmPayment.DataList)
            {
                string s = "Invoice No: " + item.OrderNo + " Date: " + item.OrderDate.ToShortDateString();
                strList.Add(s);
            }
            string particular = String.Join(", ", strList.ToArray());
            if ((vmPayment.DataList?.Count() ?? 0) > 0)
            {
                foreach (var item in vmPayment.DataList)
                {
                    vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                    {
                        Particular = "Invoice No: " + item.OrderNo + " Date: " + item.OrderDate.ToShortDateString() + " Money Receipt No" + item.MoneyReceiptNo + " MR Date: " + item.TransactionDate.ToShortDateString() + " Reference: " + item.ReferenceNo,
                        Debit = 0,
                        Credit = Convert.ToDouble(item.InAmount),
                        Accounting_HeadFK = item.PaymentFromHeadGLId.Value
                    });
                }
            }


            if (vmPayment.PaymentToHeadGLId != null)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = particular,
                    Debit = (Convert.ToDouble(vmPayment.DataList.Sum(x => x.InAmount)) + (vmPayment.DataListIncome != null ? Convert.ToDouble(vmPayment.DataListIncome.Sum(x => x.OthersIncomeAmount)) : 0)) - (Convert.ToDouble(vmPayment.BankCharge + (vmPayment.DataListExpenses != null ? vmPayment.DataListExpenses.Sum(x => x.ExpensesAmount) : 0))),
                    Credit = 0,
                    Accounting_HeadFK = vmPayment.PaymentToHeadGLId.Value
                });
            }
            if ((vmPayment.DataListExpenses?.Count() ?? 0) > 0)
            {
                foreach (var item in vmPayment.DataListExpenses)
                {
                    vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                    {
                        Particular = item.ExpensessReference,
                        Debit = Convert.ToDouble(item.ExpensesAmount),
                        Credit = 0,
                        Accounting_HeadFK = item.ExpensesHeadGLId.Value
                    });


                }
            }

            if ((vmPayment.DataListIncome?.Count() ?? 0) > 0)
            {
                foreach (var item in vmPayment.DataListIncome)
                {
                    vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                    {
                        Particular = item.IncomeReference,
                        Debit = 0,
                        Credit = Convert.ToDouble(item.OthersIncomeAmount),
                        Accounting_HeadFK = item.OthersIncomeHeadGLId.Value
                    });


                }
            }

            if (vmPayment.BankCharge > 0)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = "",
                    Debit = Convert.ToDouble(vmPayment.BankCharge),
                    Credit = 0,
                    Accounting_HeadFK = vmPayment.BankChargeHeadGLId.Value
                });
            }

            #endregion

            var resultData = await AccountingJournalMasterPush(vMJournalSlave);
            //await SMSPush(resultData);
            return resultData.VoucherId;
        }


        public async Task<long> PaymentPushGCCL(int companyFk, VMPayment vmPayment, int journalType)
        {
            long result = -1;
            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,
                Title = "Payment No: " + vmPayment.PaymentNo + " Reference: " + vmPayment.ReferenceNo,
                Narration = "Date: " + vmPayment.TransactionDate.ToShortDateString()
                + " A/C Name: " + vmPayment.ACName + " A/C No: " + vmPayment.ACNo + " Bank Name: " + vmPayment.BankName +
                " Branch Name: " + vmPayment.BranchName
                ,
                ChqDate = vmPayment.MoneyReceiptDate,
                ChqName = vmPayment.MoneyReceiptName,
                ChqNo = vmPayment.MoneyReceiptNo,
                CompanyFK = companyFk,
                Date = vmPayment.TransactionDate,
                IsSubmit = true
            };

            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();

            #region Raw Item Cr Integration Dr
            List<string> strList = new List<string>();
            foreach (var item in vmPayment.DataList)
            {
                string s = "Purchase Order No: " + item.OrderNo + " Date: " + item.OrderDate.ToShortDateString();
                strList.Add(s);
            }
            string particular = String.Join(", ", strList.ToArray());
            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = particular,
                Debit = 0,
                Credit = Convert.ToDouble(vmPayment.DataList.Sum(x => x.OutAmount)) + Convert.ToDouble(vmPayment.BankCharge),
                Accounting_HeadFK = vmPayment.PaymentFromHeadGLId.Value
            });
            if (vmPayment.BankCharge > 0)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = particular,
                    Debit = Convert.ToDouble(vmPayment.BankCharge),
                    Credit = 0,
                    Accounting_HeadFK = vmPayment.BankChargeHeadGLId.Value
                });
            }

            foreach (var item in vmPayment.DataList)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = particular,
                    Debit = Convert.ToDouble(item.OutAmount.Value),
                    Credit = 0,
                    Accounting_HeadFK = item.PaymentToHeadGLId.Value
                });
            }

            #endregion




            var resultData = await AccountingJournalMasterPush(vMJournalSlave);
            return resultData.VoucherId;
        }
        public async Task<long> AccountingSalesReturnPushGCCL(int companyFk, VMSaleReturnDetail vmSaleReturnDetail, int journalType)
        {
            long result = -1;



            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,

                Title = vmSaleReturnDetail.SaleReturnNo + " Date: " + vmSaleReturnDetail.ReturnDate.ToString(),
                Narration = vmSaleReturnDetail.Reason,
                CompanyFK = companyFk,
                Date = vmSaleReturnDetail.ReturnDate,
                IsSubmit = true
            };

            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();
            List<string> strList = new List<string>();
            foreach (var item in vmSaleReturnDetail.DataListDetail)
            {
                strList.Add(item.ProductName + " Return Qty: " + item.Qty + " Price: " + item.Rate);
            }
            string particular = String.Join(", ", strList.ToArray());

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = particular,
                Debit = 0,
                Credit = (vmSaleReturnDetail.DataListDetail?.Count() ?? 0) > 0 ? Convert.ToDouble(vmSaleReturnDetail.DataListDetail.Sum(x => (Convert.ToDouble(x.Qty.Value * x.Rate.Value)))) : 0,
                Accounting_HeadFK = vmSaleReturnDetail.AccountingHeadId.Value //Customer
            });

            foreach (var item in vmSaleReturnDetail.DataListDetail)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.ProductName + " Return Qty: " + item.Qty + " Price: " + item.Rate,
                    Debit = Convert.ToDouble(item.Qty.Value * item.Rate.Value),
                    Credit = 0,
                    Accounting_HeadFK = item.AccountingIncomeHeadId.Value
                });
            }
            foreach (var item in vmSaleReturnDetail.DataListDetail)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.ProductName + " Return Qty: " + item.Qty + " Costing Price: " + item.COGSRate,
                    Debit = Convert.ToDouble(item.Qty.Value * item.COGSRate.Value),
                    Credit = 0,
                    Accounting_HeadFK = item.AccountingHeadId.Value
                });
            }
            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Adjust",
                Debit = 0,
                Credit = (vmSaleReturnDetail.DataListDetail?.Count() ?? 0) > 0 ? Convert.ToDouble(vmSaleReturnDetail.DataListDetail.Sum(x => (Convert.ToDouble(x.Qty.Value * x.COGSRate.Value)))) : 0,
                Accounting_HeadFK = 50606113 //GCCL Stock Adjust With Erp Cr
            });
            var resultData = await AccountingJournalMasterPush(vMJournalSlave);

            var voucherMap = VoucherMapping(result, vmSaleReturnDetail.CompanyFK.Value, vmSaleReturnDetail.SaleReturnId, vmSaleReturnDetail.IntegratedFrom);

            return resultData.VoucherId;
        }

        public async Task<long> AccountingPurchaseReturnPushFeed(int companyFk, PurchaseReturnnewViewModel purchaseModel, int journalType)
        {
            long result = -1;


            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,

                Title = purchaseModel.ReturnNo + " Date: " + purchaseModel.ReturnDate.ToString() + " Reason: " + purchaseModel.ReturnReason,
                Narration = "Return By: " + purchaseModel.ReturnBy,
                CompanyFK = companyFk,
                Date = purchaseModel.ReturnDate,
                IsSubmit = true
            };

            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();
            List<string> strList = new List<string>();
            foreach (var item in purchaseModel.PurchaseReturnDetailItem)
            {
                strList.Add(item.ProductName + " Return Qty: " + item.Qty + " Price: " + item.Rate);
            }
            string particular = String.Join(", ", strList.ToArray());

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = particular,
                Debit = (purchaseModel.PurchaseReturnDetailItem?.Count() ?? 0) > 0 ? Convert.ToDouble(purchaseModel.PurchaseReturnDetailItem.Sum(x => (Convert.ToDouble(x.Qty.Value * x.Rate.Value)))) : 0,
                Credit = 0,
                Accounting_HeadFK = purchaseModel.AccoutHeadId.Value //Supplier
            });

            foreach (var item in purchaseModel.PurchaseReturnDetailItem)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = "Return Qty: " + item.Qty + " Price: " + item.COGS,
                    Debit = 0,
                    Credit = Convert.ToDouble(item.Qty.Value * item.COGS.Value),
                    Accounting_HeadFK = item.AccountingHeadId.Value
                });
            }
            foreach (var item in purchaseModel.PurchaseReturnDetailItem)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = "Return Qty: " + item.Qty + " Costing Price: " + item.Rate,
                    Debit = 0,
                    Credit = Convert.ToDouble(item.Qty.Value * item.Rate.Value),
                    Accounting_HeadFK = item.AccountingExpenseHeadId.Value
                });
            }
            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Adjust",
                Debit = (purchaseModel.PurchaseReturnDetailItem?.Count() ?? 0) > 0 ? Convert.ToDouble(purchaseModel.PurchaseReturnDetailItem.Sum(x => (Convert.ToDouble(x.Qty.Value * x.COGS.Value)))) : 0,
                Credit = 0,
                Accounting_HeadFK = 50609522 //Feed Stock Adjust With Erp Dr
            });
            var resultData = await AccountingJournalMasterPush(vMJournalSlave);
            var voucherMap = VoucherMapping(result, purchaseModel.CompanyId, purchaseModel.PurchaseReturnId, purchaseModel.IntegratedFrom);

            return resultData.VoucherId;
        }
        public async Task<long> AccountingSalesReturnPushSeed(int companyFk, VMSaleReturnDetail vmSaleReturnDetail, int journalType)
        {
            long result = -1;
            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,

                Title = vmSaleReturnDetail.SaleReturnNo + " Date: " + vmSaleReturnDetail.ReturnDate.ToString() + " Reason: " + vmSaleReturnDetail.Reason,
                Narration = vmSaleReturnDetail.Reason,
                CompanyFK = companyFk,
                Date = vmSaleReturnDetail.ReturnDate,
                IsSubmit = true
            };

            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();
            List<string> strList = new List<string>();
            foreach (var item in vmSaleReturnDetail.DataListDetail)
            {
                strList.Add(item.ProductName + " Return Qty: " + item.Qty + " Price: " + item.Rate);
            }
            string particular = String.Join(", ", strList.ToArray());

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = particular,
                Debit = 0,
                Credit = (vmSaleReturnDetail.DataListDetail?.Count() ?? 0) > 0 ? Convert.ToDouble(vmSaleReturnDetail.DataListDetail.Sum(x => (Convert.ToDouble(x.Qty.Value * x.Rate.Value)))) : 0,
                Accounting_HeadFK = vmSaleReturnDetail.AccountingHeadId.Value //Customer
            });

            foreach (var item in vmSaleReturnDetail.DataListDetail)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = "Return Qty: " + item.Qty + " Price: " + item.Rate,
                    Debit = Convert.ToDouble(item.Qty.Value * item.Rate.Value),
                    Credit = 0,
                    Accounting_HeadFK = item.AccountingIncomeHeadId.Value
                });
            }
            foreach (var item in vmSaleReturnDetail.DataListDetail)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = "Return Qty: " + item.Qty + " Costing Price: " + item.COGSRate,
                    Debit = Convert.ToDouble(item.Qty.Value * item.COGSRate.Value),
                    Credit = 0,
                    Accounting_HeadFK = item.AccountingHeadId.Value
                });
            }
            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Adjust",
                Debit = 0,
                Credit = (vmSaleReturnDetail.DataListDetail?.Count() ?? 0) > 0 ? Convert.ToDouble(vmSaleReturnDetail.DataListDetail.Sum(x => (Convert.ToDouble(x.Qty.Value * x.COGSRate.Value)))) : 0,
                Accounting_HeadFK = 43576 //Seed Stock Adjust With Erp Cr
            });
            var resultData = await AccountingJournalMasterPush(vMJournalSlave);
            var voucherMap = VoucherMapping(result, vmSaleReturnDetail.CompanyFK.Value, vmSaleReturnDetail.SaleReturnId, vmSaleReturnDetail.IntegratedFrom);

            return resultData.VoucherId;
        }

        public async Task<long> AccountingSalesReturnPushFeed(int companyFk, VMSaleReturnDetail vmSaleReturnDetail, int journalType)
        {
            long result = -1;
            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,

                Title = vmSaleReturnDetail.SaleReturnNo + " Date: " + vmSaleReturnDetail.ReturnDate.ToString(),
                Narration = "Reason: " + vmSaleReturnDetail.Reason,
                CompanyFK = companyFk,
                Date = vmSaleReturnDetail.ReturnDate,
                IsSubmit = true
            };

            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();
            List<string> strList = new List<string>();
            foreach (var item in vmSaleReturnDetail.DataListDetail)
            {
                strList.Add(item.ProductName + " Return Qty: " + item.Qty + " Price: " + item.Rate);
            }
            string particular = String.Join(", ", strList.ToArray());

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = particular,
                Debit = 0,
                Credit = (vmSaleReturnDetail.DataListDetail?.Count() ?? 0) > 0 ? Convert.ToDouble(vmSaleReturnDetail.DataListDetail.Sum(x => (Convert.ToDouble(x.Qty.Value * ((x.Rate.Value + x.AdditionPrice) - (x.CashCommission + x.BaseCommission + x.CarryingCommission + x.SpecialDiscount)))))) : 0,
                Accounting_HeadFK = vmSaleReturnDetail.AccountingHeadId.Value //Customer
            });

            foreach (var item in vmSaleReturnDetail.DataListDetail)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = "Return Qty: " + item.Qty + " Price: " + item.Rate,
                    Debit = (Convert.ToDouble(item.Qty.Value * ((item.Rate.Value + item.AdditionPrice) - (item.CashCommission + item.BaseCommission + item.CarryingCommission + item.SpecialDiscount)))),
                    Credit = 0,
                    Accounting_HeadFK = item.AccountingIncomeHeadId.Value
                });
            }
            foreach (var item in vmSaleReturnDetail.DataListDetail)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = "Return Qty: " + item.Qty + " Costing Price: " + item.COGSRate,
                    Debit = Convert.ToDouble(item.Qty.Value * item.COGSRate.Value),
                    Credit = 0,
                    Accounting_HeadFK = item.AccountingHeadId.Value,
                    IsVirtual = true
                });
            }
            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Adjust",
                Debit = 0,
                Credit = (vmSaleReturnDetail.DataListDetail?.Count() ?? 0) > 0 ? Convert.ToDouble(vmSaleReturnDetail.DataListDetail.Sum(x => (Convert.ToDouble(x.Qty.Value * x.COGSRate.Value)))) : 0,
                Accounting_HeadFK = 50609522, //Feed Stock Adjust With Erp Cr
                IsVirtual = true
            });
            var resultData = await AccountingJournalMasterPush(vMJournalSlave);
            var voucherMap = VoucherMapping(resultData.VoucherId, companyFk, vmSaleReturnDetail.SaleReturnId, vmSaleReturnDetail.IntegratedFrom);

            return resultData.VoucherId;
        }
        public async Task<long> AccountingFeedPurchasePushFeed(int companyFk, VMStoreDetail vMStoreDetail, int journalType)
        {
            long result = -1;
            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,

                Title = vMStoreDetail.ReceivedCode + " Date: " + vMStoreDetail.ReceivedDate.ToString(),
                Narration = "Remarks: " + vMStoreDetail.Remarks,
                CompanyFK = companyFk,
                Date = vMStoreDetail.ReceivedDate,
                IsActive = true,
                IsSubmit = true
            };

            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();
            List<string> strList = new List<string>();
            foreach (var item in vMStoreDetail.DataListDetail)
            {
                strList.Add(item.ProductName + " Purchase Qty: " + item.Qty + " Unit Price: " + item.UnitPrice);
            }
            string particular = "";

            if (strList.Count() > 1)
            {
                particular = String.Join(", ", strList.ToArray());
            }
            particular = strList.FirstOrDefault();

            var vjs = new VMJournalSlave();
            vjs.Particular = particular;
            vjs.Debit = 0;
            vjs.Credit = (vMStoreDetail.DataListDetail?.Count() ?? 0) > 0 ? Convert.ToDouble(vMStoreDetail.DataListDetail.Sum(x => (x.Qty.Value * Convert.ToDouble(x.UnitPrice.Value)))) : 0;
            vjs.Accounting_HeadFK = vMStoreDetail.AccountingHeadId ?? 0; // Supplier

            vMJournalSlave.DataListSlave.Add(vjs);

            foreach (var item in vMStoreDetail.DataListDetail)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = "Purchase Qty: " + item.Qty + "Unit Price: " + item.UnitPrice,
                    Debit = item.Qty.Value * Convert.ToDouble(item.UnitPrice),
                    Credit = 0,
                    Accounting_HeadFK = item.AccountingHeadId.Value, // Stock Addition
                    IsVirtual = true,
                });
            }

            foreach (var item in vMStoreDetail.DataListDetail)
            {
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = "Purchase Qty: " + item.Qty + "Unit Price: " + item.UnitPrice,
                    Debit = item.Qty.Value * Convert.ToDouble(item.UnitPrice.Value),
                    Credit = 0,
                    Accounting_HeadFK = item.AccountingExpenseHeadId.Value
                });
            }

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Adjust",
                Debit = 0,
                Credit = (vMStoreDetail.DataListDetail?.Count() ?? 0) > 0 ? Convert.ToDouble(vMStoreDetail.DataListDetail.Sum(x => (x.Qty.Value * Convert.ToDouble(x.UnitPrice)))) : 0,
                Accounting_HeadFK = 50609522, //Seed Stock Adjust With Erp Cr
                IsVirtual = true
            });
            var resultData = await AccountingJournalMasterPush(vMJournalSlave);
            return resultData.VoucherId;
        }
        public async Task<long> AccountingProductionPushFeed(int companyFk, RequisitionModel requisitionModel, int journalType)
        {

            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,

                Title = "<a href='" + _urlInfo + "Report/GetRMDeliverReport?requisitionId=" + requisitionModel.RequisitionId + "'>" + requisitionModel.RequisitionNo + "</a>" + " Date: " + requisitionModel.RequisitionDate.ToString(),
                Narration = requisitionModel.DeliveryNo + " Date: " + requisitionModel.DeliveredDate.ToString(),
                CompanyFK = companyFk,
                Date = requisitionModel.RequisitionDate,
                IsSubmit = true
            };

            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();

            // Raw Meterials 
            foreach (var item in requisitionModel.RequisitionItemDetailDataList)
            {

                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.RProductName + " RM Qty " + item.RTotalQty + " Unit Price: " + item.RUnitPrice + " Total Price" + item.RTotalQty * item.RUnitPrice,
                    Debit = 0,
                    Credit = Convert.ToDouble(item.RTotalQty.Value * item.RUnitPrice),
                    Accounting_HeadFK = item.AccountingHeadId.Value
                });

            }
            // Bag
            foreach (var item in requisitionModel.BagDataList)
            {

                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.ProductName + " Bag Qty " + item.BagQty + " Unit Price: " + item.BagUnitPrice + " Total Price" + item.BagQty * item.BagUnitPrice,
                    Debit = 0,
                    Credit = Convert.ToDouble(item.BagQty * item.BagUnitPrice),
                    Accounting_HeadFK = item.AccountingHeadId.Value
                });

            }

            // Bag
            foreach (var item in requisitionModel.RequisitionItemDataList)
            {

                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = item.ProductName + " Production Qty " + item.OutputQty + " COGS: " + item.TPPrice + " Total Price" + item.OutputQty * item.TPPrice,
                    Debit = Convert.ToDouble(item.OutputQty * item.TPPrice),
                    Credit = 0,
                    Accounting_HeadFK = item.AccountingHeadId.Value
                });

            }

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Adjust",
                Debit = 0,
                Credit = (requisitionModel.RequisitionItemDataList?.Count() ?? 0) > 0 ? requisitionModel.RequisitionItemDataList.Sum(x => Convert.ToDouble(x.OutputQty * x.TPPrice)) : 0,
                Accounting_HeadFK = 50609522 //Feed Stock Adjust With Erp Dr
            });

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Adjust",
                Debit = (requisitionModel.RequisitionItemDetailDataList?.Count() ?? 0) > 0 ? requisitionModel.RequisitionItemDetailDataList.Sum(x => Convert.ToDouble(x.RTotalQty * x.RUnitPrice)) : 0,
                Credit = 0,
                Accounting_HeadFK = 50609522 //Feed Stock Adjust With Erp Dr
            });

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Adjust",
                Debit = (requisitionModel.BagDataList?.Count() ?? 0) > 0 ? requisitionModel.BagDataList.Sum(x => Convert.ToDouble(x.BagQty * x.BagUnitPrice)) : 0,
                Credit = 0,
                Accounting_HeadFK = 50609522 //Feed Stock Adjust With Erp Dr
            });


            var resultData = await AccountingJournalMasterPush(vMJournalSlave);


            #region Voucher Maps
            if (resultData != null)
            {
                VoucherMap voucherMap = new VoucherMap();
                voucherMap.VoucherId = resultData.VoucherId;
                voucherMap.IntegratedId = requisitionModel.RequisitionId;
                voucherMap.CompanyId = requisitionModel.CompanyId ?? 0;
                voucherMap.IntegratedFrom = requisitionModel.IntegratedFrom;

                _db.VoucherMaps.Add(voucherMap);
                _db.SaveChanges();
            }

            #endregion


            return resultData.VoucherId;
        }

        public async Task<int> FeedOrderDeliverySMSPush(VMOrderDeliverDetail vmOrderDeliverDetail)
        {
            if (vmOrderDeliverDetail.CompanyFK == (int)CompanyName.KrishibidFeedLimited)
            {
                List<string> strList = new List<string>();
                double qty = 0;
                foreach (var item in vmOrderDeliverDetail.DataListDetail)
                {
                    qty += item.DeliveredQty;
                    strList.Add(item.ProductCategory + " " + item.ProductSubCategory + " " + item.ProductName + " Delivered Qty " + item.DeliveredQty + " Unit Price: " + item.UnitPrice);
                }
                string items = String.Join(", ", strList.ToArray());
                if (vmOrderDeliverDetail.CustomerPhone != null)
                {
                    ErpSMS erpSMS = new ErpSMS
                    {


                        Message = "Dear Valued Customer - " + vmOrderDeliverDetail.CustomerName + ", your order has been successfully delivered from Krishibid Feed Ltd. " +
                               "\r\n\r\n" +
                              " Challan No: " + vmOrderDeliverDetail.ChallanNo +
                              " Invoice No: " + vmOrderDeliverDetail.OrderNo +
                              " Date : " + vmOrderDeliverDetail.DeliveryDate.Value.ToShortDateString() +
                              " Qty: " + qty + "kg." +
                              " Amount: " + ((vmOrderDeliverDetail.DataListDetail?.Count() ?? 0) > 0 ? Convert.ToDouble(vmOrderDeliverDetail.DataListDetail.Sum(item => (item.DeliveredQty * (item.UnitPrice + Convert.ToDouble(item.AdditionPrice))) - (item.DeliveredQty * Convert.ToDouble(item.EBaseCommission) + item.DeliveredQty * Convert.ToDouble(item.ECarryingCommission) + item.DeliveredQty * Convert.ToDouble(item.ECashCommission) + item.DeliveredQty * Convert.ToDouble(item.SpecialDiscount)))) : 0).ToString() +
                              " Delivered from: " + vmOrderDeliverDetail.Warehouse +
                              "\r\n\r\n" +
                              " For any query - Please contact 01700729172.",
                        CompanyId = vmOrderDeliverDetail.CompanyFK.Value,
                        Date = vmOrderDeliverDetail.DeliveryDate.Value,
                        Status = (int)EnumSmSStatus.Pending,
                        PhoneNo = vmOrderDeliverDetail.CustomerPhone.Replace(" ", "").Replace("-", ""),
                        SmsType = 3,
                        Remarks = vmOrderDeliverDetail.OrderNo,
                        TryCount = 0,
                        RowTime = DateTime.Now,
                        Subject = "Order Delivery Notification"

                    };

                    try
                    {
                        _db.ErpSMS.Add(erpSMS);
                        await _db.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        var x = ex.Message;
                    }
                }



            }
            return 1;
        }
        public async Task<int> GCCLOrderDeliverySMSPush(VMOrderDeliverDetail vmOrderDeliverDetail)
        {
            if (vmOrderDeliverDetail.CompanyFK == (int)CompanyName.KrishibidFeedLimited)
            {
                List<string> strList = new List<string>();
                double qty = 0;
                foreach (var item in vmOrderDeliverDetail.DataListDetail)
                {
                    qty += item.DeliveredQty;
                    strList.Add(item.ProductCategory + " " + item.ProductSubCategory + " " + item.ProductName + " Delivered Qty " + item.DeliveredQty + " " + item.UnitName
                        + " Unit Price: " + item.UnitPrice);
                }
                string items = String.Join(", ", strList.ToArray());
                if (vmOrderDeliverDetail.CustomerPhone != null)
                {
                    ErpSMS erpSMS = new ErpSMS
                    {


                        Message = "Dear Valued Customer - " + vmOrderDeliverDetail.CustomerName + ", your order has been successfully delivered from Glorious Crop Care Limited. " +
                               "\r\n\r\n" +
                               " Product: " + items +
                              " Challan No: " + vmOrderDeliverDetail.ChallanNo +
                              " Invoice No: " + vmOrderDeliverDetail.OrderNo +
                              " Date : " + vmOrderDeliverDetail.DeliveryDate.Value.ToShortDateString() +
                              " Amount after discount: " + ((vmOrderDeliverDetail.DataListDetail?.Count() ?? 0) > 0 ? Convert.ToDouble(vmOrderDeliverDetail.DataListDetail.Sum(x => (x.DeliveredQty * x.UnitPrice - Convert.ToDouble(x.Discount ?? 0))) - Convert.ToDouble(vmOrderDeliverDetail.SpecialDiscount ?? 0)) : 0) + // (vmOrderDeliverDetail.DataListDetail.Any() ? Convert.ToDouble(vmOrderDeliverDetail.DataListDetail.Sum(item => (item.DeliveredQty * (item.UnitPrice + Convert.ToDouble(item.AdditionPrice))) - (item.DeliveredQty * Convert.ToDouble(item.EBaseCommission) + item.DeliveredQty * Convert.ToDouble(item.ECarryingCommission) + item.DeliveredQty * Convert.ToDouble(item.ECashCommission) + item.DeliveredQty * Convert.ToDouble(item.SpecialDiscount)))) : 0).ToString() +
                              " Delivered from: " + vmOrderDeliverDetail.Warehouse +
                              "\r\n\r\n" +
                              " For any query - Please contact 01700729903.",
                        CompanyId = vmOrderDeliverDetail.CompanyFK.Value,
                        Date = vmOrderDeliverDetail.DeliveryDate.Value,
                        Status = (int)EnumSmSStatus.Pending,
                        PhoneNo = vmOrderDeliverDetail.CustomerPhone.Replace(" ", "").Replace("-", ""),
                        SmsType = 3,
                        Remarks = vmOrderDeliverDetail.OrderNo,
                        TryCount = 0,
                        RowTime = DateTime.Now,
                        Subject = "Order Delivery Notification"

                    };

                    try
                    {
                        _db.ErpSMS.Add(erpSMS);
                        await _db.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        var x = ex.Message;
                    }
                }



            }
            return 1;
        }
        public async Task<int> FeedMaterialsRecivedSMSPush(VMWarehousePOReceivingSlave vmPOReceiving)
        {
            if (vmPOReceiving.CompanyFK == (int)CompanyName.KrishibidFeedLimited)
            {
                List<string> strList = new List<string>();
                foreach (var item in vmPOReceiving.DataListSlave)
                {
                    strList.Add(item.ProductSubCategory + " " + item.ProductName + ". Net Weight  " + item.StockInQty);
                }
                string items = String.Join(", ", strList.ToArray());

                ErpSMS erpSMS = new ErpSMS
                {
                    Message = "Dear Supplier - " + vmPOReceiving.SupplierName + ", we received " + items + " KG, from You. " +
                               " PO No: " + vmPOReceiving.POCID +
                               " Challan: " + vmPOReceiving.Challan +
                               " Received Date: " + vmPOReceiving.ReceivedDate +
                               " Labour Bill: " + vmPOReceiving.LabourBill +
                               " Truck No: " + vmPOReceiving.TruckNo +
                               " Truck Fare : " + vmPOReceiving.TruckFare +
                               " Contact us at 01700729163 if you have any query. Thanks" +
                               " KRISHIBID FEED LTD.",
                    CompanyId = vmPOReceiving.CompanyFK.Value,
                    Date = vmPOReceiving.ReceivedDate,
                    Status = (int)EnumSmSStatus.Pending,
                    PhoneNo = vmPOReceiving.SupplierPhone.Replace(" ", "").Replace("-", ""),
                    SmsType = 2,
                    Remarks = vmPOReceiving.POCID,
                    TryCount = 0,
                    RowTime = DateTime.Now,
                    Subject = "Material Receive Notification"

                };

                try
                {
                    _db.ErpSMS.Add(erpSMS);
                    await _db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    var x = ex.Message;
                }

            }
            return 1;
        }


        public async Task<long> AccountingProductConvertPushFeed(int companyFk, ConvertedProductModel convertedProductModel, int journalType)
        {
            long result = -1;


            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,
                Title = convertedProductModel.InvoiceNo,//"<a href='" + _urlInfo + "Report/GCCLPurchseInvoiceReport?companyId=" + CompanyFK + "&materialReceiveId=" + vmWareHousePOReceivingSlave.MaterialReceiveId + "&reportName=GCCLPurchaseInvoiceReports'>" + vmWareHousePOReceivingSlave.POCID + "</a>" + " Date: " + vmWareHousePOReceivingSlave.PODate.ToString(),
                Narration = " Date: " + convertedProductModel.ConvertedDate.ToString(),
                CompanyFK = companyFk,
                Date = convertedProductModel.ConvertedDate,
                IsSubmit = true
            };

            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();
            #region Convert From
            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Convert From " + convertedProductModel.ProductFromName + " Convert To" + convertedProductModel.ProductToName + "Qty " + convertedProductModel.ConvertedQty,
                Debit = 0,
                Credit = Convert.ToDouble(convertedProductModel.ConvertedQty * convertedProductModel.ConvertFromUnitPrice),
                Accounting_HeadFK = convertedProductModel.ConvertFromAccountHeadId.Value
            });

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Adjust",
                Debit = Convert.ToDouble(convertedProductModel.ConvertedQty * convertedProductModel.ConvertFromUnitPrice),
                Credit = 0,
                Accounting_HeadFK = 50609522 //Feed Stock Adjust With Erp Cr
            });
            #endregion

            #region Convert To
            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Convert From " + convertedProductModel.ProductFromName + " Convert To" + convertedProductModel.ProductToName + "Qty " + convertedProductModel.ConvertedQty,
                Debit = Convert.ToDouble(convertedProductModel.ConvertedQty * convertedProductModel.ConvertedUnitPrice),
                Credit = 0,
                Accounting_HeadFK = convertedProductModel.ConvertToAccountHeadId.Value
            });

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = "Adjust",
                Debit = 0,
                Credit = Convert.ToDouble(convertedProductModel.ConvertedQty * convertedProductModel.ConvertedUnitPrice),
                Accounting_HeadFK = 50609522 //Feed Stock Adjust With Erp Cr
            });
            #endregion




            var resultData = await AccountingJournalMasterPush(vMJournalSlave);
            var voucherMap = VoucherMapping(result, companyFk, convertedProductModel.ConvertedProductId, convertedProductModel.IntegratedFrom);

            return resultData.VoucherId;
        }


        public async Task<long> AccountingSalesPushGLDL(int companyFk, GLDLBookingViewModel bookingVm, int journalType)
        {
            long result = -1;

            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,

                Title = "File No: " + bookingVm.FileNo + " " + bookingVm.BookingNo + " Date: " + bookingVm.BookingDate.ToString(),
                Narration = "Project: " + bookingVm.ProjectName + " " + bookingVm.BlockName + " " + bookingVm.PlotName,
                CompanyFK = companyFk,
                Date = bookingVm.BookingDate,
                IsSubmit = true,
                Accounting_CostCenterFK = bookingVm.AcCostCenterId
            };

            string particular = bookingVm.FileNo + " " + bookingVm.ProjectName + " " + bookingVm.BlockName + " " + bookingVm.PlotName + " " + " Size " + bookingVm.PlotSize + " Unit Price: " + bookingVm.RatePerKatha + " Total Price" + bookingVm.PlotSize * (double)bookingVm.RatePerKatha + " Total Cost: " + bookingVm.TotalCost + " Total Discount: " + bookingVm.TotalDiscount;

            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = particular,
                Debit = Convert.ToDouble(bookingVm.GrandTotalAmount),
                Credit = 0,
                Accounting_HeadFK = bookingVm.HeadGLId.Value //Customer/ LC 

            });

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = particular,
                Debit = 0,
                Credit = Convert.ToDouble(bookingVm.GrandTotalAmount),
                Accounting_HeadFK = bookingVm.AccountingIncomeHeadId.Value
            });

            var resultData = await AccountingJournalMasterPush(vMJournalSlave);
            var voucherMap = VoucherMapping(result, bookingVm.CompanyId.Value, bookingVm.BookingId, bookingVm.IntegratedFrom);

            return resultData.VoucherId;
        }
        public async Task<long> BookingMoneyCollectionPushGLDL(int companyFk, GLDLBookingViewModel bookingVm, int journalType)
        {
            long result = -1;

            VMJournalSlave vMJournalSlave = new VMJournalSlave
            {
                JournalType = journalType,

                Title = "File No: " + bookingVm.FileNo + " " + bookingVm.BookingNo + " Date: " + bookingVm.BookingDate.ToString(),
                Narration = "Project: " + bookingVm.ProjectName + " " + bookingVm.BlockName + " " + bookingVm.PlotName,
                CompanyFK = companyFk,
                Date = bookingVm.BookingDate,
                IsSubmit = true
            };

            string particular = "File No: " + bookingVm.FileNo + " " + bookingVm.ProjectName + " " + bookingVm.BlockName + " " + bookingVm.PlotName + " " + " Size " + bookingVm.PlotSize + " Unit Price: " + (double)bookingVm.RatePerKatha + " Total Price" + bookingVm.PlotSize * (double)bookingVm.RatePerKatha + " Total Cost: " + bookingVm.TotalCost + " Total Discount: " + bookingVm.TotalDiscount;

            vMJournalSlave.DataListSlave = new List<VMJournalSlave>();

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = particular,
                Debit = 0,
                Credit = Convert.ToDouble(bookingVm.BookingMoney),
                Accounting_HeadFK = bookingVm.HeadGLId.Value //Customer/ LC 

            });

            vMJournalSlave.DataListSlave.Add(new VMJournalSlave
            {
                Particular = particular,
                Debit = Convert.ToDouble(bookingVm.BookingMoney),
                Credit = 0,
                Accounting_HeadFK = bookingVm.Accounting_BankOrCashId.Value
            });

            var resultData = await AccountingJournalMasterPush(vMJournalSlave);
            var voucherMap = VoucherMapping(result, bookingVm.CompanyId.Value, bookingVm.BookingId, bookingVm.IntegratedFrom);

            return resultData.VoucherId;
        }

        public async Task<long> InstallmentCollectionPushGLDL(int companyFk, CollactionBillViewModel collectionVm, int journalType)
        {
            long result = -1;
            try
            {

                VMJournalSlave vMJournalSlave = new VMJournalSlave
                {
                    JournalType = journalType,
                    Title = collectionVm.PaymentNo + " Date: " + collectionVm.TransactionDate.ToString() + " " + collectionVm.ChequeNo,
                    Narration = "Project: " + collectionVm.ProductName + " " + collectionVm.BookingNo,
                    CompanyFK = companyFk,
                    Date = collectionVm.TransactionDate,
                    IsSubmit = true
                };

                vMJournalSlave.DataListSlave = new List<VMJournalSlave>();
                List<string> strList = new List<string>();
                if ((collectionVm.PaymentList?.Count() ?? 0) > 0)
                {
                    foreach (var item in collectionVm.PaymentList)
                    {
                        strList.Add(item.Title + " Date: " + item.InstallmentDate + " Money Receipt No: " + item.MoneyReceiptNo);

                        vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                        {
                            Particular = item.Title + " Date: " + item.InstallmentDate + " Money Receipt No: " + item.MoneyReceiptNo,
                            Debit = 0,
                            Credit = Convert.ToDouble(item.InAmount),
                            Accounting_HeadFK = item.HeadGLId.Value
                        });
                    }
                }

                string particular = String.Join(", ", strList.ToArray());

                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = particular,
                    Debit = Convert.ToDouble(collectionVm.TotalInstallment - collectionVm.BankCharge),
                    Credit = 0,
                    Accounting_HeadFK = collectionVm.Accounting_BankOrCashId.Value //Bank Or Cash 

                });
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = particular,
                    Debit = Convert.ToDouble(collectionVm.BankCharge),
                    Credit = 0,
                    Accounting_HeadFK = collectionVm.BankChargeHeadGLId.Value //Bank Charge 

                });
                var resultData = await AccountingJournalMasterPush(vMJournalSlave);
                var voucherMap = VoucherMapping(resultData.VoucherId, collectionVm.CompanyId.Value, collectionVm.PaymentMasterId, collectionVm.IntegratedFrom);
                return resultData.VoucherId;
            }
            catch (Exception ex)
            {

                return result;
            }


        }


        public async Task<long> GldlKplCollectionPush(int companyId, MoneyReceiptViewModel moneyReceiptViewModel)
        {
            long result = -1;
            try
            {

                VMJournalSlave vMJournalSlave = new VMJournalSlave
                {
                    JournalType = moneyReceiptViewModel.VoucherTypeId,
                    Title = "File No: " + moneyReceiptViewModel.FileNo + " " + moneyReceiptViewModel.MoneyReceiptNo + " Date: " + moneyReceiptViewModel.MoneyReceiptDate.ToString() + " MR No: " + moneyReceiptViewModel.SerialNumber,
                    Narration = "Project: " + moneyReceiptViewModel.ProjectName + " " + moneyReceiptViewModel.BlockName + " " + moneyReceiptViewModel.PlotName,
                    CompanyFK = companyId,
                    Date = moneyReceiptViewModel.Submitdate,
                    IsSubmit = true
                };

                vMJournalSlave.DataListSlave = new List<VMJournalSlave>();
                List<string> strList = new List<string>();
                foreach (var item in moneyReceiptViewModel.MoneyReceiptList)
                {
                    strList.Add("Collection From" + item.CollectionFrom + " Amount: " + item.PaidAmount);
                }
                string particular = String.Join(", ", strList.ToArray());

                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = particular,
                    Debit = 0,
                    Credit = Convert.ToDouble(moneyReceiptViewModel.TotalAmount),
                    Accounting_HeadFK = moneyReceiptViewModel.HeadGLId.Value
                });


                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = particular,
                    Debit = Convert.ToDouble(moneyReceiptViewModel.TotalAmount - moneyReceiptViewModel.BankCharge),
                    Credit = 0,
                    Accounting_HeadFK = moneyReceiptViewModel.Accounting_BankOrCashId.Value //Bank Or Cash 

                });
                vMJournalSlave.DataListSlave.Add(new VMJournalSlave
                {
                    Particular = particular,
                    Debit = Convert.ToDouble(moneyReceiptViewModel.BankCharge),
                    Credit = 0,
                    Accounting_HeadFK = moneyReceiptViewModel.BankChargeAccHeahId //Bank Charge 

                });
                var resultData = await AccountingJournalMasterPush(vMJournalSlave);
                var voucherMap = VoucherMapping(resultData.VoucherId, moneyReceiptViewModel.CompanyId, moneyReceiptViewModel.MoneyReceiptId, moneyReceiptViewModel.IntegratedFrom);

                var res = await MoneyReceiptStatusUpdate(moneyReceiptViewModel);
                return resultData.VoucherId;
            }
            catch (Exception ex)
            {

                return result;
            }


        }
        public async Task<int> MoneyReceiptStatusUpdate(MoneyReceiptViewModel moneyReceiptViewModel)
        {

            var installments = moneyReceiptViewModel.MoneyReceiptList.Where(x => x.Indecator == (int)IndicatorEnum.Installment).ToList();
            if (installments != null)
            {
                foreach (var item in installments)
                {
                    BookingInstallmentSchedule schedule = _db.BookingInstallmentSchedules.FirstOrDefault(h => h.InstallmentId == item.CollectionFromId);
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
                    _db.Entry(schedule).State = EntityState.Modified;
                    _db.SaveChanges();
                }
            }
            var costCollection = moneyReceiptViewModel.MoneyReceiptList.Where(x => x.Indecator == (int)IndicatorEnum.CostHead).ToList();
            if (costCollection?.Count > 0)
            {
                foreach (var item in costCollection)
                {
                    BookingCostMapping bookingCostMapping = _db.BookingCostMappings.FirstOrDefault(h => h.CostsMappingId == item.CollectionFromId);
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
                    _db.Entry(bookingCostMapping).State = EntityState.Modified;
                    _db.SaveChanges();
                }
            }

            var bookingMoney = moneyReceiptViewModel.MoneyReceiptList.FirstOrDefault(x => x.Indecator == (int)IndicatorEnum.BookingMoney);
            if (bookingMoney != null)
            {
                ProductBookingInfo productBookingInfoes = _db.ProductBookingInfoes.FirstOrDefault(h => h.BookingId == bookingMoney.CollectionFromId);
                productBookingInfoes.PaidBookingAmt = productBookingInfoes.PaidBookingAmt + bookingMoney.PaidAmount ?? 0;
                _db.Entry(productBookingInfoes).State = EntityState.Modified;
                _db.SaveChanges();
            }
            MoneyReceipt moneyReceipt = _db.MoneyReceipts.Find(moneyReceiptViewModel.MoneyReceiptId);
            moneyReceipt.IsSubmitted = true;
            moneyReceipt.SubmittedDate = moneyReceiptViewModel.Submitdate;
            moneyReceipt.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            moneyReceipt.ModifiedDate = DateTime.Now;
            _db.Entry(moneyReceipt).State = EntityState.Modified;
            _db.SaveChanges();


            return 0;
        }

    }
}
