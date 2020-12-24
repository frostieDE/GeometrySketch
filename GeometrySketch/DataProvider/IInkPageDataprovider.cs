using GeometrySketch.Model;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace GeometrySketch.DataProvider
{
    public interface IInkPageDataprovider
    {
        Task OpenInkPageAsync(InkCanvas inkCanvas, InkPage inkPage, StorageFile file);
        Task SaveInkPageAsync(InkCanvas inkCanvas, InkPage inkPage, StorageFile file);
    }
}
