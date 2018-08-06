using System;
using System.Linq.Expressions;
using AutoMapper;

namespace Bonsai.Code.Utils.Helpers
{
    /// <summary>
    /// Extensions for mapping members.
    /// </summary>
    public static class IMapperExtensions
    {
        /// <summary>
        /// Shorthand method for mapping members.
        /// </summary>
        public static IMappingExpression<TSource, TDest> MapMember<TSource, TDest, TProp>(
            this IMappingExpression<TSource, TDest> map,
            Expression<Func<TDest, TProp>> destMember,
            Expression<Func<TSource, TProp>> sourceMember)
        {
            return map.ForMember(destMember, opt => opt.MapFrom(sourceMember));
        }

        /// <summary>
        /// Shorthand method for mapping members (allows automatic mapping via the user).
        /// </summary>
        public static IMappingExpression<TSource, TDest> MapMemberDynamic<TSource, TDest, TSourceProp, TDestProp>(
            this IMappingExpression<TSource, TDest> map,
            Expression<Func<TDest, TDestProp>> destMember,
            Expression<Func<TSource, TSourceProp>> sourceMember)
        {
            return map.ForMember(destMember, opt => opt.MapFrom(sourceMember));
        }

        /// <summary>
        /// Shorthand method for ignoring members.
        /// </summary>
        public static IMappingExpression<TSource, TDest> Ignore<TSource, TDest, TDestProp>(
            this IMappingExpression<TSource, TDest> map, 
            Expression<Func<TDest, TDestProp>> destMember
        )
        {
            return map.ForMember(destMember, opt => opt.Ignore());
        }
    }
}
