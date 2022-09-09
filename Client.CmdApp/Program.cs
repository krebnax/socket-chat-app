using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Client.CmdApp.Configuration;
using SocketChatApp.Core.Models;

namespace Client.CmdApp {
    internal class Program {
        static void Main(string[] args) {
            TryConnect().Wait();
        }

        static async Task TryConnect() {
            int attempts = 0;
            while (!ClientInfo.Client.Connected) {
                try {
                    attempts++;
                    await ClientInfo.Client.ConnectAsync(Config.ENDPOINT);
                    ClientInfo.Client.BeginReceive(ClientInfo.MainBuffer, 0, ClientInfo.MainBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), ClientInfo.Client);
                    Send(JsonSerializer.Serialize(new Payload() {
                        Content = "--get-config"
                    }));
                    Send(JsonSerializer.Serialize(new Payload() {
                        Content = "--get-channels"
                    }));
                    Console.WriteLine("Connected.");
                    TrySend();
                } catch (Exception) {
                    Console.WriteLine("Failed to connect. Retrying...");
                }

                if (attempts == 3)
                    break;
            }
        }

        static void TrySend() {
            while (true) {
                var input = Console.ReadLine();
                if (ClientInfo.CurrentChannel == null && !input.StartsWith("--")) {
                    Console.WriteLine("Join a channel first with --join-channel <channel_name>. To see channel list use --get-channels");
                    continue;
                }
                var payload = new Payload() {
                    ChannelId = ClientInfo.CurrentChannel,
                    Content = input,
                    Format = PayloadFormat.Json,
                    SenderId = ClientInfo.CliendId
                };

                var serialized = JsonSerializer.Serialize(payload);
                Send(serialized);
            }
        }

        static void Send(string input) {
            byte[] buffer = Encoding.UTF8.GetBytes(input);
            ClientInfo.Client.Send(buffer);
        }

        static void OnReceive(IAsyncResult result) {
            var client = (Socket)result.AsyncState;
            int received = client.EndReceive(result);
            var dataBuffer = new byte[received];
            Array.Copy(ClientInfo.MainBuffer, dataBuffer, received);

            var text = Encoding.UTF8.GetString(dataBuffer);

            var payload = JsonSerializer.Deserialize<Payload>(text);

            if (payload.PayloadType == PayloadResponseType.Configuration) {
                ClientInfo.CliendId = payload.Content;
            }

            if (payload.PayloadType == PayloadResponseType.ChannelJoin) {
                ClientInfo.CurrentChannel = payload.ChannelId;
            }

            if (payload.SenderId != ClientInfo.CliendId)
                Console.WriteLine($"\n--> {payload.Content}");

            client.BeginReceive(ClientInfo.MainBuffer, 0, ClientInfo.MainBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), client);
        }
    }
}
