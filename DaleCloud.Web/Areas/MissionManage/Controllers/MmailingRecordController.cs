using DaleCloud.Application.MissionManage;
using DaleCloud.Code;
using DaleCloud.Code.Excel;
using DaleCloud.DBUtility;
using DaleCloud.Entity;
using DaleCloud.Entity.MissionManage;
using DaleCloud.Web;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;


namespace DaleCloud.Web.Areas.MissionManage.Controllers
{



    public class MmailingRecordController : ControllerBasev2
    {
        private MmailingRecordApp app = new MmailingRecordApp();
              


        public override ActionResult Index()
        {
            ViewData["templateUrl"] = "/Configs/Templates/template_sendingMail.xlsx";
            return View();
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string fromdate, string todate, string keyword, string mailingType)
        {
            try
            {
                var data = app.GetList(pagination, fromdate, todate, keyword, mailingType);

                return ResultDataGrid(pagination.total, data);
            }
            catch (Exception ex)
            {
                return Content(ex.ToJson());
            }
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = app.GetForm(keyValue);
            return Content(data.ToJson());
        }


        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(MmailingRecordEntity uEntity, string keyValue)
        {
            app.SubmitForm(uEntity, keyValue);
            return Success("操作成功。");
        }


        [HttpPost]
        [HandlerAjaxOnly]
        //[ValidateAntiForgeryToken]
        public ActionResult Sendmail(string id)
        {
            var data = app.GetForm(id);

            MailHelper mail = new MailHelper();
            mail.MailServer = Configs.GetValue("MailHost");
            mail.MailUserName = Configs.GetValue("MailUserName");
            mail.MailPassword = Configs.GetValue("MailPassword");
            //   mail.MailName = "DaleCloud快速开发平台";
            mail.Send(data.F_EmailAddress, "计划任务提醒", data.F_EmailContent);

            data.F_MailingType = 1;//已发送

            app.SubmitForm(data, id);

            return Success("操作成功。");
        }


        [HttpPost]
        public ActionResult ImportData()
        {
            HttpPostedFile file = System.Web.HttpContext.Current.Request.Files["file"];//获取http传输的文件 

            //  Guid CreatorUserId = new Guid(HttpContext.Current.Request["CreatorUserId"]);

            NPOIExcel nPOIExcel = new NPOIExcel();

            DataTable dt = nPOIExcel.ExcelToDataTable(file, null, true);

            StringBuilder sb = new StringBuilder();

            List<MmailingRecordEntity> mailingRecordEntityInsert = new List<MmailingRecordEntity>();
            if (dt == null || dt.Rows.Count == 0)
            {
                sb.Append("Excel中无数据");
            }
            else
            {

                if (!dt.Columns.Contains("收件人邮箱地址"))
                {
                    sb.AppendLine($"没有【收件人邮箱地址】列,");
                }

                if (!dt.Columns.Contains("预计发送时间"))
                {
                    sb.AppendLine($"没有【预计发送时间】列,");
                }

                if (!dt.Columns.Contains("邮件内容"))
                {
                    sb.AppendLine($"没有【邮件内容】列,");
                }
            }
           

            if (!string.IsNullOrEmpty(sb.ToString()))
            {
                return Error(sb.ToString());
            }


            for (int i = 0; i < dt.Rows.Count; i++)
            {

                string EmailAddress = dt.Rows[i]["收件人邮箱地址"].ToString().Trim();

                string ExpectedToSendTime = dt.Rows[i]["预计发送时间"].ToString().Trim();

                string EmailContent = dt.Rows[i]["邮件内容"].ToString().Trim();

                int lineNumber = i + 2;//行号

                if (string.IsNullOrWhiteSpace(EmailAddress))
                {
                    sb.AppendLine(string.Format("第{0}行，列名【收件人邮箱地址】，不能为空,", lineNumber));
                }

                else
                {
                    if (!RegexHelper.IsValidEmail(EmailAddress))
                    {
                        sb.AppendLine(string.Format("第{0}行，列名【收件人邮箱地址】，邮箱地址不正确,", lineNumber));
                    }
                }
                if (string.IsNullOrWhiteSpace(ExpectedToSendTime))
                {
                    sb.AppendLine(string.Format("第{0}行，列名【预计发送时间】，不能为空,", lineNumber));
                }
                else
                {
                    DateTime dateV;
                    if (!DateTime.TryParse(ExpectedToSendTime, out dateV))
                    {
                        sb.AppendLine(string.Format("第{0}行，列名【预计发送时间】，格式不正确,", lineNumber));
                    }

                }

                if (string.IsNullOrWhiteSpace(EmailContent))
                {
                    sb.AppendLine(string.Format("第{0}行，列名【邮件内容】，不能为空,", lineNumber));
                }
            }
            //错误信息为空 
            if (string.IsNullOrEmpty(sb.ToString()))
            {
                var LoginInfo = OperatorProvider.Provider.GetCurrent();

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    string EmailAddress = dt.Rows[i]["收件人邮箱地址"].ToString().Trim();

                    string ExpectedToSendTime = dt.Rows[i]["预计发送时间"].ToString().Trim();

                    string EmailContent = dt.Rows[i]["邮件内容"].ToString().Trim();

                    DateTime dateV;

                    DateTime.TryParse(ExpectedToSendTime, out dateV);

                    mailingRecordEntityInsert.Add(new MmailingRecordEntity
                    {
                        F_Id = Utils.GuId(),
                        F_CreatorUserId = LoginInfo == null ? string.Empty : LoginInfo.UserId,
                        F_CreatorTime = DateTime.Now,
                        F_MailingType = 0,//未发送
                        F_ExpectedToSendTime = dateV,
                        F_EmailAddress = EmailAddress,
                        F_EmailContent = EmailContent
                    });

                }
                DbHelperSQL.BulkInsert(mailingRecordEntityInsert, "MissionManage_MmailingRecord");

                return Success("操作成功。");

            }



            return Error(sb.ToString());
        }



        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            app.DeleteForm(keyValue);
            return Success("删除成功。");
        }
    }

}