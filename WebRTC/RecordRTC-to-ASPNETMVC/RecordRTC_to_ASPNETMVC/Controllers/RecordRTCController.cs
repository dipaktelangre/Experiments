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

        string path = AppDomain.CurrentDomain.BaseDirectory + "uploads/";

        string audioFilesPath = AppDomain.CurrentDomain.BaseDirectory + "uploads/Audio/";
        string videoFilesPath = AppDomain.CurrentDomain.BaseDirectory + "uploads/Video/";

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult MultiStramRecorder()
        {
            return View();
        }

        public ActionResult MultiStramRecorderTemp()
        {
            return View();
        }


        // ---/RecordRTC/PostRecordedAudioVideo
       [HttpPost]
        public ActionResult PostRecordedAudioVideo()
       {
           string fileType = Request.Form["fileType"]; 
           var folderPath = Path.Combine(path, Request.Form["mainFileName"]);
           if (fileType == "Video")
           {
               folderPath = Path.Combine(videoFilesPath, Request.Form["mainFileName"]);
           }

           if (fileType == "Audio")
           {
               folderPath = Path.Combine(audioFilesPath, Request.Form["mainFileName"]);
           }

            var chunkFileName = Path.Combine(folderPath, Request.Form["chunkFileName"]);
            var mainFileName = Path.Combine(folderPath, Request.Form["mainFileName"]);

            //string temp = Path.ChangeExtension(Request.Form["chunkFileName"], ".txt");

            var logFile = Path.Combine(folderPath, mainFileName+"-log.txt"); //AppDomain.CurrentDomain.BaseDirectory + "uploads/log.txt";

            CheckForDirectory(folderPath);

          // WriteLog(logFile, new Status { State = "inProgress", StatusCode = 2, StatusMessage = "Uploading ..."}, true);
            foreach (string upload in Request.Files)
            {
                var file = Request.Files[upload];
                if (file == null) continue;

                file.SaveAs(chunkFileName);
                
            }

            //WriteLog(logFile, new Status { State = "inProgress", StatusCode = 2, StatusMessage = "Some chunks uploaded ..."}, true);
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
            var folderPath = Path.Combine(videoFilesPath + Request.Form["mainFileName"]+"/");
            var audioFile = Path.Combine(folderPath+ Request.Form["audio-filename"]);
            var videoFile = Path.Combine(folderPath + Request.Form["video-filename"]);
            var convertCounter = Convert.ToInt16(Request.Form["convertCounter"]);
            string mainFile = Request.Form["mainFileName"];

            //var mp4File = Path.ChangeExtension(audioFile, ".webm");
            var temp = Path.GetFileNameWithoutExtension(videoFile);
            var output =  Path.Combine(folderPath,temp+"v"+".webm");

            var logFile =  Path.Combine(folderPath,temp, temp + ".txt");
            var logFileConversion = Path.Combine(folderPath, mainFile + "-con-log.txt");

            WriteLog(logFileConversion, "+++++++++++++++++++++++++++ Request for conversion ++++++++++++");
            WriteLog(logFileConversion, "Audio File -> " + Request.Form["audio-filename"] + "Video File ->" + Request.Form["video-filename"]);


            string outFile = "";

           // WriteLog(logFile,new Status { State = "inProgress", StatusCode = 2, StatusMessage = "File conversion started ..."}, true);

            if (System.IO.File.Exists(audioFile) && System.IO.File.Exists(videoFile))
            {
                WriteLog(logFileConversion,
                    "---------------------------------------------File Found : Convert it !--------------------------------");
                WriteLog(logFileConversion, "FileName : " + audioFile + " " + videoFile);

                outFile = ConvertFile(audioFile, videoFile, output, logFileConversion);

                var tempOutFile =  AppDomain.CurrentDomain.BaseDirectory +"uploads\\Video\\"+ mainFile + "\\" + outFile;
               // outFile = folderPath +"\\"+outFile;
                string fileContent = String.Format("file \'{0}\' \n", tempOutFile);
                var mergeFile = Path.Combine(folderPath, mainFile + ".txt");
                System.IO.File.AppendAllText(mergeFile, fileContent);
                convertCounter++;

                return Json(new { counter = convertCounter, converted = true, fileName = outFile,message = "done" });
            }

            return Json(new { counter = convertCounter, converted = false, fileName = outFile, message="file not arrieved yet" });
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

        [HttpPost]
        public JsonResult MergeChunkFiles(string mainFileName)
        {
            var fileType = Request.Form["fileType"];
            var mergerTxtFile = AppDomain.CurrentDomain.BaseDirectory + "uploads\\" + mainFileName + "\\" + mainFileName + ".txt";
            var folderPath = Path.Combine(videoFilesPath + mainFileName);
            var outputFile = Path.Combine(folderPath, mainFileName + ".webm");
            if (fileType == "Video")
            {
                folderPath = Path.Combine(videoFilesPath + mainFileName);
                mergerTxtFile = AppDomain.CurrentDomain.BaseDirectory + "uploads\\Video\\" + mainFileName + "\\" + mainFileName + ".txt";
                outputFile = Path.Combine(folderPath, mainFileName + ".webm");
            }
            if (fileType == "Audio")
            {
                folderPath = Path.Combine(audioFilesPath + mainFileName);
                mergerTxtFile = AppDomain.CurrentDomain.BaseDirectory + "uploads\\Audio" + mainFileName + "\\" + mainFileName + ".txt";
                outputFile = Path.Combine(folderPath, mainFileName + ".wav");
                MergeChunkFiles(mergerTxtFile, outputFile);
                return Json("Video/"+mainFileName + "/" + mainFileName + ".wav");
            }

            //var mergerTxtFile = Path.Combine(folderPath, mainFileName + ".txt");//folderPath + "/" + mainFileName + ".txt";

            MergeChunkFiles(mergerTxtFile, outputFile);
            return Json("Video/" + mainFileName + "/" + mainFileName + ".webm");
            
        }

        private void MergeChunkFiles(string mergerTxtFile, string outputFile)
        {
            var exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Converter/ffmpeg.exe");

            //ffmpeg -f concat -i f.txt -c copy Audio1.wav
            var parameters = String.Format("-f concat -i {0} -c copy {1}", mergerTxtFile,outputFile);
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
                StreamReader reader = p.StandardError;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    System.IO.File.AppendAllText(mergerTxtFile+"-merge-log.txt",line);
                }
                p.WaitForExit();
                //var result = p.StandardOutput.ReadToEnd();
            } // Using
        }


        private string ConvertFile(string wavFile, string webmFile, string mp4File, string logFile)
        {
            var exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Converter/ffmpeg.exe");
            var parameters = String.Format(" -i {0} -i {1} {2}", wavFile, webmFile, mp4File);
            bool isOwerWriteLog = true;

            WriteLog(logFile, "-----------Init-------------");

            WriteLog(logFile, "Parameters ->" + parameters);
            using (Process p = new Process())
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.FileName = exePath;
                p.StartInfo.Arguments = parameters;

                WriteLog(logFile, "-----------Starting convrsion -------- ");

                WriteLog(logFile, "Audio file  " + wavFile);
                WriteLog(logFile, "Video File" + webmFile);

                p.Start();
                

                #region ProcessBar

                    StreamReader reader = p.StandardError;
                    string line;
                    // Read Line by line 
                    decimal totalDuration = 0.0m;
                    decimal currentDuration = 0.0m;
                    decimal percentage = 0.0m;
                   // WriteLog(logFile, new Status { State = "inProgress", StatusCode = 2, StatusMessage = "Initializing converter ..." }, isOwerWriteLog);
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

                            WriteLog(logFile, msg);
                           // WriteLog(logFile, new Status { State = "inProgress", StatusCode = 2, StatusMessage = msg}, isOwerWriteLog);
                        }
                    } // While line

                    //WriteLog(logFile, new Status { State = "completed", StatusCode = 4, StatusMessage = "Done !" }, isOwerWriteLog);

                #endregion

                p.WaitForExit();
                //var result = p.StandardOutput.ReadToEnd();
            } // Using 

            //Delete raw file 
            //new FileInfo(wavFile).Delete();
            //new FileInfo(webmFile).Delete();

            WriteLog(logFile, "---------------- Completed ---------- File : " + mp4File);
            return Path.GetFileName(mp4File);

        }




        public static bool WriteLog(string path, string content, bool isOverWrite = false)
        {
                if (isOverWrite)
                    System.IO.File.WriteAllText(path, content);
                else
                    System.IO.File.AppendAllText(path, content + "\n");
            
            return true;
        }


        //public static bool WriteLog(string path, Status content, bool isOverWrite)
        //{
        //    var json = new JavaScriptSerializer().Serialize(content);
        //    try
        //    {
        //        if (isOverWrite)
        //            System.IO.File.WriteAllText(path, json);
        //        else
        //            System.IO.File.AppendAllText(path, json + "\n");
        //    }
        //    catch (Exception e)
        //    {
        //        return false;
        //    }
        //    return true;
        //}

        public static void CheckForDirectory(string dirPath)
        {
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

        }
    }
}