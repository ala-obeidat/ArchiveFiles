using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;

namespace Archive_files
{
    public partial class ArchiveFiles : ServiceBase
    {
        private bool ServiceWorking = true;
        private readonly TimeSpan Interval = new TimeSpan(Convert.ToInt32(ConfigurationManager.AppSettings["Interval"]), 0, 0);
        private readonly string SourceDirectory = ConfigurationManager.AppSettings["SourceDirectory"];
        private readonly string DestinationDirectory = ConfigurationManager.AppSettings["DestinationDirectory"];


        private readonly int LastAccessTime = Convert.ToInt32(ConfigurationManager.AppSettings["LastAccessTime"]);
        public ArchiveFiles()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            StartingService();
        }

        protected override void OnStop()
        {
            ServiceWorking = false;
        }
        public void StartingService()
        {
            Task.Run(async () =>
            {
                while (ServiceWorking)
                {
                    string outputFileName = string.Empty;
                    try
                    {
                        string[] files = Directory.GetFiles(SourceDirectory, "*", SearchOption.AllDirectories);
                        if (files != null && files.Length > 0)
                        {
                            await Task.Delay(2000);

                            outputFileName = Path.Combine(DestinationDirectory, DateTime.Now.ToString("dd-MM-yyyy") + ".zip");
                            await ArchiveFileCompressed(files, outputFileName);
                        }
                    }
                    catch (Exception e)
                    {
                        List<string> messages = new List<string>();
                        Exception temp = e;
                        do
                        {
                            messages.Add(temp.Message);
                            temp = temp.InnerException;

                        } while (temp.InnerException != null);

                        File.AppendAllLines(outputFileName.Replace("zip", "txt"), messages);
                    }
                    finally
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        await Task.Delay(Interval);
                    }

                }
            });
        }

        private async Task ArchiveFileCompressed(string[] files, string outputFileName)
        {
            if (files != null && files.Length > 0)
            {
                var oldAccessFile = files.Where(f => File.GetLastAccessTime(f).AddHours(LastAccessTime) < DateTime.Now);
                if (oldAccessFile.Any())
                {
                    if (File.Exists(outputFileName))
                    {
                        outputFileName = outputFileName + "  " + Guid.NewGuid().ToString();
                    }
                    using (FileStream outputDirectory = new FileStream(outputFileName, FileMode.Create))
                    {
                        CompressDirectory(oldAccessFile, outputDirectory);
                    }
                    await Task.Delay(2000);
                    try
                    {
                        File.AppendAllLines(outputFileName.Replace(".zip", " List.txt"), oldAccessFile);
                        await Task.Delay(200);
                    }
                    catch { }
                }
            }

        }

        private void CompressDirectory(IEnumerable<string> filenames, Stream outPutStream)
        {

            // 'using' statements guarantee the stream is closed properly which is a big source
            // of problems otherwise.  Its exception safe as well which is great.
            using (ZipOutputStream s = new ZipOutputStream(outPutStream))
            {

                s.SetLevel(9); // 0 - store only to 9 - means best compression

                byte[] buffer = new byte[4096];

                foreach (string file in filenames)
                {

                    // Using GetFileName makes the result compatible with XP
                    // as the resulting path is not absolute.
                    var entry = new ZipEntry(file);

                    // Setup the entry data as required.

                    // Crc and size are handled by the library for seakable streams
                    // so no need to do them here.

                    // Could also use the last write time or similar for the file.
                    entry.DateTime = DateTime.Now;
                    s.PutNextEntry(entry);

                    using (FileStream fs = File.OpenRead(file))
                    {

                        // Using a fixed size buffer here makes no noticeable difference for output
                        // but keeps a lid on memory usage.
                        int sourceBytes;
                        do
                        {
                            sourceBytes = fs.Read(buffer, 0, buffer.Length);
                            s.Write(buffer, 0, sourceBytes);
                        } while (sourceBytes > 0);
                    }
                    Task.Delay(2000);
                }

                // Finish/Close arent needed strictly as the using statement does this automatically

                // Finish is important to ensure trailing information for a Zip file is appended.  Without this
                // the created file would be invalid.
                s.Finish();

                // Close is important to wrap things up and unlock the file.
                s.Close();
            }
        }
    }
}
