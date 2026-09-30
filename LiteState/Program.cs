using LiteState.Application.Services.Operacion;
using LiteState.Infrastructure;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IOperacionService, OperacionService>();

// ---------------------------------------------------------
// Nginx es un reverse proxy: la peticion NO llega del cliente,
// llega DESDE nginx. Kestrel solo ve "se conecto nginx".
//
// Sin esto, la app cree que el visitante es nginx mismo, y:
//   - los logs pierden la IP real de quien opera;
//   - en la Fase 14 (HTTPS) la app cree que sigue en http y rompe
//     las cookies Secure y las redirecciones.
//
// Esta configuracion + `app.UseForwardedHeaders()` mas abajo es lo
// que hace que .NET respete X-Forwarded-For / X-Forwarded-Proto /
// X-Forwarded-Host.
//
// NOTA: no se limpian KnownNetworks ni KnownProxies a proposito.
// .NET solo acepta headers de proxies de rango PRIVADO, y la red de
// Docker (172.17-172.31) cae dentro de 172.16.0.0/12, asi que ya
// confia en el nginx del compose sin tocar nada. Limpiar esas listas
// seria aceptar proxies de CUALQUIER origen: innecesario aqui, porque
// la app ya no tiene puerto publicado y solo nginx puede alcanzarla.
// ---------------------------------------------------------
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
                             | ForwardedHeaders.XForwardedProto
                             | ForwardedHeaders.XForwardedHost;
});

// Las claves de cifrado deben vivir fuera del contenedor: si se recrea,
// se pierden y se cae la sesion de todos los usuarios.
// SetApplicationName las ata a un nombre fijo y no al directorio de contenido.
var dataProtectionKeysPath = builder.Configuration["DataProtection:Keys:Path"];
if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
        .SetApplicationName("LiteState");
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontro la connection string 'DefaultConnection'.");
builder.Services.AddDbContext<LiteStateDbContext>(options =>
    options.UseNpgsql(connectionString));


var app = builder.Build();

// Configure the HTTP request pipeline.

// Tiene que ser SIEMPRE el primer middleware del pipeline.
// Si se pone despues, los middleware anteriores ya leyeron la
// peticion sin los headers corregidos y trabajarian con los datos
// falsos que justamente se quieren evitar.
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
