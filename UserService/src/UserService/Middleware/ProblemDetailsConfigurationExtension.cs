namespace UserService.Middleware;

using Hellang.Middleware.ProblemDetails;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using ProblemDetailsOptions = Hellang.Middleware.ProblemDetails.ProblemDetailsOptions;
using UserService.Exceptions;

public static class ProblemDetailsConfigurationExtension
{
    public static void ConfigureProblemDetails(ProblemDetailsOptions options)
    {
        options.IncludeExceptionDetails = (ctx, ex) => false;
        options.MapFluentValidationException();
        options.MapValidationException();
        options.MapToStatusCode<ForbiddenAccessException>(StatusCodes.Status401Unauthorized);
        options.MapToStatusCode<NoRolesAssignedException>(StatusCodes.Status403Forbidden);
        options.MapToStatusCode<NotFoundException>(StatusCodes.Status404NotFound);
        options.MapToStatusCode<NotImplementedException>(StatusCodes.Status501NotImplemented);
        options.MapToStatusCode<HttpRequestException>(StatusCodes.Status503ServiceUnavailable);
        options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
    }

    private static void MapFluentValidationException(this ProblemDetailsOptions options) =>
        options.Map<FluentValidation.ValidationException>((ctx, ex) =>
        {
            var factory = ctx.RequestServices.GetRequiredService<ProblemDetailsFactory>();

            var errors = ex.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(x => x.ErrorMessage).ToArray());

            return factory.CreateValidationProblemDetails(ctx, errors);
        });

    private static void MapValidationException(this ProblemDetailsOptions options) =>
        options.Map<ValidationException>((ctx, ex) =>
        {
            var factory = ctx.RequestServices.GetRequiredService<ProblemDetailsFactory>();

            var errors = ex.Errors
                .GroupBy(x => x.Key)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(x => x.Value.ToString()).ToArray());

            return factory.CreateValidationProblemDetails(ctx, errors);
        });
}
