using KGERP.Data.CustomModel;
using KGERP.Data.Models;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace KGERP.Service.Implementation
{
    public class OfficerAssignService : IOfficerAssignService
    {
        private readonly ERPEntities context;
        public OfficerAssignService(ERPEntities context)
        {
            this.context = context;
        }

        public List<OfficerAssignModel> GetOfficerAssigns(string searchText)
        {
            IQueryable<OfficerAssign> queryable = context.OfficerAssigns.Include(x => x.Zone).Include(x => x.Employee).Where(x => x.Zone.Name.Contains(searchText) || x.Employee.Name.Contains(searchText));
            return ObjectConverter<OfficerAssign, OfficerAssignModel>.ConvertList(queryable.ToList()).ToList();
        }

        public async Task<OfficerAssignModel> OfficersAssign(int CompanyId)
        {
            OfficerAssignModel model = new OfficerAssignModel();
            model.DataList = await Task.Run(() => (from t1 in context.OfficerAssigns
                                                   join t2 in context.Employees on t1.EmpId equals t2.Id
                                                   join t3 in context.Zones on t1.ZoneId equals t3.ZoneId
                                                   join t4 in context.SubZones on t1.SubZoneId equals                                 t4.SubZoneId into t4_Join
                                                   from t4 in t4_Join.DefaultIfEmpty()
                                                   where t1.CompanyId == CompanyId
                                                   select new OfficerAssignModel
                                                   {
                                                       OfficerAssignId = t1.OfficerAssignId,
                                                       EmpId = t2.Id,
                                                       ZoneId = t3.ZoneId, 
                                                       subZoneId=t4.SubZoneId, //== 0 ? 0 : t4.SubZoneId,
                                                       EmployeeName =t2.Name,
                                                       ZoneName=t3.Name,
                                                       SubzoneName=t4.Name //== null ? "No Select" : t4.Name,
                                                   }).AsEnumerable());
            model.CompanyId= CompanyId;

            return model;
        }

        public OfficerAssignModel GetOfficerAssign(int id)
        {
            if (id <= 0)
            {
                return new OfficerAssignModel() { OfficerAssignId = id, IsActive = true };
            }
            OfficerAssign officerAssign = context.OfficerAssigns.FirstOrDefault(x => x.OfficerAssignId == id);
            return ObjectConverter<OfficerAssign, OfficerAssignModel>.Convert(officerAssign);
        }

        public bool SaveOfficerAssign(int id, OfficerAssignModel model)
        {
            if (model == null)
            {
                throw new Exception("Data missing!");
            }

            OfficerAssign officerAssign = ObjectConverter<OfficerAssignModel, OfficerAssign>.Convert(model);
            if (id > 0)
            {
                officerAssign = context.OfficerAssigns.FirstOrDefault(x => x.OfficerAssignId == id);
                if (officerAssign == null)
                {
                    throw new Exception("Data not found!");
                }
                officerAssign.ModifiedDate = DateTime.Now;
                officerAssign.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            }

            else
            {
                officerAssign.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
                officerAssign.CreatedDate = DateTime.Now;

            }
            officerAssign.CompanyId = model.CompanyId;
            officerAssign.EmpId = model.EmpId;
            officerAssign.ZoneId = model.ZoneId;
            officerAssign.Remarks = model.Remarks;
            officerAssign.StartDate = model.StartDate;
            officerAssign.EndDate = model.EndDate;
            officerAssign.IsActive = model.IsActive;
            context.Entry(officerAssign).State = officerAssign.OfficerAssignId == 0 ? EntityState.Added : EntityState.Modified;
            return context.SaveChanges() > 0;
        }

        public string GetOffierName(long EmpId)
        {
            return context.Employees.Where(x => x.Id == EmpId).First().Name;
        }

        public bool DeleteOfficerAssign(int id)
        {
            OfficerAssign officerAssign = context.OfficerAssigns.Find(id);
            if (officerAssign == null)
            {
                throw new Exception("Data not found");
            }
            context.OfficerAssigns.Remove(officerAssign);
            return context.SaveChanges() > 0;
        }

        public List<LongSelectModel> GetMarketingOfficersByCustomerZone(int customerId)
        {
            int? zoneId = context.Vendors.Where(x => x.VendorId == customerId).FirstOrDefault().ZoneId;
            if (zoneId == null)
            {
                return new List<LongSelectModel>();
            }

            return context.OfficerAssigns.Include(x => x.Employee).Where(x => x.ZoneId == zoneId).Select(x => new LongSelectModel { Text = "[" + x.Employee.EmployeeId + "] " + x.Employee.Name, Value = x.EmpId }).ToList();
        }


        public List<SelectModel> GetOfficerSelectModelsByZone(int zoneId)
        {
            return context.OfficerAssigns.Include(x => x.Employee).Where(x => x.ZoneId == zoneId).ToList().Select(x => new SelectModel()
            {
                Text = x.Employee.Name,
                Value = x.EmpId
            }).OrderBy(x => x.Text).ToList();
        }

        public List<SelectModel> GetZoneList(int CompanyId)
        {
            var res = context.Zones.Where(e => CompanyId == e.CompanyId).ToList().Select(x => new SelectModel()
            {
                Text = x.Name,
                Value = x.ZoneId
            }).ToList(); ;

            return res;
        }
        public List<SelectModel> GetSubZoneList(int Id)
        {
            var res = context.SubZones.Where(e => Id == e.ZoneId).ToList().Select(x => new SelectModel()
            {
                Text = x.Name,
                Value = x.SubZoneId
            }).ToList(); ;

            return res;
        }

        public OfficerAssignModel Assignpesron(OfficerAssignModel Model)
        {
           OfficerAssign obj = new OfficerAssign();
            obj.EmpId = Model.EmpId;
            obj.ZoneId = Model.ZoneId;
            obj.SubZoneId = Model.subZoneId;
            obj.Remarks = Model.Remarks;
            obj.StartDate = Model.StartDate;
            obj.EndDate = Model.EndDate;
            context.OfficerAssigns.Add(obj);
            context.SaveChanges();
            return null;
        }




        public List<SelectModel> GetMarketingOfficersSelectModels(int companyId)
        {
            List<OfficerAssignModel> marketingOfficers = context.Database.SqlQuery<OfficerAssignModel>(@"SELECT CAST(0 AS bigint) AS EmpId,'ALL' AS OfficerName
                                                                                           UNION
                                                                                           SELECT DISTINCT CAST(OA.EmpId AS bigint) AS EmpId,E.Name AS OfficerName
                                                                                           FROM Erp.OfficerAssign OA 
                                                                                           INNER JOIN Employee E ON OA.EmpId=E.Id
                                                                                           WHERE OA.CompanyId={0}", companyId).ToList();


            return marketingOfficers.Select(x => new SelectModel { Text = x.OfficerName, Value = x.EmpId }).ToList();
        }

        public List<SelectModel> GetMarketingOfficerSelectModelsFromOrderMaster(int companyId)
        {
            List<OfficerAssignModel> marketingOfficers = context.Database.SqlQuery<OfficerAssignModel>(@"SELECT CAST(0 AS bigint) AS EmpId,'ALL' AS OfficerName
                                                                                                         UNION
                                                                                                         SELECT DISTINCT CAST(OM.SalePersonId AS bigint) AS EmpId,E.Name AS OfficerName
                                                                                                         FROM Erp.OrderMaster OM 
                                                                                                         INNER JOIN Employee E ON OM.SalePersonId = E.Id
                                                                                                         WHERE OM.CompanyId = {0}", companyId).ToList();


            return marketingOfficers.Select(x => new SelectModel { Text = x.OfficerName, Value = x.EmpId }).ToList();
        }
    }
}
