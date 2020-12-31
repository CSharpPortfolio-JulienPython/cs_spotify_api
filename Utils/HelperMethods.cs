using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Utils
{
    static partial class HelperMethods
    {
        static public string EncodeStringToHttpParameters(Dictionary<string, object> rawData)
        {
            var tempDict = rawData.ToDictionary(
                kv => HttpUtility.UrlEncode(kv.Key),
                kv => HttpUtility.UrlEncode(kv.Value.ToString()));

            Console.WriteLine(tempDict);

            return String.Join("&", tempDict.Select(kv => $"{kv.Key}={kv.Value}"));
        }

        static public string EncodeToUrl(string url, Dictionary<string, object> parameters)
        {
            return $"{url}?{EncodeStringToHttpParameters(parameters)}";
        }

        static public Dictionary<string, object> ExtractParametersFromUrl(string url)
        {
            Dictionary<string, object> parameters = url.Split("?").Last().Split("&")
                .ToDictionary(v => v.Split("=").First(), v => (object)v.Split("=").Last());

            return parameters;
        }
    }
}