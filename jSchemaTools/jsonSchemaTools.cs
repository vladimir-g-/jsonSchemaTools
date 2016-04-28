using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq; 
using System.Collections.Generic;

// just to write into Console
using System;
// to write into debug window
using System.Diagnostics;

namespace jsonSchemaTools
{

    class JsonSchemaComparator
    {
        static public void Compare(JSchema schemaLeft, JSchema schemaRight,  List<CompareResultItem> compareResults)
        {
            CompareSchemas(schemaLeft, schemaRight, compareResults, "", true);
            CompareSchemas(schemaRight, schemaLeft, compareResults, "", false, true);

            return;
        }

        static private void CompareSchemas(JSchema schemaLeft, JSchema schemaRight,  List<CompareResultItem> compareResults, string parentObjectName = "", bool compareItemType = true, bool compareRightWithLeft = false)
        {
            string leftItemType, leftItemName, rightItemType;

            // Compare left schema with the right one, verifying types of properties

            foreach (KeyValuePair<string, JSchema> leftSchemaProperty in schemaLeft.Properties)
            {
                JObject objectLeft = JObject.Parse(leftSchemaProperty.Value.ToString());
                // Get left schema property type
                leftItemType = (string)objectLeft["type"];
                // Get left schema property name
                leftItemName = leftSchemaProperty.Key;

                // Try to find a property with the same name in the right schema
                if (schemaRight.Properties.ContainsKey(leftItemName))
                {
                    // Right schema contains item with the same key
                    JSchema rightItemSchema;

                    // Try to get value using the given key
                    if (schemaRight.Properties.TryGetValue(leftItemName, out rightItemSchema))
                    {
                        // We have got item using given key

                        // try to compare items types
                        if (compareItemType == true)
                        {
                            // We are going compare items types

                            // Transform JSchema object into JObject to get 'type' property value
                            JObject rightSchemaOjectType = JObject.Parse(rightItemSchema.ToString());
                            rightItemType = (string)rightSchemaOjectType["type"];

                            if (leftItemType == rightItemType)
                            {
                                // Verify that array and object types have the same structure
                                if (leftItemType == "object")
                                {
                                    //
                                    JSchema leftInnerObject = JSchema.Parse(objectLeft.ToString());
                                    JSchema rightInnerObject = JSchema.Parse(rightSchemaOjectType.ToString());

                                    CompareSchemas(leftInnerObject, rightInnerObject, compareResults, leftItemName, true);

                                    // Compare right schema with left one...
                                    CompareSchemas(rightInnerObject, leftInnerObject, compareResults, leftItemName, false, true);
                                }
                                if (leftItemType == "array")
                                {
                                    // Check type array items
                                    JObject leftArrayItems = (JObject)objectLeft["items"];
                                    JObject rigtArrayItems = (JObject)rightSchemaOjectType["items"];

                                    string leftArrayItemsType = (string)leftArrayItems["type"];
                                    string rightArrayItemsType = (string)rigtArrayItems["type"];

                                    if (leftArrayItemsType != rightArrayItemsType)
                                    {
                                        // Array's items have different types
                                        CompareResultItem resultItem = new CompareResultItem(leftItemName, "array of " + leftArrayItemsType, leftItemName, "array of " + rightArrayItemsType, parentObjectName);
                                        compareResults.Add(resultItem);
                                        
                                        Debug.WriteLine(String.Format("property: '{0}': left - array of {1} : right - array of {2}", leftItemName, leftArrayItemsType, rightArrayItemsType));
                                    }
                                    else
                                    {
                                        if (leftArrayItemsType == "object")
                                        {
                                            Debug.WriteLine(String.Format("'{0}' array of objects has been found", leftItemName));
                                            
                                            // Try to compare schemas of array items
                                            JSchema leftInnerObject = JSchema.Parse(leftArrayItems.ToString());
                                            JSchema rightInnerObject = JSchema.Parse(rigtArrayItems.ToString());

                                            CompareSchemas(leftInnerObject, rightInnerObject, compareResults, leftItemName, true);
                                            CompareSchemas(rightInnerObject, leftInnerObject, compareResults, leftItemName, false, true);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // The types are different
                                CompareResultItem resultItem = new CompareResultItem(leftItemName, leftItemType, leftItemName, rightItemType, parentObjectName);
                                compareResults.Add(resultItem);
                            }
                        }
                    }
                    else
                    {
                        // we didn't get item using given key
                        // treat this case as error case 

                        //TBD: Define logic
                        Debug.WriteLine(String.Format("Can't get value of '{0}' key", leftItemName));
                    }
                }
                else
                {
                    // item wasn't found in right schema
                    Debug.WriteLine(String.Format("Item '{0}' was not found in right schema", leftItemName));

                    CompareResultItem resultItem;

                    if (compareRightWithLeft == false)
                        resultItem = new CompareResultItem(leftItemName, leftItemType, "", "", parentObjectName);
                    else
                        resultItem = new CompareResultItem("", "", leftItemName, leftItemType, parentObjectName);

                    compareResults.Add(resultItem);
                }

                //Console.WriteLine("Key = {0}, Value = {1}", leftSchemaProperty.Key, leftSchemaProperty.Value);
                Debug.WriteLine(String.Format("Found: Key = '{0}', Type = {1}", leftItemName, leftItemType));
            }

            return;
        }
    }

    class CompareResultItem
    {
        public string itemNameLeft;
        public string itemTypeLeft;

        public string itemNameRight;
        public string itemTypeRight;

        public string parentObjectName;

        public CompareResultItem(string _itemNameLeft, string _itemTypeLeft, string _itemNameRight, string _itemTypeRight, string _parentObjectName)
        {
            itemNameLeft = _itemNameLeft;
            itemTypeLeft = _itemTypeLeft;
            itemNameRight = _itemNameRight;
            itemTypeRight = _itemTypeRight;
            parentObjectName = _parentObjectName;
        }
    }

}