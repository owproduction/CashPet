package com.example.cashppet_mobile.utils;

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

    private OnDataChangedListener dataChangedListener;
    private OnNotificationListener notificationListener;

    private DataManager() {
        api = ApiClient.getApiInterface();
        executorService = Executors.newSingleThreadExecutor();
        mainHandler = new Handler(Looper.getMainLooper());
    }

    public static synchronized DataManager getInstance() {
        if (instance == null) {
            instance = new DataManager();
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
                        mainHandler.post(() -> {
                            showNotification("Добро пожаловать, " + currentUser.getName() + "!");
                            if (callback != null) callback.onSuccess(currentUser);
                            if (dataChangedListener != null) dataChangedListener.onUserChanged(currentUser);
                        });
                    } else {
                        mainHandler.post(() -> {
                            if (callback != null) callback.onError("Пользователь не найден");
                        });
                    }
                }
            } catch (Exception e) {
                mainHandler.post(() -> {
                    if (callback != null) callback.onError("Ошибка сети: " + e.getMessage());
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
                    mainHandler.post(() -> {
                        showNotification("Создан новый профиль: " + currentUser.getName() + "!");
                        if (callback != null) callback.onSuccess(currentUser);
                        if (dataChangedListener != null) dataChangedListener.onUserChanged(currentUser);
                    });
                } else {
                    mainHandler.post(() -> {
                        if (callback != null) callback.onError("Ошибка регистрации");
                    });
                }
            } catch (Exception e) {
                mainHandler.post(() -> {
                    if (callback != null) callback.onError("Ошибка сети: " + e.getMessage());
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
                    mainHandler.post(() -> {
                        showNotification("💰 Расход: " + category + " -" + amount + " ₽");
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
                    mainHandler.post(() -> {
                        showNotification("💵 Доход: +" + amount + " ₽");
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
            showNotification("Недостаточно корма!");
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

                    String message = "🍽️ Покормили! Энергия +20";
                    if (result.bonus > 0) {
                        message = "🍽️ Покормили! Бонус: +" + result.bonus + " корма! 🎉";
                    }

                    mainHandler.post(() -> {
                        showNotification(message);
                        if (dataChangedListener != null) {
                            dataChangedListener.onUserChanged(currentUser);
                        }
                    });
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