using System;
using System.Linq.Expressions;

internal static class NetstandardCompatibility
{
    public static Expression<Func<T1, T2, bool>> CombineIf<T1, T2>(
        this Expression<Func<T1, T2, bool>> lhs,
        bool condition,
        Expression<Func<T1, T2, bool>> expression)
    {
        if (!condition) return lhs;
        return lhs.Combine(expression);
    }

    public static Expression<Func<T, bool>> CombineIf<T>(
        this Expression<Func<T, bool>> lhs,
        bool condition,
        Expression<Func<T, bool>> expression)
    {
        if (!condition) return lhs;
        return lhs.Combine(expression);
    }
}

internal static class Expr
{
    public static Expression<Func<T, bool>> Create<T>(
        Expression<Func<T, bool>> expression)
    {
        return expression;
    }

    public static Expression<Func<T1, T2, bool>> Create<T1, T2>(
        Expression<Func<T1, T2, bool>> expression)
    {
        return expression;
    }
}