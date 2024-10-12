using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using LandsatReflectance.Common.Models;
using LandsatReflectance.Common.Models.Request;
using LandsatReflectance.Common.Models.ResponseModels;
using Microsoft.AspNetCore.Components;

namespace LandsatReflectance.UI.Pages;

public partial class Dashboard : ComponentBase
{
    private List<Target> m_targets = new();

    protected override async Task OnInitializedAsync()
    {
    }
    
    protected override Task OnAfterRenderAsync(bool isFirstRender)
    {
        return Task.CompletedTask;
    }
}
