using System;
using System.Collections.Generic;
using System.Linq;

namespace ET.Server
{
    [Code]
    public class HttpDispatcher: Singleton<HttpDispatcher>, ISingletonAwake
    {
        private readonly Dictionary<string, Dictionary<SceneType, IHttpHandler>> dispatcher = new();
        
        public void Awake()
        {
            HashSet<Type> types = CodeTypes.Instance.GetTypes(typeof (HttpHandlerAttribute));

            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(HttpHandlerAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }

                HttpHandlerAttribute httpHandlerAttribute = (HttpHandlerAttribute)attrs[0];
                
                object obj = Activator.CreateInstance(type);

                IHttpHandler ihttpHandler = obj as IHttpHandler;
                if (ihttpHandler == null)
                {
                    throw new Exception($"HttpHandler handler not inherit IHttpHandler class: {obj.GetType().FullName}");
                }

                if (!this.dispatcher.TryGetValue(httpHandlerAttribute.Path, out var dict))
                {
                    dict = new Dictionary<SceneType, IHttpHandler>();
                    this.dispatcher.Add(httpHandlerAttribute.Path, dict);
                }
                
                dict.Add(httpHandlerAttribute.SceneType, ihttpHandler);
            }
        }

        public IHttpHandler Get(SceneType sceneType, string path)
        {
            if (this.dispatcher.TryGetValue(path,out var httpHandlerDic))
            {
                if (httpHandlerDic.TryGetValue(sceneType, out IHttpHandler httpHandler))
                {
                    return httpHandler;
                }

                foreach (SceneType type in httpHandlerDic.Keys.ToList())
                {
                    if (type.HasSameFlag(sceneType))
                    {
                        return httpHandlerDic[type];
                    }
                }
            }
            Log.Warning($"Http had no path {path}");
            return null;
        }
    }
}