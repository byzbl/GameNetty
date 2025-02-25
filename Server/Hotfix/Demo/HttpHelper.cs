using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ET.Server
{
    public static partial class HttpHelper
    {
        public static T UnMarshalRequest<T>(HttpListenerContext context, out string bodyStr)
        {
            Stream stream = context.Request.InputStream;
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            bodyStr = reader.ReadToEnd();
            T reqInfo = JsonConvert.DeserializeObject<T>(bodyStr);
            return reqInfo;
        }

        public static void Response(HttpListenerContext context, object response)
        {
            //byte[] bytes = MongoHelper.ToJson(response).ToUtf8();
            byte[] bytes = JsonConvert.SerializeObject(response).ToUtf8();
            context.Response.StatusCode = 200;
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.ContentLength64 = bytes.Length;
            context.Response.OutputStream.Write(bytes, 0, bytes.Length);
        }
        public static void Response(HttpListenerContext context, string msg)
        {
            byte[] bytes = msg.ToUtf8();
            context.Response.StatusCode = 200;
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.ContentLength64 = bytes.Length;
            context.Response.OutputStream.Write(bytes, 0, bytes.Length);
        }
        public static async ETTask<string> Get(string link)
        {
            try
            {
                using HttpClient httpClient = new();
                HttpResponseMessage response = await httpClient.GetAsync(link);
                string result = await response.Content.ReadAsStringAsync();
                return result;
            }
            catch (Exception e)
            {
                Log.Warning($"http request fail: {link.Substring(0, link.IndexOf('?'))}\n{e}");
                return "";
            }
        }

        public static async ETTask<string> PostAsync(string url, string str)
        {
            try
            {
                using HttpClient httpClient = new();
                HttpContent content = new StringContent(str);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
                //由HttpClient发出异步Post请求
                HttpResponseMessage res = await httpClient.PostAsync(url, content);
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    string ack = res.Content.ReadAsStringAsync().Result;
                    return ack;
                }

                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return null;
            }
        }
    }
}