using System.Collections.Generic;

namespace OriBFArchipelago.MapTracker.Model
{
    internal class ActionSequenceExtension
    {
        internal MoonGuid Guid { get; set; }
        internal string Name { get; set; }
        internal string Description { get; set; }
        internal List<int> ActionsToKeep { get; set; }

        public ActionSequenceExtension(string name, string description, int[] actionsToKeep, MoonGuid guid)
        {
            Guid = guid;
            Name = name;
            Description = description;
            ActionsToKeep = [..actionsToKeep];
        }
    }
}
