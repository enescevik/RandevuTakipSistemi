namespace RandevuTakipSistemi.Web.Repositories;

public class LinqToSqlRepository<T>(ApplicationDbContext dbContext) : RepositoryBase<T>(dbContext) where T : class;
