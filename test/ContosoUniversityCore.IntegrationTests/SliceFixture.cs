﻿using System.Threading;

namespace ContosoUniversityCore.IntegrationTests
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Domain;
    using FakeItEasy;
    using Infrastructure;
    using MediatR;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Respawn;

    public class SliceFixture
    {
        private static readonly Checkpoint _checkpoint;
        private static readonly IConfigurationRoot _configuration;
        private static readonly IServiceScopeFactory _scopeFactory;

        static SliceFixture()
        {
            var host = A.Fake<IHostingEnvironment>();

            A.CallTo(() => host.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var startup = new Startup(host);
            _configuration = startup.Configuration;
            var services = new ServiceCollection();
            startup.ConfigureServices(services);
            var provider = services.BuildServiceProvider();
            _scopeFactory = provider.GetService<IServiceScopeFactory>();
            _checkpoint = new Checkpoint();
        }

        public static Task ResetCheckpoint() => _checkpoint.Reset(_configuration["Data:DefaultConnection:ConnectionString"]);

        public static async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<SchoolContext>();

                try
                {
                    dbContext.BeginTransaction();

                    await action(scope.ServiceProvider);

                    await dbContext.CommitTransactionAsync();
                }
                catch (Exception)
                {
                    dbContext.RollbackTransaction();
                    throw;
                }
            }
        }

        public static async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<SchoolContext>();

                try
                {
                    dbContext.BeginTransaction();

                    var result = await action(scope.ServiceProvider);

                    await dbContext.CommitTransactionAsync();

                    return result;
                }
                catch (Exception)
                {
                    dbContext.RollbackTransaction();
                    throw;
                }
            }
        }

        public static Task ExecuteDbContextAsync(Func<SchoolContext, Task> action)
        {
            return ExecuteScopeAsync(sp => action(sp.GetService<SchoolContext>()));
        }

        public static Task<T> ExecuteDbContextAsync<T>(Func<SchoolContext, Task<T>> action)
        {
            return ExecuteScopeAsync(sp => action(sp.GetService<SchoolContext>()));
        }

        public static Task InsertAsync(params IEntity[] entities)
        {
            return ExecuteDbContextAsync(db =>
            {
                foreach (var entity in entities)
                {
                    db.Set(entity.GetType()).Add(entity);
                }
                return db.SaveChangesAsync();
            });
        }

        public static Task<T> FindAsync<T>(int id)
            where T : class, IEntity
        {
            return ExecuteDbContextAsync(db => db.Set<T>().FindAsync(id));
        }

        public static Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
        {
            return ExecuteScopeAsync(sp =>
            {
                var mediator = sp.GetService<IMediator>();

                return mediator.Send(request);
            });
        }

        public static Task SendAsync(IRequest request)
        {
            return ExecuteScopeAsync(sp =>
            {
                var mediator = sp.GetService<IMediator>();

                return mediator.Send(request);
            });
        }

        private static int CourseNumber = 1;

        public static int NextCourseNumber() => Interlocked.Increment(ref CourseNumber);
    }
}