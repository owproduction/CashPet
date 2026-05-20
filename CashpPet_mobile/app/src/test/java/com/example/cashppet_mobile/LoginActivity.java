package com.example.cashppet_mobile;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ProgressBar;
import android.widget.Toast;
import androidx.appcompat.app.AppCompatActivity;

import com.example.cashppet_mobile.models.User;
import com.example.cashppet_mobile.utils.DataManager;

public class LoginActivity extends AppCompatActivity {

    private EditText etUsername;
    private Button btnLogin, btnRegister;
    private ProgressBar progressBar;
    private DataManager dataManager;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);

        etUsername = findViewById(R.id.etUsername);
        btnLogin = findViewById(R.id.btnLogin);
        btnRegister = findViewById(R.id.btnRegister);
        progressBar = findViewById(R.id.progressBar);

        dataManager = DataManager.getInstance(this);

        btnLogin.setOnClickListener(v -> {
            String username = etUsername.getText().toString().trim();
            if (!username.isEmpty()) {
                login(username);
            } else {
                Toast.makeText(this, "Введите никнейм", Toast.LENGTH_SHORT).show();
            }
        });

        btnRegister.setOnClickListener(v -> {
            String username = etUsername.getText().toString().trim();
            if (!username.isEmpty()) {
                register(username);
            } else {
                Toast.makeText(this, "Введите никнейм", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void login(String username) {
        setLoading(true);
        dataManager.login(username, new DataManager.AuthCallback() {
            @Override
            public void onSuccess(User user) {
                setLoading(false);
                Intent intent = new Intent(LoginActivity.this, MainActivity.class);
                startActivity(intent);
                finish();
            }

            @Override
            public void onError(String error) {
                setLoading(false);
                Toast.makeText(LoginActivity.this, error, Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void register(String username) {
        setLoading(true);
        dataManager.register(username, new DataManager.AuthCallback() {
            @Override
            public void onSuccess(User user) {
                setLoading(false);
                Intent intent = new Intent(LoginActivity.this, MainActivity.class);
                startActivity(intent);
                finish();
            }

            @Override
            public void onError(String error) {
                setLoading(false);
                Toast.makeText(LoginActivity.this, error, Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void setLoading(boolean isLoading) {
        progressBar.setVisibility(isLoading ? View.VISIBLE : View.GONE);
        btnLogin.setEnabled(!isLoading);
        btnRegister.setEnabled(!isLoading);
    }
}