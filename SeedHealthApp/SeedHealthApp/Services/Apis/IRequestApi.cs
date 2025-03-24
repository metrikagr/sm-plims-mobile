using Refit;
using SeedHealthApp.Models;
using SeedHealthApp.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SeedHealthApp.Services
{
    [Headers("Authorization: Bearer")]
    public interface IRequestApi
    {
        [Get("/api/request/requests?laboratory_id={laboratoryId}")]
        Task<Models.ApiResponse<IEnumerable<Request>>> GetRequests(int laboratoryId);

        [Get("/api/request/assays?request_id={requestId}&process_id={processId}")]
        Task<Models.ApiResponse<IEnumerable<Assay>>> GetAssays(int requestId, int processId);
        
        [Get("/api/request/assay-sample-type?request_id={requestId}&process_id={processId}&assay_id={assayId}")]
        Task<Models.ApiResponse<IEnumerable<SampleType>>> GetSampleTypes(int requestId, int processId, int assayId);
        [Get("/api/request/assay-sample-type?request_id={requestId}&process_id={processId}&assay_id={assayId}")]
        Task<Models.ApiResponse<IEnumerable<SampleType>>> GetSampleTypesContent(int requestId, int processId, int assayId);

        [Get("/api/request/assay-sample?request_process_essay_id={requestProcessAssayId}")]
        Task<Models.ApiResponse<SampleType>> GetAssaySample(int requestProcessAssayId);

        [Put("/api/request/assay-sample?request_process_essay_id={requestProcessAssayId}")]
        Task<Models.ApiResponse<AssaySample>> UpdateAssaySample(int requestProcessAssayId, [Body] AssaySample assaySample);
        
        [Put("/api/request/request-process-assay?request_process_assay_id={requestProcessAssayId}")]
        Task<Models.ApiResponse<object>> PatchRequestProcessAssay(int requestProcessAssayId, [Body] RequestProcessAssayStatusDto reqProcessAssayStatus);

        [Get("/api/request/assay-sample-material?request_process_essay_id={requestProcessAssayId}")]
        Task<Models.ApiResponse<IEnumerable<SampleMaterial>>> GetAssaySampleMaterial(int requestProcessAssayId);

        [Get("/api/request/assay-sample-material?request_id={requestId}&process_id={processId}&assay_id={assayId}&sample_type_id={sampleTypeId}")]
        Task<Models.ApiResponse<IEnumerable<SampleMaterial>>> GetAssaySampleMaterial2(int requestId, int processId, int assayId, int sampleTypeId);

        [Put("/api/request/assay-sample-material?request_id={requestId}&assay_id={assayId}&sample_type_id={sampleTypeId}&activity_id={activityId}&sample_group_id={sampleGroupId}")]
        Task<Models.ApiResponse<SampleMaterial>> UpdateAssaySampleMaterial(int requestId, int assayId, int sampleTypeId, int activityId, int sampleGroupId, [Body] SampleMaterial sampleMaterial);

        [Get("/api/request/agents?request_id={requestId}&process_id={processId}&assay_id={assayId}&sample_type_id={sampleTypeId}")]
        Task<Models.ApiResponse<IEnumerable<Agent>>> GetAgents(int requestId, int processId, int assayId, int sampleTypeId);

        [Post("/api/request/result?request_id={requestId}&assay_id={assayId}&sample_type_id={sampleTypeId}&activity_id={activityId}&sample_group_id={sampleGroupId}&composite_sample_id={compositeSampleId}&agent_id={agentId}")]
        Task<Models.ApiResponse<Result>> UpsertResult(int requestId, int assayId, int sampleTypeId, int activityId, int sampleGroupId, int compositeSampleId, int agentId, [Body] Result result);

        [Get("/api/request/assays?request_id={requestId}&process_id={processId}")]
        Task<HttpResponseMessage> GetEssaysHTTP(int requestId, int processId);

        [Get("/api/request/assays?request_id={requestId}&process_id={processId}")]
        Task<string> GetEssaysHTTPContent(int requestId, int processId);

        [Post("/api/request/request-process-assay-agent?request_id={requestId}&assay_id={assayId}")]
        Task<Models.ApiResponse<object>> UpsertRequestProcessAssayAgents(int requestId, int assayId, [Body] IEnumerable<int> agentIds);

        [Get("/api/request/assay-activity-sample-material?request_process_essay_id={requestProcessAssayId}")]
        Task<Models.ApiResponse<IEnumerable<AssayActivitySample>>> GetAssayActivitySamples(int requestProcessAssayId, [AliasAs("activity_id")] int? activityId, [AliasAs("composite_sample_id")] int? compositeSampleId,
            [AliasAs("sample_offset")] int? sampleOffset, [AliasAs("sample_limit")] int? sampleLimit);
        
        [Get("/api/request/assay-activity-sample-material?request_process_essay_id={requestProcessAssayId}")]
        Task<Models.ApiResponse<IEnumerable<AssayActivitySample>>> GetAssayActivitySamples(int requestProcessAssayId);
        //[Get("/api/request/assay-activity-sample-material?request_process_essay_id={requestProcessAssayId}&activity_id={activityId}")]
        //Task<string> GetAssayActivitySamplesHttpContent(int requestProcessAssayId, int activityId);
        [Post("/api/request/assay-activity-sample-material?request_process_essay_id={requestProcessAssayId}&activity_id={activityId}")]
        Task<Models.ApiResponse<IEnumerable<AssayActivitySample>>> UpdateAssayActivitySample(int requestProcessAssayId, int activityId);

        [Put("/api/request/assay-activity-sample-material?request_process_essay_id={requestProcessAssayId}&check_status={checkStatus}")]
        Task<Models.ApiResponse<IEnumerable<int>>> UpdateAssayActivitySample(int requestProcessAssayId, bool checkStatus, [Body] IEnumerable<RequestProcessAssayActivitySampleBatch> assayActivitySamples);

        [Get("/api/request/assay-sample-step?request_process_essay_id={requestProcessAssayId}&assay_id={assayId}")]
        Task<Models.ApiResponse<RequestProcessAssaySteps>> GetAssaySteps(int requestProcessAssayId, int assayId);

        [Get("/api/request/assay-sample-step?request_process_essay_id={requestProcessAssayId}&assay_id={assayId}")]
        Task<Models.ApiResponse<BlotterStepsDto>> GetBlotterAssaySteps(int requestProcessAssayId, int assayId);

        [Get("/api/request/assay-sample-step?request_process_essay_id={requestProcessAssayId}&assay_id={assayId}")]
        Task<Models.ApiResponse<GerminationStepsDto>> GetGerminationAssaySteps(int requestProcessAssayId, int assayId);

        [Put("/api/request/assay-sample-step?request_process_essay_id={requestProcessAssayId}&step={stepId}&status={status}")]
        Task<Models.ApiResponse<string>> UpdateAssayStep(int requestProcessAssayId, int stepId, string status);

        [Post("/api/request/result?request_process_assay_id={requestProcessAssayId}")]
        Task<Models.ApiResponse<object>> CreateResult(int requestProcessAssayId, [Body] Result result);

        [Get("/api/request/result?request_process_assay_id={requestProcessAssayId}")]
        Task<Models.ApiResponse<IEnumerable<Result>>> GetResults(int requestProcessAssayId, [AliasAs("activity_id")] int? activityId, [AliasAs("composite_sample_id")] int? compositeSampleId);

        [Get("/api/request/current-date")]
        Task<Models.ApiResponse<ServerDatetimeDto>> GetServerDatetime();

        [Post("/api/elisa/assay-sample")]
        Task<Models.ApiResponse<object>> EnableElisaRequestProcessAssaysByRequest([Body] IEnumerable<int> requestIds);

        #region elisa
        [Get("/api/elisa/multi-requests?laboratory_id={laboratoryId}")]
        Task<Models.ApiResponse<IEnumerable<MultiRequest>>> GetMultiRequests(int laboratoryId);

        [Post("/api/elisa/event")]
        Task<Models.ApiResponse<ElisaEvent>> CreateEvent([Body] ElisaEvent elisaEvent);

        [Get("/api/elisa/event?event_id={eventId}")]
        Task<Models.ApiResponse<ElisaEvent>> GetEvent(int eventId);

        [Get("/api/elisa/agents")]
        Task<Models.ApiResponse<IEnumerable<Agent>>> GetElisaAgents();

        [Post("/api/elisa/requests-by-agents?laboratory_id={laboratoryId}")]
        Task<Models.ApiResponse<IEnumerable<RequestLookup>>> GetElisaRequestsByAgents(int laboratoryId, [Body] IEnumerable<int> agentIds);

        [Post("/api/elisa/sample-type-by-agents?request_id={requestId}&process_id={processId}")]
        Task<Models.ApiResponse<IEnumerable<SampleType>>> GetElisaSampleTypesByAgents(int requestId, int processId, [Body] IEnumerable<int> agentIds);

        [Post("/api/request/elisa-distribution")]
        Task<Models.ApiResponse<object>> UpsertElisaDistribution([Body] ElisaDistribution elisaDistribution);
        
        [Post("/api/elisa/distribution-dev")]
        Task<Models.ApiResponse<string>> UpsertElisaDistribution([Body] IEnumerable<SupportLocation> supportLocations);

        [Get("/api/elisa/distribution-dev?event_id={eventId}&agent_ids{pathogenIds}&support_order_ids{supportOrderIds}")]
        Task<Models.ApiResponse<IEnumerable<SupportLocation>>> GetSupportLocations(int eventId, string pathogenIds = null, string supportOrderIds = null);

        [Get("/api/elisa/assay-sample-material?request_process_essay_id={requestProcessAssayId}")]
        Task<Models.ApiResponse<IEnumerable<SampleMaterial>>> GetElisaSamples(int requestProcessAssayId);

        [Get("/api/elisa/assay-control-material?request_process_essay_id={requestProcessAssayId}")]
        Task<Models.ApiResponse<IEnumerable<SampleMaterial>>> GetElisaControlSamples(int requestProcessAssayId);

        [Post("/api/elisa/request-process-assay-by-agents?event_id={eventId}")]
        Task<Models.ApiResponse<IEnumerable<SampleType>>> GetElisaRequestProcessAssaysByAgents(int eventId, [Body] IEnumerable<int> agentIds);
        [Post("/api/elisa/request-process-assay-by-agents?event_id={eventId}")]
        Task<string> GetElisaRequestProcessAssaysByAgentsAsContent(int eventId, [Body] IEnumerable<int> agentIds);
        #endregion
    }
}
