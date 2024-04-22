using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace LSF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesReportController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public SalesReportController(APIDbContext dbContext)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            _dbContext = dbContext;
        }

        [HttpGet("GetAll")]
        public IEnumerable<SalesReport> GetAll()
        {
            return _dbContext.SalesReport.ToList();
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

                List<SalesReport> result = new List<SalesReport>();

                using (var stream = new MemoryStream())
                {
                    await excelFile.CopyToAsync(stream);

                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0]; // Pega a primeira planilha do arquivo

                        int rowCount = worksheet.Dimension.Rows;
                        int colCount = worksheet.Dimension.Columns;

                        // Dividir o processamento em lotes para melhorar a escalabilidade
                        var batchSize = 100; // Processaremos 100 linhas por vez
                        var batches = Enumerable.Range(2, rowCount - 1)
                                                .Select(x => new { Start = x, End = Math.Min(x + batchSize - 1, rowCount) });

                        var tasks = batches.Select(async batch =>
                        {
                            var batchResult = new List<SalesReport>();

                            for (int row = batch.Start; row <= batch.End; row++)
                            {
                                var sale = await ProcessRowAsync(worksheet, row, userId);
                                batchResult.Add(sale);
                            }

                            lock (result)
                            {
                                result.AddRange(batchResult);
                            }
                        });

                        await Task.WhenAll(tasks);
                    }
                }

                return Ok(result);
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
            var codeAuthSender = worksheet.Cells[row, 23].Value.ToString();
            var error = worksheet.Cells[row, 24].Value?.ToString();
            var errorDetail = worksheet.Cells[row, 25].Value?.ToString();

            return new SalesReport()
            {
                Id = 0,
                UserId = userId,
                Laundry = laundry,
                SellDate = sellDate,
                Interprise = interprise,
                InterpriseDocument = interpriseDocument,
                Equipment = equipment,
                Situation = situation == "Sucesso" ? true : false,
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
                CodeAuthSender = codeAuthSender,
                Error = error,
                ErrorDetail = errorDetail,
            };
        }
    }
}