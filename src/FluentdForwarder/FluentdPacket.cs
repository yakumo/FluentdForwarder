namespace Fluentd.Forwarder
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using MessagePack;
    using MessagePack.Formatters;
    using MessagePack.Resolvers;

    [MessagePackObject]
    public class FluentdPacket
    {
        [Key(0)]
        public string Tag { get; set; }

        [Key(1)]
        public int Time { get; private set; }

        [IgnoreMember]
        public DateTimeOffset Timestamp { get; set; }

        [Key(2)]
        public object Message { get; set; }

        [IgnoreMember]
        public string Msg { get; set; }

        [IgnoreMember]
        public byte[] Packet
        {
            get
            {
                Time = (int)Timestamp.ToUnixTimeSeconds();
                Msg = "test";
                return MessagePackSerializer.Serialize(this);
            }
        }

        static FluentdPacket()
        {
            CompositeResolver.RegisterAndSetAsDefault(
                MessagePack.Resolvers.BuiltinResolver.Instance,
                MessagePack.Resolvers.AttributeFormatterResolver.Instance,

                // replace enum resolver
                MessagePack.Resolvers.DynamicEnumAsStringResolver.Instance,

                MessagePack.Resolvers.DynamicGenericResolver.Instance,
                MessagePack.Resolvers.DynamicUnionResolver.Instance,
                MessagePack.Resolvers.DynamicObjectResolver.Instance,

                MessagePack.Resolvers.PrimitiveObjectResolver.Instance,

                // final fallback(last priority)
                MessagePack.Resolvers.DynamicContractlessObjectResolver.Instance
            );
        }
    }
}
