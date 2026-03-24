using TasteItApi.Graph.Admin.Dtos;

namespace TasteItApi.Graph.Admin.Repositories
{
    public interface IAdminGraphRepository
    {
        Task<IReadOnlyList<AdminReportedRecipeDto>> GetRecipesReportedAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AdminReportedRecipeDto>> GetRecipesReportedFilteredAsync(string? name, string? creator, bool? active, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AdminRecipeByIdDto>> GetRecipeReportedByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Models.RecipeWEB>> ChangeRecipeStateAsync(int recipeId, bool value, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AdminReportDto>> GetReportsOnRecipeAsync(int id, CancellationToken cancellationToken = default);
    }
}
