// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

namespace Community.PowerToys.Run.Plugin.VisualStudio.Core.Models
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
    }
}
