using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FFmpegConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            //string basePath = System.AppDomain.CurrentDomain.BaseDirectory;
            Console.ReadLine();
            string basePath = "D:\\Dipak\\Raw\\Demos\\FFmpegConverter\\FFmpegConverter\\ConsoleApplication1";
            string ffmpegLib = "FFmpeg/ffmpeg.exe";
            string inputFolder = "Files/input/";
            string outputFolder = "Files/output/";

            string input1 = Path.Combine(basePath, inputFolder + "1.wav"),
                input2 = Path.Combine(basePath, inputFolder + "1.webm"),
                output = Path.Combine(basePath, outputFolder + "1.webm"),
                logFile = Path.Combine(basePath, outputFolder + "1.txt");

            bool isOwerWriteLog = false;
            Process proc = new Process();
            
            proc.StartInfo.FileName = Path.Combine(basePath,ffmpegLib);
            //proc.StartInfo.Arguments = "-i " + args[0] + " " + args[1];

            proc.StartInfo.Arguments = String.Format("-y -i {0} -i {1} {2}", input1, input2, output);
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.UseShellExecute = false;
            if (!proc.Start())
            {
                Console.WriteLine("Error starting");
                return;
            }
            StreamReader reader = proc.StandardError;
            string line;
            // Read Line by line 
            decimal totalDuration = 0.0m;
            decimal currentDuration = 0.0m;
            decimal percentage = 0.0m;
            Console.WriteLine("Initializing ...");
            WriteLog(logFile, "Initializing ...", isOwerWriteLog);
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
                    totalDuration = hrs*60*60 + min*60 + sec;
                   // Console.WriteLine("Total duration : {0} sec", totalDuration);
                }
                //Console.WriteLine(line);

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
                    //Console.WriteLine("Current duration : {0} sec", currentDuration);
                }

                if (currentDuration != 0.0m)
                {
                    percentage = currentDuration / totalDuration * 100;
                    Console.WriteLine("Progress : {0} %", percentage.ToString("F"));
                    WriteLog(logFile, String.Format("Progress : {0} %", percentage.ToString("F")), isOwerWriteLog);
                }
                   

                
            }
            proc.Close();
            Console.WriteLine("Completed !");
            WriteLog(logFile, "Completed !", isOwerWriteLog);
            Console.ReadLine();
        }

        public static bool WriteLog(string path, string content, bool isOverWrite)
        {
            try
            {
                if(isOverWrite)
                    File.WriteAllText(path,content);
                else
                    File.AppendAllText(path,content+"\n");
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }
    }
}
