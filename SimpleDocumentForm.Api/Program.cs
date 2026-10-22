using System.Text.Json;
using System.Text.Json.Serialization;
using SimpleDocumentForm.Api.Forms;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddProblemDetails();

builder.Services.AddSingleton<IFormRepository>(serviceProvider => new InMemoryFormRepository(
    PredefinedForms.All,
    serviceProvider.GetRequiredService<ILogger<InMemoryFormRepository>>()));

var app = builder.Build();

// Build the predefined forms eagerly so invalid definitions are reported at startup.
_ = app.Services.GetRequiredService<IFormRepository>();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();

app.MapControllers();

app.Run();
