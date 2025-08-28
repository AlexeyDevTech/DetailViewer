using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DetailViewer.Core.Models
{
    /// <summary>
    /// Представляет структуру данных для одного узла в JSON-файле классификатора.
    /// Используется для десериализации.
    /// </summary>
    public class ClassifierData
    {
        /// <summary>
        /// Код класса.
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; set; }

        /// <summary>
        /// Описание класса.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// Список путей к изображениям.
        /// </summary>
        [JsonPropertyName("image_paths")]
        public List<string> ImagePaths { get; set; }

        /// <summary>
        /// Список дочерних узлов классификатора.
        /// </summary>
        [JsonPropertyName("children")]
        public List<ClassifierData> Children { get; set; }
    }
}