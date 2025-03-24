using SeedHealthApp.Models.Db;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace SeedHealthApp.Services
{
    public class RequestProcessAssayActivitySampleRepository : IRequestProcessAssayActivitySampleRepository
    {
        SQLiteAsyncConnection db;
        async Task Init()
        {
            if (db != null)
                return;

            // Get an absolute path to the database file
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "MyData.db");

            db = new SQLiteAsyncConnection(databasePath);

            await db.CreateTableAsync<RequestProcessAssayActivitySampleDb>();
        }
        public async Task Add(RequestProcessAssayActivitySampleDb requestProcessAssayActivitySampleDb)
        {
            await Init();
            requestProcessAssayActivitySampleDb.local_guid = Guid.NewGuid().ToString();

            var id = await db.InsertAsync(requestProcessAssayActivitySampleDb);
        }

        public async Task<RequestProcessAssayActivitySampleDb> Get(int id)
        {
            await Init();

            var requestProcessAssayActivitySample = await db.Table<RequestProcessAssayActivitySampleDb>()
                .FirstOrDefaultAsync(c => c.local_guid == id.ToString());

            return requestProcessAssayActivitySample;
        }

        public async Task<IEnumerable<RequestProcessAssayActivitySampleDb>> GetAll()
        {
            await Init();

            var requestProcessAssayActivitySamples = await db.Table<RequestProcessAssayActivitySampleDb>().ToListAsync();
            return requestProcessAssayActivitySamples;
        }

        public async Task Remove(int id)
        {
            await Init();

            await db.DeleteAsync<RequestProcessAssayActivitySampleDb>(id);
        }

        public async Task<IEnumerable<RequestProcessAssayActivitySampleDb>> GetByRequestProcessAssayId(int requestProcessAssayId)
        {
            await Init();

            var requestProcessAssayActivitySamples = await db.Table<RequestProcessAssayActivitySampleDb>()
                .Where(x =>x.request_process_essay_id == requestProcessAssayId)
                .ToListAsync();
            return requestProcessAssayActivitySamples;
        }

        public async Task RemoveByRequestProcessAssayId(int requestProcessAssayId)
        {
            await Init();

            int modifiedRows = await db.ExecuteAsync("DELETE FROM RequestProcessAssayActivitySample WHERE request_process_essay_id = ?", requestProcessAssayId);
        }
        public async Task AddOrUpdate(RequestProcessAssayActivitySampleDb requestProcessAssayActivitySampleDb)
        {
            await Init();

            var entity = await db.Table<RequestProcessAssayActivitySampleDb>().FirstOrDefaultAsync(x => x.request_process_essay_id == requestProcessAssayActivitySampleDb.request_process_essay_id
                && x.activity_id == requestProcessAssayActivitySampleDb.activity_id && x.composite_sample_id == requestProcessAssayActivitySampleDb.composite_sample_id);
            if(entity == null)
            {
                await db.InsertAsync(requestProcessAssayActivitySampleDb);
            }
            else
            {
                entity.number_of_seeds = requestProcessAssayActivitySampleDb.number_of_seeds;
                await db.UpdateAsync(entity);
            }
        }

        public async Task<IEnumerable<RequestProcessAssayActivitySampleDb>> GetFiltered(int requestProcessAssayId, int? activityId = null, int? compositeSampleId = null, int? sampleOffset = null, int? sampleLimit = 100)
        {
            await Init();

            AsyncTableQuery<RequestProcessAssayActivitySampleDb> requestProcessAssayActivitySamplesQuery = db.Table<RequestProcessAssayActivitySampleDb>()
                .Where(x => x.request_process_essay_id == requestProcessAssayId);
            if(activityId != null)
                requestProcessAssayActivitySamplesQuery = requestProcessAssayActivitySamplesQuery.Where(x => x.activity_id == activityId);
            if (compositeSampleId != null)
                requestProcessAssayActivitySamplesQuery = requestProcessAssayActivitySamplesQuery.Where(x => x.composite_sample_id == compositeSampleId);
            if (sampleOffset != null)
            {
                var maxSampleOrder = sampleOffset + sampleLimit;
                requestProcessAssayActivitySamplesQuery = requestProcessAssayActivitySamplesQuery.Where(x => x.num_order > sampleOffset && x.num_order <= maxSampleOrder);
            }
            var requestProcessAssayActivitySamples = await requestProcessAssayActivitySamplesQuery.ToListAsync();
            return requestProcessAssayActivitySamples;
        }
        public async Task AddOrUpdateMany(IEnumerable<RequestProcessAssayActivitySampleDb> requestProcessAssayActivitySamplesDb)
        {
            await Init();

            DateTime now = DateTime.UtcNow;

            var updateList = requestProcessAssayActivitySamplesDb.Where(x => x.Id > 0);

            await db.UpdateAllAsync(updateList);

            var insertList = requestProcessAssayActivitySamplesDb.Where(x => x.Id == 0);

            await db.InsertAllAsync(insertList);

            //foreach (var requestProcessAssayActivitySampleDb in requestProcessAssayActivitySamplesDb)
            //{
            //    var entity = await db.Table<RequestProcessAssayActivitySampleDb>().FirstOrDefaultAsync(x => x.request_process_essay_id == requestProcessAssayActivitySampleDb.request_process_essay_id
            //    && x.activity_id == requestProcessAssayActivitySampleDb.activity_id && x.composite_sample_id == requestProcessAssayActivitySampleDb.composite_sample_id);
            //    if (entity == null)
            //    {
            //        requestProcessAssayActivitySampleDb.Modified = now;
            //        await db.InsertAsync(requestProcessAssayActivitySampleDb);
            //    } 
            //    else
            //    {
            //        entity.number_of_seeds = requestProcessAssayActivitySampleDb.number_of_seeds;
            //        entity.MarkedToSync = requestProcessAssayActivitySampleDb.MarkedToSync;
            //        entity.Modified = now;
            //        await db.UpdateAsync(entity);
            //    }
            //}
        }

        public async Task<IEnumerable<RequestProcessAssayActivitySampleDb>> MarkedToSync(int requestProcessAssayId)
        {
            await Init();

            var markedToSync = await db.Table<RequestProcessAssayActivitySampleDb>()
                .Where(x => x.request_process_essay_id == requestProcessAssayId && x.MarkedToSync)
                .ToListAsync();

            return markedToSync;
        }

        public async Task<bool> HasChanges(int requestProcessAssayId)
        {
            await Init();

            var changesCount = await db.Table<RequestProcessAssayActivitySampleDb>()
                .Where(x => x.request_process_essay_id == requestProcessAssayId && x.MarkedToSync)
                .CountAsync();

            return changesCount > 0;
        }

        public async Task<bool> MarkChangesAsDone(int requestProcessAssayId)
        {
            await Init();

            var assayActivitySamplesDb = await db.Table<RequestProcessAssayActivitySampleDb>()
                .Where(x => x.request_process_essay_id == requestProcessAssayId && x.MarkedToSync)
                .ToListAsync();

            DateTime now = DateTime.UtcNow;
            foreach (var entity in assayActivitySamplesDb)
            {
                entity.MarkedToSync = false;
                entity.Modified = now;
                await db.UpdateAsync(entity);
            }

            return true;
        }

        public async Task InsertManyForced(IEnumerable<RequestProcessAssayActivitySampleDb> requestProcessAssayActivitySampleDb)
        {
            await Init();
            var first = requestProcessAssayActivitySampleDb.FirstOrDefault();
            var rowsDeleted = await db.ExecuteAsync($"DELETE FROM RequestProcessAssayActivitySample where request_process_essay_id = ${first.request_process_essay_id}");

            var rowsInserted = await db.InsertAllAsync(requestProcessAssayActivitySampleDb);
        }
    }
}
