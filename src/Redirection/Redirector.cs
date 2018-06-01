// <copyright file="Redirector.cs" company="dymanoid">
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

    public static class Redirector
    {
        private static Dictionary<MethodInfo, MethodRedirection> redirections = new Dictionary<MethodInfo, MethodRedirection>();

        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public static void PerformRedirections()
        {
            PerformRedirections(0);
        }

        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public static void PerformRedirections(ulong bitmask)
        {
            var callingAssembly = Assembly.GetCallingAssembly();

            IEnumerable<MethodInfo> allMethods = callingAssembly
                .GetTypes()
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));

            foreach (MethodInfo method in allMethods)
            {
                foreach (RedirectAttribute attribute in method.GetCustomAttributes(typeof(RedirectAttribute), false))
                {
                    ProcessMethod(method, callingAssembly, attribute, bitmask);
                }
            }
        }

        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public static void RevertRedirections()
        {
            var callingAssembly = Assembly.GetCallingAssembly();

            var redirectionsToRemove = redirections.Values.Where(r => r.RedirectionSource == callingAssembly).ToList();
            foreach (MethodRedirection item in redirectionsToRemove)
            {
                redirections.Remove(item.Method);
                item.Dispose();
            }
        }

        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        private static void ProcessMethod(MethodInfo method, Assembly callingAssembly, RedirectAttribute attribute, ulong bitmask)
        {
            if (attribute.BitSetRequiredOption != 0 && (bitmask & attribute.BitSetRequiredOption) == 0)
            {
                return;
            }

            string originalName = string.IsNullOrEmpty(attribute.MethodName) ? method.Name : attribute.MethodName;

            MethodInfo originalMethod =
                attribute.ClassType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == originalName && method.IsCompatibleWith(m));

            if (originalMethod == null)
            {
                throw new InvalidOperationException($"Redirector: Original method {originalName} has not been found for redirection");
            }

            if (attribute is RedirectFromAttribute)
            {
                if (!redirections.ContainsKey(originalMethod))
                {
                    redirections.Add(originalMethod, originalMethod.CreateRedirectionTo(method, callingAssembly));
                }

                return;
            }

            if (attribute is RedirectToAttribute)
            {
                if (!redirections.ContainsKey(method))
                {
                    redirections.Add(method, method.CreateRedirectionTo(originalMethod, callingAssembly));
                }
            }
        }
    }
}
