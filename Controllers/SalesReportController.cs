using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LSF.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SalesReportController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public SalesReportController(APIDbContext dbContext)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            _dbContext = dbContext;
        }

        [HttpGet("GetById")]
        public IActionResult GetById([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userId = Int32.Parse(User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value);

                var salesReportsQuery = _dbContext.SalesReport.Where(report => report.UserId == userId);

                if (startDate.HasValue && endDate.HasValue)
                {
                    salesReportsQuery = salesReportsQuery.Where(report => report.SellDate >= startDate.Value && report.SellDate <= endDate.Value);
                }
                else if (startDate.HasValue)
                {
                    salesReportsQuery = salesReportsQuery.Where(report => report.SellDate >= startDate.Value);
                }
                else if (endDate.HasValue)
                {
                    salesReportsQuery = salesReportsQuery.Where(report => report.SellDate <= endDate.Value);
                }

                var salesReports = salesReportsQuery.ToList();

                return Ok(salesReports);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Ocorreu um erro ao processar a solicitação: {ex.Message}");
            }
        }

        [HttpPost("PostReport")]
        public async Task<IActionResult> PostReport(IFormFile excelFile)
        {
            var userId = Int32.Parse(User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value);

            try
            {
                if (excelFile == null || excelFile.Length == 0)
                    return BadRequest("Arquivo inválido.");

                if (!excelFile.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Formato de arquivo inválido. Apenas arquivos Excel (.xlsx) são suportados.");

                List<SalesReport> newSalesReports = new List<SalesReport>();

                using (var stream = new MemoryStream())
                {
                    await excelFile.CopyToAsync(stream);

                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0]; // Pega a primeira planilha do arquivo

                        int rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++) // Assumindo que a primeira linha são cabeçalhos
                        {
                            var sale = await ProcessRowAsync(worksheet, row, userId);
                            if (sale != null)
                            {
                                newSalesReports.Add(sale);
                            }
                        }
                    }
                }

                if (newSalesReports.Count > 0)
                {
                    _dbContext.SalesReport.AddRange(newSalesReports);
                    await _dbContext.SaveChangesAsync();
                }

                return Ok(newSalesReports);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Ocorreu um erro ao processar o arquivo: {ex.Message}");
            }
        }

        private async Task<SalesReport> ProcessRowAsync(ExcelWorksheet worksheet, int row, int userId)
        {
            var laundry = worksheet.Cells[row, 1].Value?.ToString();
            DateTime sellDate = DateTime.FromOADate((double)worksheet.Cells[row, 2].Value);
            var interprise = worksheet.Cells[row, 3].Value?.ToString();
            var interpriseDocument = worksheet.Cells[row, 4].Value?.ToString();
            var equipment = worksheet.Cells[row, 5].Value?.ToString();
            var situation = worksheet.Cells[row, 6].Value?.ToString();
            var paymentType = worksheet.Cells[row, 7].Value?.ToString();
            var value = (double)worksheet.Cells[row, 8].Value;
            var valueWithNoDiscount = (double)worksheet.Cells[row, 9].Value;
            var provider = worksheet.Cells[row, 10].Value?.ToString();
            var acquirer = worksheet.Cells[row, 11].Value?.ToString();
            var cardFlag = worksheet.Cells[row, 12].Value?.ToString();
            var cardType = worksheet.Cells[row, 13].Value?.ToString();
            var cardNumber = worksheet.Cells[row, 14].Value?.ToString();
            var authorizer = worksheet.Cells[row, 15].Value?.ToString();
            var voucher = worksheet.Cells[row, 16].Value?.ToString();
            var voucherCategory = worksheet.Cells[row, 17].Value?.ToString();
            var cupom = worksheet.Cells[row, 18].Value?.ToString();
            var cPFClient = worksheet.Cells[row, 19].Value?.ToString();
            var nameClient = worksheet.Cells[row, 20].Value?.ToString();
            var requisition = worksheet.Cells[row, 21].Value?.ToString();
            var cupomRequisition = worksheet.Cells[row, 22].Value?.ToString();
            var codeAuthSender = worksheet.Cells[row, 23].Value?.ToString();
            var error = worksheet.Cells[row, 24].Value?.ToString();
            var errorDetail = worksheet.Cells[row, 25].Value?.ToString();

            // Verifica se o relatório já existe no banco de dados
            var existingReport = await _dbContext.SalesReport.FirstOrDefaultAsync(report => report.SellDate == sellDate && report.UserId == userId);
            if (existingReport != null)
            {
                return null; // Ignorar linhas duplicadas
            }

            return new SalesReport()
            {
                UserId = userId,
                Laundry = laundry,
                SellDate = sellDate,
                Interprise = interprise,
                InterpriseDocument = interpriseDocument,
                Equipment = equipment,
                Situation = situation == "Sucesso",
                PaymentType = paymentType,
                Value = value,
                ValueWithNoDiscount = valueWithNoDiscount,
                Provider = provider,
                Acquirer = acquirer,
                CardFlag = cardFlag,
                CardType = cardType,
                CardNumber = cardNumber,
                Authorizer = authorizer,
                Voucher = voucher,
                VoucherCategory = voucherCategory,
                Cupom = cupom,
                CPFClient = cPFClient,
                NameClient = nameClient,
                Requisition = requisition,
                CupomRequisition = cupomRequisition,
                CodeAuthSender = codeAuthSender,
                Error = error,
                ErrorDetail = errorDetail,
            };
        }
    }
}
