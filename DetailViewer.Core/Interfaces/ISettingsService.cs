using DetailViewer.Core.Models;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    public interface ISettingsService
    {
        AppSettings LoadSettings();
        Task SaveSettingsAsync(AppSettings settings);
    }
}
