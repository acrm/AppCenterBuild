using System.Text.Json.Serialization;

namespace AppCenterBuild.Model.Branches
{
    public class ProtectionDto
    {
        public bool Enabled { get; set; }

        [JsonPropertyName("required_status_checks")]
        public RequiredStatusChecksDto? RequiredStatusChecks { get; set; }
    }
}
