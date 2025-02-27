using System.Collections.Generic;

namespace ET.Server
{
#pragma warning disable ET0032
    public  class BehaviorLog
#pragma warning restore ET0032
    {
        public string TableName { get; set; }
        public Dictionary<string, object> Fileds = new Dictionary<string, object>();
    }
}

