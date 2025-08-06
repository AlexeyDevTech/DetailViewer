
using DetailViewer.Core.Models;
using System.Collections.Generic;

namespace DetailViewer.Core.Interfaces
{
    public interface IClassifierService
    {
        ClassifierData GetClassifierByCode(string code);
        IEnumerable<ClassifierData> GetAllClassifiers();
    }
}
