using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using DBRuns.Data;
using DBRuns.Models;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace DBRuns.Services
{

    public class UserService
    {

        private readonly DBRunContext Context;
        private readonly IActionContextAccessor ActionContextAccessor;
        private readonly IConfiguration Configuration;
        private readonly RunService RunService;



        public UserService(DBRunContext context, IActionContextAccessor actionContextAccessor, IConfiguration configuration, RunService runService)
        {
            Context = context;
            ActionContextAccessor = actionContextAccessor;
            Configuration = configuration;
            RunService = runService;
        }



        #region DATA ACCESS LAYER

        public async Task<ItemList<User>> GetUserAsync(string filter, int itemsPerPage, int pageNumber)
        {
            if (itemsPerPage == 0)
                itemsPerPage = Int32.Parse(Configuration["ItemsPerPageDefault"]);
            if (pageNumber == 0)
                pageNumber = Int32.Parse(Configuration["PageNumberDefault"]);

            ItemList<User> itemList =
                new ItemList<User>()
                {
                    ItemsPerPage = itemsPerPage,
                    PageNumber = pageNumber
                };

            int offset = itemsPerPage * (pageNumber - 1);
            String sql;
            string whereStr = "";
            List<string> parms;
            string parsedFilter = Utils.ParseFilter(filter, typeof(User), out parms);

            if(filter != "")
                whereStr = " where " + parsedFilter;

            sql = 
                @"
                    select 
                        count(*) as count
                    from 
                        Users
                    " + whereStr;
            List<ItemsCount> itemsCounts = await Context.ItemsCounts.FromSqlRaw(sql, parms.ToArray()).ToListAsync();
            itemList.QueriedItemsCount = itemsCounts.First().Count;
            itemList.PageCount = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(itemList.QueriedItemsCount) / Convert.ToDecimal(itemsPerPage)));

            sql = 
                @"
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
            itemList.items = await Context.Users.FromSqlRaw(sql, parms.ToArray()).ToListAsync();

            return itemList;
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

            // Bulk delete user's runs
            await RunService.DeleteRunByUserAsync(user.Id);

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
                    Configuration["MailHost"],
                    Int32.Parse(Configuration["SmtpPort"]),
                    Configuration["MailUser"],
                    Configuration["MailPwd"],
                    true,
                    true,
                    emailTitle,
                    new List<string>() { user.Email },
                    null,
                    null,
                    Configuration["MailFrom"],
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
                    //PwdHash = Utils.GetMd5Hash(signData.Password),
                    Password = signData.Password,
                    IsVerified = false,
                    SignInFailCount = 0
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
