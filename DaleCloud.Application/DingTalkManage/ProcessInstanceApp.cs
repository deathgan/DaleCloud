using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DaleCloud.Code;
using DaleCloud.Entity.DingTalk;
using DaleCloud.Repository.DingTalk;
using DaleCloud.Entity.DingTalkManage;
using DaleCloud.Repository.DingTalkManage;

namespace DaleCloud.Application.DingTalk
{
    public class ProcessInstanceApp
    {
        private ProcessInstanceRepository service = new ProcessInstanceRepository();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public List<ProcessInstanceEntity> GetList(string keyword = "")
        {
            var expression = ExtLinq.True<ProcessInstanceEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.Title.Contains(keyword));
                expression = expression.Or(t => t.Approvers.Contains(keyword));
            }
            return service.IQueryable(expression).OrderBy(t => t.CreateTime).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ProcessInstanceEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }

        public ProcessInstanceEntity GetModelByCode(string keyValue)
        {
            var expression = ExtLinq.True<ProcessInstanceEntity>();
            if (!string.IsNullOrEmpty(keyValue))
            {
                expression = expression.And(t => t.ProcessCode.Contains(keyValue));
            }
            return service.IQueryable(expression).OrderBy(t => t.CreateTime).Single();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValue"></param>
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.uuId == keyValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uEntity"></param>
        /// <param name="keyValue"></param>
        public void SubmitForm(ProcessInstanceEntity uEntity, string keyValue)
        {
            if (!string.IsNullOrEmpty(keyValue))
            {
                service.Update(uEntity);
            }
            else
            {
                service.Insert(uEntity);
            }
        }
    }
}
