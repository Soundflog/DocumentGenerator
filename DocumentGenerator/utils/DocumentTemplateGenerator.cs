using System.IO;
using DocumentGenerator.Models;
using OfficeOpenXml;
using Xceed.Words.NET;

namespace DocumentGenerator.utils;

// Класс для генерации финального документа на основе шаблона и заполненных полей
public static class DocumentTemplateGenerator
{
    public static byte[] Generate(string templatePath, List<Field> fields)
    {
        var extension = Path.GetExtension(templatePath).ToLower();
        if (extension == ".docx")
            return GenerateWord(templatePath, fields);
        
        if (extension == ".xlsx")
            return GenerateExcel(templatePath, fields);
        
        throw new NotSupportedException("Неподдерживаемый формат шаблона");
    }

    private static byte[] GenerateWord(string templatePath, List<Field> fields)
    {
        // DocX.IsCommercialLicenseRed = false
        using var document = DocX.Load(templatePath);
        foreach (var field in fields)
        {
            var placeholder = "{{" + field.Name + "}}";
            document.ReplaceText(placeholder, field.Value);
        }

        using var ms = new MemoryStream();
        document.SaveAs(ms);
        return ms.ToArray();
    }

    private static byte[] GenerateExcel(string templatePath, List<Field> fields)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage(new FileInfo(templatePath));
        var worksheet = package.Workbook.Worksheets[0];
        if (worksheet.Dimension == null) return package.GetAsByteArray();
        for (var row = worksheet.Dimension.Start.Row; row <= worksheet.Dimension.End.Row; row++)
        {
            for (var col = worksheet.Dimension.Start.Column; col <= worksheet.Dimension.End.Column; col++)
            {
                var cell = worksheet.Cells[row, col];
                if (cell?.Value == null) continue;
                var cellText = cell.Value.ToString();
                foreach (var field in fields)
                {
                    var placeholder = "{{" + field.Name + "}}";
                    if (cellText != null && cellText.Contains(placeholder))
                    {
                        cell.Value = cellText.Replace(placeholder, field.Value);
                    }
                }
            }
        }

        return package.GetAsByteArray();
    }
}