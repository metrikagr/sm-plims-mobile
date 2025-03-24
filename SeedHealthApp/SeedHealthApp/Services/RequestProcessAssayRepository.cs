using SeedHealthApp.Models.Db;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace SeedHealthApp.Services
{
    public class RequestProcessAssayRepository : IRequestProcessAssayRepository
    {
        SQLiteAsyncConnection db;
        async Task Init()
        {
            if (db != null)
                return;

            // Get an absolute path to the database file
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "MyData.db");

            db = new SQLiteAsyncConnection(databasePath);

            await db.CreateTableAsync<RequestProcessAssayDb>();
        }
        public Task Add(RequestProcessAssayDb requestProcessAssayDb)
        {
            throw new NotImplementedException();
        }

        public async Task AddOrUpdate(RequestProcessAssayDb requestProcessAssayDb)
        {
            await Init();

            var entity = await db.Table<RequestProcessAssayDb>()
                .FirstOrDefaultAsync(x => x.request_process_assay_id == requestProcessAssayDb.request_process_assay_id);
            if (entity == null)
            {
                await db.InsertAsync(requestProcessAssayDb);
            }
            else
            {
                entity.is_available_offline = requestProcessAssayDb.is_available_offline;
                entity.last_synced_date = requestProcessAssayDb.last_synced_date;
                await db.UpdateAsync(entity);
            }
        }

        public async Task<RequestProcessAssayDb> Get(int id)
        {
            await Init();

            var requestProcessAssay = await db.Table<RequestProcessAssayDb>()
                .FirstOrDefaultAsync(x => x.request_process_assay_id == id);

            return requestProcessAssay;
        }

        public Task<IEnumerable<RequestProcessAssayDb>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<RequestProcessAssayDb>> GetAvailableOffline()
        {
            throw new NotImplementedException();
        }

        public Task Remove(int id)
        {
            throw new NotImplementedException();
        }
    }
}
