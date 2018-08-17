using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace AdventureLandLibrary.Global
{
    public static class Loader
    {
        private static dynamic _data { get; set; }
        public static dynamic data
        {
            get
            {
                if (_data == null)
                {
                    _data = GetData();
                }

                return _data;
            }
        }

        public static DirectoryInfo GetCurrentVersionDirectory()
        {
            return GetVersionDirectory((int)data.version);
        }

        public static DirectoryInfo GetVersionDirectory(int version)
        {
            var directory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\AdventureLandService\" + version);

            if (!directory.Exists)
            {
                directory.Create();
            }

            return directory;
        }

        private static dynamic GetData()
        {

            string data = DownloadFile("data.js");

            data = data.Replace("var G=", "");
            data = data.Replace("};", "}");

            dynamic json = JObject.Parse(data);

            var newFile = SaveJSON(json);

            if (newFile)
            {
                SaveDenseJSON(json);

                DownloadAndSaveJSFile("game.js", (int)json.version);
                DownloadAndSaveJSFile("common_functions.js", (int)json.version);
                DownloadAndSaveJSFile("functions.js", (int)json.version);
                DownloadAndSaveJSFile("html.js", (int)json.version);
                DownloadAndSaveJSFile("keyboard.js", (int)json.version);
            }

            return json;
        }

        public static string DownloadFile(string filename)
        {
            string fileContents = "";

            string url = @"http://adventure.land/" + filename;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (string.IsNullOrEmpty(response.CharacterSet))
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                fileContents = readStream.ReadToEnd();

                response.Close();
                readStream.Close();
            }

            return fileContents;
        }

        private static void DownloadAndSaveJSFile(string filename, int version)
        {

            var dataDirectory = new DirectoryInfo(GetVersionDirectory(version) + @"\js\");

            if (!dataDirectory.Exists)
            {
                dataDirectory.Create();
            }

            var filePath = dataDirectory.FullName + filename;

            var fileinfo = new FileInfo(filePath);

            if (!fileinfo.Exists)
            {
                string contents = DownloadFile(@"\js\" + filename);
                using (StreamWriter writer = File.CreateText(filePath))
                {
                    writer.Write(contents);
                }
            }
        }

        private static bool SaveJSON(dynamic json)
        {
            var dataDirectory = new DirectoryInfo(GetVersionDirectory((int)json.version) + @"\data\");

            if (!dataDirectory.Exists)
            {
                dataDirectory.Create();
            }

            var filename = dataDirectory.FullName + @"\data.json";

            var fileinfo = new FileInfo(filename);


            if (!fileinfo.Exists)
            {
                var jsonString = JsonConvert.SerializeObject(json, Formatting.Indented);

                using (StreamWriter writer = File.CreateText(filename))
                {
                    writer.Write(jsonString);
                }

                return true;
            }

            return false;
        }

        private static dynamic ReadJSON(string filename)
        {
            string raw = File.ReadAllText(filename);

            dynamic json = JObject.Parse(raw);

            return json;
        }

        private static dynamic CloneJSON(dynamic json)
        {
            string serialized = JsonConvert.SerializeObject(json);

            dynamic clone = JObject.Parse(serialized);

            return clone;
        }

        private static void SaveDenseJSON(dynamic json)
        {
            var clone = CloneJSON(json);

            JObject temp = clone;

            temp.Remove("tilesets");
            temp.Remove("dimensions");
            temp.Remove("itemsets");
            temp.Remove("animations");
            temp.Remove("positions");
            temp.Remove("sprites");

            foreach (JProperty geometry in temp["geometry"])
            {
                ((JObject)geometry.Value).Remove("tiles");
                ((JObject)geometry.Value).Remove("placements");
                ((JObject)geometry.Value).Remove("groups");
            }

            var dataDirectory = new DirectoryInfo(GetVersionDirectory((int)json.version) + @"\data\");

            if (!dataDirectory.Exists)
            {
                dataDirectory.Create();
            }

            var filename = dataDirectory.FullName + @"\densedata.json";

            var fileinfo = new FileInfo(filename);


            if (!fileinfo.Exists)
            {
                var jsonString = JsonConvert.SerializeObject(temp, Formatting.Indented);

                using (StreamWriter writer = File.CreateText(filename))
                {
                    writer.Write(jsonString);
                }
            }

        }
    }
}
