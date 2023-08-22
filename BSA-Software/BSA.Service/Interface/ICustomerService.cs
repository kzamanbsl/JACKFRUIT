using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BSA.Data.Models;
using BSA.Service.ServiceModel;
using BSA.Utility;    

namespace BSA.Service.Interface
{
    public interface ICustomerService : IDisposable
    {
        List<CustomerModel> GetCustomers( string searchText); 
        List<CustomerModel> GetAllCustomers( string searchText); 
        CustomerModel GetCustomer(int id); 
        bool SaveCustomer(int id, CustomerModel model);
        bool DeleteCustomer(int id); 
    }
}
