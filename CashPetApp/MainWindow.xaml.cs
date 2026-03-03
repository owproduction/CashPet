using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Linq;

namespace FinancialTamagotchi
{
    public partial class MainWindow : Window
    {
        // Текущий пользователь
        private User currentUser;
        private List<FinancialGoal> goals = new List<FinancialGoal>();
        private List<Transaction> transactions = new List<Transaction>();
        private Random random = new Random();

        // HTTP клиент для API
        private HttpClient client = new HttpClient();
        private DispatcherTimer hungerTimer;
        private int secondsWithoutFood = 0;

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                // Настройка HTTP клиента
                client.BaseAddress = new Uri("http://localhost:8000");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                // Показываем окно входа после загрузки окна
                this.Loaded += (s, e) =>
                {
                    ShowLoginDialog();
                };

                SetupButtonEffects();
                StartPetAnimation();
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Ошибка инициализации", ex.Message);
            }
        }

        // Метод для показа ошибок в красивом диалоге
        private void ShowErrorDialog(string title, string message)
        {
            var dialog = new Window
            {
                Title = title,
                Width = 350,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = Brushes.White,
                WindowStyle = WindowStyle.SingleBorderWindow
            };

            var mainPanel = new StackPanel { Margin = new Thickness(20, 20, 20, 20) };

            // Иконка ошибки
            mainPanel.Children.Add(new TextBlock
            {
                Text = "❌",
                FontSize = 48,
                Foreground = Brushes.Red,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            });

            // Заголовок
            mainPanel.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Red,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            });

            // Сообщение об ошибке
            mainPanel.Children.Add(new TextBlock
            {
                Text = message,
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            });

            // Кнопка закрытия
            var closeButton = new Button
            {
                Content = "OK",
                Width = 100,
                Height = 35,
                FontSize = 14,
                Background = Brushes.Red,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            closeButton.Click += (s, e) => dialog.Close();
            mainPanel.Children.Add(closeButton);

            dialog.Content = mainPanel;
            dialog.ShowDialog();
        }

        // Метод для показа успешных операций
        private void ShowSuccessDialog(string title, string message)
        {
            var dialog = new Window
            {
                Title = title,
                Width = 350,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = Brushes.White,
                WindowStyle = WindowStyle.SingleBorderWindow
            };

            var mainPanel = new StackPanel { Margin = new Thickness(20, 20, 20, 20) };

            // Иконка успеха
            mainPanel.Children.Add(new TextBlock
            {
                Text = "✅",
                FontSize = 48,
                Foreground = Brushes.Green,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            });

            // Заголовок
            mainPanel.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Green,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            });

            // Сообщение
            mainPanel.Children.Add(new TextBlock
            {
                Text = message,
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            });

            // Кнопка закрытия
            var closeButton = new Button
            {
                Content = "OK",
                Width = 100,
                Height = 35,
                FontSize = 14,
                Background = Brushes.Green,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            closeButton.Click += (s, e) => dialog.Close();
            mainPanel.Children.Add(closeButton);

            dialog.Content = mainPanel;
            dialog.ShowDialog();
        }

        // Метод для показа информационных сообщений
        private void ShowInfoDialog(string title, string message)
        {
            var dialog = new Window
            {
                Title = title,
                Width = 350,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = Brushes.White,
                WindowStyle = WindowStyle.SingleBorderWindow
            };

            var mainPanel = new StackPanel { Margin = new Thickness(20, 20, 20, 20) };

            // Иконка информации
            mainPanel.Children.Add(new TextBlock
            {
                Text = "ℹ️",
                FontSize = 48,
                Foreground = Brushes.DodgerBlue,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            });

            // Заголовок
            mainPanel.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DodgerBlue,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            });

            // Сообщение
            mainPanel.Children.Add(new TextBlock
            {
                Text = message,
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            });

            // Кнопка закрытия
            var closeButton = new Button
            {
                Content = "OK",
                Width = 100,
                Height = 35,
                FontSize = 14,
                Background = Brushes.DodgerBlue,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            closeButton.Click += (s, e) => dialog.Close();
            mainPanel.Children.Add(closeButton);

            dialog.Content = mainPanel;
            dialog.ShowDialog();
        }

        private async void ShowLoginDialog()
        {
            var dialog = new Window
            {
                Title = "Вход в Финансовый Тамагоччи",
                Width = 350,
                Height = 280,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = Brushes.White,
                WindowStyle = WindowStyle.SingleBorderWindow,
                ShowInTaskbar = false
            };

            var mainPanel = new StackPanel { Margin = new Thickness(20, 20, 20, 20) };

            // Заголовок
            var title = new TextBlock
            {
                Text = "🐹 Финансовый Тамагоччи",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Green,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            mainPanel.Children.Add(title);

            // Имя пользователя
            mainPanel.Children.Add(new TextBlock
            {
                Text = "Ваш никнейм:",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            });

            var nameBox = new TextBox
            {
                FontSize = 16,
                Height = 40,
                Padding = new Thickness(10, 10, 10, 10),
                Margin = new Thickness(0, 0, 0, 20),
                Text = "Игрок"
            };
            mainPanel.Children.Add(nameBox);

            // Кнопки
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var loginButton = new Button
            {
                Content = "Войти",
                Width = 120,
                Height = 40,
                FontSize = 16,
                Background = Brushes.LimeGreen,
                Foreground = Brushes.White,
                Margin = new Thickness(5, 5, 5, 5)
            };
            loginButton.Click += async (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(nameBox.Text))
                {
                    ShowErrorDialog("Ошибка входа", "Введите никнейм!");
                    return;
                }

                try
                {
                    // Ищем пользователя по имени
                    var response = await client.GetAsync("/users/");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var users = JsonSerializer.Deserialize<List<User>>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        var user = users?.Find(u => u.name == nameBox.Text);

                        if (user != null)
                        {
                            currentUser = user;
                            dialog.Close();
                            await LoadUserData();
                            StartHungerTimer();
                        }
                        else
                        {
                            ShowErrorDialog("Ошибка входа", "Пользователь не найден. Зарегистрируйтесь.");
                        }
                    }
                    else
                    {
                        var errorJson = await response.Content.ReadAsStringAsync();
                        ShowErrorDialog("Ошибка сервера", $"Код ошибки: {response.StatusCode}\n{errorJson}");
                    }
                }
                catch (HttpRequestException)
                {
                    ShowErrorDialog("Ошибка подключения", "Не удалось подключиться к серверу. Убедитесь, что сервер запущен по адресу http://localhost:8000");
                }
                catch (Exception ex)
                {
                    ShowErrorDialog("Ошибка входа", ex.Message);
                }
            };

            var registerButton = new Button
            {
                Content = "Регистрация",
                Width = 120,
                Height = 40,
                FontSize = 16,
                Background = Brushes.DodgerBlue,
                Foreground = Brushes.White,
                Margin = new Thickness(5, 5, 5, 5)
            };
            registerButton.Click += async (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(nameBox.Text))
                {
                    ShowErrorDialog("Ошибка регистрации", "Введите никнейм!");
                    return;
                }

                try
                {
                    var newUser = new
                    {
                        name = nameBox.Text,
                        email = nameBox.Text + "@game.local", // Генерируем уникальный email
                        food_currency = 100,
                        pet_energy = 80
                    };

                    var json = JsonSerializer.Serialize(newUser);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("/users/", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        currentUser = JsonSerializer.Deserialize<User>(responseJson, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        ShowSuccessDialog("Добро пожаловать!", $"Регистрация прошла успешно, {currentUser.name}!");
                        dialog.Close();
                        await LoadUserData();
                        StartHungerTimer();
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        ShowErrorDialog("Ошибка регистрации", "Этот никнейм уже используется. Выберите другой.");
                    }
                    else
                    {
                        var errorJson = await response.Content.ReadAsStringAsync();
                        ShowErrorDialog("Ошибка регистрации", $"Код ошибки: {response.StatusCode}\n{errorJson}");
                    }
                }
                catch (HttpRequestException)
                {
                    ShowErrorDialog("Ошибка подключения", "Не удалось подключиться к серверу. Убедитесь, что сервер запущен по адресу http://localhost:8000");
                }
                catch (Exception ex)
                {
                    ShowErrorDialog("Ошибка регистрации", ex.Message);
                }
            };

            buttonPanel.Children.Add(loginButton);
            buttonPanel.Children.Add(registerButton);
            mainPanel.Children.Add(buttonPanel);

            dialog.Content = mainPanel;
            dialog.ShowDialog();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser != null)
            {
                ShowProfileDialog();
            }
        }

        private void ShowProfileDialog()
        {
            var dialog = new Window
            {
                Title = "👤 Профиль",
                Width = 300,
                Height = 250,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = Brushes.White
            };

            var mainPanel = new StackPanel { Margin = new Thickness(20, 20, 20, 20) };

            mainPanel.Children.Add(new TextBlock
            {
                Text = "Ваш профиль",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DodgerBlue,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            });

            mainPanel.Children.Add(new TextBlock
            {
                Text = $"👤 Никнейм: {currentUser.name}",
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 10)
            });

            mainPanel.Children.Add(new TextBlock
            {
                Text = $"🍯 Баланс: {currentUser.current_balance:N2} ₽",
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 10)
            });

            mainPanel.Children.Add(new TextBlock
            {
                Text = $"🥕 Корм: {currentUser.food_currency}",
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 20)
            });

            var closeButton = CreateButton("Закрыть", Brushes.Gray);
            closeButton.Click += (s, e) => dialog.Close();
            mainPanel.Children.Add(closeButton);

            dialog.Content = mainPanel;
            dialog.ShowDialog();
        }

        private async Task LoadUserData()
        {
            try
            {
                // Загружаем данные пользователя
                var userResponse = await client.GetAsync($"/users/{currentUser.user_id}");
                if (userResponse.IsSuccessStatusCode)
                {
                    var json = await userResponse.Content.ReadAsStringAsync();
                    currentUser = JsonSerializer.Deserialize<User>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                // Загружаем цели
                var goalsResponse = await client.GetAsync($"/goals/?user_id={currentUser.user_id}");
                if (goalsResponse.IsSuccessStatusCode)
                {
                    var json = await goalsResponse.Content.ReadAsStringAsync();
                    goals = JsonSerializer.Deserialize<List<FinancialGoal>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<FinancialGoal>();
                }

                // Загружаем транзакции
                var transResponse = await client.GetAsync($"/transactions/?user_id={currentUser.user_id}&days=30");
                if (transResponse.IsSuccessStatusCode)
                {
                    var json = await transResponse.Content.ReadAsStringAsync();
                    var transList = JsonSerializer.Deserialize<List<TransactionDto>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (transList != null)
                    {
                        transactions = transList.Select(t => new Transaction
                        {
                            Date = t.date,
                            Amount = t.amount,
                            Type = t.type,
                            Category = t.category
                        }).ToList();
                    }
                }

                UpdateUI();
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Ошибка загрузки данных", ex.Message);
            }
        }

        private void StartHungerTimer()
        {
            hungerTimer = new DispatcherTimer();
            hungerTimer.Interval = TimeSpan.FromSeconds(30); // Каждые 30 секунд
            hungerTimer.Tick += HungerTimer_Tick;
            hungerTimer.Start();
        }

        private async void HungerTimer_Tick(object sender, EventArgs e)
        {
            if (currentUser == null) return;

            try
            {
                // Получаем статус питомца с сервера
                var response = await client.GetAsync($"/pet/status/{currentUser.user_id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var status = JsonSerializer.Deserialize<PetStatus>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (status != null)
                    {
                        currentUser.food_currency = status.food_currency;
                        currentUser.pet_energy = status.pet_energy;

                        Dispatcher.Invoke(UpdateUI);

                        // Показываем предупреждение если питомец голоден
                        if (currentUser.pet_energy <= 20 && currentUser.pet_energy > 0)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                ShowInfoDialog("Питомец голоден!",
                                    $"Энергия питомца: {currentUser.pet_energy}%\nПокормите его скорее!");
                            });
                        }
                        else if (currentUser.pet_energy <= 0)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                ShowInfoDialog("Питомец уснул!",
                                    "Питомец уснул от голода.\nПокормите его, чтобы разбудить!");
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка таймера: {ex.Message}");
            }
        }

        private void StartPetAnimation()
        {
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
            var buttons = new[] { ExpenseButton, IncomeButton, GoalsButton, ChartsButton, FeedPetButton, ProfileButton };

            foreach (var button in buttons)
            {
                if (button != null)
                {
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
        }

        private void UpdateUI()
        {
            if (currentUser == null) return;

            // Обновляем денежный баланс
            BalanceText.Text = $"{currentUser.current_balance:N2} ₽";
            PetBalanceText.Text = $"{currentUser.current_balance:N0} ₽";

            // Обновляем игровую валюту
            FoodCurrencyText.Text = currentUser.food_currency.ToString();
            FoodText.Text = currentUser.food_currency.ToString();

            // Обновляем энергию
            EnergyBar.Value = currentUser.pet_energy;

            // Обновляем имя пользователя в профиле
            if (UserNameText != null)
            {
                UserNameText.Text = currentUser.name;
            }

            // Обновляем внешний вид питомца
            UpdatePetAppearance();
        }

        private void UpdatePetAppearance()
        {
            if (currentUser.pet_energy <= 0)
            {
                PetEmoji.Text = "😴";
                PetBorder.Background = new SolidColorBrush(Color.FromRgb(200, 200, 200));
                MoodText.Text = "Уснул 😴";
            }
            else if (currentUser.pet_energy <= 20)
            {
                PetEmoji.Text = "😢";
                PetBorder.Background = new SolidColorBrush(Color.FromRgb(255, 235, 156));
                MoodText.Text = "Очень голоден! 🥺";
            }
            else if (currentUser.pet_energy <= 40)
            {
                PetEmoji.Text = "😕";
                PetBorder.Background = new SolidColorBrush(Color.FromRgb(255, 224, 102));
                MoodText.Text = "Хочет кушать 😐";
            }
            else if (currentUser.pet_energy <= 60)
            {
                PetEmoji.Text = "😐";
                PetBorder.Background = new SolidColorBrush(Color.FromRgb(255, 214, 51));
                MoodText.Text = "Нормально 😐";
            }
            else if (currentUser.pet_energy <= 80)
            {
                PetEmoji.Text = "😊";
                PetBorder.Background = new SolidColorBrush(Color.FromRgb(255, 193, 7));
                MoodText.Text = "Хорошо! 😊";
            }
            else
            {
                PetEmoji.Text = "😄";
                PetBorder.Background = new SolidColorBrush(Color.FromRgb(255, 193, 7));
                MoodText.Text = "Отлично! 😄";
            }
        }

        private async void ExpenseButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowAddExpenseDialog();
        }

        private async void IncomeButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowAddIncomeDialog();
        }

        private async void GoalsButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowGoalsDialog();
        }

        private async void ChartsButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowChartsDialog();
        }

        private async void FeedPetButton_Click(object sender, RoutedEventArgs e)
        {
            await FeedPet();
        }

        private async Task FeedPet()
        {
            if (currentUser.food_currency < 10)
            {
                ShowErrorDialog("Недостаточно корма", "У вас недостаточно корма! Добавьте доход, чтобы заработать корм.");
                return;
            }

            try
            {
                var response = await client.PostAsync($"/pet/feed?user_id={currentUser.user_id}&food_amount=10", null);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<FeedResult>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result != null)
                    {
                        currentUser.food_currency = result.food_currency;
                        currentUser.pet_energy = result.pet_energy;

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

                        string message = $"Питомец покормлен! +20⚡";
                        if (result.bonus > 0)
                        {
                            message += $"\nБонус: +{result.bonus}🥕";
                        }

                        ShowSuccessDialog("Кормление успешно!", message);
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorJson = await response.Content.ReadAsStringAsync();
                    ShowErrorDialog("Ошибка кормления", errorJson.Replace("\"", ""));
                }
                else
                {
                    ShowErrorDialog("Ошибка кормления", "Произошла неизвестная ошибка при кормлении питомца.");
                }
            }
            catch (HttpRequestException)
            {
                ShowErrorDialog("Ошибка подключения", "Не удалось подключиться к серверу. Убедитесь, что сервер запущен.");
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Ошибка кормления", ex.Message);
            }
        }

        private async Task ShowAddExpenseDialog()
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

            // Текущий баланс для информации
            var balanceInfo = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(10, 10, 10, 10),
                Margin = new Thickness(0, 0, 0, 15)
            };

            var balanceText = new TextBlock
            {
                Text = $"💰 Текущий баланс: {currentUser.current_balance:N2} ₽",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Green
            };
            balanceInfo.Child = balanceText;
            mainPanel.Children.Add(balanceInfo);

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
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var addButton = CreateButton("Добавить", Brushes.OrangeRed);
            addButton.Click += async (s, e) =>
            {
                if (!double.TryParse(amountBox.Text, out double amount) || amount <= 0)
                {
                    ShowErrorDialog("Ошибка ввода", "Введите корректную сумму!");
                    return;
                }

                // Проверка на клиенте
                if (amount > currentUser.current_balance)
                {
                    ShowErrorDialog("Недостаточно средств",
                        $"У вас на балансе {currentUser.current_balance:N2} ₽, а вы пытаетесь потратить {amount:N2} ₽.\n\nПополните баланс или уменьшите сумму траты.");
                    return;
                }

                try
                {
                    var expense = new
                    {
                        user_id = currentUser.user_id,
                        amount = amount,
                        category = categoryBox.SelectedItem.ToString(),
                        date = DateTime.Today.ToString("yyyy-MM-dd")
                    };

                    var json = JsonSerializer.Serialize(expense);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("/expenses/", content);

                    if (response.IsSuccessStatusCode)
                    {
                        await LoadUserData();
                        ShowSuccessDialog("Трата добавлена!", $"Трата на {amount:N2}₽ успешно добавлена!");
                        dialog.Close();
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        var errorJson = await response.Content.ReadAsStringAsync();
                        ShowErrorDialog("Ошибка", errorJson.Replace("\"", ""));
                    }
                    else
                    {
                        ShowErrorDialog("Ошибка", "Произошла ошибка при добавлении траты.");
                    }
                }
                catch (HttpRequestException)
                {
                    ShowErrorDialog("Ошибка подключения", "Не удалось подключиться к серверу. Убедитесь, что сервер запущен.");
                }
                catch (Exception ex)
                {
                    ShowErrorDialog("Ошибка", ex.Message);
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

        private async Task ShowAddIncomeDialog()
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
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var addButton = CreateButton("Добавить", Brushes.LimeGreen);
            addButton.Click += async (s, e) =>
            {
                if (!double.TryParse(amountBox.Text, out double amount) || amount <= 0)
                {
                    ShowErrorDialog("Ошибка ввода", "Введите корректную сумму!");
                    return;
                }

                try
                {
                    var income = new
                    {
                        user_id = currentUser.user_id,
                        amount = amount,
                        source = sourceBox.SelectedItem.ToString(),
                        date = DateTime.Today.ToString("yyyy-MM-dd"),
                        is_recurring = false
                    };

                    var json = JsonSerializer.Serialize(income);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("/incomes/", content);

                    if (response.IsSuccessStatusCode)
                    {
                        await LoadUserData();
                        ShowSuccessDialog("Доход добавлен!",
                            $"Доход {amount:N2}₽ успешно добавлен!\n\n🎉 Получено +10🥕 за доход!");
                        dialog.Close();
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        var errorJson = await response.Content.ReadAsStringAsync();
                        ShowErrorDialog("Ошибка", errorJson.Replace("\"", ""));
                    }
                    else
                    {
                        ShowErrorDialog("Ошибка", "Произошла ошибка при добавлении дохода.");
                    }
                }
                catch (HttpRequestException)
                {
                    ShowErrorDialog("Ошибка подключения", "Не удалось подключиться к серверу. Убедитесь, что сервер запущен.");
                }
                catch (Exception ex)
                {
                    ShowErrorDialog("Ошибка", ex.Message);
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

        private async Task ShowGoalsDialog()
        {
            // Обновляем цели перед показом
            await LoadUserData();

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

            if (goals == null || goals.Count == 0)
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
                    var goalCard = await CreateGoalCard(goal);
                    if (goalCard != null)
                    {
                        goalsPanel.Children.Add(goalCard);
                    }
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
                if (goals != null && goals.Count > 0)
                {
                    dialog.Close();
                    ShowAddMoneyToGoalDialog();
                }
                else
                {
                    ShowInfoDialog("Информация", "Сначала добавьте цель!");
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

        private async Task<Border> CreateGoalCard(FinancialGoal goal)
        {
            if (goal == null) return null;

            var progress = goal.current_amount / goal.target_amount;
            var daysLeft = (goal.deadline - DateTime.Now).Days;

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
            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };

            var nameText = new TextBlock
            {
                Text = goal.name,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = goal.is_completed ? Brushes.LimeGreen : Brushes.DodgerBlue
            };

            var statusText = new TextBlock
            {
                Text = goal.is_completed ? " ✓ Выполнено" : $" ⌛ {daysLeft} дней",
                FontSize = 14,
                Margin = new Thickness(10, 0, 0, 0),
                Foreground = goal.is_completed ? Brushes.LimeGreen : Brushes.Orange
            };

            headerPanel.Children.Add(nameText);
            headerPanel.Children.Add(statusText);
            stack.Children.Add(headerPanel);

            // Прогресс
            stack.Children.Add(new TextBlock
            {
                Text = $"{goal.current_amount:N0}₽ / {goal.target_amount:N0}₽",
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
                Text = $"📅 До {goal.deadline:dd.MM.yyyy}",
                FontSize = 12,
                FontStyle = FontStyles.Italic,
                Foreground = Brushes.Gray
            });

            // Кнопка получения награды
            if (goal.is_completed && !goal.reward_claimed)
            {
                var claimButton = CreateSmallButton("🎁 Получить награду", Brushes.Orange);
                claimButton.Click += async (s, e) =>
                {
                    try
                    {
                        var response = await client.PostAsync($"/goals/{goal.goal_id}/claim_reward?user_id={currentUser.user_id}", null);

                        if (response.IsSuccessStatusCode)
                        {
                            await LoadUserData();
                            ShowSuccessDialog("Награда получена!",
                                $"Получено {goal.reward_amount}🥕 за выполнение цели '{goal.name}'!");
                            await ShowGoalsDialog();
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                        {
                            var errorJson = await response.Content.ReadAsStringAsync();
                            ShowErrorDialog("Ошибка", errorJson.Replace("\"", ""));
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowErrorDialog("Ошибка", ex.Message);
                    }
                };
                stack.Children.Add(claimButton);
            }

            card.Child = stack;
            return card;
        }

        private async Task ShowChartsDialog()
        {
            try
            {
                var response = await client.GetAsync($"/transactions/stats/{currentUser.user_id}");
                if (!response.IsSuccessStatusCode)
                {
                    ShowErrorDialog("Ошибка", "Не удалось загрузить статистику");
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                var stats = JsonSerializer.Deserialize<StatsResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (stats == null) return;

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

                var totalIncome = stats.totals?.total_income ?? 0;
                var totalExpense = stats.totals?.total_expense ?? 0;
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

                // График по дням
                var dailyTitle = new TextBlock
                {
                    Text = "📅 Доходы и расходы по дням (последние 7 дней):",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.DodgerBlue,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                mainPanel.Children.Add(dailyTitle);

                if (stats.daily != null)
                {
                    foreach (var day in stats.daily)
                    {
                        var dayPanel = CreateDayChart(day.day, day.income, day.expense);
                        mainPanel.Children.Add(dayPanel);
                    }
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

                if (stats.categories == null || stats.categories.Length == 0)
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
                    double maxExpense = stats.categories.Max(x => x.total);
                    foreach (var item in stats.categories)
                    {
                        var categoryPanel = CreateCategoryChart(item.category, item.total, maxExpense);
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
            catch (HttpRequestException)
            {
                ShowErrorDialog("Ошибка подключения", "Не удалось подключиться к серверу для загрузки статистики.");
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Ошибка загрузки статистики", ex.Message);
            }
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

        private async void ShowAddGoalDialog()
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
            addButton.Click += async (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(nameBox.Text))
                {
                    ShowErrorDialog("Ошибка", "Введите название цели!");
                    return;
                }

                if (!double.TryParse(targetBox.Text, out double target) || target <= 0)
                {
                    ShowErrorDialog("Ошибка", "Введите корректную целевую сумму!");
                    return;
                }

                if (!double.TryParse(currentBox.Text, out double current) || current < 0)
                {
                    current = 0;
                }

                if (current > target)
                {
                    ShowErrorDialog("Ошибка", "Текущая сумма не может превышать целевую!");
                    return;
                }

                if (!datePicker.SelectedDate.HasValue)
                {
                    ShowErrorDialog("Ошибка", "Выберите дату дедлайна!");
                    return;
                }

                try
                {
                    // Рассчитываем награду
                    int reward = (int)(target / 1000);
                    if (reward < 50) reward = 50;
                    if (reward > 100) reward = 100;

                    var newGoal = new
                    {
                        user_id = currentUser.user_id,
                        target_amount = target,
                        current_amount = current,
                        name = nameBox.Text,
                        deadline = datePicker.SelectedDate.Value.ToString("yyyy-MM-dd"),
                        reward_amount = reward
                    };

                    var json = JsonSerializer.Serialize(newGoal);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("/goals/", content);

                    if (response.IsSuccessStatusCode)
                    {
                        ShowSuccessDialog("Цель добавлена!",
                            $"Цель '{nameBox.Text}' успешно добавлена!\nНаграда за выполнение: {reward}🥕");
                        await LoadUserData();
                        dialog.Close();
                        await ShowGoalsDialog();
                    }
                    else
                    {
                        ShowErrorDialog("Ошибка", "Не удалось добавить цель.");
                    }
                }
                catch (Exception ex)
                {
                    ShowErrorDialog("Ошибка", ex.Message);
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

        private async void ShowAddMoneyToGoalDialog()
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

            // Текущий баланс
            var balanceInfo = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(10, 10, 10, 10),
                Margin = new Thickness(0, 0, 0, 15)
            };

            var balanceText = new TextBlock
            {
                Text = $"💰 Текущий баланс: {currentUser.current_balance:N2} ₽",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Green
            };
            balanceInfo.Child = balanceText;
            mainPanel.Children.Add(balanceInfo);

            // Выбор цели
            mainPanel.Children.Add(CreateLabel("Выберите цель:"));

            var goalsCombo = new ComboBox
            {
                FontSize = 16,
                Height = 40,
                Padding = new Thickness(10, 10, 10, 10),
                Margin = new Thickness(0, 0, 0, 20)
            };

            var activeGoals = goals.Where(g => !g.is_completed).ToList();
            foreach (var goal in activeGoals)
            {
                goalsCombo.Items.Add(goal);
            }

            if (goalsCombo.Items.Count == 0)
            {
                ShowInfoDialog("Информация", "Нет активных целей для пополнения!");
                dialog.Close();
                return;
            }

            goalsCombo.SelectedIndex = 0;
            goalsCombo.DisplayMemberPath = "name";
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
            addButton.Click += async (s, e) =>
            {
                if (!double.TryParse(amountBox.Text, out double amount) || amount <= 0)
                {
                    ShowErrorDialog("Ошибка", "Введите корректную сумму!");
                    return;
                }

                if (amount > currentUser.current_balance)
                {
                    ShowErrorDialog("Недостаточно средств",
                        $"У вас на балансе {currentUser.current_balance:N2} ₽");
                    return;
                }

                var selectedGoal = goalsCombo.SelectedItem as FinancialGoal;
                if (selectedGoal == null) return;

                try
                {
                    var response = await client.PostAsync(
                        $"/goals/{selectedGoal.goal_id}/add_money?amount={amount}&user_id={currentUser.user_id}", null);

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonSerializer.Deserialize<GoalUpdateResult>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        await LoadUserData();

                        if (result != null && result.bonus > 0)
                        {
                            ShowSuccessDialog("Цель пополнена!",
                                $"Цель '{selectedGoal.name}' пополнена на {amount:N2}₽!\nБонус: +{result.bonus}🥕 за активность!");
                        }
                        else
                        {
                            ShowSuccessDialog("Цель пополнена!",
                                $"Цель '{selectedGoal.name}' пополнена на {amount:N2}₽!");
                        }

                        dialog.Close();
                        await ShowGoalsDialog();
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        var errorJson = await response.Content.ReadAsStringAsync();
                        ShowErrorDialog("Ошибка", errorJson.Replace("\"", ""));
                    }
                    else
                    {
                        ShowErrorDialog("Ошибка", "Не удалось пополнить цель.");
                    }
                }
                catch (Exception ex)
                {
                    ShowErrorDialog("Ошибка", ex.Message);
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

    // Классы для работы с API
    public class User
    {
        public int user_id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public double current_balance { get; set; }
        public int food_currency { get; set; }
        public int pet_energy { get; set; }
    }

    public class FinancialGoal
    {
        public int goal_id { get; set; }
        public int user_id { get; set; }
        public double target_amount { get; set; }
        public double current_amount { get; set; }
        public string name { get; set; }
        public DateTime deadline { get; set; }
        public bool is_completed { get; set; }
        public int reward_amount { get; set; }
        public bool reward_claimed { get; set; }
    }

    public class TransactionDto
    {
        public DateTime date { get; set; }
        public double amount { get; set; }
        public string type { get; set; }
        public string category { get; set; }
    }

    public class Transaction
    {
        public DateTime Date { get; set; }
        public double Amount { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
    }

    public class PetStatus
    {
        public int food_currency { get; set; }
        public int pet_energy { get; set; }
        public double hours_without_food { get; set; }
    }

    public class FeedResult
    {
        public int food_currency { get; set; }
        public int pet_energy { get; set; }
        public int bonus { get; set; }
    }

    public class StatsResponse
    {
        public Totals totals { get; set; }
        public DailyStat[] daily { get; set; }
        public CategoryStat[] categories { get; set; }
    }

    public class Totals
    {
        public double total_income { get; set; }
        public double total_expense { get; set; }
    }

    public class DailyStat
    {
        public string day { get; set; }
        public double income { get; set; }
        public double expense { get; set; }
    }

    public class CategoryStat
    {
        public string category { get; set; }
        public double total { get; set; }
    }

    public class GoalUpdateResult
    {
        public int bonus { get; set; }
    }
}