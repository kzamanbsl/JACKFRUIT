using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Data.Models;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGERP.Service.Implementation
{
    public class YearlyHolidayService : IYearlyHoliday
    {
        ERPEntities yearlyHolidayRepository = new ERPEntities();


        public List<YearlyHolidayModel> GetYearlyHolidayEvent(string searchText)
        {
            dynamic result = yearlyHolidayRepository.Database.SqlQuery<YearlyHolidayModel>("exec sp_YearlyHolidayEvent").ToList();
            return result;
        }
        public List<YearlyHolidayModel> GetYearlyHolidays(string searchText)
        {
            return ObjectConverter<YearlyHoliday, YearlyHolidayModel>.ConvertList(yearlyHolidayRepository.YearlyHolidays.Where(x => x.HolidayDate.Year == DateTime.Now.Year || x.HolidayCategory.Contains(searchText) ||  x.Purpose.Contains(searchText) ).OrderByDescending(x => x.HolidayDate).ToList()).ToList();

        }
        public YearlyHolidayModel GetYearlyHoliday(int id)
        {
            if (id == 0)
            {
                return new YearlyHolidayModel();
            }
            return ObjectConverter<YearlyHoliday, YearlyHolidayModel>.Convert(yearlyHolidayRepository.YearlyHolidays.FirstOrDefault(x => x.YearlyHolidayId == id));
        }

        public bool SaveYearlyHoliday(int id, YearlyHolidayModel model)
        {
            if (model == null)
            {
                throw new Exception("Holiday data missing");
            }
            YearlyHoliday yearlyHoliday = ObjectConverter<YearlyHolidayModel, YearlyHoliday>.Convert(model);


            if (id > 0)
            {
                yearlyHoliday = yearlyHolidayRepository.YearlyHolidays.FirstOrDefault(x => x.YearlyHolidayId == id);
                if (yearlyHoliday == null)
                {
                    throw new Exception("Holiday Data not found!");
                }

                //yearlyHoliday.ModifiedDate = DateTime.Now;
                //yearlyHoliday.ModifedBy = "";
                //employeeRepository.Entry(employee).State = System.Data.Entity.EntityState.Modified;
            }
            else
            {
                yearlyHoliday.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
                yearlyHoliday.CreatedDate = DateTime.Now;
            }

            yearlyHoliday.HolidayDate = model.HolidayDate;
            yearlyHoliday.HolidayCategory = model.HolidayCategory;
            yearlyHoliday.Purpose = model.Purpose;

            yearlyHolidayRepository.Entry(yearlyHoliday).State = yearlyHoliday.YearlyHolidayId == 0 ? EntityState.Added : EntityState.Modified;
            return yearlyHolidayRepository.SaveChanges() > 0;
        }



        //bool IYearlyHolidayService.DeleteYearlyHoliday(int id)
        //{
        //    YearlyHoliday yearlyHoliday = yearlyHolidayRepository.YearlyHolidays.FirstOrDefault(x => x.YearlyHolidayId == id);
        //    yearlyHolidayRepository.YearlyHolidays.Remove(yearlyHoliday);
        //    return yearlyHolidayRepository.SaveChanges() > 0;
        //}


    }
}
