using Server.CmdApp.Configuration;
using Server.CmdApp.Providers.Enums;
using System;
using System.Net.Sockets;
using System.Text;

namespace Server.CmdApp.Providers {
    internal class CallbackProvider {
        public AsyncCallback CreateCallback(CallbackType callbackType) {
            switch (callbackType) {
                case CallbackType.Accept:
                    return AcceptCallback;
                case CallbackType.Receive:
                    return ReceiveCallback;
                case CallbackType.Send:
                    return SendCallback;
            }

            return null;
        }

        private void AcceptCallback(IAsyncResult result) {
            var client = ServerInfo.Listener.EndAccept(result);
            ServerInfo.Clients.Add(client);

            client.BeginReceive(
                buffer: ServerInfo.MainBuffer,
                offset: 0,
                size: ServerInfo.MainBuffer.Length,
                socketFlags: SocketFlags.None,
                callback: CreateCallback(CallbackType.Receive),
                state: client);

            ServerInfo.Listener.BeginAccept(CreateCallback(CallbackType.Accept), null);
        }

        private void ReceiveCallback(IAsyncResult result) {
            var client = (Socket)result.AsyncState;
            int received = client.EndReceive(result);
            var dataBuffer = new byte[received];
            Array.Copy(ServerInfo.MainBuffer, dataBuffer, received);

            var text = Encoding.UTF8.GetString(dataBuffer);


            Console.WriteLine($"Received: {text}");

            var data = Encoding.UTF8.GetBytes(text);

            client.BeginSend(
                buffer: data,
                offset: 0,
                size: data.Length,
                socketFlags: SocketFlags.None,
                callback: CreateCallback(CallbackType.Send),
                state: client);
            client.BeginReceive(
                buffer: ServerInfo.MainBuffer,
                offset: 0,
                size: ServerInfo.MainBuffer.Length,
                socketFlags: SocketFlags.None,
                callback: CreateCallback(CallbackType.Receive),
                state: client);

        }
        private void SendCallback(IAsyncResult result) {
            var client = (Socket)result.AsyncState;
            client.EndSend(result);
        }
    }
}

