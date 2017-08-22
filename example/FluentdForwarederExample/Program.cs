using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Fluentd.Forwarder;

namespace FluentdForwarederExample
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1 || args.Length > 2)
            {
                Console.WriteLine($"usage: {Process.GetCurrentProcess().ProcessName} [server] [server port]");
                return;
            }

            Console.WriteLine($"connect to {args[0]}:{args[1]}");

            var isLoop = true;

            Task.Delay(TimeSpan.FromSeconds(10)).ContinueWith((res) =>
            {
                isLoop = false;
            });

            using (var ff = new FluentdForwarder(args[0], int.Parse(args[1])))
            {
                var msg = new Dictionary<string, string>(){
                    {"Test", "Hello"},
                    {"temp", "user"}
                };
                ff.SendMessage("debug.test", msg, DateTimeOffset.Now.AddMinutes(-5)).ContinueWith((res) =>
                {
                    isLoop = false;
                });
            }

            while(isLoop)
            {
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            }
        }
    }
}
