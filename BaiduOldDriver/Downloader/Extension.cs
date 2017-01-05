using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace Downloader
{
    public static class Extension
    {

        #region HttpWebRequest.AddRange(long)
        static MethodInfo httpWebRequestAddRangeHelper = typeof(WebHeaderCollection).GetMethod
                                                ("AddWithoutValidate", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// Adds a byte range header to a request for a specific range from the beginning or end of the requested data.
        /// </summary>
        /// <param name="request">The <see cref="System.Web.HttpWebRequest"/> to add the range specifier to.</param>
        /// <param name="start">The starting or ending point of the range.</param>
        public static void AddRange(this HttpWebRequest request, long start) { request.AddRange(start, -1L); }

        /// <summary>Adds a byte range header to the request for a specified range.</summary>
        /// <param name="request">The <see cref="System.Web.HttpWebRequest"/> to add the range specifier to.</param>
        /// <param name="start">The position at which to start sending data.</param>
        /// <param name="end">The position at which to stop sending data.</param>
        public static void AddRange(this HttpWebRequest request, long start, long end)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (start < 0) throw new ArgumentOutOfRangeException("start", "Starting byte cannot be less than 0.");
            if (end < start) end = -1;

            string key = "Range";
            string val = string.Format("bytes={0}-{1}", start, end == -1 ? "" : end.ToString());

            httpWebRequestAddRangeHelper.Invoke(request.Headers, new object[] { key, val });
        }
        #endregion
    }
}
