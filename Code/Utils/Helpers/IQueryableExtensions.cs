using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
        public static async Task<HashSet<T>> ToHashSetAsync<T>(this IQueryable<T> source)
        {
            var hash = new HashSet<T>();

            await foreach (var elem in source.AsAsyncEnumerable())
                hash.Add(elem);

            return hash;
        }

        /// <summary>
        /// Saves the query to a hashset.
        /// </summary>
        public static async Task<HashSet<TResult>> ToHashSetAsync<TSource, TResult>(this IQueryable<TSource> source, Func<TSource, TResult> map)
        {
            var hash = new HashSet<TResult>();

            await foreach (var elem in source.AsAsyncEnumerable())
                hash.Add(map(elem));

            return hash;
        }
    }
}
