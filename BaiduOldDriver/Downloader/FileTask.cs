using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;


namespace Downloader
{
    public class FileTask
    {
        public string SavePath;
        private long Downloaded;
        public List<DownloadSource> DownloadSources = new List<DownloadSource>();
        public List<Block> Blocks = new List<Block>();
        public LinkedList<Block> PendingBlocks = new LinkedList<Block>();
        public LinkedList<Block> WorkingBlocks = new LinkedList<Block>();
        private FileStream LocalFile;
        private Adapter OfAdapter;
        private Thread RefreshThread;
        private int ReSourceCount;
        private int DownloadSourcesCountAll;
        public bool Failed;
        public FileTask(Adapter OfAdapter, string SavePath)
        {
            this.OfAdapter = OfAdapter;
            Blocks = OfAdapter.GetBlockList(this);
            Blocks.ForEach(b => PendingBlocks.AddLast(b));
            LocalFile = File.Open(SavePath, FileMode.Create);
            LocalFile.SetLength(OfAdapter.GetSize());
            RefreshThread = new Thread(new ThreadStart(Refresh));
            RefreshThread.Start();
        }
        public void Abort()
        {
            lock (this)
            {
                RefreshThread.Abort();
                DownloadSources.ForEach(s => s.Close());
                Failed = true;
                LocalFile.Dispose();
            }
        }
        private void Refresh()
        {
            while (true)
            {
                if (IsCompleted())
                {
                    LocalFile.Close();
                    LocalFile = null;
                    break;
                }
                DownloadSources.RemoveAll(s => s.IsClosed());
                var needReSource = false;
                if (DownloadSources.Count * 2 <= DownloadSourcesCountAll)
                {
                    ReSourceCount++;
                    needReSource = true;
                }
                if (needReSource)
                {
                    if (ReSourceCount > 100)
                    {
                        Failed = true;
                        return;
                    }
                    DownloadSources = OfAdapter.GetURLList().Select(u => new DownloadSource(this, u)).ToList();
                    DownloadSourcesCountAll = DownloadSources.Count;
                }
                try
                {
                    Thread.Sleep(1000);
                }
                catch (Exception) { }
            }
        }
        public bool IsCompleted()
        {
            lock(this)
                return PendingBlocks.Count == 0;
        }
        public Block GetBlock(DownloadSource Source)
        {
            Block ret = null;
            lock (this)
            {
                if (PendingBlocks.Count > 0)
                {
                    ret = PendingBlocks.First.Value;
                    PendingBlocks.RemoveFirst();
                    WorkingBlocks.AddLast(ret);
                }
                else if (WorkingBlocks.Count > 0)
                {
                    ret = WorkingBlocks.FirstOrDefault(b => b.WorkingSource.Count < 3 && b.WorkingSource.All(o => o.URL != Source.URL));
                }
                if (ret != null)
                {
                    ret.WorkingSource.Add(Source);
                }
            }
            return ret;
        }
        public long GetDownloadedBytes()
        {
            lock(this)
                return Downloaded + WorkingBlocks.Select(b => b.WorkingSource.Max(s => s.GetDownloadedBytes())).Sum();
        }
        public void ReportSuccess(Block OfBlock, DownloadSource Source, byte[] data)
        {
            if (OfBlock.Completed) return;
            OfBlock.Completed = true;
            if (OfBlock.WorkingSource.Count > 1)
                OfBlock.WorkingSource.ForEach(s => { if (s != Source) s.Abort(); });
            lock (this)
            {
                WorkingBlocks.Remove(OfBlock);
                Downloaded += OfBlock.Length;
                if (LocalFile != null)
                {
                    LocalFile.Seek(OfBlock.Start, SeekOrigin.Begin);
                    LocalFile.Write(data, 0, data.Length);
                    LocalFile.Flush();
                }
            }
        }
        public void ReportFailure(Block OfBlock, DownloadSource Source)
        {
            lock (this)
            {
                OfBlock.WorkingSource.Remove(Source);
                if (OfBlock.WorkingSource.Count == 0)
                {
                    WorkingBlocks.Remove(OfBlock);
                    PendingBlocks.AddFirst(OfBlock);
                }
            }
        }
    }
}
