package com.example.cashpet3.ui.dialogs;

import android.app.Dialog;
import android.os.Bundle;
import android.text.TextUtils;
import android.view.LayoutInflater;
import android.view.View;
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

public class IncomeDialog extends DialogFragment {

    private EditText etAmount;
    private Spinner spinnerSource;
    private EditText etDescription;
    private Button btnConfirm;
    private Button btnCancel;
    private ProgressBar progressBar;
    private TextView tvError;

    private SessionManager sessionManager;
    private ApiService apiService;
    private User currentUser;
    private IncomeListener listener;

    private String selectedSource = "Зарплата";

    public interface IncomeListener {
        void onIncomeAdded();
    }

    public static IncomeDialog newInstance(IncomeListener listener) {
        IncomeDialog dialog = new IncomeDialog();
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
        View view = inflater.inflate(R.layout.dialog_income, null);

        initViews(view);
        setupSpinner();
        setupListeners();

        builder.setView(view);
        return builder.create();
    }

    private void initViews(View view) {
        etAmount = view.findViewById(R.id.etAmount);
        spinnerSource = view.findViewById(R.id.spinnerSource);
        etDescription = view.findViewById(R.id.etDescription);
        btnConfirm = view.findViewById(R.id.btnConfirm);
        btnCancel = view.findViewById(R.id.btnCancel);
        progressBar = view.findViewById(R.id.progressBar);
        tvError = view.findViewById(R.id.tvError);
    }

    private void setupSpinner() {
        String[] sources = {"💼 Зарплата", "💻 Фриланс", "🎁 Подарок",
                "📈 Инвестиции", "🏆 Премия", "🏠 Аренда",
                "💰 Другое"};

        ArrayAdapter<String> adapter = new ArrayAdapter<>(requireContext(),
                android.R.layout.simple_spinner_item, sources);
        adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        spinnerSource.setAdapter(adapter);

        spinnerSource.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
                String selected = parent.getItemAtPosition(position).toString();
                // Убираем эмодзи, оставляем только текст источника
                selectedSource = selected.replaceAll("[^а-яА-Яa-zA-Z]", "");
                if (selectedSource.isEmpty()) {
                    selectedSource = "Зарплата";
                }
            }

            @Override
            public void onNothingSelected(AdapterView<?> parent) {
                selectedSource = "Зарплата";
            }
        });
    }

    private void setupListeners() {
        btnConfirm.setOnClickListener(v -> addIncome());
        btnCancel.setOnClickListener(v -> dismiss());
    }

    private void addIncome() {
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

        String description = etDescription.getText().toString().trim();
        if (TextUtils.isEmpty(description)) {
            description = "Доход от " + selectedSource;
        }

        String currentDate = new SimpleDateFormat("yyyy-MM-dd", Locale.getDefault()).format(new Date());

        setLoading(true);

        ApiService.IncomeRequest incomeRequest = new ApiService.IncomeRequest(
                currentUser.getUserId(), amount, selectedSource, currentDate);

        apiService.createIncome(incomeRequest).enqueue(new Callback<ResponseBody>() {
            @Override
            public void onResponse(Call<ResponseBody> call, Response<ResponseBody> response) {
                setLoading(false);

                if (response.isSuccessful()) {
                    Toast.makeText(getContext(), String.format(Locale.getDefault(), "💵 Доход: +%.2f ₽ от %s", amount, selectedSource), Toast.LENGTH_SHORT).show();
                    if (listener != null) {
                        listener.onIncomeAdded();
                    }
                    dismiss();
                } else {
                    tvError.setText("Ошибка при добавлении дохода");
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
        spinnerSource.setEnabled(!isLoading);
        etDescription.setEnabled(!isLoading);
    }
}