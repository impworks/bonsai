using System;
using System.Threading.Tasks;

namespace Bonsai.Areas.Admin.Logic.MediaHandlers
{
    public class MediaHandlerHelper
    {
        /// <summary>
        /// Runs the media handler in a fire-and-forget manner.
        /// </summary>
        public static void FireAndForget(Func<Task> asyncFunc)
        {
            Task.Run(async () =>
            {
                try
                {
                    await asyncFunc();
                }
                catch
                {
                    // todo: log
                }
            });
        }
    }
}
