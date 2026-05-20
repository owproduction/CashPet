package com.example.cashpet3.api.models;

import com.google.gson.annotations.SerializedName;

public class PetStatus {

    @SerializedName("food_currency")
    private int foodCurrency;

    @SerializedName("pet_energy")
    private int petEnergy;

    @SerializedName("hours_without_food")
    private double hoursWithoutFood;

    // Конструкторы
    public PetStatus() {}

    public PetStatus(int foodCurrency, int petEnergy, double hoursWithoutFood) {
        this.foodCurrency = foodCurrency;
        this.petEnergy = petEnergy;
        this.hoursWithoutFood = hoursWithoutFood;
    }

    // Getters и Setters
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

    public double getHoursWithoutFood() {
        return hoursWithoutFood;
    }

    public void setHoursWithoutFood(double hoursWithoutFood) {
        this.hoursWithoutFood = hoursWithoutFood;
    }

    // Вспомогательные методы
    public boolean isHungry() {
        return petEnergy < 30;
    }

    public boolean isVeryHungry() {
        return petEnergy < 15;
    }

    public String getHungerStatus() {
        if (petEnergy <= 0) return "Критический голод!";
        if (petEnergy <= 20) return "Очень голоден!";
        if (petEnergy <= 40) return "Голоден";
        if (petEnergy <= 60) return "Нормально";
        if (petEnergy <= 80) return "Сыт";
        return "Очень сыт";
    }
}