using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Hangfire;
using In.ProjectEKA.HipService.Common;
using In.ProjectEKA.HipService.Creation.Model;
using In.ProjectEKA.HipService.Gateway;
using In.ProjectEKA.HipService.OpenMrs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog;

namespace In.ProjectEKA.HipService.Creation
{
    using static Constants;

    [ApiController]
    public class CreationController : Controller
    {
        private readonly IGatewayClient gatewayClient;
        private readonly ILogger<CreationController> logger;
        private readonly HttpClient httpClient;
        private readonly OpenMrsConfiguration openMrsConfiguration;
        private readonly ICreationService creationService;

        public CreationController(IGatewayClient gatewayClient,
            ILogger<CreationController> logger,
            HttpClient httpClient,
            OpenMrsConfiguration openMrsConfiguration, ICreationService creationService)
        {
            this.gatewayClient = gatewayClient;
            this.logger = logger;
            this.httpClient = httpClient;
            this.openMrsConfiguration = openMrsConfiguration;
            this.creationService = creationService;
        }

        [Route(AADHAAR_GENERATE_OTP)]
        public async Task<ActionResult> GenerateAadhaarOtp(
            [FromHeader(Name = CORRELATION_ID)] string correlationId, [FromBody] AadhaarOTPGenerationRequest aadhaarOtpGenerationRequest)
        {
            HttpResponseMessage response = null;
            if (Request != null)
            {
                if (Request.Cookies.ContainsKey(REPORTING_SESSION))
                {
                    string sessionId = Request.Cookies[REPORTING_SESSION];
            
                    Task<StatusCodeResult> statusCodeResult = IsAuthorised(sessionId);
                    if (!statusCodeResult.Result.StatusCode.Equals(StatusCodes.Status200OK))
                    {
                        return statusCodeResult.Result;
                    }
                }
                else
                {
                    return StatusCode(StatusCodes.Status401Unauthorized);
                }
            }

            try
            {
                logger.Log(LogLevel.Information,
                    LogEvents.Creation,
                    "Request for generate-aadhaar-otp to gateway: {@GatewayResponse}",
                    aadhaarOtpGenerationRequest);
                logger.Log(LogLevel.Information,
                    LogEvents.Creation, $"correlationId: {{correlationId}}," +
                                        $" aadhaar: {{aadhaar}}",
                     correlationId, aadhaarOtpGenerationRequest.aadhaar);
                string text = await creationService.EncryptText(aadhaarOtpGenerationRequest.aadhaar);
                response = await gatewayClient.CallABHAService(HttpMethod.Post,AADHAAR_GENERATE_OTP, new AadhaarOTPGenerationRequest(text), correlationId);
            }
            catch (Exception exception)
            {
                logger.LogError(LogEvents.Creation, exception, "Error happened for " +
                                                               "generate-aadhaar-otp  request");
                
            }

            var responseContent = await response?.Content.ReadAsStringAsync();
            Log.Information(responseContent);
            if (response != null && response.IsSuccessStatusCode)
            {
                return Accepted(creationService.AadhaarOTPGenerationResponse(responseContent));
            }
            return StatusCode(StatusCodes.Status500InternalServerError,responseContent);
            
        }
        
        [NonAction]
        private async Task<StatusCodeResult> IsAuthorised(String sessionId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, openMrsConfiguration.Url + WHO_AM_I);
            request.Headers.Add("Cookie", OPENMRS_SESSION_ID_COOKIE_NAME + "=" + sessionId);

            var response = await httpClient.SendAsync(request).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            return StatusCode(StatusCodes.Status200OK);
        }
        
    }
}