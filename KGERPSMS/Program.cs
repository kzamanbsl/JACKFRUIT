using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmsSchedulerCore.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SmsSchedulerCore
{
    public class Program
    {
        public static IConfiguration _config;
        public static readonly ERPDbContext _db = new ERPDbContext();
        public static AppSetting appSetting { get; set; }
        private static List<VwSmslist> lstSms { get; set; }
        public static HttpClient _client = new HttpClient();
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            DateTime currentDate = DateTime.Now;
            _config = builder.Build();
            appSetting = _config.Get<AppSetting>();
            _client.BaseAddress = new Uri(appSetting.SmsAPi.baseUrl);
            //apiInfo = JsonConvert.DeserializeObject<APIInfo>(x);
            //Console.WriteLine("Hello World!");
            if (Utility.Utility.IsConnectedToInternet())
            {
                await processMultiData();
            }
        }


        //public static async Task processMultiData()
        //{
        //    try
        //    {
        //        lstSms = await _db.VwSmslists.Where(e => (e.Status == (int)EnumSmSStatus.Pending || e.Status == (int)EnumSmSStatus.Failed)
        //        && e.TryCount < appSetting.SmsAPi.retry && e.Date <= DateTime.Now).Take(40).ToListAsync();
        //        if (lstSms.Count <= 0)
        //        {
        //            return;
        //        }
        //    //    var parameters = new Dictionary<string, string> {
        //    //    { "api_key", $"{appSetting.SmsAPi.api_key}" },
        //    //    { "api_secret", $"{appSetting.SmsAPi.api_secret}" },
        //    //    {"request_type","MULTIBODY_CAMPAIGN" },
        //    //    {"message_type","TEXT" },//UNICODE
        //    //    {"campaign_title","Test" },//UNICODE            
        //    //};
        //    //    for (int i = 0; i < lstSms.Count(); i++)
        //    //    {
        //    //        int sl = i + 1;
        //    //        parameters.Add($"sms[{sl}][mobile]", lstSms[i].PhoneNo);
        //    //        parameters.Add($"sms[{sl}][message_body]", lstSms[i].Message);
        //    //    }

        //    //    var encodedContent = new FormUrlEncodedContent(parameters);

        //    //    var resp = await _client.PostAsync(appSetting.SmsAPi.postUrl, encodedContent);
        //        //if (resp.IsSuccessStatusCode)
        //        //{
        //            //MultiSmsSuccessResponse sMSResp = new MultiSmsSuccessResponse();
        //            //sMSResp = await resp.Content.ReadFromJsonAsync<MultiSmsSuccessResponse>();
        //            //var finallist = sMSResp.invalid_numbers.Distinct().ToList();
        //            //for (int i = 0; i < finallist.Count(); i++)
        //            //{

        //            //    //lstSms.Where(o => o.PhoneNo == finallist[i]).ToList().ForEach(e => e=new VwSmslist
        //            //    //{
        //            //    //    CompanyId = e.CompanyId,
        //            //    //    CompanyName = e.CompanyName,
        //            //    //    Date = e.Date,
        //            //    //    Id = e.Id,
        //            //    //    Message = e.Message,
        //            //    //    PhoneNo = e.PhoneNo,
        //            //    //    Remarks = e.Remarks,
        //            //    //    RowTime = e.RowTime,
        //            //    //    SmstypeName = e.SmstypeName,
        //            //    //    Status = (int)EnumSmSStatus.Failed,
        //            //    //    Subject = e.Subject,
        //            //    //    TryCount = e.TryCount + 1
        //            //    //});
        //            //    for (int j = 0; j < lstSms.Where(o => o.PhoneNo == finallist[i]).ToList().Count(); j++)
        //            //    {
        //            //        lstSms.Where(o => o.PhoneNo == finallist[i]).ToList()[j].Status = (int)EnumSmSStatus.Failed;
        //            //        var c = lstSms.Where(o => o.PhoneNo == finallist[i]).ToList()[j].TryCount;
        //            //        lstSms.Where(o => o.PhoneNo == finallist[i]).ToList()[j].TryCount = c + 1;
        //            //    }

        //            //    //var temp = lstSms.FirstOrDefault(e => e.PhoneNo == finallist[i]);
        //            //    //if (temp != null)
        //            //    //{
        //            //    //    int index = lstSms.IndexOf(temp);
        //            //    //    temp.Status = (int)EnumSmSStatus.Failed;
        //            //    //    temp.TryCount = temp.TryCount + 1;
        //            //    //    lstSms[index] = temp;
        //            //    //}
        //            //}
        //            lstSms.Where(e => e.Status != (int)EnumSmSStatus.Failed)
        //                .ToList().ForEach(c => c.Status = (int)EnumSmSStatus.Success);

        //            string Remarks = $"Successfully Send Massage {lstSms.Where(e => e.Status == (int)EnumSmSStatus.Success).Count()}" +
        //                $" and Failed {lstSms.Where(e => e.Status == (int)EnumSmSStatus.Failed).Count()} of Total {lstSms.Count()}";
        //            string xmlData = ListToXml.ToXml<List<VwSmslist>>(lstSms, "ds");
        //            await UpdateInDatabase(xmlData, 1, Remarks);
        //        //}
        //        //else
        //        //{
        //        //    MultiSmsFailedResponse sMSResp = new MultiSmsFailedResponse();
        //        //    sMSResp = await resp.Content.ReadFromJsonAsync<MultiSmsFailedResponse>();
        //        //    for (int i = 0; i < lstSms.Count(); i++)
        //        //    {
        //        //        var temp = lstSms[i];
        //        //        temp.Status = (int)EnumSmSStatus.Failed;
        //        //        temp.TryCount = temp.TryCount + 1;
        //        //        lstSms[i] = temp;
        //        //    }
        //        //    string xmlData = ListToXml.ToXml<List<VwSmslist>>(lstSms, "ds");
        //        //    await UpdateInDatabase(xmlData, 2, sMSResp.error.error_code + " " + sMSResp.error.error_message);
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        await UpdateInDatabase("", 2, ex.Message);
        //    }
        //    //MULTIBODY_CAMPAIGN


        //}
        public static async Task processMultiData()
        {
            try
            {
                lstSms = await _db.VwSmslists.Where(e => (e.Status == (int)EnumSmSStatus.Pending || e.Status == (int)EnumSmSStatus.Failed)
                && e.TryCount < appSetting.SmsAPi.retry && e.Date <= DateTime.Now).Take(40).ToListAsync();
                if (lstSms.Count <= 0)
                {
                    return;
                }
                var parameters = new Dictionary<string, string> {
                { "api_key", $"{appSetting.SmsAPi.api_key}" },
                { "api_secret", $"{appSetting.SmsAPi.api_secret}" },
                {"request_type","MULTIBODY_CAMPAIGN" },
                {"message_type","TEXT" },//UNICODE
                {"campaign_title","Test" },//UNICODE            
            };
                for (int i = 0; i < lstSms.Count(); i++)
                {
                    int sl = i + 1;
                    parameters.Add($"sms[{sl}][mobile]", lstSms[i].PhoneNo);
                    parameters.Add($"sms[{sl}][message_body]", lstSms[i].Message);
                }

                var encodedContent = new FormUrlEncodedContent(parameters);

                var resp = await _client.PostAsync(appSetting.SmsAPi.postUrl, encodedContent);
                if (resp.IsSuccessStatusCode)
                {
                    MultiSmsSuccessResponse sMSResp = new MultiSmsSuccessResponse();
                    sMSResp = await resp.Content.ReadFromJsonAsync<MultiSmsSuccessResponse>();
                    var finallist = sMSResp.invalid_numbers.Distinct().ToList();
                    for (int i = 0; i < finallist.Count(); i++)
                    {

                        //lstSms.Where(o => o.PhoneNo == finallist[i]).ToList().ForEach(e => e=new VwSmslist
                        //{
                        //    CompanyId = e.CompanyId,
                        //    CompanyName = e.CompanyName,
                        //    Date = e.Date,
                        //    Id = e.Id,
                        //    Message = e.Message,
                        //    PhoneNo = e.PhoneNo,
                        //    Remarks = e.Remarks,
                        //    RowTime = e.RowTime,
                        //    SmstypeName = e.SmstypeName,
                        //    Status = (int)EnumSmSStatus.Failed,
                        //    Subject = e.Subject,
                        //    TryCount = e.TryCount + 1
                        //});
                        for (int j = 0; j < lstSms.Where(o => o.PhoneNo == finallist[i]).ToList().Count(); j++)
                        {
                            lstSms.Where(o => o.PhoneNo == finallist[i]).ToList()[j].Status = (int)EnumSmSStatus.Failed;
                            var c = lstSms.Where(o => o.PhoneNo == finallist[i]).ToList()[j].TryCount;
                            lstSms.Where(o => o.PhoneNo == finallist[i]).ToList()[j].TryCount = c + 1;
                        }

                        //var temp = lstSms.FirstOrDefault(e => e.PhoneNo == finallist[i]);
                        //if (temp != null)
                        //{
                        //    int index = lstSms.IndexOf(temp);
                        //    temp.Status = (int)EnumSmSStatus.Failed;
                        //    temp.TryCount = temp.TryCount + 1;
                        //    lstSms[index] = temp;
                        //}
                    }
                    lstSms.Where(e => e.Status != (int)EnumSmSStatus.Failed)
                        .ToList().ForEach(c => c.Status = (int)EnumSmSStatus.Success);

                    string Remarks = $"Successfully Send Massage {lstSms.Where(e => e.Status == (int)EnumSmSStatus.Success).Count()}" +
                        $" and Failed {lstSms.Where(e => e.Status == (int)EnumSmSStatus.Failed).Count()} of Total {lstSms.Count()}";
                    string xmlData = ListToXml.ToXml<List<VwSmslist>>(lstSms, "ds");
                    await UpdateInDatabase(xmlData, 1, Remarks);
                }
                else
                {
                    MultiSmsFailedResponse sMSResp = new MultiSmsFailedResponse();
                    sMSResp = await resp.Content.ReadFromJsonAsync<MultiSmsFailedResponse>();
                    for (int i = 0; i < lstSms.Count(); i++)
                    {
                        var temp = lstSms[i];
                        temp.Status = (int)EnumSmSStatus.Failed;
                        temp.TryCount = temp.TryCount + 1;
                        lstSms[i] = temp;
                    }
                    string xmlData = ListToXml.ToXml<List<VwSmslist>>(lstSms, "ds");
                    await UpdateInDatabase(xmlData, 2, sMSResp.error.error_code + " " + sMSResp.error.error_message);
                }
            }
            catch (Exception ex)
            {
                await UpdateInDatabase("", 2, ex.Message);
            }
            //MULTIBODY_CAMPAIGN


        }
        public static async Task UpdateInDatabase(string Xml, int Type = 1, string Remarks = "")
        {

            Storeproc storeproc = new Storeproc();
            SqlConnection connection = new SqlConnection(appSetting.ConnectionStrings.KGERPDB);
            SqlParameter[] parameters = new SqlParameter[3];
            SqlParameter ProcId = new SqlParameter(parameterName: "@ProcID", dbType: System.Data.SqlDbType.NVarChar);
            SqlParameter Dxml01 = new SqlParameter(parameterName: "@Dxml01", dbType: System.Data.SqlDbType.NVarChar);
            SqlParameter Desc01 = new SqlParameter(parameterName: "@Desc01", dbType: System.Data.SqlDbType.NVarChar);

            ProcId.Value = Type == 1 ? "UPDATESUCCESS01" : "UPDATEFAILED01";
            Dxml01.Value = Xml;
            Desc01.Value = Remarks;
            parameters[0] = ProcId;
            parameters[1] = Dxml01;
            parameters[2] = Desc01;

            var result = await Task.Run(() => (storeproc.GetDataSet(connection, "[dbo].[SMSInfoUpdate]", parameters)));
        }


        //public static async Task processData()
        //{

        //    //MULTIBODY_CAMPAIGN
        //    lstSms = await _db.VwSmslists.Where(e => e.Status == (int)EnumSmSStatus.Pending).Take(40).ToListAsync();
        //    var parameters = new Dictionary<string, string> { { "api_key", $"{appSetting.SmsAPi.api_key}" },
        //        { "api_secret", $"{appSetting.SmsAPi.api_secret}" },
        //        {"request_type","SINGLE_SMS" },
        //        {"message_type","TEXT" },//UNICODE
        //        {"mobile",$"{lstSms[0].PhoneNo}" },
        //        {"message_body",$"{lstSms[0].Message}" }

        //    };
        //    var encodedContent = new FormUrlEncodedContent(parameters);
        //    SingleSMSResp sMSResp = new SingleSMSResp();
        //    var resp = await _client.PostAsync(appSetting.SmsAPi.postUrl, encodedContent);
        //    if (resp.IsSuccessStatusCode)
        //    {
        //        sMSResp = await resp.Content.ReadFromJsonAsync<SingleSMSResp>();
        //    }
        //    else
        //    {
        //        sMSResp = await resp.Content.ReadFromJsonAsync<SingleSMSResp>();
        //    }

        //}
    }
}
