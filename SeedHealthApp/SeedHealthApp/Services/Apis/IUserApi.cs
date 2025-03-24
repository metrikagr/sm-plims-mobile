using Refit;
using SeedHealthApp.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SeedHealthApp.Services
{
    [Headers("Authorization: Bearer")]
    public interface IUserApi
    {
        [Get("/api/user/me")]
        Task<Models.ApiResponse<UserInfo>> GetUserInfo();
    }
}
