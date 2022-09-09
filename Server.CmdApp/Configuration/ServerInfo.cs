using System;
using System.Collections.Generic;
using System.Net.Sockets;
using SocketChatApp.Core.Models;

namespace Server.CmdApp.Configuration {
    internal class ServerInfo {
        private const int BUFFER_SIZE = 4096;
        private static Socket _listener;
        private static byte[] _buffer;
        private static List<Socket> _clients;
        private static Dictionary<Guid, Socket> _clientIds;
        private static string _serverId;
        private static List<Channel<Socket>> _channels;

        private ServerInfo() { }

        internal static string ServerId {
            get {
                if (_serverId == null)
                    _serverId = Guid.NewGuid().ToString();

                return _serverId;
            }
        }

        internal static Socket Listener {
            get {
                if (_listener == null)
                    _listener = new Socket(Config.ENDPOINT.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                return _listener;
            }
        }

        internal static List<Socket> Clients {
            get {
                if (_clients == null)
                    _clients = new List<Socket>();

                return _clients;
            }
        }

        internal static byte[] MainBuffer {
            get {
                if (_buffer == null)
                    _buffer = new byte[BUFFER_SIZE];

                return _buffer;
            }
        }

        internal static Dictionary<Guid, Socket> ClientIds {
            get {
                if (_clientIds == null)
                    _clientIds = new Dictionary<Guid, Socket>();

                return _clientIds;
            }
        }

        internal static List<Channel<Socket>> Channels {
            get {
                if (_channels == null) {
                    _channels = new List<Channel<Socket>>() {
                        new Channel<Socket>() {
                            Id = Guid.NewGuid().ToString(),
                            Name = "default1",
                            Members = new List<Socket>(),
                            MaxSlots = 2
                        },
                        new Channel<Socket>() {
                            Id = Guid.NewGuid().ToString(),
                            Name = "default2",
                            Members = new List<Socket>(),
                            MaxSlots = 2
                        },
                    };
                }

                return _channels;
            }
        }
    }
}
