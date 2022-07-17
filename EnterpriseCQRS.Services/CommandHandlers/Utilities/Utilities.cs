using EnterpriseCQRS.Domain.Responses;
using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace EnterpriseCQRS.Services.CommandHandlers.Utilities
{
    public class Utilities<T> where T : class
    {
        public async Task<GenericResponse<IList<T>>> test(Uri url)
        {
            var response = new GenericResponse<IList<T>>
            {
                Result = await CallToConsumeWebService(url).ConfigureAwait(false)
            };

            return response;
        }

        /// <summary>
        /// Process to consume web service
        /// </summary>
        /// <returns></returns>
        public async Task<IList<T>> CallToConsumeWebService(Uri url)
        {
            var response = await ConsumeServiceGetAsync(url).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response))
            {
                return JsonConvert.DeserializeObject<IList<T>>(response);
            }

            return null;
        }


        /// <summary>
        /// Consume External Service
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<string> ConsumeServiceGetAsync(Uri url)
        {
            using var httpClient = new HttpClient();

            using var response = await Policy.HandleResult<HttpResponseMessage>(message => !message.IsSuccessStatusCode)
                  .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(2), (result, timeSpan, retryCount, context) => { })
                  .ExecuteAsync(() => httpClient.GetAsync(url));

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }
}
