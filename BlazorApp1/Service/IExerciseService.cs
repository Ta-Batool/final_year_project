using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model;

namespace BlazorApp1.Service
{
    public interface IExerciseService
    {
        Task<List<ExerciseEntry>> GetForDateAsync(string clientId, DateTime date);
        Task AddAsync(ExerciseEntry entry);
        Task UpdateStatusAsync(string id, ExerciseStatus status);
        Task DeleteAsync(string id);
    }
}
