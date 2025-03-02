﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NextGenSoftware.OASIS.API.Providers.CargoOASIS.Enum;
using NextGenSoftware.OASIS.API.Providers.CargoOASIS.Infrastructure.Builder;
using NextGenSoftware.OASIS.API.Providers.CargoOASIS.Infrastructure.Interfaces;
using NextGenSoftware.OASIS.API.Providers.CargoOASIS.Infrastructure.Services.HttpHandler;
using NextGenSoftware.OASIS.API.Providers.CargoOASIS.Models.Cargo;
using NextGenSoftware.OASIS.API.Providers.CargoOASIS.Models.Common;
using NextGenSoftware.OASIS.API.Providers.CargoOASIS.Models.Request;
using NextGenSoftware.OASIS.API.Providers.CargoOASIS.Models.Response;

namespace NextGenSoftware.OASIS.API.Providers.CargoOASIS.Infrastructure.Handlers.Queries
{
    public class GetAllUserCollectiblesHandler : IHandle<Response<PaginationResponseWithResults<IEnumerable<GetAllUserCollectiblesResponseModel>>>, GetAllUserCollectiblesRequestModel>
    {
        private readonly IHttpHandler _httpClient;

        public GetAllUserCollectiblesHandler(IHttpHandler httpClient)
        {
            _httpClient = httpClient;
        }
        
        /// <summary>
        /// Get All User Collectibles
        /// More information: https://docs.cargo.build/cargo-js/cargo.api#get-all-collectibles-for-a-user
        /// </summary>
        /// <param name="request">Request parameters</param>
        /// <returns>All User Collectibles</returns>
        public async Task<Response<PaginationResponseWithResults<IEnumerable<GetAllUserCollectiblesResponseModel>>>> Handle(GetAllUserCollectiblesRequestModel request)
        {
            var response = new Response<PaginationResponseWithResults<IEnumerable<GetAllUserCollectiblesResponseModel>>>();
            try
            {
                var queryBuilder = new UrlQueryBuilder();
                queryBuilder.AppendParameter("limit", request.Limit);
                queryBuilder.AppendParameter("page", request.Page);
                
                var urlQuery = $"https://api2.cargo.build/v3/all-collectibles/{request.Address}{queryBuilder.GetQuery()}";
                var httRequest = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(urlQuery),
                };
                var httpResponse = await _httpClient.SendAsync(httRequest);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    response.Message = httpResponse.ReasonPhrase;
                    response.ResponseStatus = ResponseStatus.Fail;
                    return response;
                }
                var responseString = await httpResponse.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<PaginationResponseWithResults<IEnumerable<GetAllUserCollectiblesResponseModel>>>(responseString);
                response.Payload = data;
                return response;
            }
            catch (Exception e)
            {
                response.ResponseStatus = ResponseStatus.Fail;
                response.Message = e.Message;
                return response;
            }
        }
    }
}