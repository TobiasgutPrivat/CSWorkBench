// singleton service
// TODO store somewhere
class UserService
{
    private Dictionary<string, User> users = []; //UserId, UserSettings

    public User GetCurrentUser(string UserId)
    {
        if (!users.ContainsKey(UserId))
            users[UserId] = new User();
        return users[UserId];
    }
} 