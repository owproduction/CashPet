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
        private int foodCurrency = 100; // Игровая валюта (корм)
        private int petEnergy = 80; // Энергия питомца (0-100)
        private string petMood = "Отличное! 😊";
        private List<FinancialGoal> goals = new List<FinancialGoal>();
        private Random random = new Random();

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

                // Запускаем анимацию питомца
                StartPetAnimation();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка инициализации: " + ex.Message);
            }
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
            var buttons = new[] { ExpenseButton, IncomeButton, GoalsButton, FoodShopButton, FeedPetButton };

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
            if (petEnergy <= 20)
            {
                PetEmoji.Text = "😴"; // Сонный
                PetBorder.Background = new SolidColorBrush(Color.FromRgb(255, 235, 156)); // Бледно-желтый
                petMood = "Устал... 😴";
            }
            else if (petEnergy <= 50)
            {
                PetEmoji.Text = "😐"; // Нейтральный
                PetBorder.Background = new SolidColorBrush(Color.FromRgb(255, 224, 102)); // Светло-желтый
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

        private void FoodShopButton_Click(object sender, RoutedEventArgs e)
        {
            ShowShopDialog();
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
                petEnergy = Math.Min(100, petEnergy + 20);

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

                MessageBox.Show("Питомец покормлен! +20⚡", "Кормление",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Недостаточно корма! Купите больше в магазине.", "Ошибка",
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

                    // Награда за доход: игровая валюта
                    int foodReward = (int)(amount / 1000); // 1 корм за каждые 1000 рублей
                    if (foodReward < 1) foodReward = 1;
                    if (foodReward > 50) foodReward = 50;

                    foodCurrency += foodReward;

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

            // Награда за выполнение
            if (goal.IsCompleted && !goal.RewardClaimed)
            {
                var rewardText = new TextBlock
                {
                    Text = $" 🎁 +{goal.RewardAmount}🥕",
                    FontSize = 14,
                    Margin = new Thickness(10, 0, 0, 0),
                    Foreground = Brushes.Orange,
                    FontWeight = FontWeights.Bold
                };
                headerPanel.Children.Add(rewardText);
            }

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

        private void ShowShopDialog()
        {
            var dialog = new Window
            {
                Title = "🛒 Магазин корма",
                Width = 500,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = Brushes.White
            };

            var mainPanel = new StackPanel { Margin = new Thickness(20, 20, 20, 20) };

            // Заголовок
            var title = new TextBlock
            {
                Text = "Купить корм для питомца",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Purple,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            mainPanel.Children.Add(title);

            // Товары
            var products = new[]
            {
                new { Name = "🥕 Маленький пакет", Amount = 10, Price = 100.0, Description = "10 единиц корма" },
                new { Name = "🥕🥕 Средний пакет", Amount = 25, Price = 225.0, Description = "25 единиц корма (скидка 10%)" },
                new { Name = "🥕🥕🥕 Большой пакет", Amount = 50, Price = 400.0, Description = "50 единиц корма (скидка 20%)" },
                new { Name = "🎁 Сюрприз-пакет", Amount = 15, Price = 120.0, Description = "15 корма + случайный бонус!" }
            };

            foreach (var product in products)
            {
                var productCard = new Border
                {
                    Background = Brushes.WhiteSmoke,
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(15, 15, 15, 15),
                    Margin = new Thickness(0, 0, 0, 10),
                    BorderBrush = Brushes.LightGray,
                    BorderThickness = new Thickness(1, 1, 1, 1)
                };

                var stack = new StackPanel();

                // Название и цена
                var headerPanel = new StackPanel();
                headerPanel.Orientation = Orientation.Horizontal;

                headerPanel.Children.Add(new TextBlock
                {
                    Text = product.Name,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Purple,
                    VerticalAlignment = VerticalAlignment.Center
                });

                headerPanel.Children.Add(new TextBlock
                {
                    Text = $" - {product.Price}₽",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Green,
                    Margin = new Thickness(10, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center
                });

                stack.Children.Add(headerPanel);

                // Описание
                stack.Children.Add(new TextBlock
                {
                    Text = product.Description,
                    FontSize = 12,
                    Margin = new Thickness(0, 5, 0, 10),
                    Foreground = Brushes.Gray
                });

                // Кнопка покупки
                var buyButton = CreateSmallButton($"Купить ({product.Amount}🥕)", Brushes.Purple);
                buyButton.Click += (s, e) =>
                {
                    if (balance >= product.Price)
                    {
                        balance -= product.Price;
                        foodCurrency += product.Amount;

                        // Бонус для сюрприз-пакета
                        if (product.Name.Contains("Сюрприз"))
                        {
                            int bonus = random.Next(5, 20);
                            foodCurrency += bonus;
                            MessageBox.Show($"Вы купили {product.Name}!\nПолучено: {product.Amount}🥕\nБонус: +{bonus}🥕\nИтого: {product.Amount + bonus}🥕",
                                "Покупка + бонус!", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show($"Вы купили {product.Name}!\nПолучено: {product.Amount}🥕",
                                "Покупка совершена!", MessageBoxButton.OK, MessageBoxImage.Information);
                        }

                        UpdateUI();
                    }
                    else
                    {
                        MessageBox.Show("Недостаточно средств для покупки!", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                };

                stack.Children.Add(buyButton);
                productCard.Child = stack;
                mainPanel.Children.Add(productCard);
            }

            // Кнопка закрытия
            var closeButton = CreateButton("Закрыть", Brushes.Gray);
            closeButton.HorizontalAlignment = HorizontalAlignment.Center;
            closeButton.Click += (s, e) => dialog.Close();

            mainPanel.Children.Add(closeButton);
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
}