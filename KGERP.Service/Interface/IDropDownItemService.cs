using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KGERP.Service.Interface
{
    public interface IDropDownItemService
    {
        List<DropDownItemModel> GetDropDownItems(string searchText);
        List<SelectModel> GetDropDownItemSelectModels(int id);
        DropDownItemModel GetDropDownItem(int id);
        bool SaveDropDownItem(int id, DropDownItemModel upazila, out string message);
        bool DeleteDropDownItem(int id);
        Task<DropDownItemModel> GetDropDownItems(int companyId);
        // List<SelectModel> getCompanySelectModels();
    }
}
