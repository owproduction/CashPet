package com.example.cashpet3.ui.dialogs;

import android.app.Dialog;
import android.os.Bundle;
import android.text.TextUtils;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ProgressBar;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.appcompat.app.AlertDialog;
import androidx.fragment.app.DialogFragment;

import com.example.cashpet3.R;
import com.example.cashpet3.api.ApiClient;
import com.example.cashpet3.api.ApiService;
import com.example.cashpet3.api.models.User;
import com.example.cashpet3.utils.SessionManager;

import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;

import okhttp3.ResponseBody;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class ExpenseDialog extends DialogFragment {

    private EditText etAmount;
    private Spinner spinnerCategory;
    private EditText etDescription;
    private Button btnConfirm;
    private Button btnCancel;
    private ProgressBar progressBar;
    private TextView tvError;

    private SessionManager sessionManager;
    private ApiService apiService;
    private User currentUser;
    private ExpenseListener listener;

    private String selectedCategory = "Продукты";

    public interface ExpenseListener {
        void onExpenseAdded();
    }

    public static ExpenseDialog newInstance(ExpenseListener listener) {
        ExpenseDialog dialog = new ExpenseDialog();
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
        View view = inflater.inflate(R.layout.dialog_expense, null);

        initViews(view);
        setupSpinner();
        setupListeners();

        builder.setView(view);
        return builder.create();
    }

    private void initViews(View view) {
        etAmount = view.findViewById(R.id.etAmount);
        spinnerCategory = view.findViewById(R.id.spinnerCategory);
        etDescription = view.findViewById(R.id.etDescription);
        btnConfirm = view.findViewById(R.id.btnConfirm);
        btnCancel = view.findViewById(R.id.btnCancel);
        progressBar = view.findViewById(R.id.progressBar);
        tvError = view.findViewById(R.id.tvError);
    }

    private void setupSpinner() {
        String[] categories = {"🍔 Продукты", "🚗 Транспорт", "🏠 Жильё",
                "👕 Одежда", "💊 Здоровье", "🎮 Развлечения",
                "📚 Образование", "💸 Другое"};

        ArrayAdapter<String> adapter = new ArrayAdapter<>(requireContext(),
                android.R.layout.simple_spinner_item, categories);
        adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        spinnerCategory.setAdapter(adapter);

        spinnerCategory.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
                String selected = parent.getItemAtPosition(position).toString();
                // Убираем эмодзи, оставляем только текст категории
                selectedCategory = selected.replaceAll("[^а-яА-Яa-zA-Z]", "");
                if (selectedCategory.isEmpty()) {
                    selectedCategory = "Продукты";
                }
            }

            @Override
            public void onNothingSelected(AdapterView<?> parent) {
                selectedCategory = "Продукты";
            }
        });
    }

    private void setupListeners() {
        btnConfirm.setOnClickListener(v -> addExpense());
        btnCancel.setOnClickListener(v -> dismiss());
    }

    private void addExpense() {
        String amountStr = etAmount.getText().toString().trim();

        if (TextUtils.isEmpty(amountStr)) {
            tvError.setText("Введите сумму");
            tvError.setVisibility(View.VISIBLE);
            return;
        }

        double amount;
        try {
            amount = Double.parseDouble(amountStr);
            if (amount <= 0) {
                tvError.setText("Сумма должна быть больше 0");
                tvError.setVisibility(View.VISIBLE);
                return;
            }
        } catch (NumberFormatException e) {
            tvError.setText("Некорректная сумма");
            tvError.setVisibility(View.VISIBLE);
            return;
        }

        if (currentUser == null) {
            tvError.setText("Пользователь не найден");
            tvError.setVisibility(View.VISIBLE);
            return;
        }

        if (amount > currentUser.getCurrentBalance()) {
            tvError.setText(String.format(Locale.getDefault(), "Недостаточно средств! Баланс: %.2f ₽", currentUser.getCurrentBalance()));
            tvError.setVisibility(View.VISIBLE);
            return;
        }

        String description = etDescription.getText().toString().trim();
        if (TextUtils.isEmpty(description)) {
            description = "";
        }

        String currentDate = new SimpleDateFormat("yyyy-MM-dd", Locale.getDefault()).format(new Date());

        setLoading(true);

        ApiService.ExpenseRequest expenseRequest = new ApiService.ExpenseRequest(
                currentUser.getUserId(), amount, selectedCategory, description, currentDate);

        apiService.createExpense(expenseRequest).enqueue(new Callback<ResponseBody>() {
            @Override
            public void onResponse(Call<ResponseBody> call, Response<ResponseBody> response) {
                setLoading(false);

                if (response.isSuccessful()) {
                    Toast.makeText(getContext(), String.format(Locale.getDefault(), "💰 Расход: %s -%.2f ₽", selectedCategory, amount), Toast.LENGTH_SHORT).show();
                    if (listener != null) {
                        listener.onExpenseAdded();
                    }
                    dismiss();
                } else {
                    tvError.setText("Ошибка при добавлении расхода");
                    tvError.setVisibility(View.VISIBLE);
                }
            }

            @Override
            public void onFailure(Call<ResponseBody> call, Throwable t) {
                setLoading(false);
                tvError.setText("Ошибка: " + t.getMessage());
                tvError.setVisibility(View.VISIBLE);
            }
        });
    }

    private void setLoading(boolean isLoading) {
        progressBar.setVisibility(isLoading ? View.VISIBLE : View.GONE);
        btnConfirm.setEnabled(!isLoading);
        btnCancel.setEnabled(!isLoading);
        etAmount.setEnabled(!isLoading);
        spinnerCategory.setEnabled(!isLoading);
        etDescription.setEnabled(!isLoading);
    }
}