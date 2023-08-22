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
    public interface ISeedCategoryService : IDisposable
    {
        List<SeedCategoryModel> GetSeedCategorys( string searchText,string memberId); 
        List<SeedCategoryModel> GetAllSeedCategory( string searchText);
        List<SelectModel> GetAllSeedCategory();
        SeedCategoryModel GetSeedCategory(int id); 
        bool SaveSeedCategory(int id, SeedCategoryModel model);
        bool DeleteSeedCategory(int id); 
    }
}
