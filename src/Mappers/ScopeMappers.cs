﻿using AutoMapper;
using IdentityServer4.RavenDB.Storage.Entities;

namespace IdentityServer4.RavenDB.Storage.Mappers
{
    /// <summary>
    /// Extension methods to map to/from entity/model for scopes.
    /// </summary>
    public static class ScopeMappers
    {
        static ScopeMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<ScopeMapperProfile>())
                .CreateMapper();
        }

        internal static IMapper Mapper { get; }

        /// <summary>
        /// Maps an entity to a model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static ApiScope ToModel(this ApiScope entity)
        {
            return entity == null ? null : Mapper.Map<ApiScope>(entity);
        }

        /// <summary>
        /// Maps a model to an entity.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public static ApiScope ToEntity(this ApiScope model)
        {
            return model == null ? null : Mapper.Map<ApiScope>(model);
        }
    }
}
