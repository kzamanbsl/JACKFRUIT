using KGERP.Data.CustomModel;
using KGERP.Data.Models;
using KGERP.Service.Implementation.Configuration;
using KGERP.Utility;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KGERP.Service.Implementation.Dashboard_service
{
    public class DashboardService
    {
        private ERPEntities _context;
        private readonly ConfigurationService _configurationService;
        public DashboardService(ERPEntities context, ConfigurationService configurationService)
        {
            _context = context;
            _configurationService = configurationService;
        }
        public async Task<DashboardViewModel> AllCount(int companyId)
        {
            DashboardViewModel vm = new DashboardViewModel();
            vm.UserDataAccessModel = await _configurationService.GetUserDataAccessModelByEmployeeId();

            vm.TotalSupplier = _context.Vendors.Where(x => x.CompanyId == companyId && x.VendorTypeId == (int)Provider.Supplier && x.IsActive==true).Count();
            vm.TotalVendor = _context.Vendors.Where(x => x.CompanyId == companyId && x.VendorTypeId == (int)Provider.Customer && x.IsActive == true).Count();
            vm.TotalDeport = _context.Vendors.Where(x => x.CompanyId == companyId && x.VendorTypeId == (int)Provider.Deport && x.IsActive == true).Count();
            vm.TotalDealer = _context.Vendors.Where(x => x.CompanyId == companyId && x.VendorTypeId == (int)Provider.Dealer && x.IsActive == true).Count();
           
            //vm.TotalPurchase = _context.PurchaseOrders.Where(x => x.CompanyId == companyId && x.PurchaseDate == DateTime.Today && x.IsActive).Count();
            //vm.TotalPurcaseAmmount = (from t1 in _context.PurchaseOrders
            //                          join t2 in _context.PurchaseOrderDetails on t1.PurchaseOrderId equals t2.PurchaseOrderId
            //                          where t1.CompanyId == companyId && t1.PurchaseDate == DateTime.Today && t1.IsActive
            //                          select t2.PurchaseAmount).DefaultIfEmpty(0).Sum();

            vm.TotalSale = _context.OrderMasters.Where(x => x.CompanyId == companyId && x.OrderDate == DateTime.Today && x.StockInfoTypeId==(int)StockInfoTypeEnum.Company && x.IsActive==true && x.IsOpening==false).Count();
            
            vm.TotalSaleAmmount = (from t1 in _context.OrderMasters
                                   join t2 in _context.OrderDetails on t1.OrderMasterId equals t2.OrderMasterId
                                   where t1.CompanyId == companyId && t1.OrderDate == DateTime.Today && t1.StockInfoTypeId == (int)StockInfoTypeEnum.Company && t1.IsActive==true && t1.IsOpening == false
                                   select t2.Amount).DefaultIfEmpty(0).Sum();

            vm.TotalDamageAmmount = (from t1 in _context.DamageMasters
                                   join t2 in _context.DamageDetails on t1.DamageMasterId equals t2.DamageMasterId
                                   where t1.CompanyId == companyId && t1.OperationDate == DateTime.Today 
                                   && t1.DamageFromId == (int)EnumDamageFrom.Customer && t1.StatusId != (int)EnumDamageStatus.Received
                                   && t1.DamageFromId == (int)EnumDamageFrom.Dealer && t1.StatusId != (int)EnumDamageStatus.Received
                                   && t1.DamageFromId == (int)EnumDamageFrom.Deport && t1.StatusId != (int)EnumDamageStatus.Received
                                   && t1.IsActive == true && t2.IsActive == true
                                     select t2.TotalPrice).DefaultIfEmpty(0).Sum();

            //vm.Payment = (decimal)(from t1 in _context.Vendors
            //                       join t2 in _context.Payments on t1.VendorId equals t2.VendorId
            //                       join t3 in _context.PaymentMasters on t2.PaymentMasterId equals t3.PaymentMasterId
            //                       where t1.VendorTypeId == 1 && t1.CompanyId == companyId && t1.IsActive && t2.IsActive && t3.IsActive && t3.TransactionDate == DateTime.Today 
            //                       select t2.InAmount).DefaultIfEmpty(0).Sum();

            //vm.Collection = (decimal)(from t1 in _context.Payments
            //                          join t2 in _context.Vendors on t1.VendorId equals t2.VendorId
            //                          join t3 in _context.PaymentMasters on t1.PaymentMasterId equals t3.PaymentMasterId
            //                          where t2.VendorTypeId == 2 && t1.CompanyId == companyId && t1.IsActive && t2.IsActive && t3.IsActive && t3.TransactionDate == DateTime.Today
            //                          select t1.OutAmount ?? 0).DefaultIfEmpty(0).Sum();

            var enddate = DateTime.Now.AddMonths(-1);
            //vm.TotalMonthPurchaseAmmount = (from t1 in _context.PurchaseOrders
            //                                join t2 in _context.PurchaseOrderDetails on t1.PurchaseOrderId equals t2.PurchaseOrderId
            //                                where t1.CompanyId == companyId && t1.PurchaseDate >= enddate && t1.PurchaseDate <= DateTime.Now && t1.IsActive && t2.IsActive
            //                                select t2.PurchaseAmount).DefaultIfEmpty(0).Sum();

            vm.TotalMonthSeleAmmount = (from t1 in _context.OrderMasters
                                        join t2 in _context.OrderDetails on t1.OrderMasterId equals t2.OrderMasterId
                                        where t1.CompanyId == companyId && t1.StockInfoTypeId == (int)StockInfoTypeEnum.Company && t1.IsActive==true && t1.IsOpening == false 
                                        && t2.IsActive==true && t1.OrderDate >= enddate && t1.OrderDate <= DateTime.Now
                                        select t2.Amount).DefaultIfEmpty(0).Sum();


            //vm.MonthPurchasePayment = (decimal)(from t1 in _context.Vendors
            //                                    join t2 in _context.Payments on t1.VendorId equals t2.VendorId
            //                                    join t3 in _context.PaymentMasters on t2.PaymentMasterId equals t3.PaymentMasterId
            //                                    where t1.VendorTypeId == 1 && t1.IsActive && t2.IsActive && t3.IsActive && t1.CompanyId == companyId && t2.TransactionDate >= enddate && t3.TransactionDate <= DateTime.Now
            //                                    select t2.OutAmount ?? 0).DefaultIfEmpty(0).Sum();


            //vm.MonthSaleCollection = (decimal)(from t1 in _context.Payments
            //                                   join t2 in _context.Vendors on t1.VendorId equals t2.VendorId
            //                                   join t3 in _context.PaymentMasters on t1.PaymentMasterId equals t3.PaymentMasterId

            //                                   where t2.VendorTypeId == 2 && t1.IsActive && t2.IsActive && t3.IsActive && t1.CompanyId == companyId && t1.TransactionDate >= enddate && t1.TransactionDate <= DateTime.Now
            //                                   select t1.InAmount).DefaultIfEmpty(0).Sum();

            return vm;
        }


    }
}
