using KGERP.Data.Models;
using KGERP.Service.Interface;
using KGERP.Utility;
using System.Collections.Generic;
using System.Linq;

namespace KGERP.Service.Implementation
{
    public class BankBranchService : IBankBranchService
    {
        ERPEntities bankBranchRepository = new ERPEntities();

        public List<SelectModel> GetBankBranchByBank(int bankId)
        {
            return bankBranchRepository.BankBranches.ToList().Where(x => x.BankId == bankId).Select(x => new SelectModel()
            {
                Text = x.Name,
                Value = x.BankBranchId
            }).ToList();
        }

        public List<BankBranch> GetBankBranches()
        {
            return bankBranchRepository.BankBranches.ToList();
        }

        public List<SelectModel> GetBankBranchSelectModels()
        {
            return bankBranchRepository.BankBranches.Select(x => new SelectModel()
            {
                Text = x.Name.ToString(),
                Value = x.BankId.ToString()
            }).ToList();
        }

    }
}
