using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using In.ProjectEKA.HipService.Common;
using In.ProjectEKA.HipService.Gateway;
using In.ProjectEKA.HipService.Link.Model;
using In.ProjectEKA.HipService.Logger;
using In.ProjectEKA.HipService.UserAuth.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace In.ProjectEKA.HipService.UserAuth
{
    using static Constants;

    [ApiController]
    public class UserAuthController : Controller
    {
        private readonly IGatewayClient gatewayClient;
        private readonly ILogger<UserAuthController> logger;
        private readonly GatewayConfiguration gatewayConfiguration;
        private readonly IUserAuthService userAuthService;

        public UserAuthController(IGatewayClient gatewayClient,
            ILogger<UserAuthController> logger,
            GatewayConfiguration gatewayConfiguration,
            IUserAuthService userAuthService)
        {
            this.gatewayClient = gatewayClient;
            this.logger = logger;
            this.gatewayConfiguration = gatewayConfiguration;
            this.userAuthService = userAuthService;
        }

        [Route(FETCH_MODES)]
        public async Task<ActionResult> GetAuthModes(
            [FromHeader(Name = CORRELATION_ID)] string correlationId, [FromBody] FetchRequest fetchRequest)
        {
            string cmSuffix = gatewayConfiguration.CmSuffix;
            GatewayFetchModesRequestRepresentation gr =
                userAuthService.FetchModeResponse(fetchRequest, gatewayConfiguration);
            Guid requestId = gr.requestId;

            try
            {
                logger.Log(LogLevel.Information,
                    LogEvents.UserAuth,
                    "Request for fetch-modes to gateway: {@GatewayResponse}", gr.dump(gr));
                logger.LogInformation($"cmSuffix: {{cmSuffix}}, correlationId: {{correlationId}}," +
                                      $" healthId: {{healthId}}, requestId: {{requestId}}",
                    cmSuffix, correlationId, fetchRequest.healthId, requestId);
                await gatewayClient.SendDataToGateway(PATH_FETCH_AUTH_MODES, gr, cmSuffix, correlationId);

                var i = 0;
                do
                {
                    Thread.Sleep(2000);
                    if (UserAuthMap.RequestIdToAuthModes.ContainsKey(requestId))
                    {
                        logger.LogInformation(LogEvents.UserAuth,
                            "Response about to be send for requestId: {RequestId} with authModes: {AuthModes}",
                            requestId, UserAuthMap.RequestIdToAuthModes[requestId]
                        );
                        return Ok(UserAuthMap.RequestIdToAuthModes[requestId]);
                    }

                    i++;
                } while (i < 5);
            }
            catch (Exception exception)
            {
                logger.LogError(LogEvents.UserAuth, exception, "Error happened for requestId: {RequestId} for" +
                                                               " fetch-mode request", requestId);
            }

            return new StatusCodeResult((int) HttpStatusCode.GatewayTimeout);
        }

        [Authorize]
        [HttpPost(PATH_ON_FETCH_AUTH_MODES)]
        public AcceptedResult SetAuthModes(OnFetchAuthModeRequest request)
        {
            Log.Information("On fetch mode request received." +
                            $" RequestId:{request.RequestId}, " +
                            $" Timestamp:{request.Timestamp}," +
                            $" ResponseRequestId:{request.Resp.RequestId}, ");
            if (request.Error != null)
            {
                Log.Information($" Error Code:{request.Error.Code}," +
                                $" Error Message:{request.Error.Message}.");
            }
            else if (request.Auth != null)
            {
                string authModes = string.Join(',', request.Auth.Modes);

                UserAuthMap.RequestIdToAuthModes.Add(Guid.Parse(request.Resp.RequestId), authModes);
            }

            Log.Information($"Response RequestId:{request.Resp.RequestId}");
            return Accepted();
        }

        [Route(PATH_HIP_AUTH_INIT)]
        public async Task<ActionResult> GetTransactionId(
            [FromHeader(Name = CORRELATION_ID)] string correlationId, [FromBody] AuthInitRequest authInitRequest)
        {
            string cmSuffix = gatewayConfiguration.CmSuffix;
            GatewayAuthInitRequestRepresentation gatewayAuthInitRequestRepresentation =
                userAuthService.AuthInitResponse(authInitRequest, gatewayConfiguration);
            Guid requestId = gatewayAuthInitRequestRepresentation.requestId;

            try
            {
                logger.Log(LogLevel.Information,
                    LogEvents.UserAuth,
                    "Request for auth-init to gateway: {@GatewayResponse}",
                    gatewayAuthInitRequestRepresentation.dump(gatewayAuthInitRequestRepresentation));
                logger.LogInformation($"cmSuffix: {{cmSuffix}}, correlationId: {{correlationId}}," +
                                      $" healthId: {{healthId}}, requestId: {{requestId}}",
                    cmSuffix, correlationId, authInitRequest.healthId, requestId);
                await gatewayClient.SendDataToGateway(PATH_AUTH_INIT, gatewayAuthInitRequestRepresentation, cmSuffix,
                    correlationId);
                var i = 0;
                do
                {
                    Thread.Sleep(2000);
                    if (UserAuthMap.RequestIdToTransactionIdMap.ContainsKey(requestId))
                    {
                        logger.LogInformation(LogEvents.UserAuth,
                            "Response about to be send for requestId: {RequestId} with transactionId: {TransactionId}",
                            requestId, UserAuthMap.RequestIdToTransactionIdMap[requestId]
                        );
                        return Ok(UserAuthMap.RequestIdToTransactionIdMap[requestId]);
                    }

                    i++;
                } while (i < 5);
            }
            catch (Exception exception)
            {
                logger.LogError(LogEvents.UserAuth, exception, "Error happened for requestId: {RequestId} for" +
                                                               " auth-init request", requestId);
            }

            return new StatusCodeResult((int) HttpStatusCode.GatewayTimeout);
        }

        [Authorize]
        [HttpPost(PATH_ON_AUTH_INIT)]
        public AcceptedResult SetTransactionId(AuthOnInitRequest request)
        {
            Log.Information("Auth on init request received." +
                            $" RequestId:{request.RequestId}, " +
                            $" Timestamp:{request.Timestamp},");
            if (request.Error != null)
            {
                Log.Information($" Error Code:{request.Error.Code}," +
                                $" Error Message:{request.Error.Message}.");
            }
            else if (request.Auth != null)
            {
                string transactionId = request.Auth.TransactionId;
                UserAuthMap.RequestIdToTransactionIdMap.Add(Guid.Parse(request.Resp.RequestId), transactionId);
            }

            Log.Information($"Response RequestId:{request.Resp.RequestId}");
            return Accepted();
        }

        [Route(HIP_AUTH_CONFIRM)]
        public async Task<ActionResult> GetAccessToken(
            [FromHeader(Name = CORRELATION_ID)] string correlationId, [FromBody] AuthConfirmRequest authConfirmRequest)
        {
            string cmSuffix = gatewayConfiguration.CmSuffix;
            GatewayAuthConfirmRequestRepresentation gatewayAuthConfirmRequestRepresentation =
                userAuthService.AuthConfirmResponse(authConfirmRequest);
            Guid requestId = gatewayAuthConfirmRequestRepresentation.requestId;

            try
            {
                logger.Log(LogLevel.Information,
                    LogEvents.UserAuth,
                    "Request for auth-confirm to gateway: {@GatewayResponse}",
                    gatewayAuthConfirmRequestRepresentation.dump(gatewayAuthConfirmRequestRepresentation));
                logger.LogInformation($"cmSuffix: {{cmSuffix}}, correlationId: {{correlationId}}," +
                                      $" authCode: {{authCode}}, transactionId: {{transactionId}} requestId: {{requestId}}",
                    cmSuffix, correlationId, authConfirmRequest.authCode, authConfirmRequest.transactionId, requestId);
                await gatewayClient.SendDataToGateway(PATH_AUTH_CONFIRM, gatewayAuthConfirmRequestRepresentation
                    , cmSuffix, correlationId);
                var i = 0;
                do
                {
                    Thread.Sleep(2000);
                    if (UserAuthMap.RequestIdToAccessToken.ContainsKey(requestId))
                    {
                        logger.LogInformation(LogEvents.UserAuth,
                            "Response about to be send for requestId: {RequestId} with accessToken: {AccessToken}",
                            requestId, UserAuthMap.RequestIdToAccessToken[requestId]
                        );
                        return Ok(UserAuthMap.RequestIdToAccessToken[requestId]);
                    }

                    i++;
                } while (i < 5);
            }
            catch (Exception exception)
            {
                logger.LogError(LogEvents.UserAuth, exception, "Error happened for requestId: {RequestId}", requestId);
            }

            return new StatusCodeResult((int) HttpStatusCode.GatewayTimeout);
        }

        [Authorize]
        [HttpPost(ON_AUTH_CONFIRM)]
        public AcceptedResult SetAccessToken(OnAuthConfirmRequest request)
        {
            Log.Information("Auth on confirm request received." +
                            $" RequestId:{request.requestID}, " +
                            $" Timestamp:{request.timestamp}," +
                            $" ResponseRequestId:{request.resp.RequestId}, ");
            if (request.error != null)
            {
                Log.Information($" Error Code:{request.error.Code}," +
                                $" Error Message:{request.error.Message}.");
            }
            else if (request.auth != null)
            {
                string accessToken = request.auth.accessToken;


                UserAuthMap.RequestIdToAccessToken.Add(Guid.Parse(request.resp.RequestId), accessToken);
            }

            Log.Information($"Response RequestId:{request.resp.RequestId}");
            return Accepted();
        }
    }
}