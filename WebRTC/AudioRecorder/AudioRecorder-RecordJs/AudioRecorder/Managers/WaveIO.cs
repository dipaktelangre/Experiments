/**********************************
 Wave concatenation class

 CopyRights Ehab Essa 2006
***********************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
namespace Concatenation_Waves
{
    class WaveIO : IDisposable
    {
        // some fields that require cleanup
        private SafeHandle handle;
        private bool disposed = false; // to detect redundant calls
        public int length;
        public short  channels;
        public int samplerate;
        public int DataLength;
        public short BitsPerSample;

        private  void WaveHeaderIN(string spath)
        {
            FileStream fs = new FileStream(spath, FileMode.Open, FileAccess.Read);
         
            BinaryReader br = new BinaryReader(fs);
            length = (int)fs.Length - 8;
            fs.Position = 22;
            channels = br.ReadInt16();
            fs.Position = 24;
            samplerate = br.ReadInt32();
            fs.Position = 34;

            BitsPerSample = br.ReadInt16();
            DataLength = (int)fs.Length - 44;
            br.Close ();
            fs.Close();

        }
  
        private  void WaveHeaderOUT(string sPath)
        {
            FileStream fs = new FileStream(sPath, FileMode.Create, FileAccess.Write );

            BinaryWriter bw = new BinaryWriter(fs);
            fs.Position = 0;
            bw.Write(new char[4] { 'R', 'I', 'F', 'F' });
       
            bw.Write(length);
          
            bw.Write(new char[8] {'W','A','V','E','f','m','t',' '});
          
            bw.Write((int)16);
  
            bw.Write((short)1);
            bw.Write(channels);
        
            bw.Write(samplerate );
       
            bw.Write((int)(samplerate * ((BitsPerSample * channels) / 8)));
        
            bw.Write((short )((BitsPerSample * channels) / 8));
       
            bw.Write(BitsPerSample);
      
            bw.Write(new char[4] {'d','a','t','a'});
            bw.Write(DataLength);
            bw.Close();
            fs.Close();
        }
        public void Merge(string[] files, string outfile)
        {
            WaveIO wa_IN = new WaveIO();
            WaveIO wa_out = new WaveIO();

            wa_out.DataLength = 0;
            wa_out.length = 0;


            //Gather header data
            foreach (string path in files)
            {
                wa_IN.WaveHeaderIN(@path);
                wa_out.DataLength += wa_IN.DataLength;
                wa_out.length += wa_IN.length;

            }

            //Recontruct new header
            wa_out.BitsPerSample = wa_IN.BitsPerSample;
            wa_out.channels = wa_IN.channels;
            wa_out.samplerate = wa_IN.samplerate;
            wa_out.WaveHeaderOUT(@outfile);

            foreach (string path in files)
            {
                FileStream fs = new FileStream(@path, FileMode.Open, FileAccess.Read);
                FileStream fo = new FileStream(@outfile, FileMode.Append, FileAccess.Write, FileShare.None);
                BinaryWriter bw = new BinaryWriter(fo);
                fs.Position = 44;

                byte[] arrfile = new byte[fs.Length - 44];

                fs.Read(arrfile, 0, arrfile.Length);
                fs.Close();

                
                bw.Write(arrfile);
                bw.Close();
                fo.Close();
            }
          }


        // Append new file data without header to existing oldfile 
        public void Append(string oldFile, string newFile)
        {
            WaveIO wa_IN = new WaveIO();
            WaveIO wa_out = new WaveIO();

            wa_out.DataLength = 0;
            wa_out.length = 0;


            //Gather header data
            string[] files = new[] {oldFile, newFile};
 
            foreach (string path in files)
            {
                wa_IN.WaveHeaderIN(@path);
                wa_out.DataLength += wa_IN.DataLength;
                wa_out.length += wa_IN.length;

            }

            // change header of existing file 
            wa_out.ChangeHeader(@oldFile);


            FileStream fs = new FileStream(@newFile, FileMode.Open, FileAccess.Read);
            byte[] arrfile = new byte[fs.Length - 44];
            fs.Position = 44;
            fs.Read(arrfile, 0, arrfile.Length);
            fs.Close();


            //Read existing file append new file data 
            FileStream fo = new FileStream(@oldFile, FileMode.Append, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fo);
            bw.Write(arrfile);
            bw.Close();
            fo.Close();
            
        }

        public void ChangeHeader(string sPath)
        {
            FileStream fs = new FileStream(sPath, FileMode.Append, FileAccess.Write);

            BinaryWriter bw = new BinaryWriter(fs);
           // fs.Position = 0;
            //bw.Write(new char[4] { 'R', 'I', 'F', 'F' });
            fs.Position = 4;

            bw.Write(length);

            bw.Write(new char[8] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });

            bw.Write((int)16);

            bw.Write((short)1);
            bw.Write(channels);

            bw.Write(samplerate);

            bw.Write((int)(samplerate * ((BitsPerSample * channels) / 8)));

            bw.Write((short)((BitsPerSample * channels) / 8));

            bw.Write(BitsPerSample);

            bw.Write(new char[4] { 'd', 'a', 't', 'a' });
            bw.Write(DataLength);
            bw.Close();
            fs.Close();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    if (handle != null)
                    {
                        handle.Dispose();
                    }
                }

                // Dispose unmanaged managed resources.

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
