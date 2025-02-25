using System;
using System.Collections.Generic;
using System.Threading;
using StackExchange.Redis;

namespace ET.Server
{
    public static class RedisHelper
    {
        public static IDatabase GetClient(Scene entity, int zone)
        {
            RedisComponent component = entity.Root().GetComponent<RedisManagerComponent>().GetZoneRedis(zone);
            return component.Database;
        }

        public static async ETTask<bool> HashAddAsync(Scene scene, int zone, string key, string id, string value)
        {
            try
            {
                return await GetClient(scene, zone).HashSetAsync(key, id, value);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Warning("Redis Trigger ReConnect");
                scene.Root().GetComponent<RedisManagerComponent>().ReConnect(zone);
                return await GetClient(scene, zone).HashSetAsync(key, id, value);
            }
        }

        public static async ETTask<string> HashGetAsync(Scene scene, int zone, string key, string id)
        {
            try
            {
                return await GetClient(scene, zone).HashGetAsync(key, id);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Warning("Redis Trigger ReConnect");
                scene.Root().GetComponent<RedisManagerComponent>().ReConnect(zone);
                return await GetClient(scene, zone).HashGetAsync(key, id);
            }
        }

        public static string HashGet(Scene scene, int zone, string key, string id)
        {
            try
            {
                return GetClient(scene, zone).HashGet(key, id);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Warning("Redis Trigger ReConnect");
                scene.Root().GetComponent<RedisManagerComponent>().ReConnect(zone);
                return GetClient(scene, zone).HashGet(key, id);
            }
        }

        public static bool HashAdd(Scene scene, int zone, string key, string id, string value)
        {
            try
            {
                return GetClient(scene, zone).HashSet(key, id, value);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Warning("Redis Trigger ReConnect");
                scene.Root().GetComponent<RedisManagerComponent>().ReConnect(zone);
                return GetClient(scene, zone).HashSet(key, id, value);
            }
        }

        public static async ETTask<bool> ZSetAdd(Scene scene, int zone, string key, long unitId, long score)
        {
            try
            {
                return await GetClient(scene, zone).SortedSetAddAsync(key, unitId, score);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Warning("Redis Trigger ReConnect");
                scene.Root().GetComponent<RedisManagerComponent>().ReConnect(zone);
                return await GetClient(scene, zone).SortedSetAddAsync(key, unitId, score);
            }
        }

        public static async ETTask<bool> SetAdd(Scene scene, int zone, string key, long unitId)
        {
            try
            {
                return await GetClient(scene, zone).SetAddAsync(key, unitId);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Warning("Redis Trigger ReConnect");
                scene.Root().GetComponent<RedisManagerComponent>().ReConnect(zone);
                return await GetClient(scene, zone).SetAddAsync(key, unitId);
            }
        }

        private static async ETTask<bool> StringSetAsync(Scene scene, int zone, string key, string value, TimeSpan t)
        {
            try
            {
                return await GetClient(scene, zone).StringSetAsync(key, value, t);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Warning("Redis Trigger ReConnect");
                scene.Root().GetComponent<RedisManagerComponent>().ReConnect(zone);
                return await GetClient(scene, zone).StringSetAsync(key, value, t);
            }
        }

        public static async ETTask<bool> SetString(Scene scene, int zone, string key, string value, TimeSpan t)
        {
            return await StringSetAsync(scene, zone, key, value, t);
        }

        private static async ETTask<bool> StringSetAsync(Scene scene, int zone, string key, string value)
        {
            try
            {
                return await GetClient(scene, zone).StringSetAsync(key, value);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Warning("Redis Trigger ReConnect");
                scene.Root().GetComponent<RedisManagerComponent>().ReConnect(zone);
                return await GetClient(scene, zone).StringSetAsync(key, value);
            }
        }

        private static async ETTask<string> StringGetAsync(Scene scene, int zone, string key)
        {
            try
            {
                return await GetClient(scene, zone).StringGetAsync(key);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Warning("Redis Trigger ReConnect");
                scene.Root().GetComponent<RedisManagerComponent>().ReConnect(zone);
                return await GetClient(scene, zone).StringGetAsync(key);
            }
        }

        public static async ETTask<string> GetString(Scene scene, int zone, string key)
        {
            return await StringGetAsync(scene, zone, key);
        }

        private static async ETTask KeyDeleteAsync(Scene scene, int zone, string key)
        {
            try
            {
                await GetClient(scene, zone).KeyDeleteAsync(key);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Warning("Redis Trigger ReConnect");
                scene.Root().GetComponent<RedisManagerComponent>().ReConnect(zone);
                await GetClient(scene, zone).KeyDeleteAsync(key);
            }
        }

        public static async ETTask DeleteKey(Scene scene, int zone, string key)
        {
            await KeyDeleteAsync(scene, zone, key);
        }

        public static async ETTask<bool> SetAccountToken(Scene scene, int zone, long accountId, string token)
        {
            return await StringSetAsync(scene, zone, $"Account:{accountId}", token, TimeSpan.FromDays(2));
        }

