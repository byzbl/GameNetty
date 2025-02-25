using System;
using System.Net;
using Newtonsoft.Json;

namespace ET.Server
{
    [HttpHandler(SceneType.All, "/nodePing")]
    public class HttpNodePingHandler: IHttpHandler
    {
        public async ETTask Handle(Scene scene, HttpListenerContext context)
        {
            Log.Debug($"{scene.SceneType} nodePing ok");
            HttpHelper.Response(context, "ok");
            await ETTask.CompletedTask;
        }
    }
}