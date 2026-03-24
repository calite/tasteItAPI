using TasteItApi.Graph.Admin.Dtos;
using TasteItApi.Graph.Admin.Repositories;
using TasteItApi.Models;

namespace TasteItApi.Graph.Admin.Services
{
    public class AdminGraphService : IAdminGraphService
    {
        private readonly IAdminGraphRepository _repository;

        public AdminGraphService(IAdminGraphRepository repository)
        {
            _repository = repository;
        }

        public Task<IReadOnlyList<AdminReportedRecipeDto>> GetRecipesReportedAsync(CancellationToken cancellationToken = default)
        {
            return _repository.GetRecipesReportedAsync(cancellationToken);
        }

        public Task<IReadOnlyList<AdminReportedRecipeDto>> GetRecipesReportedFilteredAsync(string? name, string? creator, bool? active, CancellationToken cancellationToken = default)
        {
            return _repository.GetRecipesReportedFilteredAsync(name, creator, active, cancellationToken);
        }

        public Task<IReadOnlyList<AdminRecipeByIdDto>> GetRecipeReportedByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repository.GetRecipeReportedByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<RecipeWEB>> ChangeRecipeStateAsync(int recipeId, bool value, CancellationToken cancellationToken = default)
        {
            return _repository.ChangeRecipeStateAsync(recipeId, value, cancellationToken);
        }

        public Task<IReadOnlyList<AdminReportDto>> GetReportsOnRecipeAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repository.GetReportsOnRecipeAsync(id, cancellationToken);
        }
    }
}
