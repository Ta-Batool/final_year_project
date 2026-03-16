using Model;
using MongoDB.Driver;

namespace API.Services
{
    public class WorkoutService
    {
        private readonly IMongoCollection<WorkoutExercise> _exercises;
        private readonly IMongoCollection<WorkoutRoutine> _routines;

        public WorkoutService(IMongoDatabase db)
        {
            _exercises = db.GetCollection<WorkoutExercise>("WorkoutExercises");
            _routines = db.GetCollection<WorkoutRoutine>("WorkoutRoutines");
        }

        // Exercises
        public Task<List<WorkoutExercise>> GetExercisesAsync()
            => _exercises.Find(_ => true).SortBy(x => x.Name).ToListAsync();

        public async Task<WorkoutExercise> CreateExerciseAsync(WorkoutExercise x)
        {
            await _exercises.InsertOneAsync(x);
            return x;
        }

        public Task DeleteExerciseAsync(string id)
            => _exercises.DeleteOneAsync(e => e.Id == id);

        // Routines
        public Task<List<WorkoutRoutine>> GetRoutinesAsync()
            => _routines.Find(_ => true).SortBy(x => x.Name).ToListAsync();

        public async Task<WorkoutRoutine> CreateRoutineAsync(WorkoutRoutine r)
        {
            await _routines.InsertOneAsync(r);
            return r;
        }

        public Task DeleteRoutineAsync(string id)
            => _routines.DeleteOneAsync(r => r.Id == id);
    }
}
