using System;
using System.Collections.Generic;
using System.Net;

namespace ET.Server
{
    [EnableClass]
    public class ServerNode
    {
        public int Index { get; set; }
        public int ZoneId { get; set; }

        public int NodeId
        {
            get
            {
                //NodeId == ProcessId 涉及到ActorId生成 Int14(8191)为上限
                // 每种类型的服务器数量不能超过100
                return (int)(Math.Log2((int)NodeType)) * 100 + Index;
            }
        }

        public string NodeName
        {
            get
            {
                return $"{NodeType}_{Index}_{NodeId}";
            }
        }

        public SceneType NodeType { get; set; }

        public string InnerIP { get; set; } //集群Pod内网IP
        public int InnerPort { get; set; }

        public string OuterIP { get; set; } = "0.0.0.0"; //进程监听地址
        public int OuterPort { get; set; } = 30300;

        public string PublicIP { get; set; } //对外公网IP

        //每个节点服务都提供Http端口,用于业务和Center监控服务监控状态
        public int HttpPort { get; set; }

        public long ExpireTime { get; set; } //有效期
    }

    [EnableClass]
    public class GetNodeInfoResp
    {
        public int Index { get; set; }
        public ServerNode CenterNode { get; set; }
    }

    [EnableClass]
    public class NodePingResp
    {
        public int ErrCode { get; set; }

        public ServerNode CenterNode { get; set; }
    }

    [EnableClass]
    public class AddNodeInfoReq
    {
        public ServerNode Node { get; set; }
    }

    [EnableClass]
    public class AddNodeInfoResp
    {
        public int ErrorCode { get; set; }

        public List<ServerNode> LstNodeInfo { get; set; }
    }
}