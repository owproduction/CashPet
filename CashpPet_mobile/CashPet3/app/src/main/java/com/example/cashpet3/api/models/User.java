package com.example.cashpet3.api.models;

import com.google.gson.annotations.SerializedName;

import java.util.Date;

public class User {

    @SerializedName("user_id")
    private int userId;

    @SerializedName("name")
    private String name;

    @SerializedName("email")
    private String email;

    @SerializedName("current_balance")
    private double currentBalance;

    @SerializedName("food_currency")
    private int foodCurrency;

    @SerializedName("pet_energy")
    private int petEnergy;

    @SerializedName("registration_date")
    private Date registrationDate;

    @SerializedName("last_feed_time")
    private Date lastFeedTime;

    @SerializedName("total_saved")
    private double totalSaved;

    // Конструкторы
    public User() {}

    public User(String name, String email) {
        this.name = name;
        this.email = email;
        this.currentBalance = 15000;
        this.foodCurrency = 100;
        this.petEnergy = 80;
    }

    // Getters и Setters
    public int getUserId() {
        return userId;
    }

    public void setUserId(int userId) {
        this.userId = userId;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public String getEmail() {
        return email;
    }

    public void setEmail(String email) {
        this.email = email;
    }

    public double getCurrentBalance() {
        return currentBalance;
    }

    public void setCurrentBalance(double currentBalance) {
        this.currentBalance = currentBalance;
    }

    public int getFoodCurrency() {
        return foodCurrency;
    }

    public void setFoodCurrency(int foodCurrency) {
        this.foodCurrency = foodCurrency;
    }

    public int getPetEnergy() {
        return petEnergy;
    }

    public void setPetEnergy(int petEnergy) {
        this.petEnergy = petEnergy;
    }

    public Date getRegistrationDate() {
        return registrationDate;
    }

    public void setRegistrationDate(Date registrationDate) {
        this.registrationDate = registrationDate;
    }

    public Date getLastFeedTime() {
        return lastFeedTime;
    }

    public void setLastFeedTime(Date lastFeedTime) {
        this.lastFeedTime = lastFeedTime;
    }

    public double getTotalSaved() {
        return totalSaved;
    }

    public void setTotalSaved(double totalSaved) {
        this.totalSaved = totalSaved;
    }

    // Вспомогательный метод для определения настроения питомца
    public String getMood() {
        if (petEnergy <= 0) return "Уснул";
        if (petEnergy <= 20) return "Очень голоден!";
        if (petEnergy <= 40) return "Хочет кушать";
        if (petEnergy <= 60) return "Нормально";
        if (petEnergy <= 80) return "Хорошо!";
        return "Отлично!";
    }

    // Вспомогательный метод для получения эмодзи питомца
    public String getPetEmoji() {
        if (petEnergy <= 0) return "😴";
        if (petEnergy <= 20) return "😢";
        if (petEnergy <= 40) return "😕";
        if (petEnergy <= 60) return "😐";
        if (petEnergy <= 80) return "😊";
        return "😄";
    }
}