
namespace KGERP.Data.CustomModel
{
    public class DashboardViewModel
    {
        public int TotalDeport { get; set; }
        public int TotalDealer { get; set; }
        public int TotalVendor { get; set; } // Customer
        public int TotalSupplier { get; set; }
        public int TotalPurchase { get; set; }
        public decimal TotalPurcaseAmmount { get; set; }
        public int TotalSale { get; set; }
        public double TotalSaleAmmount { get; set; }
        public decimal Payment { get; set; }
        public decimal Collection { get; set; }
        public decimal MonthPurchasePayment { get; set; }
        public decimal MonthSaleCollection { get; set; }
        public decimal TotalMonthPurchaseAmmount { get; set; }
        public double TotalMonthSeleAmmount { get; set; }
        public UserDataAccessModel UserDataAccessModel { get; set; }

    }
}
