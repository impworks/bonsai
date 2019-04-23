using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions.Internal;

namespace Bonsai.Code.Utils.Helpers
{
    /// <summary>
    /// Extensions for the IQueryable interface.
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        /// Finds the object by a predicate or throws an OperationException.
        /// </summary>
        public static async Task<T> GetAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, string error)
            where T: class
        {
            var result = await source.FirstOrDefaultAsync(predicate);

            if(result == null)
                throw new OperationException(error);

            return result;
        }

        /// <summary>
        /// Removes all objects matching a predicate.
        /// </summary>
        public static async Task RemoveWhereAsync<T>(this DbSet<T> source, Expression<Func<T, bool>> predicate)
            where T : class
        {
            var existing = await source.Where(predicate)
                                       .ToListAsync();

            source.RemoveRange(existing);
        }

        /// <summary>
        /// Saves the query to a hashset.
        /// </summary>
        public static Task<HashSet<T>> ToHashSetAsync<T>(this IQueryable<T> source)
        {
            return source.AsAsyncEnumerable()
                         .Aggregate(
                             new HashSet<T>(),
                             (acc, x) =>
                             {
                                 acc.Add(x);
                                 return acc;
                             }
                         );
        }

        /// <summary>
        /// Saves the query to a hashset.
        /// </summary>
        public static Task<HashSet<TResult>> ToHashSetAsync<TSource, TResult>(this IQueryable<TSource> source, Func<TSource, TResult> map)
        {
            return source.AsAsyncEnumerable()
                         .Aggregate(
                             new HashSet<TResult>(),
                             (acc, x) =>
                             {
                                 acc.Add(map(x));
                                 return acc;
                             }
                         );
        }
    }
}
