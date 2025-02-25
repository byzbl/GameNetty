using System.Net;

namespace ET.Server
{
    [Invoke((long)SceneType.Location)]
    public class FiberInit_Location: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            //location 内存占用
            //0 86M
            //100w 286M
            //1000w 1680M
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<MessageSender>();
            root.AddComponent<LocationManagerComoponent>();

            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.Get(root.Fiber.Id);
            root.AddComponent<HttpComponent, string>($"http://+:{startSceneConfig.GetHttpPort()}/");
            await ETTask.CompletedTask;
        }
    }
}