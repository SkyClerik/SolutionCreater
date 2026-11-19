using System.Diagnostics;

namespace DotnetGenerator
{
    /// <summary>
    /// Главное окно приложения. Отвечает за UI, подсказки и валидацию ввода.
    /// </summary>
    public class GeneratorForm : Form
    {
        private readonly SolutionBuilderService _builderService = new();
        private readonly List<TemplateInfo> _templates =
        [
            new("ASP.NET Core Empty", "web", "Минималистичный веб-сервер без лишних файлов. Идеально для локальных мостов с браузером и легких API."),
            new("ASP.NET Core Web API", "webapi", "Готовый каркас для крупных REST API с поддержкой контроллеров, OpenAPI/Swagger и маршрутизации."),
            new("Console App", "console", "Классическое консольное приложение без графического интерфейса для фоновых скриптов и утилит."),
            new("Windows Forms", "winforms", "Классическое оконное приложение для Windows с быстрой версткой интерфейса."),
            new("WPF App", "wpf", "Современное оконное приложение для Windows на базе XAML с поддержкой анимаций и стилей.")
        ];

        private readonly TextBox _pathTextBox = new();
        private readonly TextBox _nameTextBox = new();
        private readonly ComboBox _typeComboBox = new();
        private readonly Label _descriptionLabel = new();
        private readonly Button _createButton = new();
        private readonly Button _browseButton = new();

        public GeneratorForm()
        {
            InitializeWindow();
            InitializeControls();
        }

        /// <summary>
        /// Настраивает базовые параметры и тему окна.
        /// </summary>
        private void InitializeWindow()
        {
            Text = "Solution Creator .NET";
            Size = new Size(480, 325);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(32, 32, 32);
            ForeColor = Color.White;
        }

        /// <summary>
        /// Создает и размещает элементы управления на форме.
        /// </summary>
        private void InitializeControls()
        {
            var mainFont = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            var descFont = new Font("Segoe UI", 8.5f, FontStyle.Italic);
            var inputBack = Color.FromArgb(45, 45, 45);

            var pathLabel = new Label { Text = "Путь к папке проектов:", Location = new Point(20, 20), Size = new Size(200, 20), Font = mainFont };

            _pathTextBox.Location = new Point(20, 45);
            _pathTextBox.Size = new Size(330, 25);
            _pathTextBox.Font = mainFont;
            _pathTextBox.BackColor = inputBack;
            _pathTextBox.ForeColor = Color.White;

            _browseButton.Text = "Обзор...";
            _browseButton.Location = new Point(360, 44);
            _browseButton.Size = new Size(85, 26);
            _browseButton.Font = mainFont;
            _browseButton.BackColor = Color.FromArgb(60, 60, 60);
            _browseButton.FlatStyle = FlatStyle.Flat;
            _browseButton.Click += BrowseButton_Click;

            var nameLabel = new Label { Text = "Имя решения и проекта:", Location = new Point(20, 85), Size = new Size(200, 20), Font = mainFont };

            _nameTextBox.Location = new Point(20, 110);
            _nameTextBox.Size = new Size(210, 25);
            _nameTextBox.Font = mainFont;
            _nameTextBox.BackColor = inputBack;
            _nameTextBox.ForeColor = Color.White;

            var typeLabel = new Label { Text = "Тип приложения:", Location = new Point(245, 85), Size = new Size(150, 20), Font = mainFont };

            _typeComboBox.Location = new Point(245, 110);
            _typeComboBox.Size = new Size(200, 25);
            _typeComboBox.Font = mainFont;
            _typeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _typeComboBox.BackColor = inputBack;
            _typeComboBox.ForeColor = Color.White;
            _typeComboBox.DisplayMember = nameof(TemplateInfo.DisplayName);
            _typeComboBox.SelectedIndexChanged += TypeComboBox_SelectedIndexChanged;

            foreach (var template in _templates)
                _typeComboBox.Items.Add(template);

            _descriptionLabel.Location = new Point(20, 148);
            _descriptionLabel.Size = new Size(425, 45);
            _descriptionLabel.Font = descFont;
            _descriptionLabel.ForeColor = Color.FromArgb(180, 180, 180);

            _typeComboBox.SelectedIndex = 0;

            _createButton.Text = "Создать структуру решения";
            _createButton.Location = new Point(20, 205);
            _createButton.Size = new Size(425, 38);
            _createButton.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            _createButton.BackColor = Color.FromArgb(0, 122, 204);
            _createButton.FlatStyle = FlatStyle.Flat;
            _createButton.FlatAppearance.BorderSize = 0;
            _createButton.Click += CreateButton_Click;

            Controls.AddRange([pathLabel, _pathTextBox, _browseButton, nameLabel, _nameTextBox, typeLabel, _typeComboBox, _descriptionLabel, _createButton]);
        }

        /// <summary>
        /// Обновляет текст подсказки при выборе другого шаблона проекта.
        /// </summary>
        private void TypeComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_typeComboBox.SelectedItem is TemplateInfo selected)
                _descriptionLabel.Text = selected.Description;
        }

        /// <summary>
        /// Открывает диалог выбора корневой директории.
        /// </summary>
        private void BrowseButton_Click(object? sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
                _pathTextBox.Text = fbd.SelectedPath;
        }

        /// <summary>
        /// Проверяет введенные данные и запускает генерацию решения.
        /// </summary>
        private void CreateButton_Click(object? sender, EventArgs e)
        {
            if (!TryGetValidatedConfig(out SolutionConfig? config) || config == null)
                return;

            SetUiState(isBusy: true);

            try
            {
                string createdDir = _builderService.CreateSolution(config);
                MessageBox.Show($"Решение '{config.Name}.sln' успешно создано!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Process.Start("explorer.exe", $"\"{createdDir}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Сбой генерации", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetUiState(isBusy: false);
            }
        }

        /// <summary>
        /// Валидирует поля формы и собирает объект конфигурации.
        /// </summary>
        private bool TryGetValidatedConfig(out SolutionConfig? config)
        {
            config = null;
            string rootPath = _pathTextBox.Text.Trim();
            string name = _nameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
            {
                MessageBox.Show("Укажите существующую папку.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrEmpty(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                MessageBox.Show("Укажите корректное имя проекта.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            string templateKey = (_typeComboBox.SelectedItem as TemplateInfo)?.CliKey ?? "web";

            config = new SolutionConfig(rootPath, name, templateKey);
            return true;
        }

        /// <summary>
        /// Блокирует или разблокирует кнопку создания на время выполнения команд.
        /// </summary>
        private void SetUiState(bool isBusy)
        {
            _createButton.Enabled = !isBusy;
            _createButton.Text = isBusy ? "Создание..." : "Создать структуру решения";
        }
    }
}
