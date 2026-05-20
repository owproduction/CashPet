package com.example.cashpet3;

import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.view.View;
import android.widget.Button;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;
import androidx.drawerlayout.widget.DrawerLayout;
import androidx.swiperefreshlayout.widget.SwipeRefreshLayout;

import com.example.cashpet3.api.ApiClient;
import com.example.cashpet3.api.ApiService;
import com.example.cashpet3.api.models.FeedResult;
import com.example.cashpet3.api.models.PetStatus;
import com.example.cashpet3.api.models.User;
import com.example.cashpet3.ui.dialogs.ExpenseDialog;
import com.example.cashpet3.ui.dialogs.GoalsDialog;
import com.example.cashpet3.ui.dialogs.IncomeDialog;
import com.example.cashpet3.ui.dialogs.LoginDialog;
import com.example.cashpet3.utils.AnimationHelper;
import com.example.cashpet3.utils.NotificationHelper;
import com.example.cashpet3.utils.SessionManager;
import com.google.android.material.navigation.NavigationView;

import java.util.Locale;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class MainActivity extends AppCompatActivity implements LoginDialog.LoginListener {

    // UI Components
    private DrawerLayout drawerLayout;
    private NavigationView navigationView;
    private SwipeRefreshLayout swipeRefreshLayout;

    // Pet info
    private TextView welcomeText, balanceText, foodCurrencyText;
    private TextView petBalanceText, foodText, moodText, energyPercent;
    private ProgressBar energyBar;
    private Button feedPetButton;
    private View petBorder;
    private TextView petEmoji;

    // Notification
    private View notificationBorder;
    private TextView notificationText;
    private Button closeNotificationButton;

    // Data
    private SessionManager sessionManager;
    private ApiService apiService;
    private NotificationHelper notificationHelper;
    private Handler handler;
    private Runnable petStatusRunnable;

    private User currentUser;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        // Initialize helpers
        sessionManager = new SessionManager(this);
        apiService = ApiClient.getApiService();
        notificationHelper = new NotificationHelper(this);
        handler = new Handler(Looper.getMainLooper());

        // Initialize UI
        initViews();
        setupNavigation();
        setupSwipeRefresh();

        // Check if user is logged in
        if (sessionManager.isLoggedIn()) {
            currentUser = sessionManager.getUser();
            updateUI();
            loadUserData();
            startPetStatusChecker();
        } else {
            showLoginDialog();
        }

        // Setup click listeners
        setupClickListeners();
    }

    private void initViews() {
        drawerLayout = findViewById(R.id.drawerLayout);
        navigationView = findViewById(R.id.navigationView);
        swipeRefreshLayout = findViewById(R.id.swipeRefreshLayout);

        welcomeText = findViewById(R.id.welcomeText);
        balanceText = findViewById(R.id.balanceText);
        foodCurrencyText = findViewById(R.id.foodCurrencyText);

        petBalanceText = findViewById(R.id.petBalanceText);
        foodText = findViewById(R.id.foodText);
        moodText = findViewById(R.id.moodText);
        energyPercent = findViewById(R.id.energyPercent);
        energyBar = findViewById(R.id.energyBar);
        feedPetButton = findViewById(R.id.feedPetButton);
        petBorder = findViewById(R.id.petBorder);
        petEmoji = findViewById(R.id.petEmoji);

        notificationBorder = findViewById(R.id.notificationBorder);
        notificationText = findViewById(R.id.notificationText);
        closeNotificationButton = findViewById(R.id.closeNotificationButton);

        // Start pet animation
        AnimationHelper.startPetPulseAnimation(petBorder);
    }

    private void setupNavigation() {
        navigationView.setNavigationItemSelectedListener(item -> {
            int id = item.getItemId();

            if (id == R.id.nav_expense) {
                showAddExpenseDialog();
            } else if (id == R.id.nav_income) {
                showAddIncomeDialog();
            } else if (id == R.id.nav_goals) {
                showGoalsDialog();
            } else if (id == R.id.nav_charts) {
                showChartsDialog();
            } else if (id == R.id.nav_profile) {
                showProfileDialog();
            } else if (id == R.id.nav_settings) {
                showSettingsDialog();
            }

            drawerLayout.closeDrawers();
            return true;
        });

        updateNavigationHeader();
    }

    private void updateNavigationHeader() {
        if (navigationView.getHeaderView(0) != null) {
            TextView navUserName = navigationView.getHeaderView(0).findViewById(R.id.navUserName);
            TextView navUserEmail = navigationView.getHeaderView(0).findViewById(R.id.navUserEmail);

            if (currentUser != null) {
                navUserName.setText(currentUser.getName());
                navUserEmail.setText(currentUser.getEmail());
            } else {
                navUserName.setText("Гость");
                navUserEmail.setText("");
            }
        }
    }

    private void setupSwipeRefresh() {
        swipeRefreshLayout.setOnRefreshListener(() -> {
            loadUserData();
            swipeRefreshLayout.setRefreshing(false);
        });
    }

    private void setupClickListeners() {
        feedPetButton.setOnClickListener(v -> feedPet());
        closeNotificationButton.setOnClickListener(v -> hideNotification());
    }

    private void showLoginDialog() {
        LoginDialog dialog = new LoginDialog();
        dialog.show(getSupportFragmentManager(), "login_dialog");
    }

    @Override
    public void onLoginSuccess(User user) {
        currentUser = user;
        updateUI();
        loadUserData();
        startPetStatusChecker();
        updateNavigationHeader();
    }

    private void loadUserData() {
        if (currentUser == null) return;

        apiService.getUser(currentUser.getUserId()).enqueue(new Callback<User>() {
            @Override
            public void onResponse(Call<User> call, Response<User> response) {
                if (response.isSuccessful() && response.body() != null) {
                    currentUser = response.body();
                    sessionManager.saveUser(currentUser);
                    updateUI();
                }
            }

            @Override
            public void onFailure(Call<User> call, Throwable t) {
                showNotification("Ошибка загрузки данных: " + t.getMessage());
            }
        });
    }

    private void updateUI() {
        if (currentUser == null) return;

        runOnUiThread(() -> {
            welcomeText.setText("Добро пожаловать, " + currentUser.getName() + "!");
            balanceText.setText(String.format(Locale.getDefault(), "%.0f ₽", currentUser.getCurrentBalance()));
            foodCurrencyText.setText(String.valueOf(currentUser.getFoodCurrency()));

            petBalanceText.setText(String.format(Locale.getDefault(), "%.0f ₽", currentUser.getCurrentBalance()));
            foodText.setText(String.valueOf(currentUser.getFoodCurrency()));
            energyBar.setProgress(currentUser.getPetEnergy());
            energyPercent.setText(currentUser.getPetEnergy() + "%");

            petEmoji.setText(currentUser.getPetEmoji());
            moodText.setText(currentUser.getMood());
        });
    }

    private void feedPet() {
        if (currentUser == null) {
            showNotification("Сначала войдите в аккаунт!");
            return;
        }

        if (currentUser.getFoodCurrency() < 10) {
            showNotification("Недостаточно корма! Добавьте доход, чтобы получить корм!");
            return;
        }

        AnimationHelper.animateButtonPress(feedPetButton, () -> {
            apiService.feedPet(currentUser.getUserId(), 10).enqueue(new Callback<FeedResult>() {
                @Override
                public void onResponse(Call<FeedResult> call, Response<FeedResult> response) {
                    if (response.isSuccessful() && response.body() != null) {
                        FeedResult result = response.body();
                        currentUser.setFoodCurrency(result.getFoodCurrency());
                        currentUser.setPetEnergy(result.getPetEnergy());
                        sessionManager.saveUser(currentUser);
                        updateUI();

                        String message = "🍽️ Питомец покормлен! Энергия +20, Корм -10";
                        if (result.hasBonus()) {
                            message += "\n🎉 Бонус: +" + result.getBonus() + " корма!";
                            notificationHelper.showBonusNotification(result.getBonus());
                        }
                        showNotification(message);
                    } else {
                        showNotification("Ошибка при кормлении");
                    }
                }

                @Override
                public void onFailure(Call<FeedResult> call, Throwable t) {
                    showNotification("Ошибка: " + t.getMessage());
                }
            });
        });
    }

    private void startPetStatusChecker() {
        petStatusRunnable = new Runnable() {
            @Override
            public void run() {
                if (currentUser != null) {
                    checkPetStatus();
                }
                handler.postDelayed(this, 30000);
            }
        };
        handler.post(petStatusRunnable);
    }

    private void checkPetStatus() {
        apiService.getPetStatus(currentUser.getUserId()).enqueue(new Callback<PetStatus>() {
            @Override
            public void onResponse(Call<PetStatus> call, Response<PetStatus> response) {
                if (response.isSuccessful() && response.body() != null) {
                    PetStatus status = response.body();
                    currentUser.setFoodCurrency(status.getFoodCurrency());
                    currentUser.setPetEnergy(status.getPetEnergy());
                    sessionManager.saveUser(currentUser);
                    updateUI();

                    if (status.isHungry()) {
                        showNotification("⚠️ Питомец голоден! Покормите его!");
                        notificationHelper.showHungerNotification();
                    }
                }
            }

            @Override
            public void onFailure(Call<PetStatus> call, Throwable t) {
                // Silent fail
            }
        });
    }

    private void showAddExpenseDialog() {
        ExpenseDialog dialog = ExpenseDialog.newInstance(() -> {
            loadUserData();
            showNotification("💰 Расход добавлен!");
        });
        dialog.show(getSupportFragmentManager(), "expense_dialog");
    }

    private void showAddIncomeDialog() {
        IncomeDialog dialog = IncomeDialog.newInstance(() -> {
            loadUserData();
            showNotification("💵 Доход добавлен!");
        });
        dialog.show(getSupportFragmentManager(), "income_dialog");
    }

    private void showGoalsDialog() {
        GoalsDialog dialog = GoalsDialog.newInstance(() -> {
            loadUserData();
            showNotification("🎯 Цели обновлены!");
        });
        dialog.show(getSupportFragmentManager(), "goals_dialog");
    }

    private void showChartsDialog() {
        Toast.makeText(this, "Графики (будет реализовано)", Toast.LENGTH_SHORT).show();
    }

    private void showProfileDialog() {
        if (currentUser != null) {
            String message = "👤 Профиль\n\n" +
                    "Никнейм: " + currentUser.getName() + "\n" +
                    "Email: " + currentUser.getEmail() + "\n" +
                    "Баланс: " + String.format(Locale.getDefault(), "%.2f", currentUser.getCurrentBalance()) + " ₽\n" +
                    "Корм: " + currentUser.getFoodCurrency() + "\n" +
                    "Энергия питомца: " + currentUser.getPetEnergy() + "%";

            new androidx.appcompat.app.AlertDialog.Builder(this)
                    .setTitle("Ваш профиль")
                    .setMessage(message)
                    .setPositiveButton("OK", null)
                    .show();
        } else {
            showNotification("Сначала войдите в аккаунт!");
        }
    }

    private void showSettingsDialog() {
        new androidx.appcompat.app.AlertDialog.Builder(this)
                .setTitle("Настройки")
                .setMessage("Настройки будут доступны в следующем обновлении!\n\nПланируется:\n- Смена питомца\n- Настройка уведомлений\n- Смена темы")
                .setPositiveButton("OK", null)
                .show();
    }

    private void showNotification(String message) {
        runOnUiThread(() -> {
            notificationText.setText(message);
            notificationBorder.setVisibility(View.VISIBLE);
            AnimationHelper.fadeInNotification(notificationBorder);

            handler.removeCallbacks(hideNotificationRunnable);
            handler.postDelayed(hideNotificationRunnable, 5000);
        });
    }

    private Runnable hideNotificationRunnable = () -> {
        if (notificationBorder.getVisibility() == View.VISIBLE) {
            AnimationHelper.fadeOutNotification(notificationBorder, () ->
                    notificationBorder.setVisibility(View.GONE));
        }
    };

    private void hideNotification() {
        handler.removeCallbacks(hideNotificationRunnable);
        AnimationHelper.fadeOutNotification(notificationBorder, () ->
                notificationBorder.setVisibility(View.GONE));
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        if (handler != null && petStatusRunnable != null) {
            handler.removeCallbacks(petStatusRunnable);
        }
    }
}