// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Community.PowerToys.Run.Plugin.VisualStudio.Components;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.VisualStudio.Helpers
{
    public class VisualStudioService
    {
        private const string VsWhereDir = @"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer";
        private const string VsWhereBin = "vswhere.exe";
        private const string VisualStudioDataDir = @"%LOCALAPPDATA%\Microsoft\VisualStudio";

        private ReadOnlyCollection<VisualStudioInstance>? _instances;

        public VisualStudioService()
        {
        }

        public void InitInstances()
        {
            var paths = new string?[] { null, VsWhereDir };
            var exceptions = new List<(string? Path, Exception Exception)>(paths.Length);

            foreach (var path in paths)
            {
                try
                {
                    var vsWherePath = VsWhereBin;

                    if (path != null)
                    {
                        vsWherePath = Path.Combine(path, VsWhereBin);
                    }

                    vsWherePath = Environment.ExpandEnvironmentVariables(vsWherePath);

                    var startInfo = new ProcessStartInfo(vsWherePath, "-all -prerelease -format json")
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                    };

                    using var process = Process.Start(startInfo);
                    if (process == null)
                    {
                        continue;
                    }

                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(TimeSpan.FromSeconds(5));
                    if (string.IsNullOrWhiteSpace(output))
                    {
                        continue;
                    }

                    var instancesJson = JsonSerializer.Deserialize<List<Json.VisualStudioInstance>>(output);
                    if (instancesJson == null)
                    {
                        continue;
                    }

                    var instances = new List<VisualStudioInstance>(instancesJson.Count);
                    foreach (var instance in instancesJson)
                    {
                        var applicationPrivateSettingsPath = GetApplicationPrivateSettingsPathByInstanceId(instance.InstanceId);
                        if (string.IsNullOrWhiteSpace(applicationPrivateSettingsPath))
                        {
                            continue;
                        }

                        instances.Add(new VisualStudioInstance(instance, applicationPrivateSettingsPath));
                    }

                    _instances = new ReadOnlyCollection<VisualStudioInstance>(instances);
                    break;
                }
                catch (Exception ex)
                {
                    exceptions.Add((path, ex));
                }
            }

            // Log errors only if no instances are initialized
            if (_instances?.Count == 0)
            {
                foreach (var ex in exceptions)
                {
                    Log.Exception($"Failed to execute vswhere.exe from {ex.Path ?? "PATH"}", ex.Exception, typeof(VisualStudioService));
                }
            }
        }

        public IEnumerable<CodeContainer> GetResults(bool showPrerelease)
        {
            if (_instances == null)
            {
                return Enumerable.Empty<CodeContainer>();
            }

            var query = _instances.AsQueryable();

            if (!showPrerelease)
            {
                query = query.Where(i => !i.IsPrerelease);
            }

            return query.SelectMany(i => i.GetCodeContainers()).OrderBy(c => c.Name).ThenBy(c => c.Instance.IsPrerelease);
        }

        private static string? GetApplicationPrivateSettingsPathByInstanceId(string instanceId)
        {
            var dataPath = Environment.ExpandEnvironmentVariables(VisualStudioDataDir);
            var directory = Directory.EnumerateDirectories(dataPath, $"*{instanceId}", SearchOption.TopDirectoryOnly)
                .Select(d => new DirectoryInfo(d))
                .Where(d => !d.Name.StartsWith("SettingsBackup_", StringComparison.Ordinal))
                .ToArray();

            if (directory.Length == 1)
            {
                var applicationPrivateSettingspath = Path.Combine(directory[0].FullName, "ApplicationPrivateSettings.xml");

                if (File.Exists(applicationPrivateSettingspath))
                {
                    return applicationPrivateSettingspath;
                }
            }

            Log.Error($"Failed to find ApplicationPrivateSettings.xml for instance {instanceId}", typeof(VisualStudioService));

            return null;
        }
    }
}
