using AppCenterBuild.Model.Branches;
using AppCenterBuild.Model.Builds;

namespace AppCenterBuild.Model
{
    record BranchBuildPair(BranchDto Branch, BranchBuildListItem? Build);
}
