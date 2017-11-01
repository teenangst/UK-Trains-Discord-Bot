using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace UK_Trains_Discord
{
    static class Wikipedia
    {
        public static async Task<string> getPage(string search)
        {
            //OMFG I am never using the Wikipedia API again
            int tries = 5;
            Console.WriteLine($"Search input: {search}");
            if (CustomCommands.commands.ContainsKey(search.Substring(1).ToLower()))
            {
                return CustomCommands.commands[search.Substring(1).ToLower()];
            }
            else if (Regex.IsMatch(search, "\\" + CustomCommands.prefix + @"\d{1,3}$"))
            {
                retry:;
                Console.WriteLine("Is Class Number");
                HttpClient client = new HttpClient();

                //Get XML of the page - the JSON is broken
                HttpResponseMessage response = await client.GetAsync($"https://en.wikipedia.org/w/api.php?action=opensearch&search=British%20Rail%20Class%20{search.Substring(1)}&limit=1&profile=strict&format=xml"); //The JSON format Wikipedia gives is broken
                if (response.IsSuccessStatusCode)
                {
                    Stream receiveStream = await response.Content.ReadAsStreamAsync();
                    StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

                    //Convert JSON to XML for ease of use
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(readStream.ReadToEnd());
                    string jsonText = JsonConvert.SerializeXmlNode(doc);
                    JObject jo = JObject.Parse(jsonText);

                    if (jo["SearchSuggestion"]["Section"].ToArray().Length == 0)
                    {
                        return "British Rail Class Not Found";
                    }
                    else
                    {
                        return (string)jo["SearchSuggestion"]["Section"]["Item"]["Url"]["#text"];
                    }
                }
                else
                {
                    //Retry 5 times if wikipedia is not responding
                    tries--;
                    if (tries > 0)
                    {
                        goto retry;
                    }
                    else
                    {
                        return "Problem Accessing Wikipedia";
                    }
                }
            }
            else
            {
                return "No Command Match";
            }
        }
    }
}
