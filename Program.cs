using Amazon.S3;
using Amazon;
using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configurar conex�o com o banco de dados
var connection = builder.Configuration.GetConnectionString("ConectaSQL");
builder.Services.AddDbContext<APIDbContext>(option => option.UseMySql(
    connection,
    ServerVersion.Parse("10.4.32-MariaDB")
));

// Configurar o cliente Amazon S3 manualmente
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new AmazonS3Client(config["AWS:AccessKey"], config["AWS:SecretKey"], RegionEndpoint.GetBySystemName(config["AWS:Region"]));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = ParameterLocation.Header,
        Description = "Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
            },
            Array.Empty<string>()
        }
    });
});

var jwtIssuer = builder.Configuration.GetSection("Jwt:Issuer").Get<string>();
var jwtKey = builder.Configuration.GetSection("Jwt:Key").Get<string>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
 .AddJwtBearer(options =>
 {
     options.TokenValidationParameters = new TokenValidationParameters
     {
         ValidateIssuer = true,
         ValidateAudience = true,
         ValidateLifetime = true,
         ValidateIssuerSigningKey = true,
         ValidIssuer = jwtIssuer,
         ValidAudience = jwtIssuer,
         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
     };
 });

builder.Services.AddAuthorization();

//builder.Services.AddScoped<ExcelService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

//ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
//using (var scope = app.Services.CreateScope())
//{
//    var serviceProvider = scope.ServiceProvider;

//    // Obtenha uma inst�ncia de ExcelService dentro do escopo
//    var excelService = serviceProvider.GetRequiredService<ExcelService>();

//    // Atualiza o banco de dados com os dados da planilha Excel
//    excelService.AtualizarBancoDadosComPlanilhaExcelBotErros(@"C:\Users\gatob\Desktop\Gabriel\NovoLSF\BotError.xlsx");
//}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
