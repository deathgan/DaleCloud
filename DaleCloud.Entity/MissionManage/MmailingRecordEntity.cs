/*******************************************************************************
 * Copyright © 2016 NFine.Framework 版权所有
 * Author: DaleCloud
 * Description: DaleCloud快速开发平台
 * Website：
*********************************************************************************/
using System;

namespace DaleCloud.Entity.MissionManage
{
    public class MmailingRecordEntity : IEntity<MmailingRecordEntity>, ICreationAudited, IDeleteAudited, IModificationAudited
    {
        public string F_Id { get; set; }
        /// <summary>
        /// 邮件发送状态
        /// </summary>
        public int F_MailingType { get; set; }

        /// <summary>
        /// 预计发送时间
        /// </summary>
        public DateTime? F_ExpectedToSendTime { get; set; }

        /// <summary>
        /// 收件人邮箱地址
        /// </summary>
        public string F_EmailAddress { get; set; }

        /// <summary>
        /// 发送内容
        /// </summary>
        public string F_EmailContent { get; set; }

        public int? F_SortCode { get; set; }
        public bool? F_DeleteMark { get; set; }
        public bool? F_EnabledMark { get; set; }
        public string F_Description { get; set; }
        public DateTime? F_CreatorTime { get; set; }
        public string F_CreatorUserId { get; set; }
        public DateTime? F_LastModifyTime { get; set; }
        public string F_LastModifyUserId { get; set; }
        public DateTime? F_DeleteTime { get; set; }
        public string F_DeleteUserId { get; set; }
    }
}
