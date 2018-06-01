// <copyright file="RedirectAttribute.cs" company="dymanoid">
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
    /// A base class for the special redirection attributes that can be applied to the methods.
    /// </summary>
    public abstract class RedirectAttribute : Attribute
    {
        protected RedirectAttribute(Type methodType, string methodName, ulong bitSetRequiredOption)
        {
            MethodType = methodType ?? throw new ArgumentNullException(nameof(methodType));
            MethodName = methodName;
            BitSetRequiredOption = bitSetRequiredOption;
        }

        protected RedirectAttribute(Type methodType, string methodName)
            : this(methodType, methodName, 0)
        {
        }

        protected RedirectAttribute(Type methodType, ulong bitSetOption)
            : this(methodType, null, bitSetOption)
        {
        }

        protected RedirectAttribute(Type methodType)
            : this(methodType, null, 0)
        {
        }

        /// <summary>
        /// Gets or sets the type where the method that will be redirected is defined.
        /// </summary>
        public Type MethodType { get; set; }

        /// <summary>
        /// Gets or sets the method name to redirect. If not set, the name of the
        /// method this attribute is attached to will be used.
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// Gets or sets the required bit set option for the method redirection.
        /// 0 means not specified.
        /// </summary>
        public ulong BitSetRequiredOption { get; set; }
    }
}
