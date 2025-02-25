using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace ET.Server
{
    [HttpHandler(SceneType.Center, "/getNodeInfo")]
    public class HttpGetNodeInfoHandler: IHttpHandler
    {
        /// <summary>
        /// 获取CenterNode 和 自身的Index
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="context"></param>
        public async ETTask Handle(Scene scene, HttpListenerContext context)
        {
            Log.Info($"HttpGetNodeInfoHandler Request: {context.Request.Url.ToString()}");

            NodeManagerComponent component = scene.GetComponent<NodeManagerComponent>();
            using (await scene.Fiber.Root.GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.Center, 1))
            {
                SceneType reqType = EnumHelper.FromString<SceneType>(context.Request.QueryString["SceneType"]);

                int index = 0;
                for (int i = 1; i < 100; i++)
                {
                   
                    int tryId =  (int)(Math.Log2((int)reqType)) * 100 + i;
                    if (component.ApplyNodeIds.ContainsKey(tryId))
                    {
                        continue;
                    }

                    if (component.DicServerNodes.ContainsKey(tryId))
                    {
                        continue;
                    }

                    index = i;
                    //增加十秒的有效期
                    component.ApplyNodeIds[tryId] = TimeInfo.Instance.ServerNow() + 15000;
                    break;
                }

                GetNodeInfoResp response = new GetNodeInfoResp();
                response.Index = index;
                response.CenterNode = NodeHelper.BuildServerNode(scene.Id);

                HttpHelper.Response(context, response);

                Log.Info($"HttpGetNodeInfoHandler Response : {JsonConvert.SerializeObject(response)}");
            }

            await ETTask.CompletedTask;
        }
    }
}