using StruLog.Entites.Stores;
using StruLog.SM;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace StruLog.Entites
{
    internal class Config
    {
        internal ImmutableList<StoreManager> usingStores;
        /// <summary>
        /// by config params
        /// </summary>
        internal Func<DateTime> currentTime_Func;
        internal StoreSettings stores;
        internal StoreManager insideLoggingStore;
        /// <summary>
        /// Special project name for logger operations
        /// </summary>
        internal string projectName;
    }
}
