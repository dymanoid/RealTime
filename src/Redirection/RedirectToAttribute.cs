// <copyright file="RedirectToAttribute.cs" company="dymanoid">
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
    /// <remarks>NOTE: only the methods belonging to the same assembly that calls Perform/RevertRedirections are redirected.</remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class RedirectToAttribute : RedirectAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToAttribute"/> class.</summary>
        ///
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="methodType"/> is null.</exception>
        ///
        /// <param name="methodType">The type where the method that will be redirected is defined.</param>
        /// <param name="methodName">The name of the target method. If null,
        /// the name of the attribute's target method will be used.</param>
        /// <param name="bitSetRequiredOption">The required bit set option.</param>
        public RedirectToAttribute(Type methodType, string methodName, ulong bitSetRequiredOption)
            : base(methodType, methodName, bitSetRequiredOption)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToAttribute"/> class with
        /// default <see cref="RedirectAttribute.BitSetRequiredOption"/>.</summary>
        ///
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="methodType"/> is null.</exception>
        ///
        /// <param name="methodType">The type where the method that will be redirected is defined.</param>
        /// <param name="methodName">The name of the target method. If null,
        /// the name of the attribute's target method will be used.</param>
        public RedirectToAttribute(Type methodType, string methodName)
            : base(methodType, methodName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToAttribute"/> class with empty
        /// <see cref="RedirectAttribute.MethodName"/>. The name of the method this attribute is
        /// attached to will be used.</summary>
        ///
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="methodType"/> is null.</exception>
        ///
        /// <param name="methodType">The type where the method that will be redirected is defined.</param>
        /// <param name="bitSetRequiredOption">The required bit set option.</param>
        public RedirectToAttribute(Type methodType, ulong bitSetRequiredOption)
            : base(methodType, bitSetRequiredOption)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToAttribute"/> class with
        /// default <see cref="RedirectAttribute.BitSetRequiredOption"/> and empty
        /// <see cref="RedirectAttribute.MethodName"/>. The name of the method this attribute is
        /// attached to will be used.</summary>
        ///
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="methodType"/> is null.</exception>
        ///
        /// <param name="methodType">The type where the method that will be redirected is defined.</param>
        public RedirectToAttribute(Type methodType)
            : base(methodType)
        {
        }
    }
}
