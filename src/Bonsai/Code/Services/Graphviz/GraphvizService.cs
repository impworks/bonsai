using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace Bonsai.Code.Services.Graphviz;

/// <summary>
/// Runs the native Graphviz "dot" binary.
/// </summary>
public class GraphvizService: IGraphvizService
{
    public GraphvizService(IWebHostEnvironment env)
    {
        _env = env;
    }

    private readonly IWebHostEnvironment _env;
    private string _dotPath;

    /// <summary>
    /// Lays out several DOT sources in a single "dot" invocation.
    /// dot reads a stream of graphs from stdin and emits one JSON document per
    /// graph, so a search over hundreds of candidate layouts costs one process
    /// start instead of hundreds.
    /// </summary>
    public async Task<IReadOnlyList<GraphvizLayout>> RenderAsync(IReadOnlyList<string> sources, CancellationToken token)
    {
        if (sources.Count == 0)
            return [];

        var dotPath = GetDotPath();
        var startInfo = new ProcessStartInfo
        {
            FileName = dotPath,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        startInfo.ArgumentList.Add("-Tjson");

        using var proc = new Process { StartInfo = startInfo };

        try
        {
            proc.Start();
        }
        catch (Win32Exception ex)
        {
            // No executable, no point in being asked again for the next graph.
            throw new GraphvizNotFoundException(dotPath, ex);
        }

        // stdin has to be filled while stdout is being drained: dot lays out and
        // emits each graph as it reads it, and a full output pipe would otherwise
        // deadlock against a full input one.
        var feed = FeedAsync(proc, sources, token);
        var errors = proc.StandardError.ReadToEndAsync(token);

        List<GraphvizLayout> results;
        try
        {
            results = Parse(proc.StandardOutput);
        }
        catch (JsonException ex)
        {
            await feed;
            throw new Exception($"Failed to parse the Graphviz output: {await errors}", ex);
        }

        await feed;
        await proc.WaitForExitAsync(token);

        if (proc.ExitCode != 0)
            throw new Exception($"Graphviz exited with code {proc.ExitCode}: {await errors}");

        if (results.Count != sources.Count)
            throw new Exception($"Graphviz returned {results.Count} layouts for {sources.Count} graphs: {await errors}");

        return results;
    }

    #region Private helpers

    /// <summary>
    /// Writes every source into the process's stdin and closes it.
    /// </summary>
    private static async Task FeedAsync(Process proc, IReadOnlyList<string> sources, CancellationToken token)
    {
        try
        {
            foreach (var source in sources)
            {
                await proc.StandardInput.WriteAsync(source.AsMemory(), token);
                await proc.StandardInput.WriteAsync('\n');
            }
        }
        finally
        {
            proc.StandardInput.Close();
        }
    }

    /// <summary>
    /// Reads the concatenated JSON documents dot writes, one per graph.
    /// </summary>
    private static List<GraphvizLayout> Parse(TextReader output)
    {
        var results = new List<GraphvizLayout>();
        var serializer = new JsonSerializer();

        using var reader = new JsonTextReader(output) { SupportMultipleContent = true, CloseInput = false };
        while (reader.Read())
        {
            if (reader.TokenType != JsonToken.StartObject)
                continue;

            var graph = serializer.Deserialize<DotGraph>(reader);
            results.Add(Convert(graph));
        }

        return results;
    }

    /// <summary>
    /// Picks the canvas height and the node centres out of a parsed graph.
    /// </summary>
    private static GraphvizLayout Convert(DotGraph graph)
    {
        var height = double.NaN;
        var bb = (graph.Bb ?? "").Split(',');
        if (bb.Length == 4)
            height = ParseCoord(bb[3]);

        var nodes = new Dictionary<string, GraphvizPoint>();
        foreach (var obj in graph.Objects ?? [])
        {
            if (string.IsNullOrEmpty(obj.Name) || string.IsNullOrEmpty(obj.Pos))
                continue;

            var parts = obj.Pos.Split(',');
            if (parts.Length < 2)
                continue;

            nodes[obj.Name] = new GraphvizPoint(ParseCoord(parts[0]), ParseCoord(parts[1]));
        }

        return new GraphvizLayout { Height = height, Nodes = nodes };
    }

    private static double ParseCoord(string value)
    {
        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
            ? result
            : double.NaN;
    }

    /// <summary>
    /// Returns the path to the "dot" executable.
    /// A copy shipped next to the app wins, as with ffmpeg; otherwise it is
    /// looked up on PATH, which is where the Docker image's package puts it.
    /// </summary>
    private string GetDotPath()
    {
        if (_dotPath != null)
            return _dotPath;

        var executable = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "dot.exe" : "dot";
        var local = Path.Combine(_env.ContentRootPath, "External", "graphviz", executable);

        return _dotPath = File.Exists(local) ? local : "dot";
    }

    #endregion

    #region Parsed shape of dot's JSON output

    private class DotGraph
    {
        [JsonProperty("bb")]
        public string Bb { get; set; }

        [JsonProperty("objects")]
        public List<DotObject> Objects { get; set; }
    }

    private class DotObject
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("pos")]
        public string Pos { get; set; }
    }

    #endregion
}
