using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Bonsai.Code.Services.Jobs
{
    public class JobDescriptor: IDisposable
    {
        public IJob Job { get; set; }
        public object Arguments { get; set; }
        
        public string ResourceKey { get; set; }
        
        public CancellationTokenSource Cancellation { get; set; }
        public Guid JobStateId { get; set; }

        public IServiceScope Scope { get; set; }

        public void Dispose()
        {
            Scope?.Dispose();
            Scope = null;
        }

        public override string ToString()
        {
            return $"{JobStateId} ({Job?.GetType().Name ?? "Unknown"})";
        }
    }
}