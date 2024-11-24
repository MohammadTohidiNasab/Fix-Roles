var builder = WebApplication.CreateBuilder(args);

var cnnString = builder.Configuration.GetConnectionString("DivarConnection");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DivarDbContext>(options => options.UseSqlServer(cnnString));
builder.Services.AddMemoryCache();
builder.Services.AddSession(); // Add Session services to project

// Identity and role management
builder.Services.AddIdentity<CustomUser, IdentityRole>()
    .AddEntityFrameworkStores<DivarDbContext>()
    .AddDefaultTokenProviders();

// Register repositories
builder.Services.AddScoped<IAdminRepository>(sp => new AdoAdminRepository(cnnString));
builder.Services.AddScoped<IAdvertisementRepository, EfAdvertisementRepository>();
builder.Services.AddScoped<IUserRepository>(sp => new AdoUserRepository(cnnString));




//polecy
builder.Services.AddAuthorization(options =>
{
    foreach (AccessLevel level in Enum.GetValues(typeof(AccessLevel)))
    {
        options.AddPolicy(level.ToString(), policy => policy.RequireClaim("Permission", level.ToString()));
    }
});





//تنظیمات پسورد
builder.Services.Configure<IdentityOptions>(c =>
{
    c.Password.RequiredLength = 5;
    c.Password.RequiredUniqueChars = 5;
    c.Password.RequireLowercase = true;
    c.Password.RequireUppercase = false;
    c.User.RequireUniqueEmail = true;
    c.Password.RequireDigit = false;

});


var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Add session to app
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
