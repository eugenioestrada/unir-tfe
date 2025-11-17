using System.Diagnostics;
using System.Net.Http;
using NUnit.Framework;

namespace GameTribunal.UI.Tests;

/// <summary>
/// Starts the web application once before the Playwright test suite executes so the browser can reach it.
/// </summary>
[SetUpFixture]
public sealed class UiTestEnvironment
{
    private const string TargetUrl = "https://localhost:7000";
    private Process? _webAppProcess;

    [OneTimeSetUp]
    public async Task StartApplicationAsync()
    {
        var srcDirectory = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", ".."));
        var webProjectDirectory = Path.Combine(srcDirectory, "GameTribunal.Web");
        var projectFile = Path.Combine(webProjectDirectory, "GameTribunal.Web.csproj");

        if (!File.Exists(projectFile))
        {
            throw new FileNotFoundException($"No se encontró el proyecto web en {projectFile}.");
        }

        _webAppProcess = StartWebApplication(webProjectDirectory);
        await WaitForApplicationHealthyAsync(_webAppProcess, new Uri(TargetUrl), TimeSpan.FromSeconds(60));
        TestContext.Progress.WriteLine($"Aplicación iniciada para pruebas UI en {TargetUrl}.");
    }

    [OneTimeTearDown]
    public void StopApplication()
    {
        if (_webAppProcess is null)
        {
            return;
        }

        try
        {
            if (!_webAppProcess.HasExited)
            {
                _webAppProcess.Kill(entireProcessTree: true);
                _webAppProcess.WaitForExit((int)TimeSpan.FromSeconds(5).TotalMilliseconds);
            }
        }
        catch (Exception ex)
        {
            TestContext.Error.WriteLine($"No se pudo detener la aplicación web: {ex.Message}");
        }
        finally
        {
            _webAppProcess.Dispose();
            _webAppProcess = null;
        }
    }

    private static Process StartWebApplication(string workingDirectory)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = workingDirectory,
            Arguments = $"run --configuration Debug --urls \"{TargetUrl}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "Development";
        startInfo.EnvironmentVariables["ASPNETCORE_URLS"] = TargetUrl;

        var process = Process.Start(startInfo) ?? throw new InvalidOperationException("No se pudo iniciar la aplicación web para las pruebas UI.");

        process.OutputDataReceived += (_, args) =>
        {
            if (!string.IsNullOrWhiteSpace(args.Data))
            {
                TestContext.Progress.WriteLine(args.Data);
            }
        };

        process.ErrorDataReceived += (_, args) =>
        {
            if (!string.IsNullOrWhiteSpace(args.Data))
            {
                TestContext.Error.WriteLine(args.Data);
            }
        };

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return process;
    }

    private static async Task WaitForApplicationHealthyAsync(Process process, Uri targetUri, TimeSpan timeout)
    {
        using var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        using var client = new HttpClient(handler);
        var expiration = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < expiration)
        {
            if (process.HasExited)
            {
                throw new InvalidOperationException($"La aplicación salió de forma inesperada con código {process.ExitCode}.");
            }

            try
            {
                using var response = await client.GetAsync(targetUri);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
                // La app todavía no está lista; continuamos esperando.
            }

            await Task.Delay(500);
        }

        throw new TimeoutException($"La aplicación no estuvo lista en {timeout.TotalSeconds} segundos.");
    }
}
