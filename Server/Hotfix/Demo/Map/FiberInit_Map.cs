using System.Net;

namespace ET.Server
{
    [Invoke((long)SceneType.Map)]
    public class FiberInit_Map: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<MessageSender>();
            root.AddComponent<UnitComponent>();
            root.AddComponent<AOIManagerComponent>();
            // root.AddComponent<RoomManagerComponent>();
            root.AddComponent<LocationProxyComponent>();
            root.AddComponent<MessageLocationSenderComponent>();

            if (Options.Instance.AppType == AppType.TestDocker || Options.Instance.AppType == AppType.Docker)
            {
                StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.Get((int)root.Id);
                root.AddComponent<HttpComponent, string>($"http://+:{startSceneConfig.GetHttpPort()}/");
            }

            root.AddComponent<DBManagerComponent>();
            var dbComponent = root.GetComponent<DBManagerComponent>().GetZoneDB(1);
            /*var roleInfo = await dbComponent.GetSqlSugarScopeProvider().Queryable<RoleInfo>()
                    .Where(d => d.Id == 1).FirstAsync();
            if (roleInfo != null)
            {
               Log.Info(roleInfo.Name);
            }*/
            await ETTask.CompletedTask;
        }
    }
}