// singleton service
// TODO store somewhere
class UserService
{
    private Dictionary<int, User> users = []; //UserId, UserSettings

    public User GetCurrentUser()
    {
        //TODO get or create user based on localstorage entry (one browserApp = one User)
        throw new NotImplementedException();
    }
} 