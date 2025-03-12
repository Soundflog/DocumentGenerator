namespace DocumentGenerator.Models;

// Класс для представления поля, которое нужно заполнить
public enum FieldType
{
    String,
    Numeric
}

public class Field
{
    public string Name { get; set; }
    public string Value { get; set; }
    public FieldType DataType { get; set; }
    public string Description { get; set; }
}