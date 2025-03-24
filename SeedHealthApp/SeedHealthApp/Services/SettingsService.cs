using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace SeedHealthApp.Services
{
    public class SettingsService : ISettingsService
    {
        public string ServerUrl
        {
            get => Preferences.Get("server_url_key", string.Empty);
            set => Preferences.Set("server_url_key", value);
        }
        public string Username
        {
            get => Preferences.Get("username_key", string.Empty);
            set => Preferences.Set("username_key", value);
        }
        public string Token
        {
            get => Preferences.Get("token_key", string.Empty);
            set => Preferences.Set("token_key", value);
        }
        public string RefreshToken
        {
            get => Preferences.Get("refresh_token_key", string.Empty);
            set => Preferences.Set("refresh_token_key", value);
        }
        public DateTime ExpiresAt
        {
            get => Preferences.Get("expires_at_key", new DateTime(0));
            set => Preferences.Set("expires_at_key", value);
        }
        public bool IsOffline
        {
            get => Preferences.Get("is_offline_key", false);
            set => Preferences.Set("is_offline_key", value);
        }
        public int RoleId { get => Preferences.Get("role_id", 0); set => Preferences.Set("role_id", value); }
        public string Role { get => Preferences.Get("role_code", ""); set => Preferences.Set("role_code", value); }
        public int InstitutionId { get => Preferences.Get("institution_id", 0); set => Preferences.Set("institution_id", value); }
        public string InstitutionName { get => Preferences.Get("institution_name", ""); set => Preferences.Set("institution_name", value); }
        public int LaboratoryId { get => Preferences.Get("laboratory_id", 0); set => Preferences.Set("laboratory_id", value); }
        public string LaboratoryName { get => Preferences.Get("laboratory_name", ""); set => Preferences.Set("laboratory_name", value); }
    }
}
