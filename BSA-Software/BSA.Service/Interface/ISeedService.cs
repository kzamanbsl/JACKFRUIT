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
    public interface ISeedService : IDisposable
    {
        List<SeedModel> GetSeeds( string searchText, string memberId); 
        List<SeedModel> GetAllSeed( string searchText); 
        List<SeedModel> SeedVerification( string searchText); 
        SeedModel GetSeed(int id); 
        bool SaveSeed(int id, SeedModel model);
        bool DeleteSeed(int id); 
    }
}
