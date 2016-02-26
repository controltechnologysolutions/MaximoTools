using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace MaximoDBPopulator {
    public class Configuration {
        private const string ConfigFilePath = "config.xml";

        public string ConnectionString { get; private set; }
        public int BatchSize { get; private set; }
        public List<PopulationConfig> Populations { get; private set; }

        public Configuration() {
            BatchSize = 1000;
            Populations = new List<PopulationConfig>();
        }

        public void Load() {
            if (!File.Exists(ConfigFilePath)) {
                throw new SystemException("Missing config.xml file.");
            }

            var doc = new XmlDocument();
            doc.Load(ConfigFilePath);

            if (doc.DocumentElement == null) {
                throw new SystemException("Fail to load config.xml.");
            }

            var root = doc.DocumentElement;

            ConnectionString = LoadStringField(root, //
                "/configuration/connstring", //
                "<connstring> is required.", //
                "<connstring> is empty.");

            var batchSizeString = LoadStringField(root, "/configuration/batchsize");
            if (!string.IsNullOrEmpty(batchSizeString)) {
                var batchSize = 0;
                if (!int.TryParse(batchSizeString, out batchSize)) {
                    throw new SystemException($"Batch size ({batchSizeString}) is not valid.");
                }
                BatchSize = batchSize;
            }

            var populationsNode = root.SelectSingleNode("/configuration/populations");
            if (populationsNode == null) {
                throw new SystemException("<populations> is required.");
            }

            var populationNodes = populationsNode.SelectNodes("population");
            if (populationNodes.Count == 0) {
                throw new SystemException("No <population> inside <populations>.");
            }

            var populationIndex = 0;
            foreach (XmlNode populationNode in populationNodes) {
                var popConfig = new PopulationConfig();
                popConfig.Load(populationNode, populationIndex);
                Populations.Add(popConfig);
                populationIndex++;
            }

            Console.WriteLine("Configuration Loaded.");
        }

        private static string LoadStringField(XmlNode root, string path, string noNodeMsg = null, string emptyMsg = null) {
            var connStringNode = root.SelectSingleNode(path);
            if (connStringNode == null) {
                if (string.IsNullOrEmpty(noNodeMsg)) {
                    return null;
                }
                throw new SystemException(noNodeMsg);
            }
            var value = connStringNode.InnerText;

            if (!string.IsNullOrEmpty(value) || string.IsNullOrEmpty(emptyMsg)) return value;

            throw new SystemException(emptyMsg);
        }

        public class PopulationConfig {

            public string TableName { get; private set; }
            public int NumberOfEntries { get; private set; }
            public long StartIndex { get; private set; }
            public string BaseQuery { get; private set; }
            public Dictionary<string, string> Customizations { get; private set; }
            public List<string> Exclusions { get; private set; }

            public PopulationConfig() {
                StartIndex = 1;
                Customizations = new Dictionary<string, string>();
                Exclusions = new List<string>();
            }

            public void Load(XmlNode root, int index) {
                TableName = LoadStringField(root, //
                    "tablename", //
                    NoNodeMsg("tablename", index), //
                    EmptyMsg("tablename", index));

                var numberOfEntriesString = LoadStringField(root, //
                    "numberofentries", //
                    NoNodeMsg("numberofentries", index), //
                    EmptyMsg("numberofentries", index));


                var numberOfEntries = 0;
                if (!int.TryParse(numberOfEntriesString, out numberOfEntries)) {
                    throw new SystemException($"The number of entries ({numberOfEntriesString}) is not valid on population of number {index} (zero-based).");
                }
                NumberOfEntries = numberOfEntries;

                BaseQuery = LoadStringField(root, //
                    "basequery", //
                    NoNodeMsg("basequery", index), //
                    EmptyMsg("basequery", index));

                var startIndexString = LoadStringField(root, "startindex");
                if (!string.IsNullOrEmpty(startIndexString)) {
                    var startIndex = 0L;
                    if (!long.TryParse(startIndexString, out startIndex)) {
                        throw new SystemException($"Start index ({startIndexString}) is not valid on population of number {index} (zero-based).");
                    }
                    StartIndex = startIndex;
                }

                LoadExclusions(root);
                LoadCustomizations(root, index);
            }

            private static string NoNodeMsg(string elementName, int index) {
                return $"<{elementName}> is required on population of number {index} (zero-based).";
            }

            private static string EmptyMsg(string elementName, int index) {
                return $"<{elementName}> is empty on population of number {index} (zero-based).";
            }

            private void LoadExclusions(XmlNode root) {
                var exclusionsNode = root.SelectSingleNode("exclusions");

                var exclusionNodes = exclusionsNode?.SelectNodes("exclusion");
                if (exclusionNodes == null || exclusionNodes.Count == 0) {
                    return;
                }

                foreach (XmlNode exclusionNode in exclusionNodes) {
                    LoadExclusion(exclusionNode);
                }
            }

            private void LoadExclusion(XmlNode exclusionNode) {
                var columnNameAtt = exclusionNode.Attributes["columnname"];
                var columnName = columnNameAtt?.Value;
                if (string.IsNullOrEmpty(columnName)) {
                    return;
                }
                Exclusions.Add(columnName);
            }

            private void LoadCustomizations(XmlNode root, int populationIndex) {
                var customizationsNode = root.SelectSingleNode("customizations");

                var customizationNodes = customizationsNode?.SelectNodes("customization");
                if (customizationNodes == null || customizationNodes.Count == 0) {
                    return;
                }

                var customizationIndex = 0;
                foreach (XmlNode customizationNode in customizationNodes) {
                    LoadCustomization(customizationNode, populationIndex, customizationIndex);
                    customizationIndex++;
                }
            }

            private void LoadCustomization(XmlNode customizationNode, int populationIndex, int customizationIndex) {
                var columnName = customizationNode.Attributes["columnname"]?.Value;
                if (string.IsNullOrEmpty(columnName)) {
                    throw new SystemException($"Customization on number {customizationIndex} (zero-based) from population of number {populationIndex} (zero-based) have not a valid columnName attribute.");
                }

                var value = customizationNode.Attributes["value"]?.Value;
                if (string.IsNullOrEmpty(value)) {
                    throw new SystemException($"Customization on number {customizationIndex} (zero-based) from population of number {populationIndex} (zero-based) have not a valid value attribute.");
                }

                Customizations.Add(columnName, value);
            }
        }
    }
}
