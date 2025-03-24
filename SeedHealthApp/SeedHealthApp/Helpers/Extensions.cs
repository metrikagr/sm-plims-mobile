using Prism.Services;
using System;
using System.Threading.Tasks;

namespace SeedHealthApp.Extensions
{
    public static class Extensions
    {
        public static async Task DisplayErrorAlertAsync(this IPageDialogService pageDialogService, Exception ex)
        {
            await pageDialogService.DisplayAlertAsync("Error", ex.Message + Environment.NewLine + ex.InnerException?.Message, "OK");
        }
    }
}
