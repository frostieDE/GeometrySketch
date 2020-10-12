using GeometrySketch.Model;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Controls;

namespace GeometrySketch.DataProvider
{
    public class InkPageDataprovider : IInkPageDataprovider
    {
        public async Task OpenInkPageAsync(InkCanvas inkCanvas, InkPage inkPage, StorageFile file)
        {
            IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
            using (var inputStream = stream.GetInputStreamAt(0))
            {
                using (var dataReader = new DataReader(inputStream))
                {
                    InkPage ip = new InkPage();
                    await dataReader.LoadAsync(100);
                    var json = dataReader.ReadString(100);
                    ip = (InkPage)JsonConvert.DeserializeObject<InkPage>(json);
                    inkPage.IsGridVisible = ip.IsGridVisible;
                }
            }
            using (var inputStream = stream.GetInputStreamAt(100))
            {
                await inkCanvas.InkPresenter.StrokeContainer.LoadAsync(inputStream);
            }
            stream.Dispose();            
        }
        public async Task SaveInkPageAsync(InkCanvas inkCanvas, InkPage inkPage, StorageFile file)
        {
            await FileIO.WriteBytesAsync(file, new byte[0]);
            CachedFileManager.DeferUpdates(file);

            IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            using (IOutputStream outputStream = stream.GetOutputStreamAt(0))
            {
                using (var dataWriter = new DataWriter(outputStream))
                {                    
                    var json = JsonConvert.SerializeObject(inkPage, Formatting.Indented);
                    dataWriter.WriteString(json);
                    await dataWriter.StoreAsync();
                }
            }
            using (IOutputStream outputStream = stream.GetOutputStreamAt(100))
            {
                await inkCanvas.InkPresenter.StrokeContainer.SaveAsync(outputStream);
                await outputStream.FlushAsync();
            }
            stream.Dispose();
        }
    }
}
