using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Reflection;

namespace NetDisk
{
    public static class Extension
    {
        public static CookieCollection GetAllCookies(this CookieContainer container)
        {
            var allCookies = new CookieCollection();
            var domainTableField = container.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(x => x.Name == "m_domainTable");
            var domains = (IDictionary)domainTableField.GetValue(container);

            foreach (var val in domains.Values)
            {
                var type = val.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).First(x => x.Name == "m_list");
                var values = (IDictionary)type.GetValue(val);
                foreach (CookieCollection cookies in values.Values)
                {
                    allCookies.Add(cookies);
                }
            }
            return allCookies;
        }
        public static CookieContainer ClearVersion(this CookieContainer container)
        {
            var domainTableField = container.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(x => x.Name == "m_domainTable");
            var domains = (IDictionary)domainTableField.GetValue(container);

            foreach (var val in domains.Values)
            {
                var type = val.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).First(x => x.Name == "m_list");
                var values = (IDictionary)type.GetValue(val);
                foreach (CookieCollection cookies in values.Values)
                {
                    foreach(Cookie cookie in cookies)
                    {
                        cookie.Version = 0;
                    }
                }
            }
            return container;
        }

        private const int UnEscapeDotsAndSlashes = 0x2000000;
        private const int SimpleUserSyntax = 0x20000;

        public static void LeaveDotsAndSlashesEscaped(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            FieldInfo fieldInfo = uri.GetType().GetField("m_Syntax", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo == null)
                throw new MissingFieldException("'m_Syntax' field not found");

            object uriParser = fieldInfo.GetValue(uri);
            fieldInfo = typeof(UriParser).GetField("m_Flags", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo == null)
                throw new MissingFieldException("'m_Flags' field not found");

            object uriSyntaxFlags = fieldInfo.GetValue(uriParser);

            // Clear the flag that we don't want
            uriSyntaxFlags = (int)uriSyntaxFlags & ~UnEscapeDotsAndSlashes;
            uriSyntaxFlags = (int)uriSyntaxFlags & ~SimpleUserSyntax;
            fieldInfo.SetValue(uriParser, uriSyntaxFlags);
        }

        public static Uri SafeUri(string str)
        {
            var uri = new Uri(str);
            LeaveDotsAndSlashesEscaped(uri);
            return uri;
        }

    }
}
