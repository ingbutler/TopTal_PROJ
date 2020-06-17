using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using DBRuns.Data;
using DBRuns.Models;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

        public async Task<UserList> GetUserAsync(string filter, int itemsPerPage, int pageNumber)
        {
            UserList userList =
                new UserList()
                {
                    ItemsPerPage = itemsPerPage,
                    PageNumber = pageNumber
                };

            int offset = itemsPerPage * (pageNumber - 1);
            String sql;
            string whereStr = "";
            List<string> parms;
            List<string> columns = typeof(User).GetProperties().Select(x => x.Name).ToList();
            string parsedFilter = Utils.ParseFilter(filter, columns, out parms);

            if(filter != "")
                whereStr = " where " + parsedFilter;

            sql = 
                $@"
                    select 
                        count(*) as count
                    from 
                        Users
                ";
            List<ItemsCount> itemsCounts = await Context.ItemsCounts.FromSqlRaw(sql, parms.ToArray()).ToListAsync();
            userList.QueriedItemsCount = itemsCounts.First().Count;
            userList.PageCount = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(userList.QueriedItemsCount) / Convert.ToDecimal(itemsPerPage)));

            sql = 
                $@"
                    select 
                        * 
                    from 
                        Users 
                    " + whereStr + @"
                    order by 
                        Email
                    offset
                        " + offset + @" rows
                    fetch next
                        " + itemsPerPage + @" rows only
                ";
            userList.users = await Context.Users.FromSqlRaw(sql, parms.ToArray()).ToListAsync();

            return userList;
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



        public async Task<int> UpdateUserAsync(User user)
        {
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

            string path = httpRequest.Path.Value.ToLower();
            path = path.Replace("signup", "");  // Remove action
            path = path.Replace("signin", "");
            
            string callbackUrl = httpRequest.Scheme + "://" + httpRequest.Host + path + "VerifyUser/" + user.Id;

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

        public async Task<User> SignupAsync(SignData signData)
        {
            User user =
                new User()
                {
                    Email = signData.Email,
                    PwdHash = Utils.GetMd5Hash(signData.Password),
                    IsVerified = false
                };

            if (!UsersExist())              // First user is admin
                user.Role = Roles.ADMIN;
            else
                user.Role = Roles.USER;

            int result = await InsertUserAsync(user);
            if (result == 1)
                return user;
            else
                return null;
        }



        public async Task<User> CheckCredentials(string email, string password)
        {
            User user = await GetUserByEmailAsync(email);
            if (user == null)
                return null;
            else if (user.SignInFailCount >= 3)
                return new User() { IsBlocked = true };

            if (!Utils.VerifyMd5Hash(password, user.PwdHash))
            {
                user.SignInFailCount = user.SignInFailCount + 1;
                await UpdateUserAsync(user);
                return null;
            }
            else
            {
                user.SignInFailCount = 0;       // FailCount reset
                await UpdateUserAsync(user);
                return user;
            }
        }

        #endregion BUSINESS LOGIC


    }

}
