using System.Web.Mvc;

namespace DaleCloud.Web.Areas.WeixinApp
{
    public class WeixinAppAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "WeixinApp";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                this.AreaName + "_Default",
                this.AreaName + "/{controller}/{action}/{id}",
                new { area = this.AreaName, controller = "Home", action = "Index", id = UrlParameter.Optional },
                new string[] { "DaleCloud.Web.Areas." + this.AreaName + ".Controllers" }
            );
        }
    }
}