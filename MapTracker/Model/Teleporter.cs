namespace OriBFArchipelago.MapTracker.Model
{
    internal class Teleporter
    {
        public MoonGuid Guid { get; set; }
        public string GameIdentifier { get; set; }
        public string LogicIdentifier { get; set; }
        public bool IsActivaded { get; set; }
        public string FriendlyName { get; set; }
        public Teleporter(MoonGuid guid, string gameIdentifier, string logicIdentifier, string friendlyName, bool isActivated = false)
        {
            Guid = guid;
            GameIdentifier = gameIdentifier;
            LogicIdentifier = logicIdentifier;
            IsActivaded = isActivated;
            FriendlyName = friendlyName ?? "Unknown teleporter";
        }

        public override string ToString()
        {
            return $"{GameIdentifier} - {LogicIdentifier} - {FriendlyName}";
        }
    }
}
