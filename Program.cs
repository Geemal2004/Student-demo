using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.Services;
using BlindMatchPAS.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity with strict password policy
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Register services
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IProposalService, ProposalService>();
builder.Services.AddScoped<IMatchingService, MatchingService>();
builder.Services.AddScoped<IResearchAreaService, ResearchAreaService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IExpertiseService, ExpertiseService>();

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StudentOnly", p => p.RequireRole("Student"));
    options.AddPolicy("SupervisorOnly", p => p.RequireRole("Supervisor"));
    options.AddPolicy("LeaderOrAdmin", p => p.RequireRole("ModuleLeader", "SysAdmin"));
});

var app = builder.Build();

// Security middleware headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

// Exception handler
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Custom error pages
app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

// Seed data on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();

    await EnsureDatabaseReadyAsync(dbContext);
    await SeedData.InitializeAsync(services);
}

app.Run();

static async Task EnsureDatabaseReadyAsync(ApplicationDbContext dbContext)
{
    try
    {
        await dbContext.Database.MigrateAsync();
    }
    catch (SqlException ex) when (ex.Number == 2714)
    {
        await MarkInitialMigrationAsAppliedAsync(dbContext);
        await dbContext.Database.MigrateAsync();
    }
}

static async Task MarkInitialMigrationAsAppliedAsync(ApplicationDbContext dbContext)
{
    const string baselineMigrationSql = """
IF OBJECT_ID(N'[__EFMigrationsHistory]', N'U') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416100429_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260416100429_InitialCreate', N'8.0.26');
END;
""";

    await dbContext.Database.ExecuteSqlRawAsync(baselineMigrationSql);
}
