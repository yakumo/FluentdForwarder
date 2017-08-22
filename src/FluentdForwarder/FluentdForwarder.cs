namespace Fluentd.Forwarder
{
    using System;
    using System.Diagnostics;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using MessagePack;
    using MessagePack.Resolvers;

    public class FluentdForwarder : IDisposable
    {
        public string Host { get; private set; }
        public int Port { get; private set; }
        public bool UseHeartbeat
        {
            get { return usingHeartbeat; }
            set
            {
                if (value == usingHeartbeat) return;
                if (value)
                {
                    if (String.IsNullOrWhiteSpace(Host)) return;
                    BootHeartbeatTask();
                }
                usingHeartbeat = value;
            }
        }

        private bool serverAvail;
        private bool usingHeartbeat;
        readonly UdpClient udpClient;

        const double heartbeatIntervalSeconds = 30;
        const double heatbeatLoopIntervalSeconds = 0.5;

        public FluentdForwarder() : this("127.0.0.1", 24224) { }
        public FluentdForwarder(string host) : this(host, 24224) { }

        public FluentdForwarder(string host, int port)
        {
            Host = host;
            Port = port;

            serverAvail = false;
            usingHeartbeat = false;
            udpClient = new UdpClient(port);
        }

        ~FluentdForwarder()
        {
            Dispose(false);
        }

        private void BootHeartbeatTask()
        {
            var heartbeatData = new byte[] { 0 };
            Task.Factory.StartNew(async () =>
            {
                DateTime nextTime = DateTime.Now.AddSeconds(-0.5);
                while (usingHeartbeat)
                {
                    if (nextTime < DateTime.Now)
                    {
                        Debug.Write("heart beat ?");
                        await udpClient.SendAsync(heartbeatData, heartbeatData.Length, Host, Port);
                        var result = await udpClient.ReceiveAsync();
                        if (result != null && result.Buffer.Length > 0)
                        {
                            serverAvail = true;
                            Debug.WriteLine(", ok, server avail.");
                        }else{
                            Debug.WriteLine(" ... not avail.");
                        }
                        nextTime = DateTime.Now.AddSeconds(heartbeatIntervalSeconds);
                    }
                    await Task.Delay(TimeSpan.FromSeconds(heatbeatLoopIntervalSeconds));
                }
                Debug.WriteLine("exit heartbeat.");
            });
        }

        public async Task SendMessage(string tag, object message, DateTimeOffset? timestamp = null)
        {
            DateTimeOffset ts = timestamp != null && timestamp.HasValue ? timestamp.Value : DateTimeOffset.Now;
            var packet = new FluentdPacket()
            {
                Tag = tag,
                Timestamp = ts,
                Message = message,
            }.Packet;

            var client = new TcpClient();
            await client.ConnectAsync(Host, Port).ConfigureAwait(false);
            var s = client.GetStream();
            await s.WriteAsync(packet, 0, packet.Length).ConfigureAwait(false);
            await s.FlushAsync().ConfigureAwait(false);
        }

        public static IFluentdServer BuildServer(int port)
        {
            return new FluentdServer(port);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    usingHeartbeat = false;
                    udpClient.Dispose();
                }

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~FluentdForwarder() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
