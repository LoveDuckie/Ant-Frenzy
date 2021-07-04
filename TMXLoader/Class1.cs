using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Windows.Storage.Streams;
using Windows.Storage;
using System.Threading.Tasks;

using System.Xml.Serialization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.ComponentModel;

namespace TMXLoader
{
    public class Level
    {
        public Level()
        {

        }

        public async void LoadLevel(StorageFolder pFolder, string pFileName)
        {
            XDocument _document;
            StreamReader _reader;

            // Determine that the file exists before carrying out operations with it
            if (await DoesFileExist(pFolder, pFileName))
            {

            }
            else
            {
                return;
            }
        }

        public async static Task<bool> DoesFileExist(StorageFolder pFolder, string pFileName)
        {
            try
            {
                StorageFile _file = await pFolder.GetFileAsync(pFileName);
                return true;
            }
            catch
            {
                return false;
            }

        }
    }
}
