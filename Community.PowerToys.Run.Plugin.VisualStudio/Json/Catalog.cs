// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Community.PowerToys.Run.Plugin.VisualStudio.Json
{
    public sealed class Catalog
    {
        [JsonPropertyName("productLineVersion")]
        public required string ProductLineVersion { get; set; }
    }
}
