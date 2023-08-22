using KGERP.Data.Models;
using AttendenceApproveApplication = KGERP.Data.Models.Extended.AttendenceApproveApplication;

namespace KGERP.ViewModel
{
    public class AttendenceViewModel
    {
        public AttendenceApproveApplication Attendence { get; set; }
        public AttendenceViewModel()
        {
            AttendenceApproveApplication attendence = new AttendenceApproveApplication();
        }
    }
}