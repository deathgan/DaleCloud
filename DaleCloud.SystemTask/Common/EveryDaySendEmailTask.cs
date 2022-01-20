using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DaleCloud.Code;
using DaleCloud.DingTalk;
using DaleCloud.DingTalk.Entities;
using DaleCloud.Entity.DingTalk;
using DaleCloud.Application.DingTalk;
using DaleCloud.Application.CommonService;
using FluentScheduler;
using DaleCloud.Application.MissionManage;

namespace DaleCloud.SystemTask
{
    public class EveryDaySendEmailTask : IJob
    {
        public EveryDaySendEmailTask()
        {
            //System.Threading.Thread tasktimer = new System.Threading.Thread(new System.Threading.ThreadStart(SendRemindMsg));
            //tasktimer.Start();
        }
        /// <summary>
        /// 开启执行任务
        /// </summary>
        void IJob.Execute()
        {
            SendingEmail();
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        public void SendingEmail()
        {
            MmailingRecordApp app = new MmailingRecordApp();         
            MailHelper mail = new MailHelper();
            mail.MailServer = Configs.GetValue("MailHost");
            mail.MailUserName = Configs.GetValue("MailUserName");
            mail.MailPassword = Configs.GetValue("MailPassword");               
            try
            {
                var list = app.GetUnsentList();
                

                if (list != null && list.Count > 0)
                {                    
                    foreach (var item in list)
                    {
                        mail.Send(item.F_EmailAddress, "计划任务提醒", item.F_EmailContent);

                        item.F_MailingType = 1;//已发送

                        app.SubmitForm(item, item.F_Id);                        
                    }
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }



    }


}
