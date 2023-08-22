using KGERP.Data.Models;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KGERP.Service.Implementation.SMSService
{
   public class SMSServices :ISmsServices
    {
    
        //string Apikey = "KEY-czvwjcex2sdevdgf390x7k20vs7eeo4t";
        //string ApiSecret = "YnRUb0JH38cKmI@n";
        //string BaseUrl = "https://portal.adnsms.com";

        private readonly ERPEntities _db;
        public SMSServices( ERPEntities entities)
        {
            this._db = entities;
        }
       


        //faild
        public async Task<List<vwSMSList>> GetSmsList(int status)
        {
            if (status==99)
            {
                var res= await _db.vwSMSLists.ToListAsync();
                return res;
            }
            else
            {
                var result = await _db.vwSMSLists.Where(s=>s.Status==status).ToListAsync();
                return result;
            }
        }
        public async Task<List<vwSMSList>> GetSmsCompanyWise(int Status,int companyId)
        {
            return await _db.vwSMSLists.Where(e => e.Status == Status&&e.CompanyId==companyId).ToListAsync();
        }


        public async Task<SmsListVm> GetSmsCompanyWiseList(int companyId ,int Type, DateTime? fromDate, DateTime? toDate)
        {
            SmsListVm vm = new SmsListVm();
            vm.CompanyId = companyId;
            vm.type = Type;
            vm.FromDate = fromDate.Value;
            vm.ToDate = toDate.Value;
            vm.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            vm.StrToDate = toDate.Value.ToString("yyyy-MM-dd");
            vm.SmsTypeList =await GetSmsTypeList();
            vm.DataList= await _db.vwSMSLists
                .Where(q=>q.CompanyId == companyId
                && q.Date>fromDate && q.Date < toDate
                &&(Type==0?q.SmsType>0:q.SmsType==Type)
                )
                .Select(s=> new SmsVm
                {
                    CompanyId = companyId,
                    Status = s.Status,
                    CompanyName=s.CompanyName,
                    Date=s.Date,
                    Subject=s.Subject,
                    PhoneNo=s.PhoneNo,
                    Message=s.Message,
                    TryCount=s.TryCount,
                    RowTime =s.RowTime,
                    Remarks=s.Remarks,
                    SmsType=s.SmsType,
                    SMSTypeName=s.SMSTypeName
                }).OrderByDescending(o=>o.Date)
                .ToListAsync();

                return vm;
        }


        //Send Sms  
        public async Task<bool> SendSms(ErpSMS erpSM)
        {
            int r = -1;
            //ErpSM sM = MapModel(erpSM);
            _db.ErpSMS.Add(erpSM);

            try
            {
                r = await _db.SaveChangesAsync();
                return r > 0;
            }
            catch (Exception)
            {

                return false;
            }
         
        }

        public async Task<List<SmsType>> GetSmsTypeList()
        {
            return await _db.SmsTypes.Where(e => e.IsActive == true).ToListAsync();

        }
        public async Task<SmsType> GetSmsTypeById(int id)
        {
            return await _db.SmsTypes.Where(e => e.IsActive == true && e.Id == id).SingleOrDefaultAsync();

        }
        public async Task<bool>SendSms(List<ErpSMS> erpSM)
        {
            int r = -1;
            _db.ErpSMS.AddRange(erpSM);
            try
            {
                r = await _db.SaveChangesAsync();
                return r > 0;
            }
            catch (Exception ex)
            {

                return false;
            }
        }


        private ErpSMS MapModel(ErpSMS erpSM)
        {
            ErpSMS model = new ErpSMS();
            model.Message = erpSM.Message;
            model.PhoneNo = erpSM.PhoneNo;
            model.Status = erpSM.Status;
            model.SmsType = erpSM.SmsType;
            model.Date = erpSM.Date;
            model.Remarks = erpSM.Remarks;
            model.Subject = erpSM.Subject;
            return model;
        }
    }
}
