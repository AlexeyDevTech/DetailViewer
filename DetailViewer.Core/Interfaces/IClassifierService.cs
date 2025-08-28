using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    public interface IClassifierService
    {
        Task LoadClassifiersAsync(string filePath = "eskd_classifiers.json");
        IEnumerable<ClassifierData> GetAllClassifiers();
        ClassifierData? GetClassifierByCode(string code);
    }
}