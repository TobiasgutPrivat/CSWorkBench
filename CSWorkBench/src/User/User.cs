using System.Text.Json.Serialization;

class User
{
    [JsonIgnore]
    public IDragDropChannel dragDropChannel  { get; set; } // create a dragDropChannel

    // some User settings
}