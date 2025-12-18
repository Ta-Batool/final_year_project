namespace API.DTOs
{
    public record HealthAlertDto(
        string Type,        // BP / Glucose / Weight
        string Severity,    // Normal / Warning / Danger
        string Message,
        DateTime LoggedAt
    );
}
