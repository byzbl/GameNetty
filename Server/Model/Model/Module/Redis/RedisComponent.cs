using StackExchange.Redis;

namespace ET.Server
{
    [ChildOf(typeof (RedisManagerComponent))]
    public class RedisComponent: Entity, IAwake<string>, IDestroy
    {
        public ConnectionMultiplexer Con { get; set; }

        public IDatabase Database { get; set; }
    }
}