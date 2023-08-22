using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using BSA.Data.Models;
using BSA.Service.Implementation;
using BSA.Service.Interface;
using BSA.Service.ServiceModel;
using BSA.Utility;
using BSA.Utility.Util;
using PagedList;

namespace BSA.Controllers
{
    public class CustomersController : Controller
    {
        private BSAEntities db = new BSAEntities();
        ICustomerService customerService = new CustomerService();
        IDropDownItemService dropDownItemService = new DropDownItemService();

        // GET: Customers
        [SessionExpire]
        public ActionResult Index(int? Page_No, string searchText)
        {
            searchText = searchText ?? "";
            string memberid = System.Web.HttpContext.Current.User.Identity.Name;
            List<CustomerModel> customers = customerService.GetCustomers(searchText).Where(x => x.CreatedBy == memberid).ToList();
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(customers.ToPagedList(No_Of_Page, Size_Of_Page));
        }
        [SessionExpire]
        public ActionResult CreateOrEdit(int id)
        {
            CustomerModel model = customerService.GetCustomer(id);
            model.CustomerTypes = dropDownItemService.GetDropDownItemSelectModels(2);
            model.CustStatus = dropDownItemService.GetDropDownItemSelectModels(3);
            model.PaymentsStatus = dropDownItemService.GetDropDownItemSelectModels(3);
            //model.Districts = DistrictService.GetDistrictSelectModels();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOrEdit(CustomerModel model, HttpPostedFileBase file)
        {
            string picture = string.Empty;
            if (file != null && file.ContentLength > 0)
            {
                var supportedTypes = new[] { "jpg", "jpeg", "png", "bmp" };
                var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);
                if (!supportedTypes.Contains(fileExt))
                {
                    string ErrorMessage = Constants.FileType;
                    throw new Exception(ErrorMessage);
                }
                else
                {
                    //int count = 1;
                    //string fileExtension = Path.GetExtension(file.FileName);
                    ////picture = model.CustomerId + fileExtension;
                    ////picture = file.FileName;
                    ////string fullPath = Path.Combine(Server.MapPath("~/Images/Picture"), picture);
                    ////string fileNameOnly = Path.GetFileName(fullPath);
                    //var pathFile = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Images/Picture"), file.FileName);

                    //string extension = Path.GetExtension(pathFile);
                    //string path = Path.GetDirectoryName(pathFile);
                    //string newFullPath = pathFile;

                    //while (System.IO.File.Exists(newFullPath))
                    //{
                    //    picture = string.Format("{0}({1})", file.FileName, count++);
                    //    //newFullPath = Path.Combine(path, picture + extension);
                    //    newFullPath = Path.Combine(path, picture);
                    //    picture = picture + extension;
                    //}
                    //file.SaveAs(Path.Combine(path, picture));
                    //model.LogoUrl = pathFile;  

                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/Images/Picture"), fileName);
                    file.SaveAs(path);
                    string fileName1 = Path.GetPathRoot(file.FileName);
                    string extention = Path.GetExtension(file.FileName).ToLower();
                    model.LogoUrl = path;
                }
            }


            if (model.OID <= 0)
            {
                bool exist = db.Customers.Where(x => x.ContactPersonName == model.ContactPersonName).Any();
                if (exist)
                {
                    TempData["errMessage"] = "Exists";
                    return RedirectToAction("CreateOrEdit");
                }
                else
                {
                    customerService.SaveCustomer(0, model);
                }
            }
            else
            {
                Customer kttlCustomer = db.Customers.FirstOrDefault(x => x.OID == model.OID);
                if (kttlCustomer == null)
                {
                    TempData["errMessage1"] = "Client not found!";
                    return RedirectToAction("CreateOrEdit");
                }
                customerService.SaveCustomer(model.OID, model);
                TempData["DataUpdate"] = "Data Save Successfully!";
            }

            return RedirectToAction("Index");
        }

