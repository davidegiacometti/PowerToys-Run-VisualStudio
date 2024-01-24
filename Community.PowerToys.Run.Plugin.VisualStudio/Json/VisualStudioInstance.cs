// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Community.PowerToys.Run.Plugin.VisualStudio.Json
{
    public sealed class VisualStudioInstance
    {
        [JsonPropertyName("instanceId")]
        public required string InstanceId { get; set; }

        [JsonPropertyName("productPath")]
        public required string ProductPath { get; set; }

        [JsonPropertyName("isPrerelease")]
        public bool IsPrerelease { get; set; }

        [JsonPropertyName("displayName")]
        public required string DisplayName { get; set; }

        [JsonPropertyName("catalog")]
        public required Catalog Catalog { get; set; }
    }
}
