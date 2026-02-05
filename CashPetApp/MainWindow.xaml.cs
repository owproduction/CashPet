using System;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FinancialTamagotchi
{
    public partial class MainWindow : Window
    {
        // Текущий пользователь (пока заглушка)
        private class User
        {
            public int user_id { get; set; }
            public string name { get; set; }
            public double current_balance { get; set; }
        }

        private User currentUser;
        private HttpClient client;

        public MainWindow()
        {
            InitializeComponent();

            // Инициализация HTTP клиента
            client = new HttpClient();
            try
            {
                client.BaseAddress = new Uri("http://localhost:8000");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            }
            catch
            {
                // Игнорируем ошибки инициализации
            }

            // Заглушка пользователя для демонстрации
            currentUser = new User
            {
                user_id = 1,
                name = "Иван Иванов",
                current_balance = 15000.50
            };

            // Обновляем интерфейс
            UpdateUI();

            // Загружаем реальные данные с сервера (асинхронно)
            LoadUserData();
        }

        private async void LoadUserData()
        {
            try
            {
                var response = await client.GetAsync($"/users/{currentUser.user_id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var user = JsonSerializer.Deserialize<User>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (user != null)
                    {
                        currentUser = user;
                        Dispatcher.Invoke(UpdateUI);
                    }
                }
            }
            catch (Exception ex)
            {
                // Если сервер не доступен, используем заглушку
                Console.WriteLine($"Не удалось загрузить данные: {ex.Message}");
            }
        }

        private void UpdateUI()
        {
            WelcomeText.Text = $"Привет, {currentUser.name}!";
            CurrencyText.Text = $"{currentUser.current_balance:N2} ₽";
        }

        // Обработчики нажатий кнопок меню
        private void ExpenseButton_Click(object sender, RoutedEventArgs e)
        {
            ShowAddExpenseDialog();
        }

        private void IncomeButton_Click(object sender, RoutedEventArgs e)
        {
            ShowAddIncomeDialog();
        }

        private void PetButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPetDialog();
        }

        private void GoalsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowGoalsDialog();
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем контекстное меню
            var contextMenu = new ContextMenu
            {
                Background = Brushes.White,
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1)
            };

            // Пункты меню
            var menuItems = new[]
            {
                new { Header = "🔄 Обновить баланс", Action = new Action(() => LoadUserData()) },
                new { Header = "👤 Профиль", Action = new Action(() => ShowProfileDialog()) },
                new { Header = "⚙ Настройки", Action = new Action(() => ShowSettingsDialog()) },
                new { Header = "", Action = (Action)null },
                new { Header = "🚪 Выход", Action = new Action(() => Application.Current.Shutdown()) }
            };

            foreach (var item in menuItems)
            {
                if (string.IsNullOrEmpty(item.Header))
                {
                    contextMenu.Items.Add(new Separator());
                    continue;
                }

                var menuItem = new MenuItem
                {
                    Header = item.Header,
                    FontSize = 14,
                    Padding = new Thickness(10, 5, 10, 5)
                };

                if (item.Action != null)
                {
                    menuItem.Click += (s, args) => item.Action();
                }

                contextMenu.Items.Add(menuItem);
            }

            // Показываем меню
            contextMenu.PlacementTarget = MenuButton;
            contextMenu.IsOpen = true;
        }

        // Диалог добавления траты
        private void ShowAddExpenseDialog()
        {
            var dialog = new Window
            {
                Title = "💸 Добавить трату",
                Width = 400,
                Height = 450,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Owner = this,
                Background = Brushes.White
            };

            var stackPanel = new StackPanel
            {
                Margin = new Thickness(20)
            };

            // Создаем промежуточный StackPanel для отступов
            var innerStackPanel = new StackPanel();

            // Сумма
            var amountLabel = new TextBlock
            {
                Text = "Сумма (₽):",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var amountTextBox = new TextBox
            {
                FontSize = 16,
                Height = 40,
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 20)
            };

            // Категория
            var categoryLabel = new TextBlock
            {
                Text = "Категория:",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var categoryComboBox = new ComboBox
            {
                FontSize = 16,
                Height = 40,
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 20)
            };

            categoryComboBox.Items.Add("🍔 Продукты");
            categoryComboBox.Items.Add("🚌 Транспорт");
            categoryComboBox.Items.Add("🏠 Коммуналка");
            categoryComboBox.Items.Add("👕 Одежда");
            categoryComboBox.Items.Add("💊 Здоровье");
            categoryComboBox.Items.Add("🎬 Развлечения");
            categoryComboBox.SelectedIndex = 0;

            // Описание
            var descLabel = new TextBlock
            {
                Text = "Описание (необязательно):",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var descTextBox = new TextBox
            {
                FontSize = 16,
                Height = 60,
                Padding = new Thickness(10),
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Margin = new Thickness(0, 0, 0, 20)
            };

            // Кнопки
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var addButton = new Button
            {
                Content = "Добавить",
                Width = 120,
                Height = 40,
                FontSize = 16,
                Background = Brushes.OrangeRed,
                Foreground = Brushes.White
            };

            addButton.Click += (s, e) =>
            {
                if (double.TryParse(amountTextBox.Text, out double amount) && amount > 0)
                {
                    // Здесь будет вызов API
                    currentUser.current_balance -= amount;
                    UpdateUI();

                    MessageBox.Show($"Трата на {amount:N2}₽ добавлена!\nКатегория: {categoryComboBox.SelectedItem}\n{(!string.IsNullOrEmpty(descTextBox.Text) ? $"Описание: {descTextBox.Text}" : "")}",
                                    "Успешно!", MessageBoxButton.OK, MessageBoxImage.Information);
                    dialog.Close();
                }
                else
                {
                    MessageBox.Show("Введите корректную сумму!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            var cancelButton = new Button
            {
                Content = "Отмена",
                Width = 120,
                Height = 40,
                FontSize = 16,
                Background = Brushes.Gray,
                Foreground = Brushes.White,
                Margin = new Thickness(10, 0, 0, 0)
            };

            cancelButton.Click += (s, e) => dialog.Close();

            buttonPanel.Children.Add(addButton);
            buttonPanel.Children.Add(cancelButton);

            // Собираем всё
            innerStackPanel.Children.Add(amountLabel);
            innerStackPanel.Children.Add(amountTextBox);
            innerStackPanel.Children.Add(categoryLabel);
            innerStackPanel.Children.Add(categoryComboBox);
            innerStackPanel.Children.Add(descLabel);
            innerStackPanel.Children.Add(descTextBox);
            innerStackPanel.Children.Add(buttonPanel);

            stackPanel.Children.Add(innerStackPanel);
            dialog.Content = stackPanel;
            dialog.ShowDialog();
        }

        // Диалог добавления дохода
        private void ShowAddIncomeDialog()
        {
            var dialog = new Window
            {
                Title = "💰 Добавить заработок",
                Width = 400,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Owner = this,
                Background = Brushes.White
            };

            var stackPanel = new StackPanel
            {
                Margin = new Thickness(20)
            };

            var innerStackPanel = new StackPanel();

            // Сумма
            var amountLabel = new TextBlock
            {
                Text = "Сумма (₽):",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var amountTextBox = new TextBox
            {
                FontSize = 16,
                Height = 40,
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 20)
            };

            // Источник
            var sourceLabel = new TextBlock
            {
                Text = "Источник дохода:",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var sourceComboBox = new ComboBox
            {
                FontSize = 16,
                Height = 40,
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 20)
            };

            sourceComboBox.Items.Add("💼 Зарплата");
            sourceComboBox.Items.Add("🏠 Аренда");
            sourceComboBox.Items.Add("📈 Инвестиции");
            sourceComboBox.Items.Add("🎁 Подарок");
            sourceComboBox.Items.Add("💼 Фриланс");
            sourceComboBox.SelectedIndex = 0;

            // Регулярный доход
            var recurringCheckBox = new CheckBox
            {
                Content = "📅 Регулярный доход",
                FontSize = 16,
                Margin = new Thickness(0, 10, 0, 20)
            };

            // Кнопки
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var addButton = new Button
            {
                Content = "Добавить",
                Width = 120,
                Height = 40,
                FontSize = 16,
                Background = Brushes.LimeGreen,
                Foreground = Brushes.White
            };

            addButton.Click += (s, e) =>
            {
                if (double.TryParse(amountTextBox.Text, out double amount) && amount > 0)
                {
                    // Здесь будет вызов API
                    currentUser.current_balance += amount;
                    UpdateUI();

                    MessageBox.Show($"Доход {amount:N2}₽ добавлен!\nИсточник: {sourceComboBox.SelectedItem}\n{(recurringCheckBox.IsChecked == true ? "(Регулярный доход)" : "(Разовый доход)")}",
                                    "Успешно!", MessageBoxButton.OK, MessageBoxImage.Information);
                    dialog.Close();
                }
                else
                {
                    MessageBox.Show("Введите корректную сумму!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            var cancelButton = new Button
            {
                Content = "Отмена",
                Width = 120,
                Height = 40,
                FontSize = 16,
                Background = Brushes.Gray,
                Foreground = Brushes.White,
                Margin = new Thickness(10, 0, 0, 0)
            };

            cancelButton.Click += (s, e) => dialog.Close();

            buttonPanel.Children.Add(addButton);
            buttonPanel.Children.Add(cancelButton);

            // Собираем всё
            innerStackPanel.Children.Add(amountLabel);
            innerStackPanel.Children.Add(amountTextBox);
            innerStackPanel.Children.Add(sourceLabel);
            innerStackPanel.Children.Add(sourceComboBox);
            innerStackPanel.Children.Add(recurringCheckBox);
            innerStackPanel.Children.Add(buttonPanel);

            stackPanel.Children.Add(innerStackPanel);
            dialog.Content = stackPanel;
            dialog.ShowDialog();
        }

        // Диалог питомца
        private void ShowPetDialog()
        {
            var dialog = new Window
            {
                Title = "🐹 Мой финансовый питомец",
                Width = 500,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Owner = this,
                Background = new LinearGradientBrush(Colors.LightYellow, Colors.Gold, new Point(0, 0), new Point(1, 1))
            };

            var stackPanel = new StackPanel
            {
                Margin = new Thickness(20),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Питомец
            var petBorder = new Border
            {
                Width = 200,
                Height = 200,
                Background = new RadialGradientBrush(Colors.Yellow, Colors.Gold),
                CornerRadius = new CornerRadius(100),
                BorderBrush = Brushes.Goldenrod,
                BorderThickness = new Thickness(5),
                Margin = new Thickness(0, 0, 0, 30)
            };

            var petContent = new TextBlock
            {
                Text = "💰",
                FontSize = 80,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            petBorder.Child = petContent;

            // Имя питомца
            var petName = new TextBlock
            {
                Text = "Копилко-хомяк",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DarkRed,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            // Статистика
            var statsBorder = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(15),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 0, 30)
            };

            var statsPanel = new StackPanel();

            var statsItems = new[]
            {
                $"🍯 Баланс: {currentUser.current_balance:N2} ₽",
                $"💰 Накоплено: {(currentUser.current_balance * 0.3):N2} ₽",
                $"😊 Настроение: Отличное!",
                $"⚡ Энергия: 100%",
                $"📈 Уровень: Новичок"
            };

            foreach (var stat in statsItems)
            {
                var statText = new TextBlock
                {
                    Text = stat,
                    FontSize = 16,
                    Margin = new Thickness(0, 5, 0, 5)
                };
                statsPanel.Children.Add(statText);
            }

            statsBorder.Child = statsPanel;

            // Кнопка закрытия
            var closeButton = new Button
            {
                Content = "Закрыть",
                Width = 120,
                Height = 40,
                FontSize = 16,
                Background = Brushes.Gray,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            closeButton.Click += (s, e) => dialog.Close();

            // Анимация питомца
            var animation = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 0.9,
                To = 1.1,
                Duration = TimeSpan.FromSeconds(1),
                AutoReverse = true,
                RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever
            };

            petBorder.RenderTransformOrigin = new Point(0.5, 0.5);
            petBorder.RenderTransform = new ScaleTransform();
            petBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            petBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);

            // Собираем всё
            stackPanel.Children.Add(petBorder);
            stackPanel.Children.Add(petName);
            stackPanel.Children.Add(statsBorder);
            stackPanel.Children.Add(closeButton);

            dialog.Content = stackPanel;
            dialog.ShowDialog();
        }

        // Диалог целей
        private void ShowGoalsDialog()
        {
            var dialog = new Window
            {
                Title = "🎯 Мои финансовые цели",
                Width = 500,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.CanResize,
                Owner = this,
                Background = Brushes.White
            };

            var mainPanel = new StackPanel
            {
                Margin = new Thickness(20)
            };

            // Заголовок
            var title = new TextBlock
            {
                Text = "Ваши цели",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DodgerBlue,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            // Список целей
            var goalsPanel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 20)
            };

            var goals = new[]
            {
                new { Name = "💻 Новый ноутбук", Target = 50000.0, Current = 25000.0, Deadline = "15.06.2024" },
                new { Name = "🏝 Отпуск на море", Target = 100000.0, Current = 30000.0, Deadline = "01.08.2024" },
                new { Name = "🚗 Машина мечты", Target = 500000.0, Current = 50000.0, Deadline = "31.12.2024" }
            };

            foreach (var goal in goals)
            {
                var progress = goal.Current / goal.Target;

                var goalCard = new Border
                {
                    Background = Brushes.WhiteSmoke,
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(15),
                    Margin = new Thickness(0, 0, 0, 10),
                    BorderBrush = Brushes.LightGray,
                    BorderThickness = new Thickness(1)
                };

                var cardStack = new StackPanel();

                // Название
                var nameText = new TextBlock
                {
                    Text = goal.Name,
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 5)
                };

                // Прогресс
                var progressText = new TextBlock
                {
                    Text = $"{goal.Current:N0}₽ / {goal.Target:N0}₽",
                    FontSize = 14,
                    Margin = new Thickness(0, 0, 0, 5)
                };

                var progressBar = new ProgressBar
                {
                    Value = progress * 100,
                    Height = 20,
                    Maximum = 100,
                    Foreground = progress > 0.7 ? Brushes.LimeGreen :
                                 progress > 0.4 ? Brushes.Orange : Brushes.DodgerBlue
                };

                var percentageText = new TextBlock
                {
                    Text = $"{progress:P0}",
                    FontSize = 12,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 2, 0, 0)
                };

                // Дедлайн
                var deadlineText = new TextBlock
                {
                    Text = $"📅 До {goal.Deadline}",
                    FontSize = 12,
                    FontStyle = FontStyles.Italic,
                    Margin = new Thickness(0, 5, 0, 0)
                };

                cardStack.Children.Add(nameText);
                cardStack.Children.Add(progressText);
                cardStack.Children.Add(progressBar);
                cardStack.Children.Add(percentageText);
                cardStack.Children.Add(deadlineText);

                goalCard.Child = cardStack;
                goalsPanel.Children.Add(goalCard);
            }

            // Кнопки
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var addGoalButton = new Button
            {
                Content = "➕ Добавить цель",
                Width = 150,
                Height = 40,
                FontSize = 14,
                Background = Brushes.DodgerBlue,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 10, 0)
            };

            addGoalButton.Click += (s, e) =>
            {
                MessageBox.Show("Функция добавления цели будет реализована позже!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            };

            var closeButton = new Button
            {
                Content = "Закрыть",
                Width = 100,
                Height = 40,
                FontSize = 14,
                Background = Brushes.Gray,
                Foreground = Brushes.White
            };

            closeButton.Click += (s, e) => dialog.Close();

            buttonPanel.Children.Add(addGoalButton);
            buttonPanel.Children.Add(closeButton);

            // Собираем всё
            mainPanel.Children.Add(title);
            mainPanel.Children.Add(goalsPanel);
            mainPanel.Children.Add(buttonPanel);

            dialog.Content = mainPanel;
            dialog.ShowDialog();
        }

        // Остальные диалоги
        private void ShowProfileDialog()
        {
            MessageBox.Show($"👤 Профиль\n\nИмя: {currentUser.name}\nID: {currentUser.user_id}\nБаланс: {currentUser.current_balance:N2} ₽",
                            "Ваш профиль", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowSettingsDialog()
        {
            MessageBox.Show("⚙ Настройки\n\nЭта функция будет доступна в следующем обновлении!",
                            "Настройки", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}