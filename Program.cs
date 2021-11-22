using System;
using Microsoft.Extensions.Configuration;
using Confluent.Kafka;

namespace kafka_producer_consumer
{
    class Program
    {
        public static ProducerConfig producerConfig;
        public static ConsumerConfig consumerAConfig;
        public static ConsumerConfig consumerBConfig;
        public static string Topic;

        static void Main(string[] args)
        {
            // build the Kafka config
            Program.createConfig();

            // Produce 5 messages to topic
            Console.WriteLine("Producing [5] test messages to topic [" + Topic + "]");
            using (var producer = new ProducerBuilder<string, string>(producerConfig).Build())
            {
                try
                {
                    var result1 = producer.ProduceAsync(Topic, new Message<string, string> { Key = "service-a", Value = "message-1" }).GetAwaiter().GetResult();
                    var result2 = producer.ProduceAsync(Topic, new Message<string, string> { Key = "service-b", Value = "message-2" }).GetAwaiter().GetResult();
                    var result3 = producer.ProduceAsync(Topic, new Message<string, string> { Key = "service-a", Value = "message-3" }).GetAwaiter().GetResult();
                    var result4 = producer.ProduceAsync(Topic, new Message<string, string> { Key = "service-a", Value = "message-4" }).GetAwaiter().GetResult();
                    var result5 = producer.ProduceAsync(Topic, new Message<string, string> { Key = "service-b", Value = "message-5" }).GetAwaiter().GetResult();
                    Console.WriteLine("Produced messages with offset [" + result1.Offset + "] through [" + result5.Offset + "]");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Oops, something went wrong: {e}");
                }
            }

            // Consumer 'A' consumes messages 1-2
            Console.WriteLine("\nConsumer-A:  Reading [2] Messages");
            using (var consumerA = new ConsumerBuilder<string, string>(consumerAConfig).Build())
            {
                consumerA.Subscribe(Program.Topic);
                
                for (int i = 0; i < 2; i++) {
                    var result = consumerA.Consume(10000);
                    if(result != null) {
                        Console.WriteLine("Consumer-A:  Offset [" + result.Offset + "] Message[" + result.Message.Key.ToString() + " -> " + result.Message.Value.ToString() + "]");
                    }
                }
                consumerA.Close();
            }

            // Consumer 'B' consumes messages 1-5
            Console.WriteLine("\nConsumer-B:  Reading [5] Messages");
            using (var consumerB = new ConsumerBuilder<string, string>(consumerBConfig).Build())
            {
                consumerB.Subscribe(Topic);
                for (int i = 0; i < 5; i++) {
                    var result = consumerB.Consume(10000);
                    if(result != null && result.Message != null && result.Message.Key != null && result.Message.Value != null) {
                        Console.WriteLine("Consumer-B:  Offset [" + result.Offset + "] Message[" + result.Message.Key.ToString() + " -> " + result.Message.Value.ToString() + "]");
                    }
                }
                consumerB.Close();
            }

            // Consumer 'A' consumes messages 3-5
            Console.WriteLine("\nConsumer-A:  Reading [3] Messages");
            using (var consumerA = new ConsumerBuilder<string, string>(consumerAConfig).Build())
            {
                consumerA.Subscribe(Topic);
                
                for (int i = 0; i < 3; i++) {
                    var result = consumerA.Consume(10000);
                    if(result != null) {
                        Console.WriteLine("Consumer-A:  Offset [" + result.Offset + "] Message[" + result.Message.Key.ToString() + " -> " + result.Message.Value.ToString() + "]");
                    }
                }
                consumerA.Close();
            }
        }

    // Create config objects for producer and two consumers
    public static void createConfig() {
        // connection settings
        var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json").Build();
        var endpoint = config.GetValue<string>("StreamingEndpoint");
        var username = config.GetValue<string>("Username");
        var password = config.GetValue<string>("Password");

        // topic name
        Program.Topic = "topic-a";

        // producer
        Program.producerConfig = new ProducerConfig 
                { 
                BootstrapServers = endpoint,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = username,
                SaslPassword = password,
                MessageMaxBytes = 1024*1024
                };

        // consumer 'a'
        Program.consumerAConfig = new ConsumerConfig
                {
                BootstrapServers = endpoint,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = username,
                SaslPassword = password,
                MessageMaxBytes = 1024*1024,
                GroupId = "consumer-a",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                SessionTimeoutMs = 30000
                };

        // consumer 'b'
        Program.consumerBConfig = new ConsumerConfig
                {
                BootstrapServers = endpoint,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = username,
                SaslPassword = password,
                MessageMaxBytes = 1024*1024,
                GroupId = "consumer-b",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                SessionTimeoutMs = 30000
                };
        }
    }
}
