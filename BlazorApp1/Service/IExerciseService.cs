using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model;

namespace BlazorApp1.Service
{
    public interface IExerciseService
    {
        Task<List<ExerciseLog>> GetTodayAsync(string clientId);
        Task<List<ExerciseLog>> GetByDateAsync(string clientId, DateTime date);
        Task AddAsync(ExerciseLog log);
        Task DeleteAsync(string id);
    }
}
