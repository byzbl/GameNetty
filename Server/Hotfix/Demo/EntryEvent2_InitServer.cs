using System;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace ET.Server
{
    [Event(SceneType.Main)]
    public class EntryEvent2_InitServer: AEvent<Scene, EntryEvent2>
    {
        protected override async ETTask Run(Scene root, EntryEvent2 args)
        {
            switch (Options.Instance.AppType)
            {
                case AppType.Server:
                {
                    int process = root.Fiber.Process;
                    StartProcessConfig startProcessConfig = StartProcessConfigCategory.Instance.Get(process);
                    if (startProcessConfig.Port != 0)
                    {
                        await FiberManager.Instance.Create(SchedulerType.ThreadPool, ConstFiberId.NetInner, 0, SceneType.NetInner, "NetInner");
                    }

                    // 根据配置创建纤程
                    var processScenes = StartSceneConfigCategory.Instance.GetByProcess(process);
                    foreach (StartSceneConfig startConfig in processScenes)
                    {
                        await FiberManager.Instance.Create(SchedulerType.ThreadPool, startConfig.Id, startConfig.Zone, startConfig.Type,
                            startConfig.Name);
                    }

                    break;
                }
                case AppType.Watcher:
                {
                    root.AddComponent<WatcherComponent>();
                    break;
                }
                case AppType.GameTool:
                {
                    break;
                }
                case AppType.Docker: //docker 模式增加中心服

                    string publicIp = Environment.GetEnvironmentVariable("PublicIP");
                    Log.Info($"PublicIP:{publicIp}");
                    string innerIp = Environment.GetEnvironmentVariable("InnerIP");
                    Log.Info($"InnerIP:{innerIp}");
                    string centerUrl = Environment.GetEnvironmentVariable("CenterUrl");
                    if (centerUrl != "")
                    {
                        Options.Instance.CenterUrl = $"http://{centerUrl}";
                    }

                    string sceneType = Environment.GetEnvironmentVariable("SceneType");
                    if (sceneType != "")
                    {
                        Options.Instance.SceneType = EnumHelper.FromString<SceneType>(sceneType);
                    }

                    if (Options.Instance.CenterUrl == "")
                    {
                        Log.Error($"Docker 模式下 CenterUrl 不存在");
                        throw new Exception("CenterUrl 不存在");
                    }

                    if (Options.Instance.SceneType == SceneType.None)
                    {
                        Log.Error($"Docker 模式下 SceneType 不存在");
                        throw new Exception("SceneType 不存在");
                    }

                    var allCfg = StartSceneConfigCategory.Instance.DataMap.Values.ToList();
                    foreach (StartSceneConfig config in allCfg)
                    {
                        StartSceneConfigCategory.Instance.Remove(config.Id);
                    }

                    ServerNode node = new ServerNode
                    {
                        NodeType = Options.Instance.SceneType,
                        InnerIP = innerIp,
                        InnerPort = 20000,
                        OuterIP = "0.0.0.0",
                        OuterPort = 30300,
                        PublicIP = publicIp,
                        HttpPort = 8088,
                    };
                    /*switch (Options.Instance.SceneType)
                    {
                        // case SceneType.PayServer:
                        //case SceneType.Center:
                        case SceneType.Http:
                            // case SceneType.GMManager:
                            node.OuterPort = 8088;
                            break;
                    }*/

                    await InitSceneForDocker(root, node);
                    break;
                case AppType.TestDocker: //本地模拟docker使用

                    var all = StartSceneConfigCategory.Instance.DataMap.Values.ToList();
                    foreach (StartSceneConfig config in all)
                    {
                        StartSceneConfigCategory.Instance.Remove(config.Id);
                    }

                    Options.Instance.CenterUrl = "http://172.18.68.114:30010";
                    //创建一个centerScene
                    ServerNode sceneNode = new ServerNode
                    {
                        NodeType = Options.Instance.SceneType,
                        InnerIP = "127.0.0.1",
                        //InnerPort = 20001,
                        OuterIP = "0.0.0.0"
                        //  OuterPort = 30300
                    };
                    switch (Options.Instance.SceneType)
                    {
                        case SceneType.Center:
                            sceneNode.Index = 1;
                            sceneNode.InnerPort = 20001;
                            sceneNode.HttpPort = 30010;
                            break;
                        case SceneType.Gate:
                            sceneNode.InnerPort = 20003;
                            sceneNode.OuterPort = 30303;
                            sceneNode.HttpPort = 30003;
                            break;
                        case SceneType.Location:
                            sceneNode.InnerPort = 20004;
                            sceneNode.HttpPort = 30004;
                            break;
                        case SceneType.Map:
                            sceneNode.InnerPort = 20005;
                            sceneNode.HttpPort = 30005;
                            break;
                        case SceneType.Realm:
                            sceneNode.InnerPort = 20006;
                            sceneNode.OuterPort = 30304;
                            sceneNode.HttpPort = 30006;
                            break;
                        case SceneType.RouterManager:
                            sceneNode.InnerPort = 20007;
                            sceneNode.HttpPort = 30300;
                            break;
                        /*case SceneType.UnitCache:
                            sceneNode.InnerPort = 20008;
                            break;
                        case SceneType.PayServer:
                            sceneNode.InnerPort = 20009;
                            break;
                        case SceneType.Rank:
                            sceneNode.InnerPort = 20010;
                            sceneNode.OuterPort = 30011;
                            break;*/
                        /*case SceneType.Http:
                            sceneNode.InnerPort = 20011;
                            sceneNode.HttpPort = 30300;
                            break;*/
                        case SceneType.Router:
                            sceneNode.InnerPort = 20012;
                            sceneNode.OuterPort = 30302;
                            sceneNode.HttpPort = 30012;
                            sceneNode.PublicIP = "172.18.68.114";
                            break;
                    }

                    await InitSceneForDocker(root, sceneNode);

                    break;
            }

            if (Options.Instance.Console == 1)
            {
                root.AddComponent<ConsoleComponent>();
            }
        }

        public async ETTask InitSceneForDocker(Scene root, ServerNode node)
        {
            //非center服务器需要向center申请Index,用于生成唯一id,服务器名等
            if (Options.Instance.SceneType != SceneType.Center)
            {
                await root.GetComponent<TimerComponent>().WaitAsync(500);
                while (true)
                {
                    try
                    {
                        //需要请求分配节点id
                        string resp = await HttpHelper.Get($"{Options.Instance.CenterUrl}/getNodeInfo?SceneType={Options.Instance.SceneType}");
                        if (resp == null)
                        {
                            await root.GetComponent<TimerComponent>().WaitAsync(1000);
                            Log.Warning("请求中心服失败");
                            continue;
                        }

                        var respObj = JsonConvert.DeserializeObject<GetNodeInfoResp>(resp);
                        node.Index = respObj.Index;
                        //加入centerNode
                        NodeHelper.AddOrUpdateNodeConfig(respObj.CenterNode);
                        break;
                    }
                    catch (Exception e)
                    {
                        Log.Warning($"请求中心服失败:{e.Message}");
                        await root.GetComponent<TimerComponent>().WaitAsync(1000);
                    }
                }
            }

            NodeHelper.AddOrUpdateNodeConfig(node);

            Options.Instance.Process = node.NodeId;

            int process2 = root.Fiber.Process;
            StartProcessConfig startProcessConfig2 = StartProcessConfigCategory.Instance.Get(process2);
            if (startProcessConfig2.Port != 0)
            {
                await FiberManager.Instance.Create(SchedulerType.ThreadPool, ConstFiberId.NetInner, 0, SceneType.NetInner, "NetInner");
            }

            // 根据配置创建纤程
            var startConfig = StartSceneConfigCategory.Instance.Get(process2);
            await FiberManager.Instance.Create(SchedulerType.ThreadPool, startConfig.Id, startConfig.Zone, startConfig.Type,
                startConfig.Name);

            if (Options.Instance.SceneType != SceneType.Center)
            {
                //服务启动成功之后通知Center 增加节点
                ServerNode myNode = NodeHelper.BuildServerNode(node.NodeId);
                var request = new AddNodeInfoReq { Node = myNode };
                string url = $"{Options.Instance.CenterUrl}/addNode";
                string resp2 = await HttpHelper.PostAsync(url, JsonConvert.SerializeObject(request));
                if (resp2 == null)
                {
                    Log.Error("NodeComponent addNode Failed");
                    return;
                }

                Log.Info(resp2);
                AddNodeInfoResp respNodes = JsonConvert.DeserializeObject<AddNodeInfoResp>(resp2);
                if (respNodes.ErrorCode != 0)
                {
                    Log.Error("NodeComponent addNode Failed");
                    return;
                }

                foreach (var nodeInfo in respNodes.LstNodeInfo)
                {
                    NodeHelper.AddOrUpdateNodeConfig(nodeInfo);
                }
            }
        }
    }
}