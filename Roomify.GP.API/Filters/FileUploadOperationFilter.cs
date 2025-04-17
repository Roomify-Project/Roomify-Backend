using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;


namespace Roomify.GP.API.Filters
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileParameter = context.ApiDescription.ActionDescriptor.Parameters
                .FirstOrDefault(p => p.ParameterType == typeof(IFormFile));

            if (fileParameter != null)
            {
                // Add the parameter to Swagger with 'multipart' file upload support
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = fileParameter.Name,
                    In = ParameterLocation.Query, // Use 'Query' or 'Header' or handle it via 'Body' using form data
                    Required = true,
                    Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    }
                });
            }
        }
    }


}
