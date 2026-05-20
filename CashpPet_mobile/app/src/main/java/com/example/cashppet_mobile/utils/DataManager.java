package com.example.cashppet_mobile.utils;

import android.content.Context;
import android.content.SharedPreferences;
import android.os.Handler;
import android.os.Looper;

import com.example.cashppet_mobile.api.ApiClient;
import com.example.cashppet_mobile.api.ApiInterface;
import com.example.cashppet_mobile.models.*;

import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.List;
import java.util.Locale;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

import retrofit2.Call;
import retrofit2.Response;

public class DataManager {
    private static DataManager instance;
    private final ApiInterface api;
    private final ExecutorService executorService;
    private final Handler mainHandler;

    private User currentUser;
    private List<Goal> goals;
    private Context appContext;

    private OnDataChangedListener dataChangedListener;
    private OnNotificationListener notificationListener;

    private DataManager(Context context) {
        this.appContext = context.getApplicationContext();
        api = ApiClient.getApiInterface();
        executorService = Executors.newSingleThreadExecutor();
        mainHandler = new Handler(Looper.getMainLooper());
        loadSavedUser();
    }

    public static synchronized DataManager getInstance(Context context) {
        if (instance == null) {
            instance = new DataManager(context);
        }
        return instance;
    }

    public void setOnDataChangedListener(OnDataChangedListener listener) {
        this.dataChangedListener = listener;
    }

    public void setOnNotificationListener(OnNotificationListener listener) {
        this.notificationListener = listener;
    }

    public User getCurrentUser() { return currentUser; }
    public List<Goal> getGoals() { return goals; }

    private void saveUser(User user) {
        SharedPreferences prefs = appContext.getSharedPreferences("app_data", Context.MODE_PRIVATE);
        SharedPreferences.Editor editor = prefs.edit();
        editor.putInt("user_id", user.getUserId());
        editor.putString("user_name", user.getName());
        editor.putString("user_email", user.getEmail());
        editor.putFloat("user_balance", (float) user.getCurrentBalance());
        editor.putInt("user_food", user.getFoodCurrency());
        editor.putInt("user_energy", user.getPetEnergy());
        editor.apply();
    }

    private void loadSavedUser() {
        SharedPreferences prefs = appContext.getSharedPreferences("app_data", Context.MODE_PRIVATE);
        int userId = prefs.getInt("user_id", -1);
        if (userId != -1) {
            currentUser = new User();
            currentUser.setUserId(userId);
            currentUser.setName(prefs.getString("user_name", ""));
            currentUser.setEmail(prefs.getString("user_email", ""));
            currentUser.setCurrentBalance(prefs.getFloat("user_balance", 0));
            currentUser.setFoodCurrency(prefs.getInt("user_food", 100));
            currentUser.setPetEnergy(prefs.getInt("user_energy", 80));
        }
    }

    public void login(String username, AuthCallback callback) {
        executorService.execute(() -> {
            try {
                Call<List<User>> call = api.getUsers();
                Response<List<User>> response = call.execute();

                if (response.isSuccessful() && response.body() != null) {
                    User foundUser = null;
                    for (User user : response.body()) {
                        if (user.getName().equals(username)) {
                            foundUser = user;
                            break;
                        }
                    }

                    if (foundUser != null) {
                        currentUser = foundUser;
                        saveUser(currentUser);
                        final String successMessage = "Добро пожаловать, " + currentUser.getName() + "!";
                        mainHandler.post(() -> {
                            showNotification(successMessage);
                            if (callback != null) callback.onSuccess(currentUser);
                            if (dataChangedListener != null) dataChangedListener.onUserChanged(currentUser);
                        });
                    } else {
                        mainHandler.post(() -> {
                            if (callback != null) callback.onError("Пользователь не найден");
                        });
                    }
                } else {
                    mainHandler.post(() -> {
                        if (callback != null) callback.onError("Ошибка входа");
                    });
                }
            } catch (Exception e) {
                final String errorMessage = "Ошибка сети: " + e.getMessage();
                mainHandler.post(() -> {
                    if (callback != null) callback.onError(errorMessage);
                });
            }
        });
    }

