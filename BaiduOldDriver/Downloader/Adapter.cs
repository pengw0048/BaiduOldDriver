using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downloader
{
    public abstract class Adapter
    {
        public abstract List<string> GetURLList();
        public abstract List<Block> GetBlockList(FileTask OfTask);
        public abstract long GetSize();
    }
}
