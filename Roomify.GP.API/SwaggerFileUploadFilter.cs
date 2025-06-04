using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Http;

namespace Roomify.GP.API.Helpers
{
    public class SwaggerFileUploadFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ParameterDescriptions.Any(p => p.Type == typeof(IFormFile)))
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    ["image"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "binary"
                                    },
                                    ["descriptionText"] = new OpenApiSchema { Type = "string" },
                                    ["roomStyle"] = new OpenApiSchema { Type = "string" },
                                    ["roomType"] = new OpenApiSchema { Type = "string" },
                                    ["userId"] = new OpenApiSchema { Type = "string", Format = "uuid" },
                                    ["saveToHistory"] = new OpenApiSchema { Type = "boolean" }
                                },
                                Required = new HashSet<string> { "image", "descriptionText" }
                            }
                        }
                    }
                };
            }
        }
    }
}
