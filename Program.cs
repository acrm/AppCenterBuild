using AppCenterBuild.Model.Branches;
using AppCenterBuild.Utils;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

var root = Directory.GetCurrentDirectory();
var dotenv = Path.Combine(root, ".env");
DotEnvLoader.Load(dotenv);

var config = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

var apiToken = config["ApiToken"]; // "4fc0e2526bc42932dd18597991b482bf3b36ebf2";
var user = config["UserName"]; //"acrm-sjork";
var app = config["AppName"]; //"UserApp";


using var httpClient = new HttpClient();
httpClient.BaseAddress = new Uri($"https://api.appcenter.ms/v0.1/apps/{user}/{app}/");
httpClient.DefaultRequestHeaders.Add("X-API-Token", apiToken);

var listBranchesRequest = new HttpRequestMessage(HttpMethod.Get, "branches");

var response = httpClient.Send(listBranchesRequest);
using var reader = new StreamReader(response.Content.ReadAsStream());
var responseBody = reader.ReadToEnd();

var data = JsonSerializer.Deserialize<BranchListItemDto[]>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
if (data == null) return;

var buildStartTasks = data
    .Select(item => httpClient.PostAsJsonAsync<BranchBuildRequestParams?>(
        $"branches/{item.Branch?.Name}/builds",
        new BranchBuildRequestParams { SourceVersion = item.Branch?.Commit?.Sha },
        CancellationToken.None))
    .ToArray();


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