using System.IO;
using System.Text.RegularExpressions;
using DocumentGenerator.Models;
using OfficeOpenXml;
using Xceed.Words.NET;

namespace DocumentGenerator.utils;

// Класс для парсинга шаблона и извлечения полей
public static partial class TemplateParser
{
    // Регулярное выражение для поиска плейсхолдеров вида {{FieldName}}
    private static readonly Regex PlaceholderRegex = MyRegex();

    /// <summary>
    /// Определяет тип файла по расширению и вызывает соответствующий парсер.
    /// </summary>
    public static List<Field> Parse(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLower();
        if (extension == ".xlsx")
            return ParseExcel(filePath);
        if (extension == ".docx")
            return ParseWord(filePath);
        throw new NotSupportedException("Формат файла не поддерживается");
    }

    /// <summary>
    /// Реализует разбор Excel-документа: сканирует первую вкладку, находит в ячейках шаблонные плейсхолдеры вида {{FieldName}}
    /// и возвращает список полей.
    /// </summary>
    private static List<Field> ParseExcel(string filePath)
    {
        var fields = new List<Field>();
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            var worksheet = package.Workbook.Worksheets[0];
            if (worksheet.Dimension == null)
                return fields;

            for (int row = worksheet.Dimension.Start.Row; row <= worksheet.Dimension.End.Row; row++)
            {
                for (int col = worksheet.Dimension.Start.Column; col <= worksheet.Dimension.End.Column; col++)
                {
                    var cell = worksheet.Cells[row, col];
                    if (cell?.Value != null)
                    {
                        string cellValue = cell.Value.ToString();
                        var matches = PlaceholderRegex.Matches(cellValue);
                        foreach (Match match in matches)
                        {
                            string fieldName = match.Groups[1].Value.Trim();
                            // Автоматически определяем тип поля на основе имени
                            FieldType type = FieldType.String;
                            string lowerName = fieldName.ToLower();
                            if (lowerName.Contains("кол") || lowerName.Contains("сумм") || lowerName.Contains("цена"))
                                type = FieldType.Numeric;

                            if (!fields.Exists(f => f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase)))
                            {
                                fields.Add(new Field
                                {
                                    Name = fieldName,
                                    Value = string.Empty,
                                    DataType = type,
                                    Description = $"Описание для {fieldName}"
                                });
                            }
                        }
                    }
                }
            }
        }

        return fields;
    }

    private static List<Field> ParseWord(string filePath)
    {
        var fields = new List<Field>();
        using var document = DocX.Load(filePath);
        var text = document.Text;
        var matches = PlaceholderRegex.Matches(text);
        foreach (Match match in matches)
        {
            var fieldName = match.Groups[1].Value.Trim();
            var type = FieldType.String;
            var lowerName = fieldName.ToLower();
            if (lowerName.Contains("кол") || lowerName.Contains("сумм") || lowerName.Contains("цена"))
                type = FieldType.Numeric;

            if (!fields.Exists(f => f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase)))
            {
                fields.Add(new Field
                {
                    Name = fieldName,
                    Value = string.Empty,
                    DataType = type,
                    Description = $"Описание для {fieldName}"
                });
            }
        }

        return fields;
    }

    [GeneratedRegex(@"\{\{(.+?)\}\}", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}