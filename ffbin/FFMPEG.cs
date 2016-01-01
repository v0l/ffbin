using HtmlAgilityPack;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;

namespace ffbin
{
    public class FFMPEG
    {
        private readonly string UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36 FFBIN/1.0";
        private readonly string Url = "http://ffmpeg.zeranoe.com/builds/";
        private readonly string H2Contains = "Latest Zeranoe FFmpeg Build Version";
        private readonly string BinDir = "C:\\ffbin";

        private string VersionFile
        {
            get
            {
                return string.Format("{0}\\VERSION.json", BinDir);
            }
        }

        private void CheckFolder()
        {
            if (!Directory.Exists(BinDir))
            {
                Directory.CreateDirectory(BinDir);
            }
        }

        private void CheckPath()
        {
            string path = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User);
            if (!path.Contains(BinDir))
            {
                Environment.SetEnvironmentVariable("Path", path.EndsWith(";") ? path + BinDir : path + ';' + BinDir, EnvironmentVariableTarget.User);
            }
        }

        private void DownloadStatic(FFMPEGVersion v)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(v.DownloadUrl);
                req.UserAgent = UserAgent;

                string tmp = string.Format("{0}\\ffmpeg-{1}.7z", Path.GetTempPath(), v.GitVersion);

                using (FileStream fs = new FileStream(tmp, FileMode.Create, FileAccess.ReadWrite))
                {
                    req.GetResponse().GetResponseStream().CopyTo(fs);
                }

                ProcessStartInfo pi = new ProcessStartInfo(Get7ZipPath());
                pi.Arguments = GetCommandLine(v, tmp);
                pi.UseShellExecute = false;

                Process p = Process.Start(pi);
                p.WaitForExit();
            }
            catch { }
        }

        private string Get7ZipPath()
        {
            string ret = null;

            try
            {
                RegistryKey r = Registry.CurrentUser;
                RegistryKey k = r.OpenSubKey("SOFTWARE\\7-Zip");
                
                if (k != null)
                {
                    ret = string.Format("{0}7z.exe", (string)k.GetValue("Path"));

                    if (!File.Exists(ret))
                    {
                        ret = null;
                    }
                }
            }
            catch { }

            return ret;
        }

        private string GetCommandLine(FFMPEGVersion v, string file)
        {
            return string.Format("e -y -o{0} \"{1}\" \"{2}\\bin\\ffmpeg.exe\" \"{2}\\bin\\ffplay.exe\" \"{2}\\bin\\ffprobe.exe\"", BinDir, file, v.VersionString);
        }

        private FFMPEGVersion GetLatestVersion()
        {
            FFMPEGVersion ret = null;

            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
                req.UserAgent = UserAgent;

                HtmlDocument doc = new HtmlDocument();
                doc.Load(req.GetResponse().GetResponseStream());

                foreach(HtmlNode n in doc.DocumentNode.SelectNodes("//h2"))
                {
                    if (n.InnerHtml.Contains(H2Contains))
                    {
                        string v = n.InnerHtml.Split(':')[1].Replace("(", string.Empty).Replace(")", string.Empty).Trim();
                        string[] vs = v.Split(' ');

                        ret = new FFMPEGVersion
                        {
                            GitVersion = vs[0],
                            ReleaseDate = DateTime.ParseExact(vs[1], "yyyy-MM-dd", CultureInfo.InvariantCulture)
                        };
                    }
                }
            }
            catch { }

            return ret;
        }

        private FFMPEGVersion GetCurrentVersion()
        {
            FFMPEGVersion ret = null;

            try
            {
                if (File.Exists(VersionFile))
                {
                    using (StreamReader r = new StreamReader(new FileStream(VersionFile, FileMode.Open, FileAccess.Read)))
                    {
                        ret = JsonConvert.DeserializeObject<FFMPEGVersion>(r.ReadToEnd());
                    }
                }
                else
                {
                    ret = new FFMPEGVersion();

                    using (StreamWriter r = new StreamWriter(new FileStream(VersionFile, FileMode.Create, FileAccess.ReadWrite)))
                    {
                        r.Write(JsonConvert.SerializeObject(ret));
                    }
                }
            }
            catch { }

            return ret;
        }

        private void UpdateCurrentVersion(FFMPEGVersion v)
        {
            try
            {
                using (StreamWriter r = new StreamWriter(new FileStream(VersionFile, FileMode.Create, FileAccess.ReadWrite)))
                {
                    r.Write(JsonConvert.SerializeObject(v));
                }
            }
            catch { }
        }

        public void Update()
        {
            FFMPEGVersion current = GetCurrentVersion();
            FFMPEGVersion latest = GetLatestVersion();

            if (current != null && latest != null)
            {
                CheckPath();
                CheckFolder();

                if (latest.ReleaseDate > current.ReleaseDate)
                {
                    DownloadStatic(latest);
                    UpdateCurrentVersion(latest);
                }
            }
        }
    }

    class FFMPEGVersion
    {
        public string GitVersion { get; set; }
        public DateTime ReleaseDate { get; set; }

        public string VersionString
        {
            get
            {
                return string.Format("ffmpeg-{1}-{2}-{0}-static",
                    Environment.Is64BitOperatingSystem ? "win64" : "win32",
                    ReleaseDate.ToString("yyyyMMdd"),
                    GitVersion);
            }
        }

        public string DownloadUrl
        {
            get
            {
                return string.Format("http://ffmpeg.zeranoe.com/builds/{0}/static/{1}.7z",
                    Environment.Is64BitOperatingSystem ? "win64" : "win32",
                    VersionString);
            }
        }
    }
}
