using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Newtonsoft.Json.Schema;

namespace jsonSchemaTools
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                if (args.Length == 2)
                {
                    // Check whether schema output command was pointed out
                    if (args[0] == "-o")
                    {
                        // print json schema command was pointed out
                        ExecutePrintJsonSchemaCommand(args[1]);
                    }
                    else
                        ShowAboutInfo();
                }
                if (args.Length == 3)
                {
                    if (args[0] == "-c")
                    {
                        // compare json schemas
                        ExecuteSchemasComparisonCommand(args[1], args[2]);
                    }
                    else
                        ShowAboutInfo();
                }
            }
            else
            {
                ShowAboutInfo();
                return;
            }
        }

        private static void ExecuteSchemasComparisonCommand(string fileNameLeft, string fileNameRight)
        {
            if (!File.Exists(fileNameLeft))
            {
                Console.WriteLine("File '{0}' not found!", fileNameLeft);
                return;
            }

            if (!File.Exists(fileNameRight))
            {
                Console.WriteLine("File '{0}' not found!", fileNameRight);
                return;
            }

            JSONSchemaExtractor extractor = new JSONSchemaExtractor();
            JSchema schemaLeft, schemaRight;
            List<CompareResultItem> compareResults;

            try
            {
                schemaLeft = extractor.GetSchemaAsObject(fileNameLeft);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception during schema extraction for the file: {0}", fileNameLeft);
                Console.WriteLine(e.Message);
                return;
            }

            try
            {
                schemaRight = extractor.GetSchemaAsObject(fileNameRight);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception during schema extraction for the file: {0}", fileNameRight);
                Console.WriteLine(e.Message);
                return;
            }

            try
            {
                // Try to compare
                compareResults = new List<CompareResultItem>();
                JsonSchemaComparator.Compare(schemaLeft, schemaRight, compareResults);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception during schema comparison");
                Console.WriteLine(e.Message);
                return;
            }

            Console.WriteLine("Comparing the following files: \n");
            // Write wich file is left and which is right
            Console.WriteLine("Left file: {0}", fileNameLeft);
            Console.WriteLine("Right file: {0}\n", fileNameRight);

            if (compareResults.Count == 0)
            {
                Console.WriteLine("Files are identical.");
            }
            else
            {
                // Compare results output
                ShowCompareResults(compareResults);
            }
        }
        private static void ShowCompareResults(List<CompareResultItem> resultsList)
        {
            ConsoleColor currentForegroundColor = Console.ForegroundColor;

            Console.WriteLine("The following differences have been found:\n");
            foreach (CompareResultItem diffItem in resultsList)
            {
                if (diffItem.parentObjectName.Length > 0)
                {
                    // Write Parent property name
                    Console.Write("[{0}] ==> ", diffItem.parentObjectName);
                }

                if (diffItem.itemNameLeft == diffItem.itemNameRight)
                {
                    // Looks like types are different
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("'{0}'", diffItem.itemNameLeft);
                    Console.ForegroundColor = currentForegroundColor;
                    Console.Write(" : ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("different types: ");
                    Console.ForegroundColor = currentForegroundColor;
                    Console.Write("'{0}' <-> '{1}'", diffItem.itemTypeLeft, diffItem.itemTypeRight);
                }
                else
                {
                    // item is missed in right schema
                    Console.ForegroundColor = ConsoleColor.Cyan;
                        
                    if (diffItem.itemNameRight.Length == 0)
                        Console.Write("'{0}'", diffItem.itemNameLeft);
                    else
                        Console.Write("'{0}'", diffItem.itemNameRight);

                    Console.ForegroundColor = currentForegroundColor;
                    if (diffItem.itemNameRight.Length == 0)
                        Console.Write(" : '{0}' ", diffItem.itemTypeLeft);
                    else
                        Console.Write(" : '{0}' ", diffItem.itemTypeRight);

                    Console.Write("is ");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("missed ");
                    Console.ForegroundColor = currentForegroundColor;
                    Console.Write("in the ");
                    if (diffItem.itemNameRight.Length == 0)
                    {
                        Console.Write("right ");
                    }
                    else
                    {
                        Console.Write("left ");
                    }
                    Console.Write("file");
                }

                Console.WriteLine();
            }
        }
        private static void ExecutePrintJsonSchemaCommand(string fileName)
        {
            JSONSchemaExtractor schemaExtractor;
            if (!File.Exists(fileName))
            {
                Console.WriteLine("File '{0}' not found!", fileName);
                return;
            }

            schemaExtractor = new JSONSchemaExtractor();
            try
            {
                Console.WriteLine(schemaExtractor.GetSchemaAsText(fileName));
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception during getting schema from the file: {0}", fileName);
                Console.WriteLine(e.Message);
            }
        }

        private static void ShowAboutInfo()
        {
            Console.WriteLine("JSON schema comparator. Compares only structures of two json files.\n");
            Console.WriteLine("Use: jSchemaTools [command] [file 1] [file 2]");

            Console.WriteLine("\n[command]:");
            Console.WriteLine("\t-c : compare json schemas of two files (file 1 and file 2) and show compare results");
            Console.WriteLine("\t-o : print file 1 json schema");

            return;
        }
    }
}
