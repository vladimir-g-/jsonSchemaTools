using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace jsonSchemaTools
{
    class Program
    {
        static void Main(string[] args)
        {
            string sPath1 = "";
            string sPath2 = "";

            if (args.Length > 1)
            {
                sPath1 = args[0];
                sPath2 = args[1];

                ExecuteSchemasComparison(sPath1, sPath2);
            }
            else
            {
                ShowAboutInfo();
                return;
            }
        }

        private static void ExecuteSchemasComparison(string fileNameLeft, string fileNameRight)
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
            if (compareResults.Count == 0)
            {
                Console.WriteLine("Files are identical.");
            }
            else
            {
                // Compare results output
                ShowCompareResults(compareResults);

                //Console.WriteLine("\nPress any key to exit ...");
                //Console.ReadKey();
            }
        }
         private static void ShowCompareResults(List<CompareResultItem> resultsList)
        {
            ConsoleColor currentForegroundColor = Console.ForegroundColor;

            Console.WriteLine("Found the following differences:\n");
            foreach (CompareResultItem diffItem in resultsList)
            {
                if (diffItem.parentObjectName.Length > 0)
                {
                    //Console.Write("Parent property '{0}': ", diffItem.parentObjectName);
                    Console.Write("[{0}] ==> ", diffItem.parentObjectName);
                }

                if (diffItem.itemNameLeft == diffItem.itemNameRight)
                {
                    // Looks like types are different
                    //Console.Write("key ");
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
                   // Console.Write("key ");
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

        private static void ShowAboutInfo()
        {
            Console.WriteLine("JSON schema comparator. Compares only structures of two json files.\n");
            Console.WriteLine("Use: jSchemaTools [file1 path] [file2 path]");

            return;
        }
    }
}
