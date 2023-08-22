using KGERP.Data.CustomModel;
using KGERP.Data.Models;
using KGERP.Service.Implementation.Accounting;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;
using KGERP.Service.Implementation.Configuration;
using System.Text.RegularExpressions;

namespace KGERP.Service.Implementation
{
    public class VendorService : IVendorService
    {
        private readonly ERPEntities _context;
        private readonly ConfigurationService _configurationService;
        public VendorService(ERPEntities context)
        {
            this._context = context;
            _configurationService = new ConfigurationService(context);
        }

        public List<VendorModel> GetAllCustomers(int vendorTypeId)
        {
            return _context.Database.SqlQuery<VendorModel>("exec spGetAllCustomerList {0}", vendorTypeId).ToList();
        }

        //public IEnumerable<VendorModel> GetVendors(int companyId, int vendorTypeId, string searchText, bool isActive)
        //{
        //    IQueryable<VendorModel> vendors = context.Database.SqlQuery<VendorModel>("exec spGetVendorList {0},{1},{2}", companyId, vendorTypeId, isActive).AsQueryable();
        //    return vendors.Where(x =>
        //                  (x.Name.ToLower().Contains(searchText.ToLower()) || string.IsNullOrEmpty(searchText)) ||
        //                  (x.Name.ToLower().Contains(searchText.ToLower()) || string.IsNullOrEmpty(searchText)) ||
        //                  (x.Phone.ToLower().Contains(searchText.ToLower()) || string.IsNullOrEmpty(searchText)) ||
        //                  (x.Code.ToLower().Contains(searchText.ToLower()) || string.IsNullOrEmpty(searchText)) ||
        //                  (x.CustomerType.ToLower().Contains(searchText.ToLower()) || string.IsNullOrEmpty(searchText))).ToList();
        //}

        public async Task<VendorModel> GetVendors(int companyId, int vendorTypeId)
        {
            VendorModel vmCommonCustomer = new VendorModel();
            vmCommonCustomer.CompanyId = companyId;
            vmCommonCustomer.DataList = await Task.Run(() => (from t1 in _context.Vendors.Where(x => x.IsActive == true && x.VendorTypeId == vendorTypeId && x.CompanyId == companyId)
                                                              join t2 in _context.Upazilas on t1.UpazilaId equals t2.UpazilaId into t2_def
                                                              from t2 in t2_def.DefaultIfEmpty()
                                                              join t3 in _context.Districts on t2.DistrictId equals t3.DistrictId into t3_def
                                                              from t3 in t3_def.DefaultIfEmpty()
                                                              join t4 in _context.Divisions on t3.DivisionId equals t4.DivisionId into t4_def
                                                              from t4 in t4_def.DefaultIfEmpty()
                                                              join t6 in _context.Zones on t1.ZoneId equals t6.ZoneId into t6_def
                                                              from t6 in t6_def.DefaultIfEmpty()
                                                              join t7 in _context.HeadGLs on t1.HeadGLId equals t7.Id
                                                              join t8 in _context.Head5 on t7.ParentId equals t8.Id
                                                              join t9 in _context.Head4 on t8.ParentId equals t9.Id


                                                              select new VendorModel
                                                              {
                                                                  CountryId = t8.Id,
                                                                  NomineeName = t8.AccName + " " + t9.AccName,
                                                                  VendorId = t1.VendorId,
                                                                  Name = t1.Name,
                                                                  Email = t1.Email,
                                                                  ContactName = t1.ContactName,
                                                                  Address = t1.Address,
                                                                  Code = t1.Code,
                                                                  DistrictId = t2.DistrictId,
                                                                  UpazilaId = t1.UpazilaId.Value,
                                                                  DistrictName = t3.Name,
                                                                  UpazilaName = t2.Name,
                                                                  CountryName = t4.Name,
                                                                  CreatedBy = t1.CreatedBy,
                                                                  Remarks = t1.Remarks,
                                                                  CompanyId = t1.CompanyId,
                                                                  Phone = t1.Phone,
                                                                  ZoneName = t6.Name,
                                                                  CreditLimit = t1.CreditLimit,
                                                                  NID = t1.NID,
                                                                  SecurityAmount = t1.SecurityAmount,
                                                                  CustomerStatus = t1.CustomerStatus ?? 1,
                                                                  Propietor = t1.Propietor,
                                                                  HeadGLId = t1.HeadGLId,
                                                                  VendorTypeId = t1.VendorTypeId
                                                              }).OrderByDescending(x => x.VendorId).AsEnumerable());



            return vmCommonCustomer;
        }



