using GeometrySketch.Model;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace GeometrySketch.DataProvider
{
    public class SettingsDataProvider : ISettingsDataProvider
    {
        private static readonly string _autosaveSettings = "AutosaveSettings.json";
        private static readonly StorageFolder _localFolder = ApplicationData.Current.LocalFolder;

        public async Task<Settings> AutoLoadSettingsAsync()
        {
            var storageFile = await _localFolder.TryGetItemAsync(_autosaveSettings) as StorageFile;
            Settings settings = new Settings();

            if (storageFile == null)
            {

            }

            else
            {
                using (var stream = await storageFile.OpenAsync(FileAccessMode.Read))
                {
                    using (var dataReader = new DataReader(stream))
                    {
                        await dataReader.LoadAsync((uint)stream.Size);
                        var json = dataReader.ReadString((uint)stream.Size);
                        Settings es = new Settings();
                        es = (Settings)JsonConvert.DeserializeObject<Settings>(json);
                        settings = es;
                    }
                }
            }

            return settings;
        }

        public async Task AutoSaveSettingsAsync(Settings settings)
        {
            var storageFile = await _localFolder.CreateFileAsync(_autosaveSettings, CreationCollisionOption.ReplaceExisting);

            using (var stream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (var dataWriter = new DataWriter(stream))
                {
                    var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                    dataWriter.WriteString(json);
                    await dataWriter.StoreAsync();
                }
            }
        }
    }
}
