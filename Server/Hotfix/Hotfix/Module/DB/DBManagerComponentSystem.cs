using System;

namespace ET.Server
{
    [FriendOf(typeof (DBManagerComponent))]
    public static partial class DBManagerComponentSystem
    {
        public static DBComponent GetZoneDB(this DBManagerComponent self, int zone)
        {
            DBComponent dbComponent = self.GetChild<DBComponent>(zone);
            if (dbComponent != null)
            {
                return dbComponent;
            }

            StartZoneConfig startZoneConfig = StartZoneConfigCategory.Instance.Get(zone);
            if (startZoneConfig.DBConnection == "")
            {
                throw new Exception($"zone: {zone} not found mongo connect string");
            }

            string dbConnStr = startZoneConfig.DBConnection.Trim();
            string dbName = startZoneConfig.DBName;
            string mysqlConnStr = startZoneConfig.MysqlConnection;
            /* 读配置文件
             if (dbConnStr == "")
            {
                if (startZoneConfig.DBConnectionFile == "")
                {
                    throw new Exception($"zone: {zone} not found mongo connect string");
                }

                string s = File.ReadAllText(startZoneConfig.DBConnectionFile);
                dbConnStr = AesServerHelper.AESDecrypt(s, AesServerHelper.MYSQL_AES_KEY, AesServerHelper.MYSQL_AES_IV);

                string[] dbConnStrS = dbConnStr.Split("/");
                dbName = dbConnStrS[^1].Trim();
            }*/

            dbComponent = self.AddChildWithId<DBComponent, string, string,string>(zone, dbConnStr, dbName,mysqlConnStr);
            return dbComponent;
        }
    }
}