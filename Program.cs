using AppCenterBuild.Model;
using AppCenterBuild.Model.Branches;
using AppCenterBuild.Model.Builds;
using AppCenterBuild.Utils;
using System.Net.Http.Json;

var config = Helper.GetConfig();
var apiToken = config["ApiToken"];
var userName = config["UserName"];
var appName = config["AppName"];
var buildCheckSeconds = int.Parse(config["BuildCheckSeconds"]);

using var httpClient = new HttpClient();
httpClient.BaseAddress = new Uri($"https://api.appcenter.ms/v0.1/apps/{userName}/{appName}/");
httpClient.DefaultRequestHeaders.Add("X-API-Token", apiToken);

Func<Task<BranchListItemDto[]>> getBranchesAsync = async () => (await httpClient.GetFromJsonAsync<BranchListItemDto[]>("branches")) ?? Array.Empty<BranchListItemDto>();

Func<BranchDto, Task<BranchBuildPair>> getOrStartBuildAsync = branch =>
    httpClient
        .GetFromJsonAsync<BranchBuildListItem[]>($"branches/{branch.Name}/builds", CancellationToken.None)
        .ContinueWith(previous =>
        {
            var currentVersionBuild = previous.Result?.FirstOrDefault(buildItem =>
                buildItem.SourceVersion == branch?.Commit?.Sha
                && buildItem.Result != "canceled"
                && buildItem.Result != "failed");
            if (currentVersionBuild != null)
            {
                return Task.FromResult(new BranchBuildPair(branch, (BranchBuildListItem?)currentVersionBuild));
            }

            return httpClient
                .PostAsJsonAsync<BranchBuildRequestParams?>(
                    $"branches/{branch.Name}/builds",
                    new BranchBuildRequestParams { SourceVersion = branch.Commit?.Sha },
                    CancellationToken.None)
                .ContinueWith(previous => new BranchBuildPair(branch, currentVersionBuild));
        })
        .Unwrap();

Func<BranchBuildPair[], Task<BranchBuildPair[]>> waitAllBuildsComplteteAsync = async branchBuildPairs =>
    await Task.WhenAll(branchBuildPairs.Select(async pair =>
    {
        var branch = pair.Branch;
        var build = pair.Build;
        if (build == null || build.BuildNumber == null || build.FinishTime == null || build.StartTime == null || build.Status == "completed")
        {
            return pair;
        }

        return await Task.Run(async () =>
        {
            while (true) // todo: cancelation
            {
                var currentBuildInfo = await httpClient
                    .GetFromJsonAsync<BranchBuildListItem>($"branches/{branch.Name}/builds/{build.Id}", CancellationToken.None);
                if (currentBuildInfo != null && currentBuildInfo.Status == "completed")
                {
                    return new BranchBuildPair(branch, currentBuildInfo);
                }

                await Task.Delay(buildCheckSeconds * 1000);
            }
        });
    }));

Action<BranchBuildPair[]> printBuildsInfo = branchBuildPairs =>
{
    foreach (var (branch, build) in branchBuildPairs)
    {
        if (build == null || build.BuildNumber == null || build.FinishTime == null || build.StartTime == null)
        {
            Console.WriteLine($"{branch?.Name} no build information");
            continue;
        }

        if (build.Status != "completed")
        {
            Console.WriteLine($"{branch?.Name} build in progress");
            continue;
        }

        var duration = Math.Ceiling((build.FinishTime - build.StartTime).Value.TotalSeconds);
        var buildNumber = build.BuildNumber;
        var slash = "%2F";
        var link = $"https://appcenter.ms/download?url={slash}v0.1{slash}apps{slash}{userName}{slash}{appName}{slash}builds{slash}{buildNumber}{slash}downloads{slash}logs";
        Console.WriteLine($"{branch?.Name} build {build?.Status} in {duration} seconds. Link to build logs {link}");
    }
};

await Task.WhenAll(
    (await getBranchesAsync())
    .Select(branchItem => getOrStartBuildAsync(branchItem.Branch)))
.ContinueWith(async previous => await waitAllBuildsComplteteAsync(previous.Result))
.Unwrap()
.ContinueWith(previous => printBuildsInfo(previous.Result));
