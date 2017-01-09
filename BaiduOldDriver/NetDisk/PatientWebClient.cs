using System;
using System.Net;

class PatientWebClient : WebClient
{
    public int Timeout = 300000;
    public PatientWebClient(int Timeout): base()
    {
        this.Timeout = Timeout;
    }
    public PatientWebClient() : base()
    {

    }
    protected override WebRequest GetWebRequest(Uri uri)
    {
        WebRequest w = base.GetWebRequest(uri);
        w.Timeout = Timeout;
        (w as HttpWebRequest).ReadWriteTimeout = Timeout;
        return w;
    }
}
