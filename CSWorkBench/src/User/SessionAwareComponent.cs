namespace CSWorkBench;

using Microsoft.AspNetCore.Components;

public class SessionAwareComponent : ComponentBase, IDisposable
{
    [Inject] protected SessionSettings sessionSettings { get; set; } = null!;

    protected override void OnInitialized()
    {
        sessionSettings.OnChange += StateHasChanged;
        base.OnInitialized();
    }

    public void Dispose()
    {
        sessionSettings.OnChange -= StateHasChanged;
    }
}
