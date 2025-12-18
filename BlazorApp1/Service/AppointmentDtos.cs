namespace BlazorApp1.Service
{
    public record TimeSlotDto(DateTime Start, DateTime End);

    public record AvailabilityResponseDto(
        string DoctorId,
        DateTime Date,
        int SlotMinutes,
        List<TimeSlotDto> AvailableSlots
    );

    public record BookAppointmentRequest(
        string PatientUserId,
        string DoctorId,
        DateTime Start,
        int DurationMinutes,
        string? Notes
    );
}
