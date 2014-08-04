using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace DictManiac.Sources
{
    enum DownloadTaskState
    {
        Idle,
        ReadingUrls,
        Downloading,
        End
    }

    delegate string WordToUrl(string word);

    class DownloadTask
    {
        private DownloadTaskState state;
        private List<string> urls;
        private string[] downloadedContents;
        private Thread thread;
        private bool abortSignal;
        private string urlsFile;
        private string writePath;
        private WordToUrl wordToUrl;

        public DownloadTask()
        {
            state = DownloadTaskState.Idle;
            urls = new List<string>();
            downloadedContents = null;
            thread = null;
            abortSignal = false;
            urlsFile = "frequency.txt";
            writePath = "IcibaHtmls\\";
            wordToUrl = GetRealUrl;
        }

        public void OptionOutputPath(string newOutputPath)
        {
            writePath = newOutputPath;
        }

        public void OptionUrlsFile(string newUrlsFile)
        {
            urlsFile = newUrlsFile;
        }

        public void OptionWordToUrl(WordToUrl newWordToUrl)
        {
            wordToUrl = newWordToUrl;
        }

        public DownloadTaskState GetState()
        {
            return state;
        }

        public void Start()
        {
            if (state == DownloadTaskState.Idle)
            {
                ChangeState(DownloadTaskState.ReadingUrls);
                //MyLogger.Attach("DownloadingTask: thread starts");
                thread = new Thread(() => ThreadStart());
                thread.Start();
            }
            else
            {
                Log(string.Format("Current state is {0}, unable to start.", state.ToString()));
            }
        }

        private void ChangeState(DownloadTaskState newState)
        {
            state = newState;
            Log("change state, new state is: " + newState);
        }

        private void ThreadStart()
        {
            Log("DownloadingTask: thread starts");
            ReadUrls();
            if (state != DownloadTaskState.End)
            {
                Download();
            }
            Log("Thread DownloadTask end.");
        }


        private void ReadUrls()
        {
            Log("Begin reading urls, source file = " + urlsFile);
            string content;
            if (FileUtil.ReadFile(urlsFile, out content))
            {
                foreach (var line in content.Split('\n'))
                {
                    string word = line.Trim('\r', ' ', '\t');
                    if (!string.IsNullOrEmpty(word))
                        urls.Add(word);
                }
            }
            else
            {
                Log("Fail to read urls file: " + urlsFile);
                Log("Terminating download task..");
                ChangeState(DownloadTaskState.End);
            }
        }

        private string Sanitize(string possiblyDangerousFileName)
        {
            return possiblyDangerousFileName.Replace('\'', '_');
        }

        private string GetRealUrl(string word)
        {
            return "http://www.iciba.com/" + word;
        }

        private void Download()
        {
            ChangeState(DownloadTaskState.Downloading);
            Log("files to download: " + urls.Count);
            if (!string.IsNullOrEmpty(writePath) && !writePath.EndsWith("\\"))
                writePath += "\\";
            Log("Output path: " + writePath);
            FileUtil.EnsureDirectory(writePath);
            int successCount = 0;
            if (urls.Count != 0)
            {
                downloadedContents = new string[urls.Count];
                for (int i = 0; i < urls.Count; i++)
                {
                    string thisUrl = wordToUrl(urls[i]);
                    string outPath = writePath + Sanitize(urls[i]) + ".html";
                    byte[] content;
                    if (FileUtil.FileExists(outPath))
                    {
                        Log(i + "," + urls[i] + ": already exists");
                    }
                    else if (WebUtil.DownloadBytesSynchronous(thisUrl, out content))
                    {
                        //downloadedContents[i] = content;
                        if (FileUtil.WriteFile(outPath, content))
                        {
                            Log(i + "," + urls[i] + ": OK");
                            successCount++;
                        }
                        else
                        {
                            Log(i + ",Fail to write file: " + outPath);
                        }
                    }
                    else
                    {
                        Log(i + ",Fail to download:" + thisUrl);
                        downloadedContents[i] = null;
                    }
                }
            }

            Log(string.Format("{0} of {1} files are downloaded.", successCount, urls.Count));
        }

        private void Log(string msg)
        {
            MyLogger.Attach(msg);
        }
    }
}
