﻿using Microsoft.EntityFrameworkCore;

namespace RandevuTakipSistemi.Web.Repositories;

public class RepositoryBase<T> : IRepository<T> where T : class
{
    private readonly DbContext _dbContext;
    private readonly DbSet<T> _set;

    /// <summary>
    /// Constructor to set the context
    /// </summary>
    /// <param name="dbContext"></param>
    protected RepositoryBase(DbContext dbContext)
    {
        _dbContext = dbContext;
        _set = _dbContext.Set<T>();
    }

    /// <summary>
    /// Insert entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public void Insert(T entity)
    {
        _set.Add(entity);
    }

    /// <summary>
    /// Insert entities
    /// </summary>
    /// <param name="entities">Entities</param>
    public void InsertRange(IEnumerable<T> entities)
    {
        _set.AddRange(entities);
    }

    /// <summary>
    /// Insert entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public async Task InsertAsync(T entity)
    {
        await _set.AddAsync(entity);
    }

    /// <summary>
    /// Insert entities
    /// </summary>
    /// <param name="entities">Entities</param>
    public async Task InsertRangeAsync(IEnumerable<T> entities)
    {
        await _set.AddRangeAsync(entities);
    }

    /// <summary>
    /// Delete an item from the repository
    /// </summary>
    /// <param name="entity">The entity to delete</param>
    public void Delete(T entity)
    {
        _set.Remove(entity);
    }
    
    /// <summary>
    /// Delete items from the repository
    /// </summary>
    /// <param name="entities">Entities</param>
    public void DeleteRange(IEnumerable<T> entities)
    {
        _set.RemoveRange(entities);
    }

    /// <summary>
    /// Commit the changes to the repository
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">Indicates whether is called after the changes
    /// have been sent successfully to the database.</param>
    public void SaveAll(bool acceptAllChangesOnSuccess = true)
    {
        _dbContext.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <summary>
    /// Commit the changes to the repository
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">Indicates whether is called after the changes
    /// have been sent successfully to the database.</param>
    /// <returns>Task</returns>
    public async Task SaveAllAsync(bool acceptAllChangesOnSuccess = true)
    {
        await _dbContext.SaveChangesAsync(acceptAllChangesOnSuccess);
    }

    #region Properties

    /// <summary>
    ///     Find all items of entity type T in the repository using
    ///     IQueryable to allow further filtering.
    /// </summary>
    /// <returns>Returns a Query Object</returns>
    public IQueryable<T> Table => _dbContext.Set<T>();

    /// <summary>
    /// Execute the query by string.
    /// </summary>
    /// <param name="command">Command string. SQL query.</param>
    /// <returns>The string list of results.</returns>
    public IEnumerable<string> ExecuteQuery(string command)
    {
        var result = _dbContext.Database.ExecuteSqlRaw(command);
        return [result.ToString()];
    }

    /// <summary>
    /// Execute the query by string.
    /// </summary>
    /// <param name="command">Command string. SQL query.</param>
    /// <returns>The string list of results.</returns>
    public async Task<IEnumerable<string>> ExecuteQueryAsync(string command)
    {
        var result = await _dbContext.Database.ExecuteSqlRawAsync(command);
        return [result.ToString()];
    }

    #endregion
}
