using System;
using System.Collections.Generic;
using System.Text;

namespace StruLog.Entites
{
    internal abstract class HandlingIntensivity
    {

    }
    /// <summary>
    /// Для фиксации данных за промежуток времени
    /// </summary>
    internal abstract class PeriodicHandlingIntensivity : HandlingIntensivity
    {
        internal TimeSpan PeriodLength { get; set; }
        /// <summary>
        /// Конец интервала
        /// </summary>
        internal DateTime EndTime { get; set; }
        internal virtual void TryMovePeriod()
        {
            EndTime = DateTime.UtcNow + PeriodLength;
        }

    }
    internal class TelegramHandlingIntensivity : PeriodicHandlingIntensivity
    {
        internal uint nLogEntriesPerPeriod { get; set; } = 0;

        internal override void TryMovePeriod()
        {

            if (EndTime < DateTime.UtcNow)
            {
                base.TryMovePeriod();
                nLogEntriesPerPeriod = 0;
            }
        }
    }
}
