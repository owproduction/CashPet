package com.example.cashpet3.api.models;

import com.google.gson.annotations.SerializedName;
import java.util.List;

public class StatsResponse {

    @SerializedName("totals")
    private Totals totals;

    @SerializedName("daily")
    private List<DailyStats> daily;

    @SerializedName("categories")
    private List<CategoryStats> categories;

    // Конструкторы
    public StatsResponse() {}

    public StatsResponse(Totals totals, List<DailyStats> daily, List<CategoryStats> categories) {
        this.totals = totals;
        this.daily = daily;
        this.categories = categories;
    }

    // Getters и Setters
    public Totals getTotals() {
        return totals;
    }

    public void setTotals(Totals totals) {
        this.totals = totals;
    }

    public List<DailyStats> getDaily() {
        return daily;
    }

    public void setDaily(List<DailyStats> daily) {
        this.daily = daily;
    }

    public List<CategoryStats> getCategories() {
        return categories;
    }

    public void setCategories(List<CategoryStats> categories) {
        this.categories = categories;
    }

    // ========== ВСПОМОГАТЕЛЬНЫЙ КЛАСС TOTALS ==========
    public static class Totals {
        @SerializedName("total_income")
        private double totalIncome;

        @SerializedName("total_expense")
        private double totalExpense;

        public Totals() {}

        public double getTotalIncome() {
            return totalIncome;
        }

        public void setTotalIncome(double totalIncome) {
            this.totalIncome = totalIncome;
        }

        public double getTotalExpense() {
            return totalExpense;
        }

        public void setTotalExpense(double totalExpense) {
            this.totalExpense = totalExpense;
        }

        public double getNetSavings() {
            return totalIncome - totalExpense;
        }

        public String getFormattedIncome() {
            return String.format("+,2f ₽", totalIncome);
        }

        public String getFormattedExpense() {
            return String.format("-,2f ₽", totalExpense);
        }
    }

    // ========== ВСПОМОГАТЕЛЬНЫЙ КЛАСС DAILY STATS ==========
    public static class DailyStats {
        @SerializedName("day")
        private String day;

        @SerializedName("income")
        private double income;

        @SerializedName("expense")
        private double expense;

        public DailyStats() {}

        public String getDay() {
            return day;
        }

        public void setDay(String day) {
            this.day = day;
        }

        public double getIncome() {
            return income;
        }

        public void setIncome(double income) {
            this.income = income;
        }

        public double getExpense() {
            return expense;
        }

        public void setExpense(double expense) {
            this.expense = expense;
        }

        public double getBalance() {
            return income - expense;
        }
    }

    // ========== ВСПОМОГАТЕЛЬНЫЙ КЛАСС CATEGORY STATS ==========
    public static class CategoryStats {
        @SerializedName("category")
        private String category;

        @SerializedName("total")
        private double total;

        public CategoryStats() {}

        public String getCategory() {
            return category;
        }

        public void setCategory(String category) {
            this.category = category;
        }

        public double getTotal() {
            return total;
        }

        public void setTotal(double total) {
            this.total = total;
        }

        public String getFormattedTotal() {
            return String.format(",2f ₽", total);
        }
    }
}