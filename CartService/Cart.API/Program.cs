
using Cart.Infrastructure.Extensions;
using Cart.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddCartInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// builder.Services.AddSingleton<RabbitMqPublisher>();

builder.Services.AddCartInfrastructure(builder.Configuration);


var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
