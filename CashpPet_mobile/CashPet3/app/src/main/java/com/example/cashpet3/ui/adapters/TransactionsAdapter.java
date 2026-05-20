package com.example.cashpet3.ui.adapters;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.example.cashpet3.R;
import com.example.cashpet3.api.models.Transaction;

import java.text.SimpleDateFormat;
import java.util.List;
import java.util.Locale;

public class TransactionsAdapter extends RecyclerView.Adapter<TransactionsAdapter.ViewHolder> {

    private List<Transaction> transactions;
    private SimpleDateFormat dateFormat = new SimpleDateFormat("dd.MM", Locale.getDefault());

    public TransactionsAdapter(List<Transaction> transactions) {
        this.transactions = transactions;
    }

    public void updateData(List<Transaction> newTransactions) {
        this.transactions = newTransactions;
        notifyDataSetChanged();
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext())
                .inflate(R.layout.item_transaction, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        Transaction transaction = transactions.get(position);

        String date = transaction.getDate() != null ?
                dateFormat.format(transaction.getDate()) : "---";

        holder.tvDate.setText(date);
        holder.tvCategory.setText(transaction.getCategory());

        String amountStr = String.format(Locale.getDefault(), "%.0f ₽", transaction.getAmount());
        if (transaction.isIncome()) {
            holder.tvAmount.setText("+" + amountStr);
            holder.tvAmount.setTextColor(holder.itemView.getContext().getColor(R.color.income_color));
            holder.tvIcon.setText("💰");
        } else {
            holder.tvAmount.setText("-" + amountStr);
            holder.tvAmount.setTextColor(holder.itemView.getContext().getColor(R.color.expense_color));
            holder.tvIcon.setText("💸");
        }
    }

    @Override
    public int getItemCount() {
        return transactions != null ? transactions.size() : 0;
    }

    static class ViewHolder extends RecyclerView.ViewHolder {
        TextView tvDate, tvCategory, tvAmount, tvIcon;

        ViewHolder(@NonNull View itemView) {
            super(itemView);
            tvDate = itemView.findViewById(R.id.tvDate);
            tvCategory = itemView.findViewById(R.id.tvCategory);
            tvAmount = itemView.findViewById(R.id.tvAmount);
            tvIcon = itemView.findViewById(R.id.tvIcon);
        }
    }
}