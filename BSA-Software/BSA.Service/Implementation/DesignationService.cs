using BSA.Service.Interface;
using BSA.Data.Models;
using BSA.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSA.Service.Implementation
{
    public class DesignationService : IDesignationService
    {
        BSAEntities designationRepository = new BSAEntities();
        public List<Designation> GetDesignations()
        {
            return designationRepository.Designations.ToList();
        }

        public List<SelectModel> GetDesignationSelectModels()
        {
            return designationRepository.Designations.OrderBy(x=>x.Name).ToList().Select(x => new SelectModel()
            {
                Text = x.Name.ToString(),
                Value = x.DesignationId.ToString()
            }).ToList();
        }
    }
}
