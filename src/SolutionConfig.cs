namespace DotnetGenerator
{
    /// <summary>
    /// Описание шаблона проекта для отображения в интерфейсе.
    /// </summary>
    public record TemplateInfo(string DisplayName, string CliKey, string Description);

    /// <summary>
    /// Конфигурация для создания нового .NET решения.
    /// </summary>
    public record SolutionConfig(string RootPath, string Name, string TemplateKey, string Framework = "net8.0");
}
