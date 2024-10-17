using System.Diagnostics;
using System.IO.Compression;
using GoogleMapsComponents.Maps;
using LandsatReflectance.Models;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

namespace LandsatReflectance.UI.Services;

public class Wrs2AreasService
{
    private readonly IWebAssemblyHostEnvironment m_environment;
    private readonly IJSRuntime m_jsRuntime;
    
    private Wrs2Area[] m_wrs2Areas = [];
    
    
    public Wrs2AreasService(IWebAssemblyHostEnvironment environment, IJSRuntime jsRuntime)
    {
        m_environment = environment;
        m_jsRuntime = jsRuntime;
    }

    public bool IsInitialized()
    {
        return m_wrs2Areas.Length == 0;
    }

    public async Task InitWrs2Areas()
    {
        if (m_wrs2Areas.Length == 0)
        {
            m_wrs2Areas = await FetchWrs2Areas();
        }
        
        if (m_environment.IsDevelopment())
        {
            Console.WriteLine($"[Wrs2AreasService] Loaded {m_wrs2Areas.Length} areas.");

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (var wrs2Area in m_wrs2Areas)
            {
                IEnumerable<IEnumerable<LatLngLiteral>> paths = m_wrs2Areas
                    .Select(area => area.LatLongCoordinates.Select(latLong => new LatLngLiteral(latLong.Latitude, latLong.Longitude)));
                
                var polygonOptions = new PolygonOptions
                {
                    Paths = paths
                };
                
                // do something else
            }
            stopwatch.Start();
            
            Console.WriteLine($"[Wrs2AreasService] Finished iterating in {stopwatch.Elapsed.TotalSeconds:F}s ({stopwatch.Elapsed.TotalMilliseconds}ms)");
        }
    }
    
    private async Task<Wrs2Area[]> FetchWrs2Areas()
    {
        var fileBytes = await m_jsRuntime.InvokeAsync<byte[]>("fetchWrs2AreasGz");

        if (fileBytes == null || fileBytes.Length == 0)
        {
            // TODO: Create custom exception & figure out exception handling
            throw new Exception("Failed to fetch or retrieve the Gzip file.");
        }

        using var inputStream = new MemoryStream(fileBytes);
        await using var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);
        using var decompressedStream = new MemoryStream();
        await gzipStream.CopyToAsync(decompressedStream);
        var decompressedBytes = decompressedStream.ToArray();

        if (m_environment.IsDevelopment())
        {
             Console.WriteLine($"[Wrs2AreasService] Loaded wrs2 areas file with #bytes = {decompressedBytes.Length}");
        }

        using var decompressedByteMemoryStream = new MemoryStream(decompressedBytes);
        return ProtoBuf.Serializer.Deserialize<Wrs2Area[]>(decompressedByteMemoryStream);
    }
}