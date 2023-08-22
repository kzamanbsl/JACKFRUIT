using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BSA.Data.Models;
using BSA.Utility;

namespace BSA.Service.Interface
{
    public interface IDropDownItemService
    {
        List<DropDownItem> GetDropDownItems();
        List<SelectModel> GetDropDownItemSelectModels(int id);
        // List<SelectModel> getCompanySelectModels();
    }
}