        // GET: Customers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer kttlCustomer = db.Customers.Find(id);
            if (kttlCustomer == null)
            {
                return HttpNotFound();
            }
            return View(kttlCustomer);
        }

        // GET: Customers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer kttlCustomer = db.Customers.Find(id);
            if (kttlCustomer == null)
            {
                return HttpNotFound();
            }
            return View(kttlCustomer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Customer kttlCustomer = db.Customers.Find(id);
            db.Customers.Remove(kttlCustomer);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public byte[] GetBytesFromFile(string fullFilePath)
        {
            // this method is limited to 2^32 byte files (4.2 GB)
            FileStream fs = null;
            try
            {
                fs = System.IO.File.OpenRead(fullFilePath);
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
                return bytes;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
        }

        public FileResult CustomerStatusReport(string NID)
        {
            if (NID == "22334499007")//Block
            {
                string filepath = Server.MapPath("/UserManual/CustomerStatusReport_Block.pdf");
                byte[] pdfByte = GetBytesFromFile(filepath);
                return File(pdfByte, "application/pdf", "CustomerStatusReport_Block.pdf");
            }
            else if (NID == "22334499005")//Irregular
            {
                string filepath = Server.MapPath("/UserManual/CustomerStatusReport_Irregular.pdf");
                byte[] pdfByte = GetBytesFromFile(filepath);
                return File(pdfByte, "application/pdf", "CustomerStatusReport_Irregular.pdf");
            }
            else if (NID == "22334499004")//Regular
            {
                string filepath = Server.MapPath("/UserManual/CustomerStatusReport_Regular_22334499004.pdf");
                byte[] pdfByte = GetBytesFromFile(filepath);
                return File(pdfByte, "application/pdf", "CustomerStatusReport_Regular_22334499004.pdf");
            }
            else if (NID == "22334499003")//Regular
            {
                string filepath = Server.MapPath("/UserManual/CustomerStatusReport_Regular_22334499003.pdf");
                byte[] pdfByte = GetBytesFromFile(filepath);
                return File(pdfByte, "application/pdf", "CustomerStatusReport_Regular_22334499003.pdf");
            }
            else if (NID == "22334499004")//Regular
            {
                string filepath = Server.MapPath("/UserManual/CustomerStatusReport_Regular_22334499004.pdf");
                byte[] pdfByte = GetBytesFromFile(filepath);
                return File(pdfByte, "application/pdf", "CustomerStatusReport_Regular_22334499004.pdf");
            }
            else if (NID == "22334499006")//Regular
            {
                string filepath = Server.MapPath("/UserManual/CustomerStatusReport_Regular_22334499006.pdf");
                byte[] pdfByte = GetBytesFromFile(filepath);
                return File(pdfByte, "application/pdf", "CustomerStatusReport_Regular_22334499006.pdf");
            }
            else
            {
                string filepath = Server.MapPath("/UserManual/CustomerStatusReport_Regular_22334499001.pdf");
                byte[] pdfByte = GetBytesFromFile(filepath);
                return File(pdfByte, "application/pdf", "CustomerStatusReport_Regular_22334499001.pdf");
            }

        }

        public ActionResult CustomerStatusReports()
        {
            //string filepath = Server.MapPath("/UserManual/CustomerStatusReport.pdf");
            //byte[] pdfByte = GetBytesFromFile(filepath);
            //return File(pdfByte, "application/pdf", "CustomerStatusReport.pdf");
            return View(CustomerStatusReportsActionResult());
        }

        public FileResult CustomerStatusReportsActionResult()
        {
            string filepath = Server.MapPath("/UserManual/CustomerStatusReport.pdf");
            byte[] pdfByte = GetBytesFromFile(filepath);
            return File(pdfByte, "application/pdf", "CustomerStatusReport.pdf");
        }

        public FileResult AllCustomerList()
        {
            string filepath = Server.MapPath("/UserManual/AllCustomerList.pdf");
            byte[] pdfByte = GetBytesFromFile(filepath);
            return File(pdfByte, "application/pdf", "AllCustomerList.pdf");
        }
        [SessionExpire]
        public ActionResult CustomerList(int? Page_No, string searchText)
        {
            //return View(db.Customers.ToList());
            searchText = searchText ?? "";
            string memberid = System.Web.HttpContext.Current.User.Identity.Name;
            List<CustomerModel> Customers = customerService.GetAllCustomers(searchText).ToList();
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(Customers.ToPagedList(No_Of_Page, Size_Of_Page));
        }


        #region // Batch Upload
        [SessionExpire]
        public ActionResult BulkDataUpload(int? MemberNo)
        {
            int comId;
            if (MemberNo > 0)
            {
                Session["MemberNo"] = MemberNo;
                comId = (int)Session["MemberNo"];
            }


            return View();
        }

        [SessionExpire]
        [HttpPost]
        public ActionResult BulkDataUpload(CustomerModel file)
        {
            try
            {
                string message = UploadExcelFile(file);
                ViewBag.ExcelIssues = message;
                return View();
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        private string UploadExcelFile(CustomerModel file)
        {
            int MemberNo2 = (int)Session["MemberNo"] > 0 ? (int)Session["MemberNo"] : 0;
            if (MemberNo2 > 0)
            {

            }
            string ValidDisplayMessage = "";

            if (file.ExcelFile != null && file.ExcelFile.ContentLength > 0)
            {
                OleDbConnection conn = new OleDbConnection();
                OleDbCommand cmd = new OleDbCommand();
                OleDbDataAdapter da = new OleDbDataAdapter();
                DataSet ds = new DataSet();
                string connString = "";
                string strFileName = DateTime.Now.ToString("ddMMyyyy_HHmmss");
                string strFileType = Path.GetExtension(file.ExcelFile.FileName).ToString().ToLower();
                var fileName = Path.GetFileName(file.ExcelFile.FileName);
                var path = Path.Combine(Server.MapPath("~/FileUpload"), fileName);

                if (strFileType == ".xls" || strFileType == ".xlsx")
                {
                    try
                    {
                        file.ExcelFile.SaveAs(path);
                    }
                    catch (Exception ex)
                    {
                        //logger.Error(ex);
                    }
                }
                else
                {
                    return "";
                }
                if (strFileType.Trim() == ".xls")
                {
                    connString = string.Format(ConfigurationManager.ConnectionStrings["Excel03ConString"].ToString(), path);//"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + strNewPath + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                }
                else if (strFileType.Trim() == ".xlsx")
                {
                    connString = string.Format(ConfigurationManager.ConnectionStrings["Excel07ConString"].ToString(), path);//"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + strNewPath + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                }
                try
                {
                    connString = string.Format(connString, path);
                    OleDbConnection connExcel = new OleDbConnection(connString);
                    OleDbCommand cmdExcel = new OleDbCommand();
                    OleDbDataAdapter oda = new OleDbDataAdapter();
                    DataTable dt = new DataTable();
                    cmdExcel.Connection = connExcel;
                    connExcel.Open();
                    DataTable dtExcelSchema;
                    dtExcelSchema = connExcel.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    string SheetName = dtExcelSchema.Rows[0]["TABLE_NAME"].ToString();
                    cmdExcel.CommandText = "SELECT * From [" + SheetName + "]";
                    oda.SelectCommand = cmdExcel;
                    oda.Fill(ds);
                    string str = ValidExcel(ds);
                    if (str.Length == 0)
                    {
                        try
                        {
                            List<CustomerModel> lstSuccessCustomerModel = new List<CustomerModel>();
                            List<CustomerModel> lstErrorCustomerModel = new List<CustomerModel>();
                            dt = ds.Tables[0];
                            lstSuccessCustomerModel.Clear();
                            lstErrorCustomerModel.Clear();
                            DataTable dtError = new DataTable();
                            int t = 0;
                            int s = 0;
                            int u = 0;

                            string dupData = "";
                            foreach (DataRow dr in dt.Rows)
                            {
                                ++t;
                                if (!string.IsNullOrEmpty(dr["Customer Name"].ToString()) && !string.IsNullOrEmpty(dr["Mobile"].ToString()))
                                {
                                    string mobile = "0" + dr["Mobile"].ToString();
                                    Customer objCustomer = null;
                                    objCustomer = db.Customers.FirstOrDefault(x => x.MobileNo == mobile && x.OID == MemberNo2);
                                    if (objCustomer != null)
                                    {
                                        ++u;
                                        if (!string.IsNullOrEmpty(dupData))
                                        {
                                            dupData += "\n Name: " + dr["Customer Name"].ToString() + ", Mobile no. 0" + dr["Mobile"].ToString();
                                        }
                                        else
                                        {
                                            dupData = "\n Name: " + dr["Customer Name"].ToString() + ", Mobile no. 0" + dr["Mobile"].ToString();
                                        }
                                    }
                                    else
                                    {
                                        objCustomer = new Customer();
                                        objCustomer.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
                                        objCustomer.CreatedDate = DateTime.Now;
                                        //objCustomer.MemberName = !string.IsNullOrEmpty(dr["MemberName"].ToString()) ? Convert.ToDateTime(dr["MemberName"].ToString()) : (DateTime?)null;
                                        objCustomer.CustomerName = dr["Customer Name"].ToString();
                                        objCustomer.TradeLicense = dr["TradeLicense"].ToString();
                                        objCustomer.RegistrationNo = dr["RegistrationNo"].ToString();
                                        objCustomer.AddressOne = dr["AddressOne"].ToString();
                                        objCustomer.AddressTwo = dr["AddressTwo"].ToString();
                                        objCustomer.Designation = dr["Designation"].ToString();
                                        objCustomer.MemberName = dr["MemberName"].ToString();
                                        objCustomer.MobileNo = "0" + dr["Mobile"].ToString();
                                        objCustomer.Telephone = !string.IsNullOrEmpty(dr["Telephone"].ToString()) ? "0" + dr["Telephone"].ToString() : "";
                                        objCustomer.Email = dr["Email"].ToString();
                                        objCustomer.NID = dr["NID"].ToString();

                                        if (!string.IsNullOrEmpty(System.Web.HttpContext.Current.User.Identity.Name))
                                        {
                                            string memberId = System.Web.HttpContext.Current.User.Identity.Name; 
                                            BSAMember objBSAMember = null;
                                            objBSAMember = db.BSAMembers.Where(x => x.MemberId.ToUpper() == memberId.ToUpper().Trim()).FirstOrDefault();
                                            if (objBSAMember != null)
                                            {
                                                objCustomer.MemberNo = objBSAMember.Id;
                                                objCustomer.MemberName = objBSAMember.MemberNameBn;
                                                //objCustomer.MemberName = objBSAMember.MemberNameEn;
                                            }
                                            else
                                            {
                                                ValidDisplayMessage += " Member Name is not valid";
                                            }
                                        }

                                        if (!string.IsNullOrEmpty(dr["Division"].ToString().Trim()))
                                        {
                                            string pName = dr["Division"].ToString().Trim();
                                            Division objDivision = null;
                                            objDivision = db.Divisions.Where(x => x.Name.ToUpper() == pName.ToUpper().Trim()).FirstOrDefault();
                                            if (objDivision != null)
                                            {
                                                objCustomer.DivisionId = objDivision.DivisionId;
                                            }
                                            else
                                            {
                                                ValidDisplayMessage += dr["Division"].ToString().Trim() + " Division is not valid";
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(dr["District"].ToString().Trim()))
                                        {
                                            string pName = dr["District"].ToString().Trim();
                                            District objDistrict = null;
                                            objDistrict = db.Districts.Where(x => x.Name.ToUpper() == pName.ToUpper().Trim()).FirstOrDefault();
                                            if (objDistrict != null)
                                            {
                                                objCustomer.DistrictId = objDistrict.DistrictId;
                                            }
                                            else
                                            {
                                                ValidDisplayMessage += dr["District"].ToString().Trim() + " District is not valid";
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(dr["Upazila"].ToString().Trim()))
                                        {
                                            string pName = dr["Upazila"].ToString().Trim();
                                            Upazila objUpazila = null;
                                            objUpazila = db.Upazilas.Where(x => x.Name.ToUpper() == pName.ToUpper().Trim()).FirstOrDefault();
                                            if (objUpazila != null)
                                            {
                                                objCustomer.UpazilaId = objUpazila.UpazilaId;
                                            }
                                            else
                                            {
                                                ValidDisplayMessage += dr["Upazila"].ToString().Trim() + " Upazila is not valid";
                                            }
                                        }

                                        objCustomer.Remarks = dr["Remarks"].ToString();
                                        objCustomer.ConcatPersonEmail = dr["ConcatPersonEmail"].ToString();
                                        int MemberNo = (int)Session["MemberNo"] > 0 ? (int)Session["MemberNo"] : 0;
                                        if (MemberNo > 0)
                                        {
                                            objCustomer.MemberNo = MemberNo;
                                        }
                                        try
                                        {
                                            db.Customers.Add(objCustomer);
                                            db.SaveChanges();
                                        }
                                        catch (Exception ex)
                                        {
                                        }
                                        if (objCustomer.OID > 0)
                                        {
                                            ++s;
                                        }
                                        ModelState.Clear();
                                    }
                                }
                                else
                                {
                                }
                            }

                            if (t > 0)
                            {
                                string result = "";
                                if (s > 0)
                                {
                                    result = s + " Saved";
                                    ValidDisplayMessage = "Total number of Valid data: " + result + " Out of " + t + "\n";
                                }
                                if (u > 0)
                                {
                                    ValidDisplayMessage += "Total number of Dulicate data: " + u + " Out of " + t + "\n";
                                    ValidDisplayMessage += "\r" + dupData;
                                }
                            }
                        }

                        catch (Exception ex)
                        {
                            //logger.Error(ex);
                        }
                    }
                    else
                    {
                        ValidDisplayMessage = str;
                    }
                }
                catch (Exception ex)
                {
                    //logger.Error(ex);
                }
                try
                {
                    da.Dispose();
                    conn.Close();
                    conn.Dispose();
                    System.IO.File.Delete(path);
                }
                catch (Exception ex)
                {
                    //logger.Error(ex);
                }
            }
            else
            {
            }
            return ValidDisplayMessage;
        }

        private string ValidExcel(DataSet ds)
        {
            string[] header = {
                "TradeLicense","CustomerType","RegistrationNo",
                "Customer Name","Mobile","Email","Designation",
                "Telephone","NID","Division","Upazila","District",
                "AddressOne","AddressTwo","ContactPersonName","ConcatPersonEmail","Remarks"};

            StringBuilder errorMsg = new StringBuilder();
            DataTable dt = new DataTable();
            dt = ds.Tables[0];
            var query = dt.AsEnumerable().Where(r => r.Field<string>("Customer Name") == null && r.Field<string>("Mobile") == null);
            foreach (var row in query.ToList())
            {
                row.Delete();
                dt.AcceptChanges();
            }

            int rowcount = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (!string.IsNullOrEmpty(row["Customer Name"].ToString())) //checking blank rows out of count otherwise sytem does not go for next step
                {
                    string completed = (string)row["Customer Name"];
                    if (!string.IsNullOrEmpty(completed))
                    {
                        rowcount++;
                    }
                }
            }
            if (rowcount <= 3000)
            {
                string flag = string.Empty;
                StringBuilder errorMsgClo = new StringBuilder();

                if (dt.Columns.Count == 17)
                {
                    foreach (DataColumn c in dt.Columns)
                    {
                        string str = c.ColumnName;
                        if (!header.Contains(str))
                        {
                            errorMsgClo.Append(str + ", ");
                        }
                    }
                    if (errorMsgClo.Length > 0)
                    {
                        errorMsg.Append("Excel column are invalid, Column : " + errorMsgClo.ToString());
                    }
                }
                else
                {
                    errorMsg.Append("Excel Template formate is not correct.");
                }
            }
            else
            {
                errorMsg.Append("Please upload 300 Client at a time");
            }
            return errorMsg.ToString();
        }

        private CustomerModel CreateObject(DataRow dr)
        {
            string errorMsg = string.Empty;
            CustomerModel customerModel = new CustomerModel();
            CustomerModel objCrmModel = customerModel;
            try
            {
                List<string> lstProjectName = new List<string>();


                if (!string.IsNullOrEmpty(dr["Division"].ToString()))
                {
                    using (BSAEntities unit = new BSAEntities())
                    {
                        Division objDivision = unit.Divisions.Where(x => x.Name == dr["Division"].ToString()).FirstOrDefault();
                        if (objDivision == null)
                        {
                            errorMsg = "This Division May not exist in Project list.";
                        }
                    }
                }
                else { errorMsg = "This Division required"; }

                if (!string.IsNullOrEmpty(dr["District"].ToString()))
                {
                    using (BSAEntities unit = new BSAEntities())
                    {
                        District objDistrict = unit.Districts.Where(x => x.Name == dr["District"].ToString()).FirstOrDefault();
                        if (objDistrict == null)
                        {
                            errorMsg = "This District May not exist in Project list.";
                        }
                    }
                }
                else { errorMsg = "This District required"; }
                if (!string.IsNullOrEmpty(dr["Upazila"].ToString()))
                {
                    using (BSAEntities unit = new BSAEntities())
                    {
                        Upazila objUpazila = unit.Upazilas.Where(x => x.Name == dr["Upazila"].ToString()).FirstOrDefault();
                        if (objUpazila == null)
                        {
                            errorMsg = "This Upzilla May not exist in Project list.";
                        }
                    }
                }
                else { errorMsg = "This District required"; }

                if (!string.IsNullOrEmpty(dr["Mobile"].ToString()))
                {
                    customerModel.MobileNo = dr["Mobile"].ToString();
                    using (BSAEntities unit = new BSAEntities())
                    {
                        Customer objCust = unit.Customers.Where(x => x.MobileNo == dr["Mobile"].ToString()).FirstOrDefault();
                        if (objCust == null)
                        {
                            errorMsg = "This Mobile is already Exist";
                        }
                    }
                }
                else { errorMsg = "Mobile 1 is not empty."; }


                if (!string.IsNullOrEmpty(dr["Customer Name"].ToString()))
                {
                    customerModel.CustomerName = dr["Customer Name"].ToString();
                }
                if (!string.IsNullOrEmpty(dr["Designation"].ToString()))
                {
                    customerModel.Designation = dr["Designation"].ToString();
                }
                //if (!string.IsNullOrEmpty(dr["Organization"].ToString()))
                //{
                //    customerModel.Organization = dr["Organization"].ToString();
                //}
                if (!string.IsNullOrEmpty(dr["AddressOne"].ToString()))
                {
                    customerModel.AddressOne = dr["AddressOne"].ToString();
                }

                if (!string.IsNullOrEmpty(dr["AddressTwo"].ToString()))
                {
                    customerModel.AddressTwo = dr["AddressTwo"].ToString();
                }

            }
            catch (Exception ex)
            {
                // logger.Error(ex);
            }
            return customerModel;
        }

        #endregion
    }
}