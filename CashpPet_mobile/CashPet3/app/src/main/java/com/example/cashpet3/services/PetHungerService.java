package com.example.cashpet3.services;

import android.app.Service;
import android.content.Intent;
import android.os.Handler;
import android.os.IBinder;
import android.os.Looper;

import androidx.annotation.Nullable;

import com.example.cashpet3.api.ApiClient;
import com.example.cashpet3.api.ApiService;
import com.example.cashpet3.api.models.PetStatus;
import com.example.cashpet3.utils.NotificationHelper;
import com.example.cashpet3.utils.SessionManager;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class PetHungerService extends Service {

    private Handler handler;
    private Runnable hungerRunnable;
    private SessionManager sessionManager;
    private NotificationHelper notificationHelper;
    private ApiService apiService;

    @Override
    public void onCreate() {
        super.onCreate();
        sessionManager = new SessionManager(this);
        notificationHelper = new NotificationHelper(this);
        apiService = ApiClient.getApiService();
        handler = new Handler(Looper.getMainLooper());

        // Проверяем статус питомца каждые 30 секунд
        hungerRunnable = new Runnable() {
            @Override
            public void run() {
                checkPetStatus();
                handler.postDelayed(this, 30000); // 30 секунд
            }
        };
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        handler.post(hungerRunnable);
        return START_STICKY;
    }

    private void checkPetStatus() {
        int userId = sessionManager.getCurrentUserId();
        if (userId == -1) {
            stopSelf();
            return;
        }

        apiService.getPetStatus(userId).enqueue(new Callback<PetStatus>() {
            @Override
            public void onResponse(Call<PetStatus> call, Response<PetStatus> response) {
                if (response.isSuccessful() && response.body() != null) {
                    PetStatus status = response.body();

                    // Обновляем локальные данные
                    sessionManager.updateFoodCurrency(status.getFoodCurrency());
                    sessionManager.updatePetEnergy(status.getPetEnergy());

                    // Показываем уведомление если питомец голоден
                    if (status.isHungry()) {
                        notificationHelper.showHungerNotification();
                    } else if (status.getPetEnergy() < 30) {
                        notificationHelper.showLowEnergyNotification(status.getPetEnergy());
                    }
                }
            }

            @Override
            public void onFailure(Call<PetStatus> call, Throwable t) {
                // Ошибка сети - игнорируем
            }
        });
    }

    @Override
    public void onDestroy() {
        super.onDestroy();
        if (handler != null && hungerRunnable != null) {
            handler.removeCallbacks(hungerRunnable);
        }
    }

    @Nullable
    @Override
    public IBinder onBind(Intent intent) {
        return null;
    }
}