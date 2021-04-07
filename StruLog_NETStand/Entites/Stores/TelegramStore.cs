﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace StruLog.Entites.Stores
{
    /// <summary>
    /// Настройки телеграма
    /// </summary>
    internal class TelegramStore : Store
    {
        internal string token;
        internal ImmutableList<long> chatIds;
        internal IntensivityControl intensivity;

        internal class IntensivityControl 
        {
            internal bool enable;
            internal long nMessagesPerPeriod;
            internal TimeSpan period;
        }
    }
}
