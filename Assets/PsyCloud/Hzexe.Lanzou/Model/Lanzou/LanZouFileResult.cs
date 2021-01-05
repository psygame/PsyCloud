using System;
using System.Collections.Generic;
using System.Text;

namespace Hzexe.Lanzou.Model.Lanzou
{

    [Serializable]
    public class ResponseBase
    {
        public int zt;
    }

    [Serializable]
    public class LanZouFileResult : ResponseBase
    {
        public string info { get; set; }
        public LanZouFileResultInfo[] text { get; set; }

    }

    [Serializable]
    public class LanZouFileResultInfo
    {
        public string icon { get; set; }
        public string id { get; set; }
        public string f_id { get; set; }
        public string name_all { get; set; }
        public string name { get; set; }
        public string size { get; set; }
        public string time { get; set; }
        public string downs { get; set; }
        public string onof { get; set; }
        public string is_newd { get; set; }

    }

    [Serializable]
    public class LanZouResult : ResponseBase
    {
        public LanZouInfo info { get; set; }
        public string text { get; set; }

    }

    [Serializable]
    public class LanZouInfo
    {
        public string pwd { get; set; }
        public string onof { get; set; }
        public string f_id { get; set; }
        public string taoc { get; set; }
        public string is_newd { get; set; }
    }

    [Serializable]
    public class LanZouPwd : ResponseBase
    {
        public string info { get; set; }
        public string text { get; set; }
    }
}
