namespace VersionLifecycle.Core.Interfaces;

/// <summary>
/// Generic repository interface for data access.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Gets all entities (async).
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Gets an entity by internal ID (async).
    /// </summary>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Gets an entity by external ID (async).
    /// </summary>
    Task<T?> GetByExternalIdAsync(Guid externalId);

    /// <summary>
    /// Adds a new entity (async).
    /// </summary>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Updates an existing entity (async).
    /// </summary>
    Task<T> UpdateAsync(T entity);

    /// <summary>
    /// Deletes an entity by external ID (async).
    /// </summary>
    Task<bool> DeleteAsync(Guid externalId);

    /// <summary>
    /// Checks if an entity exists by external ID (async).
    /// </summary>
    Task<bool> ExistsAsync(Guid externalId);

    /// <summary>
    /// Gets count of all entities (async).
    /// </summary>
    Task<int> CountAsync();
}
