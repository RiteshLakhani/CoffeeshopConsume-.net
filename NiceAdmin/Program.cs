using NiceAdmin;
using NiceAdmin.Services;

var builder = WebApplication.CreateBuilder(args);

// Register EmailSender with dependency injection
builder.Services.AddSingleton<IEmailService>(new EmailService("smtp.gmail.com", 587, "ritesh.lakhani1507@gmail.com", "ritesh@15072005"));

//Login 
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();


// Add services to the container
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Enable session middleware
app.UseSession(); // Must be added before app.MapControllerRoute

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Global exception handler for non-development environments
    app.UseHsts(); // Use HTTP Strict Transport Security
}
else
{
    app.UseDeveloperExceptionPage(); // Developer exception page for development environment
}

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
app.UseStaticFiles(); // Serve static files

app.UseRouting(); // Set up routing
// enables the authentication middleware in ASP.NET Core to handle user authentication for securing endpoints.​
app.UseAuthentication();
app.UseAuthorization(); // Set up authorization middleware

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"); // Route for areas

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); // Default route

app.Run(); // Run the application
