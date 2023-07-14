// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Community.PowerToys.Run.Plugin.VisualStudio.Components;
using Community.PowerToys.Run.Plugin.VisualStudio.Helpers;
using Microsoft.PowerToys.Settings.UI.Library;
using Wox.Infrastructure;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.GitKraken
{
    public class Main : IPlugin, ISettingProvider, IContextMenu
    {
        private const string ShowPrerelease = nameof(ShowPrerelease);
        private const bool ShowPrereleaseDefaultValue = true;
        private readonly VisualStudioService _visualStudioService;
        private bool _showPrerelease;

        public string Name => "Visual Studio";

        public string Description => "Open Visual Studio recents.";

        public IEnumerable<PluginAdditionalOption> AdditionalOptions => new List<PluginAdditionalOption>
        {
            new PluginAdditionalOption
            {
                Key = ShowPrerelease,
                Value = ShowPrereleaseDefaultValue,
                DisplayLabel = "Show prerelease",
                DisplayDescription = "Include results from prerelease",
            },
        };

        public Main()
        {
            _visualStudioService = new VisualStudioService();
        }

        public void Init(PluginInitContext context)
        {
            _visualStudioService.InitInstances();
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
            _showPrerelease = settings != null && settings.AdditionalOptions != null
                ? settings.AdditionalOptions.FirstOrDefault(x => x.Key == ShowPrerelease)?.Value ?? ShowPrereleaseDefaultValue
                : ShowPrereleaseDefaultValue;
        }

        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            if (selectedResult.ContextData is not CodeContainer container)
            {
                return new List<ContextMenuResult>();
            }

            return new List<ContextMenuResult>
            {
                new ContextMenuResult
                {
                    Title = "Run as administrator (Ctrl+Shift+Enter)",
                    Glyph = "\xE7EF",
                    FontFamily = "Segoe MDL2 Assets",
                    AcceleratorKey = Key.Enter,
                    AcceleratorModifiers = ModifierKeys.Control | ModifierKeys.Shift,
                    Action = _ =>
                    {
                        Helper.OpenInShell(container.Instance.InstancePath, container.FullPath, runAs: Helper.ShellRunAsType.Administrator);
                        return true;
                    },
                },
                new ContextMenuResult
                {
                    Title = "Open containing folder (Ctrl+Shift+E)",
                    Glyph = "\xE838",
                    FontFamily = "Segoe MDL2 Assets",
                    AcceleratorKey = Key.E,
                    AcceleratorModifiers = ModifierKeys.Control | ModifierKeys.Shift,
                    Action = _ =>
                    {
                        Helper.OpenInShell(Path.GetDirectoryName(container.FullPath));
                        return true;
                    },
                },
            };
        }
    }
}
