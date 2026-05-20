package com.example.cashpet3.utils;

import android.view.View;
import android.view.animation.Animation;
import android.view.animation.AnimationSet;
import android.view.animation.ScaleAnimation;
import android.view.animation.TranslateAnimation;
import android.widget.ImageView;

public class AnimationHelper {

    // Анимация пульсации для питомца
    public static void startPetPulseAnimation(View petView) {
        AnimationSet animationSet = new AnimationSet(true);

        ScaleAnimation scaleAnimation = new ScaleAnimation(
                1.0f, 1.05f,
                1.0f, 1.05f,
                Animation.RELATIVE_TO_SELF, 0.5f,
                Animation.RELATIVE_TO_SELF, 0.5f
        );
        scaleAnimation.setDuration(1000);
        scaleAnimation.setRepeatMode(Animation.REVERSE);
        scaleAnimation.setRepeatCount(Animation.INFINITE);

        animationSet.addAnimation(scaleAnimation);
        petView.startAnimation(animationSet);
    }

    // Анимация нажатия на кнопку
    public static void animateButtonPress(View button, Runnable onEnd) {
        AnimationSet animationSet = new AnimationSet(true);

        ScaleAnimation scaleAnimation = new ScaleAnimation(
                1.0f, 0.95f,
                1.0f, 0.95f,
                Animation.RELATIVE_TO_SELF, 0.5f,
                Animation.RELATIVE_TO_SELF, 0.5f
        );
        scaleAnimation.setDuration(100);
        scaleAnimation.setRepeatMode(Animation.REVERSE);
        scaleAnimation.setRepeatCount(1);

        scaleAnimation.setAnimationListener(new Animation.AnimationListener() {
            @Override
            public void onAnimationStart(Animation animation) {}

            @Override
            public void onAnimationEnd(Animation animation) {
                if (onEnd != null) {
                    onEnd.run();
                }
            }

            @Override
            public void onAnimationRepeat(Animation animation) {}
        });

        animationSet.addAnimation(scaleAnimation);
        button.startAnimation(animationSet);
    }

    // Анимация выезжания меню
    public static void slideMenu(View menuView, boolean expand, int duration) {
        Animation animation;

        if (expand) {
            animation = new TranslateAnimation(
                    -menuView.getWidth(), 0,
                    0, 0
            );
        } else {
            animation = new TranslateAnimation(
                    0, -menuView.getWidth(),
                    0, 0
            );
        }

        animation.setDuration(duration);
        animation.setFillAfter(true);
        menuView.startAnimation(animation);

        if (expand) {
            menuView.setVisibility(View.VISIBLE);
        } else {
            animation.setAnimationListener(new Animation.AnimationListener() {
                @Override
                public void onAnimationStart(Animation animation) {}

                @Override
                public void onAnimationEnd(Animation animation) {
                    menuView.setVisibility(View.GONE);
                }

                @Override
                public void onAnimationRepeat(Animation animation) {}
            });
        }
    }

    // Анимация появления уведомления
    public static void fadeInNotification(View notificationView) {
        notificationView.setVisibility(View.VISIBLE);
        notificationView.setAlpha(0f);
        notificationView.animate()
                .alpha(1f)
                .setDuration(300)
                .start();
    }

    // Анимация исчезновения уведомления
    public static void fadeOutNotification(View notificationView, Runnable onEnd) {
        notificationView.animate()
                .alpha(0f)
                .setDuration(300)
                .withEndAction(onEnd)
                .start();
    }

    // Анимация поедания корма (для питомца)
    public static void feedAnimation(ImageView petImage, Runnable onEnd) {
        AnimationSet animationSet = new AnimationSet(true);

        ScaleAnimation scaleAnimation = new ScaleAnimation(
                1.0f, 1.2f,
                1.0f, 1.2f,
                Animation.RELATIVE_TO_SELF, 0.5f,
                Animation.RELATIVE_TO_SELF, 0.5f
        );
        scaleAnimation.setDuration(200);
        scaleAnimation.setRepeatMode(Animation.REVERSE);
        scaleAnimation.setRepeatCount(1);

        scaleAnimation.setAnimationListener(new Animation.AnimationListener() {
            @Override
            public void onAnimationStart(Animation animation) {}

            @Override
            public void onAnimationEnd(Animation animation) {
                if (onEnd != null) {
                    onEnd.run();
                }
            }

            @Override
            public void onAnimationRepeat(Animation animation) {}
        });

        animationSet.addAnimation(scaleAnimation);
        petImage.startAnimation(animationSet);
    }

    // Анимация загрузки
    public static void startLoadingAnimation(View loadingView) {
        loadingView.setVisibility(View.VISIBLE);
        loadingView.animate()
                .alpha(1f)
                .setDuration(200)
                .start();
    }

    // Остановка анимации загрузки
    public static void stopLoadingAnimation(View loadingView) {
        loadingView.animate()
                .alpha(0f)
                .setDuration(200)
                .withEndAction(() -> loadingView.setVisibility(View.GONE))
                .start();
    }
}