using DaleCloud.Code;
using DaleCloud.Entity.MissionManage;
using DaleCloud.Repository.MissionManage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaleCloud.Application.MissionManage
{


    public class MmailingRecordApp
    {
        private MmailingRecordRepository service = new MmailingRecordRepository();


        /// <summary>
        /// 发送记录
        /// </summary>
        /// <param name="pagination">分页</param>
        /// <param name="startdate">起始时间</param>
        /// <param name="enddate">结束时间</param>
        /// <param name="keyword">关键字</param>
        /// <returns></returns>
        public List<MmailingRecordEntity> GetList(Pagination pagination, string startdate, string enddate, string keyword = "", string mailingType = "")
        {
            var expression = ExtLinq.True<MmailingRecordEntity>();
            if (!string.IsNullOrEmpty(startdate) && !string.IsNullOrEmpty(enddate))
            {
                DateTime sdate = DateTime.Parse(startdate);
                DateTime edate = DateTime.Parse(enddate);
                expression = expression.And(t => t.F_ExpectedToSendTime > sdate && t.F_ExpectedToSendTime < edate);
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.F_EmailContent.Contains(keyword));
            }
            if (!string.IsNullOrEmpty(mailingType))
            {
                int mType = Convert.ToInt32(mailingType);

                expression = expression.And(t => t.F_MailingType == mType);
            }



            return service.FindList(expression, pagination).OrderByDescending(t => t.F_CreatorTime).ToList();
        }

        /// <summary>
        /// 获取所有要发送的邮件
        /// </summary>
        /// <returns></returns>
        public List<MmailingRecordEntity> GetUnsentList()
        {
            var expression = ExtLinq.True<MmailingRecordEntity>();

            expression = expression.And(t => t.F_DeleteMark == null)
                .And(t => t.F_ExpectedToSendTime <= DateTime.Now)
                .And(t => t.F_MailingType == 0);

            return service.IQueryable(expression).OrderBy(t => t.F_ExpectedToSendTime).ToList();

        }



        public MmailingRecordEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);

        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(MmailingRecordEntity entity, string keyValue)
        {
            if (!string.IsNullOrEmpty(keyValue))
            {
                entity.Modify(keyValue);
                service.Update(entity);
            }
            else
            {
                entity.Create();
                service.Insert(entity);
            }
        }
    }
}
