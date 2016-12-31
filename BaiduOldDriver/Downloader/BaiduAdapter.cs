using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetDisk;

namespace Downloader
{
    public class BaiduAdapter : Adapter
    {
        public Credential credential;
        public string path;
        public long size;
        public override List<string> GetURLList()
        {
            return Operation.GetDownload(path, credential).urls.Select(e => e.url).Distinct().ToList();
        }
        public override List<Block> GetBlockList(FileTask OfTask)
        {
            var ret = new List<Block>();
            long pos = 0;
            while(pos < size)
            {
                var length = Math.Min(size - pos, 1024 * 1024);
                var block = new Block();
                block.OfTask = OfTask;
                block.Start = pos;
                block.Length = length;
                ret.Add(block);
                pos += length;
            }
            return ret;
        }
        public override long GetSize()
        {
            return size;
        }
    }
}
