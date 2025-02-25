using System.Collections.Generic;
using System.Net;

namespace ET
{
    public partial class StartSceneConfigCategory
    {
        public MultiMap<int, StartSceneConfig> Gates = new();

        public MultiMap<int, StartSceneConfig> ProcessScenes = new();

        public Dictionary<long, Dictionary<string, StartSceneConfig>> ClientScenesByName = new();

        public StartSceneConfig LocationConfig;

        public List<StartSceneConfig> Realms = new();

        public List<StartSceneConfig> Routers = new();

        public List<StartSceneConfig> Maps = new();

        public StartSceneConfig Match;

        public StartSceneConfig Benchmark;

        public List<StartSceneConfig> GetByProcess(int process)
        {
            return this.ProcessScenes[process];
        }

        public StartSceneConfig GetBySceneName(int zone, string name)
        {
            return this.ClientScenesByName[zone][name];
        }

        partial void PostInit()
        {
            foreach (StartSceneConfig startSceneConfig in this.DataList)
            {
                this.PostInit(startSceneConfig);
            }
        }

        public void AddOrUpdate(StartSceneConfig cfg)
        {
            if (this._dataMap.TryGetValue(cfg.Id, out StartSceneConfig oldcfg))
            {
                _dataList.Remove(oldcfg);
                _dataMap.Remove(oldcfg.Id);
            }

            _dataList.Add(cfg);
            _dataMap.Add(cfg.Id, cfg);
            this.PostInit(cfg);
        }

        private void PostInit(StartSceneConfig startSceneConfig)
        {
            this.ProcessScenes.Add(startSceneConfig.Process, startSceneConfig);

            if (!this.ClientScenesByName.ContainsKey(startSceneConfig.Zone))
            {
                this.ClientScenesByName.Add(startSceneConfig.Zone, new Dictionary<string, StartSceneConfig>());
            }

            this.ClientScenesByName[startSceneConfig.Zone].Add(startSceneConfig.Name, startSceneConfig);

            switch (startSceneConfig.Type)
            {
                case SceneType.Realm:
                    this.Realms.Add(startSceneConfig);
                    break;
                case SceneType.Gate:
                    this.Gates.Add(startSceneConfig.Zone, startSceneConfig);
                    break;
                case SceneType.Location:
                    this.LocationConfig = startSceneConfig;
                    break;
                case SceneType.Router:
                    this.Routers.Add(startSceneConfig);
                    break;
                case SceneType.Map:
                    this.Maps.Add(startSceneConfig);
                    break;
                case SceneType.Match:
                    this.Match = startSceneConfig;
                    break;
                case SceneType.BenchmarkServer:
                    this.Benchmark = startSceneConfig;
                    break;
            }
        }

        public void Remove(int Id)
        {
            if (this._dataMap.TryGetValue(Id, out StartSceneConfig startSceneConfig))
            {
                this._dataList.Remove(startSceneConfig);
                this._dataMap.Remove(startSceneConfig.Id);

                this.ProcessScenes.Remove(startSceneConfig.Process, startSceneConfig);

                if (!this.ClientScenesByName.ContainsKey(startSceneConfig.Zone))
                {
                    this.ClientScenesByName.Add(startSceneConfig.Zone, new Dictionary<string, StartSceneConfig>());
                }

                this.ClientScenesByName[startSceneConfig.Zone].Remove(startSceneConfig.Name);

                switch (startSceneConfig.Type)
                {
                    case SceneType.Realm:
                        this.Realms.Remove(startSceneConfig);
                        break;
                    case SceneType.Gate:
                        this.Gates.Remove(startSceneConfig.Zone, startSceneConfig);
                        break;
                    case SceneType.Location:
                        this.LocationConfig = null;
                        break;
                    case SceneType.Router:
                        this.Routers.Remove(startSceneConfig);
                        break;
                    case SceneType.Map:
                        this.Maps.Remove(startSceneConfig);
                        break;
                    case SceneType.Match:
                        this.Match = null;
                        break;
                    case SceneType.BenchmarkServer:
                        this.Benchmark = null;
                        break;
                }
            }
        }
    }

    public partial class StartSceneConfig
    {
        public ActorId ActorId;

        public SceneType Type;

        public StartProcessConfig StartProcessConfig
        {
            get
            {
                return StartProcessConfigCategory.Instance.Get(this.Process);
            }
        }

        public StartZoneConfig StartZoneConfig
        {
            get
            {
                return StartZoneConfigCategory.Instance.Get(this.Zone);
            }
        }

        // 内网地址外网端口，通过防火墙映射端口过来
        private IPEndPoint innerIPPort;

        public IPEndPoint InnerIPPort
        {
            get
            {
                if (this.innerIPPort == null)
                {
                    this.innerIPPort = NetworkHelper.ToIPEndPoint($"{this.StartProcessConfig.InnerIP}:{this.Port}");
                }

                return this.innerIPPort;
            }
        }

        private IPEndPoint outerIPPort;

        // 外网地址外网端口
        public IPEndPoint OuterIPPort
        {
            get
            {
                if (this.outerIPPort == null)
                {
                    this.outerIPPort = NetworkHelper.ToIPEndPoint($"{this.StartProcessConfig.OuterIP}:{this.Port}");
                }

                return this.outerIPPort;
            }
        }

        partial void PostInit()
        {
            this.ActorId = new ActorId(this.Process, this.Id, 1);
            this.Type = EnumHelper.FromString<SceneType>(this.SceneType);
        }

        // http监听端口,用于判断节点是否正常启动 
        private readonly int HttpPort;

        public int GetHttpPort()
        {
            if (this.HttpPort == 0)
            {
                return this.Port;
            }

            return this.HttpPort;
        }

        public StartSceneConfig(int Id, int Process, int Zone, string SceneType, string Name, int Port, int httpPort)
        {
            this.Id = Id;
            this.Process = Process;
            this.Zone = Zone;
            this.SceneType = SceneType;
            this.Name = Name;
            this.Port = Port;
            this.HttpPort = httpPort;
            this.PostInit();
        }
    }
}