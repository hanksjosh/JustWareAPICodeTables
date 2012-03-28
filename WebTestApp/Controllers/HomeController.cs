using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JustWareApiCodeTables;
using JustWareApiCodeTables.JustWareApi;

namespace WebTestApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";
            WebCodeTableCache wctc = new WebCodeTableCache(HttpContext.Cache);
            List<CaseType> ctList = new List<CaseType>();

            bool isCached = wctc.IsCodeTableCached<CaseType>();
            ctList.Add(new CaseType(){Code = "C", Description = "D"});
            wctc.AddToDictionary<CaseType>(ctList);

            List<CaseType> cachedList = wctc.QueryCacheCodeTable<CaseType>("1==1");

            isCached = wctc.IsCodeTableCached<CaseType>();

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your quintessential app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your quintessential contact page.";

            return View();
        }
    }
}
