using System;
using System.Collections.Generic;
using System.Threading;
using StackExchange.Redis;

namespace ET.Server
{
    public static class RedisHelper
    {
        public static IDatabase GetClient(Scene entity)
        {
            RedisComponent component = entity.Root().GetComponent<RedisManagerComponent>().GetZoneRedis(entity.Root().Zone());
            return component.Database;
        }

        public static async ETTask<bool> HashAddAsync(Scene scene, string key, string id, string value)
        {
            try
            {
                return await GetClient(scene).HashSetAsync(key, id, value);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Warning("Redis Trigger ReConnect");
                scene.Root().GetComponent<RedisManagerComponent>().ReConnect(scene.Zone());
                return await GetClient(scene).HashSetAsync(key, id, value);
            }
        }

        public static async ETTask<string> HashGetAsync(Scene scene, string key, string id)
        {
            try
            {
                return await GetClient(scene).HashGetAsync(key, id);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Warning("Redis Trigger ReConnect");
                scene.Root().GetComponent<RedisManagerComponent>().ReConnect(scene.Zone());
                return await GetClient(scene).HashGetAsync(key, id);
            }
        }

        public static string HashGet(Scene scene, string key, string id)
        {
            try
            {
                return GetClient(scene).HashGet(key, id);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Warning("Redis Trigger ReConnect");
                scene.Root().GetComponent<RedisManagerComponent>().ReConnect(scene.Zone());
                return GetClient(scene).HashGet(key, id);
            }
        }

        public static bool HashAdd(Scene scene, string key, string id, string value)
        {
            try
            {
                return GetClient(scene).HashSet(key, id, value);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Warning("Redis Trigger ReConnect");
                scene.Root().GetComponent<RedisManagerComponent>().ReConnect(scene.Zone());
                return GetClient(scene).HashSet(key, id, value);
            }
        }

        public static async ETTask<bool> ZSetAdd(Scene scene, string key, long unitId, long score)
        {
            try
            {
                return await GetClient(scene).SortedSetAddAsync(key, unitId, score);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Warning("Redis Trigger ReConnect");
                scene.Root().GetComponent<RedisManagerComponent>().ReConnect(scene.Zone());
                return await GetClient(scene).SortedSetAddAsync(key, unitId, score);
            }
        }

        public static async ETTask<bool> SetAdd(Scene scene, string key, long unitId)
        {
            try
            {
                return await GetClient(scene).SetAddAsync(key, unitId);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Warning("Redis Trigger ReConnect");
                scene.Root().GetComponent<RedisManagerComponent>().ReConnect(scene.Zone());
                return await GetClient(scene).SetAddAsync(key, unitId);
            }
        }

        private static async ETTask<bool> StringSetAsync(Scene scene, string key, string value, TimeSpan t)
        {
            try
            {
                return await GetClient(scene).StringSetAsync(key, value, t);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Warning("Redis Trigger ReConnect");
                scene.Root().GetComponent<RedisManagerComponent>().ReConnect(scene.Zone());
                return await GetClient(scene).StringSetAsync(key, value, t);
            }
        }

        public static async ETTask<bool> SetString(Scene scene, string key, string value, TimeSpan t)
        {
            return await StringSetAsync(scene, key, value, t);
        }

        private static async ETTask<bool> StringSetAsync(Scene scene, string key, string value)
        {
            try
            {
                return await GetClient(scene).StringSetAsync(key, value);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Warning("Redis Trigger ReConnect");
                scene.Root().GetComponent<RedisManagerComponent>().ReConnect(scene.Zone());
                return await GetClient(scene).StringSetAsync(key, value);
            }
        }

        private static async ETTask<string> StringGetAsync(Scene scene, string key)
        {
            try
            {
                return await GetClient(scene).StringGetAsync(key);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Warning("Redis Trigger ReConnect");
                scene.Root().GetComponent<RedisManagerComponent>().ReConnect(scene.Zone());
                return await GetClient(scene).StringGetAsync(key);
            }
        }

        public static async ETTask<string> GetString(Scene scene, string key)
        {
            return await StringGetAsync(scene, key);
        }

        private static async ETTask KeyDeleteAsync(Scene scene, string key)
        {
            try
            {
                await GetClient(scene).KeyDeleteAsync(key);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Warning("Redis Trigger ReConnect");
                scene.Root().GetComponent<RedisManagerComponent>().ReConnect(scene.Zone());
                await GetClient(scene).KeyDeleteAsync(key);
            }
        }

        public static async ETTask DeleteKey(Scene scene, string key)
        {
            await KeyDeleteAsync(scene, key);
        }

