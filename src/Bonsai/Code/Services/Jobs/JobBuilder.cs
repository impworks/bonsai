using System;

namespace Bonsai.Code.Services.Jobs
{
    /// <summary>
    /// Details for starting a new job. 
    /// </summary>
    public class JobBuilder
    {
        /// <summary>
        /// Creates a new JobBuilder for specified job type.
        /// </summary>
        public static JobBuilder For<TJob>() where TJob: IJob => new JobBuilder(typeof(TJob));

        private JobBuilder(Type type)
        {
            Type = type;
        }
        
        public Type Type { get; }
        public object Args { get; private set; }
        public bool IsSuperseding { get; private set; }

        /// <summary>
        /// Adds arguments to the job.
        /// </summary>
        public JobBuilder WithArgs(object args)
        {
            Args = args;
            return this;
        }

        /// <summary>
        /// Terminates all other jobs of this kind before running this one.
        /// </summary>
        public JobBuilder SupersedeAll()
        {
            IsSuperseding = true;
            return this;
        }
    }
}