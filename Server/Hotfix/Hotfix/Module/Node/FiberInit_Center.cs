using System.Net;

namespace ET.Server
{
    [Invoke((long)SceneType.Center)]
    public class FiberInit_Center: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<MessageSender>();

            root.AddComponent<DBManagerComponent>();
           // root.AddComponent<RedisManagerComponent>();

            //mysql 连接
            /*DBHelper.GetMysqlGame(root);
            DBHelper.GetMysqlLog(root);
            DBHelper.GetMongoDB(root);
            RedisHelper.GetClient(root, root.Zone());*/

            root.AddComponent<NodeManagerComponent>();

            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.Get(root.Fiber.Id);
            root.AddComponent<HttpComponent, string>($"http://+:{startSceneConfig.GetHttpPort()}/");

            Log.Info($"http init {root.Scene().Id}");
            await ETTask.CompletedTask;
        }
    }
}