        public VendorModel GetVendor(int id)
        {
            Vendor vendor = _context.Vendors.Find(id);
            if (vendor == null)
            {
                return new VendorModel();
            }
            return ObjectConverter<Vendor, VendorModel>.Convert(vendor);
        }
        public async Task<bool> SaveVendor(int id, VendorModel model, string message)
        {
            int noOfRowsAffected = 0;
            message = string.Empty;
            Vendor vendor = ObjectConverter<VendorModel, Vendor>.Convert(model);




            if (id > 0)
            {
                vendor = _context.Vendors.FirstOrDefault(x => x.VendorId == id);
                if (vendor == null)
                {
                    throw new Exception("Data not found!");
                }
                vendor.ModifiedDate = DateTime.Now;
                vendor.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            }

            else
            {
                vendor.CreatedDate = DateTime.Now;
                vendor.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
            }

            if (model.Code == null)
            {
                HeadGL gl = await _context.HeadGLs.FindAsync(model.HeadGLId);
                if (gl != null)
                {
                    vendor.Code = gl.AccCode;
                }
            }
            else
            {
                vendor.Code = model.Code;
            }


            vendor.HeadGLId = model.HeadGLId;
            vendor.CustomerType = model.CustomerType;
            vendor.CreditCommission = model.CreditCommission ?? 0;
            vendor.DistrictId = model.DistrictId;
            vendor.UpazilaId = model.UpazilaId;
            vendor.VendorTypeId = model.VendorTypeId;
            vendor.ZoneId = model.ZoneId;
            vendor.SubZoneId = model.SubZoneId;
            vendor.Name = model.Name;
            vendor.NID = model.NID;
            vendor.CountryId = model.CountryId;
            vendor.State = model.State;
            vendor.ContactName = model.ContactName;
            vendor.Phone = model.Phone;
            vendor.Email = model.Email;
            vendor.Address = model.Address;
            vendor.Remarks = model.Remarks;

            vendor.NomineeName = model.NomineeName;
            vendor.NomineePhone = model.NomineePhone;
            vendor.CreditRatioFrom = model.CreditRatioFrom;
            vendor.CreditRatioTo = model.CreditRatioTo;
            vendor.CreditLimit = model.CreditLimit;
            vendor.MonthlyTarget = model.MonthlyTarget;
            vendor.YearlyTarget = model.YearlyTarget;
            vendor.MonthlyIncentive = model.MonthlyIncentive;
            vendor.YearlyIncentive = model.YearlyIncentive;
            vendor.ImageUrl = model.ImageUrl;
            vendor.NomineeImageUrl = model.NomineeImageUrl;
            vendor.ClosingTime = model.ClosingTime;
            vendor.NoOfCheck = model.NoOfCheck;
            vendor.CheckNo = model.CheckNo;
            vendor.IsActive = model.IsActive;
            vendor.IsForeign = model.CountryId == 19 ? false : true; //19=Bangladesh
            _context.Entry(vendor).State = vendor.VendorId == 0 ? EntityState.Added : EntityState.Modified;

            try
            {
                noOfRowsAffected = await _context.SaveChangesAsync();
                if (noOfRowsAffected > 0 && model.VendorTypeId == (int)Provider.Customer)
                {
                    //Setting up Customer Offer
                    await _context.Database.ExecuteSqlCommandAsync("exec spSetCustomerCommission {0},{1},{2}", vendor.CompanyId, vendor.VendorId, vendor.CreatedBy);
                }


                if (vendor.CompanyId == (int)CompanyName.KrishibidFeedLimited)
                {
                    var zone = await _context.Zones.FindAsync(vendor.ZoneId);
                    var parentId = zone.HeadGLId;
                    VMHeadIntegration integration = new VMHeadIntegration
                    {
                        AccName = vendor.Name,
                        LayerNo = 6,
                        Remarks = "GL Layer",
                        IsIncomeHead = false,
                        ParentId = parentId,

                        CompanyFK = vendor.CompanyId,
                        CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                        CreatedDate = DateTime.Now,
                    };
                    HeadGL headGlId = await _configurationService.PayableHeadIntegrationAdd(integration);
                    //if (headGlId != null)
                    //{
                    //    await VendorsCodeAndHeadGLIdEdit(commonCustomer.VendorId, headGlId);
                    //}
                }

                return noOfRowsAffected > 0;
            }

            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage);
                    }
                }
                message = "Data not saved";
                return false;

            }
        }

        public List<CustomerReceivableCustomModel> GetCustomerReceivables(int vendorId)
        {
            List<CustomerReceivableCustomModel> customerReceivable = _context.Database.SqlQuery<CustomerReceivableCustomModel>("exec spGetCustomerReceiableLadger {0}", vendorId).ToList();
            return customerReceivable;
        }

        public List<CustomerLedgerCustomModel> GetCustomerLedger(int id)
        {
            List<CustomerLedgerCustomModel> customerLedgers = _context.Database.SqlQuery<CustomerLedgerCustomModel>("exec spGetCustomerLedger {0}", id).ToList();
            return customerLedgers;
        }

        public object GetCustomerAutoComplete(string prefix, int companyId)
        {

            return _context.Vendors.Where(x => x.CompanyId == companyId && x.IsActive && x.VendorTypeId == (int)Provider.Customer && x.Name.Contains(prefix)).Select(x => new
            {
                label = x.Name,
                val = x.VendorId
            }).OrderBy(x => x.label).Take(20).ToList();
        }

        public object CustomerAssociatesCustomerId(int customerId)
        {

            return _context.Vendors.Where(x => (x.VendorReferenceId == customerId || x.VendorId == customerId) && x.IsActive).Select(x => new
            {
                label = x.Name + (x.Phone != null ? " Phone: " + x.Phone : "") + (x.NID != null ? " NID: " + x.NID : ""),
                val = x.VendorId
            }).OrderBy(x => x.label).ToList();

        }
        public object GetClientAutoComplete(string prefix, int companyId)
        {

            return _context.KGRECustomers.Where(x => x.CompanyId == companyId && x.FullName.Contains(prefix)).Select(x => new
            {
                label = x.FullName,
                val = x.ClientId
            }).OrderBy(x => x.label).Take(20).ToList();
        }
        public object GetSupplierAutoComplete(string prefix, int companyId)
        {
            return _context.Vendors.Where(x => x.CompanyId == companyId
            && x.IsActive
            && x.VendorTypeId == 1
            && x.Name.Contains(prefix))
                .Select(x => new
                {
                    label = x.Name,
                    val = x.VendorId
                }).Take(20).ToList();
        }

        public VendorModel GetVendorByType(int id, int vendorTypeId)
        {
            Vendor vendor = _context.Vendors.FirstOrDefault(x => x.VendorId == id && x.VendorTypeId == vendorTypeId);

            if (vendor == null)
            {
                if (vendorTypeId == (int)Provider.Supplier)
                {
                    return new VendorModel { IsActive = true, VendorTypeId = vendorTypeId, SupplierOrCustomer = "Supplier" };
                }
                else if (vendorTypeId == (int)Provider.Customer)
                {
                    return new VendorModel
                    {
                        IsActive = true,
                        VendorTypeId = vendorTypeId,
                        SupplierOrCustomer = "Customer",
                        MonthlyIncentive = "As per company policy",
                        YearlyIncentive = "As per company policy",
                        Condition = "Condition : If customer fails to 100% closing, any incentive, carrying and any other adjustment will not be adjusted."
                    };
                }
                else
                {
                    return new VendorModel { IsActive = true, VendorTypeId = vendorTypeId, SupplierOrCustomer = "RentCompany" };
                }


            }

            vendor.ImageUrl = vendor.ImageUrl ?? "~/Images/VendorImage/default.png";
            vendor.NomineeImageUrl = vendor.NomineeImageUrl ?? "~/Images/VendorImage/default.png";

            return ObjectConverter<Vendor, VendorModel>.Convert(vendor);
        }
        public async Task<VendorModel> GetCustomerPayments(int companyId)
        {
            VendorModel vendorModel = new VendorModel();

            vendorModel.DataList = await Task.Run(() => (from t1 in _context.Vendors
                                                         join t2 in _context.Payments on t1.VendorId equals t2.PaymentId into pv
                                                         from t2 in pv.DefaultIfEmpty()

                                                         where t1.CompanyId == companyId
                                                         select new VendorModel
                                                         {
                                                             VendorId = t1.VendorId,
                                                             CompanyId = t1.CompanyId,
                                                             Name = t1.Name,
                                                             Code = t1.Code,
                                                             Address = t1.Address,
                                                             Phone = t1.Phone,
                                                             LastPaymentDate = t1.LastPaymentDate,
                                                             Balance = (t2.OutAmount ?? 0) - t2.InAmount,
                                                             IsActive = t1.IsActive,
                                                         }).AsEnumerable());

            return vendorModel;
        }
        public List<VendorModel> GetCustomerPayments(string searchText, int companyId, int customer)
        {
            return _context.Database.SqlQuery<VendorModel>("spGetCustomerAccounts {0},{1}", searchText, companyId).ToList();

            //IQueryable<Vendor> vendors = context.Vendors
            //    .Include(x => x.Payments)
            //    .Where(x => x.CompanyId == companyId && x.VendorTypeId == customer && x.IsActive &&
            //    (x.Name.Contains(searchText) ||
            //     x.ContactName.Contains(searchText) || x.Phone.Contains(searchText) ||
            //     x.Code.Contains(searchText) || x.CustomerType.Contains(searchText)));
            //if (vendors == null)
            //{
            //    return new List<VendorModel>();
            //}
            //List<VendorModel> models = ObjectConverter<Vendor, VendorModel>.ConvertList(vendors.ToList()).ToList();

            //return models;
        }

        public List<SelectModel> GetVendorSelectModels(int vendorTypeId)
        {
            int companyId = Convert.ToInt32(System.Web.HttpContext.Current.Session["CompanyId"]);
            return _context.Vendors.Where(x => x.CompanyId == companyId && x.VendorTypeId == vendorTypeId).ToList().Select(x => new SelectModel()
            {
                Text = x.Name,
                Value = x.VendorId
            }).ToList();
        }

        public List<SelectModel> GetCustomerSelectModel(int companyId, int vendorType)
        {
            return _context.Vendors.ToList().Where(x => x.CompanyId == companyId && x.VendorTypeId == vendorType).OrderBy(x => x.Name).Select(x => new SelectModel()
            {
                Text = string.Format("[{0}] {1}", x.Code, x.Name),
                Value = x.VendorId
            }).OrderBy(x => x.Text).ToList();
        }


        public string GetAutoGeneratedVendorCode(int companyId, int upazilaId, int vendorTypeId)
        {
            string code = string.Empty;
            Vendor lastVendor = _context.Vendors.Where(x => x.CompanyId == companyId && x.UpazilaId == upazilaId && x.VendorTypeId == vendorTypeId).OrderByDescending(x => x.Code).FirstOrDefault();

            string customerPrefix = vendorTypeId == 1 ? "S" : "C";
            if (lastVendor == null)
            {
                Upazila upazila = _context.Upazilas.Find(upazilaId);
                return customerPrefix + upazila.Code + "01";

            }
            code = GenerateCustomerCode(lastVendor.Code);


            return code;
        }

        private string GenerateCustomerCode(string lastVendorCode)
        {
            string prefix = lastVendorCode.Substring(0, 5);
            int code = Convert.ToInt32(lastVendorCode.Substring(5, 2));
            code = ++code;
            return prefix + code.ToString().PadLeft(2, '0');

        }

        public VendorModel GetSupplier(int supplierId)
        {
            Vendor vendor = _context.Vendors.Find(supplierId);
            if (vendor == null)
            {
                return new VendorModel();
            }
            VendorModel model = ObjectConverter<Vendor, VendorModel>.Convert(vendor);
            model.Message = _context.Database.SqlQuery<string>(@"exec spPurchaseOrderOpenBySupplier {0}", model.VendorId).FirstOrDefault();
            return model;
        }

        public VendorModel GetVendorPaymentStatus(int id)
        {
            Vendor vendor = _context.Vendors.Find(id);
            if (vendor == null)
            {
                return new VendorModel();
            }

            vendor.PaymentDue = _context.Database.SqlQuery<decimal>(@"select isnull( cast ((sum(OutAmount)-sum(InAmount)) as decimal),0) as PaymentDue from Erp.Payment where VendorId={0}", vendor.VendorId).ToList().FirstOrDefault();
            return ObjectConverter<Vendor, VendorModel>.Convert(vendor);
        }

        public List<MonthSelectModel> GetMonthSelectModes()
        {
            return new List<MonthSelectModel>
            {
                new MonthSelectModel{Text="January",Value="January" },
                new MonthSelectModel{Text="February",Value="February" },
                new MonthSelectModel{Text="March",Value="March" },
                new MonthSelectModel{Text="April",Value="April" },
                new MonthSelectModel{Text="May",Value="May" },
                new MonthSelectModel{Text="June",Value="June" },
                new MonthSelectModel{Text="July",Value="July" },
                new MonthSelectModel{Text="August",Value="August" },
                new MonthSelectModel{Text="September",Value="September" },
                new MonthSelectModel{Text="October",Value="October" },
                new MonthSelectModel{Text="November",Value="November" },
                new MonthSelectModel{Text="December",Value="December" },
            };
        }

        public bool BulkCustomerSave(List<VendorModel> models)
        {
            if (!models.Any()) { return false; }

            int companyId = models.First().CompanyId;
            List<SubZone> subZones = _context.SubZones.Where(x => x.IsActive == true && x.CompanyId == companyId).ToList();
            List<dynamic> subZonesList = new List<dynamic>();

            foreach (var item in subZones)
            {
                var obj = new { item.SubZoneId, Name = Regex.Replace(item.Name.Trim().ToUpper(), @"\s+", ""), item.RegionId, item.ZoneId };
                subZonesList.Add(obj);
            }

            List<Upazila> upazilas = _context.Upazilas.Where(x => x.IsActive == true).ToList();
            List<Upazila> upazilaList = new List<Upazila>();

            foreach (var item in upazilas)
            {
                Upazila obj = new Upazila {UpazilaId = item.UpazilaId, Name = Regex.Replace(item.Name.Trim().ToUpper(), @"\s+", "")};
                upazilaList.Add(obj);
            }

            List<District> districts = _context.Districts.Where(x => x.IsActive == true).ToList();
            List<District> districtList = new List<District>();

            foreach (var item in districts)
            {
                District obj = new District {DistrictId = item.DistrictId, Name = Regex.Replace(item.Name.Trim().ToUpper(), @"\s+", "") };
                districtList.Add(obj);
            }

            foreach (var model in models)
            {
                if (!string.IsNullOrEmpty(model.SubZoneName))
                {
                    model.SubZoneName = model.SubZoneName.Trim();
                    var subZoneName = model.SubZoneName.ToUpperInvariant();
                    subZoneName = Regex.Replace(subZoneName, @"\s+", "");
                    //var matchedNameObj = subZonesList.FirstOrDefault(x => x.Name.Substring(3).Equals(subZoneName));
                    var matchedNameObj = subZonesList.FirstOrDefault(x => x.Name.Equals(subZoneName));
                    if (matchedNameObj != null)
                    {
                        model.ZoneId = matchedNameObj?.ZoneId ?? null;
                        model.RegionId = matchedNameObj?.RegionId ?? null;
                        model.SubZoneId = matchedNameObj?.SubZoneId ?? null;
                    }
                }

                if (!string.IsNullOrEmpty(model.UpazilaName))
                {
                    model.UpazilaName = model.UpazilaName.Trim();
                    var upazilaName = model.UpazilaName.ToUpperInvariant();
                    upazilaName = Regex.Replace(upazilaName, @"\s+", "");
                    var matchedNameObj = upazilaList.FirstOrDefault(x => x.Name.Equals(upazilaName));
                    if (matchedNameObj != null)
                    {
                        model.UpazilaId = matchedNameObj?.UpazilaId ?? null;
                    }
                }

                if (!string.IsNullOrEmpty(model.DistrictName))
                {
                    model.DistrictName = model.DistrictName.Trim();
                    var districtName = model.DistrictName.ToUpperInvariant();
                    districtName = Regex.Replace(districtName, @"\s+", "");
                    var matchedNameObj = districtList.FirstOrDefault(x => x.Name.Equals(districtName));
                    if (matchedNameObj != null)
                    {
                        model.DistrictId = matchedNameObj?.DistrictId ?? null;
                    }
                }
            }

            List<Vendor> vendors = ObjectConverter<VendorModel, Vendor>.ConvertList(models.ToList()).ToList();

            if (!vendors.Any()) { throw new Exception("Customer is Missing!"); }
            if (!vendors.Any(c => c.SubZoneId > 0)) { throw new Exception("Customer Territory Missing!"); }

            var subZoneIds = vendors.Select(c => c.SubZoneId).ToList();

            if (vendors.Count() != subZoneIds.Count()) { throw new Exception("Customer Territory Missing!"); }

            //Accounts Receivable Seed Head4 Id = ??
            List<SubZone> customerSubZones = _context.SubZones.Where(c => subZoneIds.Contains(c.SubZoneId)).ToList();
            var hGlId = _context.Database.SqlQuery<int>("spGetNewId").FirstOrDefault();

            using (var scope = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.Vendors.AddRange(vendors);
                    var vendorSaveResult = _context.SaveChanges();

                    if (vendorSaveResult > 0 && companyId == (int)CompanyName.KrishibidSeedLimited || companyId == (int)CompanyName.GloriousCropCareLimited || companyId == (int)CompanyName.KrishibidFeedLimited)
                    {

                        List<HeadGL> headGls = new List<HeadGL>();
                        int acCodeSlNo = 1;
                        int orderNo = 0;

                        foreach (var vendor in vendors)
                        {
                            string newAccountCode = "";
                           

                            var accountHeadId = customerSubZones.FirstOrDefault(c => c.SubZoneId == vendor.SubZoneId)?.AccountHeadId;
                            Head5 parentHead = _context.Head5.FirstOrDefault(x => x.Id == accountHeadId);

                            List<HeadGL> childHeads = _context.HeadGLs.Where(x => x.ParentId == accountHeadId).ToList();


                            if (childHeads.Any())
                            {
                                string lastAccCode = childHeads.OrderByDescending(x => x.AccCode).FirstOrDefault()?.AccCode;
                                string parentPart = lastAccCode?.Substring(0, 10);
                                string childPart = lastAccCode?.Substring(10, 3);

                                newAccountCode = parentPart + (Convert.ToInt32(childPart) + acCodeSlNo).ToString().PadLeft(3, '0');
                            }
                            else
                            {
                                var genCode = acCodeSlNo.ToString().PadLeft(3, '0');
                                newAccountCode = parentHead?.AccCode + genCode;
                            }
                           

                            var totalAccHead = headGls.Count(c => c.ParentId == accountHeadId);
                            if (totalAccHead == 0)
                            {
                                orderNo = 1;
                                acCodeSlNo=1;
                            }
                            else
                            {
                                orderNo++;
                                acCodeSlNo++;
                            }

                            HeadGL headGl = new HeadGL
                            {
                                Id = hGlId,
                                AccName = vendor.Name,
                                AccCode = newAccountCode,
                                OrderNo = orderNo,
                                LayerNo = 6,
                                IsIncomeHead = false,
                                ParentId = accountHeadId,
                                Remarks = "GL Layer",
                                CompanyId = companyId,

                                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                                CreateDate = DateTime.Now,

                                IsActive = true

                            };

                            headGls.Add(headGl);
                            hGlId++;
                        }

                        _context.HeadGLs.AddRange(headGls);
                        _context.SaveChanges();

                        var index = 0;
                        foreach (var hGl in headGls)
                        {
                            // Select vendor by index is very risky but can't found any alternative way!
                            vendors[index].HeadGLId = hGl.Id;
                            vendors[index].Code = hGl.AccCode;

                            vendors[index].ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                            vendors[index].ModifiedDate = DateTime.Now;
                            index++;
                        }

                        _context.SaveChanges();

                        scope.Commit();
                        return true;
                    }

                }
                catch (Exception e)
                {
                    scope.Rollback();
                    return false;
                }

            }

            return false;

        }


        public List<SelectModel> GetCustomerSelectModelsByCompany(int companyId, int customer)
        {
            return _context.Vendors.OrderBy(x => x.Name).Where(x => x.CompanyId == companyId && x.VendorTypeId == customer && x.IsActive).ToList().Select(x => new SelectModel()
            {
                Text = x.Name,
                Value = x.VendorId
            }).ToList();

        }


        public List<SelectModel> GetCustomerNameSelectModel(int? companyId, int customer)
        {
            return _context.Vendors.ToList().Where(x => x.CompanyId == companyId && x.VendorTypeId == customer).OrderBy(x => x.Name).Select(x => new SelectModel()
            {
                Text = x.Name,
                Value = x.VendorId
            }).OrderBy(x => x.Text).ToList();
        }

        public bool DeleteVendor(int id)
        {
            int noOfRowsDeleted = 0;
            Vendor vendor = _context.Vendors.First(x => x.VendorId == id);
            if (vendor == null)
            {
                return false;
            }
            _context.Vendors.Remove(vendor);
            try
            {
                noOfRowsDeleted = _context.SaveChanges();
            }
            catch (Exception)
            {
                return false;
            }
            return noOfRowsDeleted > 0;

        }

        public List<SelectModel> GetCustomerSelectModels(int companyId, string productType)
        {
            List<SelectModel> selectModels = new List<SelectModel>();
            selectModels.Add(new SelectModel { Text = "ALL", Value = 0 });
            List<VendorModel> customers = _context.Database.SqlQuery<VendorModel>(@"SELECT     DISTINCT 
                                                                            V.VendorId,
	  	                                                                    V.Name 
                                                                  FROM       Erp.OrderDeliver OD
                                                                  INNER JOIN Erp.OrderDeliverDetail ODD ON OD.OrderDeliverId = ODD.OrderDeliverId
                                                                  INNER JOIN Erp.OrderMaster OM ON OD.OrderMasterId = OM.OrderMasterId
                                                                  INNER JOIN Erp.Vendor V ON OM.CustomerId = V.VendorId
                                                                  WHERE OD.ProductType = {0} AND OD.CompanyId = {1} 
                                                                  ORDER BY V.Name", productType, companyId).ToList();



            List<SelectModel> customerSelectModels = customers.Select(x => new SelectModel { Text = x.Name, Value = x.VendorId }).ToList();
            selectModels.AddRange(customerSelectModels);
            return selectModels;
        }

        public object GetRentCompanyAutoComplete(string prefix, int companyId)
        {
            return _context.Vendors.Where(x => x.CompanyId == companyId && x.IsActive && x.VendorTypeId == 3 && x.Name.Contains(prefix)).Select(x => new
            {
                label = x.Name,
                val = x.VendorId
            }).Take(20).ToList();
        }

        public async Task<VendorDeedListVm> GetAllVendorDeed(int companyId)
        {
            var dataList = new VendorDeedListVm();
            dataList.DataList = await Task.Run(() => (from t1 in _context.VendorDeeds
                                                      join t2 in _context.Vendors on t1.VendorId equals t2.VendorId
                                                      where t1.CompanyId == companyId
                                                      && t1.IsActive
                                                      select new VendorDeedVm
                                                      {
                                                          CompanyId = t1.CompanyId,
                                                          VendorDeedId = t1.VendorDeedId,
                                                          MonthlyTarget = t1.MonthlyTarget ?? 0,
                                                          YearlyTarget = t1.YearlyTarget ?? 0,
                                                          VendorId = t1.VendorId,
                                                          VendorName = t2.Name,
                                                          CreditRatioFrom = t1.CreditRatioFrom ?? 0,
                                                          CreditRatioTo = t1.CreditRatioTo ?? 0,
                                                          CreditLimit = t1.CreditLimit ?? 0,
                                                          Days = t1.Days ?? 0,
                                                          Transport = t1.Transport ?? 0,
                                                          ClosingDate = t1.ClosingDate,
                                                          ExtraCondition1 = t1.ExtraCondition1 ?? 0,
                                                          ExtraBenifite = t1.ExtraBenifite ?? 0,
                                                          DepositRate = t1.DepositRate ?? 0
                                                      }
                                            ).OrderByDescending(o => o.VendorDeedId).AsEnumerable());
            dataList.CompanyId = companyId;
            return dataList;
        }

        public async Task<int> SaveVendorDeed(VendorDeedVm model)
        {

            try
            {
                var obj = await _context.VendorDeeds.SingleOrDefaultAsync(q => q.VendorDeedId == model.VendorDeedId);

                if (obj == null)
                {
                    obj = new VendorDeed()
                    {
                        CompanyId = model.CompanyId,
                        VendorId = model.VendorId,
                        CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    };

                    _context.VendorDeeds.Add(obj);

                }
                else
                {
                    obj.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                    obj.ModifiedDate = DateTime.Now;
                }
                obj.MonthlyTarget = model.MonthlyTarget;
                obj.YearlyTarget = model.YearlyTarget;
                obj.CreditRatioFrom = model.CreditRatioFrom;
                obj.CreditRatioTo = model.CreditRatioTo;
                obj.CreditLimit = model.CreditLimit;
                obj.Days = model.Days;
                obj.Transport = model.Transport;
                obj.ClosingDate = model.ClosingDate;
                obj.ExtraCondition1 = model.ExtraCondition1;
                obj.ExtraBenifite = model.ExtraBenifite;
                obj.DepositRate = model.DepositRate;

                await _context.SaveChangesAsync();

                return obj.VendorDeedId;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<VendorDeedVm> GetVendorDeedById(int vendorDeedId)
        {
            var data = new VendorDeedVm();
            data = await Task.Run(() => (from t1 in _context.VendorDeeds
                                         join t2 in _context.Vendors on t1.VendorId equals t2.VendorId
                                         where t1.VendorDeedId == vendorDeedId
                                         select new VendorDeedVm
                                         {
                                             CompanyId = t1.CompanyId,
                                             VendorDeedId = t1.VendorDeedId,
                                             MonthlyTarget = t1.MonthlyTarget ?? 0,
                                             YearlyTarget = t1.YearlyTarget ?? 0,
                                             VendorId = t1.VendorId,
                                             CreditRatioFrom = t1.CreditRatioFrom ?? 0,
                                             CreditRatioTo = t1.CreditRatioTo ?? 0,
                                             CreditLimit = t1.CreditLimit ?? 0,
                                             Days = t1.Days ?? 0,
                                             Transport = t1.Transport ?? 0,
                                             ClosingDate = t1.ClosingDate,
                                             ExtraCondition1 = t1.ExtraCondition1 ?? 0,
                                             ExtraBenifite = t1.ExtraBenifite ?? 0,
                                             DepositRate = t1.DepositRate ?? 0,
                                             VendorName = t2.Name
                                         }).SingleOrDefaultAsync());


            data.ClosingDateText = data.ClosingDate == null ? "" : data.ClosingDate.Value.ToShortDateString();
            return data;
        }

        public async Task<bool> RemoveVendorDeed(int vendorDeedId)
        {

            var obj = await _context.VendorDeeds.SingleOrDefaultAsync(q => q.VendorDeedId == vendorDeedId);

            if (obj == null)
            {
                return false;
            }

            obj.IsActive = false;
            obj.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            obj.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
