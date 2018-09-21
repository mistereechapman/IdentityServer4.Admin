﻿using System;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Moq;
using Skoruba.IdentityServer4.Admin.BusinessLogic.Repositories;
using Skoruba.IdentityServer4.Admin.BusinessLogic.Repositories.Interfaces;
using Skoruba.IdentityServer4.Admin.BusinessLogic.Resources;
using Skoruba.IdentityServer4.Admin.EntityFramework.DbContexts;
using Skoruba.IdentityServer4.Admin.BusinessLogic.Services;
using Skoruba.IdentityServer4.Admin.BusinessLogic.Services.Interfaces;
using Skoruba.IdentityServer4.Admin.EntityFramework.Entities.Identity;
using Skoruba.IdentityServer4.Admin.UnitTests.Mocks;
using Xunit;

namespace Skoruba.IdentityServer4.Admin.UnitTests.Services
{
    public class PersistedGrantServiceTests
    {
        public PersistedGrantServiceTests()
        {
            var databaseName = Guid.NewGuid().ToString();

            _dbContextOptions = new DbContextOptionsBuilder<AdminDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            _storeOptions = new ConfigurationStoreOptions();
            _operationalStore = new OperationalStoreOptions();
        }

        private readonly DbContextOptions<AdminDbContext> _dbContextOptions;
        private readonly ConfigurationStoreOptions _storeOptions;
        private readonly OperationalStoreOptions _operationalStore;

        private IPersistedGrantRepository<AdminDbContext, UserIdentity, UserIdentityRole, Guid, UserIdentityUserClaim, UserIdentityUserRole, UserIdentityUserLogin, UserIdentityRoleClaim, UserIdentityUserToken> GetPersistedGrantRepository(AdminDbContext context)
        {
            var persistedGrantRepository = new PersistedGrantRepository<AdminDbContext, UserIdentity, UserIdentityRole, Guid, UserIdentityUserClaim, UserIdentityUserRole, UserIdentityUserLogin, UserIdentityRoleClaim, UserIdentityUserToken>(context);

            return persistedGrantRepository;
        }

        private IPersistedGrantService<AdminDbContext, UserIdentity, UserIdentityRole, Guid, UserIdentityUserClaim,
                UserIdentityUserRole, UserIdentityUserLogin, UserIdentityRoleClaim, UserIdentityUserToken>
            GetPersistedGrantService(IPersistedGrantRepository<AdminDbContext, UserIdentity, UserIdentityRole, Guid, UserIdentityUserClaim, UserIdentityUserRole, UserIdentityUserLogin, UserIdentityRoleClaim, UserIdentityUserToken> repository, IPersistedGrantServiceResources persistedGrantServiceResources)
        {
            var persistedGrantService = new PersistedGrantService<AdminDbContext, UserIdentity, UserIdentityRole, Guid, UserIdentityUserClaim,
                UserIdentityUserRole, UserIdentityUserLogin, UserIdentityRoleClaim, UserIdentityUserToken>(repository,
                persistedGrantServiceResources);

            return persistedGrantService;
        }

        [Fact]
        public async Task GetPersitedGrantAsync()
        {
            using (var context = new AdminDbContext(_dbContextOptions, _storeOptions, _operationalStore))
            {
                var persistedGrantRepository = GetPersistedGrantRepository(context);

                var localizerMock = new Mock<IPersistedGrantServiceResources>();
                var localizer = localizerMock.Object;

                var persistedGrantService = GetPersistedGrantService(persistedGrantRepository, localizer);

                //Generate persisted grant
                var persistedGrantKey = Guid.NewGuid().ToString();
                var persistedGrant = PersistedGrantMock.GenerateRandomPersistedGrant(persistedGrantKey);

                //Try add new persisted grant
                await context.PersistedGrants.AddAsync(persistedGrant);
                await context.SaveChangesAsync();

                //Try get persisted grant
                var persistedGrantAdded = await persistedGrantService.GetPersitedGrantAsync(persistedGrantKey);

                //Assert
                persistedGrant.ShouldBeEquivalentTo(persistedGrantAdded);
            }
        }

        [Fact]
        public async Task DeletePersistedGrantAsync()
        {
            using (var context = new AdminDbContext(_dbContextOptions, _storeOptions, _operationalStore))
            {
                var persistedGrantRepository = GetPersistedGrantRepository(context);

                var localizerMock = new Mock<IPersistedGrantServiceResources>();
                var localizer = localizerMock.Object;

                var persistedGrantService = GetPersistedGrantService(persistedGrantRepository, localizer);

                //Generate persisted grant
                var persistedGrantKey = Guid.NewGuid().ToString();
                var persistedGrant = PersistedGrantMock.GenerateRandomPersistedGrant(persistedGrantKey);

                //Try add new persisted grant
                await context.PersistedGrants.AddAsync(persistedGrant);
                await context.SaveChangesAsync();

                //Try delete persisted grant
                await persistedGrantService.DeletePersistedGrantAsync(persistedGrantKey);

                var grant = await persistedGrantRepository.GetPersitedGrantAsync(persistedGrantKey);

                //Assert
                grant.Should().BeNull();
            }
        }

        [Fact]
        public async Task DeletePersistedGrantsAsync()
        {
            using (var context = new AdminDbContext(_dbContextOptions, _storeOptions, _operationalStore))
            {
                var persistedGrantRepository = GetPersistedGrantRepository(context);

                var localizerMock = new Mock<IPersistedGrantServiceResources>();
                var localizer = localizerMock.Object;

                var persistedGrantService = GetPersistedGrantService(persistedGrantRepository, localizer);

                const int subjectId = 1;

                for (var i = 0; i < 4; i++)
                {
                    //Generate persisted grant
                    var persistedGrantKey = Guid.NewGuid().ToString();
                    var persistedGrant = PersistedGrantMock.GenerateRandomPersistedGrant(persistedGrantKey, subjectId.ToString());

                    //Try add new persisted grant
                    await context.PersistedGrants.AddAsync(persistedGrant);
                }

                await context.SaveChangesAsync();

                //Try delete persisted grant
                await persistedGrantService.DeletePersistedGrantsAsync(subjectId.ToString());

                var grant = await persistedGrantRepository.GetPersitedGrantsByUser(subjectId.ToString());

                //Assert
                grant.TotalCount.Should().Be(0);
            }
        }
    }
}