using StruLog.Entites.Stores;
using StruLog.SM;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace StruLog.Entites
{
    internal class Config
    {
        internal ImmutableList<StoreManager> usingStores;
        internal TimeRepresentation time;
        internal StoresSettings stores;
        internal StoreManager insideLoggingStore;
    }
}
