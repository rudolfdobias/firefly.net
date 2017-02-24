using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Firefly.Models.Directives;
using Firefly.Repository.Filters;
using Firefly.Extensions;
using Firefly.Helpers;
using Firefly.Models;
using Firefly.Repository.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QueryString = Firefly.Helpers.QueryString;

namespace Firefly.Repository
{
    public enum SearchMode
    {
        And,
        Or
    }

    public class HttpRepository<TEntity> where TEntity : class, IEntity, new()
    {
        private const string LimitField = "limit";
        private const string OffsetField = "page";
        private const string OrderField = "sort";
        public const string SearchModeField = "searchMode";

        protected readonly DbContext DbContext;
        protected readonly DbSet<TEntity> CurrentDbSet;
        protected readonly IOptions<RepositoryConfig> Config;
        protected readonly ILogger<HttpRepository<TEntity>> Logger;
        private readonly List<IResourceScope<TEntity>> _scopes = new List<IResourceScope<TEntity>>();
        private readonly List<IFilter<TEntity>> _filters = new List<IFilter<TEntity>>();
        private readonly List<IDirective<TEntity>> _directives = new List<IDirective<TEntity>>();

        public delegate Task<IQueryable<TEntity>> QueryBuildingHandler(IQueryable<TEntity> builder,
            HttpContext httpContext);

        public delegate Task<TEntity> EntityCreatingHandler(TEntity entity);

        public delegate Task<TEntity> EntityDeletingHandler(TEntity entity);

        public delegate Task<TEntity> EntityPatchHandler(TEntity entity, JsonPatchDocument<TEntity> patch);

        public delegate List<object> BeforeSendHandler(List<TEntity> data, bool creating);

        /// <summary>
        /// Event that happens after query buildere creation
        /// </summary>
        public event QueryBuildingHandler OnCreateQuery;

        /// <summary>
        /// Event that happens after the field filters were applied on query builder
        /// </summary>
        public event QueryBuildingHandler OnFilter;

        /// <summary>
        /// Event for manipulation with entity that's just about to be created
        /// </summary>
        public event EntityCreatingHandler BeforeCreate;

        /// <summary>
        /// Event for manipulation with entity that's just about to be patched
        /// </summary>
        public event EntityPatchHandler BeforePatch;

        /// <summary>
        /// Event for manipulation with an entity just before saving, after BeforeCreate or BeforePatch
        /// </summary>
        public event EntityCreatingHandler BeforeSave;

        /// <summary>
        /// Event triggered before entity deletion
        /// </summary>
        public event EntityDeletingHandler BeforeDelete;

        /// <summary>
        /// Event triggered before the entity/collection is sent to the client.
        /// </summary>
        public event BeforeSendHandler BeforeSend;

        public HttpRepository(DbContext applicationDbContext, IOptions<RepositoryConfig> config,
            ILogger<HttpRepository<TEntity>> logger)
        {
            Config = config;
            Logger = logger;
            DbContext = applicationDbContext;
            CurrentDbSet = DbContext.Set<TEntity>();

            logger.LogDebug("Initialized Http Repository instance of " + typeof(TEntity));
        }

        /// <summary>
        /// Get entity collection, protected by Scopes
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="page"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public virtual async Task<IActionResult> GetProtected(int limit, int page, HttpContext httpContext)
        {
            return await Get(limit, page, await CreateGetQuery(httpContext), httpContext);
        }

        /// <summary>
        /// Gets single entity, protected by Scopes
        /// </summary>
        /// <param name="id"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public virtual async Task<IActionResult> GetProtected(Guid id, HttpContext httpContext)
        {
            return await Get(id, await CreateGetQuery(httpContext));
        }

        /// <summary>
        /// Creates single Entity, protected by Scopes
        /// </summary>
        /// <param name="value"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public virtual async Task<IActionResult> PostProtected(TEntity value, HttpContext httpContext)
        {
            value = await SavingEntity(value, httpContext);
            return await Create(value);
        }

