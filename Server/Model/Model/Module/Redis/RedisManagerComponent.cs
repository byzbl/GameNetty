using System.Collections.Generic;

namespace ET.Server
{
    public enum ERedisType
    {
        Default = 1,
    }

    [ComponentOf(typeof (Scene))]
    public class RedisManagerComponent: Entity, IAwake
    {
        // redisType RedisComponent
        public Dictionary<ERedisType, RedisComponent[]> DicRedisComponents { get; set; } = new Dictionary<ERedisType, RedisComponent[]>();
    }
}