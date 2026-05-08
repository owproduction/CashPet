package com.example.cashppet_mobile.models;

import com.google.gson.annotations.SerializedName;

public class Transaction {
    @SerializedName("transaction_id")
    private int transactionId;
    @SerializedName("user_id")
    private int userId;
    private double amount;
    private String type;
    private String category;
    private String date;
    private String description;

    public int getTransactionId() { return transactionId; }
    public void setTransactionId(int transactionId) { this.transactionId = transactionId; }

    public int getUserId() { return userId; }
    public void setUserId(int userId) { this.userId = userId; }

    public double getAmount() { return amount; }
    public void setAmount(double amount) { this.amount = amount; }

    public String getType() { return type; }
    public void setType(String type) { this.type = type; }

    public String getCategory() { return category; }
    public void setCategory(String category) { this.category = category; }

    public String getDate() { return date; }
    public void setDate(String date) { this.date = date; }

    public String getDescription() { return description; }
    public void setDescription(String description) { this.description = description; }
}