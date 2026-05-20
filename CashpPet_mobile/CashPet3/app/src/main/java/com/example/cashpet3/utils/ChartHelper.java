package com.example.cashpet3.utils;

import android.content.Context;
import android.graphics.Color;

import com.example.cashpet3.R;
import com.example.cashpet3.api.models.StatsResponse;
import com.github.mikephil.charting.charts.BarChart;
import com.github.mikephil.charting.charts.PieChart;
import com.github.mikephil.charting.data.BarData;
import com.github.mikephil.charting.data.BarDataSet;
import com.github.mikephil.charting.data.BarEntry;
import com.github.mikephil.charting.data.PieData;
import com.github.mikephil.charting.data.PieDataSet;
import com.github.mikephil.charting.data.PieEntry;
import com.github.mikephil.charting.formatter.IndexAxisValueFormatter;
import com.github.mikephil.charting.utils.ColorTemplate;

import java.util.ArrayList;
import java.util.List;

public class ChartHelper {

    public static void setupPieChart(PieChart pieChart, StatsResponse stats) {
        List<PieEntry> entries = new ArrayList<>();

        if (stats.getCategories() != null) {
            for (StatsResponse.CategoryStats category : stats.getCategories()) {
                if (category.getTotal() > 0) {
                    entries.add(new PieEntry((float) category.getTotal(), category.getCategory()));
                }
            }
        }

        PieDataSet dataSet = new PieDataSet(entries, "Расходы по категориям");
        dataSet.setColors(ColorTemplate.MATERIAL_COLORS);
        dataSet.setValueTextSize(12f);
        dataSet.setValueTextColor(Color.BLACK);

        PieData data = new PieData(dataSet);
        pieChart.setData(data);
        pieChart.setDescription(null);
        pieChart.setCenterText("Расходы");
        pieChart.setCenterTextSize(14f);
        pieChart.invalidate();
    }

    public static void setupBarChart(BarChart barChart, StatsResponse stats) {
        List<BarEntry> incomeEntries = new ArrayList<>();
        List<BarEntry> expenseEntries = new ArrayList<>();
        List<String> days = new ArrayList<>();

        if (stats.getDaily() != null) {
            for (int i = 0; i < stats.getDaily().size(); i++) {
                StatsResponse.DailyStats daily = stats.getDaily().get(i);
                incomeEntries.add(new BarEntry(i, (float) daily.getIncome()));
                expenseEntries.add(new BarEntry(i, (float) daily.getExpense()));
                days.add(daily.getDay());
            }
        }

        BarDataSet incomeDataSet = new BarDataSet(incomeEntries, "Доходы");
        incomeDataSet.setColor(Color.rgb(76, 175, 80));

        BarDataSet expenseDataSet = new BarDataSet(expenseEntries, "Расходы");
        expenseDataSet.setColor(Color.rgb(255, 87, 34));

        BarData barData = new BarData(incomeDataSet, expenseDataSet);
        barData.setBarWidth(0.4f);
        barData.setValueTextSize(10f);

        barChart.setData(barData);
        barChart.setDescription(null);
        barChart.getXAxis().setValueFormatter(new IndexAxisValueFormatter(days));
        barChart.getXAxis().setGranularity(1f);
        barChart.getXAxis().setLabelRotationAngle(-45f);
        barChart.invalidate();
    }
}