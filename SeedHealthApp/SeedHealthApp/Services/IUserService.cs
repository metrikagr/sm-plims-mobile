using SeedHealthApp.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SeedHealthApp.Services
{
    public interface IUserService
    {
        Task<UserInfo> GetUserInfoAsync();
    }
}
