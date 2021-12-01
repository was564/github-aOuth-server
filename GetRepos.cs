using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Octokit;

namespace Was564.MarkdownEditor.Function
{
    public class GetRepos
    {
        private readonly ILogger<GetRepos> _logger;

        public GetRepos(ILogger<GetRepos> log)
        {
            _logger = log;
        }

        [FunctionName("GetRepos")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.Http, Name = "access_token", In = OpenApiSecurityLocationType.Header, Scheme = OpenApiSecuritySchemeType.Bearer)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Dictionary<string, string>), Description = "사용자가 엑세스할 수 있는 모든 Repository를 표현하는 Dicitonary 입니다. Key는 레포의 ID를, Value는 레포의 이름을 나타냅니다.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/plain", bodyType: typeof(string), Description = "헤더에 제공된 토큰이 잘못된 방식으로 되어 있습니다.")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized, Description = "인증에 실패하였습니다.")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.InternalServerError, Description = "이 서버가 맛이 갔거나 Github 서버가 맛이 갔습니다.")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "repo")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var token = string.Empty;
            try
            {
                token = AuthUtils.GetTokenFromRequest(req);
            }
            catch
            {
                return new BadRequestObjectResult("Invalid Token");
            }

            var client = new GitHubClient(new ProductHeaderValue("was564"))
            {
                Credentials = new Credentials(token)
            };

            try
            {
                var repos = await client.Repository.GetAllForCurrent();
                var result = repos.ToDictionary(repo => repo.Id, repo => repo.FullName);

                return new OkObjectResult(result);
            }
            catch (AuthorizationException e)
            {
                return new UnauthorizedResult();
            }
            catch (ApiException e)
            {
                return new ContentResult() { StatusCode = 500 };
            }
        }
    }
}

