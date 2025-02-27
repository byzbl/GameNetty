using Confluent.Kafka;

namespace ET.Server
{
    public class KafkaComponent: Entity, IAwake, IDestroy
    {
        public IProducer<Null, string> Producer { get; set; }
    }
}