using System.Net;

namespace ET
{
    public partial class StartProcessConfig
    {
        public string InnerIP => this.StartMachineConfig.InnerIP;

        public string OuterIP => this.StartMachineConfig.OuterIP;

        // 内网地址外网端口，通过防火墙映射端口过来
        private IPEndPoint ipEndPoint;

        public IPEndPoint IPEndPoint
        {
            get
            {
                if (ipEndPoint == null)
                {
                    this.ipEndPoint = NetworkHelper.ToIPEndPoint(this.InnerIP, this.Port);
                }

                return this.ipEndPoint;
            }
        }

        public StartMachineConfig StartMachineConfig => StartMachineConfigCategory.Instance.Get(this.MachineId);

        partial void PostInit()
        {
        }

        public StartProcessConfig(int Id,int MachineId, int Port)
        {
            this.Id = Id;
            this.MachineId = MachineId;
            this.Port = Port;
            this.PostInit();
        }
    }
    
    public partial class StartProcessConfigCategory
    {
        public void AddOrUpdate(StartProcessConfig cfg)
        {
            if (this._dataMap.TryGetValue(cfg.Id, out StartProcessConfig oldcfg))
            {
                _dataList.Remove(oldcfg);
                _dataMap.Remove(oldcfg.Id);
            }
            this.DataMap[cfg.Id] = cfg;
            this.DataList.Add(cfg);
        }
        public void Remove(int Id)
        {
            if (this._dataMap.TryGetValue(Id, out StartProcessConfig oldcfg))
            {
                this._dataList.Remove(oldcfg);
                this._dataMap.Remove(oldcfg.Id);
            }
        }
        
    }
}