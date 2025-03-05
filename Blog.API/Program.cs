using Blog.Application.BlogPost.Commands.Create;
using Blog.Core;
using Blog.Infrastructure.Logging;
using Blog.Infrastructure.Persistence;
using Blog.Infrastructure.Repositories;
using Elastic.Serilog.Sinks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.Elasticsearch;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<DbContext, BlogDbContext>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IMediator, Mediator>();

builder.Services.AddScoped<IValidator<BlogPostCreateCommand>, BlogPostCreateCommandValidator>();
builder.Services.AddScoped<IRequestHandler<BlogPostCreateCommand, int>, BlogPostCreateCommandHandler>();

builder.Services.AddScoped<ILoggerService, SerilogLoggerService>();

// Serilog yapılandırması
var uri = ""; 
var username = "";  
var password = "";  

// Serilog yapılandırması
Log.Logger = new LoggerConfiguration()
    .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri(uri))
    {
        AutoRegisterTemplate = true,  // Elasticsearch template kaydı
        IndexFormat = "logs-{0:yyyy.MM.dd}",  // Günlük loglar için index formatı
        ModifyConnectionSettings = conn =>
            conn.BasicAuthentication(username, password)  // Bonsai kimlik doğrulama bilgileri
                .ServerCertificateValidationCallback((sender, cert, chain, sslPolicyErrors) => true)  // SSL hatalarını yoksay
    })
    .CreateLogger();

// Örnek log
Log.Information("This is a log message!");


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

await app.RunAsync();