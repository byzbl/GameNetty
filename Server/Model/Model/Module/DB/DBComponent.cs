using MongoDB.Driver;
using SqlSugar;

namespace ET.Server
{
    /// <summary>
    /// 用来缓存数据
    /// </summary>
    [ChildOf(typeof (DBManagerComponent))]
    public class DBComponent: Entity, IAwake<string, string, string>
    {
        public const int TaskCount = 31;

        public MongoClient mongoClient;
        public IMongoDatabase database;

        public SqlSugarScopeProvider m_sqlSugerProvider;

#pragma warning disable ET0006
        public SqlSugarScopeProvider GetSqlSugarScopeProvider()
        {
            return m_sqlSugerProvider;
        }
#pragma warning restore ET0006
    }
}