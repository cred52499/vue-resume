namespace backend;

public class VisitLog
{
    public int Id { get; set; }
    public DateTime VisitedAt { get; set; } = DateTime.UtcNow;
}