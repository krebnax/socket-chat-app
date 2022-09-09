using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Client.CmdApp.Configuration;

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
                Send(input);
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
            Console.WriteLine($"Received: {text}");

            client.BeginReceive(ClientInfo.MainBuffer, 0, ClientInfo.MainBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), client);
        }
    }
}
