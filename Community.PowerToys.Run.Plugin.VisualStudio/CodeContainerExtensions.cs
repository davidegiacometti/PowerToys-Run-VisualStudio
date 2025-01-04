// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using Community.PowerToys.Run.Plugin.VisualStudio.Core.Models;
using Community.PowerToys.Run.Plugin.VisualStudio.Properties;
using Wox.Infrastructure;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.VisualStudio
{
    public static class CodeContainerExtensions
    {
        private static readonly string _pluginName = Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty;

        public static Result ToResult(this CodeContainer codeContainer, MatchResult matchResult)
        {
            var instance = codeContainer.Instance;
            return new Result
            {
                Title = codeContainer.Name,
                SubTitle = string.Format(Resources.Result_Subtitle, instance.DisplayName, codeContainer.FullPath),
                IcoPath = instance.InstancePath,
                Score = matchResult.Score,
                TitleHighlightData = matchResult.MatchData,
                ContextData = codeContainer,
                Action = _ =>
                {
                    Helper.OpenInShell(instance.InstancePath, $"\"{codeContainer.FullPath}\"");
                    return true;
                },
            };
        }

        public static List<ContextMenuResult> ToContextMenuResults(this CodeContainer codeContainer)
        {
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
                        Helper.OpenInShell(codeContainer.Instance.InstancePath, $"\"{codeContainer.FullPath}\"", runAs: Helper.ShellRunAsType.Administrator);
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
                        Helper.OpenInShell(Path.GetDirectoryName(codeContainer.FullPath));
                        return true;
                    },
                },
            };
        }
    }
}
