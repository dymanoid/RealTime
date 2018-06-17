// <copyright file="MethodInfoExtensions.cs" company="dymanoid">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Security.Permissions;

    /// <summary>
    /// Contains various extension methods for the <see cref="MethodInfo"/> class.
    /// </summary>
    internal static class MethodInfoExtensions
    {
        /// <summary>
        /// Creates a <see cref="MethodRedirection"/> instance for the <paramref name="method"/>
        /// using the target <paramref name="target"/> and the <paramref name="redirectionSource"/>
        /// assembly and redirects the calls from <paramref name="method"/> to the <paramref name="target"/>.
        /// </summary>
        ///
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        ///
        /// <param name="method">A method to create the redirection for.</param>
        /// <param name="target">The target method for the redirection.</param>
        /// <param name="redirectionSource">An <see cref="Assembly"/> that is the redirection source.</param>
        ///
        /// <returns>A <see cref="MethodRedirection"/> instance containing the redirected method description.</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal static MethodRedirection CreateRedirectionTo(this MethodInfo method, MethodInfo target, Assembly redirectionSource)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (redirectionSource == null)
            {
                throw new ArgumentNullException(nameof(redirectionSource));
            }

            return new MethodRedirection(method, target, redirectionSource);
        }

        /// <summary>
        /// Compares the provided methods to determine whether a redirection from <paramref name="otherMethod"/>
        /// to this method is possible.
        /// </summary>
        ///
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        ///
        /// <param name="method">The method to compare.</param>
        /// <param name="otherMethod">The method to compare with.</param>
        /// <param name="asInstanceMethod">if true, a static method with first argument that is
        /// compatible with the type of the <paramref name="otherMethod"/> will be considered
        /// compatible.</param>
        ///
        /// <returns>True if the methods are compatible; otherwise, false.</returns>
        internal static bool IsCompatibleWith(this MethodInfo method, MethodInfo otherMethod, bool asInstanceMethod)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            if (otherMethod == null)
            {
                throw new ArgumentNullException(nameof(otherMethod));
            }

            if (method.ReturnType != otherMethod.ReturnType)
            {
                return false;
            }

            var thisParameters = method.GetParameters().ToList();
            var otherParameters = otherMethod.GetParameters().ToList();
            if (asInstanceMethod)
            {
                RemoveInstanceParameter(thisParameters, otherMethod.ReflectedType);
                RemoveInstanceParameter(otherParameters, method.ReflectedType);
            }

            if (thisParameters.Count != otherParameters.Count)
            {
                return false;
            }

            for (int i = 0; i < thisParameters.Count; i++)
            {
                if (!otherParameters[i].ParameterType.IsAssignableFrom(thisParameters[i].ParameterType))
                {
                    return false;
                }
            }

            return true;
        }

        private static void RemoveInstanceParameter(List<ParameterInfo> parameters, Type methodClass)
        {
            ParameterInfo firstParameter = parameters.FirstOrDefault();

            if (firstParameter == null)
            {
                return;
            }

            if (methodClass.IsValueType)
            {
                if (firstParameter.ParameterType != methodClass.MakeByRefType())
                {
                    return;
                }
            }
            else if (!methodClass.IsAssignableFrom(firstParameter.ParameterType))
            {
                return;
            }

            parameters.RemoveAt(0);
        }
    }
}