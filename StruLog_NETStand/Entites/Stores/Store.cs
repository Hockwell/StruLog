using System;
using System.Collections.Generic;

namespace StruLog.Entites.Stores
{
    internal abstract class Store
    {
        /// <summary>
        /// Это не исходная строка конфига, а уже набор действий для вывода
        /// </summary>
        internal object outputPattern;
        internal LogLevel minLogLevel;
    }
}
