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
    public class ResultRepository : IResultRepository
    {
        SQLiteAsyncConnection db;
        async Task Init()
        {
            if (db != null)
                return;

            // Get an absolute path to the database file
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "MyData.db");

            db = new SQLiteAsyncConnection(databasePath);

            await db.CreateTableAsync<ResultDb>();
        }
        public Task Add(ResultDb resultDb)
        {
            throw new NotImplementedException();
        }

        public async Task AddOrUpdate(ResultDb resultDb)
        {
            await Init();

            var entity = await db.Table<ResultDb>().FirstOrDefaultAsync(x => x.request_process_assay_id == resultDb.request_process_assay_id
                && x.activity_id == resultDb.activity_id && x.composite_sample_id == resultDb.composite_sample_id && x.agent_id == resultDb.agent_id);
            if (entity == null)
            {
                resultDb.Modified = DateTime.UtcNow;
                await db.InsertAsync(resultDb);
            }
            else
            {
                entity.number_result = resultDb.number_result;
                entity.text_result = resultDb.text_result;
                entity.auxiliary_result = resultDb.auxiliary_result;
                entity.MarkedToSync = resultDb.MarkedToSync;
                entity.Modified = DateTime.UtcNow;
                entity.reprocess = resultDb.reprocess;

                await db.UpdateAsync(entity);
            }
        }

        public Task<ResultDb> Get(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ResultDb>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ResultDb>> GetByRequestProcessAssayId(int requestProcessAssayId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ResultDb>> GetFiltered(int requestProcessAssayId, int? activityId = null, int? compositeSampleId = null, int? sampleOffset = null, int? sampleLimit = 100)
        {
            await Init();

            var resultsQuery = db.Table<ResultDb>()
                .Where(x => x.request_process_assay_id == requestProcessAssayId);
            if (activityId != null)
                resultsQuery = resultsQuery.Where(x => x.activity_id == activityId);
            if (compositeSampleId != null)
                resultsQuery = resultsQuery.Where(x => x.composite_sample_id == compositeSampleId);
            //if (sampleOffset != null)
            //    requestProcessAssayActivitySamplesQuery = requestProcessAssayActivitySamplesQuery.Where(x => x.num_order > sampleOffset && x.num_order <= sampleOffset + sampleLimit);

            var results = await resultsQuery.ToListAsync();
            return results;
        }

        public Task Remove(int id)
        {
            throw new NotImplementedException();
        }

        public Task RemoveByRequestProcessAssayId(int requestProcessAssayId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ResultDb>> MarkedToSync(int requestProcessAssayId)
        {
            await Init();

            var markedToSync = await db.Table<ResultDb>()
                .Where(x => x.request_process_assay_id == requestProcessAssayId && x.MarkedToSync)
                .ToListAsync();

            return markedToSync;
        }

        public async Task<bool> HasChanges(int requestProcessAssayId)
        {
            await Init();

            var changesCount = await db.Table<ResultDb>()
                .Where(x => x.request_process_assay_id == requestProcessAssayId && x.MarkedToSync)
                .CountAsync();

            return changesCount > 0;
        }

        public async Task<bool> MarkChangesAsDone(int requestProcessAssayId)
        {
            await Init();

            var dbResults = await db.Table<ResultDb>()
                .Where(x => x.request_process_assay_id == requestProcessAssayId && x.MarkedToSync)
                .ToListAsync();

            DateTime now = DateTime.UtcNow;
            foreach (var entity in dbResults)
            {
                entity.MarkedToSync = false;
                entity.Modified = now;
                await db.UpdateAsync(entity);
            }

            return true;
        }

        public async Task AddOrUpdateMany(IEnumerable<ResultDb> ResultDbs)
        {
            await Init();
            var first = ResultDbs.FirstOrDefault();
            _ = await db.ExecuteAsync($"DELETE FROM Result where request_process_assay_id = ${first.request_process_assay_id}");

            _ = await db.InsertAllAsync(ResultDbs);

            //foreach (var resultDb in ResultDbs)
            //{
            //    var entity = await db.Table<ResultDb>().FirstOrDefaultAsync(x => x.request_process_assay_id == resultDb.request_process_assay_id
            //    && x.activity_id == resultDb.activity_id && x.composite_sample_id == resultDb.composite_sample_id && x.agent_id == resultDb.agent_id);
            //    if (entity == null)
            //    {
            //        resultDb.Modified = DateTime.UtcNow;
            //        await db.InsertAsync(resultDb);
            //    }
            //    else
            //    {
            //        entity.number_result = resultDb.number_result;
            //        entity.text_result = resultDb.text_result;
            //        entity.auxiliary_result = resultDb.auxiliary_result;
            //        entity.MarkedToSync = resultDb.MarkedToSync;
            //        resultDb.Modified = DateTime.UtcNow;

            //        await db.UpdateAsync(entity);
            //    }
            //}
        }
    }
}
