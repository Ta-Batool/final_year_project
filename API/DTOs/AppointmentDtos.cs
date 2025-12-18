namespace API.DTOs
{
    public record TimeSlotDto(System.DateTime Start, System.DateTime End);

    public record AvailabilityResponseDto(
        string DoctorId,
        System.DateTime Date,
        int SlotMinutes,
        System.Collections.Generic.List<TimeSlotDto> AvailableSlots
    );

    public record BookAppointmentRequest(
        string PatientUserId,
        string DoctorId,
        System.DateTime Start,
        int DurationMinutes,
        string? Notes
    );
}
