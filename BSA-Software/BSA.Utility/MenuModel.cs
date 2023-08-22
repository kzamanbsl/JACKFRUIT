using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BSA.Utility
{
    public class MenuModel 
    {
        public long UserMenuId { get; set; }
        public int CompanyId { get; set; }
        
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public int SubMenuId { get; set; }
        public string SubMenuName { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Link { get; set; }
        public string UserId { get; set; }
        public bool IsView { get; set; }
        public bool IsAdd { get; set; }
        public bool IsUpdate { get; set; }
        public bool IsDelete { get; set; }
        public int MenuOrder { get; set; }
        public int SubMenuOrder { get; set; }
    }
}
