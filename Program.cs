using AppCenterBuild.Model.Branches;
using AppCenterBuild.Utils;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

IConfigurationRoot GetConfig()
{
    var root = Directory.GetCurrentDirectory();
    var dotenv = Path.Combine(root, ".env");
    DotEnvLoader.Load(dotenv);

    var config = new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .Build();
    return config;
}

var config = GetConfig();
var apiToken = config["ApiToken"];
var user = config["UserName"];
var app = config["AppName"];

using var httpClient = new HttpClient();
httpClient.BaseAddress = new Uri($"https://api.appcenter.ms/v0.1/apps/{user}/{app}/");
httpClient.DefaultRequestHeaders.Add("X-API-Token", apiToken);

var branchListItems = await httpClient.GetFromJsonAsync<BranchListItemDto[]>("branches");
if (branchListItems == null) return;

foreach (var item in branchListItems)
{
    Console.WriteLine($"{item.Branch?.Name}, {item.Branch?.Commit?.Sha}");
}

//var buildStartTasks = branchListItems
//    .Select(item => httpClient.PostAsJsonAsync<BranchBuildRequestParams?>(
//        $"branches/{item.Branch?.Name}/builds",
//        new BranchBuildRequestParams { SourceVersion = item.Branch?.Commit?.Sha },
//        CancellationToken.None))
//    .ToArray();

//Task.WaitAll(buildStartTasks);

var buildsInfoRequestTasks = branchListItems
    .Select(branchItem =>
    {
        return httpClient
            .GetFromJsonAsync<BranchBuildListItem[]>($"branches/{branchItem.Branch?.Name}/builds", CancellationToken.None)
            .ContinueWith(previous =>
            {
                var currentVersionBuild = previous.Result?.FirstOrDefault(buildItem => buildItem.SourceVersion == branchItem.Branch?.Commit?.Sha);
                return currentVersionBuild;
            });
    })
    .ToArray();
Task.WaitAll(buildsInfoRequestTasks);

foreach (var task in buildsInfoRequestTasks.Where(task => task.Result != null))
{
    Console.WriteLine($"{task.Result.BuildNumber} {task.Result.LastChangedDate}");
}


public class BranchBuildListItem
{
    public int Id { get; set; }

    public string? BuildNumber { get; set; }

    public DateTime? QueueTime { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? LastChangedDate { get; set; }

    public string? Status { get; set; }

    public string? Reason { get; set; }

    public string? SourceBranch { get; set; }

    public string? SourceVersion { get; set; }

    public string[]? Tags { get; set; }
}


//var listBranchesRequest = new HttpRequestMessage(HttpMethod.Get, "branches");

//var response = httpClient.Send(listBranchesRequest);
//using var reader = new StreamReader(response.Content.ReadAsStream());
//var responseBody = reader.ReadToEnd();

//var data = JsonSerializer.Deserialize<BranchListItemDto[]>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

//var buildStartTasks = data
//    .Select(item => httpClient.PostAsJsonAsync<BranchBuildRequestParams?>(
//        $"branches/{item.Branch?.Name}/builds",
//        new BranchBuildRequestParams { SourceVersion = item.Branch?.Commit?.Sha },
//        CancellationToken.None))
//    .ToArray();

//Task.WaitAll(buildStartTasks);


public class BranchBuildRequestParams
{
    public string? SourceVersion { get; set; }

    public bool Debug { get; set; }
}

//foreach (var item in data)
//{
//    Console.WriteLine($"{item.Branch?.Name}, {item.Branch?.Commit?.Sha}");
//    //var startBranchBuildRequest = new HttpRequestMessage(HttpMethod.Post, $"https://api.appcenter.ms/v0.1/apps/{user}/{app}/branches/main/builds");
//    //startBranchBuildRequest.Headers.Add("X-API-Token", apiToken);
//    //var startBranchBuildResponse = httpClient.Send(startBranchBuildRequest);
//    //using var responseReader = new StreamReader(startBranchBuildResponse.Content.ReadAsStream());
//    //var startBranchBuildResponseBody = responseReader.ReadToEnd();

//    await 

//}