package com.financial.tamagotchi;

import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;

import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.Toolbar;

public class MainActivity extends AppCompatActivity {

    private TextView textViewWelcome;
    private TextView textViewBalance;
    private TextView textViewFood;
    private Button buttonExpense;
    private Button buttonIncome;
    private Button buttonGoals;
    private Button buttonCharts;
    private Button buttonPet;
    private Button buttonProfile;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        // Инициализация view элементов
        initViews();

        // Настройка тулбара
        setupToolbar();

        // Установка тестовых данных
        setTestData();

        // Настройка обработчиков кнопок
        setupClickListeners();
    }

    private void initViews() {
        textViewWelcome = findViewById(R.id.textViewWelcome);
        textViewBalance = findViewById(R.id.textViewBalance);
        textViewFood = findViewById(R.id.textViewFood);
        buttonExpense = findViewById(R.id.buttonExpense);
        buttonIncome = findViewById(R.id.buttonIncome);
        buttonGoals = findViewById(R.id.buttonGoals);
        buttonCharts = findViewById(R.id.buttonCharts);
        buttonPet = findViewById(R.id.buttonPet);
        buttonProfile = findViewById(R.id.buttonProfile);
    }

    private void setupToolbar() {
        Toolbar toolbar = findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);
        if (getSupportActionBar() != null) {
            getSupportActionBar().setTitle("Финансовый Тамагоччи");
        }
    }

    private void setTestData() {
        textViewWelcome.setText("Привет, Игрок!");
        textViewBalance.setText("🍯 15,000 ₽");
        textViewFood.setText("🥕 100");
    }

    private void setupClickListeners() {
        // Пока просто показываем тосты, позже заменим на реальные активности
        buttonExpense.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                android.widget.Toast.makeText(MainActivity.this,
                        "Добавить трату", android.widget.Toast.LENGTH_SHORT).show();
            }
        });

        buttonIncome.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                android.widget.Toast.makeText(MainActivity.this,
                        "Добавить доход", android.widget.Toast.LENGTH_SHORT).show();
            }
        });

        buttonGoals.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                android.widget.Toast.makeText(MainActivity.this,
                        "Мои цели", android.widget.Toast.LENGTH_SHORT).show();
            }
        });

        buttonCharts.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                android.widget.Toast.makeText(MainActivity.this,
                        "Графики", android.widget.Toast.LENGTH_SHORT).show();
            }
        });

        buttonPet.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                android.widget.Toast.makeText(MainActivity.this,
                        "Мой питомец", android.widget.Toast.LENGTH_SHORT).show();
            }
        });

        buttonProfile.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                android.widget.Toast.makeText(MainActivity.this,
                        "Профиль", android.widget.Toast.LENGTH_SHORT).show();
            }
        });
    }
}