using Orleans.Hosting;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

var azureConn=builder.Configuration.GetConnectionString("AzureConnection");
builder.Host.UseOrleans(builder =>
{
    builder.UseLocalhostClustering();
    builder.AddMemoryGrainStorage("cacheStorage");
    builder.AddAzureBlobGrainStorage("azureStorage", options =>
    {
        options.ConfigureBlobServiceClient(azureConn);
        options.ContainerName = "mails";
    });
});

builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


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
