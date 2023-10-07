// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Wox.Infrastructure;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.VisualStudio.Components
{
    public class CodeContainer
    {
        public string Name { get; }

        public string FullPath { get; }

        public bool IsFavorite { get; }

        public DateTime LastAccessed { get; }

        public VisualStudioInstance Instance { get; }

        public CodeContainer(Json.CodeContainer codeContainer, VisualStudioInstance instance)
        {
            Name = Path.GetFileName(codeContainer.Value.LocalProperties.FullPath);
            FullPath = codeContainer.Value.LocalProperties.FullPath;
            IsFavorite = codeContainer.Value.IsFavorite;
            LastAccessed = codeContainer.Value.LastAccessed;
            Instance = instance;
        }

        public Result ToResult(MatchResult matchResult)
        {
            return new Result
            {
                Title = Name,
                SubTitle = $"{Instance.DisplayName}: {FullPath}",
                IcoPath = Instance.InstancePath,
                Score = matchResult.Score,
                TitleHighlightData = matchResult.MatchData,
                ContextData = this,
                Action = _ =>
                {
                    Helper.OpenInShell(Instance.InstancePath, $"\"{FullPath}\"");
                    return true;
                },
            };
        }
    }
}
