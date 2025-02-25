using System;
using SqlSugar;

namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class DBManagerComponent: Entity, IAwake
    {
        [StaticField]
        public static SqlSugarScope SqlSugar;
    }
}