using SeedHealthApp.Models;
using SeedHealthApp.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SeedHealthApp.Services
{
    public interface IRequestService
    {
        Task<IEnumerable<Request>> GetRequests(int laboratoryId);
        Task<IEnumerable<MultiRequest>> GetMultiRequests(int laboratoryId);
        Task<IEnumerable<Assay>> GetAssayHeaders(int requestId, int processId = 0);
        Task<IEnumerable<SampleType>> GetRequestProcessAssays(int requestId, int processId, int assayId);
        Task<SampleType> GetRequestProcessAssay(int requestProcessAssayId);
        Task<ApiResponse<AssaySample>> UpdateRequestProcessAssay(int requestProcessAssayId, AssaySample assaySample);
        Task<bool> UpdateRequestProcessAssayStatus(int requestProcessAssayId, int newStatusId);
        Task<IEnumerable<SampleMaterial>> GetRequestProcessAssaySamples(int requestProcessAssayId);
        //Task<IEnumerable<SampleMaterial>> GetRequestProcessAssaySamples(int requestId, int processId, int assayId, int sampleTypeId);
        Task<ApiResponse<SampleMaterial>> UpdateAssaySampleMaterial(int requestId, int assayId, int sampleTypeId, int activityId, int sampleGroupId, SampleMaterial sampleMaterial);
        Task<IEnumerable<Agent>> GetAgents(int requestId, int processId, int assayId, int sampleTypeId);
        Task<Models.ApiResponse<Result>> UpsertResult(int requestId, int assayId, int sampleTypeId, int activityId, int sampleGroupId, int compositeSampleId, int agentId, Result result);
        Task<Models.ApiResponse<IEnumerable<Agent>>> GetElisaAgents();
        Task<Models.ApiResponse<IEnumerable<RequestLookup>>> GetElisaRequestsByAgents(int laboratoryId, IEnumerable<int> agentIds);
        Task<Models.ApiResponse<object>> EnableElisaRequestProcessAssaysByRequest(IEnumerable<int> requestIds);

        Task<Models.ApiResponse<object>> UpsertElisaDistribution(ElisaDistribution elisaDistribution);
        //Task<Models.ApiResponse<object>> UpsertRequestProcessAssayAgents(int requestId, int assayId, IEnumerable<int> agentIds);
        Task<Models.ApiResponse<IEnumerable<SampleType>>> GetElisaSampleTypesByAgents(int requestId, int processId, IEnumerable<int> agentIds);
        Task<IEnumerable<AssayActivitySample>> GetAssayActivitySamples(int requestProcessAssayId, bool isAvailableOffline, int? activityId = null, int? compositeSampleId = null,
            int? sampleOffset = null, int? sampleLimit = null);
        Task<IEnumerable<AssayActivitySample>> GetAssayActivitySamples(int requestProcessAssayId, bool isAvailableOffline);
        Task<Models.ApiResponse<IEnumerable<int>>> UpdateAssayActivitySampleMany(int requestProcessAssayId, bool checkStatus,
            IEnumerable<RequestProcessAssayActivitySampleBatch> assayActivitySamples, bool useLocalStorage);
        Task<bool> UpdateAssayActivitySampleMany(IEnumerable<AssayActivitySample> assayActivitySamples, bool useLocalStorage);
        Task<Models.ApiResponse<int>> UpdateAssayActivitySample(int requestProcessAssayId,
            RequestProcessAssayActivitySampleBatch assayActivitySamples, bool useLocalStorage);
        Task<RequestProcessAssaySteps> GetAssaySteps(int requestProcessAssayId, int assayId, bool isAvailableOffline);
        Task<Models.ApiResponse<string>> UpdateAssayStep(int requestProcessAssayId, int stepId, string status);
        Task CreateAssayActivitySample(bool isAvailableOffline, AssayActivitySample assayActivitySample);
        Task CreateResult(int requestProcessAssayId, Result result, bool useLocalStorage);
        Task CreateOrUpdateAssayLocal(SampleType sampleType);
        Task CreateOrUpdateResultLocal(Result result);
        Task<IEnumerable<Result>> GetResults(int requestProcessAssayId, int? activityId, int? compositeSampleId, bool useLocalStorage);
        #region SyncService
        Task<bool> HasChangesLocal(int requestProcessAssayId);
        Task<IEnumerable<AssayActivitySample>> GetAssayActivitySampleChangesLocal(int requestProcessAssayId);
        Task<bool> MarkAsDoneAssayActivitySampleChangesLocal(int requestProcessAssayId);
        Task<IEnumerable<Result>> GetResultChangesLocal(int requestProcessAssayId);
        Task<bool> MarkAsDoneResultChangesLocal(int requestProcessAssayId);
        Task CreateOrUpdateManyResultLocal(IEnumerable<Result> results);
        Task CreateManyForcedAssayActivitySampleLocal(IEnumerable<AssayActivitySample> results);
        Task<DateTime> GetServerDatetime();
        #endregion

        #region Elisa
        Task<int> CreateEvent(ElisaEvent elisaEvent);
        Task<ElisaEvent> GetEvent(int eventId);
        Task<string> UpsertElisaDistribution(IEnumerable<SupportLocation> supportLocations);
        Task<IEnumerable<SupportLocation>> GetSupportLocationsAsync(int eventId, string pathogenIds = null, string supportOrderIds = null );
        Task<IEnumerable<SampleMaterial>> GetElisaSamplesAsync(int requestProcessAssayId);
        Task<IEnumerable<SampleMaterial>> GetElisaControlSamplesAsync(int requestProcessAssayId);
        Task<IEnumerable<SampleType>> GetElisaRequestProcessAssaysByAgents(int event_id, IEnumerable<int> agentIds);
        #endregion
    }
}
