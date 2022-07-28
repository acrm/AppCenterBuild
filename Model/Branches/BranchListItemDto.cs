namespace AppCenterBuild.Model.Branches
{
    public class BranchListItemDto
    {
        public BranchDto? Branch { get; set; }

        public bool Configured { get; set; }

        public string? Trigger { get; set; }
    }
}