    public void register(String username, AuthCallback callback) {
        executorService.execute(() -> {
            try {
                User newUser = new User(username, username + "@game.local");
                Call<User> call = api.createUser(newUser);
                Response<User> response = call.execute();

                if (response.isSuccessful() && response.body() != null) {
                    currentUser = response.body();
                    saveUser(currentUser);
                    final String successMessage = "Создан новый профиль: " + currentUser.getName() + "!";
                    mainHandler.post(() -> {
                        showNotification(successMessage);
                        if (callback != null) callback.onSuccess(currentUser);
                        if (dataChangedListener != null) dataChangedListener.onUserChanged(currentUser);
                    });
                } else {
                    mainHandler.post(() -> {
                        if (callback != null) callback.onError("Ошибка регистрации");
                    });
                }
            } catch (Exception e) {
                final String errorMessage = "Ошибка сети: " + e.getMessage();
                mainHandler.post(() -> {
                    if (callback != null) callback.onError(errorMessage);
                });
            }
        });
    }

    public void addExpense(double amount, String category) {
        executorService.execute(() -> {
            try {
                Transaction expense = new Transaction();
                expense.setUserId(currentUser.getUserId());
                expense.setAmount(amount);
                expense.setCategory(category);
                expense.setType("expense");
                expense.setDate(getCurrentDate());

                Call<Transaction> call = api.createExpense(expense);
                Response<Transaction> response = call.execute();

                if (response.isSuccessful()) {
                    final String message = "💰 Расход: " + category + " -" + amount + " ₽";
                    mainHandler.post(() -> {
                        showNotification(message);
                        updateUserBalance();
                    });
                }
            } catch (Exception e) {
                e.printStackTrace();
            }
        });
    }

    public void addIncome(double amount, String source) {
        executorService.execute(() -> {
            try {
                Transaction income = new Transaction();
                income.setUserId(currentUser.getUserId());
                income.setAmount(amount);
                income.setCategory(source);
                income.setType("income");
                income.setDate(getCurrentDate());

                Call<Transaction> call = api.createIncome(income);
                Response<Transaction> response = call.execute();

                if (response.isSuccessful()) {
                    final String message = "💵 Доход: +" + amount + " ₽";
                    mainHandler.post(() -> {
                        showNotification(message);
                        updateUserBalance();
                    });
                }
            } catch (Exception e) {
                e.printStackTrace();
            }
        });
    }

    public void feedPet() {
        if (currentUser.getFoodCurrency() < 10) {
            final String message = "Недостаточно корма!";
            mainHandler.post(() -> showNotification(message));
            return;
        }

        executorService.execute(() -> {
            try {
                Call<ApiInterface.FeedResult> call = api.feedPet(currentUser.getUserId(), 10);
                Response<ApiInterface.FeedResult> response = call.execute();

                if (response.isSuccessful() && response.body() != null) {
                    ApiInterface.FeedResult result = response.body();
                    currentUser.setFoodCurrency(result.food_currency);
                    currentUser.setPetEnergy(result.pet_energy);

                    String message;
                    if (result.bonus > 0) {
                        message = "🍽️ Покормили! Бонус: +" + result.bonus + " корма! 🎉";
                    } else {
                        message = "🍽️ Покормили! Энергия +20, Корм -10";
                    }

                    final String finalMessage = message;
                    mainHandler.post(() -> {
                        showNotification(finalMessage);
                        if (dataChangedListener != null) {
                            dataChangedListener.onUserChanged(currentUser);
                        }
                    });
                } else {
                    final String errorMessage = "Ошибка при кормлении";
                    mainHandler.post(() -> showNotification(errorMessage));
                }
            } catch (Exception e) {
                e.printStackTrace();
            }
        });
    }

    private void updateUserBalance() {
        executorService.execute(() -> {
            try {
                Call<User> call = api.getUser(currentUser.getUserId());
                Response<User> response = call.execute();
                if (response.isSuccessful() && response.body() != null) {
                    currentUser = response.body();
                    mainHandler.post(() -> {
                        if (dataChangedListener != null) dataChangedListener.onUserChanged(currentUser);
                    });
                }
            } catch (Exception e) {
                e.printStackTrace();
            }
        });
    }

    private String getCurrentDate() {
        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd", Locale.getDefault());
        return sdf.format(new Date());
    }

    private void showNotification(String message) {
        if (notificationListener != null) {
            notificationListener.onNotification(message);
        }
    }

    public interface AuthCallback {
        void onSuccess(User user);
        void onError(String error);
    }

    public interface OnDataChangedListener {
        void onUserChanged(User user);
    }

    public interface OnNotificationListener {
        void onNotification(String message);
    }
}