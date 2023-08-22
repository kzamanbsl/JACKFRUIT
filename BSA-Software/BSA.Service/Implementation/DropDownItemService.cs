using BSA.Service.Interface;
using BSA.Data.Models;
using BSA.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSA.Service.Implementation
{
    public class DropDownItemService : IDropDownItemService
    {
        BSAEntities dropdownItemRepository = new BSAEntities();
        public List<DropDownItem> GetDropDownItems()
        {
            return dropdownItemRepository.DropDownItems.ToList();
        }

        

        public List<SelectModel> GetDropDownItemSelectModels(int id)
        {
            return dropdownItemRepository.DropDownItems.ToList().Where(x=>x.DropDownTypeId==id).Select(x => new SelectModel()
            {
                Text = x.Name.ToString(),
                Value = x.DropDownItemId.ToString()
            }).ToList();
        }
    }
}
