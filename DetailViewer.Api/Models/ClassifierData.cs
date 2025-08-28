#pragma warning disable CS8618

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DetailViewer.Api.Models
{
    public class ClassifierData
    {
        /// <summary>
        /// Получает или устанавливает код классификатора.
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; set; }

        /// <summary>
        /// Получает или устанавливает описание классификатора.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// Получает или устанавливает список путей к изображениям, связанных с классификатором.
        /// </summary>
        [JsonPropertyName("image_paths")]
        public List<string> ImagePaths { get; set; }

        /// <summary>
        /// Получает или устанавливает список дочерних классификаторов.
        /// </summary>
        [JsonPropertyName("children")]
        public List<ClassifierData> Children { get; set; }
    }
}