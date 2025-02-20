namespace ET.Server
{
    [MessageLocationHandler(SceneType.Map)]
    public class C2M_TestRequestHandler: MessageLocationHandler<Unit, C2M_TestRequest, M2C_TestResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_TestRequest request, M2C_TestResponse response)
        {
            response.response = request.request;
            await ETTask.CompletedTask;
        }
    }
}