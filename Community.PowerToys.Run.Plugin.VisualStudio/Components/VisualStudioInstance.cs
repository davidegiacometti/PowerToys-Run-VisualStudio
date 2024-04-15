// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml;

namespace Community.PowerToys.Run.Plugin.VisualStudio.Components
{
    public class VisualStudioInstance
    {
        public string InstancePath { get; }

        public bool IsPrerelease { get; }

        public string DisplayName { get; }

        public string ApplicationPrivateSettingsPath { get; }

        public VisualStudioInstance(Json.VisualStudioInstance json, string applicationPrivateSettingsPath)
        {
            InstancePath = json.ProductPath;
            IsPrerelease = json.IsPrerelease;
            DisplayName = json.DisplayName;
            ApplicationPrivateSettingsPath = applicationPrivateSettingsPath;
        }

        public IEnumerable<CodeContainer> GetCodeContainers()
        {
            var codeContainersString = GetCodeContainersString();
            if (codeContainersString != null)
            {
                var codeContainers = JsonSerializer.Deserialize(codeContainersString, Json.CodeContainerSerializerContext.Default.ListCodeContainer);
                if (codeContainers != null)
                {
                    foreach (var c in codeContainers)
                    {
                        if (Path.Exists(c.Value.LocalProperties.FullPath))
                        {
                            yield return new CodeContainer(c, this);
                        }
                    }
                }
            }
        }

        private string? GetCodeContainersString()
        {
            if (ApplicationPrivateSettingsPath == null)
            {
                return null;
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(ApplicationPrivateSettingsPath);

            using var collectionNodes = xmlDoc.GetElementsByTagName("collection");
            var collectionName = "CodeContainers.Offline";
            var collectionNode = null as XmlNode;

            foreach (XmlNode node in collectionNodes)
            {
                var nameAttribute = node.Attributes?["name"];
                if (nameAttribute != null && nameAttribute.Value == collectionName)
                {
                    collectionNode = node;
                    break;
                }
            }

            if (collectionNode != null)
            {
                var valueNode = collectionNode?.SelectSingleNode("value");
                return valueNode?.InnerText;
            }

            return null;
        }
    }
}
