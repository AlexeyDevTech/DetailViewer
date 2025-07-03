using DetailViewer.Core.Models;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    public interface ISettingsService
    {
        Task<AppSettings> LoadSettingsAsync();
        Task SaveSettingsAsync(AppSettings settings);
    }
}
