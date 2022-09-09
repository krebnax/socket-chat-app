using Server.CmdApp.Configuration;
using Server.CmdApp.Providers.Enums;
using SocketChatApp.Core.Models;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

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
            ServerInfo.ClientIds.Add(Guid.NewGuid(), client);

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

            Payload receivedPayload = JsonSerializer.Deserialize<Payload>(text);

            Console.WriteLine($"Received: {text}");

            // TODO: implement command line service
            var cmd = new CommandLineService();
            bool isTalkingToServer = receivedPayload.Content.StartsWith(CommandLineService.COMMAND_LITERAL);

            if (isTalkingToServer) {
                var serialized = JsonSerializer.Serialize(cmd.GetCommandResult(receivedPayload.Content, client));
                var data = Encoding.UTF8.GetBytes(serialized);

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
            } else {
                var senderId = ServerInfo.ClientIds.First(x => x.Value == client).Key.ToString();
                var payload = new Payload() {
                    ChannelId = receivedPayload.ChannelId,
                    Format = PayloadFormat.Json,
                    PayloadType = PayloadResponseType.Response,
                    SenderId = senderId,
                    ContentAccess = ContentAccess.Public,
                    Content = receivedPayload.Content
                };
                var serialized = JsonSerializer.Serialize(payload);
                var data = Encoding.UTF8.GetBytes(serialized);
                var toClients = ServerInfo.Channels.First(x => x.Id == receivedPayload.ChannelId).Members;
                foreach (var socket in toClients) {
                    socket.BeginSend(
                       buffer: data,
                       offset: 0,
                       size: data.Length,
                       socketFlags: SocketFlags.None,
                       callback: CreateCallback(CallbackType.Send),
                       state: socket);
                    socket.BeginReceive(
                        buffer: ServerInfo.MainBuffer,
                        offset: 0,
                        size: ServerInfo.MainBuffer.Length,
                        socketFlags: SocketFlags.None,
                        callback: CreateCallback(CallbackType.Receive),
                        state: socket);
                }
            }
        }
        private void SendCallback(IAsyncResult result) {
            var client = (Socket)result.AsyncState;
            client.EndSend(result);
        }
    }
}

