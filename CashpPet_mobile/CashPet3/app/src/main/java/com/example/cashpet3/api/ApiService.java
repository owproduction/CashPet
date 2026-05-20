package com.example.cashpet3.api;

import com.example.cashpet3.api.models.FeedResult;
import com.example.cashpet3.api.models.Goal;
import com.example.cashpet3.api.models.PetStatus;
import com.example.cashpet3.api.models.StatsResponse;
import com.example.cashpet3.api.models.Transaction;
import com.example.cashpet3.api.models.User;

import java.util.List;

import okhttp3.ResponseBody;
import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.DELETE;
import retrofit2.http.GET;
import retrofit2.http.POST;
import retrofit2.http.PUT;
import retrofit2.http.Path;
import retrofit2.http.Query;

public interface ApiService {

    // ========== ПОЛЬЗОВАТЕЛИ ==========

    @GET("/users/")
    Call<List<User>> getUsers();

    @GET("/users/{user_id}")
    Call<User> getUser(@Path("user_id") int userId);

    @POST("/users/")
    Call<User> createUser(@Body User user);

    @PUT("/users/{user_id}")
    Call<User> updateUser(@Path("user_id") int userId, @Body User user);

    @DELETE("/users/{user_id}")
    Call<ResponseBody> deleteUser(@Path("user_id") int userId);

    // ========== РАСХОДЫ ==========

    @POST("/expenses/")
    Call<ResponseBody> createExpense(@Body ExpenseRequest expense);

    @GET("/expenses/")
    Call<List<ResponseBody>> getExpenses(
            @Query("user_id") Integer userId,
            @Query("start_date") String startDate,
            @Query("end_date") String endDate
    );

    // ========== ДОХОДЫ ==========

    @POST("/incomes/")
    Call<ResponseBody> createIncome(@Body IncomeRequest income);

    @GET("/incomes/")
    Call<List<ResponseBody>> getIncomes(
            @Query("user_id") Integer userId,
            @Query("start_date") String startDate,
            @Query("end_date") String endDate
    );

    // ========== ЦЕЛИ ==========

    @GET("/goals/")
    Call<List<Goal>> getGoals(@Query("user_id") Integer userId);

    @POST("/goals/")
    Call<Goal> createGoal(@Body Goal goal);

    @POST("/goals/{goal_id}/add_money")
    Call<Goal> addMoneyToGoal(
            @Path("goal_id") int goalId,
            @Query("amount") double amount,
            @Query("user_id") int userId
    );

    @POST("/goals/{goal_id}/claim_reward")
    Call<ResponseBody> claimGoalReward(
            @Path("goal_id") int goalId,
            @Query("user_id") int userId
    );

    // ========== ТРАНЗАКЦИИ ==========

    @GET("/transactions/")
    Call<List<Transaction>> getTransactions(
            @Query("user_id") int userId,
            @Query("days") Integer days
    );

    @GET("/transactions/stats/{user_id}")
    Call<StatsResponse> getTransactionStats(@Path("user_id") int userId);

    // ========== ПИТОМЕЦ ==========

    @POST("/pet/feed")
    Call<FeedResult> feedPet(
            @Query("user_id") int userId,
            @Query("food_amount") int foodAmount
    );

    @GET("/pet/status/{user_id}")
    Call<PetStatus> getPetStatus(@Path("user_id") int userId);

    // ========== ТЕСТОВЫЕ ДАННЫЕ ==========

    @POST("/test/create_sample_user")
    Call<ResponseBody> createSampleUser();

    // ========== ВНУТРЕННИЕ КЛАССЫ ДЛЯ ЗАПРОСОВ ==========

    class ExpenseRequest {
        private int user_id;
        private double amount;
        private String category;
        private String description;
        private String date;
        private boolean is_planned;

        public ExpenseRequest(int user_id, double amount, String category, String description, String date) {
            this.user_id = user_id;
            this.amount = amount;
            this.category = category;
            this.description = description;
            this.date = date;
            this.is_planned = false;
        }

        // Getters
        public int getUser_id() { return user_id; }
        public double getAmount() { return amount; }
        public String getCategory() { return category; }
        public String getDescription() { return description; }
        public String getDate() { return date; }
        public boolean isIs_planned() { return is_planned; }
    }

    class IncomeRequest {
        private int user_id;
        private double amount;
        private String source;
        private String date;
        private boolean is_recurring;

        public IncomeRequest(int user_id, double amount, String source, String date) {
            this.user_id = user_id;
            this.amount = amount;
            this.source = source;
            this.date = date;
            this.is_recurring = false;
        }

        // Getters
        public int getUser_id() { return user_id; }
        public double getAmount() { return amount; }
        public String getSource() { return source; }
        public String getDate() { return date; }
        public boolean isIs_recurring() { return is_recurring; }
    }
}