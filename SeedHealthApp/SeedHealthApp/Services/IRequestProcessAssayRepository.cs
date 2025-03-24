using SeedHealthApp.Models.Db;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SeedHealthApp.Services
{
    public interface IRequestProcessAssayRepository
    {
        Task Add(RequestProcessAssayDb requestProcessAssayDb);
        Task<IEnumerable<RequestProcessAssayDb>> GetAll();
        Task<RequestProcessAssayDb> Get(int id);
        Task Remove(int id);
        Task<IEnumerable<RequestProcessAssayDb>> GetAvailableOffline();
        Task AddOrUpdate(RequestProcessAssayDb requestProcessAssayDb);
    }
}
