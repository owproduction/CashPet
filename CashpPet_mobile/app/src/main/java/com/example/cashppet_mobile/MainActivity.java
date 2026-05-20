package com.example.cashppet_mobile;

import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.view.View;
import android.view.animation.Animation;
import android.view.animation.AnimationUtils;
import android.widget.*;
import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.Toolbar;
import androidx.cardview.widget.CardView;

import com.example.cashppet_mobile.models.User;
import com.example.cashppet_mobile.utils.DataManager;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.card.MaterialCardView;
import com.google.android.material.progressindicator.LinearProgressIndicator;
import com.google.android.material.textfield.TextInputEditText;
import com.google.android.material.textfield.MaterialAutoCompleteTextView;

import java.util.Locale;

public class MainActivity extends AppCompatActivity implements DataManager.OnDataChangedListener, DataManager.OnNotificationListener {

    private TextView tvWelcome, tvBalance, tvFoodCurrency, tvPetEmoji, tvMoodText, tvEnergyPercent;
    private LinearProgressIndicator progressEnergy;
    private CardView petContainer;
    private MaterialButton btnFeedPet;
    private LinearLayout layoutNotification;
    private TextView tvNotification;
    private MaterialCardView btnAddExpense, btnAddIncome;
    private Button btnCloseNotification;

    private DataManager dataManager;
    private Animation petAnimation;
    private Handler handler;
    private Runnable hideNotificationRunnable;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        initViews();
        setupToolbar();
        setupDataManager();
        setupListeners();
        startPetAnimation();

