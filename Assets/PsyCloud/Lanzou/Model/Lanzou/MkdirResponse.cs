using System;

namespace PsyCloud
{
    [Serializable]
    public class MkdirResponse : ResponseBase
    {
        /// <summary>
        /// 创建成功
        /// </summary>
        public string info { get; set; }
        /// <summary>
        /// ID
        /// </summary>
        public string text { get; set; }
    }
}
