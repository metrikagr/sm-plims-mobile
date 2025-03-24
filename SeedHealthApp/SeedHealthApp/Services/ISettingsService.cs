using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Services
{
    public interface ISettingsService
    {
        string ServerUrl { get; set; }
        string Username { get; set; }
        string Token { get; set; }
        string RefreshToken { get; set; }
        DateTime ExpiresAt { get; set; }
        bool IsOffline { get; set; }
        int RoleId { get; set; }
        string Role { get; set; }
        int InstitutionId { get; set; }
        string InstitutionName { get; set; }
        int LaboratoryId { get; set; }
        string LaboratoryName { get; set; }
    }
}
