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
    
    public Wrs2Area[] Wrs2Areas { get; private set; } = [];
    
    
    public Wrs2AreasService(IWebAssemblyHostEnvironment environment, IJSRuntime jsRuntime)
    {
        m_environment = environment;
        m_jsRuntime = jsRuntime;
    }

    public bool IsInitialized()
    {
        return Wrs2Areas.Length == 0;
    }

    public async Task InitWrs2Areas()
    {
        if (Wrs2Areas.Length == 0)
        {
            Wrs2Areas = await FetchWrs2Areas();
        }
        
        if (m_environment.IsDevelopment())
        {
            Console.WriteLine($"[Wrs2AreasService] Loaded {Wrs2Areas.Length} areas.");
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
        return ProtoBuf.Serializer.Deserialize<Wrs2Area[]>(decompressedByteMemoryStream).ToArray();
    }
}