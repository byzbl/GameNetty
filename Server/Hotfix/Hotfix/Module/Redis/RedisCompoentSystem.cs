using StackExchange.Redis;

namespace ET.Server
{
    public class RedisComponentAwakeSystem: AwakeSystem<RedisComponent, string>
    {
        protected override void Awake(RedisComponent self, string strCon)
        {
            string password = null;

            string[] strings = strCon.Split('@');
            if (strings.Length == 2)
            {
                password = strings[1];
            }
            else if (strings.Length != 1)
            {
                Log.Error("redic config error");
                return;
            }

            string[] strings1 = strings[0].Split(':');
            if (strings.Length != 2)
            {
                Log.Error("redic config error");
                return;
            }

            string host = strings1[0];
            int port = int.Parse(strings1[1]);

            ConnectionMultiplexer _conn = ConnectionMultiplexer.Connect($"{host}:{port},defaultDatabase={0},password={password}");
            self.Con = _conn;
            self.Database = self.Con.GetDatabase(0);

            Log.Info($"====Redis连接:ping===== {self.Database.Ping()}");
        }
    }

    public static class RedisCompoentSystem
    {
        public class BehaviorLogComponentDestroySystem: DestroySystem<RedisComponent>
        {
            protected override void Destroy(RedisComponent self)
            {
                self.Dispose();
            }
        }
    }
}