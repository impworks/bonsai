using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Code.Utils.Helpers
{
    /// <summary>
    /// Extensions for the IQueryable interface.
    /// </summary>
    public static class IQueryableExtensions
    {
        /// <summary>
        /// Finds the object by a predicate or throws an OperationException.
        /// </summary>
        public static async Task<T> GetAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, string error)
            where T: class
        {
            var result = await source.FirstOrDefaultAsync(predicate)
                                     .ConfigureAwait(false);

            if(result == null)
                throw new OperationException(error);

            return result;
        }
    }
}
