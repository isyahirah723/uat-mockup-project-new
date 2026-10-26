using Microsoft.EntityFrameworkCore;
using UAT_System_API.Data;
using System.Text.Json.Serialization; 

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins(
                        "http://localhost:5173",
                        "https://localhost:5173",
                        "http://localhost:5174")
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<EmailService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseCors("AllowFrontend");
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Uploaded files are read by the Vue app with fetch() to embed them in the PDF report.
        // Always send the header (not only when an Origin header is present): the browser first
        // loads the same image through an <img> tag, which sends no Origin, and caches that
        // response. Without this header the cached copy later makes fetch() fail with a CORS error.
        // Restrict this to the real frontend origin when deploying.
        ctx.Context.Response.Headers["Access-Control-Allow-Origin"] = "*";
    }
});

app.UseAuthorization();
app.MapControllers();

app.Run();