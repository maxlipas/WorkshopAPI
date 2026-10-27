using System.Text.Json.Serialization;

namespace RestAPI.Models;

public class Worker
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Position { get; set; } = null!;
    public decimal DiscountPercent { get; set; }
    public int WorkshopId { get; set; }

    [JsonIgnore]
    public Workshop? Workshop { get; set; }

    [JsonIgnore]
    public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
}
