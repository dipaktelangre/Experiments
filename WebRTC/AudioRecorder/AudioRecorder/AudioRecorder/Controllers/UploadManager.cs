using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;


namespace DocBlueList.Web.Managers
{
    public class UploadManager
    {
        public void Upload(HttpFileCollectionBase files, string folderPath, string fileSuffix)
        {


            bool exists = Directory.Exists(folderPath);

            if (!exists)
                Directory.CreateDirectory(folderPath);

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];

                var fileName = GetFileNameWithSuffix(Path.GetFileName(file.FileName), fileSuffix);

                //Create Thumbnail 
                CreateThumnail(file, folderPath, fileSuffix);
                //var path = Path.Combine(folderPath, fileName);
                file.SaveAs(Path.Combine(folderPath, fileName));
            }
        }


        public void CreateThumnail(HttpPostedFileBase file, string folderPath, string fileSuffix)
        {
            using (var image = Image.FromStream(file.InputStream, true, true)) /* Creates Image from specified data stream */
            {
                using (var thumb = image.GetThumbnailImage(
                     36, /* width*/
                     30, /* height*/
                     () => false,
                     IntPtr.Zero))
                {
                    var jpgInfo = ImageCodecInfo.GetImageEncoders().Where(codecInfo => codecInfo.MimeType == "image/png").First(); /* Returns array of image encoder objects built into GDI+ */
                    using (var encParams = new EncoderParameters(1))
                    {

                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }
                        //Change file extention to thumb
                        var fileName = Path.ChangeExtension(file.FileName, "thumb");

                        string outputPath = Path.Combine(folderPath, GetFileNameWithSuffix(fileName, fileSuffix));
                        long quality = 100;
                        encParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                        thumb.Save(outputPath, jpgInfo, encParams);
                        //thumb.Save(Path.ChangeExtension(file.FileName, "thumb"))
                    }
                }
            }
        }


        public String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }

        public string GetFileNameWithSuffix(string fileName, string fileSuffix)
        {
            string namewithoutExt = Path.GetFileNameWithoutExtension(fileName);
            string fileExt = Path.GetExtension(fileName);

            return namewithoutExt + fileSuffix + fileExt;

        }


    }
}



