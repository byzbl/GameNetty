namespace ET
{
    partial class StartMachineConfig
    {

        private readonly string PublicIP;

        //容器环境下OuterIP(监听端口)跟PublicIP(对外端口不一致)
        //本机部署一致
        public string GetPublicIP()
        {
            if (string.IsNullOrEmpty(this.PublicIP))
            {
                return this.OuterIP;
            }

            return this.PublicIP;
        }
        public StartMachineConfig(int Id, string InnerIP, string OuterIP, string WatcherPort,string PublicIP)
        {
            this.Id = Id;
            this.InnerIP = InnerIP;
            this.OuterIP = OuterIP;
            this.WatcherPort = WatcherPort;
            this.PublicIP = PublicIP;
            PostInit();
        }

        
    }

    partial class StartMachineConfigCategory
    {
        public void AddOrUpdate(StartMachineConfig cfg)
        {
            if (this._dataMap.TryGetValue(cfg.Id, out StartMachineConfig oldcfg))
            {
                _dataList.Remove(oldcfg);
                _dataMap.Remove(oldcfg.Id);
            }
            _dataList.Add(cfg);
            _dataMap.Add(cfg.Id, cfg);
        }
        
        public void Remove(int Id)
        {
            if (this._dataMap.TryGetValue(Id, out StartMachineConfig oldcfg))
            {
                this._dataList.Remove(oldcfg);
                this._dataMap.Remove(oldcfg.Id);
            }
        }
    }
}