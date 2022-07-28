using System.Text.Json.Serialization;

namespace AppCenterBuild.Model.Branches
{
    public class RequiredStatusChecksDto
    {
        [JsonPropertyName("enforcement_level")]
        public string? EnforcementLevel { get; set; }

        public object[]? Contexts { get; set; }

        public object[]? Checks { get; set; }
    }
}
