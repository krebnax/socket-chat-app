using Server.CmdApp.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Server.CmdApp.Providers.Managers {
    internal class SendManager {
        internal void Send(Socket client, byte[] data, AsyncCallback? sendCallback, AsyncCallback? receiveCallback) {
            client.BeginSend(
                buffer: data,
                offset: 0,
                size: data.Length,
                socketFlags: SocketFlags.None,
                callback: sendCallback,
                state: client);
            client.BeginReceive(
                buffer: ServerInfo.MainBuffer,
                offset: 0,
                size: ServerInfo.MainBuffer.Length,
                socketFlags: SocketFlags.None,
                callback: receiveCallback,
                state: client);
        }

        internal void Send(List<Socket> clients, byte[] data, AsyncCallback? sendCallback, AsyncCallback? receiveCallback) {
            foreach (var socket in clients) {
                socket.BeginSend(
                    buffer: data,
                    offset: 0,
                    size: data.Length,
                    socketFlags: SocketFlags.None,
                    callback: sendCallback,
                    state: socket);
                socket.BeginReceive(
                    buffer: ServerInfo.MainBuffer,
                    offset: 0,
                    size: ServerInfo.MainBuffer.Length,
                    socketFlags: SocketFlags.None,
                    callback: receiveCallback,
                    state: socket);
            }
        }
    }
}
