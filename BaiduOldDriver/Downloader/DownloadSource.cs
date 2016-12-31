using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Downloader
{
    public class DownloadSource
    {
        private FileTask OfTask;
        public string URL;
        public int FailCount;
        private long Downloaded;
        private WebDownload wc;
        private bool Closed;
        private Block CurrentBlock;
        public DownloadSource(FileTask OfTask, string URL)
        {
            this.OfTask = OfTask;
            this.URL = URL;
            wc = new WebDownload();
            wc.DownloadProgressChanged += (s, e) =>
            {
                lock (this)
                    Downloaded = e.BytesReceived;
            };
            wc.DownloadDataCompleted += Wc_DownloadDataCompleted;
            Wc_DownloadDataCompleted(null, null);
        }
        private void Wc_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            lock (this)
            {
                if (Closed)
                {
                    wc.Dispose();
                    return;
                }
                if (e != null && !e.Cancelled)
                {
                    if (e.Error == null)
                    {
                        Downloaded = 0;
                        OfTask.ReportSuccess(CurrentBlock, this, e.Result);
                    }
                    else
                    {
                        OfTask.ReportFailure(CurrentBlock, this);
                        FailCount++;
                        if (FailCount >= 10)
                        {
                            Close();
                            return;
                        }
                    }
                }
                CurrentBlock = OfTask.GetBlock(this);
                if (CurrentBlock == null)
                {
                    Abort();
                    return;
                }
                wc.from = CurrentBlock.Start;
                wc.to = CurrentBlock.Start + CurrentBlock.Length - 1;
                wc.DownloadDataAsync(new Uri(URL));
            }
        }
        public void Abort()
        {
            try
            {
                wc.CancelAsync();
            }
            catch (Exception) { }
        }
        public void Close()
        {
            lock (this)
                Closed = true;
            Abort();
        }
        public bool IsClosed()
        {
            lock (this)
                return Closed;
        }
        public long GetDownloadedBytes()
        {
            lock (this)
                return Downloaded;
        }
    }
    public class WebDownload : WebClient
    {
        public long from, to;
        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = (HttpWebRequest)WebRequest.Create(address);
            request.AddRange(from, to);
            return request;
        }
    }
}
