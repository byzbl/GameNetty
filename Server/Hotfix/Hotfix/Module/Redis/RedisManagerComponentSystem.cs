using System;
using System.IO;

namespace ET.Server
{
    public static class RedisManagerComponentSystem
    {
        public class RedisManagerComponentSystemAwakeSystem: AwakeSystem<RedisManagerComponent>
        {
            protected override void Awake(RedisManagerComponent self)
            {
                foreach (ERedisType dbType in Enum.GetValues(typeof (ERedisType)))
                {
                    self.DicRedisComponents[dbType] = new RedisComponent[IdGenerater.MaxZone];
                }
            }
        }

        public static RedisComponent GetZoneRedis(this RedisManagerComponent self, int zone, ERedisType reidsType = ERedisType.Default)
        {
            RedisComponent redisComponent = self.DicRedisComponents[reidsType][zone];
            if (redisComponent != null)
            {
                return redisComponent;
            }

            StartZoneConfig startZoneConfig = StartZoneConfigCategory.Instance.Get(zone);
            string strCon = startZoneConfig.RedisConnection;
            /*
            if (strCon == "")
            {
                if (startZoneConfig.RedisConnectionFile != "")
                {
                    string s = File.ReadAllText(startZoneConfig.RedisConnectionFile);
                    strCon = AesServerHelper.AESDecrypt(s, AesServerHelper.MYSQL_AES_KEY, AesServerHelper.MYSQL_AES_IV);
                }
            }
            */

            if (strCon == "")
            {
                Log.Error("redis config error");
                return null;
            }

        

            RedisComponent component = self.AddChild<RedisComponent, string>(strCon);
            self.DicRedisComponents[reidsType][zone] = component;
            return component;
        }

        public static void ReConnect(this RedisManagerComponent self, int zone, ERedisType reidsType = ERedisType.Default)
        {
            RedisComponent redisComponent = self.DicRedisComponents[reidsType][zone];
            redisComponent?.Dispose();
            self.DicRedisComponents[reidsType][zone] = null;
            self.GetZoneRedis(zone, reidsType);
        }
    }
}