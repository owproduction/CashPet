using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FinancialTamagotchi
{
    public partial class MainWindow : Window
    {
        private double balance = 15000.50;
        private List<FinancialGoal> goals = new List<FinancialGoal>();

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                UpdateBalance();
                SetupButtonEffects();

                // Добавляем тестовые цели
                goals.Add(new FinancialGoal("Новый ноутбук", 50000, 25000, DateTime.Now.AddMonths(3)));
                goals.Add(new FinancialGoal("Отпуск на море", 100000, 30000, DateTime.Now.AddMonths(6)));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка инициализации: " + ex.Message);
            }
        }

        private void SetupButtonEffects()
        {
            var buttons = new[] { ExpenseButton, IncomeButton, GoalsButton, SettingsButton };

            foreach (var button in buttons)
            {
                // Эффект при наведении
                button.MouseEnter += (s, e) =>
                {
                    var scale = new ScaleTransform(1.05, 1.05);
                    button.RenderTransformOrigin = new Point(0.5, 0.5);
                    button.RenderTransform = scale;
                };

                button.MouseLeave += (s, e) =>
                {
                    button.RenderTransform = null;
                };
            }
        }

        private void UpdateBalance()
        {
            BalanceText.Text = $"{balance:N2} ₽";
            PetBalanceText.Text = $"{balance:N0} ₽";
        }

        private void ExpenseButton_Click(object sender, RoutedEventArgs e)
        {
            ShowAddExpenseDialog();
        }

        private void IncomeButton_Click(object sender, RoutedEventArgs e)
        {
            ShowAddIncomeDialog();
        }

        private void GoalsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowGoalsDialog();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("⚙️ Настройки\n\nЭта функция будет доступна в следующем обновлении!",
                "Настройки", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowAddExpenseDialog()
        {
            var dialog = new Window
            {
                Title = "💸 Добавить трату",
                Width = 400,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = Brushes.White
            };

            var mainPanel = new StackPanel { Margin = new Thickness(20, 20, 20, 20) };

            // Сумма
            mainPanel.Children.Add(CreateLabel("Сумма (₽):"));

            var amountBox = new TextBox
            {
                FontSize = 16,
                Height = 40,
                Padding = new Thickness(10, 10, 10, 10),
                Margin = new Thickness(0, 0, 0, 20)
            };
            mainPanel.Children.Add(amountBox);

            // Категория
            mainPanel.Children.Add(CreateLabel("Категория:"));

            var categoryBox = new ComboBox
            {
                FontSize = 16,
                Height = 40,
                Padding = new Thickness(10, 10, 10, 10),
                Margin = new Thickness(0, 0, 0, 20)
            };

            categoryBox.Items.Add("🍔 Продукты");
            categoryBox.Items.Add("🚌 Транспорт");
            categoryBox.Items.Add("🏠 Жильё");
            categoryBox.Items.Add("👕 Одежда");
            categoryBox.Items.Add("💊 Здоровье");
            categoryBox.Items.Add("🎬 Развлечения");
            categoryBox.Items.Add("📚 Образование");
            categoryBox.SelectedIndex = 0;

            mainPanel.Children.Add(categoryBox);

            // Кнопки
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var addButton = CreateButton("Добавить", Brushes.OrangeRed);
            addButton.Click += (s, e) =>
            {
                if (double.TryParse(amountBox.Text, out double amount) && amount > 0)
                {
                    if (amount > balance)
                    {
                        MessageBox.Show("Недостаточно средств!", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    balance -= amount;
                    UpdateBalance();

                    MessageBox.Show($"Трата на {amount:N2}₽ добавлена!\nКатегория: {categoryBox.SelectedItem}",
                        "Успешно!", MessageBoxButton.OK, MessageBoxImage.Information);
                    dialog.Close();
                }
                else
                {
                    MessageBox.Show("Введите корректную сумму!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            var cancelButton = CreateButton("Отмена", Brushes.Gray);
            cancelButton.Click += (s, e) => dialog.Close();

            buttonPanel.Children.Add(addButton);
            buttonPanel.Children.Add(cancelButton);
            mainPanel.Children.Add(buttonPanel);

            dialog.Content = mainPanel;
            dialog.ShowDialog();
        }

        private void ShowAddIncomeDialog()
        {
            var dialog = new Window
            {
                Title = "💰 Добавить доход",
                Width = 400,
                Height = 350,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = Brushes.White
            };

            var mainPanel = new StackPanel { Margin = new Thickness(20, 20, 20, 20) };

            // Сумма
            mainPanel.Children.Add(CreateLabel("Сумма (₽):"));

            var amountBox = new TextBox
            {
                FontSize = 16,
                Height = 40,
                Padding = new Thickness(10, 10, 10, 10),
                Margin = new Thickness(0, 0, 0, 20)
            };
            mainPanel.Children.Add(amountBox);

            // Источник
            mainPanel.Children.Add(CreateLabel("Источник:"));

            var sourceBox = new ComboBox
            {
                FontSize = 16,
                Height = 40,
                Padding = new Thickness(10, 10, 10, 10),
                Margin = new Thickness(0, 0, 0, 20)
            };

            sourceBox.Items.Add("💼 Зарплата");
            sourceBox.Items.Add("🏠 Аренда");
            sourceBox.Items.Add("📈 Инвестиции");
            sourceBox.Items.Add("🎁 Подарок");
            sourceBox.Items.Add("💻 Фриланс");
            sourceBox.Items.Add("🏆 Премия");
            sourceBox.SelectedIndex = 0;

            mainPanel.Children.Add(sourceBox);

            // Кнопки
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var addButton = CreateButton("Добавить", Brushes.LimeGreen);
            addButton.Click += (s, e) =>
            {
                if (double.TryParse(amountBox.Text, out double amount) && amount > 0)
                {
                    balance += amount;
                    UpdateBalance();

                    MessageBox.Show($"Доход {amount:N2}₽ добавлен!\nИсточник: {sourceBox.SelectedItem}",
                        "Успешно!", MessageBoxButton.OK, MessageBoxImage.Information);
                    dialog.Close();
                }
                else
                {
                    MessageBox.Show("Введите корректную сумму!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            var cancelButton = CreateButton("Отмена", Brushes.Gray);
            cancelButton.Click += (s, e) => dialog.Close();

            buttonPanel.Children.Add(addButton);
            buttonPanel.Children.Add(cancelButton);
            mainPanel.Children.Add(buttonPanel);

            dialog.Content = mainPanel;
            dialog.ShowDialog();
        }

        private void ShowGoalsDialog()
        {
            var dialog = new Window
            {
                Title = "🎯 Мои финансовые цели",
                Width = 600,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = Brushes.White
            };

            var mainPanel = new StackPanel { Margin = new Thickness(20, 20, 20, 20) };

            // Заголовок
            var title = new TextBlock
            {
                Text = "Ваши финансовые цели",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DodgerBlue,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            mainPanel.Children.Add(title);

            // Панель целей
            var goalsScroll = new ScrollViewer
            {
                Height = 400,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var goalsPanel = new StackPanel();

            if (goals.Count == 0)
            {
                goalsPanel.Children.Add(new TextBlock
                {
                    Text = "У вас пока нет целей. Добавьте первую!",
                    FontSize = 16,
                    FontStyle = FontStyles.Italic,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 20)
                });
            }
            else
            {
                foreach (var goal in goals)
                {
                    goalsPanel.Children.Add(CreateGoalCard(goal));
                }
            }

            goalsScroll.Content = goalsPanel;
            mainPanel.Children.Add(goalsScroll);

            // Кнопки
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var addGoalButton = CreateButton("➕ Добавить цель", Brushes.DodgerBlue);
            addGoalButton.Click += (s, e) =>
            {
                dialog.Close();
                ShowAddGoalDialog();
            };

            var addMoneyButton = CreateButton("💰 Пополнить цель", Brushes.LimeGreen);
            addMoneyButton.Click += (s, e) =>
            {
                if (goals.Count > 0)
                {
                    ShowAddMoneyToGoalDialog();
                }
                else
                {
                    MessageBox.Show("Сначала добавьте цель!", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            };

            var closeButton = CreateButton("Закрыть", Brushes.Gray);
            closeButton.Click += (s, e) => dialog.Close();

            buttonPanel.Children.Add(addGoalButton);
            buttonPanel.Children.Add(addMoneyButton);
            buttonPanel.Children.Add(closeButton);

            mainPanel.Children.Add(buttonPanel);
            dialog.Content = mainPanel;
            dialog.ShowDialog();
        }

        private Border CreateGoalCard(FinancialGoal goal)
        {
            var progress = goal.CurrentAmount / goal.TargetAmount;
            var daysLeft = (goal.Deadline - DateTime.Now).Days;

            var card = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(15, 15, 15, 15),
                Margin = new Thickness(0, 0, 0, 10),
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1, 1, 1, 1)
            };

            var stack = new StackPanel();

            // Название и статус
            var headerPanel = new StackPanel();
            headerPanel.Orientation = Orientation.Horizontal;

            var nameText = new TextBlock
            {
                Text = goal.Name,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = goal.IsCompleted ? Brushes.LimeGreen : Brushes.DodgerBlue
            };

            var statusText = new TextBlock
            {
                Text = goal.IsCompleted ? " ✓ Выполнено" : $" ⌛ {daysLeft} дней",
                FontSize = 14,
                Margin = new Thickness(10, 0, 0, 0),
                Foreground = goal.IsCompleted ? Brushes.LimeGreen : Brushes.Orange
            };

            headerPanel.Children.Add(nameText);
            headerPanel.Children.Add(statusText);
            stack.Children.Add(headerPanel);

            // Прогресс
            stack.Children.Add(new TextBlock
            {
                Text = $"{goal.CurrentAmount:N0}₽ / {goal.TargetAmount:N0}₽",
                FontSize = 16,
                Margin = new Thickness(0, 10, 0, 5)
            });

            var progressBar = new ProgressBar
            {
                Value = progress * 100,
                Height = 20,
                Maximum = 100,
                Foreground = progress >= 1 ? Brushes.LimeGreen :
                            progress > 0.7 ? Brushes.DodgerBlue :
                            progress > 0.4 ? Brushes.Orange : Brushes.OrangeRed
            };
            stack.Children.Add(progressBar);

            stack.Children.Add(new TextBlock
            {
                Text = $"{progress:P0}",
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 2, 0, 5)
            });

            // Дедлайн
            stack.Children.Add(new TextBlock
            {
                Text = $"📅 До {goal.Deadline:dd.MM.yyyy}",
                FontSize = 12,
                FontStyle = FontStyles.Italic,
                Foreground = Brushes.Gray
            });

            card.Child = stack;
            return card;
        }

        private void ShowAddGoalDialog()
        {
            var dialog = new Window
            {
                Title = "🎯 Добавить новую цель",
                Width = 400,
                Height = 450,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = Brushes.White
            };

            var mainPanel = new StackPanel { Margin = new Thickness(20, 20, 20, 20) };

            // Название цели
            mainPanel.Children.Add(CreateLabel("Название цели:"));

            var nameBox = new TextBox
            {
                FontSize = 16,
                Height = 40,
                Padding = new Thickness(10, 10, 10, 10),
                Margin = new Thickness(0, 0, 0, 20)
            };
            mainPanel.Children.Add(nameBox);

            // Целевая сумма
            mainPanel.Children.Add(CreateLabel("Целевая сумма (₽):"));

            var targetBox = new TextBox
            {
                FontSize = 16,
                Height = 40,
                Padding = new Thickness(10, 10, 10, 10),
                Margin = new Thickness(0, 0, 0, 20)
            };
            mainPanel.Children.Add(targetBox);

            // Текущая сумма
            mainPanel.Children.Add(CreateLabel("Текущая сумма (₽, опционально):"));

            var currentBox = new TextBox
            {
                FontSize = 16,
                Height = 40,
                Padding = new Thickness(10, 10, 10, 10),
                Margin = new Thickness(0, 0, 0, 20),
                Text = "0"
            };
            mainPanel.Children.Add(currentBox);

            // Дедлайн
            mainPanel.Children.Add(CreateLabel("Дедлайн:"));

            var datePicker = new DatePicker
            {
                FontSize = 16,
                Height = 40,
                Margin = new Thickness(0, 0, 0, 20),
                SelectedDate = DateTime.Now.AddMonths(3)
            };
            mainPanel.Children.Add(datePicker);

            // Кнопки
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var addButton = CreateButton("Добавить цель", Brushes.DodgerBlue);
            addButton.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(nameBox.Text))
                {
                    MessageBox.Show("Введите название цели!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!double.TryParse(targetBox.Text, out double target) || target <= 0)
                {
                    MessageBox.Show("Введите корректную целевую сумму!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!double.TryParse(currentBox.Text, out double current) || current < 0)
                {
                    current = 0;
                }

                if (current > target)
                {
                    MessageBox.Show("Текущая сумма не может превышать целевую!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!datePicker.SelectedDate.HasValue)
                {
                    MessageBox.Show("Выберите дату дедлайна!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var newGoal = new FinancialGoal(
                    nameBox.Text,
                    target,
                    current,
                    datePicker.SelectedDate.Value
                );

                goals.Add(newGoal);
                MessageBox.Show($"Цель '{nameBox.Text}' добавлена!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                dialog.Close();
                ShowGoalsDialog();
            };

            var cancelButton = CreateButton("Отмена", Brushes.Gray);
            cancelButton.Click += (s, e) => dialog.Close();

            buttonPanel.Children.Add(addButton);
            buttonPanel.Children.Add(cancelButton);
            mainPanel.Children.Add(buttonPanel);

            dialog.Content = mainPanel;
            dialog.ShowDialog();
        }

        private void ShowAddMoneyToGoalDialog()
        {
            var dialog = new Window
            {
                Title = "💰 Пополнить цель",
                Width = 400,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = Brushes.White
            };

            var mainPanel = new StackPanel { Margin = new Thickness(20, 20, 20, 20) };

            // Выбор цели
            mainPanel.Children.Add(CreateLabel("Выберите цель:"));

            var goalsCombo = new ComboBox
            {
                FontSize = 16,
                Height = 40,
                Padding = new Thickness(10, 10, 10, 10),
                Margin = new Thickness(0, 0, 0, 20)
            };

            foreach (var goal in goals)
            {
                if (!goal.IsCompleted)
                {
                    goalsCombo.Items.Add(goal);
                }
            }

            if (goalsCombo.Items.Count == 0)
            {
                MessageBox.Show("Все цели уже выполнены или нет активных целей!", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                dialog.Close();
                return;
            }

            goalsCombo.SelectedIndex = 0;

            // Используем DisplayMemberPath для отображения имени
            goalsCombo.DisplayMemberPath = "Name";
            mainPanel.Children.Add(goalsCombo);

            // Сумма пополнения
            mainPanel.Children.Add(CreateLabel("Сумма пополнения (₽):"));

            var amountBox = new TextBox
            {
                FontSize = 16,
                Height = 40,
                Padding = new Thickness(10, 10, 10, 10),
                Margin = new Thickness(0, 0, 0, 20)
            };
            mainPanel.Children.Add(amountBox);

            // Кнопки
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var addButton = CreateButton("Пополнить", Brushes.LimeGreen);
            addButton.Click += (s, e) =>
            {
                if (!double.TryParse(amountBox.Text, out double amount) || amount <= 0)
                {
                    MessageBox.Show("Введите корректную сумму!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (amount > balance)
                {
                    MessageBox.Show("Недостаточно средств на балансе!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var selectedGoal = goalsCombo.SelectedItem as FinancialGoal;
                if (selectedGoal != null)
                {
                    selectedGoal.CurrentAmount += amount;
                    balance -= amount;
                    UpdateBalance();

                    MessageBox.Show($"Цель '{selectedGoal.Name}' пополнена на {amount:N2}₽!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    dialog.Close();
                    ShowGoalsDialog();
                }
            };

            var cancelButton = CreateButton("Отмена", Brushes.Gray);
            cancelButton.Click += (s, e) => dialog.Close();

            buttonPanel.Children.Add(addButton);
            buttonPanel.Children.Add(cancelButton);
            mainPanel.Children.Add(buttonPanel);

            dialog.Content = mainPanel;
            dialog.ShowDialog();
        }

        // Вспомогательные методы
        private TextBlock CreateLabel(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            };
        }

        private Button CreateButton(string content, Brush background)
        {
            var button = new Button
            {
                Content = content,
                Width = 120,
                Height = 40,
                FontSize = 14,
                Background = background,
                Foreground = Brushes.White,
                Margin = new Thickness(5, 5, 5, 5)
            };

            // Простой стиль без сложного TemplateBinding
            button.Template = new ControlTemplate(typeof(Button))
            {
                VisualTree = GetButtonTemplate()
            };

            return button;
        }

        private FrameworkElementFactory GetButtonTemplate()
        {
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(5));
            borderFactory.SetValue(Border.BackgroundProperty, new System.Windows.Data.Binding("Background")
            {
                RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent)
            });

            var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            borderFactory.AppendChild(contentPresenter);
            return borderFactory;
        }
    }

    public class FinancialGoal
    {
        public string Name { get; set; }
        public double TargetAmount { get; set; }
        public double CurrentAmount { get; set; }
        public DateTime Deadline { get; set; }
        public bool IsCompleted => CurrentAmount >= TargetAmount;

        public FinancialGoal(string name, double targetAmount, double currentAmount, DateTime deadline)
        {
            Name = name;
            TargetAmount = targetAmount;
            CurrentAmount = currentAmount;
            Deadline = deadline;
        }
    }
}