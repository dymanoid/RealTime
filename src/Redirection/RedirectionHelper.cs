// <copyright file="RedirectionHelper.cs" company="dymanoid">
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

    /// <summary>
    /// Helper class to deal with detours. This version is for Unity 5 x64 on Windows.
    /// We provide three different methods of detouring.
    /// </summary>
    internal static class RedirectionHelper
    {
        /// <summary>
        /// Redirects all calls from method '<paramref name="from"/>' to method '<paramref name="to"/>'.
        /// </summary>
        ///
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        ///
        /// <param name="from">The method to redirect from.</param>
        /// <param name="to">The method to redicrect to.</param>
        ///
        /// <returns>An <see cref="RedirectCallsState"/> instance that holds the data for reverting
        /// the redirection.</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public static RedirectCallsState RedirectCalls(MethodBase from, MethodBase to)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (to == null)
            {
                throw new ArgumentNullException(nameof(to));
            }

            // GetFunctionPointer enforces compilation of the method.
            IntPtr fptr1 = from.MethodHandle.GetFunctionPointer();
            IntPtr fptr2 = to.MethodHandle.GetFunctionPointer();

            return PatchJumpTo(fptr1, fptr2);
        }

        /// <summary>
        /// Redirects all calls from method '<paramref name="from"/>' to method '<paramref name="to"/>'.
        /// </summary>
        ///
        /// <param name="from">The method to redirect from.</param>
        /// <param name="to">The method to redicrect to.</param>
        ///
        /// <returns>An <see cref="RedirectCallsState"/> instance that holds the data for reverting
        /// the redirection.</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public static RedirectCallsState RedirectCalls(RuntimeMethodHandle from, RuntimeMethodHandle to)
        {
            // GetFunctionPointer enforces compilation of the method.
            IntPtr fptr1 = from.GetFunctionPointer();
            IntPtr fptr2 = to.GetFunctionPointer();

            return PatchJumpTo(fptr1, fptr2);
        }

        /// <summary>
        /// Reverts a method redirection previously created with <see cref="RedirectCalls(MethodBase, MethodBase)"/>
        /// or <see cref="RedirectCalls(RuntimeMethodHandle, RuntimeMethodHandle)"/> methods.
        /// </summary>
        ///
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        ///
        /// <param name="method">The method to revert the redirection of.</param>
        /// <param name="state">A <see cref="RedirectCallsState"/> instance holding the data for
        /// reverting the redirection.</param>
        ///
        /// <returns>True on success; otherwise, false.</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public static bool RevertRedirect(MethodBase method, RedirectCallsState state)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            try
            {
                IntPtr fptr1 = method.MethodHandle.GetFunctionPointer();
                RevertJumpTo(fptr1, state);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Primitive patching. Inserts a jump to '<paramref name="target"/>' at '<paramref name="site"/>'.
        /// Works even if both methods' callers have already been compiled.
        /// </summary>
        ///
        /// <param name="site">A pointer of the jump site.</param>
        /// <param name="target">A pointer to the jump target.</param>
        ///
        /// <returns>An <see cref="RedirectCallsState"/> instance that holds the data for reverting
        /// the redirection.</returns>
        private static RedirectCallsState PatchJumpTo(IntPtr site, IntPtr target)
        {
            var state = new RedirectCallsState();

            // R11 is volatile.
            unsafe
            {
                byte* sitePtr = (byte*)site.ToPointer();
                state.CallSite = *sitePtr;
                state.Offset1 = *(sitePtr + 1);
                state.Offset10 = *(sitePtr + 10);
                state.Offset11 = *(sitePtr + 11);
                state.Offset12 = *(sitePtr + 12);
                state.Addr = *((ulong*)(sitePtr + 2));

                *sitePtr = 0x49; // mov r11, target
                *(sitePtr + 1) = 0xBB;
                *((ulong*)(sitePtr + 2)) = (ulong)target.ToInt64();
                *(sitePtr + 10) = 0x41; // jmp r11
                *(sitePtr + 11) = 0xFF;
                *(sitePtr + 12) = 0xE3;
            }

            return state;
        }

        private static void RevertJumpTo(IntPtr site, RedirectCallsState state)
        {
            unsafe
            {
                byte* sitePtr = (byte*)site.ToPointer();
                *sitePtr = state.CallSite; // mov r11, target
                *(sitePtr + 1) = state.Offset1;
                *((ulong*)(sitePtr + 2)) = state.Addr;
                *(sitePtr + 10) = state.Offset10; // jmp r11
                *(sitePtr + 11) = state.Offset11;
                *(sitePtr + 12) = state.Offset12;
            }
        }
    }
}