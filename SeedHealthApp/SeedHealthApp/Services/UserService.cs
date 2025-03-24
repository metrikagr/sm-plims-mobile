using MonkeyCache.FileStore;
using Refit;
using SeedHealthApp.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace SeedHealthApp.Services
{
    public class UserService : IUserService
    {
        private readonly string _baseUrl;
        private readonly ISettingsService _settingsService;
        public UserService(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            _baseUrl = _settingsService.ServerUrl;
            Barrel.ApplicationId = "SeedHealthCache";
        }
        public async Task<UserInfo> GetUserInfoAsync()
        {
            string route = "/api/user/me";
            var currentNetworkAccess = Connectivity.NetworkAccess;
            if (currentNetworkAccess == NetworkAccess.Internet)
            {
                var webApi = RestService.For<IUserApi>(_baseUrl, new RefitSettings()
                {
                    AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
                });
                var apiResponse = await webApi.GetUserInfo();

                if (apiResponse.Status == 200)
                {
                    // Save to cache
                    Barrel.Current.Add(key: route, data: apiResponse.Data, expireIn: TimeSpan.FromDays(7));
                    return apiResponse.Data;
                }
                else
                {
                    throw new Exception(apiResponse.Message);
                }
            }
            else
            {
                if (Barrel.Current.Exists(route))
                {
                    var userInfo = Barrel.Current.Get<UserInfo>(key: route);
                    return userInfo;
                }
                else
                {
                    throw new Exception("Internet is required");
                }
            }
        }
    }
}
