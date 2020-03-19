using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JsonLd.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IoTPnP.GraphGenerator
{
    public class GraphGenerator
    {
        const string ClassJsonContextName = "Class.json";
        const string InterfaceJsonContextName = "Interface.json";
        const string TemplateJsonContextName = "CapabilityModel.json";

        public string GenerateGraph(string contextDir, string metamodelDir, string outputDir = null)
        {
            Console.WriteLine("Parse metamodel graph...");
            Graph metamodelGraph = new Graph();
            GetMetamodel(contextDir, metamodelDir, ref metamodelGraph);

            Console.WriteLine("Serialize graph to json file...");
            string graphJsonString = JsonConvert.SerializeObject(metamodelGraph, Formatting.Indented);

            if (string.IsNullOrEmpty(outputDir))
            {
                outputDir = Directory.GetCurrentDirectory();
            }
            string graphJsonFile = Path.Combine(outputDir, "graph.json");
            File.WriteAllText(graphJsonFile, graphJsonString);

            return graphJsonFile;
        }

        private void GetMetamodel(string contextDir, string metamodelDir, ref Graph metamodelGraph)
        {
            if (!Directory.Exists(contextDir))
            {
                throw new DirectoryNotFoundException($"context directory doesn't exist: {metamodelDir}");
            }

            if (!Directory.Exists(metamodelDir))
            {
                throw new DirectoryNotFoundException($"metamodel directory doesn't exist: {metamodelDir}");
            }

            // Load JSON-LD context objects
            JObject classContextJsonObject = LoadJsonObject(Path.Combine(contextDir, ClassJsonContextName));
            JObject interfaceContextJsonObject = LoadJsonObject(Path.Combine(contextDir, InterfaceJsonContextName));
            JObject templateContextJsonObject = LoadJsonObject(Path.Combine(contextDir, TemplateJsonContextName));

            GraphFactoryOptions graphFactoryOptions = new GraphFactoryOptions();
            graphFactoryOptions.SetDefaultContext(interfaceContextJsonObject);

            // Build metamodel graph
            List<string> fileNames = new List<string>();
            fileNames.AddRange(Directory.GetFiles(metamodelDir, "*.json"));
            fileNames.AddRange(Directory.GetFiles(metamodelDir, "*.jsonld"));
            foreach (string fileName in fileNames)
            {
                Console.WriteLine("Parse JSON-LD file: {0}", fileName);

                string json = File.ReadAllText(fileName);
                JObject jsonObj = JObject.Parse(json);

                // Replace the context IRI with a local context object
                var contextObject = jsonObj["@context"];
                int tokenCount = contextObject.Children().Count();
                for (int i = 0; i < tokenCount; i++)
                {
                    string context = contextObject[i].ToString();

                    // Replace context IRI http://azureiot.com/v0/contexts/Class.json with local context object
                    if (context.EndsWith(ClassJsonContextName, StringComparison.OrdinalIgnoreCase))
                    {
                        contextObject[i] = classContextJsonObject["@context"];
                    }
                    // Replace context IRI http://azureiot.com/v0/contexts/Interface.json with local context object
                    else if (context.EndsWith(InterfaceJsonContextName, StringComparison.OrdinalIgnoreCase))
                    {
                        contextObject[i] = interfaceContextJsonObject["@context"];
                    }
                    // Replace context IRI http://azureiot.com/v0/contexts/Template.json with local context object
                    else if (context.EndsWith(TemplateJsonContextName, StringComparison.OrdinalIgnoreCase))
                    {
                        contextObject[i] = templateContextJsonObject["@context"];
                    }
                }

                Graph loadedGraph = GraphFactory.CreateGraph(jsonObj, graphFactoryOptions);
                metamodelGraph.Merge(loadedGraph);
            }

            string[] subDirectories = Directory.GetDirectories(metamodelDir);
            foreach (string subDirectory in subDirectories)
            {
                GetMetamodel(contextDir, subDirectory, ref metamodelGraph);
            }
        }

        private JObject LoadJsonObject(string jsonFilePath)
        {
            string json = File.ReadAllText(jsonFilePath);
            JObject jsonObj = JObject.Parse(json);
            return jsonObj;
        }
    }
}
