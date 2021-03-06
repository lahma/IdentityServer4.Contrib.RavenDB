﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.RavenDB.Storage.Indexes;
using IdentityServer4.RavenDB.Storage.Mappers;
using IdentityServer4.RavenDB.Storage.Stores;
using IdentityServer4.Stores;
using Xunit;

namespace IdentityServer4.RavenDB.IntegrationTests.Stores
{
    public class PersistedGrantStoreTests : IntegrationTest
    {
        private static PersistedGrant CreateTestObject()
        {
            return new PersistedGrant
            {
                Key = Guid.NewGuid().ToString(),
                Type = "authorization_code",
                ClientId = Guid.NewGuid().ToString(),
                SubjectId = Guid.NewGuid().ToString(),
                CreationTime = new DateTime(2016, 08, 01),
                Expiration = new DateTime(2016, 08, 31),
                Data = Guid.NewGuid().ToString()
            };
        }

        [Fact]
        public async Task StoreAsync_WhenPersistedGrantStored_ExpectSuccess()
        {
            using var ravenStore = GetDocumentStore();
            await new PersistentGrantIndex().ExecuteAsync(ravenStore);

            var persistedGrant = CreateTestObject();

            using (var session = ravenStore.OpenAsyncSession())
            {
                var store = new PersistedGrantStore(session, FakeLogger<PersistedGrantStore>.Create());
                await store.StoreAsync(persistedGrant);
            }

            WaitForIndexing(ravenStore);

            using (var session = ravenStore.OpenSession())
            {
                var foundGrant = session.Query<PersistedGrant>().FirstOrDefault(x => x.Key == persistedGrant.Key);
                Assert.NotNull(foundGrant);
            }
        }

        [Fact]
        public async Task GetAsync_WithKeyAndPersistedGrantExists_ExpectPersistedGrantReturned()
        {
            using var ravenStore = GetDocumentStore();
            await new PersistentGrantIndex().ExecuteAsync(ravenStore);

            var persistedGrant = CreateTestObject();

            using (var session = ravenStore.OpenSession())
            {
                session.Store(persistedGrant.ToEntity());
                session.SaveChanges();
            }
            
            WaitForIndexing(ravenStore);

            PersistedGrant foundPersistedGrant;
            using (var session = ravenStore.OpenAsyncSession())
            {
                var store = new PersistedGrantStore(session, FakeLogger<PersistedGrantStore>.Create());
                foundPersistedGrant = await store.GetAsync(persistedGrant.Key);
            }

            Assert.NotNull(foundPersistedGrant);
        }

        [Fact]
        public async Task GetAsync_WithSubAndTypeAndPersistedGrantExists_ExpectPersistedGrantReturned()
        {
            using var ravenStore = GetDocumentStore();
            await new PersistentGrantIndex().ExecuteAsync(ravenStore);

            var persistedGrant = CreateTestObject();

            using (var session = ravenStore.OpenSession())
            {
                session.Store(persistedGrant.ToEntity());
                session.SaveChanges();
            }

            WaitForIndexing(ravenStore);

            IList<PersistedGrant> foundPersistedGrants;
            using (var session = ravenStore.OpenAsyncSession())
            {
                var store = new PersistedGrantStore(session, FakeLogger<PersistedGrantStore>.Create());
                var filter = new PersistedGrantFilter
                {
                    SubjectId = persistedGrant.SubjectId
                };
                foundPersistedGrants = (await store.GetAllAsync(filter)).ToList();
            }

            Assert.NotNull(foundPersistedGrants);
            Assert.NotEmpty(foundPersistedGrants);
        }

        [Fact]
        public async Task RemoveAsync_WhenKeyOfExistingReceived_ExpectGrantDeleted()
        {
            using var ravenStore = GetDocumentStore();
            await new PersistentGrantIndex().ExecuteAsync(ravenStore);

            var persistedGrant = CreateTestObject();

            using (var session = ravenStore.OpenSession())
            {
                session.Store(persistedGrant.ToEntity());
                session.SaveChanges();
            }

            using (var session = ravenStore.OpenAsyncSession())
            {
                var store = new PersistedGrantStore(session, FakeLogger<PersistedGrantStore>.Create());
                await store.RemoveAsync(persistedGrant.Key);
            }

            using (var session = ravenStore.OpenSession())
            {
                var foundGrant = session.Query<PersistedGrant>()
                    .FirstOrDefault(x => x.Key == persistedGrant.Key);
                Assert.Null(foundGrant);
            }
        }

        [Fact]
        public async Task RemoveAsync_WhenSubIdAndClientIdOfExistingReceived_ExpectGrantDeleted()
        {
            using var ravenStore = GetDocumentStore();
            await new PersistentGrantIndex().ExecuteAsync(ravenStore);

            var persistedGrant = CreateTestObject();

            using (var session = ravenStore.OpenSession())
            {
                session.Store(persistedGrant.ToEntity());
                session.SaveChanges();
            }

            WaitForIndexing(ravenStore);

            using (var session = ravenStore.OpenAsyncSession())
            {
                var store = new PersistedGrantStore(session, FakeLogger<PersistedGrantStore>.Create());
                var filter = new PersistedGrantFilter
                {
                    SubjectId = persistedGrant.SubjectId,
                    ClientId = persistedGrant.ClientId

                };
                await store.RemoveAllAsync(filter);
            }

            using (var session = ravenStore.OpenSession())
            {
                var foundGrant = session.Query<PersistedGrant, PersistentGrantIndex>()
                    .FirstOrDefault(x => x.Key == persistedGrant.Key);
                Assert.Null(foundGrant);
            }
        }

