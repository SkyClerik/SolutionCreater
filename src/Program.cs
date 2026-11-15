using System.Diagnostics;

namespace DotnetGenerator
{
    public class GeneratorForm : Form
    {
        private readonly TextBox _pathTextBox;
        private readonly TextBox _nameTextBox;
        private readonly ComboBox _typeComboBox;
        private readonly Button _createButton;
        private readonly Button _browseButton;

        public GeneratorForm()
        {
            this.Text = "Solution Creator .NET";
            this.Size = new Size(480, 260);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(32, 32, 32);
            this.ForeColor = Color.White;

            Font mainFont = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            Label pathLabel = new Label { Text = "Путь к папке проектов:", Location = new Point(20, 20), Size = new Size(200, 20), Font = mainFont };
            _pathTextBox = new TextBox { Location = new Point(20, 45), Size = new Size(330, 25), Font = mainFont, BackColor = Color.FromArgb(45, 45, 45), ForeColor = Color.White };
            _browseButton = new Button { Text = "Обзор...", Location = new Point(360, 44), Size = new Size(85, 26), Font = mainFont, BackColor = Color.FromArgb(60, 60, 60), FlatStyle = FlatStyle.Flat };
            _browseButton.Click += BrowseButton_Click;

            Label nameLabel = new Label { Text = "Имя решения и проекта:", Location = new Point(20, 85), Size = new Size(200, 20), Font = mainFont };
            _nameTextBox = new TextBox { Location = new Point(20, 110), Size = new Size(220, 25), Font = mainFont, BackColor = Color.FromArgb(45, 45, 45), ForeColor = Color.White };

            Label typeLabel = new Label { Text = "Тип приложения:", Location = new Point(260, 85), Size = new Size(150, 20), Font = mainFont };
            _typeComboBox = new ComboBox { Location = new Point(260, 110), Size = new Size(185, 25), Font = mainFont, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(45, 45, 45), ForeColor = Color.White };

            _typeComboBox.Items.Add("Console App");
            _typeComboBox.Items.Add("Windows Forms");
            _typeComboBox.Items.Add("WPF App");
            _typeComboBox.SelectedIndex = 0;

            _createButton = new Button { Text = "Создать структуру решения", Location = new Point(20, 165), Size = new Size(425, 38), Font = new Font("Segoe UI", 10f, FontStyle.Bold), BackColor = Color.FromArgb(0, 122, 204), FlatStyle = FlatStyle.Flat };
            _createButton.FlatAppearance.BorderSize = 0;
            _createButton.Click += CreateButton_Click;

            this.Controls.Add(pathLabel);
            this.Controls.Add(_pathTextBox);
            this.Controls.Add(_browseButton);
            this.Controls.Add(nameLabel);
            this.Controls.Add(_nameTextBox);
            this.Controls.Add(typeLabel);
            this.Controls.Add(_typeComboBox);
            this.Controls.Add(_createButton);
        }

        private void BrowseButton_Click(object? sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    _pathTextBox.Text = fbd.SelectedPath;
                }
            }
        }

        private void CreateButton_Click(object? sender, EventArgs e)
        {
            string rootPath = _pathTextBox.Text.Trim();
            string name = _nameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
            {
                MessageBox.Show("Укажите правильную папку.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                MessageBox.Show("Недопустимое имя.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string templateKey = _typeComboBox.SelectedIndex switch
            {
                1 => "winforms",
                2 => "wpf",
                _ => "console"
            };

            string targetDir = Path.Combine(rootPath, name);
            if (Directory.Exists(targetDir))
            {
                MessageBox.Show("Такая папка уже существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Directory.CreateDirectory(targetDir);

                // 1. Создаем .sln файл в корне
                RunDotnetCommand($"new sln -n \"{name}\"", targetDir);

                // 2. Создаем сам проект в подпапке
                string projectDir = Path.Combine(targetDir, name);
                string framework = "net8.0";
                RunDotnetCommand($"new {templateKey} -f {framework} -n \"{name}\" -o \"{projectDir}\"", targetDir);

                // 3. Добавляем проект в солюшен
                string csprojPath = Path.Combine(projectDir, $"{name}.csproj");
                RunDotnetCommand($"sln add \"{csprojPath}\"", targetDir);

                MessageBox.Show($"Решение '{name}.sln' успешно создано!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                Process.Start("explorer.exe", $"\"{targetDir}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Сбой генерации", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void RunDotnetCommand(string arguments, string workingDirectory)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true
            };

            using (Process? process = Process.Start(psi))
            {
                if (process != null)
                {
                    process.WaitForExit();
                    if (process.ExitCode != 0)
                    {
                        string error = process.StandardError.ReadToEnd();
                        throw new Exception($"Ошибка выполнения команды 'dotnet {arguments}': {error}");
                    }
                }
            }
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GeneratorForm());
        }
    }
}
