package com.example.cashpet3.api.models;

import com.google.gson.annotations.SerializedName;

public class FeedResult {

    @SerializedName("food_currency")
    private int foodCurrency;

    @SerializedName("pet_energy")
    private int petEnergy;

    @SerializedName("bonus")
    private int bonus;

    // Конструкторы
    public FeedResult() {}

    public FeedResult(int foodCurrency, int petEnergy, int bonus) {
        this.foodCurrency = foodCurrency;
        this.petEnergy = petEnergy;
        this.bonus = bonus;
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

    public int getBonus() {
        return bonus;
    }

    public void setBonus(int bonus) {
        this.bonus = bonus;
    }

    // Вспомогательные методы
    public boolean hasBonus() {
        return bonus > 0;
    }

    public String getBonusMessage() {
        if (bonus > 0) {
            return "🎉 Бонус! +" + bonus + " корма!";
        }
        return "";
    }
}