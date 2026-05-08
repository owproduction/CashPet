package com.example.cashppet_mobile.models;

import com.google.gson.annotations.SerializedName;

public class User {
    @SerializedName("user_id")
    private int userId;
    private String name;
    private String email;
    @SerializedName("current_balance")
    private double currentBalance;
    @SerializedName("food_currency")
    private int foodCurrency;
    @SerializedName("pet_energy")
    private int petEnergy;
    @SerializedName("registration_date")
    private String registrationDate;
    @SerializedName("last_feed_time")
    private String lastFeedTime;

    public User() {}

    public User(String name, String email) {
        this.name = name;
        this.email = email;
        this.currentBalance = 15000;
        this.foodCurrency = 100;
        this.petEnergy = 80;
    }

    // Getters
    public int getUserId() { return userId; }
    public String getName() { return name; }
    public String getEmail() { return email; }
    public double getCurrentBalance() { return currentBalance; }
    public int getFoodCurrency() { return foodCurrency; }
    public int getPetEnergy() { return petEnergy; }
    public String getRegistrationDate() { return registrationDate; }
    public String getLastFeedTime() { return lastFeedTime; }

    // Setters
    public void setUserId(int userId) { this.userId = userId; }
    public void setName(String name) { this.name = name; }
    public void setEmail(String email) { this.email = email; }
    public void setCurrentBalance(double currentBalance) { this.currentBalance = currentBalance; }
    public void setFoodCurrency(int foodCurrency) { this.foodCurrency = foodCurrency; }
    public void setPetEnergy(int petEnergy) { this.petEnergy = petEnergy; }
    public void setRegistrationDate(String registrationDate) { this.registrationDate = registrationDate; }
    public void setLastFeedTime(String lastFeedTime) { this.lastFeedTime = lastFeedTime; }
}