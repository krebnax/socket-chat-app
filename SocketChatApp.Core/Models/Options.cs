namespace SocketChatApp.Core.Models {
    public enum ContentAccess {
        Private = 0,
        Public = 1000
    }

    public enum PayloadFormat {
        Json = 0,
        Xml = 1000,
        Plain = 2000
    }

    public enum PayloadResponseType {
        Response = 0,
        Configuration = 1000,
        ChannelJoin = 2000
    }
}
