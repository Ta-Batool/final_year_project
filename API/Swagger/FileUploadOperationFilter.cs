using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API.Swagger
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Find parameters that are IFormFile / IEnumerable<IFormFile>
            var hasFile = context.MethodInfo.GetParameters().Any(p =>
                p.ParameterType == typeof(IFormFile) ||
                p.ParameterType == typeof(IFormFileCollection) ||
                (p.ParameterType.IsGenericType &&
                 p.ParameterType.GetGenericArguments().Any(t => t == typeof(IFormFile))));

            if (!hasFile) return;

            operation.RequestBody = new OpenApiRequestBody
            {
                Content =
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties =
                            {
                                // Generic "file" field (matches common naming)
                                ["file"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Format = "binary"
                                }
                            },
                            Required = new HashSet<string> { "file" }
                        }
                    }
                }
            };
        }
    }
}
