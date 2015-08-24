using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concatenation_Waves;
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
                //string fileSuffix = uploadManager.GetTimestamp(DateTime.Now);
                var fileName = Request.Form["fileName"];
                var file = Request.Files["blob"];
                string filePath = Path.Combine(folder, fileName+".wav");

                string outFilePath = Path.Combine(folder, fileName +"o"+ ".wav");
                //File not exist, write it 


                if (!System.IO.File.Exists(filePath))
                {
                    file.SaveAs(filePath);
                }
                else
                {
                    string tempFile = Path.Combine(folder, fileName+"-temp" + ".wav");
                    file.SaveAs(tempFile);
                    string[] filesToMerge = new string[2] {filePath,tempFile};

                    WaveIO wa = new WaveIO();
                    wa.Merge(filesToMerge, outFilePath);

                    System.IO.File.Delete(filePath);
                    System.IO.File.Delete(tempFile);
                    System.IO.File.Move(outFilePath, filePath);

                }

                //var blob = Request.Form["blob"];

                //Request.SaveAs(Server.MapPath("~/" + "Files/" + fileSuffix + ".wav"), false);
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
