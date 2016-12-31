using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downloader
{
    public class Block
    {
        public FileTask OfTask;
        public List<DownloadSource> WorkingSource = new List<DownloadSource>();
        public long Start;
        public long Length;
        public bool Completed;
    }
}
