using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Windows.Storage;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;


namespace AntRunner.Utility
{
    public class TestStats : JsonSerializer
    {
        // In Euclidean.
        public float DistanceFromGoal = 0f;
        public int ReplanTime = 0;
        public string PathfindingMethod = "";

        /// <summary>
        /// Serialize the stats into a JSON based text-file
        /// </summary>
        /// <param name="pStats">The stats object</param>
        /// <param name="pFilename">The name of the file in question</param>
        public async static void SerializeStats(TestStats pStats, string pFilename)
        {
            string _output = JsonConvert.SerializeObject(pStats, Formatting.Indented);
            string _datetimeName = DateTime.Now.ToString("ddMMyyyyHHmmssff");

            // Generate the storage file that we are going to store in.
            StorageFile _file = await ApplicationData.Current.LocalFolder.CreateFileAsync(pFilename, CreationCollisionOption.ReplaceExisting);

            // Output the text file that contains the testing information
            await FileIO.WriteTextAsync(_file, _output);
        }
    }
}
