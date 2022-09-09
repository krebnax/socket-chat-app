namespace Server.CmdApp.Models {
    internal class SpamGuardModel {
        public string ClientId { get; set; }
        public long LastTick { get; set; }
        public bool HasWarning { get; set; }
    }
}
