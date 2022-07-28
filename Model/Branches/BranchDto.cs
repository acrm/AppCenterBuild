using System.Text.Json.Serialization;

namespace AppCenterBuild.Model.Branches
{
    public class BranchDto
    {
        public string? Name { get; set; }

        public CommitDto? Commit { get; set; }

        public bool Protected { get; set; }

        public ProtectionDto? Protection { get; set; }

        [JsonPropertyName("protection_url")]
        public string? ProtectionUrl { get; set; }
    }
}
