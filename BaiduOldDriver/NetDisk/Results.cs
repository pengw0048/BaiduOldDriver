using System;
using System.Runtime.Serialization;

namespace NetDisk
{
    [DataContract]
    public class QuotaResult
    {
        [DataMember]
        public int errno;
        [DataMember]
        public long total;
        [DataMember]
        public long free;
        [DataMember]
        public bool expire;
        [DataMember]
        public long used;
        public Exception ex;
    }
}
