using Bsa.lib.CustomModel;
using Bsa.lib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bsa.lib.Implementation
{
    public class WebConfigurationService
    {
        private readonly BSAWebEntities _db;

        public WebConfigurationService(BSAWebEntities db)
        {
            _db = db;
        }
        #region
        public async Task<MdSecretaryMessageModel> GetMessage()
        {
            MdSecretaryMessageModel vm = new MdSecretaryMessageModel();
            vm.DataList = await Task.Run(() => MessageDataLoad());
            return vm;
        }

        public IEnumerable<MdSecretaryMessageModel> MessageDataLoad()
        {
            var v = (from t1 in _db.BSAMessages 
                     where t1.IsActive == true
                     select new MdSecretaryMessageModel
                     {
                         ID = t1.Id,
                         Message = t1.Message, 
                         Title = t1.Title, 
                         IsActive = t1.IsActive
                     }).OrderByDescending(x => x.ID).AsEnumerable();
            return v;
        }

        public async Task<int> MessageAdd(MdSecretaryMessageModel messageModel)
        {
            var result = -1;
            BSAMessage bSAMessage = new BSAMessage
            { 
                Title = messageModel.Title,
                Message = messageModel.Message, 
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _db.BSAMessages.Add(bSAMessage);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = bSAMessage.Id;
            }
            return result;
        }
        public async Task<int> MessageEdit(MdSecretaryMessageModel message)
        {
            var result = -1;
            //to select Accountining_Chart_Two data.....
            BSAMessage bSAMessage = _db.BSAMessages.Find(message.ID);
            bSAMessage.Title = message.Title; 
            bSAMessage.Message = message.Message;  
            bSAMessage.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            bSAMessage.ModifiedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = bSAMessage.Id;
            }
            return result;
        }
        public async Task<int> MessageDelete(int id)
        {
            int result = -1;
            if (id != 0)
            {
                BSAMessage bSAMessage = _db.BSAMessages.Find(id);
                bSAMessage.IsActive = false;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = bSAMessage.Id;
                }
            }
            return result;
        }
        #endregion
    }
}
