using PFEstudioJuridico.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Firebase.Auth.Providers;
using Firebase.Auth;
using PFEstudioJuridico.Services; // Aseg�rate de tener esta l�nea para recibir el servicio

var builder = WebApplication.CreateBuilder(args);

var dataProtectionKeysPath = builder.Configuration["DataProtection:KeysPath"] ?? "/app/keys";
Directory.CreateDirectory(dataProtectionKeysPath);
builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configurar el contexto de la base de datos
builder.Services.AddDbContext<AppDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("CadenaSQL"));
});

// Configurar Firebase Auth Client
var firebaseApiKey = builder.Configuration["Firebase:ApiKey"];
var firebaseAuthDomain = builder.Configuration["Firebase:AuthDomain"];
if (!string.IsNullOrWhiteSpace(firebaseApiKey) && !string.IsNullOrWhiteSpace(firebaseAuthDomain))
{
    builder.Services.AddSingleton(new FirebaseAuthClient(new FirebaseAuthConfig
    {
        ApiKey = firebaseApiKey,
        AuthDomain = firebaseAuthDomain,
        Providers = new FirebaseAuthProvider[]
        {
            new EmailProvider()
        }
    }));
}

// Configurar la autenticaci�n
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Acceso/Login";
        options.AccessDeniedPath = "/Acceso/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = true;
    });
builder.Services.AddScoped<AuditoriaService>();

// Registrar el servicio de auditor�a
//builder.Services.AddScoped<AuditoriaService>();

// Agregar soporte para HttpClient
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Usar un controlador de errores gen�rico en producci�n
}
else
{
    app.UseDeveloperExceptionPage(); // Usar la p�gina de excepciones para desarrollo
}

app.UseStaticFiles(); // Middleware para archivos est�ticos

app.UseRouting(); // Middleware para la enrutaci�n

app.UseAuthentication(); // Middleware para autenticaci�n
app.UseAuthorization(); // Middleware para autorizaci�n

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// Configurar la ruta por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Acceso}/{action=Login}/{id?}");

app.Run(); // Ejecutar la aplicaci�n
