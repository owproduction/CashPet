package com.example.cashppet_mobile.api;

import com.example.cashppet_mobile.models.*;

import java.util.List;

import retrofit2.Call;
import retrofit2.http.*;

public interface ApiInterface {

    @GET("users/")
    Call<List<User>> getUsers();

    @GET("users/{userId}")
    Call<User> getUser(@Path("userId") int userId);

    @POST("users/")
    Call<User> createUser(@Body User user);

    @POST("expenses/")
    Call<Transaction> createExpense(@Body Transaction transaction);

    @POST("incomes/")
    Call<Transaction> createIncome(@Body Transaction transaction);

    @GET("goals/")
    Call<List<Goal>> getGoals(@Query("user_id") int userId);

    @POST("goals/")
    Call<Goal> createGoal(@Body Goal goal);

    @POST("goals/{goalId}/add_money")
    Call<Goal> addMoneyToGoal(@Path("goalId") int goalId, @Query("amount") double amount, @Query("user_id") int userId);

    @POST("goals/{goalId}/claim_reward")
    Call<Object> claimReward(@Path("goalId") int goalId, @Query("user_id") int userId);

    @GET("transactions/")
    Call<List<Transaction>> getTransactions(@Query("user_id") int userId, @Query("days") int days);

    @POST("pet/feed")
    Call<FeedResult> feedPet(@Query("user_id") int userId, @Query("food_amount") int foodAmount);

    @GET("pet/status/{userId}")
    Call<PetStatus> getPetStatus(@Path("userId") int userId);

    class FeedResult {
        public int food_currency;
        public int pet_energy;
        public int bonus;
    }
}