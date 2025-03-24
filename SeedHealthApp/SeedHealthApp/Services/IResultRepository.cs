using SeedHealthApp.Models.Db;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SeedHealthApp.Services
{
    public interface IResultRepository
    {
        Task Add(ResultDb resultDb);
        Task AddOrUpdate(ResultDb ResultDb);
        Task<IEnumerable<ResultDb>> GetAll();
        Task<ResultDb> Get(int id);
        Task Remove(int id);
        Task<IEnumerable<ResultDb>> GetByRequestProcessAssayId(int requestProcessAssayId);
        Task<IEnumerable<ResultDb>> GetFiltered(int requestProcessAssayId, int? activityId = null, int? compositeSampleId = null,
            int? sampleOffset = null, int? sampleLimit = 100);
        Task RemoveByRequestProcessAssayId(int requestProcessAssayId);
        Task<IEnumerable<ResultDb>> MarkedToSync(int requestProcessAssayId);
        Task<bool> HasChanges(int requestProcessAssayId);
        Task<bool> MarkChangesAsDone(int requestProcessAssayId);
        Task AddOrUpdateMany(IEnumerable<ResultDb> ResultDbs);
    }
}
