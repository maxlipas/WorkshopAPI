using System.Text.Json.Serialization;

namespace RestAPI.Models;

public class Receipt
{
    public int Id { get; set; }
    public int WorkerId { get; set; }
    public int WorkwearId { get; set; }
    public DateOnly ReceivedAt { get; set; }
    public bool Signed { get; set; }

    [JsonIgnore]
    public Worker? Worker { get; set; }

    [JsonIgnore]
    public Workwear? Workwear { get; set; }
}
