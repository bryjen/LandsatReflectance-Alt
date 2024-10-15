using Microsoft.AspNetCore.Components;

namespace LandsatReflectance.UI.Components;

public partial class FullPageLoadingOverlay : ComponentBase
{
    private bool m_isVisible = false;

    public async Task ExecuteWithOverlay(Func<Task> work, Func<Task> onFinishedCallback)
    {
        m_isVisible = true;
        StateHasChanged();
        
        await work();
        m_isVisible = false;
        
        StateHasChanged();
        await onFinishedCallback();
    }
}