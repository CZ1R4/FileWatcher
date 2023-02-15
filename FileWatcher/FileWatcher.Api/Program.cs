using PuxDesign.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IFileWatcherService, FileWatcherService>();

//Add CORS
builder.Services.AddCors(p =>
    p.AddPolicy("enableAll", policy => { policy.WithOrigins("*").AllowAnyMethod().AllowAnyHeader(); }));

var app = builder.Build();

app.UseCors("enableAll");

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
