using BSA.Data.Models;
using BSA.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSA.Service.Interface
{
  public  interface IDesignationService
    {
        List<Designation> GetDesignations();
        List<SelectModel> GetDesignationSelectModels();
    }
}
