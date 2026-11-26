using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;
using Studie_.Components;
using Studie_.Data;
using Studie_.Models;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.RequireInteraction = false;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 10000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Configuration.GetConnectionString("DefaultConnection");

//dbcontext registration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

//identity registration
builder.Services.AddIdentity<InstitutionUsers, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<InstitutionUsers>>();

    await db.Database.MigrateAsync();

    var institution = await db.Institutions.FirstOrDefaultAsync();
    if (institution is null)
    {
        institution = new Institutions
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Institution"
        };
        db.Institutions.Add(institution);
        await db.SaveChangesAsync();
    }

    var studentEmail = "student@test.dk";
    var existingUser = await userManager.FindByEmailAsync(studentEmail);
    if (existingUser is null)
    {
        var student = new InstitutionUsers
        {
            Id = Guid.NewGuid().ToString(),
            UserName = studentEmail,
            Email = studentEmail,
            EmailConfirmed = true,
            Name = "Test Student",
            Role = "Student",
            InstitutionId = institution.Id,
            EmailNotificationsEnabled = false
        };

        var result = await userManager.CreateAsync(student, "Student123!");
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Failed to seed test student: {errors}");
        }
    }
}

app.MapGet("/auth/complete-login", async (
    [FromQuery] string userId,
    [FromQuery] string token,
    [FromQuery] bool rememberMe,
    UserManager<InstitutionUsers> userManager,
    SignInManager<InstitutionUsers> signInManager) =>
{
    var user = await userManager.FindByIdAsync(userId);
    if (user is null)
    {
        return Results.Redirect("/login");
    }

    var isValid = await userManager.VerifyUserTokenAsync(
        user,
        TokenOptions.DefaultProvider,
        "BlazorInteractiveLogin",
        token);

    if (!isValid)
    {
        return Results.Redirect("/login");
    }

    await signInManager.SignInAsync(user, isPersistent: rememberMe);

    return Results.Redirect("/");
});
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
