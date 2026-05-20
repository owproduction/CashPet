package com.example.cashpet3.api;

import okhttp3.OkHttpClient;
import okhttp3.logging.HttpLoggingInterceptor;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

import java.util.concurrent.TimeUnit;

public class ApiClient {

    // Для эмулятора Android используем 10.0.2.2 вместо localhost
    // Для реального устройства - IP вашего компьютера в локальной сети
    private static final String BASE_URL = "http://10.0.2.2:8000/";

    private static OkHttpClient okHttpClient = null;
    private static Retrofit retrofit = null;
    private static ApiService apiService = null;

    private static OkHttpClient getOkHttpClient() {
        if (okHttpClient == null) {
            // Настройка логирования запросов (для отладки)
            HttpLoggingInterceptor loggingInterceptor = new HttpLoggingInterceptor();
            loggingInterceptor.setLevel(HttpLoggingInterceptor.Level.BODY);

            // Настройка OkHttp клиента
            okHttpClient = new OkHttpClient.Builder()
                    .connectTimeout(30, TimeUnit.SECONDS)
                    .readTimeout(30, TimeUnit.SECONDS)
                    .writeTimeout(30, TimeUnit.SECONDS)
                    .addInterceptor(loggingInterceptor)
                    .build();
        }
        return okHttpClient;
    }

    public static Retrofit getClient() {
        if (retrofit == null) {
            // Настройка Retrofit
            retrofit = new Retrofit.Builder()
                    .baseUrl(BASE_URL)
                    .client(getOkHttpClient())  // Исправлено: передаем OkHttpClient, а не Retrofit
                    .addConverterFactory(GsonConverterFactory.create())
                    .build();
        }
        return retrofit;
    }

    public static ApiService getApiService() {
        if (apiService == null) {
            apiService = getClient().create(ApiService.class);
        }
        return apiService;
    }

    // Для смены базового URL (например, для реального устройства)
    public static void setBaseUrl(String baseUrl) {
        // Создаем новый Retrofit с новым URL, но с тем же OkHttpClient
        retrofit = new Retrofit.Builder()
                .baseUrl(baseUrl)
                .client(getOkHttpClient())
                .addConverterFactory(GsonConverterFactory.create())
                .build();
        apiService = retrofit.create(ApiService.class);
    }

    // Получить текущий базовый URL
    public static String getBaseUrl() {
        return BASE_URL;
    }
}