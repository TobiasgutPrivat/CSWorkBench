
using Microsoft.JSInterop;

class SessionSettings
{
    User user { get; set; }
    private readonly IJSRuntime js;

    public SessionSettings(UserService userService, IJSRuntime js)
    {
        this.js = js;
        string? userId = GetUserIdAsync().GetAwaiter().GetResult();
        if (userId == null) {
            userId = new Guid().ToString();
            SetUserIdAsync(userId);
        };
        user = userService.GetCurrentUser(userId);
    }



    private async Task<string?> GetUserIdAsync()
    {
        return await js.InvokeAsync<string?>("localStorageHelper.getUserId");
    }

    private async Task SetUserIdAsync(string id)
    {
        await js.InvokeVoidAsync("localStorageHelper.setUserId", id);
    }
}