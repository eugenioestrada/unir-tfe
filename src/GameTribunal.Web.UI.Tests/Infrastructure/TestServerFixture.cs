using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GameTribunal.Web.UI.Tests.Infrastructure;

public sealed class TestServerFixture : IAsyncLifetime
{
    public const string CollectionName = "Playwright UI Tests";

    private Process? _process;
    private Uri? _baseAddress;

    public Uri BaseAddress => _baseAddress ?? throw new InvalidOperationException("Test server is not running.");

    public async Task InitializeAsync()
    {
        var projectPath = GetWebProjectPath();
        var port = GetAvailablePort();

        _process = StartWebHostProcess(projectPath, port);
        _baseAddress = new Uri($"http://127.0.0.1:{port}");

        await WaitForServerReady(_baseAddress, _process).ConfigureAwait(false);
    }

    public async Task DisposeAsync()
    {
        if (_process == null)
        {
            return;
        }

        try
        {
            if (!_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
            }

            await _process.WaitForExitAsync().ConfigureAwait(false);
        }
        catch (InvalidOperationException)
        {
            // Process already exited; nothing else to do.
        }
        finally
        {
            _process.Dispose();
            _process = null;
        }
    }

    private static string GetWebProjectPath()
    {
        var candidate = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "GameTribunal.Web"));
        if (!Directory.Exists(candidate))
        {
            throw new DirectoryNotFoundException($"Unable to locate GameTribunal.Web project at '{candidate}'.");
        }

        return candidate;
    }

    private static int GetAvailablePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static Process StartWebHostProcess(string projectPath, int port)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --no-build --no-launch-profile --project \"{projectPath}\"",
            WorkingDirectory = projectPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.Environment["ASPNETCORE_URLS"] = $"http://127.0.0.1:{port}";
        startInfo.Environment["DOTNET_ENVIRONMENT"] = "Development";
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";

        var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start dotnet run process.");

        process.OutputDataReceived += (_, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                Console.WriteLine($"[web] {e.Data}");
            }
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                Console.Error.WriteLine($"[web] {e.Data}");
            }
        };

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return process;
    }

    private static async Task WaitForServerReady(Uri baseAddress, Process hostProcess)
    {
        using var client = new HttpClient { BaseAddress = baseAddress };

        var timeout = TimeSpan.FromSeconds(45);
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            if (hostProcess.HasExited)
            {
                throw new InvalidOperationException($"dotnet run exited early with code {hostProcess.ExitCode}.");
            }

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                var response = await client.GetAsync("/", cts.Token).ConfigureAwait(false);

                if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
                // Server is not ready yet; keep waiting.
            }
            catch (TaskCanceledException)
            {
                // Request timed out; retry until deadline.
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250)).ConfigureAwait(false);
        }

        throw new TimeoutException($"Timed out waiting for server at {baseAddress} to become available.");
    }
}

[CollectionDefinition(TestServerFixture.CollectionName)]
public sealed class PlaywrightCollection : ICollectionFixture<TestServerFixture>
{
}
