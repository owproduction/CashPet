using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FinancialTamagotchi
{
    public partial class MainWindow : Window
    {
        private User? currentUser;
        private List<Goal> goals = new List<Goal>();
        private List<Transaction> transactions = new List<Transaction>();
        private HttpClient client = new HttpClient();
        private DispatcherTimer? hungerTimer;
        private DispatcherTimer? notificationTimer;
        private DoubleAnimation? menuAnimation;
        private bool isMenuExpanded = false;

        public MainWindow()
        {
            InitializeComponent();

            client.BaseAddress = new Uri("http://192.168.133.20:8000");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            // Установка фоновой картинки
            try
            {
                string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "back.png");

                if (!System.IO.File.Exists(imagePath))
                {
                    imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "back.png");
                }

                if (System.IO.File.Exists(imagePath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    BackgroundImage.Source = bitmap;
                    BackgroundImage.Stretch = Stretch.Fill;
                    BackgroundImage.Opacity = 1; // Полностью непрозрачная картинка
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки фона: {ex.Message}");
            }

            this.Loaded += async (s, e) =>
            {
                await ShowLoginDialogAsync();
            };

            SetupButtonEffects();
            StartPetAnimation();
            InitializeMenuAnimation();
            UpdateMenuButtonsVisibility(false);
        }

        private void InitializeMenuAnimation()
        {
            menuAnimation = new DoubleAnimation();
            menuAnimation.Duration = TimeSpan.FromMilliseconds(250);
            menuAnimation.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };
            menuAnimation.Completed += (s, e) => UpdateMenuButtonsVisibility(isMenuExpanded);
        }

        private void MenuToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (menuAnimation == null) return;

            if (isMenuExpanded)
            {
                menuAnimation.To = 80;
                SideMenuBorder.BeginAnimation(Border.WidthProperty, menuAnimation);
                isMenuExpanded = false;
            }
            else
            {
                menuAnimation.To = 260;
                SideMenuBorder.BeginAnimation(Border.WidthProperty, menuAnimation);
                isMenuExpanded = true;
            }
        }

        private void UpdateMenuButtonsVisibility(bool isExpanded)
        {
            var visibility = isExpanded ? Visibility.Visible : Visibility.Collapsed;

            if (ExpenseText != null) ExpenseText.Visibility = visibility;
            if (IncomeText != null) IncomeText.Visibility = visibility;
            if (GoalsText != null) GoalsText.Visibility = visibility;
            if (ChartsText != null) ChartsText.Visibility = visibility;
            if (SettingsText != null) SettingsText.Visibility = visibility;
            if (ProfileText != null) ProfileText.Visibility = visibility;
        }

        private void CloseNotificationButton_Click(object sender, RoutedEventArgs e)
        {
            NotificationBorder.Visibility = Visibility.Collapsed;
        }

        private void ShowNotification(string message)
        {
            Dispatcher.Invoke(() =>
            {
                NotificationText.Text = message;
                NotificationBorder.Visibility = Visibility.Visible;

                if (notificationTimer != null)
                {
                    notificationTimer.Stop();
                }
                notificationTimer = new DispatcherTimer();
                notificationTimer.Interval = TimeSpan.FromSeconds(5);
                notificationTimer.Tick += (s, e) =>
                {
                    NotificationBorder.Visibility = Visibility.Collapsed;
                    notificationTimer.Stop();
                };
                notificationTimer.Start();
            });
        }

        private async Task ShowLoginDialogAsync()
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

            var mainPanel = new StackPanel { Margin = new Thickness(20) };

            mainPanel.Children.Add(new TextBlock
            {
                Text = "🐹 Финансовый Тамагоччи",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#601AC2")),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            });

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
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 20),
                Text = "Игрок"
            };
            mainPanel.Children.Add(nameBox);

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
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50")),
                Foreground = Brushes.White,
                Margin = new Thickness(5)
            };
            loginButton.Click += async (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(nameBox.Text))
                {
                    MessageBox.Show("Введите никнейм!");
                    return;
                }

                try
                {
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
                            ShowNotification($"Добро пожаловать, {currentUser.name}!");
                        }
                        else
                        {
                            MessageBox.Show("Пользователь не найден. Зарегистрируйтесь.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка входа: {ex.Message}\n\nУбедитесь, что сервер запущен на http://localhost:8000");
                }
            };

            var registerButton = new Button
            {
                Content = "Регистрация",
                Width = 120,
                Height = 40,
                FontSize = 16,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3")),
                Foreground = Brushes.White,
                Margin = new Thickness(5)
            };
            registerButton.Click += async (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(nameBox.Text))
                {
                    MessageBox.Show("Введите никнейм!");
                    return;
                }

                try
                {
                    var newUser = new
                    {
                        name = nameBox.Text,
                        email = nameBox.Text + "@game.local",
                        food_currency = 100,
                        pet_energy = 80,
                        current_balance = 15000
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

                        MessageBox.Show($"Добро пожаловать, {currentUser!.name}!");
                        dialog.Close();
                        await LoadUserData();
                        StartHungerTimer();
                        ShowNotification($"Создан новый профиль: {currentUser.name}!");
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Ошибка регистрации: {error}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка регистрации: {ex.Message}");
                }
            };

            buttonPanel.Children.Add(loginButton);
            buttonPanel.Children.Add(registerButton);
            mainPanel.Children.Add(buttonPanel);

            dialog.Content = mainPanel;
            dialog.ShowDialog();
        }

        private void StartHungerTimer()
        {
            hungerTimer = new DispatcherTimer();
            hungerTimer.Interval = TimeSpan.FromSeconds(30);
            hungerTimer.Tick += HungerTimer_Tick;
            hungerTimer.Start();
        }

        private async void HungerTimer_Tick(object? sender, EventArgs e)
        {
            if (currentUser == null) return;

            try
            {
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

                        if (currentUser.pet_energy < 30)
                        {
                            ShowNotification("⚠️ Питомец голоден! Покормите его!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка таймера: {ex.Message}");
            }
        }

        private void StartPetAnimation()
        {
            var animation = new DoubleAnimation
            {
                From = 0.95,
                To = 1.05,
                Duration = TimeSpan.FromSeconds(2),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            PetBorder.RenderTransformOrigin = new Point(0.5, 0.5);
            PetBorder.RenderTransform = new ScaleTransform();
            PetBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            PetBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
        }

        private void SetupButtonEffects()
        {
            var buttons = new Button[] { ExpenseButton, IncomeButton, GoalsButton, ChartsButton, FeedPetButton, ProfileButton, SettingsButton };

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

        private async Task LoadUserData()
        {
            if (currentUser == null) return;

            try
            {
                var userResponse = await client.GetAsync($"/users/{currentUser.user_id}");
                if (userResponse.IsSuccessStatusCode)
                {
                    var json = await userResponse.Content.ReadAsStringAsync();
                    currentUser = JsonSerializer.Deserialize<User>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                var goalsResponse = await client.GetAsync($"/goals/?user_id={currentUser.user_id}");
                if (goalsResponse.IsSuccessStatusCode)
                {
                    var json = await goalsResponse.Content.ReadAsStringAsync();
                    goals = JsonSerializer.Deserialize<List<Goal>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<Goal>();
                }

                var transactionsResponse = await client.GetAsync($"/transactions/?user_id={currentUser.user_id}");
                if (transactionsResponse.IsSuccessStatusCode)
                {
                    var json = await transactionsResponse.Content.ReadAsStringAsync();
                    transactions = JsonSerializer.Deserialize<List<Transaction>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<Transaction>();
                }

                UpdateUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void UpdateUI()
        {
            if (currentUser == null) return;

            BalanceText.Text = $"{currentUser.current_balance:N0} ₽";
            PetBalanceText.Text = $"{currentUser.current_balance:N0} ₽";
            FoodCurrencyText.Text = currentUser.food_currency.ToString();
            FoodText.Text = currentUser.food_currency.ToString();
            EnergyBar.Value = currentUser.pet_energy;
            EnergyPercent.Text = $"{currentUser.pet_energy}%";
            WelcomeText.Text = $"Добро пожаловать, {currentUser.name}!";

            UpdatePetAppearance();
        }

        private void UpdatePetAppearance()
        {
            if (currentUser == null) return;

            if (currentUser.pet_energy <= 0)
            {
                PetEmoji.Text = "😴";
                MoodText.Text = "Уснул";
            }
            else if (currentUser.pet_energy <= 20)
            {
                PetEmoji.Text = "😢";
                MoodText.Text = "Очень голоден!";
            }
            else if (currentUser.pet_energy <= 40)
            {
                PetEmoji.Text = "😕";
                MoodText.Text = "Хочет кушать";
            }
            else if (currentUser.pet_energy <= 60)
            {
                PetEmoji.Text = "😐";
                MoodText.Text = "Нормально";
            }
            else if (currentUser.pet_energy <= 80)
            {
                PetEmoji.Text = "😊";
                MoodText.Text = "Хорошо!";
            }
            else
            {
                PetEmoji.Text = "😄";
                MoodText.Text = "Отлично!";
            }
        }

        private async void FeedPetButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null)
            {
                ShowNotification("Сначала войдите в аккаунт!");
                return;
            }

            if (currentUser.food_currency < 10)
            {
                ShowNotification("Недостаточно корма! Заработайте деньги (доход) и корм появится автоматически!");
                return;
            }

            try
            {
                // Отправляем параметры через URL (как ожидает ваш бэкенд)
                var response = await client.PostAsync($"/pet/feed?user_id={currentUser.user_id}&food_amount=10", null);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<FeedResult>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result != null)
                    {
                        currentUser.food_currency = result.food_currency;
                        currentUser.pet_energy = result.pet_energy;
                        UpdateUI();

                        if (result.bonus > 0)
                        {
                            ShowNotification($"🍽️ Вы покормили питомца! Энергия +20, Корм -10. Бонус: +{result.bonus} корма! 🎉");
                        }
                        else
                        {
                            ShowNotification($"🍽️ Вы покормили питомца! Энергия +20, Корм -10");
                        }
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ShowNotification($"Ошибка при кормлении: {error}");
                    System.Diagnostics.Debug.WriteLine($"Ошибка: {error}");
                }
            }
            catch (Exception ex)
            {
                ShowNotification($"Ошибка: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Исключение: {ex.Message}");
            }
        }

        private async void ExpenseButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null)
            {
                ShowNotification("Сначала войдите в аккаунт!");
                return;
            }

            var dialog = new Window
            {
                Title = "Добавить расход",
                Width = 380,
                Height = 420,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = Brushes.White,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true
            };

            var mainBorder = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(24),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")),
                BorderThickness = new Thickness(1),
                Effect = new DropShadowEffect { BlurRadius = 20, ShadowDepth = 3, Opacity = 0.1 }
            };

            var panel = new StackPanel { Margin = new Thickness(24) };

            var titlePanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 20) };
            titlePanel.Children.Add(new TextBlock { Text = "💸", FontSize = 28, Margin = new Thickness(0, 0, 10, 0) });
            titlePanel.Children.Add(new TextBlock
            {
                Text = "Добавить расход",
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5722")),
                VerticalAlignment = VerticalAlignment.Center
            });
            panel.Children.Add(titlePanel);

            panel.Children.Add(new TextBlock
            {
                Text = "Сумма",
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#625A5A")),
                Margin = new Thickness(0, 0, 0, 8)
            });

            var amountBox = new TextBox
            {
                FontSize = 18,
                Height = 50,
                Padding = new Thickness(15, 0, 15, 0),
                Margin = new Thickness(0, 0, 0, 20),
                Text = "100",
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5722")),
                FontWeight = FontWeights.Bold
            };
            amountBox.GotFocus += (s, args) => { if (amountBox.Text == "100") amountBox.Text = ""; };
            amountBox.LostFocus += (s, args) => { if (string.IsNullOrWhiteSpace(amountBox.Text)) amountBox.Text = "100"; };
            panel.Children.Add(amountBox);

            panel.Children.Add(new TextBlock
            {
                Text = "Категория",
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#625A5A")),
                Margin = new Thickness(0, 0, 0, 8)
            });

            var categoryBox = new ComboBox
            {
                FontSize = 16,
                Height = 50,
                Margin = new Thickness(0, 0, 0, 25),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F5")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")),
                BorderThickness = new Thickness(1)
            };

            var categories = new[]
            {
                new { Icon = "🍔", Name = "Продукты" },
                new { Icon = "🚗", Name = "Транспорт" },
                new { Icon = "🏠", Name = "Жильё" },
                new { Icon = "👕", Name = "Одежда" },
                new { Icon = "💊", Name = "Здоровье" },
                new { Icon = "🎮", Name = "Развлечения" },
                new { Icon = "📚", Name = "Образование" },
                new { Icon = "💸", Name = "Другое" }
            };

            foreach (var cat in categories)
            {
                var stack = new StackPanel { Orientation = Orientation.Horizontal };
                stack.Children.Add(new TextBlock { Text = cat.Icon, FontSize = 18, Margin = new Thickness(0, 0, 10, 0) });
                stack.Children.Add(new TextBlock { Text = cat.Name, FontSize = 15, VerticalAlignment = VerticalAlignment.Center });
                categoryBox.Items.Add(stack);
            }
            categoryBox.SelectedIndex = 0;
            panel.Children.Add(categoryBox);

            var okButton = new Button
            {
                Content = "OK",
                Height = 52,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Cursor = Cursors.Hand,
                Margin = new Thickness(0, 10, 0, 0),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5722"))
            };

            okButton.Click += async (s, args) =>
            {
                string selectedCategory = "";
                if (categoryBox.SelectedItem is StackPanel sp && sp.Children[1] is TextBlock tb)
                {
                    selectedCategory = tb.Text;
                }

                if (!decimal.TryParse(amountBox.Text, out decimal amount) || amount <= 0)
                {
                    MessageBox.Show("Введите корректную сумму!");
                    return;
                }

                if (amount > (decimal)currentUser.current_balance)
                {
                    MessageBox.Show($"Недостаточно средств! Баланс: {currentUser.current_balance:N2} ₽");
                    return;
                }

                try
                {
                    var expense = new
                    {
                        user_id = currentUser.user_id,
                        amount = (double)amount,
                        category = selectedCategory,
                        description = "",
                        date = DateTime.Now.ToString("yyyy-MM-dd"),
                        is_planned = false
                    };

                    var json = JsonSerializer.Serialize(expense);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("/expenses/", content);

                    if (response.IsSuccessStatusCode)
                    {
                        await LoadUserData();
                        dialog.Close();
                        ShowNotification($"💰 Расход: {selectedCategory} -{amount:N2} ₽");
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Ошибка: {error}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            };

            panel.Children.Add(okButton);

            var closeButton = new Button
            {
                Content = "✕",
                Width = 30,
                Height = 30,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#999999")),
                Cursor = Cursors.Hand,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, -10, -10, 0)
            };
            closeButton.Click += (s, args) => dialog.Close();

            var grid = new Grid();
            grid.Children.Add(mainBorder);
            grid.Children.Add(closeButton);
            mainBorder.Child = panel;
            dialog.Content = grid;
            dialog.ShowDialog();
        }

        private async void IncomeButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null)
            {
                ShowNotification("Сначала войдите в аккаунт!");
                return;
            }

            var dialog = new Window
            {
                Title = "Добавить доход",
                Width = 380,
                Height = 420,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = Brushes.White,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true
            };

            var mainBorder = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(24),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")),
                BorderThickness = new Thickness(1),
                Effect = new DropShadowEffect { BlurRadius = 20, ShadowDepth = 3, Opacity = 0.1 }
            };

            var panel = new StackPanel { Margin = new Thickness(24) };

            var titlePanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 20) };
            titlePanel.Children.Add(new TextBlock { Text = "💰", FontSize = 28, Margin = new Thickness(0, 0, 10, 0) });
            titlePanel.Children.Add(new TextBlock
            {
                Text = "Добавить доход",
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#601AC2")),
                VerticalAlignment = VerticalAlignment.Center
            });
            panel.Children.Add(titlePanel);

            panel.Children.Add(new TextBlock
            {
                Text = "Сумма",
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#625A5A")),
                Margin = new Thickness(0, 0, 0, 8)
            });

            var amountBox = new TextBox
            {
                FontSize = 18,
                Height = 50,
                Padding = new Thickness(15, 0, 15, 0),
                Margin = new Thickness(0, 0, 0, 20),
                Text = "1000",
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#601AC2")),
                FontWeight = FontWeights.Bold
            };
            amountBox.GotFocus += (s, args) => { if (amountBox.Text == "1000") amountBox.Text = ""; };
            amountBox.LostFocus += (s, args) => { if (string.IsNullOrWhiteSpace(amountBox.Text)) amountBox.Text = "1000"; };
            panel.Children.Add(amountBox);

            panel.Children.Add(new TextBlock
            {
                Text = "Категория",
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#625A5A")),
                Margin = new Thickness(0, 0, 0, 8)
            });

            var categoryBox = new ComboBox
            {
                FontSize = 16,
                Height = 50,
                Margin = new Thickness(0, 0, 0, 25),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F5")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")),
                BorderThickness = new Thickness(1)
            };

            var categories = new[]
            {
                new { Icon = "💼", Name = "Зарплата" },
                new { Icon = "💻", Name = "Фриланс" },
                new { Icon = "🎁", Name = "Подарок" },
                new { Icon = "📈", Name = "Инвестиции" },
                new { Icon = "🏆", Name = "Премия" },
                new { Icon = "🏠", Name = "Аренда" },
                new { Icon = "💰", Name = "Другое" }
            };

            foreach (var cat in categories)
            {
                var stack = new StackPanel { Orientation = Orientation.Horizontal };
                stack.Children.Add(new TextBlock { Text = cat.Icon, FontSize = 18, Margin = new Thickness(0, 0, 10, 0) });
                stack.Children.Add(new TextBlock { Text = cat.Name, FontSize = 15, VerticalAlignment = VerticalAlignment.Center });
                categoryBox.Items.Add(stack);
            }
            categoryBox.SelectedIndex = 0;
            panel.Children.Add(categoryBox);

            var okButton = new Button
            {
                Content = "OK",
                Height = 52,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Cursor = Cursors.Hand,
                Margin = new Thickness(0, 10, 0, 0),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#601AC2"))
            };

            okButton.Click += async (s, args) =>
            {
                string selectedCategory = "";
                if (categoryBox.SelectedItem is StackPanel sp && sp.Children[1] is TextBlock tb)
                {
                    selectedCategory = tb.Text;
                }

                if (!decimal.TryParse(amountBox.Text, out decimal amount) || amount <= 0)
                {
                    MessageBox.Show("Введите корректную сумму!");
                    return;
                }

                try
                {
                    var income = new
                    {
                        user_id = currentUser.user_id,
                        amount = (double)amount,
                        source = selectedCategory,
                        date = DateTime.Now.ToString("yyyy-MM-dd"),
                        is_recurring = false
                    };

                    var json = JsonSerializer.Serialize(income);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("/incomes/", content);

                    if (response.IsSuccessStatusCode)
                    {
                        await LoadUserData();
                        dialog.Close();
                        ShowNotification($"💵 Доход: +{amount:N2} ₽");
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Ошибка: {error}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            };

            panel.Children.Add(okButton);

            var closeButton = new Button
            {
                Content = "✕",
                Width = 30,
                Height = 30,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#999999")),
                Cursor = Cursors.Hand,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, -10, -10, 0)
            };
            closeButton.Click += (s, args) => dialog.Close();

            var grid = new Grid();
            grid.Children.Add(mainBorder);
            grid.Children.Add(closeButton);
            mainBorder.Child = panel;
            dialog.Content = grid;
            dialog.ShowDialog();
        }

        private async void GoalsButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null)
            {
                ShowNotification("Сначала войдите в аккаунт!");
                return;
            }

            var dialog = new Window
            {
                Title = "Мои цели",
                Width = 450,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = Brushes.White,
                WindowStyle = WindowStyle.SingleBorderWindow
            };

            var scrollViewer = new ScrollViewer();
            var panel = new StackPanel { Margin = new Thickness(20) };

            panel.Children.Add(new TextBlock
            {
                Text = "🎯 Финансовые цели",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#601AC2")),
                Margin = new Thickness(0, 0, 0, 15)
            });

            await LoadUserData();

            if (goals.Count == 0)
            {
                panel.Children.Add(new TextBlock
                {
                    Text = "Нет активных целей. Создайте новую!",
                    FontSize = 14,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 0, 0, 15)
                });
            }
            else
            {
                foreach (var goal in goals)
                {
                    var border = new Border
                    {
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F5")),
                        CornerRadius = new CornerRadius(10),
                        Padding = new Thickness(15),
                        Margin = new Thickness(0, 0, 0, 10)
                    };

                    var stack = new StackPanel();

                    var titleStack = new StackPanel { Orientation = Orientation.Horizontal };
                    titleStack.Children.Add(new TextBlock
                    {
                        Text = goal.name,
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#601AC2"))
                    });
                    if (goal.is_completed)
                    {
                        titleStack.Children.Add(new TextBlock
                        {
                            Text = " ✅ ВЫПОЛНЕНА",
                            FontSize = 12,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"))
                        });
                    }
                    stack.Children.Add(titleStack);

                    stack.Children.Add(new TextBlock
                    {
                        Text = $"Цель: {goal.target_amount:N2} ₽",
                        FontSize = 12,
                        Margin = new Thickness(0, 5, 0, 0)
                    });
                    stack.Children.Add(new TextBlock
                    {
                        Text = $"Накоплено: {goal.current_amount:N2} ₽",
                        FontSize = 12
                    });

                    var progressBar = new ProgressBar
                    {
                        Value = goal.current_amount,
                        Maximum = goal.target_amount,
                        Height = 6,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D0FF00")),
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9D9D9")),
                        Margin = new Thickness(0, 8, 0, 8)
                    };
                    stack.Children.Add(progressBar);

                    if (!goal.is_completed)
                    {
                        var addButton = new Button
                        {
                            Content = "💰 Пополнить цель",
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#601AC2")),
                            Foreground = Brushes.White,
                            Height = 30,
                            FontSize = 12,
                            Cursor = Cursors.Hand,
                            Margin = new Thickness(0, 5, 0, 0)
                        };

                        int goalId = goal.goal_id;
                        addButton.Click += async (s, args) =>
                        {
                            var amountDialog = new Window
                            {
                                Title = "Пополнение цели",
                                Width = 300,
                                Height = 150,
                                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                                Owner = dialog,
                                ResizeMode = ResizeMode.NoResize,
                                Background = Brushes.White,
                                WindowStyle = WindowStyle.SingleBorderWindow
                            };

                            var amountPanel = new StackPanel { Margin = new Thickness(20) };
                            amountPanel.Children.Add(new TextBlock { Text = "Сумма пополнения (₽):", FontSize = 14 });
                            var amountBox = new TextBox { FontSize = 14, Height = 35, Margin = new Thickness(0, 10, 0, 10), Text = "1000" };
                            amountPanel.Children.Add(amountBox);

                            var confirmButton = new Button
                            {
                                Content = "Пополнить",
                                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50")),
                                Foreground = Brushes.White,
                                Height = 35
                            };
                            confirmButton.Click += async (c, args2) =>
                            {
                                if (decimal.TryParse(amountBox.Text, out decimal addAmount) && addAmount > 0)
                                {
                                    try
                                    {
                                        var response = await client.PostAsync($"/goals/{goalId}/add_money?amount={(double)addAmount}&user_id={currentUser.user_id}", null);
                                        if (response.IsSuccessStatusCode)
                                        {
                                            await LoadUserData();
                                            amountDialog.Close();
                                            dialog.Close();
                                            ShowNotification($"🎯 Цель пополнена на {addAmount:N2} ₽!");
                                        }
                                        else
                                        {
                                            var error = await response.Content.ReadAsStringAsync();
                                            MessageBox.Show($"Ошибка: {error}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show($"Ошибка: {ex.Message}");
                                    }
                                }
                            };
                            amountPanel.Children.Add(confirmButton);
                            amountDialog.Content = amountPanel;
                            amountDialog.ShowDialog();
                        };
                        stack.Children.Add(addButton);
                    }
                    else if (!goal.reward_claimed)
                    {
                        var rewardButton = new Button
                        {
                            Content = $"🎁 Получить награду ({goal.reward_amount} корма)",
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800")),
                            Foreground = Brushes.White,
                            Height = 30,
                            FontSize = 12,
                            Cursor = Cursors.Hand,
                            Margin = new Thickness(0, 5, 0, 0)
                        };

                        int goalId = goal.goal_id;
                        rewardButton.Click += async (c, args2) =>
                        {
                            try
                            {
                                var response = await client.PostAsync($"/goals/{goalId}/claim_reward?user_id={currentUser.user_id}", null);
                                if (response.IsSuccessStatusCode)
                                {
                                    await LoadUserData();
                                    dialog.Close();
                                    ShowNotification($"🎉 Награда получена: +{goal.reward_amount} корма!");
                                }
                                else
                                {
                                    var error = await response.Content.ReadAsStringAsync();
                                    MessageBox.Show($"Ошибка: {error}");
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ошибка: {ex.Message}");
                            }
                        };
                        stack.Children.Add(rewardButton);
                    }

                    border.Child = stack;
                    panel.Children.Add(border);
                }
            }

            var createButton = new Button
            {
                Content = "+ Создать новую цель",
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#601AC2")),
                Foreground = Brushes.White,
                Height = 40,
                FontSize = 14,
                Cursor = Cursors.Hand,
                Margin = new Thickness(0, 10, 0, 0)
            };

            createButton.Click += async (s, args) =>
            {
                var goalDialog = new Window
                {
                    Title = "Новая цель",
                    Width = 350,
                    Height = 300,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = dialog,
                    ResizeMode = ResizeMode.NoResize,
                    Background = Brushes.White,
                    WindowStyle = WindowStyle.SingleBorderWindow
                };

                var goalPanel = new StackPanel { Margin = new Thickness(20) };
                goalPanel.Children.Add(new TextBlock { Text = "Название цели:", FontSize = 14, Margin = new Thickness(0, 0, 0, 5) });
                var nameBox = new TextBox { FontSize = 14, Height = 35, Margin = new Thickness(0, 0, 0, 15), Text = "Новая цель" };
                goalPanel.Children.Add(nameBox);
                goalPanel.Children.Add(new TextBlock { Text = "Целевая сумма (₽):", FontSize = 14, Margin = new Thickness(0, 0, 0, 5) });
                var amountBox = new TextBox { FontSize = 14, Height = 35, Margin = new Thickness(0, 0, 0, 15), Text = "10000" };
                goalPanel.Children.Add(amountBox);
                goalPanel.Children.Add(new TextBlock { Text = "Дедлайн (необязательно):", FontSize = 14, Margin = new Thickness(0, 0, 0, 5) });
                var deadlineBox = new DatePicker { FontSize = 14, Height = 35, Margin = new Thickness(0, 0, 0, 15) };
                goalPanel.Children.Add(deadlineBox);

                var createGoalButton = new Button
                {
                    Content = "Создать цель",
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50")),
                    Foreground = Brushes.White,
                    Height = 40,
                    FontSize = 14,
                    Cursor = Cursors.Hand
                };

                createGoalButton.Click += async (c, args2) =>
                {
                    if (!decimal.TryParse(amountBox.Text, out decimal amount) || amount <= 0)
                    {
                        MessageBox.Show("Введите корректную сумму!");
                        return;
                    }

                    try
                    {
                        var newGoal = new
                        {
                            user_id = currentUser.user_id,
                            name = nameBox.Text,
                            target_amount = (double)amount,
                            current_amount = 0.0,
                            deadline = deadlineBox.SelectedDate?.ToString("yyyy-MM-dd"),
                            is_completed = false,
                            reward_amount = 50,
                            reward_claimed = false
                        };

                        var json = JsonSerializer.Serialize(newGoal);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        var response = await client.PostAsync("/goals/", content);

                        if (response.IsSuccessStatusCode)
                        {
                            await LoadUserData();
                            goalDialog.Close();
                            dialog.Close();
                            ShowNotification($"🎯 Создана новая цель: {nameBox.Text} на {amount:N2} ₽");
                        }
                        else
                        {
                            var error = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"Ошибка при создании цели: {error}");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}");
                    }
                };

                goalPanel.Children.Add(createGoalButton);
                goalDialog.Content = goalPanel;
                goalDialog.ShowDialog();
            };

            panel.Children.Add(createButton);
            scrollViewer.Content = panel;
            dialog.Content = scrollViewer;
            dialog.ShowDialog();
        }

        private async void ChartsButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null)
            {
                ShowNotification("Сначала войдите в аккаунт!");
                return;
            }

            try
            {
                var response = await client.GetAsync($"/transactions/stats/{currentUser.user_id}");
                if (!response.IsSuccessStatusCode)
                {
                    ShowNotification("Ошибка загрузки статистики");
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                var stats = JsonSerializer.Deserialize<StatsResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var dialog = new Window
                {
                    Title = "Графики и статистика",
                    Width = 500,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this,
                    ResizeMode = ResizeMode.NoResize,
                    Background = Brushes.White,
                    WindowStyle = WindowStyle.SingleBorderWindow
                };

                var scrollViewer = new ScrollViewer();
                var panel = new StackPanel { Margin = new Thickness(20) };

                panel.Children.Add(new TextBlock
                {
                    Text = "📊 Финансовая статистика",
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#601AC2")),
                    Margin = new Thickness(0, 0, 0, 15)
                });

                var totalsBorder = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F5")),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(15),
                    Margin = new Thickness(0, 0, 0, 10)
                };

                var totalsStack = new StackPanel();
                totalsStack.Children.Add(new TextBlock
                {
                    Text = $"💰 Всего доходов: {(stats?.totals?.total_income ?? 0):N2} ₽",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"))
                });
                totalsStack.Children.Add(new TextBlock
                {
                    Text = $"💸 Всего расходов: {(stats?.totals?.total_expense ?? 0):N2} ₽",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5722")),
                    Margin = new Thickness(0, 5, 0, 0)
                });
                totalsStack.Children.Add(new TextBlock
                {
                    Text = $"📈 Текущий баланс: {currentUser.current_balance:N2} ₽",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#601AC2")),
                    Margin = new Thickness(0, 5, 0, 0)
                });
                totalsBorder.Child = totalsStack;
                panel.Children.Add(totalsBorder);

                if (stats?.categories != null && stats.categories.Count > 0)
                {
                    panel.Children.Add(new TextBlock
                    {
                        Text = "📂 Расходы по категориям:",
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 10, 0, 10)
                    });

                    foreach (var cat in stats.categories)
                    {
                        var catBorder = new Border
                        {
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DBBEFF")),
                            CornerRadius = new CornerRadius(8),
                            Padding = new Thickness(12),
                            Margin = new Thickness(0, 0, 0, 5)
                        };
                        catBorder.Child = new TextBlock
                        {
                            Text = $"{cat.category}: {cat.total:N2} ₽",
                            FontSize = 13,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#601AC2"))
                        };
                        panel.Children.Add(catBorder);
                    }
                }

                panel.Children.Add(new TextBlock
                {
                    Text = $"📝 Последние транзакции ({transactions.Count}):",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 10, 0, 10)
                });

                foreach (var trans in transactions.GetRange(0, Math.Min(10, transactions.Count)))
                {
                    var transBorder = new Border
                    {
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(12),
                        Margin = new Thickness(0, 0, 0, 5),
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F5"))
                    };

                    var sign = trans.type == "income" ? "+" : "-";
                    transBorder.Child = new TextBlock
                    {
                        Text = $"{trans.date?.ToString("dd.MM")} | {trans.category} | {sign}{trans.amount:N2} ₽",
                        FontSize = 12,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"))
                    };
                    panel.Children.Add(transBorder);
                }

                scrollViewer.Content = panel;
                dialog.Content = scrollViewer;
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                ShowNotification($"Ошибка загрузки статистики: {ex.Message}");
            }
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser != null)
            {
                MessageBox.Show($"👤 Профиль\n\nНикнейм: {currentUser.name}\nEmail: {currentUser.email}\nБаланс: {currentUser.current_balance:N2} ₽\nКорм: {currentUser.food_currency}\nЭнергия питомца: {currentUser.pet_energy}%",
                    "Ваш профиль", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                ShowNotification("Сначала войдите в аккаунт!");
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Настройки будут доступны в следующем обновлении!\n\nПланируется:\n- Смена питомца\n- Настройка уведомлений\n- Смена темы",
                "Настройки", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    // ========== МОДЕЛИ ДАННЫХ ==========

    public class User
    {
        public int user_id { get; set; }
        public string name { get; set; } = "";
        public string email { get; set; } = "";
        public double current_balance { get; set; }
        public int food_currency { get; set; }
        public int pet_energy { get; set; }
        public DateTime registration_date { get; set; }
        public DateTime last_feed_time { get; set; }
    }

    public class Goal
    {
        public int goal_id { get; set; }
        public int user_id { get; set; }
        public double target_amount { get; set; }
        public double current_amount { get; set; }
        public string name { get; set; } = "";
        public DateTime? deadline { get; set; }
        public bool is_completed { get; set; }
        public int reward_amount { get; set; }
        public bool reward_claimed { get; set; }
    }

    public class Transaction
    {
        public int transaction_id { get; set; }
        public int user_id { get; set; }
        public double amount { get; set; }
        public string type { get; set; } = "";
        public string category { get; set; } = "";
        public DateTime? date { get; set; }
        public string description { get; set; } = "";
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
        public Totals totals { get; set; } = new Totals();
        public List<DailyStats> daily { get; set; } = new List<DailyStats>();
        public List<CategoryStats> categories { get; set; } = new List<CategoryStats>();
    }

    public class Totals
    {
        public double total_income { get; set; }
        public double total_expense { get; set; }
    }

    public class DailyStats
    {
        public string day { get; set; } = "";
        public double income { get; set; }
        public double expense { get; set; }
    }

    public class CategoryStats
    {
        public string category { get; set; } = "";
        public double total { get; set; }
    }
}