using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ET.Server
{
    public static class NodeHelper
    {
        public static void AddOrUpdateNodeConfig(ServerNode node)
        {
            Log.Info("===========AddOrUpdateNodeConfig============== " + node.NodeName);
            StartMachineConfig cfg = new StartMachineConfig(node.NodeId, node.InnerIP, node.OuterIP, "10000", node.PublicIP);

            StartMachineConfigCategory.Instance.AddOrUpdate(cfg);

            StartProcessConfig processConfig = new StartProcessConfig(node.NodeId, node.NodeId, node.InnerPort);
            StartProcessConfigCategory.Instance.AddOrUpdate(processConfig);

            StartSceneConfig sceneConfig = new StartSceneConfig(node.NodeId, node.NodeId, node.ZoneId, node.NodeType.ToString(), node.NodeName,
                node.OuterPort, node.HttpPort);

            StartSceneConfigCategory.Instance.AddOrUpdate(sceneConfig);
        }

        public static void RemoveConfig(int id)
        {
            Log.Warning("===========RemoveConfig============== " + id);
            StartMachineConfigCategory.Instance.Remove(id);
            StartProcessConfigCategory.Instance.Remove(id);
            StartSceneConfigCategory.Instance.Remove(id);
        }

        public static ServerNode BuildServerNode(long id)
        {
            if (StartSceneConfigCategory.Instance.DataMap.TryGetValue((int)id, out StartSceneConfig cfg))
            {
                ServerNode node = new ServerNode
                {
                    Index = (int)id % (int)(Math.Log2((int)cfg.Type)),
                    NodeType = cfg.Type,
                    InnerIP = cfg.StartProcessConfig.StartMachineConfig.InnerIP,
                    InnerPort = cfg.StartProcessConfig.Port,
                    OuterIP = cfg.StartProcessConfig.StartMachineConfig.OuterIP,
                    OuterPort = cfg.Port,
                    PublicIP = cfg.StartProcessConfig.StartMachineConfig.GetPublicIP(),
                    ZoneId = cfg.Zone,
                    HttpPort = cfg.GetHttpPort()
                    
                };
                return node;
            }

            Log.Error($"找不到 StartSceneConfig {id}");
            return null;
        }
    }
}