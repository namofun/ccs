using System;
using System.Linq;
using System.Linq.Expressions;
using Xylab.Contesting.Entities;

namespace Xylab.Contesting.Models
{
    internal class Aggregator<TKey, TValue>
    {
        public TKey Key { get; }

        public TValue Value { get; }

        public Aggregator(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public static Expression<Func<IGrouping<TKey, Team>, Aggregator<TKey, TValue>>> CreateExpression(Expression<Func<IGrouping<TKey, Team>, TValue>> raw)
        {
            var param = raw.Parameters[0];
            var body = Expression.New(
                typeof(Aggregator<TKey, TValue>).GetConstructors()[0],
                Expression.Property(param, nameof(IGrouping<TKey, Team>.Key)),
                raw.Body);
            return Expression.Lambda<Func<IGrouping<TKey, Team>, Aggregator<TKey, TValue>>>(body, param);
        }
    }
}
