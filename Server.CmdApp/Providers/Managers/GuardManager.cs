using Server.CmdApp.Configuration;
using Server.CmdApp.Models;
using Server.CmdApp.Providers.Enums;
using SocketChatApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Server.CmdApp.Providers.Managers {
    internal class GuardManager {
        private SendManager _manager;
        private Socket _client;
        public GuardManager(Socket client, SendManager manager) {
            _client = client;
            _manager = manager;
        }
        internal void Guard(Payload receivedPayload, AsyncCallback sendCallback, AsyncCallback receiveCallback) {
            Func<SpamGuardModel, bool> predicate = x => x.ClientId == receivedPayload.SenderId;
            if (ServerInfo.ClientSpamGuards.Any(predicate)) {
                var spamGuard = ServerInfo.ClientSpamGuards.First(predicate);
                var difference = DateTime.Now.Ticks - spamGuard.LastTick;

                if (difference < ServerInfo.ALLOWED_TICK) {
                    if (!spamGuard.HasWarning) {
                        var payload = new Payload() {
                            PayloadType = PayloadResponseType.Warning,
                            ContentAccess = ContentAccess.Private
                        };

                        _manager.Send(
                            client: _client,
                            data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload)),
                            sendCallback: sendCallback,
                            receiveCallback: receiveCallback);
                        spamGuard.HasWarning = true;
                    } else {
                        _client.Shutdown(SocketShutdown.Both);
                    }
                }
            } else {
                ServerInfo.ClientSpamGuards.Add(new SpamGuardModel() {
                    ClientId = receivedPayload.SenderId,
                    HasWarning = false,
                    LastTick = DateTime.Now.Ticks
                });
            }
        }

    }
}
