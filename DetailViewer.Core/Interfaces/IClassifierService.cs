using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    public interface IClassifierService
    {
        Task LoadClassifiersAsync();
        IEnumerable<Classifier> GetAllClassifiers();
        Classifier? GetClassifierByNumber(int number);
    }
}