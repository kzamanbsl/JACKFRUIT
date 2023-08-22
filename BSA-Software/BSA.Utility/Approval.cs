using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSA.Utility
{
    public static class Approval
    {
        public static List<SelectModel> GetStatus()
        {
            List<SelectModel> selectModels = new List<SelectModel>()
            {
                 new SelectModel(){ Text="Pending",Value="Pending"},
                  new SelectModel(){ Text="Approved",Value="Approved"},
                      new SelectModel(){ Text="Denied",Value="Denied"},

            };

            return selectModels;
        }
    }
}
