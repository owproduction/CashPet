using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Linq;

namespace FinancialTamagotchi
{
    public partial class MainWindow : Window
    {
        private double balance = 15000.50;
        private int foodCurrency = 100; // Игровая валюта (корм)
        private int petEnergy = 80; // Энергия питомца (0-100)
        private string petMood = "Отличное! 😊";
        private List<FinancialGoal> goals = new List<FinancialGoal>();
        private List<Transaction> transactions = new List<Transaction>(); // История транзакций
        private Random random = new Random();

        // Таймер для автоматического уменьшения энергии
        private DispatcherTimer hungerTimer;
        private int secondsWithoutFood = 0;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                UpdateUI();
                SetupButtonEffects();

                // Добавляем тестовые цели
                goals.Add(new FinancialGoal("Новый ноутбук", 50000, 25000, DateTime.Now.AddMonths(3)));
                goals.Add(new FinancialGoal("Отпуск на море", 100000, 30000, DateTime.Now.AddMonths(6)));

                // Добавляем тестовые транзакции для графиков
                AddTestTransactions();

                // Запускаем анимацию питомца
                StartPetAnimation();

                // Запускаем таймер голода
                StartHungerTimer();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка инициализации: " + ex.Message);
            }
        }

        private void AddTestTransactions()
        {
            // Добавляем тестовые данные за последние 30 дней
            var random = new Random();
            var today = DateTime.Today;

            for (int i = 0; i < 30; i++)
            {
                var date = today.AddDays(-i);

                // Случайный доход
                if (random.Next(3) < 2) // 66% шанс дохода
                {
                    transactions.Add(new Transaction
                    {
                        Date = date,
                        Amount = random.Next(1000, 5000),
                        Type = "Income",
                        Category = "Зарплата"
                    });
                }

                // Случайный расход
                if (random.Next(3) < 2) // 66% шанс расхода
                {
                    transactions.Add(new Transaction
                    {
                        Date = date,
                        Amount = random.Next(500, 3000),
                        Type = "Expense",
                        Category = "Продукты"
                    });
                }
            }
        }

        private void StartHungerTimer()
        {
            hungerTimer = new DispatcherTimer();
            hungerTimer.Interval = TimeSpan.FromSeconds(30); // Каждые 30 секунд
            hungerTimer.Tick += HungerTimer_Tick;
            hungerTimer.Start();
        }

        private void HungerTimer_Tick(object sender, EventArgs e)
        {
            // Уменьшаем энергию питомца
            petEnergy = Math.Max(0, petEnergy - 5);
            secondsWithoutFood += 30;

            // Если питомец голоден очень долго
            if (secondsWithoutFood >= 120) // 2 минуты
            {
                petEnergy = Math.Max(0, petEnergy - 10);
            }

            // Обновляем интерфейс
            Dispatcher.Invoke(() =>
            {
                UpdateUI();

                // Показываем предупреждение при низкой энергии
                if (petEnergy <= 20 && petEnergy > 0)
                {
                    MoodText.Text = "Очень голоден! 🥺";
                    PetEmoji.Text = "😢";

                    // Показываем сообщение только если питомец стал сильно голодным
                    if (petEnergy == 20 || petEnergy == 10)
                    {
                        MessageBox.Show($"🥺 {PetNameText.Text} очень голоден! Энергия: {petEnergy}%\nПокормите его скорее!",
                            "Питомец голоден!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else if (petEnergy <= 0)
                {
                    PetEmoji.Text = "😴";
                    MoodText.Text = "Уснул от голода... 😴";
                    PetBorder.Background = new SolidColorBrush(Color.FromRgb(200, 200, 200));

                    MessageBox.Show($"😴 {PetNameText.Text} уснул от голода!\nПокормите его, чтобы разбудить!",
                        "Питомец уснул!", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            });
        }

        private void StartPetAnimation()
        {
            // Простая анимация питомца
            var animation = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 0.95,
                To = 1.05,
                Duration = TimeSpan.FromSeconds(2),
                AutoReverse = true,
                RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever
            };

            PetBorder.RenderTransformOrigin = new Point(0.5, 0.5);
            PetBorder.RenderTransform = new ScaleTransform();
            PetBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            PetBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
        }

        private void SetupButtonEffects()
        {
            var buttons = new[] { ExpenseButton, IncomeButton, GoalsButton, ChartsButton, FeedPetButton };

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

        private void UpdateUI()
        {
            // Обновляем денежный баланс
            BalanceText.Text = $"{balance:N2} ₽";
            PetBalanceText.Text = $"{balance:N0} ₽";

            // Обновляем игровую валюту
            FoodCurrencyText.Text = foodCurrency.ToString();
            FoodText.Text = foodCurrency.ToString();

            // Обновляем состояние питомца
            MoodText.Text = petMood;
            EnergyBar.Value = petEnergy;

            // Обновляем внешний вид питомца в зависимости от энергии
            UpdatePetAppearance();
        }

        private void UpdatePetAppearance()
        {
            if (petEnergy <= 0)
            {
                PetEmoji.Text = "😴"; // Сонный
                PetBorder.Background = new SolidColorBrush(Color.FromRgb(200, 200, 200)); // Серый
                petMood = "Уснул 😴";
            }
            else if (petEnergy <= 20)
            {
                PetEmoji.Text = "😢"; // Грустный
                PetBorder.Background = new SolidColorBrush(Color.FromRgb(255, 235, 156)); // Бледно-желтый
                petMood = "Очень голоден! 🥺";
            }
            else if (petEnergy <= 40)
            {
                PetEmoji.Text = "😕"; // Недовольный
                PetBorder.Background = new SolidColorBrush(Color.FromRgb(255, 224, 102)); // Светло-желтый
                petMood = "Хочет кушать 😐";
            }
            else if (petEnergy <= 60)
            {
                PetEmoji.Text = "😐"; // Нейтральный
                PetBorder.Background = new SolidColorBrush(Color.FromRgb(255, 214, 51)); // Желтый
                petMood = "Нормально 😐";
            }
            else if (petEnergy <= 80)
            {
                PetEmoji.Text = "😊"; // Довольный
                PetBorder.Background = new SolidColorBrush(Color.FromRgb(255, 193, 7)); // Желтый
                petMood = "Хорошо! 😊";
            }
            else
            {
                PetEmoji.Text = "😄"; // Очень довольный
                PetBorder.Background = new SolidColorBrush(Color.FromRgb(255, 193, 7)); // Ярко-желтый
                petMood = "Отлично! 😄";
            }
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

        private void ChartsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowChartsDialog();
        }

        private void FeedPetButton_Click(object sender, RoutedEventArgs e)
        {
            FeedPet();
        }

        private void FeedPet()
        {
            if (foodCurrency >= 10)
            {
                foodCurrency -= 10;

                // Восстанавливаем энергию
                int oldEnergy = petEnergy;
                petEnergy = Math.Min(100, petEnergy + 20);

                // Сбрасываем счётчик голода
                secondsWithoutFood = 0;

                // Если питомец спал, просыпается
                if (oldEnergy <= 0)
                {
                    MessageBox.Show($"{PetNameText.Text} проснулся и радостно ест! 🎉",
                        "Питомец проснулся!", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Случайный бонус
                if (random.Next(100) < 30) // 30% шанс
                {
                    int bonus = random.Next(5, 15);
                    foodCurrency += bonus;
                    MessageBox.Show($"Питомец нашел {bonus}🥕 во время еды!", "Бонус!",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                UpdateUI();

                // Анимация кормления
                var animation = new System.Windows.Media.Animation.DoubleAnimation
                {
                    From = 1.2,
                    To = 1.0,
                    Duration = TimeSpan.FromSeconds(0.3)
                };
                PetBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                PetBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);

                MessageBox.Show($"Питомец покормлен! +20⚡\nЭнергия: {petEnergy}%", "Кормление",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Недостаточно корма! Заработайте его, добавляя доходы.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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

                    // Добавляем транзакцию
                    transactions.Add(new Transaction
                    {
                        Date = DateTime.Today,
                        Amount = amount,
                        Type = "Expense",
                        Category = categoryBox.SelectedItem.ToString()
                    });

                    // Питомец теряет энергию при тратах
                    petEnergy = Math.Max(0, petEnergy - 5);

                    UpdateUI();

                    MessageBox.Show($"Трата на {amount:N2}₽ добавлена!\nКатегория: {categoryBox.SelectedItem}\n\nПитомец немного расстроился от траты... -5⚡",
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

                    // Награда за доход: фиксированные +10 корма
                    int foodReward = 10;
                    foodCurrency += foodReward;

                    // Добавляем транзакцию
                    transactions.Add(new Transaction
                    {
                        Date = DateTime.Today,
                        Amount = amount,
                        Type = "Income",
                        Category = sourceBox.SelectedItem.ToString()
                    });

                    // Питомец радуется доходу
                    petEnergy = Math.Min(100, petEnergy + 10);

                    UpdateUI();

                    MessageBox.Show($"Доход {amount:N2}₽ добавлен!\nИсточник: {sourceBox.SelectedItem}\n\n🎉 Награда: +{foodReward}🥕 (игровая валюта)\nПитомец радуется! +10⚡",
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

        private void ShowChartsDialog()
        {
            var dialog = new Window
            {
                Title = "📊 Графики доходов и расходов",
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
                Text = "Финансовая статистика",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DodgerBlue,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            mainPanel.Children.Add(title);

            // Сводка
            var summaryPanel = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(240, 248, 255)),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(15, 15, 15, 15),
                Margin = new Thickness(0, 0, 0, 20),
                BorderBrush = Brushes.LightBlue,
                BorderThickness = new Thickness(1, 1, 1, 1)
            };

            var summaryStack = new StackPanel();

            var totalIncome = transactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
            var totalExpense = transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);
            var balance = totalIncome - totalExpense;

            summaryStack.Children.Add(new TextBlock
            {
                Text = "💰 Общая статистика:",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DodgerBlue,
                Margin = new Thickness(0, 0, 0, 10)
            });

            summaryStack.Children.Add(new TextBlock
            {
                Text = $"Всего доходов: {totalIncome:N2} ₽",
                FontSize = 14,
                Margin = new Thickness(10, 0, 0, 5)
            });

            summaryStack.Children.Add(new TextBlock
            {
                Text = $"Всего расходов: {totalExpense:N2} ₽",
                FontSize = 14,
                Margin = new Thickness(10, 0, 0, 5)
            });

            summaryStack.Children.Add(new TextBlock
            {
                Text = $"Итоговый баланс: {balance:N2} ₽",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = balance >= 0 ? Brushes.Green : Brushes.Red,
                Margin = new Thickness(10, 5, 0, 0)
            });

            summaryPanel.Child = summaryStack;
            mainPanel.Children.Add(summaryPanel);

            // График по дням (последние 7 дней)
            var dailyTitle = new TextBlock
            {
                Text = "📅 Доходы и расходы по дням (последние 7 дней):",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DodgerBlue,
                Margin = new Thickness(0, 0, 0, 10)
            };
            mainPanel.Children.Add(dailyTitle);

            var today = DateTime.Today;
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var dayIncome = transactions.Where(t => t.Type == "Income" && t.Date.Date == date).Sum(t => t.Amount);
                var dayExpense = transactions.Where(t => t.Type == "Expense" && t.Date.Date == date).Sum(t => t.Amount);

                var dayPanel = CreateDayChart(date.ToString("dd.MM"), dayIncome, dayExpense);
                mainPanel.Children.Add(dayPanel);
            }

            // График по категориям
            var categoryTitle = new TextBlock
            {
                Text = "📊 Расходы по категориям:",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DodgerBlue,
                Margin = new Thickness(0, 20, 0, 10)
            };
            mainPanel.Children.Add(categoryTitle);

            var expensesByCategory = transactions
                .Where(t => t.Type == "Expense")
                .GroupBy(t => t.Category)
                .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) })
                .OrderByDescending(x => x.Total);

            if (!expensesByCategory.Any())
            {
                mainPanel.Children.Add(new TextBlock
                {
                    Text = "Нет данных о расходах",
                    FontSize = 14,
                    FontStyle = FontStyles.Italic,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(10, 0, 0, 10)
                });
            }
            else
            {
                double maxExpense = expensesByCategory.Max(x => x.Total);
                foreach (var item in expensesByCategory)
                {
                    var categoryPanel = CreateCategoryChart(item.Category, item.Total, maxExpense);
                    mainPanel.Children.Add(categoryPanel);
                }
            }

            // Кнопка закрытия
            var closeButton = CreateButton("Закрыть", Brushes.Gray);
            closeButton.HorizontalAlignment = HorizontalAlignment.Center;
            closeButton.Margin = new Thickness(0, 20, 0, 0);
            closeButton.Click += (s, e) => dialog.Close();
            mainPanel.Children.Add(closeButton);

            dialog.Content = new ScrollViewer { Content = mainPanel };
            dialog.ShowDialog();
        }

        private Border CreateDayChart(string day, double income, double expense)
        {
            var border = new Border
            {
                Background = Brushes.WhiteSmoke,
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(10, 5, 10, 5),
                Margin = new Thickness(0, 0, 0, 5),
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1, 1, 1, 1)
            };

            var stack = new StackPanel();

            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
            headerPanel.Children.Add(new TextBlock
            {
                Text = day,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Width = 80
            });

            headerPanel.Children.Add(new TextBlock
            {
                Text = $"Доход: {income:N0}₽",
                FontSize = 12,
                Foreground = Brushes.Green,
                Width = 100
            });

            headerPanel.Children.Add(new TextBlock
            {
                Text = $"Расход: {expense:N0}₽",
                FontSize = 12,
                Foreground = Brushes.Red,
                Width = 100
            });

            stack.Children.Add(headerPanel);

            // Визуальное представление
            var barPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 0) };

            if (income > 0)
            {
                var incomeBar = new Border
                {
                    Background = Brushes.LimeGreen,
                    Width = Math.Min(200, income / 50),
                    Height = 10,
                    Margin = new Thickness(0, 0, 2, 0)
                };
                barPanel.Children.Add(incomeBar);
            }

            if (expense > 0)
            {
                var expenseBar = new Border
                {
                    Background = Brushes.OrangeRed,
                    Width = Math.Min(200, expense / 50),
                    Height = 10
                };
                barPanel.Children.Add(expenseBar);
            }

            stack.Children.Add(barPanel);
            border.Child = stack;
            return border;
        }

        private Border CreateCategoryChart(string category, double amount, double maxAmount)
        {
            var border = new Border
            {
                Background = Brushes.WhiteSmoke,
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(10, 5, 10, 5),
                Margin = new Thickness(0, 0, 0, 5)
            };

            var stack = new StackPanel();

            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
            headerPanel.Children.Add(new TextBlock
            {
                Text = category,
                FontSize = 14,
                Width = 120
            });

            headerPanel.Children.Add(new TextBlock
            {
                Text = $"{amount:N0}₽",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DodgerBlue
            });

            stack.Children.Add(headerPanel);

            var bar = new Border
            {
                Background = Brushes.DodgerBlue,
                Width = (amount / maxAmount) * 300,
                Height = 10,
                Margin = new Thickness(0, 2, 0, 0)
            };
            stack.Children.Add(bar);

            border.Child = stack;
            return border;
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

            // Информация о наградах
            var rewardInfo = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(255, 248, 225)),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(15, 15, 15, 15),
                Margin = new Thickness(0, 0, 0, 20),
                BorderBrush = Brushes.Orange,
                BorderThickness = new Thickness(1, 1, 1, 1)
            };

            var rewardStack = new StackPanel();
            rewardStack.Children.Add(new TextBlock
            {
                Text = "🎁 Награды за выполнение целей:",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Orange,
                Margin = new Thickness(0, 0, 0, 5)
            });

            rewardStack.Children.Add(new TextBlock
            {
                Text = "• За выполнение цели: 50-100🥕",
                FontSize = 12,
                Margin = new Thickness(10, 0, 0, 2)
            });

            rewardStack.Children.Add(new TextBlock
            {
                Text = "• Чем больше цель, тем больше награда!",
                FontSize = 12,
                Margin = new Thickness(10, 0, 0, 0)
            });

            rewardInfo.Child = rewardStack;
            mainPanel.Children.Add(rewardInfo);

            // Панель целей
            var goalsScroll = new ScrollViewer
            {
                Height = 350,
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

            // Кнопка получения награды
            if (goal.IsCompleted && !goal.RewardClaimed)
            {
                var claimButton = CreateSmallButton("🎁 Получить награду", Brushes.Orange);
                claimButton.Click += (s, e) =>
                {
                    foodCurrency += goal.RewardAmount;
                    goal.RewardClaimed = true;
                    UpdateUI();
                    MessageBox.Show($"Получено {goal.RewardAmount}🥕 за выполнение цели '{goal.Name}'!",
                        "Награда получена!", MessageBoxButton.OK, MessageBoxImage.Information);
                    ShowGoalsDialog(); // Обновляем диалог
                };
                stack.Children.Add(claimButton);
            }

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

                // Рассчитываем награду в зависимости от размера цели
                int reward = (int)(target / 1000); // 1 корм за каждые 1000 рублей цели
                if (reward < 50) reward = 50; // Минимум 50 корма
                if (reward > 100) reward = 100; // Максимум 100 корма

                var newGoal = new FinancialGoal(
                    nameBox.Text,
                    target,
                    current,
                    datePicker.SelectedDate.Value,
                    reward
                );

                goals.Add(newGoal);
                MessageBox.Show($"Цель '{nameBox.Text}' добавлена!\n\nНаграда за выполнение: {reward}🥕",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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

                    // Маленькая награда за пополнение цели
                    if (random.Next(100) < 20) // 20% шанс
                    {
                        int bonus = random.Next(1, 5);
                        foodCurrency += bonus;
                        MessageBox.Show($"Цель '{selectedGoal.Name}' пополнена на {amount:N2}₽!\n\nБонус: +{bonus}🥕 за активность!",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Цель '{selectedGoal.Name}' пополнена на {amount:N2}₽!",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    UpdateUI();
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

        private Button CreateSmallButton(string content, Brush background)
        {
            var button = new Button
            {
                Content = content,
                Width = 150,
                Height = 30,
                FontSize = 12,
                Background = background,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 5, 0, 0)
            };

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
        public int RewardAmount { get; set; }
        public bool RewardClaimed { get; set; }
        public bool IsCompleted => CurrentAmount >= TargetAmount;

        public FinancialGoal(string name, double targetAmount, double currentAmount, DateTime deadline, int rewardAmount = 50)
        {
            Name = name;
            TargetAmount = targetAmount;
            CurrentAmount = currentAmount;
            Deadline = deadline;
            RewardAmount = rewardAmount;
            RewardClaimed = false;
        }
    }

    public class Transaction
    {
        public DateTime Date { get; set; }
        public double Amount { get; set; }
        public string Type { get; set; } // "Income" или "Expense"
        public string Category { get; set; }
    }
}