using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace ET.Server
{
    [HttpHandler(SceneType.Center, "/addNode")]
    public class HttpAddNodeInfoHanlder: IHttpHandler
    {
        public async ETTask Handle(Scene scene, HttpListenerContext context)
        {
            NodeManagerComponent managerComponent = scene.GetComponent<NodeManagerComponent>();
            using (await scene.Fiber.Root.GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.Center,1))
            {
                AddNodeInfoReq reqInfo = HttpHelper.UnMarshalRequest<AddNodeInfoReq>(context, out string bodyStr);
                AddNodeInfoResp response = new AddNodeInfoResp { ErrorCode = 0, LstNodeInfo = new List<ServerNode>() };
                var reqNode = reqInfo.Node;
                if (!managerComponent.ApplyNodeIds.ContainsKey(reqNode.NodeId))
                {
                    Log.Error($"Other2Center_AddNodeInfoReqHandle 节点:{reqNode.NodeId} 不在申请列表中");
                    response.ErrorCode = 1;
                    HttpHelper.Response(context, response);
                    return;
                }
                

                NodeHelper.AddOrUpdateNodeConfig(reqNode);

                foreach (var kv in managerComponent.DicServerNodes)
                {
                    response.LstNodeInfo.Add(kv.Value);
                }

                HttpHelper.Response(context, response);

                //节点有效期15秒
                reqNode.ExpireTime = TimeInfo.Instance.ServerNow() + 15000;

                managerComponent.DicServerNodes[reqNode.NodeId] = reqNode;
                managerComponent.ApplyNodeIds.Remove(reqNode.NodeId);
                managerComponent.BroadCastAddNode(reqNode).Coroutine();
            }

            await ETTask.CompletedTask;
        }
    }
}