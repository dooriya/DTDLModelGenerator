using System;

namespace IoTPnP.GraphGenerator
{
    class Program
    {
        static void Main(string[] args) 
        {
            string contextDir = string.Empty;
            string metamodelDir = string.Empty;
            string outputDir = string.Empty;

            if (args.Length == 3)
            {
                contextDir = args[0];
                metamodelDir = args[1];
                outputDir = args[2];
            }
            else
            {
                Console.Write("JSON-LD context directory: ");
                contextDir = Console.ReadLine();
                Console.Write("Metamodel vocabulary directory: ");
                metamodelDir = Console.ReadLine();
                Console.Write("Output directory: ");
                outputDir = Console.ReadLine();
            }

            GraphGenerator graphGenerator = new GraphGenerator();
            string graphJsonFile = graphGenerator.GenerateGraph(contextDir, metamodelDir, outputDir);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Generate normalized JSON-LD graph successfully at: {graphJsonFile}");
            Console.ResetColor();
        }
    }
}
