using System.Text.Json.Serialization;

namespace RestAPI.Models;

public class Workshop
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string HeadFullName { get; set; } = null!;

    [JsonIgnore]
    public ICollection<Worker> Workers { get; set; } = new List<Worker>();
}
