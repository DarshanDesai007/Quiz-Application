using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Quiz_Application.Auth;
using Quiz_Application.Data;
using Quiz_Application.Repositories;
using Quiz_Application.Services;

var builder = WebApplication.CreateBuilder(args);

// ----- EF Core -----
builder.Services.AddDbContext<QuizDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ----- Repositories -----
builder.Services.AddScoped<IQuestionRepo, QuestionRepo>();
builder.Services.AddScoped<IResponseRepo, ResponseRepo>();
builder.Services.AddScoped<ISessionRepo, SessionRepo>();

// ----- Services -----
builder.Services.AddScoped<IQuestionSvc, QuestionSvc>();
builder.Services.AddScoped<IResponseSvc, ResponseSvc>();
builder.Services.AddScoped<IValidatorSvc, ValidatorSvc>();
builder.Services.AddScoped<ISummarySvc, SummarySvc>();

// ----- Authentication (Basic) -----
builder.Services.AddAuthentication("BasicAuth")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("BasicAuth", null);

// ----- MVC + API -----
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Auto-apply migrations in production (so DB gets created/seeded)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<QuizDbContext>();
    db.Database.Migrate();
}

// ----- Middleware Pipeline -----
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Security headers
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
