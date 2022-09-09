using Server.CmdApp.Configuration;
using SocketChatApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Server.CmdApp {
    internal class CommandLineService {
        private const string GET_CONFIG = "--get-config";
        private const string HELP = "--help";
        private const string CREATE_CHANNEL = "--create-channel";
        private const string GET_CHANNELS = "--get-channels";
        private const string JOIN_CHANNEL = "--join-channel";
        private Payload _payload;

        private readonly string[] _avaliableCommands = new string[] {
            GET_CONFIG, HELP, CREATE_CHANNEL, GET_CHANNELS, JOIN_CHANNEL
        };

        public const string COMMAND_LITERAL = "--";

        public CommandLineService() {
            _payload = new Payload() {
                ChannelId = null,
                ContentAccess = ContentAccess.Private,
                Format = PayloadFormat.Json,
                SenderId = ServerInfo.ServerId,
                Content = "Issued command is not recognized. Refer --help for available commands."
            };
        }

        public Payload GetCommandResult(string command, Socket client) {
            var cmd = command.Split(" ")[0];
            if (!_avaliableCommands.Any(x => x.StartsWith(cmd)))
                return _payload;

            switch (cmd) {
                case GET_CONFIG:
                    return GetConfig(client);
                case HELP:
                    return GetHelp();
                case CREATE_CHANNEL:
                    return CreateChannel(command, client);
                case GET_CHANNELS:
                    return GetChannels();
                case JOIN_CHANNEL:
                    return JoinChannel(command, client);
            }

            return null;
        }

        private Payload GetHelp() {
            _payload.Content = "Available Commands (each starts with --):\nhelp: Fetches available commands\ncreate-channel <channel_name>: Creates a channel for current use\nget-channels: Fetches available channels";
            return _payload;
        }

        private Payload CreateChannel(string command, Socket client) {
            var splitted = command.Split(" ");
            if (splitted.Length == 1 || string.IsNullOrWhiteSpace(splitted[1]))
                return _payload;

            var channel = ServerInfo.Channels.FirstOrDefault(x => x.Name == splitted[1]);
            if (channel != null) {
                _payload.Content = "Channel name already exists. To join a channel use --join-channel <channel_name>";
                return _payload;
            }

            ServerInfo.Channels.Add(new Channel<Socket>() {
                Id = Guid.NewGuid().ToString(),
                Name = splitted[1],
                Members = new List<Socket>()
            });

            return JoinChannel($"--join-channel {splitted[1]}", client);
        }

        private Payload GetChannels() {
            var strBuilder = new StringBuilder("Available channels:\n");
            foreach (var channel in ServerInfo.Channels) {
                strBuilder.Append($"{channel.Name} ({channel.Members.Count}/{channel.MaxSlots})\n");
            }
            _payload.Content = strBuilder.ToString();
            return _payload;
        }

        private Payload JoinChannel(string command, Socket client) {
            var splitted = command.Split(" ");
            if (splitted.Length == 1 || string.IsNullOrWhiteSpace(splitted[1]))
                return _payload;

            var channel = ServerInfo.Channels.FirstOrDefault(x => x.Name == splitted[1]);
            if (channel == null) {
                _payload.Content = "Channel name not found. To create a channel use --create-channel <channel_name>";
                return _payload;
            }

            if (channel.MaxSlots == channel.Members.Count) {
                _payload.Content = "Channel is full";
                return _payload;
            }

            foreach (var existingChannel in ServerInfo.Channels) {
                if (existingChannel.Members.Contains(client))
                    existingChannel.Members.Remove(client);
            }

            channel.Members.Add(client);
            _payload.Content = $"Joined channel: {channel.Name} ({channel.Id})";
            _payload.PayloadType = PayloadResponseType.ChannelJoin;
            _payload.ChannelId = channel.Id;
            return _payload;
        }

        private Payload GetConfig(Socket client) {
            var result = ServerInfo.ClientIds.FirstOrDefault(x => x.Value == client);
            _payload.Content = result.Key.ToString();
            _payload.PayloadType = PayloadResponseType.Configuration;
            return _payload;
        }
    }
}
