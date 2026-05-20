package com.example.cashpet3.ui.dialogs;

import android.app.Dialog;
import android.content.Context;
import android.os.Bundle;
import android.text.TextUtils;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ProgressBar;
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

import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class LoginDialog extends DialogFragment {

    private EditText etUsername;
    private Button btnLogin;
    private Button btnRegister;
    private ProgressBar progressBar;
    private TextView tvError;

    private LoginListener listener;
    private SessionManager sessionManager;
    private ApiService apiService;

    public interface LoginListener {
        void onLoginSuccess(User user);
    }

    @Override
    public void onAttach(@NonNull Context context) {
        super.onAttach(context);
        if (context instanceof LoginListener) {
            listener = (LoginListener) context;
        }
        sessionManager = new SessionManager(context);
        apiService = ApiClient.getApiService();
    }

    @NonNull
    @Override
    public Dialog onCreateDialog(@Nullable Bundle savedInstanceState) {
        AlertDialog.Builder builder = new AlertDialog.Builder(requireActivity());

        LayoutInflater inflater = requireActivity().getLayoutInflater();
        View view = inflater.inflate(R.layout.dialog_login, null);

        etUsername = view.findViewById(R.id.etUsername);
        btnLogin = view.findViewById(R.id.btnLogin);
        btnRegister = view.findViewById(R.id.btnRegister);
        progressBar = view.findViewById(R.id.progressBar);
        tvError = view.findViewById(R.id.tvError);

        btnLogin.setOnClickListener(v -> login());
        btnRegister.setOnClickListener(v -> register());

        builder.setView(view);
        builder.setCancelable(false);

        return builder.create();
    }

    private void login() {
        String username = etUsername.getText().toString().trim();

        if (TextUtils.isEmpty(username)) {
            tvError.setText("Введите никнейм!");
            tvError.setVisibility(View.VISIBLE);
            return;
        }

        setLoading(true);

        apiService.getUsers().enqueue(new Callback<List<User>>() {
            @Override
            public void onResponse(Call<List<User>> call, Response<List<User>> response) {
                setLoading(false);

                if (response.isSuccessful() && response.body() != null) {
                    User foundUser = null;
                    for (User user : response.body()) {
                        if (user.getName().equals(username)) {
                            foundUser = user;
                            break;
                        }
                    }

                    if (foundUser != null) {
                        sessionManager.saveUser(foundUser);
                        Toast.makeText(getContext(), "Добро пожаловать, " + foundUser.getName() + "!", Toast.LENGTH_SHORT).show();
                        if (listener != null) {
                            listener.onLoginSuccess(foundUser);
                        }
                        dismiss();
                    } else {
                        tvError.setText("Пользователь не найден. Зарегистрируйтесь.");
                        tvError.setVisibility(View.VISIBLE);
                    }
                } else {
                    tvError.setText("Ошибка сервера");
                    tvError.setVisibility(View.VISIBLE);
                }
            }

            @Override
            public void onFailure(Call<List<User>> call, Throwable t) {
                setLoading(false);
                tvError.setText("Ошибка: " + t.getMessage() + "\nУбедитесь, что сервер запущен");
                tvError.setVisibility(View.VISIBLE);
            }
        });
    }

    private void register() {
        String username = etUsername.getText().toString().trim();

        if (TextUtils.isEmpty(username)) {
            tvError.setText("Введите никнейм!");
            tvError.setVisibility(View.VISIBLE);
            return;
        }

        setLoading(true);

        User newUser = new User(username, username + "@game.local");
        newUser.setCurrentBalance(15000);
        newUser.setFoodCurrency(100);
        newUser.setPetEnergy(80);

        apiService.createUser(newUser).enqueue(new Callback<User>() {
            @Override
            public void onResponse(Call<User> call, Response<User> response) {
                setLoading(false);

                if (response.isSuccessful() && response.body() != null) {
                    User user = response.body();
                    sessionManager.saveUser(user);
                    Toast.makeText(getContext(), "Добро пожаловать, " + user.getName() + "!", Toast.LENGTH_SHORT).show();
                    if (listener != null) {
                        listener.onLoginSuccess(user);
                    }
                    dismiss();
                } else {
                    tvError.setText("Ошибка регистрации");
                    tvError.setVisibility(View.VISIBLE);
                }
            }

            @Override
            public void onFailure(Call<User> call, Throwable t) {
                setLoading(false);
                tvError.setText("Ошибка: " + t.getMessage());
                tvError.setVisibility(View.VISIBLE);
            }
        });
    }

    private void setLoading(boolean isLoading) {
        progressBar.setVisibility(isLoading ? View.VISIBLE : View.GONE);
        btnLogin.setEnabled(!isLoading);
        btnRegister.setEnabled(!isLoading);
        etUsername.setEnabled(!isLoading);
    }
}