using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace DetailViewer.Api.Models
{
    public class Classifier
    {
        /// <summary>
        /// Получает или устанавливает уникальный идентификатор классификатора.
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// Получает или устанавливает имя классификатора детали.
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Получает или устанавливает номер классификатора детали (например, "000001", "000002").
        /// </summary>
        public int Number { get; set; } // например, "000001", "000002" и т.д.
        /// <summary>
        /// Получает или устанавливает описание классификатора.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Получает или устанавливает коллекцию номеров ЕСКД, связанных с данным классификатором.
        /// </summary>
        public ICollection<ESKDNumber>? ESKDNumbers { get; set; }
    }
}