namespace API.Ai
{
    public interface IAiAssistantService
    {
        Task<string> GetPatientReplyAsync(string userId, string message);
        Task<string> GetDoctorReplyAsync(string doctorId, string message);
    }
}
