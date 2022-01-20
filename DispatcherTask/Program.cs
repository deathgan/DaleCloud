using DaleCloud.Code;
using DaleCloud.DBUtility;
using DaleCloud.Entity.MissionManage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherTask
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("**************任务调度程序已启动**************", ConsoleColor.Green);

            DALHelper<MmailingRecordEntity> dALHelper = new DALHelper<MmailingRecordEntity>("MissionManage_MmailingRecord");

            List<MmailingRecordEntity> list = dALHelper.GetList("F_ExpectedToSendTime <= GetDate() And F_MailingType=0 And F_DeleteMark is Null");


            int count = (list == null || list.Count == 0) ? 0 : list.Count;

            Console.WriteLine($"**************未发送条数{count}**************", ConsoleColor.Green);

            if (count > 0) 
            {
                foreach (var item in list)
                {
                    Console.WriteLine($"**************正在发送邮件，收件人地址{item.F_EmailAddress}**************", ConsoleColor.Green);
                    MailHelper mail = new MailHelper();
                    mail.MailServer = Configs.GetValue("MailHost");
                    mail.MailUserName = Configs.GetValue("MailUserName");
                    mail.MailPassword = Configs.GetValue("MailPassword");
                    //   mail.MailName = "DaleCloud快速开发平台";
                    mail.Send(item.F_EmailAddress, "计划任务提醒", item.F_EmailContent);

                    item.F_MailingType = 1;//已发送

                    dALHelper.UpdateField(item, "F_MailingType", "1", $"F_Id='{item.F_Id}'");

                    Console.WriteLine($"**************邮件发送完成**************", ConsoleColor.Red);

                }

            }




            //    EveryDayRemindTask




            Console.ReadKey();
        }
    }
}
