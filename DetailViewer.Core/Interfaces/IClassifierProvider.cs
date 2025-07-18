
using DetailViewer.Core.Models;
using System.Collections.Generic;

namespace DetailViewer.Core.Interfaces
{
    public interface IClassifierProvider
    {
        ClassifierData GetClassifierByCode(string code);
        IEnumerable<ClassifierData> GetAllClassifiers();
    }
}
