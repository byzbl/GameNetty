using System;
using System.Net;

namespace ET.Server
{
    [Invoke((long)SceneType.RouterManager)]
    public class FiberInit_RouterManager: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.Get((int)root.Id);
            root.AddComponent<HttpComponent, string>($"http://+:{startSceneConfig.GetHttpPort()}/");

            root.AddComponent<DBManagerComponent>();
            var dbComponent = root.GetComponent<DBManagerComponent>().GetZoneDB(1);
            /*var roleInfo = dbComponent.GetSqlSugarScope().Queryable<RoleInfo>()
                    .Where(d => d.Id == 1).First();
            if (roleInfo != null)
            {
                Log.Info(roleInfo.Name);
            }*/
            root.AddComponent<RedisManagerComponent>();

            bool f = await RedisHelper.SetString(root, "test", "test123", TimeSpan.FromDays(1));
            if (!f)
            {
                Log.Info("redis测试写入失败");
            }
            else
            {
                Log.Info("redis测试写入成功");
            }

            string test = await RedisHelper.GetString(root, "test");
            Log.Info(test);

            await ETTask.CompletedTask;
        }
    }
}