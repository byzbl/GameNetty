using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof (Scene))]
    public class NodeManagerComponent: Entity, IAwake
    {
        /// <summary>
        /// 在线中的服务器节点
        /// </summary>
        public Dictionary<int, ServerNode> DicServerNodes { get; set; } = new Dictionary<int, ServerNode>();
        
        
        /// <summary>
        /// 申请中的服务器id-有效期
        /// </summary>
        public Dictionary<int, long> ApplyNodeIds { get; set; } = new Dictionary<int, long>();
        
        public long Timer { get; set; } 
    }
}