        public static async ETTask<bool> SetAccountToken(Scene scene, long accountId, string token)
        {
            return await StringSetAsync(scene, $"Account:{accountId}", token, TimeSpan.FromDays(2));
        }

        public static async ETTask<string> GetAccountToken(Scene scene, long accountId)
        {
            return await StringGetAsync(scene, $"Account:{accountId}");
        }

        public static async ETTask DelAccountToken(Scene scene, long accountId)
        {
            await KeyDeleteAsync(scene, $"Account:{accountId}");
        }

        public static async ETTask<bool> LockTakeAccount(Scene scene, string account)
        {
            return await GetClient(scene).LockTakeAsync($"Lock:Acount:{account}", account, TimeSpan.FromSeconds(15));
        }

        public static async ETTask<bool> LockReleaseAccount(Scene scene, string account)
        {
            return await GetClient(scene).LockReleaseAsync($"Lock:Acount:{account}", account);
        }

        public static async ETTask<bool> LockAsync(Scene scene, string key, TimeSpan t)
        {
            for (int i = 0; i < 50; i++)
            {
                if (await GetClient(scene).LockTakeAsync(key, key, t))
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
        public static bool LockSync(Scene scene, string key, TimeSpan t)
        {
            for (int i = 0; i < 50; i++)
            {
                if (GetClient(scene).LockTake(key, key, t))
                {
                    return true;
                }

                Thread.Sleep(100);
            }

            return false;
        }

        public static bool UnLockSync(Scene scene, string key)
        {
            return GetClient(scene).LockRelease(key, key);
        }

        public static async ETTask<bool> UnLock(Scene scene, string key)
        {
            return await GetClient(scene).LockReleaseAsync(key, key);
        }

        public static async ETTask<bool> TryLock(Scene scene, List<string> lst, TimeSpan t)
        {
            lst.Sort();
            foreach (string key in lst)
            {
                if (!await LockAsync(scene, key, t))
                {
                    return false;
                }
            }

            return true;
        }

        public static async ETTask UnLock(Scene scene, List<string> lst)
        {
            foreach (string key in lst)
            {
                await GetClient(scene).LockReleaseAsync(key, key);
            }
        }

        /// <summary>
        /// 获取当前gateId
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public static async ETTask<string> GetCurGateId(Scene scene, long accountId)
        {
            return await StringGetAsync(scene, $"Gate:{accountId}");
        }

        /// <summary>
        /// 设置当前的gateId
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="gateId"></param>
        /// <returns></returns>
        public static async ETTask<bool> SetCurGateId(Scene scene, long accountId, long gateId)
        {
            return await StringSetAsync(scene, $"Gate:{accountId}", gateId.ToString(), TimeSpan.FromDays(14));
        }

        public static async ETTask<bool> DelCurGateId(Scene scene, long accountId, long gateId)
        {
            var curId = await GetCurGateId(scene, accountId);
            if (string.IsNullOrEmpty(curId))
            {
                return true;
            }

            if (long.Parse(curId) == gateId)
            {
                await KeyDeleteAsync(scene, $"Gate:{accountId}");
                return true;
            }

            return false;
        }

        public static async ETTask<long> StringIncrementAsync(Scene scene, string key)
        {
            try
            {
                return await GetClient(scene).StringIncrementAsync(key);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Warning("Redis Trigger ReConnect");
                scene.Root().GetComponent<RedisManagerComponent>().ReConnect(scene.Zone());
                return await GetClient(scene).StringIncrementAsync(key);
            }
        }

        /// <summary>
        /// 获取参赛号牌
        /// </summary>
        /// <returns></returns>
        public static async ETTask<long> GetMinerRankEntryNumer(Scene scene, int activityId)
        {
            long num = await StringIncrementAsync(scene, $"MinerRankEntryNumer:{activityId}");
            return num;
        }

        /// <summary>
        /// 获取参赛号牌
        /// </summary>
        /// <returns></returns>
        public static async ETTask<long> GetMiniGameRankEntryNumer(Scene scene, int activityId)
        {
            long num = await StringIncrementAsync(scene, $"MiniGameRankEntryNumer:{activityId}");
            return num;
        }

        public static async ETTask<long> LoginRateLimiterNum(Scene scene, long timeNow)
        {
            string key = $"LoginRateLimiter:{timeNow}";
            long num = await StringIncrementAsync(scene, key);
            await GetClient(scene).KeyExpireAsync(key, TimeSpan.FromSeconds(5));
            return num;
        }
    }
}