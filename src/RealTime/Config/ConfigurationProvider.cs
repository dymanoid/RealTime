// <copyright file="ConfigurationProvider.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.Config
{
    internal static class ConfigurationProvider
    {
        public static Configuration LoadConfiguration()
        {
            return new Configuration();
        }

        public static void SaveConfiguration(Configuration config)
        {
        }
    }
}
