// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ExpressionPowerTools.Core.Extensions;

namespace QueryMutators
{
    /// <summary>
    /// Class that resolves all instances of <see cref="BinaryExpression"/>
    /// that are predicates for the target type.
    /// </summary>
    /// <typeparam name="T">The type to resolve.</typeparam>
    public class BinaryResolvingVisitor<T> : ExpressionVisitor
    {
        /// <summary>
        /// List of filters.
        /// </summary>
        private List<(BinaryExpression binary, int level)> filters;

        /// <summary>
        /// List of compiled filters.
        /// </summary>
        private List<Func<T, bool>> compiled;

        /// <summary>
        /// A value indicating whether the pass is for mutation/resolution.
        /// </summary>
        private bool mutate;

        /// <summary>
        /// A flag to indicate the first predicate function is encountered.
        /// </summary>
        private bool firstPredicate;

        /// <summary>
        /// A flag to indicate the resolution pass is running. This
        /// will compile to literals.
        /// </summary>
        private bool resolve;

        /// <summary>
        /// Property to track the instance being evaluated.
        /// </summary>
        private T instance;

        /// <summary>
        /// Property to track the parameter that references the instance.
        /// </summary>
        private ParameterExpression targetParameter;

        /// <summary>
        /// Tracks the level of the filter.
        /// </summary>
        private int level;

        /// <summary>
        /// Two-passes to parse predicates then intercept the initial
        /// call to hook in for parsing.
        /// </summary>
        /// <param name="node">The root of the expression tree.</param>
        /// <returns>The mutated tree.</returns>
        public Expression ResolveExpressionTree(Expression node)
        {
            mutate = false;
            filters = new List<(BinaryExpression binary, int level)>();
            level = 0;

            // grab predicates
            Visit(node);
            mutate = firstPredicate = true;

            level = 0;

            // intercept predicate entry
            var result = Visit(node);
            mutate = firstPredicate = false;
            level = 0;

            return result;
        }

        /// <summary>
        /// Pass to flatten nodes, so that variables and dates are
        /// transformed to actual values.
        /// </summary>
        /// <param name="node">The node to start with.</param>
        /// <param name="instance">The instance to use for compilation.</param>
        /// <returns>The compiled expression.</returns>
        public Expression CompileNodes(Expression node, T instance)
        {
            this.instance = instance;
            resolve = true;
            level = 0;
            var e = Visit(node);
            level = 0;
            this.instance = default;
            resolve = false;
            return e;
        }

        /// <summary>
        /// If the method doesn't rely on parameters, call it and
        /// swap with result.
        /// </summary>
        /// <param name="node">The <see cref="MethodCallExpression"/>.</param>
        /// <returns>The value.</returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            // resolver mode
            if (resolve)
            {
                // ignore for parameters - could check for "T" here too.
                if (!node.Arguments.SelectMany(a => a.AsEnumerable())
                    .OfType<ParameterExpression>().Any())
                {
                    return Compile(node);
                }
            }

            return base.VisitMethodCall(node);
        }

        /// <summary>
        /// If the member is on the instance, resolve it using the instance
        /// value.
        /// </summary>
        /// <param name="node">The <see cref="MemberExpression"/>.</param>
        /// <returns>The value.</returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            // resolver pass
            if (resolve)
            {
                // check for T
                if (node.Member.DeclaringType == typeof(T))
                {
                    // evaluate the property
                    if (node.Member is PropertyInfo property)
                    {
                        return Expression.Constant(property.GetValue(instance));
                    }
                }

                // if static or doesn't rely on parameters, then it's a go
                if (node.Expression == null ||
                    !node.Expression.AsEnumerable().OfType<ParameterExpression>().Any())
                {
                    return Compile(node);
                }
            }

            return base.VisitMember(node);
        }

        /// <summary>
        /// The <c>Where</c> starts with a predicate. We intercep the
        /// predicate to use it across all expressions, then pass back
        /// control to the original.
        /// </summary>
        /// <typeparam name="TDelegate">The delegate type.</typeparam>
        /// <param name="node">The <see cref="LambdaExpression"/>.</param>
        /// <returns>The mutated expression.</returns>
        protected override Expression VisitLambda<TDelegate>(Expression<TDelegate> node)
        {
            // must be mutation pass, the first predicate and match our pattern
            if (mutate && firstPredicate && node is Expression<Func<T, bool>> predicate)
            {
                firstPredicate = false;
                var operation = predicate.Body;

                // call to evaluation method
                var resolver = GetType().GetMethod(
                    nameof(EvaluateAll), BindingFlags.Instance | BindingFlags.NonPublic);

                var returnTarget = Expression.Label(typeof(bool));

                var innerInvoke = Expression.Return(
                    returnTarget, Expression.Invoke(predicate, predicate.Parameters));

                // call the resolution method, pass the original predicate back
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

        /// <summary>
        /// Capture the <see cref="BinaryExpression"/>.
        /// </summary>
        /// <param name="node">The <see cref="BinaryExpression"/>.</param>
        /// <returns>The expression.</returns>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            // must be first pass and proper predicate type
            if (!mutate && !resolve && node.Type == typeof(bool))
            {
                filters.Add((node, level));
            }

            level++;
            var e = base.VisitBinary(node);
            level--;
            return e;
        }

        /// <summary>
        /// Capture the parameter that refers to the instance.
        /// </summary>
        /// <param name="node">The <see cref="ParameterExpression"/>.</param>
        /// <returns>The expression.</returns>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node.Type == typeof(T))
            {
                // captured
                targetParameter = node;
            }

            return base.VisitParameter(node);
        }

        /// <summary>
        /// Iterates the list of predicates and evaluates them all.
        /// </summary>
        /// <param name="item">The item to evaluate against.</param>
        private void EvaluateAll(T item)
        {
            Console.WriteLine($"Assessing {item}");
            var compile = false;
            for (var idx = 0; idx < filters.Count; idx++)
            {
                if (compiled == null)
                {
                    compile = true;
                    compiled = new List<Func<T, bool>>();
                }

                var (filter, level) = filters[idx];

                Func<T, bool> fn;

                if (compile)
                {
                    var parameter = filter.AsEnumerable().OfType<ParameterExpression>()
                        .First(p => p.Type == typeof(T));
                    var lambda = Expression.Lambda<Func<T, bool>>(
                        filter, parameter);
                    fn = lambda.Compile();
                    compiled.Add(fn);
                }
                else
                {
                    fn = compiled[idx];
                }

                bool result = fn.Invoke(item);
                var resolved = CompileNodes(filter, item);

                if (level > 0)
                {
                    var indent = new string(' ', level);
                    Console.Write(indent);
                }

                Console.Write($"[{filter} ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("=> ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"{resolved} ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(" = ");
                if (result)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("TRUE");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("FALSE");
                }

                Console.ResetColor();
            }

            Console.WriteLine(" --- ");
        }

        /// <summary>
        /// Compiles an expression by invoking it.
        /// </summary>
        /// <param name="node">The node to compile.</param>
        /// <returns>The result.</returns>
        private Expression Compile(Expression node)
        {
            var lambda = Expression.Lambda(node);
            var fn = lambda.Compile();
            var resultType = node.Type;
            if (node is MethodCallExpression method)
            {
                resultType = method.Method.ReturnType;
            }

            return Expression.Constant(
                fn.DynamicInvoke(),
                resultType);
        }
    }
}
