using DetailViewer.Core.Data;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public static class ClassifierHelper
    {
        public static async Task<Classifier> GetOrCreateClassifierAsync(ApplicationDbContext dbContext, IClassifierService classifierService, string code)
        {
            if (string.IsNullOrWhiteSpace(code) || !int.TryParse(code, out int classifierNumber))
            {
                return null;
            }

            var classifier = await dbContext.Classifiers
                .FirstOrDefaultAsync(c => c.Number == classifierNumber);

            if (classifier == null)
            {
                var classifierInfo = classifierService.GetClassifierByCode(code);
                if (classifierInfo == null) return null;

                classifier = new Classifier
                {
                    Number = classifierNumber,
                    Description = classifierInfo.Description
                };
                dbContext.Classifiers.Add(classifier);
                await dbContext.SaveChangesAsync();
            }

            return classifier;
        }
    }
}
