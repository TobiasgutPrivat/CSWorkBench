using System.Text.Json.Serialization;

class User
{
    [JsonIgnore]
    public IDragDropChannel dragDropChannel; // create a dragDropChannel

    // some User settings
}