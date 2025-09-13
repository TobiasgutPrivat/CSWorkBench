using System.Text.Json.Serialization;

public class User
{
    [JsonIgnore]
    public IDragDropChannel dragDropChannel  { get; set; } // create a dragDropChannel

    // some User settings
}