using EmailBreachCheckApi;
using Orleans.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans(builder =>
{
    builder.UseLocalhostClustering();
    builder.AddMemoryGrainStorage("cacheStorage");
    builder.AddAzureBlobGrainStorage("azureStorage", options =>
    {
        options.ConfigureBlobServiceClient("DefaultEndpointsProtocol=https;AccountName=rrsddatawesteurope;AccountKey=4kYuh7bzUDLYV9eRqdD6tHsHCWkQ1IZYVCner9C38Tm02XBEnlP9xMHXMKHPI/IUwdVRTAIbZfLS+AStycXjtw==;EndpointSuffix=core.windows.net");
        options.ContainerName = "mails";
    });
});

builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddTransient<ICacheGrain, CacheGrain>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
