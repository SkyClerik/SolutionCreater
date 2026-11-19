using System.Diagnostics;

namespace DotnetGenerator
{
    /// <summary>
    /// Сервис, отвечающий за генерацию структуры папок и выполнение команд dotnet CLI.
    /// </summary>
    public class SolutionBuilderService
    {
        /// <summary>
        /// Создает решение (.sln), проект внутри подпапки и связывает их.
        /// </summary>
        public string CreateSolution(SolutionConfig config)
        {
            string targetDir = Path.Combine(config.RootPath, config.Name);

            if (Directory.Exists(targetDir))
                throw new InvalidOperationException("Папка с таким именем уже существует.");

            Directory.CreateDirectory(targetDir);

            try
            {
                RunDotnetCommand($"new sln -n \"{config.Name}\"", targetDir);

                string projectDir = Path.Combine(targetDir, config.Name);
                RunDotnetCommand($"new {config.TemplateKey} -f {config.Framework} -n \"{config.Name}\" -o \"{projectDir}\"", targetDir);

                string csprojPath = Path.Combine(projectDir, $"{config.Name}.csproj");
                RunDotnetCommand($"sln add \"{csprojPath}\"", targetDir);

                return targetDir;
            }
            catch
            {
                if (Directory.Exists(targetDir))
                    Directory.Delete(targetDir, true);
                throw;
            }
        }

        /// <summary>
        /// Запускает консольную утилиту dotnet с переданными аргументами.
        /// </summary>
        private void RunDotnetCommand(string arguments, string workingDirectory)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true
            };

            using Process? process = Process.Start(psi);
            if (process == null)
                throw new Exception("Не удалось запустить процесс dotnet CLI.");

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string error = process.StandardError.ReadToEnd();
                throw new Exception($"Сбой команды 'dotnet {arguments}':\n{error}");
            }
        }
    }
}
