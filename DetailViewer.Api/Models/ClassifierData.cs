#pragma warning disable CS8618

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DetailViewer.Api.Models
{
    public class ClassifierData
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("image_paths")]
        public List<string> ImagePaths { get; set; }

        [JsonPropertyName("children")]
        public List<ClassifierData> Children { get; set; }
    }
}