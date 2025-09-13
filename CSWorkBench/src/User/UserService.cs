// singleton service
// TODO store somewhere
public class UserService
{
    private Dictionary<string, User> users = []; //UserId, UserSettings
    
    public UserService()
    {
        //TODO load
    }
    
    public void Save()
    {
        //TODO save
    }

    public User GetCurrentUser(string UserId)
    {
        if (!users.ContainsKey(UserId))
            users[UserId] = new User();
        return users[UserId];
    }
} 