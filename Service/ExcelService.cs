//using LSF.Data;
//using LSF.Models;
//using OfficeOpenXml;
//using System;
//using System.IO;
//using System.Linq;

//public class ExcelService
//{
//    private readonly APIDbContext _dbContext;

//    public ExcelService(APIDbContext dbContext)
//    {
//        _dbContext = dbContext;
//    }

//    public void AtualizarBancoDadosComPlanilhaExcelTecnicos(string caminhoPlanilha)
//    {
//        using (var package = new ExcelPackage(new FileInfo(caminhoPlanilha)))
//        {
//            ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // Supondo que a planilha está na primeira guia

//            int linhaInicial = 2; // Supondo que a primeira linha contém cabeçalhos e os dados começam na segunda linha

//            int totalLinhas = worksheet.Dimension.Rows;

//            for (int linha = linhaInicial; linha <= totalLinhas; linha++)
//            {
//                // Ler os dados da planilha
//                int id = Convert.ToInt32(worksheet.Cells[linha, 1].Value); // Supondo que o ID está na primeira coluna
//                string nome = worksheet.Cells[linha, 2].Value?.ToString(); // Supondo que o nome está na segunda coluna
//                string email = worksheet.Cells[linha, 3].Value?.ToString(); // Supondo que o nome está na segunda coluna
//                string tel = worksheet.Cells[linha, 4].Value?.ToString(); // Supondo que o nome está na segunda coluna
//                string cidade = worksheet.Cells[linha, 5].Value?.ToString(); // Supondo que o nome está na segunda coluna
//                string estado = worksheet.Cells[linha, 6].Value?.ToString(); // Supondo que o nome está na segunda coluna
//                bool ativo = worksheet.Cells[linha, 7].Value?.ToString() == "SIM" ? true : false; // Supondo que o nome está na segunda coluna

//                // Procurar o registro no banco de dados
//                var tecnico = _dbContext.Technician.FirstOrDefault(t => t.Id == id);

//                // Se o técnico não existir, pode ser criado, se necessário

//                // Atualizar os dados do técnico, se necessário
//                if (tecnico == null)
//                {
//                    tecnico = new Technician
//                    {
//                        Id = id,
//                        Name = nome ?? "",
//                        Email = email ?? "",
//                        Phone = tel ?? "",
//                        City = cidade ?? "",
//                        Country = estado ?? "",
//                        Active = ativo
//                        // Defina outras propriedades conforme necessário
//                    };

//                    _dbContext.Technician.Add(tecnico);
//                }
//                else
//                {
//                    // Atualizar os dados do técnico
//                    tecnico.Name = nome ?? "";
//                    tecnico.Email = email ?? "";
//                    tecnico.Phone = tel ?? "";
//                    tecnico.City = cidade ?? "";
//                    tecnico.Country = estado ?? "";
//                    tecnico.Active = ativo;
//                    // Atualize outras propriedades conforme necessário
//                }
//            }

//            // Salvar as alterações no banco de dados
//            _dbContext.SaveChanges();
//        }
//    }

//    public void AtualizarBancoDadosComPlanilhaExcelProdutos(string caminhoPlanilha)
//    {
//        using (var package = new ExcelPackage(new FileInfo(caminhoPlanilha)))
//        {
//            ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // Supondo que a planilha está na primeira guia

//            int linhaInicial = 2; // Supondo que a primeira linha contém cabeçalhos e os dados começam na segunda linha

//            int totalLinhas = worksheet.Dimension.Rows;

//            for (int linha = linhaInicial; linha <= totalLinhas; linha++)
//            {
//                // Ler os dados da planilha
//                int id = linha - 1; // Supondo que o ID está na primeira coluna
//                string cidade = worksheet.Cells[linha, 1].Value?.ToString(); // Supondo que o nome está na segunda coluna
//                string nome_dis = worksheet.Cells[linha, 2].Value?.ToString(); // Supondo que o nome está na segunda coluna
//                string nome_res = worksheet.Cells[linha, 4].Value?.ToString(); // Supondo que o nome está na segunda coluna
//                string tel = worksheet.Cells[linha, 5].Value?.ToString(); // Supondo que o nome está na segunda coluna

//                // Procurar o registro no banco de dados
//                var produto = _dbContext.Supplier.FirstOrDefault(t => t.Id == id);

//                // Se o técnico não existir, pode ser criado, se necessário

//                // Atualizar os dados do técnico, se necessário
//                if (produto == null)
//                {
//                    produto = new Supplier
//                    {
//                        Id = id,
//                        City = cidade ?? "",
//                        SupplierName = nome_dis,
//                        SupplierResponsible = nome_res,
//                        Phone = tel

//                        // Defina outras propriedades conforme necessário
//                    };

//                    _dbContext.Supplier.Add(produto);
//                }
//                else
//                {
//                    // Atualizar os dados do técnico
//                    produto.Id = id;
//                    produto.City = cidade ?? "";
//                    produto.SupplierName = nome_dis ?? "";
//                    produto.SupplierResponsible = nome_res ?? "";
//                    produto.Phone = tel ?? "";
//                    // Atualize outras propriedades conforme necessário
//                }
//            }

//            // Salvar as alterações no banco de dados
//            _dbContext.SaveChanges();
//        }
//    }

//    public void AtualizarBancoDadosComPlanilhaExcelBotErros(string caminhoPlanilha)
//    {
//        using (var package = new ExcelPackage(new FileInfo(caminhoPlanilha)))
//        {
//            ExcelWorksheet worksheet = package.Workbook.Worksheets[1]; // Supondo que a planilha está na primeira guia

//            int linhaInicial = 1; // Supondo que a primeira linha contém cabeçalhos e os dados começam na segunda linha

//            int totalLinhas = worksheet.Dimension.Rows;

//            for (int linha = linhaInicial; linha <= totalLinhas; linha++)
//            {
//                // Ler os dados da planilha
//                int id = linha - 1;
//                string visor = worksheet.Cells[linha, 1].Value?.ToString(); // Supondo que o nome está na segunda coluna
//                string description = worksheet.Cells[linha, 2].Value?.ToString(); // Supondo que o nome está na segunda coluna
//                string cause = worksheet.Cells[linha, 3].Value?.ToString(); // Supondo que o nome está na segunda coluna

//                // Procurar o registro no banco de dados
//                var bot = _dbContext.BotError.FirstOrDefault(t => t.Id == id);

//                // Atualizar os dados do técnico, se necessário
//                if (bot == null)
//                {
//                    bot = new BotError
//                    {
//                        //Id = id,
//                        Visor = visor ?? "",
//                        Description = description,
//                        Cause = cause
//                        // Defina outras propriedades conforme necessário
//                    };

//                    _dbContext.BotError.Add(bot);
//                }
//                else
//                {
//                    //bot.Id = id;
//                    bot.Visor = visor ?? "";
//                    bot.Description = description ?? "";
//                    bot.Cause = cause ?? "";
//                }
//            }

//            _dbContext.SaveChanges();
//        }
//    }
//}
