namespace Fluentd.Forwarder
{
    using System;

    public interface IFluentdServer
    {
        int Port { get; }
    }

    internal class FluentdServer : IFluentdServer
    {
        public int Port { get; private set; }

        public FluentdServer(int port)
        {
            Port = port;
        }
    }
}
