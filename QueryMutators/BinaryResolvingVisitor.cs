using ExpressionPowerTools.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace QueryMutators
{
    public class BinaryResolvingVisitor<T> : ExpressionVisitor
    {
        private List<BinaryExpression> filters;
        private bool mutate;
        private bool firstPredicate;
        private bool resolve;
        private T instance;
        private ParameterExpression targetParameter;

        public Expression ResolveExpressionTree(Expression node)
        {
            mutate = false;
            filters = new List<BinaryExpression>();
            Visit(node);
            mutate = firstPredicate = true;
            var result = Visit(node);
            mutate = firstPredicate = false;
            return result;
        }

        public Expression CompileNodes(Expression node, T instance)
        {
            this.instance = instance;
            resolve = true;
            var e = Visit(node);
            this.instance = default;
            return e;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (resolve)
            {
                if (!node.Arguments.SelectMany(a => a.AsEnumerable())
                    .OfType<ParameterExpression>().Any())
                {
                    var lambda = Expression.Lambda(node);
                    var fn = lambda.Compile();
                    return Expression.Constant(
                        fn.DynamicInvoke(), 
                        node.Method.ReturnType);
                }
            }
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (resolve)
            {
                if (node.Member.DeclaringType == typeof(T))
                {
                    if (node.Member is PropertyInfo property)
                    {
                        return Expression.Constant(property.GetValue(instance));
                    }
                }

                if (node.Expression == null ||
                    !node.Expression.AsEnumerable().OfType<ParameterExpression>().Any())
                {
                    var lambda = Expression.Lambda(node);
                    var fn = lambda.Compile();
                    return Expression.Constant(fn.DynamicInvoke());
                }
            }

            return base.VisitMember(node);
        }

        protected override Expression VisitLambda<TDelegate>(Expression<TDelegate> node)
        {
            if (mutate && firstPredicate && node is Expression<Func<T, bool>> predicate)
            {
                firstPredicate = false;
                var operation = predicate.Body;
                var resolver = GetType().GetMethod(
                    nameof(EvaluateAll), BindingFlags.Instance | BindingFlags.NonPublic);
                var returnTarget = Expression.Label(typeof(bool));

                var innerInvoke = Expression.Return(
                    returnTarget, Expression.Invoke(predicate, predicate.Parameters));

                var expr = Expression.Block(
                    Expression.Call(
                        this.AsConstantExpression(), 
                        resolver, 
                        node.Parameters),
                    innerInvoke,
                    Expression.Label(returnTarget, Expression.Constant(false)));

                return Expression.Lambda<Func<T, bool>>(
                    expr,
                    node.Parameters);
            }

            return base.VisitLambda(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (!mutate && !resolve && node.Type == typeof(bool))
            {
                filters.Add(node);
            }
            return base.VisitBinary(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node.Type == typeof(T))
            {
                targetParameter = node;
            }
            return base.VisitParameter(node);
        }

        private void EvaluateAll(T item)
        {
            foreach (var filter in filters)
            {
                var resolved = CompileNodes(filter, item);
                var parameter = filter.AsEnumerable().OfType<ParameterExpression>()
                    .First(p => p.Type == typeof(T));
                var lambda = Expression.Lambda<Func<T, bool>>(
                    filter, parameter);
                var fn = lambda.Compile();
                bool result = fn.Invoke(item);
                Console.WriteLine($"[{filter}] => ({resolved}) = {result}");
            }
        }
    }
}