        public static async ETTask<string> GetAccountToken(Scene scene, int zone, long accountId)
        {
            return await StringGetAsync(scene, zone, $"Account:{accountId}");
        }

        public static async ETTask DelAccountToken(Scene scene, int zone, long accountId)
        {
            await KeyDeleteAsync(scene, zone, $"Account:{accountId}");
        }

        public static async ETTask<bool> LockTakeAccount(Scene scene, int zone, string account)
        {
            return await GetClient(scene, zone).LockTakeAsync($"Lock:Acount:{account}", account, TimeSpan.FromSeconds(15));
        }

        public static async ETTask<bool> LockReleaseAccount(Scene scene, int zone, string account)
        {
            return await GetClient(scene, zone).LockReleaseAsync($"Lock:Acount:{account}", account);
        }

        public static async ETTask<bool> LockAsync(Scene scene, int zone, string key, TimeSpan t)
        {
            for (int i = 0; i < 50; i++)
            {
                if (await GetClient(scene, zone).LockTakeAsync(key, key, t))
                {
                    return true;
                }

                await scene.Fiber().Root.GetComponent<TimerComponent>().WaitAsync(100);
            }

            return false;
        }

        /// <summary>
        /// 同步上锁 会卡死进程 慎用!!!!!!!!!!
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="zone"></param>
        /// <param name="key"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool LockSync(Scene scene, int zone, string key, TimeSpan t)
        {
            for (int i = 0; i < 50; i++)
            {
                if (GetClient(scene, zone).LockTake(key, key, t))
                {
                    return true;
                }

                Thread.Sleep(100);
            }

            return false;
        }

        public static bool UnLockSync(Scene scene, int zone, string key)
        {
            return GetClient(scene, zone).LockRelease(key, key);
        }

        public static async ETTask<bool> UnLock(Scene scene, int zone, string key)
        {
            return await GetClient(scene, zone).LockReleaseAsync(key, key);
        }

        public static async ETTask<bool> TryLock(Scene scene, int zone, List<string> lst, TimeSpan t)
        {
            lst.Sort();
            foreach (string key in lst)
            {
                if (!await LockAsync(scene, zone, key, t))
                {
                    return false;
                }
            }

            return true;
        }

        public static async ETTask UnLock(Scene scene, int zone, List<string> lst)
        {
            foreach (string key in lst)
            {
                await GetClient(scene, zone).LockReleaseAsync(key, key);
            }
        }

        /// <summary>
        /// 获取当前gateId
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public static async ETTask<string> GetCurGateId(Scene scene, int zone, long accountId)
        {
            return await StringGetAsync(scene, zone, $"Gate:{accountId}");
        }

        /// <summary>
        /// 设置当前的gateId
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="gateId"></param>
        /// <returns></returns>
        public static async ETTask<bool> SetCurGateId(Scene scene, int zone, long accountId, long gateId)
        {
            return await StringSetAsync(scene, zone, $"Gate:{accountId}", gateId.ToString(), TimeSpan.FromDays(14));
        }

        public static async ETTask<bool> DelCurGateId(Scene scene, int zone, long accountId, long gateId)
        {
            var curId = await GetCurGateId(scene, zone, accountId);
            if (string.IsNullOrEmpty(curId))
            {
                return true;
            }

            if (long.Parse(curId) == gateId)
            {
                await KeyDeleteAsync(scene, zone, $"Gate:{accountId}");
                return true;
            }

            return false;
        }

        public static async ETTask<long> StringIncrementAsync(Scene scene, int zone, string key)
        {
            try
            {
                return await GetClient(scene, zone).StringIncrementAsync(key);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Warning("Redis Trigger ReConnect");
                scene.Root().GetComponent<RedisManagerComponent>().ReConnect(zone);
                return await GetClient(scene, zone).StringIncrementAsync(key);
            }
        }

        /// <summary>
        /// 获取参赛号牌
        /// </summary>
        /// <returns></returns>
        public static async ETTask<long> GetMinerRankEntryNumer(Scene scene, int zone, int activityId)
        {
            long num = await StringIncrementAsync(scene, zone, $"MinerRankEntryNumer:{activityId}");
            return num;
        }

        /// <summary>
        /// 获取参赛号牌
        /// </summary>
        /// <returns></returns>
        public static async ETTask<long> GetMiniGameRankEntryNumer(Scene scene, int zone, int activityId)
        {
            long num = await StringIncrementAsync(scene, zone, $"MiniGameRankEntryNumer:{activityId}");
            return num;
        }

        public static async ETTask<long> LoginRateLimiterNum(Scene scene, int zone, long timeNow)
        {
            string key = $"LoginRateLimiter:{timeNow}";
            long num = await StringIncrementAsync(scene, zone, key);
            await GetClient(scene, zone).KeyExpireAsync(key, TimeSpan.FromSeconds(5));
            return num;
        }
    }
}