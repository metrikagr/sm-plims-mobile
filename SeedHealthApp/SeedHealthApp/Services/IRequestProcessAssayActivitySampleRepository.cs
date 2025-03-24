using SeedHealthApp.Models.Db;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SeedHealthApp.Services
{
    public interface IRequestProcessAssayActivitySampleRepository
    {
        Task Add(RequestProcessAssayActivitySampleDb requestProcessAssayActivitySampleDb);
        Task<IEnumerable<RequestProcessAssayActivitySampleDb>> GetAll();
        Task<RequestProcessAssayActivitySampleDb> Get(int id);
        Task Remove(int id);
        Task<IEnumerable<RequestProcessAssayActivitySampleDb>> GetByRequestProcessAssayId(int requestProcessAssayId);
        Task<IEnumerable<RequestProcessAssayActivitySampleDb>> GetFiltered(int requestProcessAssayId, int? activityId = null, int? compositeSampleId = null,
            int? sampleOffset = null, int? sampleLimit = 100);
        Task RemoveByRequestProcessAssayId(int requestProcessAssayId);
        Task AddOrUpdate(RequestProcessAssayActivitySampleDb requestProcessAssayActivitySampleDb);
        Task AddOrUpdateMany(IEnumerable<RequestProcessAssayActivitySampleDb> requestProcessAssayActivitySampleDb);
        Task<IEnumerable<RequestProcessAssayActivitySampleDb>> MarkedToSync(int requestProcessAssayId);
        Task<bool> HasChanges(int requestProcessAssayId);
        Task<bool> MarkChangesAsDone(int requestProcessAssayId);
        Task InsertManyForced(IEnumerable<RequestProcessAssayActivitySampleDb> requestProcessAssayActivitySampleDb);

    }
}
