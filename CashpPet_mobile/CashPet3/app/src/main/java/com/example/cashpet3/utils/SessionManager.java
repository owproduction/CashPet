package com.example.cashpet3.utils;

import android.content.Context;
import android.content.SharedPreferences;

import com.example.cashpet3.api.models.User;
import com.google.gson.Gson;
import com.google.gson.reflect.TypeToken;

import java.lang.reflect.Type;

public class SessionManager {

    private static final String PREF_NAME = "FinancialTamagotchiPrefs";
    private static final String KEY_USER = "current_user";
    private static final String KEY_IS_LOGGED_IN = "is_logged_in";

    private SharedPreferences sharedPreferences;
    private SharedPreferences.Editor editor;
    private Gson gson;

    public SessionManager(Context context) {
        sharedPreferences = context.getSharedPreferences(PREF_NAME, Context.MODE_PRIVATE);
        editor = sharedPreferences.edit();
        gson = new Gson();
    }

    // Сохранить текущего пользователя
    public void saveUser(User user) {
        String userJson = gson.toJson(user);
        editor.putString(KEY_USER, userJson);
        editor.putBoolean(KEY_IS_LOGGED_IN, true);
        editor.apply();
    }

    // Получить текущего пользователя
    public User getUser() {
        String userJson = sharedPreferences.getString(KEY_USER, null);
        if (userJson != null) {
            Type type = new TypeToken<User>() {}.getType();
            return gson.fromJson(userJson, type);
        }
        return null;
    }

    // Проверить, залогинен ли пользователь
    public boolean isLoggedIn() {
        return sharedPreferences.getBoolean(KEY_IS_LOGGED_IN, false);
    }

    // Обновить данные пользователя (баланс, корм, энергию)
    public void updateUserData(double balance, int foodCurrency, int petEnergy) {
        User user = getUser();
        if (user != null) {
            user.setCurrentBalance(balance);
            user.setFoodCurrency(foodCurrency);
            user.setPetEnergy(petEnergy);
            saveUser(user);
        }
    }

    // Обновить баланс
    public void updateBalance(double balance) {
        User user = getUser();
        if (user != null) {
            user.setCurrentBalance(balance);
            saveUser(user);
        }
    }

    // Обновить корм
    public void updateFoodCurrency(int foodCurrency) {
        User user = getUser();
        if (user != null) {
            user.setFoodCurrency(foodCurrency);
            saveUser(user);
        }
    }

    // Обновить энергию питомца
    public void updatePetEnergy(int petEnergy) {
        User user = getUser();
        if (user != null) {
            user.setPetEnergy(petEnergy);
            saveUser(user);
        }
    }

    // Очистить сессию (выйти из аккаунта)
    public void clearSession() {
        editor.clear();
        editor.apply();
    }

    // Получить ID текущего пользователя
    public int getCurrentUserId() {
        User user = getUser();
        return user != null ? user.getUserId() : -1;
    }

    // Получить имя текущего пользователя
    public String getCurrentUserName() {
        User user = getUser();
        return user != null ? user.getName() : "";
    }

    // Получить баланс текущего пользователя
    public double getCurrentBalance() {
        User user = getUser();
        return user != null ? user.getCurrentBalance() : 0;
    }

    // Получить количество корма
    public int getCurrentFoodCurrency() {
        User user = getUser();
        return user != null ? user.getFoodCurrency() : 0;
    }

    // Получить энергию питомца
    public int getCurrentPetEnergy() {
        User user = getUser();
        return user != null ? user.getPetEnergy() : 0;
    }
}