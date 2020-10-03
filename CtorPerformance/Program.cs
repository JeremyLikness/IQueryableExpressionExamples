// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using BenchmarkDotNet.Running;

namespace CtorPerformance
{
    /// <summary>
    /// Main benchmark program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main program loop.
        /// </summary>
        /// <param name="args">Ignored arguments.</param>
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<CtorTest>();
        }
    }
}
