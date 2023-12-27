using KGERP.Utility;

namespace KGERP.Data.CustomModel
{
    /// <summary>
    /// This Model work for User wise data filter and button permission in UI
    /// Method GetUserDataAccessModelByEmployeeId(long id)
    /// </summary>
    public class UserDataAccessModel
    {
        /// <summary>
        /// EmployeeId is Id
        /// </summary>
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }

        /// <summary>
        /// UserName is UserName or EmployeeId
        /// </summary>
        public string UserName { get; set; }
        public int UserTypeId { get; set; }
        public string UserTypeName { get { return BaseFunctionalities.GetEnumDescription((EnumUserType)UserTypeId); } }
        public bool IsAdmin { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }

        public int[] ZoneIds { get; set; }
        public int[] ZoneDivisionIds { get; set; }
        public int[] AreaIds { get; set; }
        public int[] RegionIds { get; set; }
        public int[] SubZoneIds { get; set; }

        public int[] SupplierIds { get; set; }
        public int[] DeportIds { get; set; }
        public int[] DealerIds { get; set; }
        public int[] CustomerIds { get; set; }
    }
}
