using Newtonsoft.Json;

namespace ET.Server
{
    [MessageHandler(SceneType.All)]
    public class Center2Other_NotifyNodeInfoChgMsgHandler: MessageHandler<Scene, Center2Other_NotifyNodeInfoChgMsg>
    {
        protected override async ETTask Run(Scene scene, Center2Other_NotifyNodeInfoChgMsg message)
        {
            if (message.Op == (int)EMsgOpType.ADD)
            {
                foreach (string s in message.NodeInfo)
                {
                    ServerNode node = JsonConvert.DeserializeObject<ServerNode>(s);
                    NodeHelper.AddOrUpdateNodeConfig(node);
                }
            }
            else if (message.Op == (int)EMsgOpType.REMOVE)
            {
                foreach (string s in message.NodeInfo)
                {
                    ServerNode node = JsonConvert.DeserializeObject<ServerNode>(s);

                    NodeHelper.RemoveConfig(node.NodeId);
                }
            }

            await ETTask.CompletedTask;
        }
    }
}