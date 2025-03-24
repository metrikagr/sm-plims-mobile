using MonkeyCache.FileStore;
using Refit;
using SeedHealthApp.Models;
using SeedHealthApp.Models.Db;
using SeedHealthApp.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace SeedHealthApp.Services
{
    public class RequestService : IRequestService
    {
        private readonly string _baseUrl;
        private readonly ISettingsService _settingsService;
        private readonly IRequestProcessAssayActivitySampleRepository _requestProcessAssayActivitySampleRepository;
        private readonly IRequestProcessAssayRepository _requestProcessAssayRepository;
        private readonly IResultRepository _resultRepository;
        public RequestService(ISettingsService settingsService, IRequestProcessAssayActivitySampleRepository requestProcessAssayActivitySampleRepository,
            IRequestProcessAssayRepository requestProcessAssayRepository, IResultRepository resultRepository)
        {
            _settingsService = settingsService;
            _requestProcessAssayActivitySampleRepository = requestProcessAssayActivitySampleRepository;
            _requestProcessAssayRepository = requestProcessAssayRepository;
            _resultRepository = resultRepository;

            _baseUrl = _settingsService.ServerUrl;
            Barrel.ApplicationId = "SeedHealthCache";
        }

        public async Task<IEnumerable<Request>> GetRequests(int laboratoryId)
        {
            string route = $"/api/request/requests?laboratory_id={laboratoryId}";
            var currentNetworkAccess = Connectivity.NetworkAccess;
            if (currentNetworkAccess == NetworkAccess.Internet)
            {
                var webApi = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
                {
                    AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
                });
                var apiResponse = await webApi.GetRequests(laboratoryId);

                if (apiResponse.Status == 200)
                {
                    // Save to cache
                    Barrel.Current.Add(key: route, data: apiResponse.Data, expireIn: TimeSpan.FromDays(7));
                    return apiResponse.Data;
                }
                else
                {
                    throw new Exception(apiResponse.Message);
                }
            }
            else
            {
                if (Barrel.Current.Exists(route))
                {
                    var requests = Barrel.Current.Get<IEnumerable<Request>>(key: route);
                    return requests;
                }
                else
                {
                    throw new Exception("Internet is required");
                }
            }
        }

        public async Task<IEnumerable<MultiRequest>> GetMultiRequests(int laboratoryId)
        {
            string route = $"/api/request/multi-requests?laboratory_id={laboratoryId}";
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                var webApi = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
                {
                    AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
                });
                var apiResponse = await webApi.GetMultiRequests(laboratoryId);

                if (apiResponse.Status == 200)
                {
                    // Save to cache
                    Barrel.Current.Add(key: route, data: apiResponse.Data, expireIn: TimeSpan.FromDays(7));
                    return apiResponse.Data;
                }
                else
                {
                    throw new Exception(apiResponse.Message);
                }
            }
            else
            {
                if (Barrel.Current.Exists(route))
                {
                    var requests = Barrel.Current.Get<IEnumerable<MultiRequest>>(key: route);
                    return requests;
                }
                else
                {
                    throw new Exception("Internet is required");
                }
            }
        }

        public async Task<IEnumerable<Assay>> GetAssayHeaders(int requestId, int processId = 0)
        {
            string route = $"/api/request/assays?request_id={requestId}&process_id={processId}";

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                var webApi = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
                {
                    AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
                });
                var str = await webApi.GetEssaysHTTPContent(requestId, processId);
                var apiResponse = await webApi.GetAssays(requestId, processId);
                if (apiResponse.Status == 200)
                {
                    // Save to cache
                    Barrel.Current.Add(key: route, data: apiResponse.Data, expireIn: TimeSpan.FromDays(7));
                    return apiResponse.Data;
                }
                else
                {
                    throw new Exception(apiResponse.Message);
                }
            }
            else
            {
                if (Barrel.Current.Exists(route))
                {
                    var assays = Barrel.Current.Get<IEnumerable<Assay>>(key: route);
                    return assays;
                }
                else
                {
                    throw new Exception("Internet is required");
                }
            }
        }

        public async Task<IEnumerable<SampleType>> GetRequestProcessAssays(int requestId, int processId, int assayId)
        {
            string route = $"/api/request/assay-sample-type?request_id={requestId}&process_id={processId}&assay_id={assayId}";

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                var webApi = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
                {
                    AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
                });
                var apiResponse = await webApi.GetSampleTypes(requestId, processId, assayId);
                if (apiResponse.Status == 200)
                {
                    // Save to cache
                    Barrel.Current.Add(key: route, data: apiResponse.Data, expireIn: TimeSpan.FromDays(7));
                    //foreach (var assay in apiResponse.Data)
                    //{
                    //    var localAssay = await _requestProcessAssayRepository.Get(assay.request_process_essay_id);
                    //    if (localAssay != null)
                    //        assay.IsAvailableOffline = localAssay.is_available_offline;
                    //}
                    return apiResponse.Data;
                }
                else
                {
                    throw new Exception(apiResponse.Message);
                }
            }
            else
            {
                if (Barrel.Current.Exists(route))
                {
                    var sampleTypes = Barrel.Current.Get<IEnumerable<SampleType>>(key: route);
                    foreach (var assay in sampleTypes)
                    {
                        var localAssay = await _requestProcessAssayRepository.Get(assay.request_process_essay_id);
                        if(localAssay != null)
                            assay.IsAvailableOffline = localAssay.is_available_offline;
                    }
                    return sampleTypes;
                }
                else
                {
                    throw new Exception("Internet is required");
                }
            }
        }
        public async Task<SampleType> GetRequestProcessAssay(int requestProcessAssayId)
        {
            string route = $"/api/request/assay-sample?request_process_assay_id={requestProcessAssayId}";

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                var webApi = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
                {
                    AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
                });
                var apiResponse = await webApi.GetAssaySample(requestProcessAssayId);
                if (apiResponse.Status == 200)
                {
                    // Save to cache
                    Barrel.Current.Add(key: route, data: apiResponse.Data, expireIn: TimeSpan.FromDays(7));
                    var localAssay = await _requestProcessAssayRepository.Get(apiResponse.Data.request_process_essay_id);
                    if (localAssay != null)
                    {
                        apiResponse.Data.IsAvailableOffline = localAssay.is_available_offline;
                        apiResponse.Data.LastSyncedDate = localAssay.last_synced_date;
                    }
                    return apiResponse.Data;
                }
                else
                {
                    throw new Exception(apiResponse.Message);
                }
            }
            else
            {
                if (Barrel.Current.Exists(route))
                {
                    var assaySample = Barrel.Current.Get<SampleType>(key: route);
                    var localAssay = await _requestProcessAssayRepository.Get(assaySample.request_process_essay_id);
                    if(localAssay != null)
                    {
                        assaySample.IsAvailableOffline = localAssay.is_available_offline;
                        assaySample.LastSyncedDate = localAssay.last_synced_date;
                    }
                    return assaySample;
                }
                else
                {
                    throw new Exception("Internet is required");
                }
            }
        }
        public async Task<Models.ApiResponse<AssaySample>> UpdateRequestProcessAssay(int requestProcessAssayId, AssaySample assaySample)
        {
            var apiResponse = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
            });
            return await apiResponse.UpdateAssaySample(requestProcessAssayId, assaySample);
        }
        public async Task<bool> UpdateRequestProcessAssayStatus(int requestProcessAssayId, int newStatusId)
        {
            var apiResponse = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
            });
            var result = await apiResponse.PatchRequestProcessAssay(requestProcessAssayId, new RequestProcessAssayStatusDto { status = newStatusId });
            if (result.Status != 200)
                throw new Exception(result.Message);
            return true;
        }
        public async Task<IEnumerable<SampleMaterial>> GetRequestProcessAssaySamples(int requestProcessAssayId)
        {
            string route = $"/api/request/assay-sample-material?request_process_assay_id={requestProcessAssayId}";
            if (Barrel.Current.Exists(route))
            {
                var tempRequestProcessAssaySamples = await Task.Run(() => Barrel.Current.Get<IEnumerable<SampleMaterial>>(key: route));
                return tempRequestProcessAssaySamples;
            }
            else
            {
                var webApi = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
                {
                    AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
                });
                var apiResponse = await webApi.GetAssaySampleMaterial(requestProcessAssayId);
                if (apiResponse.Status == 200)
                {
                    // Save to cache
                    Barrel.Current.Add(key: route, data: apiResponse.Data, expireIn: TimeSpan.FromDays(7));
                    return apiResponse.Data;
                }
                else
                {
                    throw new Exception(apiResponse.Message);
                }
            }
        }
        public async Task<Models.ApiResponse<SampleMaterial>> UpdateAssaySampleMaterial(int requestId, int assayId, int sampleTypeId, int activityId, int sampleGroupId, SampleMaterial sampleMaterial)
        {
            var apiResponse = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
            });
            return await apiResponse.UpdateAssaySampleMaterial(requestId, assayId, sampleTypeId, activityId, sampleGroupId, sampleMaterial);
        }
        public async Task<IEnumerable<Agent>> GetAgents(int requestId, int processId, int assayId, int sampleTypeId)
        {
            string route = $"/api/request/agents?request_id={requestId}&process_id={processId}&assay_id={assayId}&sample_type_id={sampleTypeId}";
            if (Barrel.Current.Exists(route))
            {
                var tempAgents = await Task.Run(() => Barrel.Current.Get<IEnumerable<Agent>>(key: route));
                return tempAgents;
            }
            else
            {
                var webApi = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
                {
                    AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
                });
                var apiResponse = await webApi.GetAgents(requestId, processId, assayId, sampleTypeId);
                if (apiResponse.Status == 200)
                {
                    // Save to cache
                    Barrel.Current.Add(key: route, data: apiResponse.Data, expireIn: TimeSpan.FromDays(7));
                    return apiResponse.Data;
                }
                else
                {
                    throw new Exception(apiResponse.Message);
                }
            }
        }
        public async Task<Models.ApiResponse<Result>> UpsertResult(int requestId, int assayId, int sampleTypeId, int activityId, int sampleGroupId, int compositeSampleId, int agentId, Result result)
        {
            var apiResponse = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
            });
            return await apiResponse.UpsertResult(requestId, assayId, sampleTypeId, activityId, sampleGroupId, compositeSampleId, agentId, result);
        }
        //public async Task<IEnumerable<SampleMaterial>> GetRequestProcessAssaySamples(int requestId, int processId, int assayId, int sampleTypeId)
        //{
        //    string route = $"/api/request/assay-sample-material?request_id={requestId}&process_id={processId}&assay_id={assayId}&sample_type_id={sampleTypeId}";
        //    if (Barrel.Current.Exists(route))
        //    {
        //        var tempRequestProcessAssaySamples = await Task.Run(() => Barrel.Current.Get<List<SampleMaterial>>(key: route));
        //        return tempRequestProcessAssaySamples;
        //    }
        //    else
        //    {
        //        var webApi = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
        //        {
        //            AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
        //        });
        //        var apiResponse = await webApi.GetAssaySampleMaterial2(requestId, processId, assayId, sampleTypeId);
        //        if (apiResponse.Status == 200)
        //        {
        //            // Save to cache
        //            Barrel.Current.Add(key: route, data: apiResponse.Data, expireIn: TimeSpan.FromDays(7));
        //            return apiResponse.Data;
        //        }
        //        else
        //        {
        //            throw new Exception(apiResponse.Message);
        //        }
        //    }
        //}

        public async Task<Models.ApiResponse<IEnumerable<Agent>>> GetElisaAgents()
        {
            var apiResponse = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
            });
            return await apiResponse.GetElisaAgents();
        }

        public async Task<Models.ApiResponse<IEnumerable<RequestLookup>>> GetElisaRequestsByAgents(int laboratoryId, IEnumerable<int> agentIds)
        {
            var apiResponse = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
            });
            return await apiResponse.GetElisaRequestsByAgents(laboratoryId, agentIds);
        }
        public async Task<Models.ApiResponse<object>> EnableElisaRequestProcessAssaysByRequest(IEnumerable<int> requestIds)
        {
            var apiResponse = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
            });
            return await apiResponse.EnableElisaRequestProcessAssaysByRequest(requestIds);
        }

        public async Task<Models.ApiResponse<object>> UpsertElisaDistribution(ElisaDistribution elisaDistribution)
        {
            var apiResponse = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
            });
            return await apiResponse.UpsertElisaDistribution(elisaDistribution);
        }

        public async Task<Models.ApiResponse<IEnumerable<SampleType>>> GetElisaSampleTypesByAgents(int requestId, int processId, IEnumerable<int> agentIds)
        {
            var apiResponse = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
            });
            return await apiResponse.GetElisaSampleTypesByAgents(requestId, processId, agentIds);
        }

        public async Task<IEnumerable<AssayActivitySample>> GetAssayActivitySamples(int requestProcessAssayId, bool isAvailableOffline, int? activityId = null, int? compositeSampleId = null,
            int? sampleOffset = null, int? sampleLimit = 100)
        {
            if (!isAvailableOffline)
            {
                var currentNetworkAccess = Connectivity.NetworkAccess;
                if (currentNetworkAccess == NetworkAccess.Internet)
                {
                    var webApi = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
                    {
                        AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
                    });
                    var apiResponse = await webApi.GetAssayActivitySamples(requestProcessAssayId, activityId, compositeSampleId, sampleOffset, sampleLimit);

                    if (apiResponse.Status == 200)
                    {
                        return apiResponse.Data;
                    }
                    else
                    {
                        throw new Exception(apiResponse.Message);
                    }
                }
                else
                    throw new Exception("Internet is required");
            }
            else
            {
                var dbAssayActivitySamples = await _requestProcessAssayActivitySampleRepository.GetFiltered(requestProcessAssayId, activityId, compositeSampleId, sampleOffset, sampleLimit);
                var result = dbAssayActivitySamples.Select(x => new AssayActivitySample
                {
                    LocalId = x.Id,
                    request_process_essay_id = x.request_process_essay_id,
                    activity_id = x.activity_id.Value,
                    composite_sample_id = x.composite_sample_id.Value,
                    number_of_seeds = x.number_of_seeds,
                    activity_sample_order = x.num_order,
                }).ToList();

                return result;
            }
        }
        public async Task<IEnumerable<AssayActivitySample>> GetAssayActivitySamples(int requestProcessAssayId, bool isAvailableOffline = false)
        {
            //string route = "/api/request/assay/activity-samples";
            var currentNetworkAccess = Connectivity.NetworkAccess;
            if (!isAvailableOffline)
            {
                if (currentNetworkAccess == NetworkAccess.Internet)
                {
                    var webApi = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
                    {
                        AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
                    });
                    var apiResponse = await webApi.GetAssayActivitySamples(requestProcessAssayId);

                    if (apiResponse.Status == 200)
                    {
                        return apiResponse.Data;
                    }
                    else
                    {
                        throw new Exception(apiResponse.Message);
                    }
                }
                else
                    throw new Exception("Internet is required");
            }
            else
            {
                var dbAssayActivitySamples = await _requestProcessAssayActivitySampleRepository.GetByRequestProcessAssayId(requestProcessAssayId);
                //var dbAssayActivitySamples = await _requestProcessAssayActivitySampleRepository.GetAll();
                var result = dbAssayActivitySamples.Select(x => new AssayActivitySample
                {
                    request_process_essay_id = x.request_process_essay_id,
                    activity_id = x.activity_id.Value,
                    composite_sample_id = x.composite_sample_id.Value,
                    number_of_seeds = x.number_of_seeds,
                    activity_sample_order = x.num_order,
                }).ToList();

                return result;
            }
        }

        public async Task<Models.ApiResponse<IEnumerable<int>>> UpdateAssayActivitySampleMany(int requestProcessAssayId, bool checkStatus,
            IEnumerable<RequestProcessAssayActivitySampleBatch> assayActivitySamples, bool isAvailableOffline)
        {
            if (isAvailableOffline)
            {
                throw new Exception("Deprecated for local storage");

                var assayActivitySamplesDb = assayActivitySamples.Select(x => new RequestProcessAssayActivitySampleDb
                {
                    request_process_essay_id = requestProcessAssayId,
                    activity_id = x.activity_id,
                    composite_sample_id = x.composite_sample_id,
                    number_of_seeds = x.number_of_seeds,
                    MarkedToSync = true
                }).ToList();
                await _requestProcessAssayActivitySampleRepository.AddOrUpdateMany(assayActivitySamplesDb);
                return new Models.ApiResponse<IEnumerable<int>>()
                {
                    Status = 200,
                    Data = null,
                };
            }
            else
            {
                var apiResponse = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
                {
                    AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
                });
                return await apiResponse.UpdateAssayActivitySample(requestProcessAssayId, checkStatus, assayActivitySamples);
            }
        }
        public async Task<bool> UpdateAssayActivitySampleMany(IEnumerable<AssayActivitySample> assayActivitySamples, bool useLocalStorage)
        {
            if (useLocalStorage)
            {
                var assayActivitySamplesDb = assayActivitySamples.Select(x => new RequestProcessAssayActivitySampleDb
                {
                    Id = x.LocalId,
                    request_process_essay_id = x.request_process_essay_id,
                    activity_id = x.activity_id,
                    composite_sample_id = x.composite_sample_id,
                    number_of_seeds = x.number_of_seeds,
                    MarkedToSync = true,
                    num_order = x.activity_sample_order
                }).ToList();
                await _requestProcessAssayActivitySampleRepository.AddOrUpdateMany(assayActivitySamplesDb);
                return true;
            }
            else
            {
                throw new NotImplementedException(nameof(UpdateAssayActivitySampleMany) + " not  implemented");
            }
        }
        public async Task<Models.ApiResponse<int>> UpdateAssayActivitySample(int requestProcessAssayId,
            RequestProcessAssayActivitySampleBatch assayActivitySamples, bool useLocalStorage)
        {
            if (useLocalStorage)
            {
                var assayActivitySampleDb = new RequestProcessAssayActivitySampleDb
                {
                    request_process_essay_id = requestProcessAssayId,
                    activity_id = assayActivitySamples.activity_id,
                    composite_sample_id = assayActivitySamples.composite_sample_id,
                    number_of_seeds = assayActivitySamples.number_of_seeds,
                    MarkedToSync = true
                };
                await _requestProcessAssayActivitySampleRepository.AddOrUpdate(assayActivitySampleDb);
                return new Models.ApiResponse<int>()
                {
                    Status = 200,
                    Data = -1,
                };
            }
            else
            {
                var apiResponse = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
                {
                    AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
                });
                var response = await apiResponse.UpdateAssayActivitySample(requestProcessAssayId, false,
                    new RequestProcessAssayActivitySampleBatch[] { assayActivitySamples } );
                var result = new Models.ApiResponse<int>()
                {
                    Code = response.Code,
                    Status = response.Status,
                    Message = response.Message,
                    Data = -1
                };
                if (response.Status == 200 && response.Data.Any())
                {
                    result.Data = response.Data.First();
                }
                return result;
            }
        }
        public async Task<RequestProcessAssaySteps> GetAssaySteps(int requestProcessAssayId, int assayId, bool isAvailableOffline)
        {
            string route = $"/api/request/assay/{requestProcessAssayId}/steps";
            if (!isAvailableOffline) //Connectivity.NetworkAccess == NetworkAccess.Internet
            {
                RequestProcessAssaySteps result = null;
                var webApi = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
                {
                    AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
                });

                if (assayId == (int)AssayMobileEnum.BlotterAndFreezePaperTest)
                {
                    var apiBlotterResponse = await webApi.GetBlotterAssaySteps(requestProcessAssayId, assayId);

                    if (apiBlotterResponse.Status == 200)
                    {
                        result = new RequestProcessAssaySteps()
                        {
                            preparation = new RequestProcessAssayStep()
                            {
                                acum = apiBlotterResponse.Data.preparation.acum,
                                total_act = apiBlotterResponse.Data.preparation.total_act_spl,
                                total_acum = apiBlotterResponse.Data.preparation.total_acum,
                                status = apiBlotterResponse.Data.preparation.status,
                                progress = apiBlotterResponse.Data.preparation.progress,
                                percent = (float)Math.Round(apiBlotterResponse.Data.preparation.percent,0),
                                users = apiBlotterResponse.Data.preparation.users,
                                start_date = apiBlotterResponse.Data.preparation.start_date?.Where(x => x != null).Min(),
                                finish_date = apiBlotterResponse.Data.preparation.finish_date?.Where(x => x != null).Max()
                            },
                            evaluation = new RequestProcessAssayStep()
                            {
                                acum = apiBlotterResponse.Data.evaluation.acum,
                                total_act = apiBlotterResponse.Data.evaluation.total_act_spl,
                                total_acum = apiBlotterResponse.Data.evaluation.total_acum,
                                status = apiBlotterResponse.Data.evaluation.status,
                                progress = apiBlotterResponse.Data.evaluation.progress,
                                percent = (float)Math.Round(apiBlotterResponse.Data.evaluation.percent, 0),
                                users = apiBlotterResponse.Data.evaluation.users,
                                start_date = apiBlotterResponse.Data.evaluation.start_date?.Where(x => x != null).Min(),
                                finish_date = apiBlotterResponse.Data.evaluation.finish_date?.Where(x => x != null).Max()
                            }
                        };
                    }
                    else
                    {
                        throw new Exception(apiBlotterResponse.Message);
                    }
                }
                else if (assayId == (int)AssayMobileEnum.GerminationTest)
                {
                    var apiGerminationResponse = await webApi.GetGerminationAssaySteps(requestProcessAssayId, assayId);
                    if (apiGerminationResponse.Status == 200)
                    {
                        result = new RequestProcessAssaySteps()
                        {
                            preparation = new RequestProcessAssayStep()
                            {
                                acum = apiGerminationResponse.Data.preparation.acum,
                                total_act = apiGerminationResponse.Data.preparation.total_act_spl,
                                total_acum = apiGerminationResponse.Data.preparation.total_acum,
                                status = apiGerminationResponse.Data.preparation.status,
                                progress = apiGerminationResponse.Data.preparation.progress,
                                percent = (float)Math.Round(apiGerminationResponse.Data.preparation.percent,0),
                                users = apiGerminationResponse.Data.preparation.users,
                                start_date = apiGerminationResponse.Data.preparation.start_date?.Where(x => x != null).Min(),
                                finish_date = apiGerminationResponse.Data.preparation.finish_date?.Where(x => x != null).Max()
                            },
                            evaluation = new RequestProcessAssayStep()
                            {
                                acum = apiGerminationResponse.Data.evaluation.acum,
                                total_act = apiGerminationResponse.Data.evaluation.total_act_spl,
                                total_acum = apiGerminationResponse.Data.evaluation.total_acum,
                                status = apiGerminationResponse.Data.evaluation.status,
                                progress = apiGerminationResponse.Data.evaluation.progress,
                                percent = (float)Math.Round(apiGerminationResponse.Data.evaluation.percent, 0),
                                users = apiGerminationResponse.Data.evaluation.users,
                                start_date = apiGerminationResponse.Data.evaluation.start_date_1?.Where(x => x != null).Min(),
                                finish_date = apiGerminationResponse.Data.evaluation.finish_date_1?.Where(x => x != null).Max()
                            },
                            sub_process_3 = new RequestProcessAssayStep()
                            {
                                acum = apiGerminationResponse.Data.evaluation_symptom.acum,
                                total_act = apiGerminationResponse.Data.evaluation_symptom.total_act_spl,
                                total_acum = apiGerminationResponse.Data.evaluation_symptom.total_acum,
                                status = apiGerminationResponse.Data.evaluation_symptom.status,
                                progress = apiGerminationResponse.Data.evaluation_symptom.progress,
                                percent = (float)Math.Round(apiGerminationResponse.Data.evaluation_symptom.percent, 0),
                                users = apiGerminationResponse.Data.evaluation_symptom.users,
                                start_date = apiGerminationResponse.Data.evaluation_symptom.start_date_2?.Where(x => x != null).Min(),
                                finish_date = apiGerminationResponse.Data.evaluation_symptom.finish_date_2?.Where(x => x != null).Max()
                            }
                        };
                    }
                    else
                    {
                        throw new Exception(apiGerminationResponse.Message);
                    }
                }
                else
                {
                    throw new Exception("Not supported assay");
                }

                // Save to cache
                Barrel.Current.Add(key: route, data: result, expireIn: TimeSpan.FromDays(7));
                return result;

                //var apiResponse = await webApi.GetAssaySteps(requestProcessAssayId, assayId);
            }
            else
            {
                if (Barrel.Current.Exists(route))
                {
                    //var assaySteps = await Task.Run(() => Barrel.Current.Get<RequestProcessAssaySteps>(key: route));
                    var assaySteps = Barrel.Current.Get<RequestProcessAssaySteps>(key: route);
                    return assaySteps;
                }
                else
                {
                    throw new Exception("Internet is required");
                }
            }
        }
        public async Task<Models.ApiResponse<string>> UpdateAssayStep(int requestProcessAssayId, int stepId, string status)
        {
            var apiResponse = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
            });
            return await apiResponse.UpdateAssayStep(requestProcessAssayId, stepId, status);
        }

        public async Task CreateAssayActivitySample(bool isAvailableOffline, AssayActivitySample assayActivitySample)
        {
            if (isAvailableOffline)
            {
                var newAssayActivitySample = new RequestProcessAssayActivitySampleDb()
                {
                    //local_id = Guid.NewGuid().ToString(),
                    request_process_essay_id = assayActivitySample.request_process_essay_id,
                    activity_id = assayActivitySample.activity_id,
                    composite_sample_id = assayActivitySample.composite_sample_id,
                    number_of_seeds = assayActivitySample.number_of_seeds,
                    num_order = assayActivitySample.activity_sample_order,
                    //status
                };

                await _requestProcessAssayActivitySampleRepository.AddOrUpdate(newAssayActivitySample);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public async Task CreateResult(int requestProcessAssayId, Result result, bool useLocalStorage = false)
        {
            if (useLocalStorage)
            {
                await _resultRepository.AddOrUpdate(new ResultDb
                {
                    request_process_assay_id = requestProcessAssayId,
                    activity_id = result.activity_id,
                    composite_sample_id = result.composite_sample_id,
                    agent_id = result.agent_id,
                    number_result = result.number_result,
                    text_result = result.text_result,
                    auxiliary_result = result.auxiliary_result,
                    MarkedToSync = true,
                    Modified = DateTime.UtcNow,
                    reprocess = result.reprocess,
                });
            }
            else
            {
                var webApi = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
                {
                    AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
                });
                var apiResponse = await webApi.CreateResult(requestProcessAssayId, result);
                if (apiResponse.Status != 200)
                    throw new Exception(apiResponse.Message);
            }
        }
        public async Task CreateOrUpdateAssayLocal(SampleType sampleType)
        {
            await _requestProcessAssayRepository.AddOrUpdate(new RequestProcessAssayDb
            {
                request_process_assay_id = sampleType.request_process_essay_id,
                is_available_offline = sampleType.IsAvailableOffline,
                last_synced_date = sampleType.LastSyncedDate
            });
        }
        public async Task CreateOrUpdateResultLocal(Result result)
        {
            await _resultRepository.AddOrUpdate(new ResultDb
            {
                request_process_assay_id = result.request_process_assay_id,
                activity_id = result.activity_id,
                composite_sample_id = result.composite_sample_id,
                agent_id = result.agent_id,
                number_result = result.number_result,
                text_result = result.text_result,
                auxiliary_result = result.auxiliary_result,
                reprocess = result.reprocess,
            });
        }
        public async Task<IEnumerable<Result>> GetResults(int requestProcessAssayId, int? activityId = null, int? compositeSampleId = null, bool useLocalStorage = false)
        {
            if (useLocalStorage)
            {
                var dbResults = await _resultRepository.GetFiltered(requestProcessAssayId, activityId, compositeSampleId);
                var result = dbResults.Select(x => new Result
                {
                    request_process_assay_id = x.request_process_assay_id,
                    activity_id = x.activity_id,
                    composite_sample_id = x.composite_sample_id,
                    agent_id = x.agent_id,
                    number_result = x.number_result,
                    text_result = x.text_result,
                    auxiliary_result = x.auxiliary_result,
                    reprocess = x.reprocess,
                }).ToList();
                return result;
            }
            else
            {
                var webApi = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
                {
                    AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
                });
                var apiResponse = await webApi.GetResults(requestProcessAssayId, activityId, compositeSampleId);
                if (apiResponse.Status == 200)
                    return apiResponse.Data;
                else
                    throw new Exception(apiResponse.Message);
            }
        }

        public async Task<IEnumerable<AssayActivitySample>> GetAssayActivitySampleChangesLocal(int requestProcessAssayId)
        {
            var assayActivitySamplesDb = await _requestProcessAssayActivitySampleRepository.MarkedToSync(requestProcessAssayId);
            return assayActivitySamplesDb.Select(x => new AssayActivitySample
            {
                request_process_essay_id = x.request_process_essay_id,
                activity_id = x.activity_id.Value,
                composite_sample_id = x.composite_sample_id.Value,
                number_of_seeds = x.number_of_seeds,
                activity_sample_order = x.num_order,
                LocalId = x.Id
            }).ToList();
        }


        public async Task<bool> HasChangesLocal(int requestProcessAssayId)
        {
            var dbAssayActivitySampleHasChanges = await _requestProcessAssayActivitySampleRepository.HasChanges(requestProcessAssayId);
            if (dbAssayActivitySampleHasChanges)
                return dbAssayActivitySampleHasChanges;
            var dbResultHasChanges = await _resultRepository.HasChanges(requestProcessAssayId);
            return dbResultHasChanges;
        }
        public async Task<bool> MarkAsDoneAssayActivitySampleChangesLocal(int requestProcessAssayId)
        {
            var isOk = await _requestProcessAssayActivitySampleRepository.MarkChangesAsDone(requestProcessAssayId);
            return isOk;
        }

        public async Task<IEnumerable<Result>> GetResultChangesLocal(int requestProcessAssayId)
        {
            var assayActivitySamplesDb = await _resultRepository.MarkedToSync(requestProcessAssayId);
            return assayActivitySamplesDb.Select(x => new Result
            {
                request_process_assay_id = x.request_process_assay_id,
                activity_id = x.activity_id,
                composite_sample_id = x.composite_sample_id,
                agent_id = x.agent_id,
                auxiliary_result = x.auxiliary_result,
                number_result = x.number_result,
                text_result = x.text_result,
                reprocess = x.reprocess,
            }).ToList();
        }
        public async Task<bool> MarkAsDoneResultChangesLocal(int requestProcessAssayId)
        {
            var isOk = await _resultRepository.MarkChangesAsDone(requestProcessAssayId);
            return isOk;
        }

        public async Task CreateOrUpdateManyResultLocal(IEnumerable<Result> results)
        {
            var dbResults = results.Select(x => new ResultDb
            {
                request_process_assay_id = x.request_process_assay_id,
                activity_id = x.activity_id,
                composite_sample_id = x.composite_sample_id,
                agent_id = x.agent_id,
                number_result = x.number_result,
                text_result = x.text_result,
                auxiliary_result = x.auxiliary_result,
                reprocess = x.reprocess,
            }).ToList();

            await _resultRepository.AddOrUpdateMany(dbResults);
        }

        public async Task CreateManyForcedAssayActivitySampleLocal(IEnumerable<AssayActivitySample> results)
        {
            var dbRequestProcessAssayActivitySamples = results.Select(assayActivitySample => new RequestProcessAssayActivitySampleDb()
            {
                //local_id = Guid.NewGuid().ToString(),
                request_process_essay_id = assayActivitySample.request_process_essay_id,
                activity_id = assayActivitySample.activity_id,
                composite_sample_id = assayActivitySample.composite_sample_id,
                number_of_seeds = assayActivitySample.number_of_seeds,
                num_order = assayActivitySample.activity_sample_order,
                //status
            }).ToList();

            await _requestProcessAssayActivitySampleRepository.InsertManyForced(dbRequestProcessAssayActivitySamples);
        }
        public async Task<DateTime> GetServerDatetime()
        {
            var webApi = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
            });
            var apiResponse = await webApi.GetServerDatetime();
            if (apiResponse.Status != 200)
                throw new Exception(apiResponse.Message);
            return DateTime.Parse(apiResponse.Data.current_date);
        }

        #region Elisa
        public async Task<int> CreateEvent(ElisaEvent elisaEvent)
        {
            var webApi = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
            });
            var apiResponse = await webApi.CreateEvent(elisaEvent);
            if (apiResponse.Status != 200)
                throw new Exception(apiResponse.Message);
            return apiResponse.Data.event_id;
        }
        public async Task<ElisaEvent> GetEvent(int eventId)
        {
            var webApi = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
            });
            var apiResponse = await webApi.GetEvent(eventId);
            if (apiResponse.Status != 200)
                throw new Exception(apiResponse.Message);
            return apiResponse.Data;
        }
        public async Task<string> UpsertElisaDistribution(IEnumerable<SupportLocation> supportLocations)
        {
            var webApi = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
            });
            var apiResponse = await webApi.UpsertElisaDistribution(supportLocations);
            if (apiResponse.Status != 200)
                throw new Exception(apiResponse.Message);
            return apiResponse.Data;
        }
        public async Task<IEnumerable<SupportLocation>> GetSupportLocationsAsync(int eventId, string pathogenIds = "", string supportOrderIds = "")
        {
            var webApi = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
            });
            var apiResponse = await webApi.GetSupportLocations(eventId, pathogenIds, supportOrderIds);
            if (apiResponse.Status != 200)
                throw new Exception(apiResponse.Message);
            return apiResponse.Data;
        }
        public async Task<IEnumerable<SampleMaterial>> GetElisaSamplesAsync(int requestProcessAssayId)
        {
            var webApi = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
            });
            var apiResponse = await webApi.GetElisaSamples(requestProcessAssayId);
            if (apiResponse.Status != 200)
                throw new Exception(apiResponse.Message);
            return apiResponse.Data;
        }
        public async Task<IEnumerable<SampleMaterial>> GetElisaControlSamplesAsync(int requestProcessAssayId)
        {
            var webApi = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
            });
            var apiResponse = await webApi.GetElisaControlSamples(requestProcessAssayId);
            if (apiResponse.Status != 200)
                throw new Exception(apiResponse.Message);
            return apiResponse.Data;
        }

        public async Task<IEnumerable<SampleType>> GetElisaRequestProcessAssaysByAgents(int event_id, IEnumerable<int> agentIds)
        {
            var webApi = RestService.For<IRequestApi>(_baseUrl, new RefitSettings()
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(_settingsService.Token)
            });
            var apiResponse = await webApi.GetElisaRequestProcessAssaysByAgents(event_id, agentIds);
            if (apiResponse.Status != 200)
                throw new Exception(apiResponse.Message);
            return apiResponse.Data;
        }
        #endregion
    }
}
