using System.Text.Json.Serialization;

namespace RestAPI.Models;

public class Workwear
{
    public int Id { get; set; }
    public string Type { get; set; } = null!;
    public int WearMonths { get; set; }
    public decimal UnitCost { get; set; }

    [JsonIgnore]
    public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
}