        /// <summary>
        /// Updates single entity, protected by Scopes
        /// </summary>
        /// <param name="id"></param>
        /// <param name="patch"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public virtual async Task<IActionResult> PatchProtected(Guid id, JsonPatchDocument<TEntity> patch,
            HttpContext httpContext)
        {
            return await Patch(id, await CreateGetQuery(httpContext), patch, httpContext);
        }

        /// <summary>
        /// Deletes single entity, protected by Scopes
        /// </summary>
        /// <param name="id"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public virtual async Task<IActionResult> DeleteProtected(Guid id, HttpContext httpContext)
        {
            return await Delete(id, await CreateGetQuery(httpContext));
        }

        /// <summary>
        /// Gets entity collection - unprotected
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="page"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public virtual async Task<IActionResult> Get(int limit, int page, HttpContext httpContext)
            => await Get(limit, page, CurrentDbSet, httpContext);

        /// <summary>
        /// Gets entity collection - unprotected
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="page"></param>
        /// <param name="query"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public virtual async Task<IActionResult> Get(int limit, int page, IQueryable<TEntity> query,
            HttpContext httpContext)
        {
            if (limit <= 0)
            {
                limit = Config.Value.DefaultLimit;
            }
            var skip = 0;
            if (page > 0)
            {
                skip = limit * (page - 1);
            }

            int? total = null;

            query = ApplyOrdering(query, httpContext.Request);
            query = await ApplyDirectives(query, httpContext);
            query = await ApplyFiltering(query, httpContext);

            var originalQuery = query.AsQueryable();

            if (Config.Value.ShowTotalCount)
            {
                total = await originalQuery.CountAsync();
            }

            query = query.Skip(skip).Take(limit);
            var data = await query.ToListAsync();
            var transformed = TransformPipeline(data, false);

            var meta = new MetaData
            {
                CurrentPage = page.Clamp(1, int.MaxValue),
                PerPage = data.Count,
                Prev = CreateHateoasUrlPrev(limit, page, httpContext),
                Next = CreateHateoasUrlNext(limit, page, total, httpContext),
                Total = total
            };

            var result = new ResultSet<object> {Data = transformed, Meta = meta};

            return new OkObjectResult(result);
        }

        /// <summary>
        /// Gets single entity - unprotected
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IActionResult> Get(Guid id)
            => await Get(id, CurrentDbSet);

        /// <summary>
        /// Gets single entity - unprotected
        /// </summary>
        /// <param name="id"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public virtual async Task<IActionResult> Get(Guid id, IQueryable<TEntity> source)
        {
            var data = await source.FirstOrDefaultAsync(x => x.Id == id);
            var transformed = TransformPipeline(data, false);

            if (transformed != null)
            {
                return new OkObjectResult(data);
            }

            return new StatusCodeResult(404);
        }

        /// <summary>
        /// Creates single entity - unprotected
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual async Task<IActionResult> Create(TEntity value)
        {
            if (BeforeCreate != null)
            {
                value = await BeforeCreate(value);
            }

            CurrentDbSet.Add(value);
            DbContext.SaveChanges();

            return new CreatedResult(value.Id.ToString(), TransformPipeline(value, false));
        }

        /// <summary>
        /// Updates single entity - unprotected
        /// </summary>
        /// <param name="patch"></param>
        /// <param name="httpContext"></param>
        /// <param name="id"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        protected virtual async Task<IActionResult> Patch(Guid id, IQueryable<TEntity> source,
            JsonPatchDocument<TEntity> patch, HttpContext httpContext)
        {
            if (patch == null)
            {
                return CreateBadRequest("Input is not in valid JsonPatch format.", Errors.InvalidArgument);
            }
            var entity = source.FirstOrDefault(x => x.Id == id);

            if (entity == null)
            {
                return new StatusCodeResult(404);
            }

            try
            {
                if (BeforePatch != null)
                {
                    entity = await BeforePatch(entity, patch);
                }
                patch.ApplyTo(entity);
                entity = await SavingEntity(entity, httpContext);
            }
            catch (JsonPatchException crap)
            {
                return CreateBadRequest(crap.Message, Errors.InvalidArgument);
            }

            await DbContext.SaveChangesAsync();
            return new OkObjectResult(TransformPipeline(entity, false));
        }

        /// <summary>
        /// Deletes single entity - unprotected
        /// </summary>
        /// <param name="id"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public virtual async Task<IActionResult> Delete(Guid id, IQueryable<TEntity> source)
        {
            var entity = source.FirstOrDefault(x => x.Id == id);
            if (entity == null)
            {
                return new StatusCodeResult(404);
            }
            if (BeforeDelete != null)
            {
                entity = await BeforeDelete(entity);
            }
            CurrentDbSet.Remove(entity);
            await DbContext.SaveChangesAsync();
            return new StatusCodeResult(204);
        }

        public HttpRepository<TEntity> AddScope(IResourceScope<TEntity> decorator)
        {
            _scopes.Add(decorator);
            return this;
        }

        public HttpRepository<TEntity> AddScope(IResourceScope<TEntity>[] decorators)
        {
            _scopes.AddRange(decorators);
            return this;
        }

        public HttpRepository<TEntity> AddFilter(IFilter<TEntity> filter)
        {
            _filters.Add(filter);
            return this;
        }

        public HttpRepository<TEntity> AddFilterRange(IFilter<TEntity>[] filters)
        {
            _filters.AddRange(filters);
            return this;
        }

        public HttpRepository<TEntity> AddDirective(IDirective<TEntity> directive)
        {
            _directives.Add(directive);
            return this;
        }

        public HttpRepository<TEntity> AddDirectiveRange(IDirective<TEntity>[] directives)
        {
            _directives.AddRange(directives);
            return this;
        }

        public async Task<IQueryable<TEntity>> GetBuilderWithFitlers(HttpContext httpContext)
        {
            var query = await GetBuilder(httpContext);
            query = ApplyOrdering(query, httpContext.Request);
            query = await ApplyDirectives(query, httpContext);
            query = await ApplyFiltering(query, httpContext);
            return query;
        }

        public Task<IQueryable<TEntity>> GetBuilder(HttpContext httpContext) => CreateGetQuery(httpContext);

        protected virtual async Task<IQueryable<TEntity>> CreateGetQuery(HttpContext httpContext)
        {
            var query = DecorateGetQuery(CurrentDbSet.AsQueryable(), httpContext);
            if (OnCreateQuery != null)
            {
                query = await OnCreateQuery(query, httpContext);
            }
            return query;
        }

        protected virtual IQueryable<TEntity> DecorateGetQuery(IQueryable<TEntity> builder,
            IResourceScope<TEntity>[] decorators, HttpContext httpContext)
        {
            return decorators.Aggregate(builder, (current, decorator) => decorator.QueryCreating(current, httpContext));
        }

        protected virtual IQueryable<TEntity> DecorateGetQuery(IQueryable<TEntity> builder, HttpContext httpContext)
        {
            return DecorateGetQuery(builder, _scopes.ToArray(), httpContext);
        }

        public Task<TEntity> DecorateCreating(TEntity entity, HttpContext httpContext) => SavingEntity(entity,
            httpContext);

        protected virtual async Task<TEntity> SavingEntity(TEntity entity, HttpContext httpContext)
        {
            entity = _scopes.Aggregate(entity, (current, decorator) => decorator.EntityCreating(current, httpContext));
            if (BeforeSave != null)
            {
                entity = await BeforeSave(entity);
            }
            return entity;
        }

        protected string CreateHateoasUrlPrev(int limit, int page, HttpContext httpContext)
        {
            var request = httpContext.Request;
            if (page <= 0)
            {
                return null;
            }

            var qs = new QueryString(request.Query.ToDictionary(x => x.Key, x => x.Value.ToString()));
            qs.Replace(LimitField, limit.ToString());
            qs.Replace(OffsetField, (page - 1).ToString());
            return GetBaseUrl(httpContext) + qs;
        }

        protected string CreateHateoasUrlNext(int limit, int page, int? total, HttpContext httpContext)
        {
            var request = httpContext.Request;
            if (total != null && total <= 0)
            {
                return null;
            }
            if (total != null && total <= limit * (page + 1))
            {
                return null;
            }

            var qs = new QueryString(request.Query.ToDictionary(x => x.Key, x => x.Value.ToString()));
            qs.Replace(LimitField, limit.ToString());
            qs.Replace(OffsetField, (page + 1).ToString());
            return GetBaseUrl(httpContext) + qs;
        }

        protected string GetBaseUrl(HttpContext httpContext)
        {
            return httpContext.Request.Host + httpContext.Request.Path;
        }

        private IQueryable<TEntity> ApplyOrdering(IQueryable<TEntity> query, HttpRequest request)
        {
            try
            {
                if (!request.Query.ContainsKey(OrderField))
                {
                    return query;
                }
                string field = request.Query[OrderField];
                if (string.IsNullOrEmpty(field))
                {
                    return query;
                }
                var sortDirection = SortDirection.Asc;
                if (field.StartsWith("-"))
                {
                    sortDirection = SortDirection.Desc;
                    field = field.Substring(1);
                }

                field = field.FirstLetterToUpper();
                return query.OrderBy(field, sortDirection);
            }
            catch (ArgumentException crap)
            {
                throw new BadRequestException(crap.Message);
            }
        }

        private async Task<IQueryable<TEntity>> ApplyFiltering(IQueryable<TEntity> query, HttpContext context)
        {
            var mode = GetSearchMode(context);

            query = ApplyFiltersToBuilder(_filters.ToArray(), query, context, mode);

            if (OnFilter != null)
            {
                return await OnFilter(query, context);
            }
            return query;
        }

        public virtual IQueryable<TEntity> ApplyFiltersToBuilder(
            IFilter<TEntity>[] filters,
            IQueryable<TEntity> builder,
            HttpContext context,
            SearchMode mode)
        {
            var predicates = new List<Expression<Func<TEntity, bool>>>();
            Expression<Func<TEntity, bool>> predicate = null;
            foreach (var filter in filters)
            {
                predicates.AddRange(filter.Build(context));
            }

            foreach (var p in predicates)
            {
                if (predicate == null)
                {
                    predicate = p;
                    continue;
                }
                predicate = mode == SearchMode.Or ? predicate.Or(p) : predicate.And(p);
            }

            if (predicate != null)
            {
                builder = builder.Where(predicate);
            }

            return builder;
        }

        private async Task<IQueryable<TEntity>> ApplyDirectives(IQueryable<TEntity> query, HttpContext context)
        {
            await Task.Run(() => { query = _directives.Aggregate(query, (current, d) => d.Apply(current, context)); });

            return query;
        }

        protected SearchMode GetSearchMode(HttpContext context)
        {
            var field = (from f in context.Request.Query
                where string.Equals(f.Key, SearchModeField, StringComparison.CurrentCultureIgnoreCase)
                select f.Value.ToString()).FirstOrDefault();
            if (string.IsNullOrEmpty(field))
            {
                return SearchMode.And;
            }
            SearchMode mode;
            try
            {
                mode = field.ToLower().FirstLetterToUpper().AsEnum<SearchMode>();
            }
            catch
            {
                throw new BadRequestException(field + " is not valid search mode.");
            }

            return mode == SearchMode.Or ? SearchMode.Or : SearchMode.And;
        }

        private object TransformPipeline(TEntity entity, bool creating)
        {
            if (BeforeSend == null)
            {
                return entity;
            }
            var eventResult = BeforeSend(new List<TEntity> {entity}, creating);
            return eventResult != null ? eventResult.FirstOrDefault() : entity;
        }

        private IEnumerable<object> TransformPipeline(List<TEntity> list, bool creating)
        {
            return BeforeSend == null ? list.ToList<object>() : BeforeSend(list, creating);
        }

        protected static IActionResult CreateBadRequest(string message, Errors code)
        {
            return new BadRequestObjectResult(
                new
                {
                    message,
                    code = code.ToString()
                }
            );
        }
    }
}