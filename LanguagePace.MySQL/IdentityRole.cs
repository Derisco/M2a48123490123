using System;
using Microsoft.AspNet.Identity;

namespace LanguagePace.MySQL
{
    /// <summary>
    ///     Implements the ASP.NET Identity IRole Interface
    /// </summary>
    class IdentityRole : IRole
    {
        /// <summary>
        ///     Default constructor for Role
        /// </summary>
        public IdentityRole()
        {
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        ///     Constructor that takes name as argument
        /// </summary>
        /// <param name="name"></param>
        public IdentityRole(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        ///     Constuctor that taks name and id as arguments
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        public IdentityRole(string name, string id)
        {
            Name = name;
            Id = id;
        }

        /// <summary>
        ///     Role Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     Role name
        /// </summary>
        public string Name { get; set; }
    }
}
