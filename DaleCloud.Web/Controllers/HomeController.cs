/*******************************************************************************
 * Copyright © 2016 DaleCloud.Framework 版权所有
 * Author: DaleCloud
 * Description: DaleCloud快速开发平台
 * Website：
*********************************************************************************/
using DaleCloud.Application.SystemManage;
using DaleCloud.Code;
using DaleCloud.Entity.SystemManage;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;

namespace DaleCloud.Web.Controllers
{
    [HandlerLogin]
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            OperatorModel model = DaleCloud.Code.OperatorProvider.Provider.GetCurrent();
            if (model != null)
            {
                ViewData["UserId"] = model.UserId;
                ViewData["UserCode"] = model.UserCode;
                ViewData["UserName"] = model.UserName;
            }
            return View();
        }
        [HttpGet]
        public ActionResult Default()
        {
            return View();
        }
        [HttpGet]
        public ActionResult About()
        {
            return View();
        }
    }
}
