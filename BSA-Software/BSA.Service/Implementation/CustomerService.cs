using BSA.Service.Interface;
using BSA.Service.ServiceModel;
using BSA.Data.Models;
using BSA.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Validation;

namespace BSA.Service.Implementation
{
    public class CustomerService : ICustomerService
    {
        private bool disposed = false;

        BSAEntities context = new BSAEntities();
        public List<CustomerModel> GetAllCustomers(string searchText)
        {
            IQueryable<Customer> Customers = context.Customers.Where(x => x.CustomerType.Contains(searchText) || x.CustomerName.Contains(searchText) || x.PaymentStatus.Contains(searchText) || x.NID.Contains(searchText)).OrderBy(x => x.OID);
            return ObjectConverter<Customer, CustomerModel>.ConvertList(Customers.ToList()).ToList();
        }

        public List<CustomerModel> GetCustomers(string searchText)
        {
            IQueryable<Customer> Customers = context.Customers.Where(x => x.CustomerType.Contains(searchText) || x.CustomerName.Contains(searchText) || x.PaymentStatus.Contains(searchText) || x.MobileNo.Contains(searchText) || x.ContactPersonName.Contains(searchText) || x.NID.Contains(searchText)).OrderBy(x => x.OID);
            return ObjectConverter<Customer, CustomerModel>.ConvertList(Customers.ToList()).ToList();
        }
        public CustomerModel GetCustomer(int id)
        {
            if (id == 0)
            {
                return new CustomerModel() { OID = id };
            }
            Customer customer = context.Customers.Find(id);
            return ObjectConverter<Customer, CustomerModel>.Convert(customer);
        }

        public bool SaveCustomer(int id, CustomerModel model)
        {
            bool result = false;
            try
            {
                Customer customer = ObjectConverter<CustomerModel, Customer>.Convert(model);
                if (id > 0)
                {
                    customer.ModifiedDate = DateTime.Now;
                    customer.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                }
                else
                {
                    customer.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
                    customer.CreatedDate = DateTime.Now;
                } 
                customer.CustomerId = model.CustomerId;
                customer.CustomerType = model.CustomerType;
                customer.CustomerName = model.CustomerName;
                customer.AddressOne = model.AddressOne;
                //customer.FiscalYear = model.FiscalYear;
                customer.Telephone = model.Telephone;
                customer.MobileNo = model.MobileNo;
                customer.TIN = model.TIN;
                customer.ContactPersonName = model.ContactPersonName;
                customer.ConcatPersonEmail = model.ConcatPersonEmail;
                customer.AddressTwo = model.AddressTwo;
                customer.DateOfBirth = model.DateOfBirth;
                customer.Nationality = model.Nationality;
                customer.Email = model.Email;
                customer.Designation = model.Designation;
                //customer.Division = model.Division;
                //customer.District = model.District;
                //customer.LogoUrl = model.LogoUrl;
                //customer.ImageUrl = model.ImageUrl;
                customer.Remarks = model.Remarks;
                context.Entry(customer).State = customer.OID == 0 ? EntityState.Added : EntityState.Modified;
                result= context.SaveChanges() > 0;
            }
            catch (DbEntityValidationException ex)
            {
                foreach (DbEntityValidationResult errors in ex.EntityValidationErrors)
                {
                    foreach (DbValidationError validationError in errors.ValidationErrors)
                    {
                        // get the error message 
                        string errorMessage = validationError.ErrorMessage;
                        result= false;
                    }
                }
            }
            return result;
        }

        public bool DeleteCustomer(int id)
        {
            throw new NotImplementedException();
        } 

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            disposed = true;
        }
    }
}
