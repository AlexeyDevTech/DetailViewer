using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace DetailViewer.Core.Models
{
    /// <summary>
    /// Представляет класс изделия согласно классификатору ЕСКД.
    /// </summary>
    public class Classifier
    {
        /// <summary>
        /// Уникальный идентификатор классификатора.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Наименование класса изделия.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Числовой код класса (например, 711111).
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Строковое представление кода класса с ведущими нулями (например, "711111").
        /// </summary>
        public string Code => Number.ToString("D6");

        /// <summary>
        /// Описание класса изделия.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Список путей к изображениям, связанным с этим классом.
        /// </summary>
        public List<string>? ImagePaths { get; set; }

        /// <summary>
        /// Коллекция децимальных номеров, использующих этот классификатор.
        /// </summary>
        public ICollection<ESKDNumber> ESKDNumbers { get; set; }
    }
}