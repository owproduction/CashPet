package com.example.cashpet3.api.models;

import com.google.gson.annotations.SerializedName;

import java.util.Date;

public class Goal {

    @SerializedName("goal_id")
    private int goalId;

    @SerializedName("user_id")
    private int userId;

    @SerializedName("name")
    private String name;

    @SerializedName("target_amount")
    private double targetAmount;

    @SerializedName("current_amount")
    private double currentAmount;

    @SerializedName("deadline")
    private Date deadline;

    @SerializedName("is_completed")
    private boolean isCompleted;

    @SerializedName("reward_amount")
    private int rewardAmount;

    @SerializedName("reward_claimed")
    private boolean rewardClaimed;

    // Конструкторы
    public Goal() {}

    public Goal(int userId, String name, double targetAmount) {
        this.userId = userId;
        this.name = name;
        this.targetAmount = targetAmount;
        this.currentAmount = 0;
        this.isCompleted = false;
        this.rewardAmount = 50;
        this.rewardClaimed = false;
    }

    // Getters и Setters
    public int getGoalId() {
        return goalId;
    }

    public void setGoalId(int goalId) {
        this.goalId = goalId;
    }

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

    public double getTargetAmount() {
        return targetAmount;
    }

    public void setTargetAmount(double targetAmount) {
        this.targetAmount = targetAmount;
    }

    public double getCurrentAmount() {
        return currentAmount;
    }

    public void setCurrentAmount(double currentAmount) {
        this.currentAmount = currentAmount;
    }

    public Date getDeadline() {
        return deadline;
    }

    public void setDeadline(Date deadline) {
        this.deadline = deadline;
    }

    public boolean isCompleted() {
        return isCompleted;
    }

    public void setCompleted(boolean completed) {
        isCompleted = completed;
    }

    public int getRewardAmount() {
        return rewardAmount;
    }

    public void setRewardAmount(int rewardAmount) {
        this.rewardAmount = rewardAmount;
    }

    public boolean isRewardClaimed() {
        return rewardClaimed;
    }

    public void setRewardClaimed(boolean rewardClaimed) {
        this.rewardClaimed = rewardClaimed;
    }

    // Вспомогательный метод для получения процента выполнения
    public int getProgressPercent() {
        if (targetAmount <= 0) return 0;
        return (int) ((currentAmount / targetAmount) * 100);
    }

    // Вспомогательный метод для форматированного отображения
    public String getFormattedProgress() {
        return String.format("%.0f / %.0f ₽", currentAmount, targetAmount);
    }
}