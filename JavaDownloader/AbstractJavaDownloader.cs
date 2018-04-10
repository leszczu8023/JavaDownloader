using System;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace JavaDownloader
{
    public abstract class AbstractJavaDownloader
    {
        private Arch arch { get; set; }
        private string downUri = "https://www.java.com/pl/download/manual.jsp";

        public AbstractJavaDownloader(Arch architecture)
        {
            arch = architecture;
        }

        public Uri GetURI()
        {
            var webClient = new WebClient();
            var htmlData = webClient.DownloadString(downUri);
            var htmlTempData = htmlData.Split(new String[] { "<a" }, StringSplitOptions.RemoveEmptyEntries);

            var findString = (arch == Arch.I586) ? "Windows Offline\"" : "Windows Offline (64-bitowa)";

            foreach (string item in htmlTempData)
            {
                var subitem = item.Split(new String[] { "</a>" }, StringSplitOptions.RemoveEmptyEntries)[0];
                if (subitem.Contains("javadl.oracle.com"))
                {
                    if (subitem.Contains(findString))
                    {
                        var www = subitem.Split(new String[] { "href=\"" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('"')[0];
                        return new Uri(www);
                    }
                }
            }
            return null;
        }

        public abstract void ReportProgress(int percentage, long received, long total);
        public abstract void OnDownloadCompleted(string targetFileName);

        public void Download(string targetDirectory)
        {
            if (string.IsNullOrEmpty(targetDirectory))
            {
                throw new ArgumentNullException("Directory name cannot be empty!");
            }
            else if (!System.IO.Directory.Exists(targetDirectory))
            {
                throw new System.IO.DirectoryNotFoundException(targetDirectory);
            }

            var wc = new WebClient();

            wc.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
            {
                ReportProgress(e.ProgressPercentage, e.BytesReceived, e.TotalBytesToReceive);
            };

            var url = GetURI();

            wc.DownloadFileAsync(url, Environment.GetEnvironmentVariable("TEMP") + "\\javatmp.exe");

            while (wc.IsBusy)
            {
                Thread.Sleep(100);
            }

            var filename = targetDirectory + "\\jre-" + FileVersionInfo.GetVersionInfo(Environment.GetEnvironmentVariable("TEMP") + "\\javatmp.exe").OriginalFilename.Split('-')[1] + "-windows-" + ((arch == Arch.AMD64) ? "x64" : "i586") + ".exe";

            if (System.IO.File.Exists(filename))
            {
                System.IO.File.Delete(filename);
            }

            System.IO.File.Move(Environment.GetEnvironmentVariable("TEMP") + "\\javatmp.exe", filename);

            OnDownloadCompleted(filename);
        }
    }

    public enum Arch
    {
        I586, AMD64
    }
}
