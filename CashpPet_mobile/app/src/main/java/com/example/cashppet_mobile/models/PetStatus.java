package com.example.cashppet_mobile.models;

import com.google.gson.annotations.SerializedName;

public class PetStatus {
    @SerializedName("food_currency")
    private int foodCurrency;
    @SerializedName("pet_energy")
    private int petEnergy;
    @SerializedName("hours_without_food")
    private double hoursWithoutFood;

    public int getFoodCurrency() { return foodCurrency; }
    public void setFoodCurrency(int foodCurrency) { this.foodCurrency = foodCurrency; }

    public int getPetEnergy() { return petEnergy; }
    public void setPetEnergy(int petEnergy) { this.petEnergy = petEnergy; }

    public double getHoursWithoutFood() { return hoursWithoutFood; }
    public void setHoursWithoutFood(double hoursWithoutFood) { this.hoursWithoutFood = hoursWithoutFood; }
}