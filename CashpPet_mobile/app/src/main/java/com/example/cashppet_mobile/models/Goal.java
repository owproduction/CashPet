package com.example.cashppet_mobile.models;

import com.google.gson.annotations.SerializedName;

public class Goal {
    @SerializedName("goal_id")
    private int goalId;
    @SerializedName("user_id")
    private int userId;
    @SerializedName("target_amount")
    private double targetAmount;
    @SerializedName("current_amount")
    private double currentAmount;
    private String name;
    private String deadline;
    @SerializedName("is_completed")
    private boolean isCompleted;
    @SerializedName("reward_amount")
    private int rewardAmount;
    @SerializedName("reward_claimed")
    private boolean rewardClaimed;

    public int getGoalId() { return goalId; }
    public void setGoalId(int goalId) { this.goalId = goalId; }

    public int getUserId() { return userId; }
    public void setUserId(int userId) { this.userId = userId; }

    public double getTargetAmount() { return targetAmount; }
    public void setTargetAmount(double targetAmount) { this.targetAmount = targetAmount; }

    public double getCurrentAmount() { return currentAmount; }
    public void setCurrentAmount(double currentAmount) { this.currentAmount = currentAmount; }

    public String getName() { return name; }
    public void setName(String name) { this.name = name; }

    public String getDeadline() { return deadline; }
    public void setDeadline(String deadline) { this.deadline = deadline; }

    public boolean isCompleted() { return isCompleted; }
    public void setCompleted(boolean completed) { isCompleted = completed; }

    public int getRewardAmount() { return rewardAmount; }
    public void setRewardAmount(int rewardAmount) { this.rewardAmount = rewardAmount; }

    public boolean isRewardClaimed() { return rewardClaimed; }
    public void setRewardClaimed(boolean rewardClaimed) { this.rewardClaimed = rewardClaimed; }
}