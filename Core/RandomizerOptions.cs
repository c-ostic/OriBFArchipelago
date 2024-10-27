using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OriBFArchipelago.Core
{
    internal class RandomizerOptions
    {
        public bool UseLocalKeystones { get; }
        public RandomizerOptions(Dictionary<string, object> apSlotData) {
            if (apSlotData != null)
            {
                UseLocalKeystones = Convert.ToInt32(RandomizerManager.Connection.SlotData["local_keystones"]) == 1;
            }
        }
    }
}
