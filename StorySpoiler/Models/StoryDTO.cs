using System.Text.Json.Serialization;

namespace StorySpoiler.Models
{
    internal class StoryDTO
    {
        [JsonPropertyName("Title")]
        public string? Name { get; set; }

        [JsonPropertyName("Description")]
        public string? Description { get; set; }

        [JsonPropertyName("Url")]
        public string? Url { get; set; }
    }
}
