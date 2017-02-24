using System;
using Firefly.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace Firefly.Repository
{
    public class HttpRepositoryFactory
    {
        private readonly IServiceProvider _services;

        public HttpRepositoryFactory(IServiceProvider services)
        {
            _services = services;
        }

        public HttpRepository<T> Create<T>() where T : class, IEntity, new()
        {
            return new HttpRepository<T>(
                _services.GetService<DbContext>(),
                _services.GetService<IOptions<RepositoryConfig>>(),
                _services.GetService<ILogger<HttpRepository<T>>>()
            );
        }
    }
}