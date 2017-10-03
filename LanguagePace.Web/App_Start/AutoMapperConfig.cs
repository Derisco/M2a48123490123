using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;

namespace LanguagePace
{
    /// <summary>
    ///     Auto mapping configuations go here
    /// </summary>
    public static class AutoMapperConfig
    {
        /// <summary>
        ///     AutoMapper mappings should be created here
        /// </summary>
        public static void RegisterMappings()
        {
            Mapper.Initialize(cfg => { 
                cfg.CreateMap<Identity.IdentityUser, Models.ApplicationUser>()
                    .ReverseMap();
            });
        }

    }
}