        [Fact]
        public async Task RemoveAsync_WhenSubIdClientIdAndTypeOfExistingReceived_ExpectGrantDeleted()
        {
            using var ravenStore = GetDocumentStore();
            await new PersistentGrantIndex().ExecuteAsync(ravenStore);

            var persistedGrant = CreateTestObject();

            using (var session = ravenStore.OpenSession())
            {
                session.Store(persistedGrant.ToEntity());
                session.SaveChanges();
            }

            WaitForIndexing(ravenStore);

            using (var session = ravenStore.OpenAsyncSession())
            {
                var store = new PersistedGrantStore(session, FakeLogger<PersistedGrantStore>.Create());
                var filter = new PersistedGrantFilter
                {
                    SubjectId = persistedGrant.SubjectId,
                    ClientId = persistedGrant.ClientId,
                    Type = persistedGrant.Type
                };
                await store.RemoveAllAsync(filter);
            }

            using (var session = ravenStore.OpenSession())
            {
                var foundGrant = session.Query<PersistedGrant>()
                    .FirstOrDefault(x => x.Key == persistedGrant.Key);
                Assert.Null(foundGrant);
            }
        }

        [Fact]
        public async Task Store_should_create_new_record_if_key_does_not_exist()
        {
            using var ravenStore = GetDocumentStore();
            await new PersistentGrantIndex().ExecuteAsync(ravenStore);

            var persistedGrant = CreateTestObject();

            using (var session = ravenStore.OpenSession())
            {
                var foundGrant = session.Query<PersistedGrant>().FirstOrDefault(x => x.Key == persistedGrant.Key);
                Assert.Null(foundGrant);
            }

            using (var session = ravenStore.OpenAsyncSession())
            {
                var store = new PersistedGrantStore(session, FakeLogger<PersistedGrantStore>.Create());
                await store.StoreAsync(persistedGrant);
            }

            WaitForIndexing(ravenStore);
            WaitForUserToContinueTheTest(ravenStore);

            using (var session = ravenStore.OpenSession())
            {
                var foundGrant = session.Query<PersistedGrant>()
                    .FirstOrDefault(x => x.Key == persistedGrant.Key);
                Assert.NotNull(foundGrant);
            }
        }

        [Fact]
        public async Task Store_should_update_record_if_key_already_exists()
        {
            using var ravenStore = GetDocumentStore();
            await new PersistentGrantIndex().ExecuteAsync(ravenStore);

            var persistedGrant = CreateTestObject();

            using (var session = ravenStore.OpenSession())
            {
                session.Store(persistedGrant.ToEntity());
                session.SaveChanges();
            }

            var newDate = persistedGrant.Expiration.Value.AddHours(1);
            using (var session = ravenStore.OpenAsyncSession())
            {
                var store = new PersistedGrantStore(session, FakeLogger<PersistedGrantStore>.Create());
                persistedGrant.Expiration = newDate;
                await store.StoreAsync(persistedGrant);
            }

            using (var session = ravenStore.OpenSession())
            {
                var foundGrant = session.Query<Storage.Entities.PersistedGrant>()
                    .FirstOrDefault(x => x.Key == persistedGrant.Key);
                Assert.NotNull(foundGrant);
                Assert.Equal(newDate, persistedGrant.Expiration);
            }
        }

        //[Fact]
        //public async Task GetAllAsync_is_implemented()
        //{
        //    using (var ravenStore = GetDocumentStore())
        //    {
        //        var persistedGrant = CreateTestObject();

        //        using (var session = ravenStore.OpenSession())
        //        {
        //            session.Store(persistedGrant.ToEntity());
        //            session.SaveChanges();
        //        }

        //        var newDate = persistedGrant.Expiration.Value.AddHours(1);
        //        using (var session = ravenStore.OpenAsyncSession())
        //        {
        //            var store = new PersistedGrantStore(session, FakeLogger<PersistedGrantStore>.Create());

        //            PersistedGrantFilter filter = new PersistedGrantFilter();
        //            await store.GetAllAsync(filter);
        //        }
        //    }
        //}

        //[Fact]
        //public async Task RemoveAllAsync_is_implemented()
        //{
        //    using (var ravenStore = GetDocumentStore())
        //    {
        //        var persistedGrant = CreateTestObject();

        //        using (var session = ravenStore.OpenSession())
        //        {
        //            session.Store(persistedGrant.ToEntity());
        //            session.SaveChanges();
        //        }

        //        var newDate = persistedGrant.Expiration.Value.AddHours(1);
        //        using (var session = ravenStore.OpenAsyncSession())
        //        {
        //            var store = new PersistedGrantStore(session, FakeLogger<PersistedGrantStore>.Create());

        //            PersistedGrantFilter filter = new PersistedGrantFilter();
        //            await store.RemoveAllAsync(filter);
        //        }
        //    }
        //}
    }
}
