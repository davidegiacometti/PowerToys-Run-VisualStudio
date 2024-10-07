// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;
using Community.PowerToys.Run.Plugin.VisualStudio.Components;
using Community.PowerToys.Run.Plugin.VisualStudio.Helpers;
using Community.PowerToys.Run.Plugin.VisualStudio.Properties;
using Microsoft.PowerToys.Settings.UI.Library;
using Wox.Infrastructure;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.VisualStudio
{
    public class Main : IPlugin, IPluginI18n, ISettingProvider, IContextMenu
    {
        private const string ShowPrerelease = nameof(ShowPrerelease);
        private const string ExcludedVersions = nameof(ExcludedVersions);
        private const bool ShowPrereleaseDefaultValue = true;
        private const string ExcludedVersionsDefaultValue = "";
        private static readonly string _pluginName = Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty;

        public static string PluginID => "D0998A1863424336A86A2B6E936C0E8E";

        private readonly VisualStudioService _visualStudioService;
        private bool _showPrerelease;
        private string _excludedVersions;

        public string Name => Resources.PluginName;

        public string Description => Resources.PluginDescription;

        public IEnumerable<PluginAdditionalOption> AdditionalOptions => new List<PluginAdditionalOption>
        {
            new()
            {
                Key = ShowPrerelease,
                Value = ShowPrereleaseDefaultValue,
                DisplayLabel = Resources.Option_ShowPrerelease_Label,
                DisplayDescription = Resources.Option_ShowPrerelease_Description,
            },
            new()
            {
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox,
                Key = ExcludedVersions,
                TextValue = ExcludedVersionsDefaultValue,
                DisplayLabel = Resources.Option_ExcludedVersions_Label,
                DisplayDescription = Resources.Option_ExcludedVersions_Description,
            },
        };

        public string GetTranslatedPluginTitle() => Resources.PluginName;

        public string GetTranslatedPluginDescription() => Resources.PluginDescription;

        public Main()
        {
            _excludedVersions = ExcludedVersionsDefaultValue;
            _visualStudioService = new VisualStudioService();
        }

        public void Init(PluginInitContext context)
        {
            ReloadVisualStudioInstances();
        }

        public List<Result> Query(Query query)
        {
            var results = new List<Result>();

            foreach (var container in _visualStudioService.GetResults(_showPrerelease))
            {
                var matchResult = StringMatcher.FuzzySearch(query.Search, container.Name);
                if (string.IsNullOrWhiteSpace(query.Search) || matchResult.Score > 0)
                {
                    results.Add(container.ToResult(matchResult));
                }
            }

            return results;
        }

        public Control CreateSettingPanel()
        {
            throw new System.NotImplementedException();
        }

        public void UpdateSettings(PowerLauncherPluginSettings settings)
        {
            var oldExcludedVersions = _excludedVersions;

            if (settings != null && settings.AdditionalOptions != null)
            {
                _showPrerelease = settings.AdditionalOptions.FirstOrDefault(x => x.Key == ShowPrerelease)?.Value ?? ShowPrereleaseDefaultValue;
                _excludedVersions = settings.AdditionalOptions.FirstOrDefault(x => x.Key == ExcludedVersions)?.TextValue ?? ExcludedVersionsDefaultValue;
            }
            else
            {
                _showPrerelease = ShowPrereleaseDefaultValue;
                _excludedVersions = ExcludedVersionsDefaultValue;
            }

            if (oldExcludedVersions != _excludedVersions)
            {
                ReloadVisualStudioInstances();
            }
        }

        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            if (selectedResult.ContextData is not CodeContainer container)
            {
                return new List<ContextMenuResult>();
            }

            return new List<ContextMenuResult>
            {
                new()
                {
                    Title = Resources.Action_RunAsAdministrator,
                    Glyph = "\xE7EF",
                    FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                    AcceleratorKey = Key.Enter,
                    AcceleratorModifiers = ModifierKeys.Control | ModifierKeys.Shift,
                    PluginName = _pluginName,
                    Action = _ =>
                    {
                        Helper.OpenInShell(container.Instance.InstancePath, $"\"{container.FullPath}\"", runAs: Helper.ShellRunAsType.Administrator);
                        return true;
                    },
                },
                new()
                {
                    Title = Resources.Action_OpenContainingFolder,
                    Glyph = "\xE838",
                    FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                    AcceleratorKey = Key.E,
                    AcceleratorModifiers = ModifierKeys.Control | ModifierKeys.Shift,
                    PluginName = _pluginName,
                    Action = _ =>
                    {
                        Helper.OpenInShell(Path.GetDirectoryName(container.FullPath));
                        return true;
                    },
                },
            };
        }

        private void ReloadVisualStudioInstances()
        {
            _visualStudioService.InitInstances(_excludedVersions.Split(' ').ToArray());
        }
    }
}
