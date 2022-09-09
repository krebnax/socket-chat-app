using Server.CmdApp.Configuration;
using Server.CmdApp.Providers;
using Server.CmdApp.Providers.Enums;
using System;

namespace Server.CmdApp {
    internal class Program {
        static void Main(string[] args) {
            ServerInfo.Listener.Bind(Config.ENDPOINT);
            ServerInfo.Listener.Listen(100);

            var callbackProvider = new CallbackProvider();
            ServerInfo.Listener.BeginAccept(callbackProvider.CreateCallback(CallbackType.Accept), null);

            Console.WriteLine("Listening...");
            Console.ReadKey();
        }
    }
}
