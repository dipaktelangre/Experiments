using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DocBlueList.Web.Managers;

namespace AudioRecorder.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        [HttpPost]
        public JsonResult Upload()
        {
            UploadManager uploadManager = new UploadManager();
            try
            {
                var folder = Server.MapPath("~/" + "Files");
                string fileSuffix = uploadManager.GetTimestamp(DateTime.Now);

                Request.SaveAs(Server.MapPath("~/" + "Files/" + fileSuffix + ".wav"), false);
                //uploadManager.Upload(Request.Files, folder, fileSuffix);
                return this.Json(new { Success = true, Status = "Uploaded Successfully" });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { Success = true, Status = "Something went wrong, Please try again." });
            }

        }

    }
}
