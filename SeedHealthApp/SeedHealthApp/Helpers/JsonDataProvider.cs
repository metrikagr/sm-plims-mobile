using Newtonsoft.Json;
using SeedHealthApp.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SeedHealthApp.Helpers
{
    static class JsonDataProvider
    {
        public static async Task<List<Request>> GetRequestsAsync()
        {
            var assembly = typeof(JsonDataProvider).GetTypeInfo().Assembly;
            //var resourceNames = assembly.GetManifestResourceNames();

            System.IO.Stream stream = assembly.GetManifestResourceStream("SeedHealthApp.Resources.requests.json");
            string json = string.Empty;
            if (stream == null)
                throw new NullReferenceException("SeedHealthApp.Resources.requests.json not found");
            using (var reader = new System.IO.StreamReader(stream))
            {
                json = await reader.ReadToEndAsync();
            }
            return JsonConvert.DeserializeObject<List<Request>>(json);
        }

        public static async Task<IEnumerable<PathogenItem>> GetPathogenItemsAsync()
        {
            var assembly = typeof(JsonDataProvider).GetTypeInfo().Assembly;
            string resourceName = "SeedHealthApp.Resources.pathogens.json";
            using (System.IO.Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new NullReferenceException($"{resourceName} not found");
                var result = await System.Text.Json.JsonSerializer.DeserializeAsync<IEnumerable<PathogenItem>>(stream);
                return result;
            }
            /*
            string json = string.Empty;
            using (var reader = new System.IO.StreamReader(stream))
            {
                json = await reader.ReadToEndAsync();
            }
            return JsonConvert.DeserializeObject<List<PathogenItem>>(json);
            */
        }
    }
}
