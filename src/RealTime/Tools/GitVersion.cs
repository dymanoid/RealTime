// <copyright file="GitVersion.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Tools
{
    using System;
    using System.Reflection;

    /// <summary>
    /// A helper class that interacts with the special types injected by the GitVersion toolset.
    /// </summary>
    internal static class GitVersion
    {
        private const string GitVersionTypeName = ".GitVersionInformation";
        private const string VersionFieldName = "FullSemVer";

        /// <summary>
        /// Gets a string representation of the full semantic assembly version of the specified <paramref name="assembly"/>.
        /// This assembly should be built using the GitVersion toolset; otherwise, a "?" version string will
        /// be returned.
        /// </summary>
        ///
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        ///
        /// <param name="assembly">An <see cref="Assembly"/> to get the version of. Should be built using the GitVersion toolset.</param>
        ///
        /// <returns>A string representation of the full semantic version of the specified <paramref name="assembly"/>,
        /// or "?" if the version could not be determined.</returns>
        public static string GetAssemblyVersion(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            Type gitVersionInformationType = assembly.GetType(assembly.GetName().Name + GitVersionTypeName);
            if (gitVersionInformationType == null)
            {
                Log.Error("Attempting to retrieve the assembly version of an assembly that is built without GitVersion support.");
                return "?";
            }

            FieldInfo versionField = gitVersionInformationType.GetField(VersionFieldName);
            if (versionField == null)
            {
                Log.Error($"Internal error: the '{GitVersionTypeName}' type has no field '{VersionFieldName}'.");
                return "?";
            }

            string version = versionField.GetValue(null) as string;
            if (string.IsNullOrEmpty(version))
            {
                Log.Warning($"The '{GitVersionTypeName}.{VersionFieldName}' value is empty.");
                return "?";
            }

            return version;
        }
    }
}
