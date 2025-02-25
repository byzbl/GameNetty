using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ET.Server
{
    [FriendOfAttribute(typeof (ET.Server.NodeManagerComponent))]
    public class NodeManagerComponentAwakeSystem: AwakeSystem<NodeManagerComponent>
    {
        protected override void Awake(NodeManagerComponent self)
        {
            self.Timer = self.Fiber().Root.GetComponent<TimerComponent>().NewRepeatedTimer(3000, TimerInvokeType.ServerNodeCheck, self);
        }
    }

    [Invoke(TimerInvokeType.ServerNodeCheck)]
    public class NodeManagerComponentTimer: ATimer<NodeManagerComponent>
    {
#pragma warning disable ET0021
        protected override async void Run(NodeManagerComponent self)
        {
            try
            {
                if (self.IsDisposed || self.Parent == null)
                {
                    return;
                }

                if (self.Scene() == null)
                {
                    return;
                }

                long now = TimeInfo.Instance.ServerNow();

                List<ServerNode> needDel = new List<ServerNode>();

                foreach (int id in self.DicServerNodes.Keys.ToList())
                {
                    try
                    {
                        // 检测节点是否在线
                        string resp = await HttpHelper.Get($"http://{self.DicServerNodes[id].InnerIP}:{self.DicServerNodes[id].HttpPort}/nodePing");
                        if (resp == "ok")
                        {
                            self.DicServerNodes[id].ExpireTime = TimeInfo.Instance.ServerNow() + 15000;
                        }
                    }
                    catch (Exception)
                    {
                       Log.Warning($"{id} 节点检测失败");
                    }
                   
                }

                foreach (int id in self.DicServerNodes.Keys.ToList())
                {
                    if (now > self.DicServerNodes[id].ExpireTime)
                    {
                        StartSceneConfig sceneConfig = StartSceneConfigCategory.Instance.Get(id);
                        //过期删除
                        needDel.Add(self.DicServerNodes[id]);
                        self.DicServerNodes.Remove(id);
                        if (sceneConfig != null)
                        {
                            Log.Warning($"节点过期删除：{sceneConfig.Type} {sceneConfig.Id}");
                            NodeHelper.RemoveConfig(id);
                        }
                    }
                }

                if (needDel.Count > 0)
                {
                    foreach (KeyValuePair<int, ServerNode> kv in self.DicServerNodes)
                    {
                        Center2Other_NotifyNodeInfoChgMsg msg = new();
                        foreach (ServerNode node in needDel)
                        {
                            string nodeInfo = JsonConvert.SerializeObject(node);
                            msg.NodeInfo.Add(nodeInfo);
                        }

                        msg.Op = (int)EMsgOpType.REMOVE;

                        self.Fiber().Root.GetComponent<MessageSender>()
                                .Send(StartSceneConfigCategory.Instance.Get(kv.Value.NodeId).ActorId, msg);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }
#pragma warning restore ET0021
    }

    public static class NodeManagerComponentSystem
    {
        public static async ETTask BroadCastAddNode(this NodeManagerComponent self, ServerNode node)
        {
            string nodeInfo = JsonConvert.SerializeObject(node);

            foreach (var kv in self.DicServerNodes)
            {
                if (kv.Key == node.NodeId)
                {
                    continue;
                }

                Center2Other_NotifyNodeInfoChgMsg msg = new();
                msg.NodeInfo.Add(nodeInfo);
                msg.Op = (int)EMsgOpType.ADD;

                self.Fiber().Root.GetComponent<MessageSender>()
                        .Send(StartSceneConfigCategory.Instance.Get(kv.Value.NodeId).ActorId, msg);
            }

            await ETTask.CompletedTask;
        }
    }
}