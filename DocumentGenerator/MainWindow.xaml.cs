using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DocumentGenerator.Models;
using DocumentGenerator.utils;
using Microsoft.Win32;
using Xceed.Words.NET;
using TemplateParser = DocumentGenerator.utils.TemplateParser;

namespace DocumentGenerator;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private string _selectedTemplatePath;
    private string _selectedTemplateExtension;
    private List<Field> _fields;
    private byte[] _generatedDocument;
    
    // Регулярное выражение для разрешённых символов (цифры, точка и минус)
    private static readonly Regex NumericRegex = MyRegex();

    public MainWindow()
    {
        InitializeComponent();
    }

    // Выбор шаблона с диска
    private void btnSelectTemplate_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = "Документы Word (*.docx)|*.docx|Документы Excel (*.xlsx)|*.xlsx|Все файлы (*.*)|*.*"
        };
        if (openFileDialog.ShowDialog() == true)
        {
            _selectedTemplatePath = openFileDialog.FileName;
            _selectedTemplateExtension = System.IO.Path.GetExtension(_selectedTemplatePath).ToLower();
            Logger.Log("Выбор шаблона", _selectedTemplatePath);
            _fields = TemplateParser.Parse(_selectedTemplatePath);
            LvFields.ItemsSource = _fields;
            MessageBox.Show("Шаблон загружен. Найдено полей: " + _fields.Count);
        }
    }

    // Заполнение шаблона введёнными данными и генерация документа
    private void btnFillTemplate_Click(object sender, RoutedEventArgs e)
    {
        _generatedDocument = DocumentTemplateGenerator.Generate(_selectedTemplatePath, _fields);
        Logger.Log("Заполнение шаблона", "Генерация документа");
        MessageBox.Show("Документ сгенерирован");
    }

    // Сохранение сгенерированного документа на диск
    private void btnSaveDocument_Click(object sender, RoutedEventArgs e)
    {
        var saveFileDialog = new SaveFileDialog();
        if (_selectedTemplateExtension == ".docx")
            saveFileDialog.Filter = "Документы Word (*.docx)|*.docx";

        else if (_selectedTemplateExtension == ".xlsx")
            saveFileDialog.Filter = "Документы Excel (*.xlsx)|*.xlsx";
        else
            saveFileDialog.Filter = "Все файлы (*.*)|*.*";

        if (saveFileDialog.ShowDialog() == true)
        {
            File.WriteAllBytes(saveFileDialog.FileName, _generatedDocument);
            Logger.Log("Сохранение документа", saveFileDialog.FileName);
            MessageBox.Show("Документ сохранён");
        }
    }

    // Отправка документа по Email
    private void btnSendEmail_Click(object sender, RoutedEventArgs e)
    {
        var recipient =
            Microsoft.VisualBasic.Interaction.InputBox("Введите email получателя:", "Отправка email",
                "example@example.com");
        if (string.IsNullOrEmpty(recipient)) return;
        var result = EmailSender.SendEmail(recipient, "Ваш документ", "Документ во вложении", _generatedDocument,
            _selectedTemplateExtension);
        if (result)
        {
            Logger.Log("Отправка email", "Получатель: " + recipient);
            MessageBox.Show("Email отправлен");
        }
        else
        {
            MessageBox.Show("Ошибка отправки email");
        }
    }
    
    // Обработчик для кнопки "Сохранить логи"
    private void btnSaveLogs_Click(object sender, RoutedEventArgs e)
    {
        // Получаем логи из базы данных
        var logs = Logger.GetLogs();

        // Создаем новый документ
        var doc = DocX.Create("Logs.docx");
        doc.InsertParagraph("Логи операций")
            .FontSize(16)
            .Bold()
            .SpacingAfter(10);

        // Добавляем записи логов в документ
        foreach (var log in logs)
        {
            doc.InsertParagraph($"{log.Timestamp:yyyy-MM-dd HH:mm:ss} - {log.OperationType} - {log.Details}");
        }

        // Диалог для сохранения файла
        var saveFileDialog = new SaveFileDialog
        {
            Filter = "Документы Word (*.docx)|*.docx",
            FileName = "OperationLogs.docx"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            doc.SaveAs(saveFileDialog.FileName);
            MessageBox.Show("Логи сохранены в документе: " + saveFileDialog.FileName);
            Logger.Log("Сохранение логов", "Логи сохранены в " + saveFileDialog.FileName);
        }
    }
    
    // Ограничение ввода для числовых полей
    private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        var textBox = sender as TextBox;
        if (textBox.DataContext is Field field && field.DataType == FieldType.Numeric)
        {
            // Если введённый символ не является цифрой, точкой или знаком минус, отменяем ввод
            e.Handled = NumericRegex.IsMatch(e.Text);
        }
    }

    [GeneratedRegex("[^0-9.-]+")]
    private static partial Regex MyRegex();
}