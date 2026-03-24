using TasteItApi.Graph.Admin.Dtos;
using TasteItApi.Models;

namespace TasteItApi.Graph.Admin.Services
{
    public interface IAdminGraphService
    {
        Task<IReadOnlyList<AdminReportedRecipeDto>> GetRecipesReportedAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AdminReportedRecipeDto>> GetRecipesReportedFilteredAsync(string? name, string? creator, bool? active, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AdminRecipeByIdDto>> GetRecipeReportedByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<RecipeWEB>> ChangeRecipeStateAsync(int recipeId, bool value, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AdminReportDto>> GetReportsOnRecipeAsync(int id, CancellationToken cancellationToken = default);
    }
}
