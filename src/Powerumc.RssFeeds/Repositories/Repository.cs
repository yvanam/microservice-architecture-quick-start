﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Powerumc.RssFeeds.Database;
using Powerumc.RssFeeds.Database.Models;
using Powerumc.RssFeeds.Domain;

namespace Powerumc.RssFeeds.Repositories
{
    public abstract class Repository<TDatabaseModel> : IRepository<TDatabaseModel> where TDatabaseModel : Entity
    {
        private readonly ILogger<Repository<TDatabaseModel>> _logger;
        private readonly IRssFeedsDbContextFactory _dbContextFactory;

        public Repository(ILogger<Repository<TDatabaseModel>> logger,
            IRssFeedsDbContextFactory dbContextFactory)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
        }

        public virtual async Task<TDatabaseModel> GetAsync(long id,
            Func<IQueryable<TDatabaseModel>, IQueryable<TDatabaseModel>> additionQuery = null)
        {
            using (var dbContext = _dbContextFactory.CreateRead())
            {
                var query = dbContext
                    .Set<TDatabaseModel>()
                    .AsQueryable();

                if (additionQuery != null)
                {
                    query = additionQuery(query);
                }
                
                return await query.SingleOrDefaultAsync(o => o.Id == id);
            }
        }

        public virtual async Task<PagingResult<IEnumerable<TDatabaseModel>>> List(
            Expression<Func<TDatabaseModel, bool>> expression, PagingInfo pagingInfo,
            Func<IQueryable<TDatabaseModel>, IQueryable<TDatabaseModel>> additionQuery = null)
        {
            using (var dbContext = _dbContextFactory.CreateRead())
            {
                var query = dbContext.
                    Set<TDatabaseModel>().
                    AsQueryable();

                if (additionQuery != null)
                {
                    query = additionQuery(query);
                }

                if (expression != null)
                {
                    query = query.Where(expression);
                }

                var pagingQuery = query.Skip((pagingInfo.PageNo - 1) * pagingInfo.PageSize)
                    .Take(pagingInfo.PageSize);

                return await Task.FromResult(new PagingResult<IEnumerable<TDatabaseModel>>
                {
                    TotalCount = query.Count(),
                    Results = pagingQuery.ToList()
                });
            }
        }

        public virtual async Task<TDatabaseModel> CreateAsync(TDatabaseModel model)
        {
            using (var dbContext = _dbContextFactory.CreateWrite())
            {
                var entity = dbContext.Set<TDatabaseModel>().Add(model);
                await dbContext.SaveChangesAsync();

                return await Task.FromResult(entity.Entity);
            }
        }

        public virtual async Task<TDatabaseModel> UpdateAsync(TDatabaseModel model)
        {
            using (var dbContext = _dbContextFactory.CreateWrite())
            {
                var entity = dbContext.Set<TDatabaseModel>().Update(model);
                await dbContext.SaveChangesAsync();
                
                return await Task.FromResult(entity.Entity);
            }
        }

        public virtual async Task RemoveAsync(TDatabaseModel model)
        {
            using (var dbContext = _dbContextFactory.CreateWrite())
            {
                var entity = dbContext.Set<TDatabaseModel>().Remove(model);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}