using System.Threading.Tasks;

namespace BlazorApp1.Service
{
    public interface ITranslationService
    {
        Task<string[]> TranslateAsync(string targetLanguage, string[] texts);
    }
}
