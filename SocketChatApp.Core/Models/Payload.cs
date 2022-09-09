using System;

namespace SocketChatApp.Core.Models {
    [Serializable]
    public class Payload {
        public ContentAccess? ContentAccess { get; set; }
        public string SenderId { get; set; }
        public string ChannelId { get; set; }
        public string Content { get; set; }
        public PayloadFormat Format { get; set; }
        public PayloadResponseType? PayloadType { get; set; } = PayloadResponseType.Response;
        public bool CanBypassGuard { get; set; } = false;
    }
}
