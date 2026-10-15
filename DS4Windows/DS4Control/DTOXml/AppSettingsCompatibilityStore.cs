/*
DS4Windows
Copyright (C) 2026 hbashton
SPDX-License-Identifier: GPL-3.0-or-later
*/

using System;
using System.IO;
using System.Xml.Serialization;

namespace DS4Windows;

/// <summary>
/// Keeps settings introduced by newer builds when an older build rewrites
/// Profiles.xml and drops elements it does not know about.
/// </summary>
internal static class AppSettingsCompatibilityStore
{
    private static string GetPath(string profilePath) => profilePath + ".compat";

    public static bool? LoadSunshineControllerBridgeEnabled(string profilePath)
    {
        string path = GetPath(profilePath);
        if (!File.Exists(path)) return null;

        try
        {
            using FileStream stream = File.OpenRead(path);
            var dto = new XmlSerializer(typeof(CompatibilitySettings))
                .Deserialize(stream) as CompatibilitySettings;
            return dto?.SunshineControllerBridgeEnabled;
        }
        catch (Exception error)
        {
            AppLogger.LogToGui(
                $"Could not read DS4Windows compatibility settings: {error.Message}",
                false);
            return null;
        }
    }

    public static bool SaveSunshineControllerBridgeEnabled(string profilePath,
        bool enabled)
    {
        string path = GetPath(profilePath);
        string tempPath = path + ".tmp";

        try
        {
            using (FileStream stream = new FileStream(tempPath,
                FileMode.Create, FileAccess.Write, FileShare.None))
            {
                new XmlSerializer(typeof(CompatibilitySettings)).Serialize(
                    stream, new CompatibilitySettings
                    {
                        SunshineControllerBridgeEnabled = enabled,
                    });
            }

            File.Move(tempPath, path, overwrite: true);
            return true;
        }
        catch (Exception)
        {
            try { File.Delete(tempPath); }
            catch { }
            return false;
        }
    }

    [XmlRoot("AppSettingsCompatibility")]
    public sealed class CompatibilitySettings
    {
        [XmlElement("SunshineControllerBridgeEnabled")]
        public bool? SunshineControllerBridgeEnabled { get; set; }
    }
}
