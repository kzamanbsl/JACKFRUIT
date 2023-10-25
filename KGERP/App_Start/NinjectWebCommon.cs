using System;
using System.Configuration;
using System.Data.Entity;
using System.Web;
using KGERP;
using KGERP.Data.Models;
using KGERP.Service.Implementation;
using KGERP.Service.Implementation.ApprovalSystemService;
using KGERP.Service.Implementation.FTP;
using KGERP.Service.Implementation.ProdMaster;
using KGERP.Service.Implementation.Realestate;
using KGERP.Service.Implementation.Realestate.BookingAprovalList;
using KGERP.Service.Implementation.Realestate.BookingCollaction;
using KGERP.Service.Implementation.Realestate.CustomersBooking;
using KGERP.Service.Implementation.SMSService;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Common;
using Ninject.Web.Common.WebHost;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(NinjectWebCommon), "Stop")]

namespace KGERP
{
    public static class NinjectWebCommon
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }

        public static void Stop()
        {
            bootstrapper.ShutDown();
        }

        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            RegisterServices(kernel);
            return kernel;
        }

        private static void RegisterServices(IKernel kernel)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ERPEntities"].ConnectionString;
            kernel.Bind<DbContext>().To<ERPEntities>().InRequestScope().WithConstructorArgument("connectionString", connectionString);
            kernel.Bind<IDropDownTypeService>().To<DropDownTypeService>().InRequestScope();
            kernel.Bind<IDropDownItemService>().To<DropDownItemService>().InRequestScope();
            kernel.Bind<ICompanyService>().To<CompanyService>().InRequestScope();
            kernel.Bind<ICompanyMenuService>().To<CompanyMenuService>().InRequestScope();
            kernel.Bind<ICompanySubMenuService>().To<CompanySubMenuService>().InRequestScope();
            kernel.Bind<ICompanyUserMenuService>().To<CompanyUserMenuService>().InRequestScope();
            kernel.Bind<IEducationService>().To<EducationService>().InRequestScope();
            kernel.Bind<IDistrictService>().To<DistrictService>().InRequestScope();
            kernel.Bind<IUpazilaService>().To<UpazilaService>().InRequestScope();
            kernel.Bind<ILeaveApplicationService>().To<LeaveApplicationService>().InRequestScope();
            kernel.Bind<ILeaveCategoryService>().To<LeaveCategoryService>().InRequestScope();
            kernel.Bind<IProductService>().To<ProductService>().InRequestScope();
            kernel.Bind<IConvertedProductService>().To<ConvertedProductService>().InRequestScope();
            kernel.Bind<IProductCategoryService>().To<ProductCategoryService>().InRequestScope();
            kernel.Bind<IProductSubCategoryService>().To<ProductSubCategoryService>().InRequestScope();
            kernel.Bind<IVendorTypeService>().To<VendorTypeService>().InRequestScope();
            kernel.Bind<IVendorService>().To<VendorService>().InRequestScope();
            kernel.Bind<IVendorOfferService>().To<VendorOfferService>().InRequestScope();
            kernel.Bind<IStockInfoService>().To<StockInfoService>().InRequestScope();
            kernel.Bind<IUnitService>().To<UnitService>().InRequestScope();
            kernel.Bind<IOrderMasterService>().To<OrderMasterService>().InRequestScope();
            kernel.Bind<IOrderDetailService>().To<OrderDetailService>().InRequestScope();
            kernel.Bind<IStoreService>().To<StoreService>().InRequestScope();
            kernel.Bind<IStoreDetailService>().To<StoreDetailService>().InRequestScope();
            kernel.Bind<IOrderDeliverService>().To<OrderDeliverService>().InRequestScope();
            kernel.Bind<IPaymentService>().To<PaymentService>().InRequestScope();
            kernel.Bind<IZoneService>().To<ZoneService>().InRequestScope();
            kernel.Bind<ISubZoneService>().To<SubZoneService>().InRequestScope();
            kernel.Bind<IProductFormulaService>().To<ProductFormulaService>().InRequestScope();
            kernel.Bind<IDemandService>().To<DemandService>().InRequestScope();
            kernel.Bind<IIssueService>().To<IssueService>().InRequestScope();
            kernel.Bind<IUpazilaAssignService>().To<UpazilaAssignService>().InRequestScope();
            kernel.Bind<IBankService>().To<BankService>().InRequestScope();
            kernel.Bind<ICostHeadsService>().To<CostHeadsService>().InRequestScope();
            kernel.Bind<IFTPService>().To<FTPService>().InRequestScope();
            kernel.Bind<IGLDLCustomerService>().To<GLDLCustomerService>().InRequestScope();
            kernel.Bind<IInstallmentTypeService>().To<InstallmentTypeService>().InRequestScope();
            kernel.Bind<IRequisitionService>().To<RequisitionService>().InRequestScope();
            kernel.Bind<IAccountHeadService>().To<AccountHeadService>().InRequestScope();
            kernel.Bind<IPaymentModeService>().To<PaymentModeService>().InRequestScope();
            kernel.Bind<IVoucherService>().To<VoucherService>().InRequestScope();
            kernel.Bind<IPFormulaDetailService>().To<PFormulaDetailService>().InRequestScope();
            kernel.Bind<IPurchaseOrderService>().To<PurchaseOrderService>().InRequestScope();
            kernel.Bind<IStockTransferService>().To<StockTransferService>().InRequestScope();
            kernel.Bind<IHeadGLService>().To<HeadGLService>().InRequestScope();
            kernel.Bind<IVoucherTypeService>().To<VoucherTypeService>().InRequestScope();
            kernel.Bind<ISaleReturnService>().To<SaleReturnService>().InRequestScope();
            kernel.Bind<IEmiService>().To<EmiService>().InRequestScope();
            kernel.Bind<IAssetLocationService>().To<AssetLocationService>().InRequestScope();
            kernel.Bind<IMessageService>().To<MessageService>().InRequestScope();
            kernel.Bind<IAssetCategoryService>().To<AssetCategoryService>().InRequestScope();
            kernel.Bind<IWorkService>().To<WorkService>().InRequestScope();
            kernel.Bind<IWorkAssignService>().To<WorkAssignService>().InRequestScope();
            kernel.Bind<IWorkStateService>().To<WorkStateService>().InRequestScope();
            kernel.Bind<IAssetService>().To<AssetService>().InRequestScope();
            kernel.Bind<IAssignAssetService>().To<AssignAssetService>().InRequestScope();
            kernel.Bind<IWorkQAService>().To<WorkQAService>().InRequestScope();
            kernel.Bind<IStockAdjustService>().To<StockAdjustService>().InRequestScope();
            kernel.Bind<IProductDetailService>().To<ProductDetailService>().InRequestScope();
            kernel.Bind<IOfficerAssignService>().To<OfficerAssignService>().InRequestScope();
            kernel.Bind<IMaterialReceiveService>().To<MaterialReceiveService>().InRequestScope();
            kernel.Bind<IECMemberService>().To<ECMemberService>().InRequestScope();
            kernel.Bind<IShareHolderService>().To<ShareHolderService>().InRequestScope();
            kernel.Bind<IBoardOfDirectorService>().To<BoardOfDirectorService>().InRequestScope();
            kernel.Bind<ILandOwnerService>().To<LandOwnerService>().InRequestScope();
            kernel.Bind<IEmployeeService>().To<EmployeeService>().InRequestScope();
            kernel.Bind<IIngredientStandardService>().To<IngredientStandardService>().InRequestScope();
            kernel.Bind<IComplainManagementService>().To<ComplainManagementService>().InRequestScope();
            kernel.Bind<IFarmerService>().To<FarmerService>().InRequestScope();
            kernel.Bind<ICreditRecoverService>().To<CreditRecoverService>().InRequestScope();
            kernel.Bind<IMonthlyTargetService>().To<MonthlyTargetService>().InRequestScope();
            kernel.Bind<IBagService>().To<BagService>().InRequestScope();
            kernel.Bind<IAccountSignatoryService>().To<AccountSignatoryService>().InRequestScope();
            kernel.Bind<IPurchaseReturnService>().To<PurchaseReturnService>().InRequestScope();
            kernel.Bind<IRentProductionService>().To<RentProductionService>().InRequestScope();
            kernel.Bind<ISmsServices>().To<SMSServices>().InRequestScope();
            kernel.Bind<ICustomerBookingService>().To<CustomerBookingService>().InRequestScope();
            kernel.Bind<IBookingInstallmentService>().To<BookingInstallmentService>().InRequestScope();
            kernel.Bind<IBookingAprovalStatus>().To<BookingAprovalStatusService>().InRequestScope();
            kernel.Bind<ITeamService>().To<TeamService>().InRequestScope();
            kernel.Bind<IBookingCollaction>().To<BookingCollactionService>().InRequestScope();
            kernel.Bind<IYearlyHoliday>().To<YearlyHolidayService>().InRequestScope();
            kernel.Bind<ICrmService>().To<CrmService>().InRequestScope();
            kernel.Bind<IPermissionHandler>().To<PermissionHandler>().InRequestScope();
            kernel.Bind<IApproval_Service>().To<Approval_Service>().InRequestScope();
            kernel.Bind<IManagerProductMapService>().To<ManagerProductMapService>().InRequestScope();
            kernel.Bind<IExpenseService>().To<ExpenseService>().InRequestScope();
            kernel.Bind<IProductionMasterService>().To<ProductionMasterService>().InRequestScope();
            kernel.Bind<IDemageService>().To<DemageService>().InRequestScope();



        }
    }
}