// <copyright file="MethodRedirection.cs" company="dymanoid">
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
    using System.Reflection;
    using System.Security.Permissions;

    internal sealed class MethodRedirection : IDisposable
    {
        private readonly RedirectCallsState callsState;

        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public MethodRedirection(MethodInfo method, MethodInfo target, Assembly redirectionSource)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            Method = method ?? throw new ArgumentNullException(nameof(method));
            callsState = RedirectionHelper.RedirectCalls(method, target);
            RedirectionSource = redirectionSource ?? throw new ArgumentNullException(nameof(redirectionSource));
        }

        public MethodInfo Method { get; private set; }

        public Assembly RedirectionSource { get; private set; }

        public bool IsDisposed { get; set; }

        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            RedirectionHelper.RevertRedirect(Method, callsState);
            Method = null;
            RedirectionSource = null;
            IsDisposed = true;
        }
    }
}
