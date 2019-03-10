using System;

namespace Bonsai.Areas.Admin.Logic.Workers
{
    /// <summary>
    /// Messenger service for waking up workers.
    /// </summary>
    public class WorkerAlarmService
    {
        /// <summary>
        /// Event indicating a new encoder job is pending.
        /// </summary>
        public event EventHandler OnNewEncoderJob = delegate { };

        /// <summary>
        /// Fires the OnNewEncoderJob event.
        /// </summary>
        public void FireNewEncoderJob()
        {
            OnNewEncoderJob(this, new EventArgs());
        }

        /// <summary>
        /// Event indicating a tree update.
        /// </summary>
        public event EventHandler OnTreeLayoutRegenerationRequired = delegate { };

        /// <summary>
        /// Fires the OnNewEncoderJob event.
        /// </summary>
        public void FireTreeLayoutRegenerationRequired()
        {
            OnTreeLayoutRegenerationRequired(this, new EventArgs());
        }
    }
}
