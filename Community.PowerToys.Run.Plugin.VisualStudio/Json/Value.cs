// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Community.PowerToys.Run.Plugin.VisualStudio.Json
{
    public sealed class Value
    {
        public required LocalProperties LocalProperties { get; set; }

        public bool IsFavorite { get; set; }

        public DateTime LastAccessed { get; set; }
    }
}
