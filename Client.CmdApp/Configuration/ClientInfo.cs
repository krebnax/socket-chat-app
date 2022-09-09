using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Client.CmdApp.Configuration {
    internal class ClientInfo {
        private const int BUFFER_SIZE = 4096;
        private static byte[] _buffer;
        private static Socket _client;

        public static byte[] MainBuffer {
            get {
                if (_buffer == null)
                    _buffer = new byte[BUFFER_SIZE];

                return _buffer;
            }
        }

        public static Socket Client {
            get {
                if (_client == null)
                    _client = new Socket(Config.ENDPOINT.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                return _client;
            }
        }

        public static string CurrentChannel { get; set; }
        public static string CliendId { get; set; }
    }
}
