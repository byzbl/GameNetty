using System;
using System.Collections.Generic;
using System.Net;

namespace ET.Server
{
    [EntitySystemOf(typeof(HttpComponent))]
    [FriendOf(typeof(HttpComponent))]
    public static partial class HttpComponentSystem
    {
        [EntitySystem]
        private static void Awake(this HttpComponent self, string address)
        {
            try
            {
                self.Listener = new HttpListener();

                foreach (string s in address.Split(';'))
                {
                    if (s.Trim() == "")
                    {
                        continue;
                    }
                    self.Listener.Prefixes.Add(s);
                }

                self.Listener.Start();

                self.Accept().Coroutine();
            }
            catch (HttpListenerException e)
            {
                //netsh http add urlacl url=http://+:30300/  user=Everyone
               // netsh advfirewall firewall Add rule name=\"http://+:30300/\" dir=in protocol=tcp localport=30300 action=allow
                throw new Exception($"请先在cmd中运行: netsh http add urlacl url=http://*:你的address中的端口/ user=Everyone, address: {address}", e);
            }
        }
        
        [EntitySystem]
        private static void Destroy(this HttpComponent self)
        {
            self.Listener.Stop();
            self.Listener.Close();
        }

        private static async ETTask Accept(this HttpComponent self)
        {
            long instanceId = self.InstanceId;
            while (self.InstanceId == instanceId)
            {
                try
                {
                    HttpListenerContext context = await self.Listener.GetContextAsync();
                    self.Handle(context).Coroutine();
                }
                catch (ObjectDisposedException)
                {
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        private static async ETTask Handle(this HttpComponent self, HttpListenerContext context)
        {
            try
            {
                IHttpHandler handler = HttpDispatcher.Instance.Get(self.IScene.SceneType, context.Request.Url.AbsolutePath);
                if (handler != null)
                {
                    await handler.Handle(self.Scene(), context);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            context.Request.InputStream.Dispose();
            context.Response.OutputStream.Dispose();
        }
    }
}