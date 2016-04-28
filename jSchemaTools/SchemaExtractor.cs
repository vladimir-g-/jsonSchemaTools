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
    class JSONSchemaExtractor
    {
        /// <summary>
        /// Return json schema from a file as a string
        /// </summary>
        /// <param name="fileName">json file name</param>
        /// <returns>json schema as string</returns>
        public string GetSchemaAsText(string fileName)
        {
            using (StreamReader file = File.OpenText(fileName))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JObject o = JObject.Load(reader);

                JSchema schema1 = JSchema.Parse( GetSchema(o) );

                return schema1.ToString();
            }
        }

        /// <summary>
        /// Return json schema from a file as a JSchema object
        /// </summary>
        /// <param name="fileName">json file name</param>
        /// <returns>a JSchema object</returns>
        public JSchema GetSchemaAsObject(string fileName)
        {
            using (StreamReader file = File.OpenText(fileName))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JObject o = JObject.Load(reader);

                JSchema schema1 = JSchema.Parse(GetSchema(o));

                return schema1;
            }
        }
        
        private string GetSchema(JObject jsonObject)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("{");
            builder.Append("'type':'object',");
            builder.Append("'properties': ");

            builder.Append(GetProperties(jsonObject));

            builder.Append("}");

            return builder.ToString();
        }

        private string GetProperties(JObject jsonObject)
        {
            StringBuilder builder = new StringBuilder();
            IEnumerable<JProperty> properties = jsonObject.Properties();
            string itemName, itemValue;
            JTokenType itemType;
            int itemCounter = 0;

            builder.Append("{");

            foreach (JProperty item in properties)
            {
                // get item's name
                itemName = item.Name;

                // get item's value
                itemValue = item.Value.ToString();

                // get item's type
                itemType = item.Value.Type;
                if (itemCounter > 0)
                {
                    builder.Append(",");
                }

                builder.Append("'" + itemName + "':");
                if (itemType == JTokenType.Object)
                {
                    JObject o = JObject.Parse(itemValue.ToString());
                    builder.Append(GetSchema(o));
                }
                else
                    if (itemType == JTokenType.Array)
                    {
                        builder.Append("{ 'type' : 'array',");
                        builder.Append("'items' : { 'type' : ");

                        int arrayItemsCounter = 0;

                        JArray array = JArray.Parse(itemValue.ToString());
                        if (array.Count > 0)
                        {
                            // get first element of array
                            //JToken firstElement = array.First;
                            JToken firstElement = array[arrayItemsCounter];
                           
                            // Skip all null items in the array and try to get non-null type value
                            while ( (firstElement.Type == JTokenType.Null) && (arrayItemsCounter < array.Count - 1) )
                            {
                                //
                                ++arrayItemsCounter;
                                firstElement = array[arrayItemsCounter];
                            }

                            // Here we should have either null or non-null type of array item
                            if (firstElement.Type == JTokenType.Object)
                            {
                                builder.Append("'object', ");
                                builder.Append("'properties': ");
                                // parse object
                                JObject o = JObject.Parse(firstElement.ToString());
                                builder.Append(GetProperties(o));
                            }
                            else
                            {
                                // this is not an object
                                builder.Append("'" + firstElement.Type.ToString().ToLower() + "'");
                            }
                        }
                        else
                        {
                            // this is an empty array
                            builder.Append("'null'");
                        }

                        builder.Append("}");
                        builder.Append("}");
                    }
                    else
                    {
                        builder.Append("{ 'type': '" + itemType.ToString().ToLower() + "' }");
                    }
                ++itemCounter;
            }

            builder.Append("}");

            return builder.ToString();
        }

    }
}