        handler = new Handler(Looper.getMainLooper());
        hideNotificationRunnable = () -> layoutNotification.setVisibility(View.GONE);
    }

    private void initViews() {
        tvWelcome = findViewById(R.id.tvWelcome);
        tvBalance = findViewById(R.id.tvBalance);
        tvFoodCurrency = findViewById(R.id.tvFoodCurrency);
        tvPetEmoji = findViewById(R.id.tvPetEmoji);
        tvMoodText = findViewById(R.id.tvMoodText);
        tvEnergyPercent = findViewById(R.id.tvEnergyPercent);
        progressEnergy = findViewById(R.id.progressEnergy);
        petContainer = findViewById(R.id.petContainer);
        btnFeedPet = findViewById(R.id.btnFeedPet);
        layoutNotification = findViewById(R.id.layoutNotification);
        tvNotification = findViewById(R.id.tvNotification);
        btnCloseNotification = findViewById(R.id.btnCloseNotification);
    }

    private void setupToolbar() {
        Toolbar toolbar = findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);
        if (getSupportActionBar() != null) {
            getSupportActionBar().setTitle("Финансовый Тамагоччи");
        }
    }

    private void setupDataManager() {
        // ПЕРЕДАЁМ CONTEXT
        dataManager = DataManager.getInstance(this);
        dataManager.setOnDataChangedListener(this);
        dataManager.setOnNotificationListener(this);

        // Загружаем данные пользователя
        User user = dataManager.getCurrentUser();
        if (user != null) {
            onUserChanged(user);
        }
    }

    private void setupListeners() {
        btnFeedPet.setOnClickListener(v -> {
            User user = dataManager.getCurrentUser();
            if (user != null && user.getFoodCurrency() >= 10) {
                dataManager.feedPet();
            } else {
                showNotification("Недостаточно корма!");
            }
        });

        btnAddExpense.setOnClickListener(v -> showExpenseDialog());
        btnAddIncome.setOnClickListener(v -> showIncomeDialog());

        btnCloseNotification.setOnClickListener(v -> layoutNotification.setVisibility(View.GONE));

        petContainer.setOnClickListener(v -> petContainer.startAnimation(petAnimation));
    }

    private void startPetAnimation() {
        petAnimation = AnimationUtils.loadAnimation(this, androidx.appcompat.R.anim.abc_fade_in);
    }

    private void showExpenseDialog() {
        android.app.AlertDialog.Builder builder = new android.app.AlertDialog.Builder(this);
        View view = getLayoutInflater().inflate(R.layout.dialog_expense, null);

        TextInputEditText etAmount = view.findViewById(R.id.etAmount);
        MaterialAutoCompleteTextView spinnerCategory = view.findViewById(R.id.spinnerCategory);

        String[] categories = getResources().getStringArray(R.array.categories);
        ArrayAdapter<String> adapter = new ArrayAdapter<>(this, android.R.layout.simple_dropdown_item_1line, categories);
        spinnerCategory.setAdapter(adapter);
        spinnerCategory.setText(categories[0], false);

        builder.setView(view);
        android.app.AlertDialog dialog = builder.create();

        view.findViewById(R.id.btnConfirm).setOnClickListener(v -> {
            String amountStr = etAmount.getText().toString();
            if (!amountStr.isEmpty()) {
                double amount = Double.parseDouble(amountStr);
                if (amount > 0) {
                    User user = dataManager.getCurrentUser();
                    if (user != null && amount <= user.getCurrentBalance()) {
                        dataManager.addExpense(amount, spinnerCategory.getText().toString());
                        dialog.dismiss();
                    } else {
                        showNotification("Недостаточно средств!");
                    }
                }
            }
        });

        dialog.show();
    }

    private void showIncomeDialog() {
        android.app.AlertDialog.Builder builder = new android.app.AlertDialog.Builder(this);
        View view = getLayoutInflater().inflate(R.layout.dialog_income, null);

        TextInputEditText etAmount = view.findViewById(R.id.etAmount);
        MaterialAutoCompleteTextView spinnerSource = view.findViewById(R.id.spinnerSource);

        String[] sources = getResources().getStringArray(R.array.sources);
        ArrayAdapter<String> adapter = new ArrayAdapter<>(this, android.R.layout.simple_dropdown_item_1line, sources);
        spinnerSource.setAdapter(adapter);
        spinnerSource.setText(sources[0], false);

        builder.setView(view);
        android.app.AlertDialog dialog = builder.create();

        view.findViewById(R.id.btnConfirm).setOnClickListener(v -> {
            String amountStr = etAmount.getText().toString();
            if (!amountStr.isEmpty()) {
                double amount = Double.parseDouble(amountStr);
                if (amount > 0) {
                    dataManager.addIncome(amount, spinnerSource.getText().toString());
                    dialog.dismiss();
                }
            }
        });

        dialog.show();
    }

    @Override
    public void onUserChanged(User user) {
        if (user != null) {
            tvWelcome.setText("Добро пожаловать, " + user.getName() + "!");
            tvBalance.setText(String.format(Locale.getDefault(), "%,.0f ₽", user.getCurrentBalance()));
            tvFoodCurrency.setText(String.valueOf(user.getFoodCurrency()));

            int energy = user.getPetEnergy();
            progressEnergy.setProgress(energy);
            tvEnergyPercent.setText(energy + "%");

            if (energy <= 0) {
                tvPetEmoji.setText("😴");
                tvMoodText.setText("Уснул");
            } else if (energy <= 20) {
                tvPetEmoji.setText("😢");
                tvMoodText.setText("Очень голоден!");
            } else if (energy <= 40) {
                tvPetEmoji.setText("😕");
                tvMoodText.setText("Хочет кушать");
            } else if (energy <= 60) {
                tvPetEmoji.setText("😐");
                tvMoodText.setText("Нормально");
            } else if (energy <= 80) {
                tvPetEmoji.setText("😊");
                tvMoodText.setText("Хорошо!");
            } else {
                tvPetEmoji.setText("😄");
                tvMoodText.setText("Отлично!");
            }
        }
    }

    @Override
    public void onNotification(String message) {
        showNotification(message);
    }

    private void showNotification(String message) {
        if (handler != null) handler.removeCallbacks(hideNotificationRunnable);
        tvNotification.setText(message);
        layoutNotification.setVisibility(View.VISIBLE);
        handler.postDelayed(hideNotificationRunnable, 5000);
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        if (handler != null) handler.removeCallbacks(hideNotificationRunnable);
    }
}