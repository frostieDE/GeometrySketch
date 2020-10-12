using GeometrySketch.Model;
using System.Threading.Tasks;

namespace GeometrySketch.DataProvider
{
    public interface ISettingsDataProvider
    {
        Task<Settings> AutoLoadSettingsAsync();
        Task AutoSaveSettingsAsync(Settings settings);
    }
}
