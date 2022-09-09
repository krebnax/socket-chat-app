using System.Net;

namespace Server.CmdApp.Configuration {
    internal static class Config {
        private const int PORT = 11_000;
        internal static readonly IPAddress IP_ADDRESS;
        internal static readonly IPEndPoint ENDPOINT;

        static Config() {
            IP_ADDRESS = new IPAddress(new byte[] { 127, 0, 0, 1 });
            ENDPOINT = new IPEndPoint(IP_ADDRESS, PORT);
        }
    }
}
