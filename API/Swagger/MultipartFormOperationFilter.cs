using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API.Swagger
{
    public class MultipartFormOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasConsumesMultipart = context.ApiDescription.SupportedRequestFormats
                .Any(f => string.Equals(f.MediaType, "multipart/form-data", StringComparison.OrdinalIgnoreCase));

            if (!hasConsumesMultipart) return;

            var properties = new Dictionary<string, OpenApiSchema>();

            foreach (var p in context.ApiDescription.ParameterDescriptions)
            {
                if (p.Source?.Id != "Form") continue;

                var name = p.Name;

                if (p.Type == typeof(IFormFile) || p.Type == typeof(IFormFileCollection))
                {
                    properties[name] = new OpenApiSchema { Type = "string", Format = "binary" };
                    continue;
                }

                if (p.Type == typeof(bool) || p.Type == typeof(bool?))
                {
                    properties[name] = new OpenApiSchema
                    {
                        Type = "boolean",
                        Default = new OpenApiBoolean(false)
                    };
                    continue;
                }

                properties[name] = new OpenApiSchema { Type = "string" };
            }

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content =
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = properties
                        }
                    }
                }
            };
        }
    }
}
