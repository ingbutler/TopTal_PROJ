using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using DBRuns.Data;
using DBRuns.Models;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace DBRuns.Services
{

    public class UserService
    {

        private readonly DBRunContext Context;
        private readonly IActionContextAccessor ActionContextAccessor;



        public UserService(DBRunContext context, IActionContextAccessor actionContextAccessor)
        {
            Context = context;
            ActionContextAccessor = actionContextAccessor;
        }



        #region DATA ACCESS LAYER

        public async Task<IEnumerable<User>> GetUserAsync()
        {
            return await Context.Users.ToListAsync();
        }



        public async Task<User> GetUserAsync(Guid id)
        {
            return await Context.Users.FirstOrDefaultAsync(x => x.Id == id);
        }



        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await Context.Users.FirstOrDefaultAsync(x => x.Email == email);
        }



        public async Task<int> InsertUserAsync(User user)
        {
            user.Id = Guid.NewGuid();

            Context.Users.Add(user);

            return await Context.SaveChangesAsync();
        }



        public async Task<int> UpdateUserAsync(Guid id, User user)
        {
            if (id != user.Id)
                throw new ArgumentException("Id not corresponding");

            Context.Entry(user).State = EntityState.Modified;

            return await Context.SaveChangesAsync();
        }



        public async Task<User> DeleteUserAsync(Guid id)
        {
            var user = await Context.Users.FindAsync(id);
            if (user == null)
                return null;

            Context.Users.Remove(user);
            await Context.SaveChangesAsync();

            return user;
        }



        private bool UsersExist()
        {
            return Context.Users.Any();
        }



        private bool UserExists(Guid id)
        {
            return Context.Users.Any(e => e.Id == id);
        }



        public async Task SendVerificationMailAsync(User user)
        {
            HttpRequest httpRequest = ActionContextAccessor.ActionContext.HttpContext.Request;
            string callbackUrl = httpRequest.Scheme + "://" + httpRequest.Host + httpRequest.Path + "/VerifyUser/" + user.Id;

            string emailTitle = "Please confirm your account";
            string emailBody = "<a href='" + callbackUrl + "'>Please click Here to confirm your email</a>";

            await
                Utils.SendMailAsync(
                    "smtps.aruba.it",
                    587,
                    "info@elidentgroup.it",
                    "infoeli12",
                    true,
                    true,
                    emailTitle,
                    new List<string>() { user.Email },
                    null,
                    null,
                    "info@elidentgroup.it",
                    null,
                    null,
                    emailBody,
                    null,
                    null,
                    null
                );
        }



        public async Task<int> VerifyUserAsync(Guid id)
        {
            User user = await GetUserAsync(id);
            if (user == null)
                return 0;

            user.IsVerified = true;

            Context.Entry(user).State = EntityState.Modified;
            return await Context.SaveChangesAsync();
        }

        #endregion DATA ACCESS LAYER




        #region BUSINESS LOGIC

        public async Task<int> SignupAsync(User user)
        {
            user.IsVerified = false;

            if (!UsersExist())
                user.Role = Roles.ADMIN;
            else
                user.Role = Roles.USER;

            return await InsertUserAsync(user);
        }


        #endregion BUSINESS LOGIC


    }

}
