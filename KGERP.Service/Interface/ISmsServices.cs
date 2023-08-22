using KGERP.Data.Models;
using KGERP.Service.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGERP.Service.Interface
{
    public interface ISmsServices
    {
        Task<List<vwSMSList>> GetSmsList(int status);


        Task<List<vwSMSList>> GetSmsCompanyWise(int Statusi, int companyId);
        Task<SmsListVm> GetSmsCompanyWiseList(int companyId, int Type, DateTime? fromDate, DateTime? toDate);

        //Send Sms  
        Task<bool> SendSms(ErpSMS erpSM);


        Task<bool> SendSms(List<ErpSMS> erpSM);
        Task<List<SmsType>> GetSmsTypeList();
        Task<SmsType> GetSmsTypeById(int id);




    }
}
