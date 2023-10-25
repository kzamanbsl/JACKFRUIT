using KGERP.Data.Models;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGERP.Service.Interface
{
    public interface IDemageService
    {
        Task<int> DemageMasterAdd(DemageMasterModel model);
        Task<int> DemageDetailAdd(DemageMasterModel model);
        Task<int> DemageDetailEdit(DemageMasterModel model);
    }
}
