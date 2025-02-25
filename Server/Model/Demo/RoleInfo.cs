using SqlSugar;

namespace ET.Server
{
    [SugarTable("user_role")]
#pragma warning disable ET0032
    public class RoleInfo
#pragma warning restore ET0032
    {
        [SugarColumn(IsPrimaryKey = true, ColumnName = "role_id")]
        public long Id { get; set; }

        [SugarColumn(ColumnName = "role_name")]
        public string Name { get; set; }

        [SugarColumn(ColumnName = "server_id")]
        public int ServerId { get; set; }

        [SugarColumn(ColumnName = "state")]
        public int State { get; set; }

        [SugarColumn(ColumnName = "account_id")]
        public long AccountId { get; set; }

        [SugarColumn(ColumnName = "last_login_time")]
        public long LastLoginTime { get; set; }

        [SugarColumn(ColumnName = "create_time")]
        public long CreateTime { get; set; } //角色创建时间
    }
}