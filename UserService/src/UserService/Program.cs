using Serilog;
using Hellang.Middleware.ProblemDetails;
using UserService.Extensions.Application;
using UserService.Extensions.Host;
using UserService.Extensions.Services;
using UserService.Databases;
using UserService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Host.AddLoggingConfiguration(builder.Environment);

builder.ConfigureServices();
var app = builder.Build();
using (var seedScope = app.Services.CreateScope())
{
    // Resolve the seed service and run the initialize method within the scope
    var seeder = seedScope.ServiceProvider.GetRequiredService<ISeedDataService>();
    seeder.Initialize().GetAwaiter().GetResult();
}
using var scope = app.Services.CreateScope();
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseProblemDetails();

app.UseHttpsRedirection();

app.UseCors("UserServiceCorsPolicy");

app.UseSerilogRequestLogging();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/api/health");
    endpoints.MapControllers();
    endpoints.MapGrpcService<GrpcUserService>();
    endpoints.MapGet("/protos/userservice.proto", async context =>
    {
        await context.Response.WriteAsync(File.ReadAllText("Protos\\UserService.proto"));
    });
});

app.UseSwaggerExtension(builder.Configuration, builder.Environment);

try
{
    Log.Information("Starting application");
    await app.RunAsync();
}
catch (Exception e)
{
    Log.Error(e, "The application failed to start correctly");
    throw;
}
finally
{
    Log.Information("Shutting down application");
    Log.CloseAndFlush();
}

public partial class Program { }