using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connection = builder.Configuration.GetConnectionString("ConectaSQL");
builder.Services.AddDbContext<APIDbContext>(option => option.UseMySql(
    connection,
    ServerVersion.Parse("10.4.32-MariaDB")
    )
);

//Funcionando
//builder.Services.AddIdentityApiEndpoints<User>()
//    .AddRoles<IdentityRole>()
//    .AddEntityFrameworkStores<APIDbContext>();
//----------

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<APIDbContext>()
    .AddDefaultTokenProviders()
    .AddApiEndpoints();

//builder.Services.AddAuthentication()
//    .AddBearerToken(IdentityConstants.BearerScheme);


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Identity.Bearer";
    options.DefaultChallengeScheme = "Identity.Bearer";
})
.AddBearerToken("Identity.Bearer", options =>
{
    // Configurações do token Bearer para o esquema 'Identity.Bearer', se necessário
});

builder.Services.AddScoped<ExcelService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Obtenha uma instância de IServiceProvider para criar escopos de serviço
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;

    // Obtenha uma instância de ExcelService dentro do escopo
    var excelService = serviceProvider.GetRequiredService<ExcelService>();

    // Atualiza o banco de dados com os dados da planilha Excel
    excelService.AtualizarBancoDadosComPlanilhaExcelTecnicos(@"C:\Users\gatob\Desktop\Gabriel\LSF\Tecnicos.xlsx");
    excelService.AtualizarBancoDadosComPlanilhaExcelProdutos(@"C:\Users\gatob\Desktop\Gabriel\LSF\Produtos.xlsx");
}

app.MapGroup("default").MapIdentityApi<User>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using(var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var roles = new[] { "Admin", "Manager", "User" };

    foreach(var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

app.Run();
