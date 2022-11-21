using Serilog;

namespace In.ProjectEKA.HipService.Common
{
    using System;
    using System.Net.Http;
    using System.Net.Mime;
    using System.Text;
    using Microsoft.Net.Http.Headers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using static Constants;

    public static class HttpRequestHelper
    {
        public static HttpRequestMessage CreateHttpRequest<T>(
            string url,
            T content,
            string token,
            string cmSuffix,
            string correlationId)
        {
            var json = JsonConvert.SerializeObject(content, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            });

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri($"{url}"))
            {
                Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json)
            };
            Log.Information("TOKEN " + token + " " + HeaderNames.Authorization);
            if (token != null)
                httpRequestMessage.Headers.Add(HeaderNames.Authorization, token);
            if (cmSuffix != null)
                httpRequestMessage.Headers.Add("X-CM-ID", cmSuffix);
            if (correlationId != null)
                httpRequestMessage.Headers.Add(CORRELATION_ID, correlationId);
            Log.Information("HEADER " + httpRequestMessage.Headers?.Authorization?.Scheme + " " + httpRequestMessage.Headers?.Authorization?.Parameter);
            return httpRequestMessage;
        }

        public static HttpRequestMessage CreateHttpRequest<T>(string url, T content, String correlationId)
        {
            // ReSharper disable once IntroduceOptionalParameters.Global
            return CreateHttpRequest(url, content, null, null, correlationId);
        }
    }
}