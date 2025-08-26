using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace DetailViewer.Core.Models
{
    public class Classifier
    {
        [Key]
        public int Id { get; set; }
        // имя классификатора детали 
        public string Name { get; set; }
        // номер классификатора детали
        public int Number { get; set; } // например, "000001", "000002" и т.д.
        public string Code => Number.ToString("D6");
        public string Description { get; set; }

        public ICollection<ESKDNumber> ESKDNumbers { get; set; }
    }
}
