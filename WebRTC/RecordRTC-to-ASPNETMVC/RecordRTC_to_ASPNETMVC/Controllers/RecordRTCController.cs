using System;
using System.Diagnostics;
using System.EnterpriseServices.CompensatingResourceManager;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using RecordRTC_to_ASPNETMVC.Models;

namespace RecordRTC_to_ASPNETMVC.Controllers
{
    // www.MuazKhan.com
    // www.WebRTC-Experiment.com
    // RecordRTC.org
    public class RecordRTCController : Controller
    {
        // ---/RecordRTC/
        public ActionResult Index()
        {
            return View();
        }

        // ---/RecordRTC/PostRecordedAudioVideo
       [HttpPost]
        public ActionResult PostRecordedAudioVideo()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + "uploads/";
           string temp = Path.ChangeExtension(Request.Form[0], ".txt");
            var logFile = Path.Combine(path, temp); //AppDomain.CurrentDomain.BaseDirectory + "uploads/log.txt";

           WriteLog(logFile, new Status { State = "inProgress", StatusCode = 2, StatusMessage = "Uploading ..."}, true);
            foreach (string upload in Request.Files)
            {
                
                var file = Request.Files[upload];
                if (file == null) continue;

                file.SaveAs(Path.Combine(path, Request.Form[0]));
                var fileName = Path.Combine(path, Request.Form[0]);
                
            }
            WriteLog(logFile, new Status { State = "inProgress", StatusCode = 2, StatusMessage = "Some chunks uploaded ..."}, true);
            return Json(Request.Form[0]);
        }

        

        // ---/RecordRTC/DeleteFile
        [HttpPost]
        public ActionResult DeleteFile()
        {
            var fileUrl = AppDomain.CurrentDomain.BaseDirectory + "uploads/" + Request.Form["delete-file"];
            new FileInfo(fileUrl + ".wav").Delete();
            new FileInfo(fileUrl + ".webm").Delete();
            return Json(true);
        }

        // ---/RecordRTC/DeleteFile
        [HttpPost]
        public ActionResult ConvertFile()
        {

            var audioFile = AppDomain.CurrentDomain.BaseDirectory + "uploads/" + Request.Form["audio-filename"];
            var videoFile = AppDomain.CurrentDomain.BaseDirectory + "uploads/" + Request.Form["video-filename"];
            //var mp4File = Path.ChangeExtension(audioFile, ".webm");
            var temp = Path.GetFileNameWithoutExtension(videoFile);
            var output = AppDomain.CurrentDomain.BaseDirectory + "uploads/" + temp+"v"+".webm";// Request.Form["video-filename"]
            var logFile = AppDomain.CurrentDomain.BaseDirectory + "uploads/" + temp + ".txt";
            string outFile = "";

            WriteLog(logFile,new Status { State = "inProgress", StatusCode = 2, StatusMessage = "File conversion started ..."}, true);

            if (System.IO.File.Exists(audioFile) && System.IO.File.Exists(videoFile))
                outFile = ConvertFile(audioFile, videoFile, output, logFile);
            return Json(outFile);
        }

        // 
        [HttpGet]
        public JsonResult CheckStatus(string logFile)
        {
            var file = AppDomain.CurrentDomain.BaseDirectory + "uploads/" + logFile;
            Status s = new Status(){
                StatusCode = 6,
                State = "NotFound",
                StatusMessage = "File Not found"
            };
            
            if (System.IO.File.Exists(file))
            {
                string statusContent = "";
                try
                {
                     statusContent = System.IO.File.ReadAllText(file);
                    // If Conversion completed delete log file

                    if (statusContent.Contains("\"State\":\"completed\""))
                    {
                        new FileInfo(file).Delete();
                    }
                }
                catch (Exception e)
                {
                    Status fileLocked = new Status(){
                        StatusCode = 7,
                        State = "FileLocked",
                        StatusMessage = "Log file locked"
                    };
                    return Json(fileLocked, JsonRequestBehavior.AllowGet);
                }
                if (statusContent == "")
                {
                    return Json(s, JsonRequestBehavior.AllowGet);
                }

                return Json(statusContent, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }


        private string ConvertFile(string wavFile, string webmFile, string mp4File, string logFile)
        {
            var exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Converter/ffmpeg.exe");
            var parameters = String.Format(" -i {0} -i {1} {2}", wavFile, webmFile, mp4File);
            bool isOwerWriteLog = true;

            using (Process p = new Process())
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.FileName = exePath;
                p.StartInfo.Arguments = parameters;
                p.Start();
                

                #region ProcessBar

                    StreamReader reader = p.StandardError;
                    string line;
                    // Read Line by line 
                    decimal totalDuration = 0.0m;
                    decimal currentDuration = 0.0m;
                    decimal percentage = 0.0m;
                    WriteLog(logFile, new Status { State = "inProgress", StatusCode = 2, StatusMessage = "Initializing converter ..." }, isOwerWriteLog);
                    while ((line = reader.ReadLine()) != null)
                    {
                        //Get total Duration 
                        // MatchCollection matchCollection = Regex.Matches(line, "Duration: (.*?), bitrate:");
                        MatchCollection matchCollection = Regex.Matches(line, "Duration: [0-9]{2}:[0-9]{2}:[0-9]{2}[.][0-9]{2}, bitrate:");
                        if (matchCollection.Count > 0)
                        {

                            string rawDuration = Regex.Match(matchCollection[0].Value, "[0-9]{2}:[0-9]{2}:[0-9]{2}[.][0-9]{2}").ToString();
                            string[] tempSplit = rawDuration.Split(':');
                            int hrs = Convert.ToInt16(tempSplit[0]);
                            int min = Convert.ToInt16(tempSplit[1]);
                            decimal sec = Convert.ToDecimal(tempSplit[2]);
                            totalDuration = hrs * 60 * 60 + min * 60 + sec;
                        }

                        // Get current duration 

                        Match match = Regex.Match(line, "time=(.*?) bitrate");
                        if (match.Value != "")
                        {
                            string rawTime = Regex.Match(match.Value, "[0-9]{2}:[0-9]{2}:[0-9]{2}[.][0-9]{2}").ToString();
                            string[] tempSplit = rawTime.Split(':');
                            int hrs = Convert.ToInt16(tempSplit[0]);
                            int min = Convert.ToInt16(tempSplit[1]);
                            decimal sec = Convert.ToDecimal(tempSplit[2]);
                            currentDuration = hrs * 60 * 60 + min * 60 + sec;
                        }

                        if (currentDuration != 0.0m)
                        {
                            percentage = currentDuration / totalDuration * 100;
                            string msg = String.Format("Progress : {0} %", percentage.ToString("F"));

                            WriteLog(logFile, new Status { State = "inProgress", StatusCode = 2, StatusMessage = msg}, isOwerWriteLog);
                        }
                    } // While line

                    WriteLog(logFile, new Status { State = "completed", StatusCode = 4, StatusMessage = "Done !" }, isOwerWriteLog);

                #endregion

                p.WaitForExit();
                //var result = p.StandardOutput.ReadToEnd();
            } // Using 

            //Delete raw file 
            //new FileInfo(wavFile).Delete();
            //new FileInfo(webmFile).Delete();

            return Path.GetFileName(mp4File);

        }


        public static bool WriteLog(string path, Status content, bool isOverWrite)
        {
            var json = new JavaScriptSerializer().Serialize(content);
            try
            {
                if (isOverWrite)
                    System.IO.File.WriteAllText(path, json);
                else
                    System.IO.File.AppendAllText(path, json + "\n");
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }
    }
}