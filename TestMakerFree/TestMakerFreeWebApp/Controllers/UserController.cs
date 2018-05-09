using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestMakerFreeWebApp.Data;
using TestMakerFreeWebApp.Data.Models;
using TestMakerFreeWebApp.ViewModels;

namespace TestMakerFreeWebApp.Controllers
{
    [Route("api/[controller]")]
    public class UserController : BaseApiController
    {
        #region Constructor

        public UserController(
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
            : base(context, roleManager, userManager, configuration)
        {

        }

        #endregion

        #region RESTful Conventions

        /// <summary>
        /// PUT: api/user
        /// </summary>
        /// <returns>Creates a new User and return it accordingly.</returns>
        [HttpPut()]
        public async Task<IActionResult> Put([FromBody]UserViewModel model)
        {
            // return a generic HTTP Status 500 (Server Error)
            // if the client payload is invalid.
            if (model == null) return new StatusCodeResult(500);

            try
            {
                // check if the Username/Email already exists
                ApplicationUser user = await UserManager.FindByNameAsync(model.UserName);
                if (user != null) return BadRequest("Username already exists");

                user = await UserManager.FindByEmailAsync(model.Email);
                if (user != null) return BadRequest("Email already exists.");

                // added in 2018.01.06 to fix GitHub issue #11
                // ref.: https://github.com/PacktPublishing/ASP.NET-Core-2-and-Angular-5/issues/11
                //if (!PasswordCheck.IsValidPassword(model.Password, UserManager.Options.Password)) return BadRequest("Password is too weak.");

                var now = DateTime.Now;



                // create a new Item with the client-sent json data
                user = new ApplicationUser()
                {
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = model.UserName,
                    Email = model.Email,
                    DisplayName = model.DisplayName,
                    CreatedDate = now,
                    LastModifiedDate = now
                };

                // Add the user to the Db with the choosen password
                await UserManager.CreateAsync(user, model.Password);

                // Assign the user to the 'RegisteredUser' role.
                await UserManager.AddToRoleAsync(user, "RegisteredUser");

                // Remove Lockout and E-Mail confirmation
                user.EmailConfirmed = true;
                user.LockoutEnabled = false;

                // persist the changes into the Database.
                DbContext.SaveChanges();

                // return the newly-created User to the client.
                return Json(user.Adapt<UserViewModel>(),
                    JsonSettings);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion
    }
}
