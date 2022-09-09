using System.Collections.Generic;

namespace SocketChatApp.Core.Models {
    public class Channel<TMemberType> where TMemberType : class {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<TMemberType> Members { get; set; }
        public byte MaxSlots { get; set; } = 2;
    }
}
