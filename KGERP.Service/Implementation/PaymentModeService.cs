using KGERP.Data.Models;
using KGERP.Service.Interface;
using KGERP.Utility;
using System.Collections.Generic;
using System.Linq;

namespace KGERP.Service.Implementation
{
    public class BankService : IBankService
    {
        ERPEntities bankRepository = new ERPEntities();
        public List<Bank> GetBanks()
        {
            return bankRepository.Banks.ToList();
        }

        public List<SelectModel> GetBankSelectModels()
        {
            return bankRepository.Banks.Where(c=>c.IsActive).ToList().Select(x => new SelectModel()
            {
                Text = x.Name.ToString(),
                Value = x.BankId.ToString()
            }).ToList();
        }
    }
}
