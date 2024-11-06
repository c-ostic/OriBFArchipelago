using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OriBFArchipelago.Core
{
    // Maintain game states that doesn't need to be saved
    internal class LocalGameState
    {
        public static bool IsGinsoExit = false;

        public static void Reset()
        {
            IsGinsoExit = false;
        }
    }
}
