using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using LanguagePace.MySQL;
using System.Threading.Tasks;

namespace LanguagePace.Identity
{
    /// <summary>
    ///     Class that implement the key ASP.NET Identity role store interfaces
    /// </summary>
    /// <typeparam name="TRole"></typeparam>
    public class RoleStore<TRole> : IQueryableRoleStore<TRole>
        where TRole : IdentityRole
    {
        public MySQLDatabase Database { get; private set; }

        public IQueryable<TRole> Roles
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        ///     Default constructor that initializes a new MySQLDatabase
        ///     instance using the Default Connection string
        /// </summary>
        public RoleStore()
        {
            new RoleStore<TRole>(new MySQLDatabase());
        }

        /// <summary>
        ///     Constructor that takes a MySQLDatabase as argument
        /// </summary>
        /// <param name="database"></param>
        public RoleStore(MySQLDatabase database)
        {
            Database = database;
        }

        /// <summary>
        ///     Creates new role in the database
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public Task CreateAsync(TRole role)
        {
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }

            Database.ExecuteStoredProcedure("role_Insert",
                new Dictionary<string, object>()
                {
                    { "roleId", role.Id },
                    { "roleName", role.Name }
                });

            return Task.FromResult<object>(null);
        }

        /// <summary>
        ///     Deletes a role from the database
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public Task DeleteAsync(TRole role)
        {
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }

            Database.ExecuteStoredProcedure("role_Delete",
                new Dictionary<string, object>() { { "roleId", role.Id } });

            return Task.FromResult<object>(null);
        }

        /// <summary>
        ///     Finds a role by it's Id
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public Task<TRole> FindByIdAsync(string roleId)
        {
            if (string.IsNullOrEmpty(roleId))
            {
                throw new ArgumentNullException("roleId");
            }

            var result = Database.QueryStoredProcedure("role_SelectById",
                new Dictionary<string, object>() { { "roleId", roleId } });

            var role = Helpers.Hydrate<IdentityRole>(result);

            return Task.FromResult<TRole>((TRole)role.FirstOrDefault());
        }

        /// <summary>
        ///     Finds a role by it's name
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public Task<TRole> FindByNameAsync(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                throw new ArgumentNullException("roleName");
            }

            var result = Database.QueryStoredProcedure("role_SelectByName",
                new Dictionary<string, object>() { { "roleName" , roleName } });

            var role = Helpers.Hydrate<IdentityRole>(result);

            return Task.FromResult<TRole>((TRole)role.FirstOrDefault());
        }

        /// <summary>
        ///     Updates a role by it's Id
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public Task UpdateAsync(TRole role)
        {
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }

            Database.ExecuteStoredProcedure("role_UpdateById",
                new Dictionary<string, object>()
                {
                    { "roleId", role.Id },
                    { "roleName", role.Name }
                });

            return Task.FromResult<object>(null);
        }

        /// <summary>
        ///     Disposes of the database
        /// </summary>
        public void Dispose()
        {
            if (Database != null)
            {
                Database.Dispose();
                Database = null;
            }
        }

    }
}
