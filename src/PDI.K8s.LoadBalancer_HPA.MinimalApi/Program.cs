using System.Net;
using System.Net.Sockets;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Controllers
app.MapGet("/api/localip", () =>
{
    var localIp = GetLocalIpAddress();
    
    return localIp;

}).WithName("GetInternalIP");

app.MapGet("/health", () => "healthy").WithName("HealthCheck");

app.Run();

static string GetLocalIpAddress()
{
    var host = Dns.GetHostEntry(Dns.GetHostName());
    foreach (var ip in host.AddressList)
    {
        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            return ip.ToString();
        }
    }
    throw new Exception("No network adapters with an IPv4 address in the system!");
}