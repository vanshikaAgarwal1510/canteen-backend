using Microsoft.EntityFrameworkCore;
using CanteenBackend.Data;
using CanteenBackend.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

//add services to the container.
builder.Services.AddControllers();

//add db context(meaning make a connection to the database if any api need db access)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=Data/canteen.db")
);


//add authentication service
var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"]!);

//“My app supports authentication.”
builder.Services.AddAuthentication(options =>
{
    //“HOW should we authenticate the user?”
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
//define how to validate the token.
.AddJwtBearer(options =>
{
    //“When I receive a token, how do I decide if it is valid?”
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,//“Only accept tokens created by MY server.”
        ValidateAudience = true,//“This token must be meant for THIS application.”
        ValidateLifetime = true,//“Reject expired tokens.”
        ValidateIssuerSigningKey = true,//“Verify token signature using my secret key.”
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)//This is the secret key used to: verify token signature ensure token was issued by YOU If someone changes token payload → signature breaks → ❌ rejected
    };
});

var app = builder.Build();

app.MapControllers();
//“Check WHO the user is.”
app.UseAuthentication();
//“Check WHAT the user is allowed to do.”
app.UseAuthorization();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    db.Database.EnsureCreated();

    if (!db.Users.Any())
    {
      
        db.Users.Add(new User
        {
            FullName = "Admin",
            Email = "admin@canteen.com",
            PasswordHash = PasswordHelper.Hash("1234"),
            Role = 1
        });

        db.SaveChanges();
    }
}

app.Run();
