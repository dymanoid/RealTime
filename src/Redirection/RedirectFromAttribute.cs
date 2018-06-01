// <copyright file="RedirectFromAttribute.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

/*
The MIT License (MIT)
Copyright (c) 2015 Sebastian Schöner
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

namespace Redirection
{
    using System;

    /// <summary>
    /// Marks a method for redirection. All marked methods are redirected by calling
    /// <see cref="Redirector.PerformRedirections"/> and reverted by <see cref="Redirector.RevertRedirections"/>
    /// </summary>
    ///
    /// <remarks>
    /// NOTE: only the methods belonging to the same assembly that calls Perform/RevertRedirections are redirected.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class RedirectFromAttribute : RedirectAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectFromAttribute"/> class.</summary>
        ///
        /// <param name="classType">The class of the method that will be redirected</param>
        /// <param name="methodName">The name of the method that will be redirected. If null,
        /// the name of the attribute's target method will be used.</param>
        /// <param name="bitSetRequiredOption">The required bit set option.</param>
        public RedirectFromAttribute(Type classType, string methodName, ulong bitSetRequiredOption)
            : base(classType, methodName, bitSetRequiredOption)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectFromAttribute"/> class.</summary>
        ///
        /// <param name="classType">The class of the method that will be redirected</param>
        /// <param name="methodName">The name of the method that will be redirected. If null,
        /// the name of the attribute's target method will be used.</param>
        public RedirectFromAttribute(Type classType, string methodName)
            : base(classType, methodName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectFromAttribute"/> class.</summary>
        ///
        /// <param name="classType">The class of the method that will be redirected</param>
        /// <param name="bitSetRequiredOption">The required bit set option.</param>
        public RedirectFromAttribute(Type classType, ulong bitSetRequiredOption)
            : base(classType, bitSetRequiredOption)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectFromAttribute"/> class.</summary>
        ///
        /// <param name="classType">The class of the method that will be redirected</param>
        public RedirectFromAttribute(Type classType)
            : base(classType)
        {
        }
    }
}
