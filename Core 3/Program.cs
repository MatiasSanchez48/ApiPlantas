using Core_3;
using Core_3.Hubs;
using Core_3.Models;
using Core_3.Services;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuramos la clase de mongodb y agarramos nuestro modelo.
var usuarioSettings = builder.Configuration.GetSection("UsuarioSettings");
var chatsSettings = builder.Configuration.GetSection("ChatsSettings");
var PlantsSettings = builder.Configuration.GetSection("PlantsSettings");

builder.Services.AddSignalR();
builder.Services.AddCors(
    options =>
    {
        options.AddDefaultPolicy(builder =>
        {
            /// Tenees que poner la URL de flutter cuando lo abris que varian despues de los 2 puntos
            builder.WithOrigins("http://10.0.2.2:9095").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
            builder.WithOrigins("http://localhost:9095").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        });
    }
    );
builder.Services.AddSingleton<IDictionary<string,UserConnection>>( opts => new Dictionary<string, UserConnection>());

// Configuramos la interface de nuestro modelo con la interface para que se pueda establecer una conexion.
builder.Services.Configure<DBSettings>(usuarioSettings);
builder.Services.Configure<DBSettings>(chatsSettings);
builder.Services.Configure<DBSettings>(PlantsSettings);

builder.Services.AddSingleton<IDBSettings>(sp =>
    sp.GetRequiredService<IOptions<DBSettings>>().Value);

// Registrar las configuraciones de MongoDB
builder.Services.AddSingleton(usuarioSettings);
builder.Services.AddSingleton(chatsSettings);
builder.Services.AddSingleton(PlantsSettings);

builder.Services.AddSingleton<Chat>();
builder.Services.AddSingleton<Usuario>();
builder.Services.AddSingleton<ChatMessage>();
builder.Services.AddSingleton<Plantas>();

builder.Services.AddSingleton<AuthServices>();
builder.Services.AddSingleton<ChatsServices>();
builder.Services.AddSingleton<JardinServices>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configuración de endpoint WebSocket
app.UseWebSockets();
app.UseRouting();

app.UseCors();

app.UseEndpoints(endpoints =>
{
     endpoints.MapHub<ChatHub>("/chat");
});

app.UseAuthorization();
app.MapControllers();
app.Run();
