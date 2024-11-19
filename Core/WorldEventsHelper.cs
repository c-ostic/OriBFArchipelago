using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OriBFArchipelago.Core
{
    internal class WorldEventsHelper
    {
        public static Dictionary<MoonGuid, WorldEventsRuntime> GameEventsRuntime = null;

        public static WorldEventsRuntime GinsoWorldEvents
        {
            get
            {
                if (GameEventsRuntime != null && GameEventsRuntime.TryGetValue(ginsoEventsGuid, out WorldEventsRuntime ginsoEventsRuntime))
                {
                    return ginsoEventsRuntime;
                }
                return null;
            }
        }

        public static WorldEventsRuntime MistyWorldEvents
        {
            get
            {
                if (GameEventsRuntime != null && GameEventsRuntime.TryGetValue(mistyEventsGuid, out WorldEventsRuntime mistyEventsRuntime))
                {
                    return mistyEventsRuntime;
                }
                return null;
            }
        }

        private static MoonGuid ginsoEventsGuid = new MoonGuid(687998245, 1199897005, -1787166542, 576748618);
        private static MoonGuid mistyEventsGuid = new MoonGuid(1061758509, 1206015992, 824243626, -2026069462);

    }
}
