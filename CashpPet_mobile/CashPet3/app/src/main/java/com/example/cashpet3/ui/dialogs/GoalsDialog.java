package com.example.cashpet3.ui.dialogs;

import android.app.Dialog;
import android.os.Bundle;
import android.text.TextUtils;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.ProgressBar;
import android.widget.ScrollView;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.appcompat.app.AlertDialog;
import androidx.fragment.app.DialogFragment;

import com.example.cashpet3.R;
import com.example.cashpet3.api.ApiClient;
import com.example.cashpet3.api.ApiService;
import com.example.cashpet3.api.models.Goal;
import com.example.cashpet3.api.models.User;
import com.example.cashpet3.utils.SessionManager;

import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.List;
import java.util.Locale;

import okhttp3.ResponseBody;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;


public class GoalsDialog extends DialogFragment {

    private LinearLayout goalsContainer;
    private Button btnCreateGoal;
    private ProgressBar progressBar;
    private TextView tvEmpty;

    private SessionManager sessionManager;
    private ApiService apiService;
    private User currentUser;
    private List<Goal> goals;
    private GoalsListener listener;

    public interface GoalsListener {
        void onGoalsUpdated();
    }

    public static GoalsDialog newInstance(GoalsListener listener) {
        GoalsDialog dialog = new GoalsDialog();
        dialog.listener = listener;
        return dialog;
    }

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        sessionManager = new SessionManager(requireContext());
        apiService = ApiClient.getApiService();
        currentUser = sessionManager.getUser();
    }

    @NonNull
    @Override
    public Dialog onCreateDialog(@Nullable Bundle savedInstanceState) {
        AlertDialog.Builder builder = new AlertDialog.Builder(requireActivity());

        LayoutInflater inflater = requireActivity().getLayoutInflater();
        View view = inflater.inflate(R.layout.dialog_goals, null);

        initViews(view);
        setupListeners();

        builder.setView(view);
        Dialog dialog = builder.create();
        dialog.setCanceledOnTouchOutside(true);

        loadGoals();

        return dialog;
    }

    private void initViews(View view) {
        goalsContainer = view.findViewById(R.id.goalsContainer);
        btnCreateGoal = view.findViewById(R.id.btnCreateGoal);
        progressBar = view.findViewById(R.id.progressBar);
        tvEmpty = view.findViewById(R.id.tvEmpty);
    }

    private void setupListeners() {
        btnCreateGoal.setOnClickListener(v -> showCreateGoalDialog());
    }

    private void loadGoals() {
        if (currentUser == null) return;

        setLoading(true);

        apiService.getGoals(currentUser.getUserId()).enqueue(new Callback<List<Goal>>() {
            @Override
            public void onResponse(Call<List<Goal>> call, Response<List<Goal>> response) {
                setLoading(false);

                if (response.isSuccessful() && response.body() != null) {
                    goals = response.body();
                    displayGoals();
                } else {
                    Toast.makeText(getContext(), "Ошибка загрузки целей", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<List<Goal>> call, Throwable t) {
                setLoading(false);
                Toast.makeText(getContext(), "Ошибка: " + t.getMessage(), Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void displayGoals() {
        goalsContainer.removeAllViews();

        if (goals == null || goals.isEmpty()) {
            tvEmpty.setVisibility(View.VISIBLE);
            return;
        }

        tvEmpty.setVisibility(View.GONE);

        for (Goal goal : goals) {
            View goalView = createGoalView(goal);
            goalsContainer.addView(goalView);
        }
    }

    private View createGoalView(Goal goal) {
        LayoutInflater inflater = LayoutInflater.from(getContext());
        View view = inflater.inflate(R.layout.item_goal, null);

        TextView tvGoalName = view.findViewById(R.id.tvGoalName);
        TextView tvGoalAmount = view.findViewById(R.id.tvGoalAmount);
        TextView tvGoalProgress = view.findViewById(R.id.tvGoalProgress);
        ProgressBar progressBarGoal = view.findViewById(R.id.progressBarGoal);
        Button btnAddMoney = view.findViewById(R.id.btnAddMoney);
        Button btnClaimReward = view.findViewById(R.id.btnClaimReward);
        TextView tvCompleted = view.findViewById(R.id.tvCompleted);

        tvGoalName.setText(goal.getName());
        tvGoalAmount.setText(String.format(Locale.getDefault(), "Цель: %.0f ₽", goal.getTargetAmount()));
        tvGoalProgress.setText(String.format(Locale.getDefault(), "Накоплено: %.0f ₽", goal.getCurrentAmount()));

        int progress = goal.getProgressPercent();
        progressBarGoal.setProgress(progress);

        if (goal.isCompleted()) {
            tvCompleted.setVisibility(View.VISIBLE);
            btnAddMoney.setVisibility(View.GONE);

            if (!goal.isRewardClaimed()) {
                btnClaimReward.setVisibility(View.VISIBLE);
                btnClaimReward.setText("🎁 Получить награду (" + goal.getRewardAmount() + " корма)");
                btnClaimReward.setOnClickListener(v -> claimReward(goal));
            } else {
                btnClaimReward.setVisibility(View.GONE);
            }
        } else {
            tvCompleted.setVisibility(View.GONE);
            btnClaimReward.setVisibility(View.GONE);
            btnAddMoney.setVisibility(View.VISIBLE);
            btnAddMoney.setOnClickListener(v -> showAddMoneyDialog(goal));
        }

        return view;
    }

    private void showCreateGoalDialog() {
        AlertDialog.Builder builder = new AlertDialog.Builder(requireContext());
        builder.setTitle("🎯 Новая цель");

        View view = LayoutInflater.from(getContext()).inflate(R.layout.dialog_create_goal, null);

        EditText etGoalName = view.findViewById(R.id.etGoalName);
        EditText etTargetAmount = view.findViewById(R.id.etTargetAmount);
        Button btnCreate = view.findViewById(R.id.btnCreate);
        Button btnCancel = view.findViewById(R.id.btnCancel);

        builder.setView(view);
        AlertDialog dialog = builder.create();

        btnCreate.setOnClickListener(v -> {
            String name = etGoalName.getText().toString().trim();
            String amountStr = etTargetAmount.getText().toString().trim();

            if (TextUtils.isEmpty(name)) {
                Toast.makeText(getContext(), "Введите название цели", Toast.LENGTH_SHORT).show();
                return;
            }

            if (TextUtils.isEmpty(amountStr)) {
                Toast.makeText(getContext(), "Введите целевую сумму", Toast.LENGTH_SHORT).show();
                return;
            }

            double targetAmount;
            try {
                targetAmount = Double.parseDouble(amountStr);
                if (targetAmount <= 0) {
                    Toast.makeText(getContext(), "Сумма должна быть больше 0", Toast.LENGTH_SHORT).show();
                    return;
                }
            } catch (NumberFormatException e) {
                Toast.makeText(getContext(), "Некорректная сумма", Toast.LENGTH_SHORT).show();
                return;
            }

            createGoal(name, targetAmount, dialog);
        });

        btnCancel.setOnClickListener(v -> dialog.dismiss());

        dialog.show();
    }

    private void createGoal(String name, double targetAmount, AlertDialog dialog) {
        setLoading(true);

        Goal newGoal = new Goal(currentUser.getUserId(), name, targetAmount);

        apiService.createGoal(newGoal).enqueue(new Callback<Goal>() {
            @Override
            public void onResponse(Call<Goal> call, Response<Goal> response) {
                setLoading(false);

                if (response.isSuccessful() && response.body() != null) {
                    Toast.makeText(getContext(), "🎯 Цель создана: " + name, Toast.LENGTH_SHORT).show();
                    dialog.dismiss();
                    loadGoals();
                    if (listener != null) {
                        listener.onGoalsUpdated();
                    }
                } else {
                    Toast.makeText(getContext(), "Ошибка при создании цели", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<Goal> call, Throwable t) {
                setLoading(false);
                Toast.makeText(getContext(), "Ошибка: " + t.getMessage(), Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void showAddMoneyDialog(Goal goal) {
        AlertDialog.Builder builder = new AlertDialog.Builder(requireContext());
        builder.setTitle("💰 Пополнить цель: " + goal.getName());

        View view = LayoutInflater.from(getContext()).inflate(R.layout.dialog_add_money, null);

        EditText etAmount = view.findViewById(R.id.etAmount);
        Button btnAdd = view.findViewById(R.id.btnAdd);
        Button btnCancel = view.findViewById(R.id.btnCancel);
        TextView tvCurrentBalance = view.findViewById(R.id.tvCurrentBalance);

        tvCurrentBalance.setText(String.format(Locale.getDefault(), "Ваш баланс: %.2f ₽", currentUser.getCurrentBalance()));

        builder.setView(view);
        AlertDialog dialog = builder.create();

        btnAdd.setOnClickListener(v -> {
            String amountStr = etAmount.getText().toString().trim();

            if (TextUtils.isEmpty(amountStr)) {
                Toast.makeText(getContext(), "Введите сумму", Toast.LENGTH_SHORT).show();
                return;
            }

            double amount;
            try {
                amount = Double.parseDouble(amountStr);
                if (amount <= 0) {
                    Toast.makeText(getContext(), "Сумма должна быть больше 0", Toast.LENGTH_SHORT).show();
                    return;
                }
            } catch (NumberFormatException e) {
                Toast.makeText(getContext(), "Некорректная сумма", Toast.LENGTH_SHORT).show();
                return;
            }

            if (amount > currentUser.getCurrentBalance()) {
                Toast.makeText(getContext(), "Недостаточно средств!", Toast.LENGTH_SHORT).show();
                return;
            }

            addMoneyToGoal(goal, amount, dialog);
        });

        btnCancel.setOnClickListener(v -> dialog.dismiss());

        dialog.show();
    }

    private void addMoneyToGoal(Goal goal, double amount, AlertDialog dialog) {
        setLoading(true);

        apiService.addMoneyToGoal(goal.getGoalId(), amount, currentUser.getUserId()).enqueue(new Callback<Goal>() {
            @Override
            public void onResponse(Call<Goal> call, Response<Goal> response) {
                setLoading(false);

                if (response.isSuccessful() && response.body() != null) {
                    Toast.makeText(getContext(), String.format(Locale.getDefault(), "💰 Цель пополнена на %.2f ₽", amount), Toast.LENGTH_SHORT).show();
                    dialog.dismiss();
                    loadGoals();

                    // Обновляем баланс пользователя
                    currentUser.setCurrentBalance(currentUser.getCurrentBalance() - amount);
                    sessionManager.saveUser(currentUser);

                    if (listener != null) {
                        listener.onGoalsUpdated();
                    }
                } else {
                    Toast.makeText(getContext(), "Ошибка при пополнении", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<Goal> call, Throwable t) {
                setLoading(false);
                Toast.makeText(getContext(), "Ошибка: " + t.getMessage(), Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void claimReward(Goal goal) {
        setLoading(true);

        apiService.claimGoalReward(goal.getGoalId(), currentUser.getUserId()).enqueue(new Callback<ResponseBody>() {
            @Override
            public void onResponse(Call<ResponseBody> call, Response<ResponseBody> response) {
                setLoading(false);

                if (response.isSuccessful()) {
                    Toast.makeText(getContext(), "🎉 Награда получена! +" + goal.getRewardAmount() + " корма", Toast.LENGTH_SHORT).show();
                    loadGoals();

                    // Обновляем корм пользователя
                    currentUser.setFoodCurrency(currentUser.getFoodCurrency() + goal.getRewardAmount());
                    sessionManager.saveUser(currentUser);

                    if (listener != null) {
                        listener.onGoalsUpdated();
                    }
                } else {
                    Toast.makeText(getContext(), "Ошибка при получении награды", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<ResponseBody> call, Throwable t) {
                setLoading(false);
                Toast.makeText(getContext(), "Ошибка: " + t.getMessage(), Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void setLoading(boolean isLoading) {
        progressBar.setVisibility(isLoading ? View.VISIBLE : View.GONE);
        btnCreateGoal.setEnabled(!isLoading);
    }
}