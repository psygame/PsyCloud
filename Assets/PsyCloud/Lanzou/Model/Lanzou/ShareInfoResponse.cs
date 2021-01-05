using System;

namespace PsyCloud
{
    [Serializable]
    public class ShareInfoResponse : ResponseBase
    {
        public ShareInfo info { get; set; }
        public string text { get; set; }
    }

    [Serializable]
    public class ShareInfo
    {
        public string pwd { get; set; }
        public string onof { get; set; }
        public string f_id { get; set; }
        public string taoc { get; set; }
        public string is_newd { get; set; }

        public string url => is_newd + "/" + f_id;
    }
}
