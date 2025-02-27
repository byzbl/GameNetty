using System;
using System.Collections.Generic;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace ET.Server
{
    public class AwakeKafkaComponen: AwakeSystem<KafkaComponent>
    {
        protected override void Awake(KafkaComponent self)
        {
            //kafka队列数据写入 最好单独线程或者服务处理,要在kafka无响应时,不影响游戏运行
            /*ProducerConfig config = new ProducerConfig
            {
                BootstrapServers = ServerConfigCategory.Instance.KafkaAddr, BatchSize = 131072, LingerMs = 200, MessageTimeoutMs = 5000,
            };
            self.Producer = new ProducerBuilder<Null, string>(config).Build();*/
        }
    }

    public static class KafkaComponentSystem
    {
        public static async ETTask ProduceAsync(this KafkaComponent self, BehaviorLog log)
        {
            await self.ProduceAsync(log.TableName, log.Fileds);
        }

        private static async ETTask ProduceAsync(this KafkaComponent self, string topic, Dictionary<string, object> message)
        {
            try
            {
                if (Options.Instance.Develop != 3)
                {
                    var messageJson = JsonConvert.SerializeObject(message);
                    await self.Producer.ProduceAsync(topic, new Message<Null, string>() { Value = messageJson });
                }
                //var dr =  await self.Producer.ProduceAsync(topic, new Message<Null, string>() { Value = messageJson });
                // Log.Info($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
            }
            catch (ProduceException<string, string> e)
            {
                Log.Error($"Delivery failed: {e.Error.Reason}");
            }
        }
    }
}