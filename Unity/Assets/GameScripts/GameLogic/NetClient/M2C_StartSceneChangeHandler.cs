namespace ET.Client
{
    [MessageHandler(SceneType.All)]
    public class M2C_StartSceneChangeHandler : MessageHandler<Scene, M2C_StartSceneChange>
    {
        protected override async ETTask Run(Scene root, M2C_StartSceneChange message)
        {
            await ETTask.CompletedTask;
            Log.Debug($"========{root.SceneType}====M2C_StartSceneChangeHandler============");
            //await SceneChangeHelper.SceneChangeTo(root, message.SceneName, message.SceneInstanceId);
        }
    }
}