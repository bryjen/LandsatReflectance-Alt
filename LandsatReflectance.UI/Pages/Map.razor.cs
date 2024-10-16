using GoogleMapsComponents;
using GoogleMapsComponents.Maps;
using LandsatReflectance.Common.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MouseEvent = GoogleMapsComponents.Maps.MouseEvent;

namespace LandsatReflectance.UI.Pages;

public partial class Map : ComponentBase
{
    [Inject]
    public required IWebAssemblyHostEnvironment Environment { get; set; } 
    
    [Inject]
    public required ISnackbar Snackbar { get; set; } 
    
    private GoogleMap m_googleMap = null!;
    private MapOptions m_mapOptions = null!;

    private const string ParentDivHeight = "height: calc(100vh - (var(--mud-appbar-height) - var(--mud-appbar-height) / 4))";

    
    protected override void OnInitialized()
    {
        m_mapOptions = new MapOptions
        {
            Zoom = 2,
            Center = new LatLngLiteral
            {
                Lat = 0,
                Lng = 0 
            },
            MapTypeId = MapTypeId.Roadmap,
            DisableDefaultUI = true,
            
            ZoomControl = true,
        };
    }

    private async Task OnAfterMapRender()
    {
        if (Environment.IsDevelopment())
        {
            Snackbar.Add("Map loaded", Severity.Info);
        }
        
        await m_googleMap.InteropObject.AddListener<MouseEvent>("click", mouseEvents => { _ = OnClick(mouseEvents); });
        
        // await LatLngBounds.CreateAsync(m_googleMap.JsRuntime);
    }

    private async Task OnClick(MouseEvent e)
    {
        if (Environment.IsDevelopment())
        {
            Snackbar.Add($"Left clicked: {e.LatLng.Lat:F}, {e.LatLng.Lng:F}", Severity.Info);
        }

        var newTarget = new Target
        {
        };
        
        var markerOptions = new MarkerOptions
        {
            Position = e.LatLng,
            Map = m_googleMap.InteropObject,
            // Title = "something",
            // Label = $"Marker @ {e.LatLng.Lat:F}, {e.LatLng.Lng:F}",
            Draggable = false,
            Icon = new Icon
            {
                Url = "http://maps.google.com/mapfiles/ms/icons/blue-dot.png"
            }
        };

        var newMarker = await Marker.CreateAsync(m_googleMap.JsRuntime, markerOptions);
        
        if (Environment.IsDevelopment())
        {
            Snackbar.Add($"Left clicked: {e.LatLng.Lat:F}, {e.LatLng.Lng:F}", Severity.Info);
            await newMarker.AddListener<MouseEvent>("click", async mouseEvent =>
            {
                await mouseEvent.Stop();
                await OnMarkerClicked(newTarget, newMarker);
            });
        }
    }

    private async Task OnMarkerClicked(Target target, Marker marker)
    {
        string markerLabel = await marker.GetLabelText();
        string markerTitle = await marker.GetTitle();
        Snackbar.Add($"Marker clicked with: (Label: \"{markerLabel}\"), (Title: \"{markerTitle}\")", Severity.Info);
    }
}