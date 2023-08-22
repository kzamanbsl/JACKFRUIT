using KGERP.Data.Models;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGERP.Service.Implementation.Realestate.CustomersBooking
{
   public interface ICustomerBookingService
    {
        Task<GLDLBookingViewModel> CustomerBookingView(int companyId, long CGId);
        Task<CollactionBillViewModel> CustomerInstallmentSchedule(int companyId, long CGId);
        Task<int> UpdateInsatllment(InstallmentScheduleShortModel model);
        List<SceduleInstallment> GetByInstallmentSchedule(int companyId, long CGId);
        Task<GLDLBookingViewModel> CustomerBookingList(int companyId, DateTime? fromDate, DateTime? toDate);
        Task<BookingInstallmentSchedule> InstallmentScheduleById(long id);
        Task<InstallmentScheduleShortModel> getInstallmetClient(long id);
        Task<List<SelectModelType>> PRMRelation();
        Task<long> SubmitBooking(GLDLBookingViewModel bookingViewModel);
        Task<long> SubmitStatusChange(GLDLBookingViewModel bookingViewModel);

    }
}
