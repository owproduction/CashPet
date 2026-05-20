package com.example.cashpet3.api.models;

import com.google.gson.annotations.SerializedName;

import java.util.Date;

public class Transaction {

    @SerializedName("transaction_id")
    private int transactionId;

    @SerializedName("user_id")
    private int userId;

    @SerializedName("amount")
    private double amount;

    @SerializedName("type")
    private String type; // "income" или "expense"

    @SerializedName("category")
    private String category;

    @SerializedName("date")
    private Date date;

    @SerializedName("description")
    private String description;

    // Конструкторы
    public Transaction() {}

    public Transaction(int userId, double amount, String type, String category, Date date) {
        this.userId = userId;
        this.amount = amount;
        this.type = type;
        this.category = category;
        this.date = date;
        this.description = "";
    }

    // Getters и Setters
    public int getTransactionId() {
        return transactionId;
    }

    public void setTransactionId(int transactionId) {
        this.transactionId = transactionId;
    }

    public int getUserId() {
        return userId;
    }

    public void setUserId(int userId) {
        this.userId = userId;
    }

    public double getAmount() {
        return amount;
    }

    public void setAmount(double amount) {
        this.amount = amount;
    }

    public String getType() {
        return type;
    }

    public void setType(String type) {
        this.type = type;
    }

    public String getCategory() {
        return category;
    }

    public void setCategory(String category) {
        this.category = category;
    }

    public Date getDate() {
        return date;
    }

    public void setDate(Date date) {
        this.date = date;
    }

    public String getDescription() {
        return description;
    }

    public void setDescription(String description) {
        this.description = description;
    }

    // Вспомогательные методы
    public boolean isIncome() {
        return "income".equals(type);
    }

    public boolean isExpense() {
        return "expense".equals(type);
    }

    public String getFormattedAmount() {
        if (isIncome()) {
            return "+" + String.format("%.2f", amount) + " ₽";
        } else {
            return "-" + String.format("%.2f", amount) + " ₽";
        }
    }

    public int getIconEmoji() {
        // Можно вернуть эмодзи в зависимости от категории
        // Но для простоты оставляем как есть
        return 0;
    }
}