namespace CSWorkBench;

using Microsoft.JSInterop;

public class SessionSettings
{
    public User? user { get; set; }
    private readonly IJSRuntime js;
    private readonly UserService userService;
    public event Action? OnChange;

    public SessionSettings(UserService userService, IJSRuntime js)
    {
        this.userService = userService;
        this.js = js;
    }

    public async Task InitializeAsync()
    {
        string? userId = await GetUserIdAsync();
        if (userId == null)
        {
            userId = new Guid().ToString();
            await SetUserIdAsync(userId);
        }
        user = userService.GetCurrentUser(userId);
        NotifyStateChanged();
        Console.WriteLine(userId + " " + user);
    }

    private async Task<string?> GetUserIdAsync()
    {
        return await js.InvokeAsync<string?>("localStorageHelper.getUserId");
    }

    private async Task SetUserIdAsync(string id)
    {
        await js.InvokeVoidAsync("localStorageHelper.setUserId", id);
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}