using LSF.Data;
using LSF.Models;
using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;

public class ExcelService
{
    private readonly APIDbContext _dbContext;

    public ExcelService(APIDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void AtualizarBancoDadosComPlanilhaExcelTecnicos(string caminhoPlanilha)
    {
        using (var package = new ExcelPackage(new FileInfo(caminhoPlanilha)))
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // Supondo que a planilha está na primeira guia

            int linhaInicial = 2; // Supondo que a primeira linha contém cabeçalhos e os dados começam na segunda linha

            int totalLinhas = worksheet.Dimension.Rows;

            for (int linha = linhaInicial; linha <= totalLinhas; linha++)
            {
                // Ler os dados da planilha
                int id = Convert.ToInt32(worksheet.Cells[linha, 1].Value); // Supondo que o ID está na primeira coluna
                string nome = worksheet.Cells[linha, 2].Value?.ToString(); // Supondo que o nome está na segunda coluna
                string email = worksheet.Cells[linha, 3].Value?.ToString(); // Supondo que o nome está na segunda coluna
                string tel = worksheet.Cells[linha, 4].Value?.ToString(); // Supondo que o nome está na segunda coluna
                string cidade = worksheet.Cells[linha, 5].Value?.ToString(); // Supondo que o nome está na segunda coluna
                string estado = worksheet.Cells[linha, 6].Value?.ToString(); // Supondo que o nome está na segunda coluna
                bool ativo = worksheet.Cells[linha, 7].Value?.ToString() == "SIM" ? true : false; // Supondo que o nome está na segunda coluna

                // Procurar o registro no banco de dados
                var tecnico = _dbContext.Tecnicos.FirstOrDefault(t => t.Id == id);

                // Se o técnico não existir, pode ser criado, se necessário

                // Atualizar os dados do técnico, se necessário
                if (tecnico == null)
                {
                    tecnico = new Tecnicos
                    {
                        Id = id,
                        UserName = nome ?? "",
                        Email = email ?? "",
                        Telefone = tel ?? "",
                        Cidade = cidade ?? "",
                        Estado = estado ?? "",
                        Ativo = ativo
                        // Defina outras propriedades conforme necessário
                    };

                    _dbContext.Tecnicos.Add(tecnico);
                }
                else
                {
                    // Atualizar os dados do técnico
                    tecnico.UserName = nome ?? "";
                    tecnico.Email = email ?? "";
                    tecnico.Telefone = tel ?? "";
                    tecnico.Cidade = cidade ?? "";
                    tecnico.Estado = estado ?? "";
                    tecnico.Ativo = ativo;
                    // Atualize outras propriedades conforme necessário
                }
            }

            // Salvar as alterações no banco de dados
            _dbContext.SaveChanges();
        }
    }

    public void AtualizarBancoDadosComPlanilhaExcelProdutos(string caminhoPlanilha)
    {
        using (var package = new ExcelPackage(new FileInfo(caminhoPlanilha)))
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // Supondo que a planilha está na primeira guia

            int linhaInicial = 2; // Supondo que a primeira linha contém cabeçalhos e os dados começam na segunda linha

            int totalLinhas = worksheet.Dimension.Rows;

            for (int linha = linhaInicial; linha <= totalLinhas; linha++)
            {
                // Ler os dados da planilha
                int id = linha - 1; // Supondo que o ID está na primeira coluna
                string cidade = worksheet.Cells[linha, 1].Value?.ToString(); // Supondo que o nome está na segunda coluna
                string nome_dis = worksheet.Cells[linha, 2].Value?.ToString(); // Supondo que o nome está na segunda coluna
                string nome_res = worksheet.Cells[linha, 4].Value?.ToString(); // Supondo que o nome está na segunda coluna
                string tel = worksheet.Cells[linha, 5].Value?.ToString(); // Supondo que o nome está na segunda coluna
              
                // Procurar o registro no banco de dados
                var produto = _dbContext.Fornecedor.FirstOrDefault(t => t.Id == id);

                // Se o técnico não existir, pode ser criado, se necessário

                // Atualizar os dados do técnico, se necessário
                if (produto == null)
                {
                    produto = new Fornecedor
                    {
                        Id = id,
                        Cidade = cidade ?? "",
                        NomeDistribuidor = nome_dis,
                        NomeResponsavel = nome_res,
                        Telefone = tel

                        // Defina outras propriedades conforme necessário
                    };

                    _dbContext.Fornecedor.Add(produto);
                }
                else
                {
                    // Atualizar os dados do técnico
                    produto.Id = id;
                    produto.Cidade = cidade ?? "";
                    produto.NomeDistribuidor = nome_dis ?? "";
                    produto.NomeResponsavel = nome_res ?? "";
                    produto.Telefone = tel ?? "";
                    // Atualize outras propriedades conforme necessário
                }
            }

            // Salvar as alterações no banco de dados
            _dbContext.SaveChanges();
        }
    }
}
