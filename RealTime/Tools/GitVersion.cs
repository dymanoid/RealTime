// <copyright file="GitVersion.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Tools
{
    using System;
    using System.Reflection;

    internal static class GitVersion
    {
        private const string GitVersionTypeName = ".GitVersionInformation";
        private const string VersionFieldName = "SemVer";

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
