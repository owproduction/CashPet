package com.example.cashpet3.utils;

import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.content.Context;
import android.os.Build;

import androidx.core.app.NotificationCompat;

import com.example.cashpet3.R;

public class NotificationHelper {

    private static final String CHANNEL_ID = "pet_hunger_channel";
    private static final String CHANNEL_NAME = "Тамагоччи";
    private static final String CHANNEL_DESCRIPTION = "Уведомления о состоянии питомца";

    private Context context;
    private NotificationManager notificationManager;

    public NotificationHelper(Context context) {
        this.context = context;
        this.notificationManager = (NotificationManager) context.getSystemService(Context.NOTIFICATION_SERVICE);
        createNotificationChannel();
    }

    private void createNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            NotificationChannel channel = new NotificationChannel(
                    CHANNEL_ID,
                    CHANNEL_NAME,
                    NotificationManager.IMPORTANCE_HIGH
            );
            channel.setDescription(CHANNEL_DESCRIPTION);
            channel.enableLights(true);
            channel.enableVibration(true);
            notificationManager.createNotificationChannel(channel);
        }
    }

    // Показать уведомление о голоде питомца
    public void showHungerNotification() {
        NotificationCompat.Builder builder = new NotificationCompat.Builder(context, CHANNEL_ID)
                .setSmallIcon(android.R.drawable.ic_dialog_alert)
                .setContentTitle("🐹 Питомец голоден!")
                .setContentText("Покормите своего финансового хомяка, чтобы он не потерял энергию!")
                .setPriority(NotificationCompat.PRIORITY_HIGH)
                .setAutoCancel(true)
                .setVibrate(new long[]{0, 500, 200, 500});

        notificationManager.notify(1, builder.build());
    }

    // Показать уведомление о низкой энергии
    public void showLowEnergyNotification(int energyPercent) {
        NotificationCompat.Builder builder = new NotificationCompat.Builder(context, CHANNEL_ID)
                .setSmallIcon(android.R.drawable.ic_dialog_alert)
                .setContentTitle("⚠️ Низкая энергия питомца")
                .setContentText("Энергия: " + energyPercent + "%! Покормите питомца!")
                .setPriority(NotificationCompat.PRIORITY_HIGH)
                .setAutoCancel(true);

        notificationManager.notify(2, builder.build());
    }

    // Показать уведомление о получении награды
    public void showRewardNotification(String goalName, int rewardAmount) {
        NotificationCompat.Builder builder = new NotificationCompat.Builder(context, CHANNEL_ID)
                .setSmallIcon(android.R.drawable.stat_notify_chat)
                .setContentTitle("🎉 Цель достигнута!")
                .setContentText("Цель \"" + goalName + "\" выполнена! Получено " + rewardAmount + " корма!")
                .setPriority(NotificationCompat.PRIORITY_DEFAULT)
                .setAutoCancel(true);

        notificationManager.notify(3, builder.build());
    }

    // Показать уведомление о пополнении баланса
    public void showIncomeNotification(double amount, String source) {
        NotificationCompat.Builder builder = new NotificationCompat.Builder(context, CHANNEL_ID)
                .setSmallIcon(android.R.drawable.stat_notify_chat)
                .setContentTitle("💰 Поступление средств")
                .setContentText("+" + String.format("%.2f", amount) + " ₽ от " + source)
                .setPriority(NotificationCompat.PRIORITY_DEFAULT)
                .setAutoCancel(true);

        notificationManager.notify(4, builder.build());
    }

    // Показать уведомление о расходе
    public void showExpenseNotification(double amount, String category) {
        NotificationCompat.Builder builder = new NotificationCompat.Builder(context, CHANNEL_ID)
                .setSmallIcon(android.R.drawable.stat_notify_chat)
                .setContentTitle("💸 Совершён расход")
                .setContentText("-" + String.format("%.2f", amount) + " ₽ на " + category)
                .setPriority(NotificationCompat.PRIORITY_DEFAULT)
                .setAutoCancel(true);

        notificationManager.notify(5, builder.build());
    }

    // Показать уведомление о бонусе при кормлении
    public void showBonusNotification(int bonusAmount) {
        NotificationCompat.Builder builder = new NotificationCompat.Builder(context, CHANNEL_ID)
                .setSmallIcon(android.R.drawable.stat_notify_chat)
                .setContentTitle("🎁 Бонусный корм!")
                .setContentText("Вы получили +" + bonusAmount + " корма за кормление питомца!")
                .setPriority(NotificationCompat.PRIORITY_DEFAULT)
                .setAutoCancel(true);

        notificationManager.notify(6, builder.build());
    }

    // Показать общее уведомление
    public void showNotification(String title, String message) {
        NotificationCompat.Builder builder = new NotificationCompat.Builder(context, CHANNEL_ID)
                .setSmallIcon(android.R.drawable.stat_notify_chat)
                .setContentTitle(title)
                .setContentText(message)
                .setPriority(NotificationCompat.PRIORITY_DEFAULT)
                .setAutoCancel(true);

        notificationManager.notify((int) System.currentTimeMillis(), builder.build());
    }
}