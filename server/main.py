from fastapi import FastAPI, HTTPException, Depends, status
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field
from typing import List, Optional
from datetime import datetime, date
import sqlite3
import uvicorn
from contextlib import contextmanager
import os
import json

# Инициализация FastAPI приложения
app = FastAPI(
    title="Финансовый Тамагоччи API",
    description="API для игры Финансовый Тамагоччи",
    version="1.0.0"
)

# Настройка CORS для работы с WPF приложением
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Путь к базе данных
DATABASE_FILE = "financial_tamagotchi.db"

# Модели Pydantic для API
class UserCreate(BaseModel):
    name: str = Field(..., min_length=2, max_length=100)
    email: str = Field(..., max_length=100)
    registration_date: Optional[date] = None
    total_saved: Optional[float] = 0.0
    current_balance: Optional[float] = 0.0
    food_currency: Optional[int] = 100  # Игровая валюта (корм)
    pet_energy: Optional[int] = 80  # Энергия питомца

class UserResponse(UserCreate):
    user_id: int

    class Config:
        from_attributes = True

class BudgetCreate(BaseModel):
    user_id: int
    category: str = Field(..., max_length=50)
    amount: float = Field(..., gt=0)
    period: str = Field(..., max_length=20)  # 'monthly', 'weekly', 'daily'
    start_date: date
    end_date: Optional[date] = None

class BudgetResponse(BudgetCreate):
    budget_id: int

class ExpenseCreate(BaseModel):
    user_id: int
    amount: float = Field(..., gt=0)
    category: str = Field(..., max_length=50)
    description: Optional[str] = Field(None, max_length=200)
    date: date
    is_planned: Optional[bool] = False

class ExpenseResponse(ExpenseCreate):
    expense_id: int

class IncomeCreate(BaseModel):
    user_id: int
    amount: float = Field(..., gt=0)
    source: str = Field(..., max_length=100)
    date: date
    is_recurring: Optional[bool] = False

class IncomeResponse(IncomeCreate):
    income_id: int

class GoalCreate(BaseModel):
    user_id: int
    target_amount: float = Field(..., gt=0)
    current_amount: Optional[float] = 0.0
    name: str = Field(..., max_length=100)
    deadline: Optional[date] = None
    is_completed: Optional[bool] = False
    reward_amount: Optional[int] = 50
    reward_claimed: Optional[bool] = False

class GoalResponse(GoalCreate):
    goal_id: int

class TransactionCreate(BaseModel):
    user_id: int
    amount: float
    type: str = Field(..., max_length=20)  # 'income' или 'expense'
    category: str = Field(..., max_length=50)
    date: datetime
    description: Optional[str] = Field(None, max_length=200)

class TransactionResponse(TransactionCreate):
    transaction_id: int

class FeedPetRequest(BaseModel):
    user_id: int
    food_amount: int = 10

# Контекстный менеджер для работы с БД
@contextmanager
def get_db_connection():
    conn = sqlite3.connect(DATABASE_FILE)
    conn.row_factory = sqlite3.Row
    try:
        yield conn
        conn.commit()
    except Exception:
        conn.rollback()
        raise
    finally:
        conn.close()

def init_db():
    """Инициализация базы данных"""
    with get_db_connection() as conn:
        cursor = conn.cursor()
        
        # Таблица пользователей (добавлены поля для игры)
        cursor.execute('''
        CREATE TABLE IF NOT EXISTS users (
            user_id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL,
            email TEXT UNIQUE NOT NULL,
            registration_date DATE DEFAULT CURRENT_DATE,
            total_saved REAL DEFAULT 0,
            current_balance REAL DEFAULT 0,
            food_currency INTEGER DEFAULT 100,
            pet_energy INTEGER DEFAULT 80,
            last_feed_time DATETIME DEFAULT CURRENT_TIMESTAMP
        )
        ''')
        
        # Таблица целей
        cursor.execute('''
        CREATE TABLE IF NOT EXISTS goals (
            goal_id INTEGER PRIMARY KEY AUTOINCREMENT,
            user_id INTEGER NOT NULL,
            target_amount REAL NOT NULL,
            current_amount REAL DEFAULT 0,
            name TEXT NOT NULL,
            deadline DATE,
            is_completed BOOLEAN DEFAULT FALSE,
            reward_amount INTEGER DEFAULT 50,
            reward_claimed BOOLEAN DEFAULT FALSE,
            FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
        )
        ''')
        
        # Таблица расходов
        cursor.execute('''
        CREATE TABLE IF NOT EXISTS expenses (
            expense_id INTEGER PRIMARY KEY AUTOINCREMENT,
            user_id INTEGER NOT NULL,
            amount REAL NOT NULL,
            category TEXT NOT NULL,
            description TEXT,
            date DATE NOT NULL,
            is_planned BOOLEAN DEFAULT FALSE,
            FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
        )
        ''')
        
        # Таблица доходов
        cursor.execute('''
        CREATE TABLE IF NOT EXISTS incomes (
            income_id INTEGER PRIMARY KEY AUTOINCREMENT,
            user_id INTEGER NOT NULL,
            amount REAL NOT NULL,
            source TEXT NOT NULL,
            date DATE NOT NULL,
            is_recurring BOOLEAN DEFAULT FALSE,
            FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
        )
        ''')
        
        # Таблица транзакций (для графиков)
        cursor.execute('''
        CREATE TABLE IF NOT EXISTS transactions (
            transaction_id INTEGER PRIMARY KEY AUTOINCREMENT,
            user_id INTEGER NOT NULL,
            amount REAL NOT NULL,
            type TEXT NOT NULL,
            category TEXT NOT NULL,
            date DATETIME NOT NULL,
            description TEXT,
            FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
        )
        ''')
        
        # Индексы для быстрого поиска
        cursor.execute('CREATE INDEX IF NOT EXISTS idx_users_email ON users(email)')
        cursor.execute('CREATE INDEX IF NOT EXISTS idx_transactions_user_date ON transactions(user_id, date)')
        cursor.execute('CREATE INDEX IF NOT EXISTS idx_goals_user ON goals(user_id)')
        
        print("✅ База данных инициализирована")

# API Endpoints

@app.get("/")
def read_root():
    return {
        "message": "Финансовый Тамагоччи API",
        "version": "1.0.0",
        "endpoints": [
            "/users",
            "/goals",
            "/expenses", 
            "/incomes",
            "/transactions",
            "/pet/feed"
        ]
    }

# ========== ПОЛЬЗОВАТЕЛИ ==========
@app.post("/users/", response_model=UserResponse, status_code=status.HTTP_201_CREATED)
def create_user(user: UserCreate):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        try:
            cursor.execute('''
            INSERT INTO users (name, email, registration_date, total_saved, current_balance, food_currency, pet_energy)
            VALUES (?, ?, COALESCE(?, CURRENT_DATE), ?, ?, ?, ?)
            ''', (user.name, user.email, user.registration_date, 
                  user.total_saved, user.current_balance, user.food_currency, user.pet_energy))
            user_id = cursor.lastrowid
            
            cursor.execute('SELECT * FROM users WHERE user_id = ?', (user_id,))
            return dict(cursor.fetchone())
        except sqlite3.IntegrityError:
            raise HTTPException(status_code=400, detail="Email уже существует")

@app.get("/users/", response_model=List[UserResponse])
def get_users():
    with get_db_connection() as conn:
        cursor = conn.cursor()
        cursor.execute('SELECT * FROM users ORDER BY user_id')
        return [dict(row) for row in cursor.fetchall()]

@app.get("/users/{user_id}", response_model=UserResponse)
def get_user(user_id: int):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        cursor.execute('SELECT * FROM users WHERE user_id = ?', (user_id,))
        user = cursor.fetchone()
        if not user:
            raise HTTPException(status_code=404, detail="Пользователь не найден")
        return dict(user)

@app.put("/users/{user_id}", response_model=UserResponse)
def update_user(user_id: int, user: UserCreate):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        cursor.execute('''
        UPDATE users 
        SET name = ?, email = ?, registration_date = ?, total_saved = ?, 
            current_balance = ?, food_currency = ?, pet_energy = ?
        WHERE user_id = ?
        ''', (user.name, user.email, user.registration_date, 
              user.total_saved, user.current_balance, user.food_currency, 
              user.pet_energy, user_id))
        
        if cursor.rowcount == 0:
            raise HTTPException(status_code=404, detail="Пользователь не найден")
        
        cursor.execute('SELECT * FROM users WHERE user_id = ?', (user_id,))
        return dict(cursor.fetchone())

@app.delete("/users/{user_id}", status_code=status.HTTP_204_NO_CONTENT)
def delete_user(user_id: int):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        cursor.execute('DELETE FROM users WHERE user_id = ?', (user_id,))
        if cursor.rowcount == 0:
            raise HTTPException(status_code=404, detail="Пользователь не найден")

# ========== РАСХОДЫ ==========
@app.post("/expenses/", response_model=ExpenseResponse, status_code=status.HTTP_201_CREATED)
def create_expense(expense: ExpenseCreate):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        
        # Проверяем баланс
        cursor.execute('SELECT current_balance, pet_energy FROM users WHERE user_id = ?', (expense.user_id,))
        user = cursor.fetchone()
        if not user:
            raise HTTPException(status_code=404, detail="Пользователь не найден")
        
        if user['current_balance'] < expense.amount:
            raise HTTPException(status_code=400, detail="Недостаточно средств")
        
        # Добавляем расход
        cursor.execute('''
        INSERT INTO expenses (user_id, amount, category, description, date, is_planned)
        VALUES (?, ?, ?, ?, ?, ?)
        ''', (expense.user_id, expense.amount, expense.category, 
              expense.description, expense.date, expense.is_planned))
        expense_id = cursor.lastrowid
        
        # Обновляем баланс и энергию питомца
        new_energy = max(0, user['pet_energy'] - 5)
        cursor.execute('''
        UPDATE users 
        SET current_balance = current_balance - ?,
            pet_energy = ?
        WHERE user_id = ?
        ''', (expense.amount, new_energy, expense.user_id))
        
        # Добавляем транзакцию для истории
        cursor.execute('''
        INSERT INTO transactions (user_id, amount, type, category, date, description)
        VALUES (?, ?, 'expense', ?, ?, ?)
        ''', (expense.user_id, expense.amount, expense.category, 
              datetime.combine(expense.date, datetime.min.time()), expense.description))
        
        cursor.execute('SELECT * FROM expenses WHERE expense_id = ?', (expense_id,))
        return dict(cursor.fetchone())

@app.get("/expenses/", response_model=List[ExpenseResponse])
def get_expenses(user_id: Optional[int] = None, start_date: Optional[date] = None, end_date: Optional[date] = None):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        query = 'SELECT * FROM expenses WHERE 1=1'
        params = []
        
        if user_id:
            query += ' AND user_id = ?'
            params.append(user_id)
        if start_date:
            query += ' AND date >= ?'
            params.append(start_date)
        if end_date:
            query += ' AND date <= ?'
            params.append(end_date)
        
        query += ' ORDER BY date DESC'
        cursor.execute(query, params)
        return [dict(row) for row in cursor.fetchall()]

# ========== ДОХОДЫ ==========
@app.post("/incomes/", response_model=IncomeResponse, status_code=status.HTTP_201_CREATED)
def create_income(income: IncomeCreate):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        
        cursor.execute('SELECT pet_energy FROM users WHERE user_id = ?', (income.user_id,))
        user = cursor.fetchone()
        if not user:
            raise HTTPException(status_code=404, detail="Пользователь не найден")
        
        # Добавляем доход
        cursor.execute('''
        INSERT INTO incomes (user_id, amount, source, date, is_recurring)
        VALUES (?, ?, ?, ?, ?)
        ''', (income.user_id, income.amount, income.source, income.date, income.is_recurring))
        income_id = cursor.lastrowid
        
        # Обновляем баланс, энергию питомца и даём +10 корма
        new_energy = min(100, user['pet_energy'] + 10)
        cursor.execute('''
        UPDATE users 
        SET current_balance = current_balance + ?,
            pet_energy = ?,
            food_currency = food_currency + 10
        WHERE user_id = ?
        ''', (income.amount, new_energy, income.user_id))
        
        # Добавляем транзакцию для истории
        cursor.execute('''
        INSERT INTO transactions (user_id, amount, type, category, date, description)
        VALUES (?, ?, 'income', ?, ?, ?)
        ''', (income.user_id, income.amount, income.source, 
              datetime.combine(income.date, datetime.min.time()), f"Доход от {income.source}"))
        
        cursor.execute('SELECT * FROM incomes WHERE income_id = ?', (income_id,))
        return dict(cursor.fetchone())

@app.get("/incomes/", response_model=List[IncomeResponse])
def get_incomes(user_id: Optional[int] = None, start_date: Optional[date] = None, end_date: Optional[date] = None):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        query = 'SELECT * FROM incomes WHERE 1=1'
        params = []
        
        if user_id:
            query += ' AND user_id = ?'
            params.append(user_id)
        if start_date:
            query += ' AND date >= ?'
            params.append(start_date)
        if end_date:
            query += ' AND date <= ?'
            params.append(end_date)
        
        query += ' ORDER BY date DESC'
        cursor.execute(query, params)
        return [dict(row) for row in cursor.fetchall()]

# ========== ЦЕЛИ ==========
@app.post("/goals/", response_model=GoalResponse, status_code=status.HTTP_201_CREATED)
def create_goal(goal: GoalCreate):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        cursor.execute('''
        INSERT INTO goals (user_id, target_amount, current_amount, name, deadline, is_completed, reward_amount, reward_claimed)
        VALUES (?, ?, ?, ?, ?, ?, ?, ?)
        ''', (goal.user_id, goal.target_amount, goal.current_amount, 
              goal.name, goal.deadline, goal.is_completed, goal.reward_amount, goal.reward_claimed))
        goal_id = cursor.lastrowid
        
        cursor.execute('SELECT * FROM goals WHERE goal_id = ?', (goal_id,))
        return dict(cursor.fetchone())

@app.get("/goals/", response_model=List[GoalResponse])
def get_goals(user_id: Optional[int] = None, completed: Optional[bool] = None):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        query = 'SELECT * FROM goals WHERE 1=1'
        params = []
        
        if user_id:
            query += ' AND user_id = ?'
            params.append(user_id)
        if completed is not None:
            query += ' AND is_completed = ?'
            params.append(completed)
        
        query += ' ORDER BY deadline ASC'
        cursor.execute(query, params)
        return [dict(row) for row in cursor.fetchall()]

@app.post("/goals/{goal_id}/add_money")
def add_money_to_goal(goal_id: int, amount: float, user_id: int):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        
        # Получаем информацию о цели
        cursor.execute('SELECT * FROM goals WHERE goal_id = ?', (goal_id,))
        goal = cursor.fetchone()
        if not goal:
            raise HTTPException(status_code=404, detail="Цель не найдена")
        
        # Проверяем баланс
        cursor.execute('SELECT current_balance FROM users WHERE user_id = ?', (user_id,))
        user = cursor.fetchone()
        if user['current_balance'] < amount:
            raise HTTPException(status_code=400, detail="Недостаточно средств")
        
        # Обновляем текущую сумму
        new_amount = goal['current_amount'] + amount
        cursor.execute('''
        UPDATE goals 
        SET current_amount = ?,
            is_completed = CASE WHEN ? >= target_amount THEN TRUE ELSE FALSE END
        WHERE goal_id = ?
        ''', (new_amount, new_amount, goal_id))
        
        # Списываем деньги с баланса
        cursor.execute('''
        UPDATE users 
        SET current_balance = current_balance - ?
        WHERE user_id = ?
        ''', (amount, user_id))
        
        # Бонус за пополнение цели (20% шанс)
        import random
        if random.random() < 0.2:
            bonus = random.randint(1, 5)
            cursor.execute('''
            UPDATE users SET food_currency = food_currency + ? WHERE user_id = ?
            ''', (bonus, user_id))
            cursor.execute('SELECT * FROM goals WHERE goal_id = ?', (goal_id,))
            result = dict(cursor.fetchone())
            result['bonus'] = bonus
            return result
        
        cursor.execute('SELECT * FROM goals WHERE goal_id = ?', (goal_id,))
        return dict(cursor.fetchone())

@app.post("/goals/{goal_id}/claim_reward")
def claim_goal_reward(goal_id: int, user_id: int):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        
        cursor.execute('SELECT * FROM goals WHERE goal_id = ? AND user_id = ?', (goal_id, user_id))
        goal = cursor.fetchone()
        if not goal:
            raise HTTPException(status_code=404, detail="Цель не найдена")
        
        if not goal['is_completed']:
            raise HTTPException(status_code=400, detail="Цель ещё не выполнена")
        
        if goal['reward_claimed']:
            raise HTTPException(status_code=400, detail="Награда уже получена")
        
        # Добавляем награду
        cursor.execute('''
        UPDATE users 
        SET food_currency = food_currency + ?
        WHERE user_id = ?
        ''', (goal['reward_amount'], user_id))
        
        # Отмечаем награду как полученную
        cursor.execute('''
        UPDATE goals SET reward_claimed = TRUE WHERE goal_id = ?
        ''', (goal_id,))
        
        return {"message": "Награда получена", "reward": goal['reward_amount']}

# ========== ТРАНЗАКЦИИ (ДЛЯ ГРАФИКОВ) ==========
@app.get("/transactions/", response_model=List[TransactionResponse])
def get_transactions(user_id: int, days: Optional[int] = 30):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        
        if days:
            cursor.execute('''
            SELECT * FROM transactions 
            WHERE user_id = ? AND date >= datetime('now', ? || ' days')
            ORDER BY date DESC
            ''', (user_id, f'-{days}'))
        else:
            cursor.execute('''
            SELECT * FROM transactions 
            WHERE user_id = ?
            ORDER BY date DESC
            ''', (user_id,))
        
        return [dict(row) for row in cursor.fetchall()]

@app.get("/transactions/stats/{user_id}")
def get_transaction_stats(user_id: int):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        
        # Статистика за всё время
        cursor.execute('''
        SELECT 
            SUM(CASE WHEN type = 'income' THEN amount ELSE 0 END) as total_income,
            SUM(CASE WHEN type = 'expense' THEN amount ELSE 0 END) as total_expense
        FROM transactions WHERE user_id = ?
        ''', (user_id,))
        totals = cursor.fetchone()
        
        # Статистика за последние 7 дней
        cursor.execute('''
        SELECT 
            date(date) as day,
            SUM(CASE WHEN type = 'income' THEN amount ELSE 0 END) as income,
            SUM(CASE WHEN type = 'expense' THEN amount ELSE 0 END) as expense
        FROM transactions 
        WHERE user_id = ? AND date >= date('now', '-7 days')
        GROUP BY date(date)
        ORDER BY date(date) DESC
        ''', (user_id,))
        daily = cursor.fetchall()
        
        # Расходы по категориям
        cursor.execute('''
        SELECT category, SUM(amount) as total
        FROM transactions 
        WHERE user_id = ? AND type = 'expense'
        GROUP BY category
        ORDER BY total DESC
        ''', (user_id,))
        categories = cursor.fetchall()
        
        return {
            "totals": dict(totals),
            "daily": [dict(row) for row in daily],
            "categories": [dict(row) for row in categories]
        }

# ========== ПИТОМЕЦ ==========
@app.post("/pet/feed")
def feed_pet(user_id: int, food_amount: int = 10):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        
        cursor.execute('SELECT food_currency, pet_energy FROM users WHERE user_id = ?', (user_id,))
        user = cursor.fetchone()
        if not user:
            raise HTTPException(status_code=404, detail="Пользователь не найден")
        
        if user['food_currency'] < food_amount:
            raise HTTPException(status_code=400, detail="Недостаточно корма")
        
        # Кормим питомца
        new_energy = min(100, user['pet_energy'] + 20)
        cursor.execute('''
        UPDATE users 
        SET food_currency = food_currency - ?,
            pet_energy = ?,
            last_feed_time = CURRENT_TIMESTAMP
        WHERE user_id = ?
        ''', (food_amount, new_energy, user_id))
        
        # Случайный бонус (30% шанс)
        import random
        bonus = 0
        if random.random() < 0.3:
            bonus = random.randint(5, 15)
            cursor.execute('''
            UPDATE users SET food_currency = food_currency + ? WHERE user_id = ?
            ''', (bonus, user_id))
        
        cursor.execute('SELECT food_currency, pet_energy FROM users WHERE user_id = ?', (user_id,))
        result = cursor.fetchone()
        
        return {
            "food_currency": result['food_currency'],
            "pet_energy": result['pet_energy'],
            "bonus": bonus
        }

@app.get("/pet/status/{user_id}")
def get_pet_status(user_id: int):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        
        cursor.execute('''
        SELECT food_currency, pet_energy, last_feed_time 
        FROM users WHERE user_id = ?
        ''', (user_id,))
        
        user = cursor.fetchone()
        if not user:
            raise HTTPException(status_code=404, detail="Пользователь не найден")
        
        # Рассчитываем голод (каждые 30 минут -5 энергии)
        last_feed = datetime.fromisoformat(user['last_feed_time'])
        hours_since_feed = (datetime.now() - last_feed).total_seconds() / 3600
        hunger_loss = int(hours_since_feed * 10)  # -10 энергии в час
        
        return {
            "food_currency": user['food_currency'],
            "pet_energy": max(0, user['pet_energy'] - hunger_loss),
            "hours_without_food": round(hours_since_feed, 1)
        }

# ========== БЮДЖЕТ ==========
@app.post("/budgets/", response_model=BudgetResponse, status_code=status.HTTP_201_CREATED)
def create_budget(budget: BudgetCreate):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        cursor.execute('''
        INSERT INTO budgets (user_id, category, amount, period, start_date, end_date)
        VALUES (?, ?, ?, ?, ?, ?)
        ''', (budget.user_id, budget.category, budget.amount, 
              budget.period, budget.start_date, budget.end_date))
        budget_id = cursor.lastrowid
        
        cursor.execute('SELECT * FROM budgets WHERE budget_id = ?', (budget_id,))
        return dict(cursor.fetchone())

@app.get("/budgets/", response_model=List[BudgetResponse])
def get_budgets(user_id: Optional[int] = None):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        if user_id:
            cursor.execute('SELECT * FROM budgets WHERE user_id = ?', (user_id,))
        else:
            cursor.execute('SELECT * FROM budgets')
        return [dict(row) for row in cursor.fetchall()]

# ========== ТЕСТОВЫЕ ДАННЫЕ ==========
@app.post("/test/create_sample_user")
def create_sample_user():
    with get_db_connection() as conn:
        cursor = conn.cursor()
        
        # Проверяем, есть ли уже пользователи
        cursor.execute('SELECT COUNT(*) as count FROM users')
        if cursor.fetchone()['count'] > 0:
            return {"message": "Пользователи уже существуют"}
        
        # Создаём тестового пользователя
        cursor.execute('''
        INSERT INTO users (name, email, total_saved, current_balance, food_currency, pet_energy)
        VALUES ('Иван Иванов', 'ivan@example.com', 50000, 150000, 100, 80)
        ''')
        user_id = 1
        
        # Тестовые цели
        cursor.execute('''
        INSERT INTO goals (user_id, target_amount, current_amount, name, deadline, reward_amount)
        VALUES 
        (?, 50000, 25000, 'Новый ноутбук', date('now', '+3 months'), 60),
        (?, 100000, 30000, 'Отпуск на море', date('now', '+6 months'), 80),
        (?, 1000000, 150000, 'Машина мечты', date('now', '+1 year'), 100)
        ''', (user_id, user_id, user_id))
        
        # Тестовые транзакции
        import random
        categories = ['Продукты', 'Транспорт', 'Жильё', 'Одежда', 'Здоровье', 'Развлечения']
        sources = ['Зарплата', 'Фриланс', 'Подарок', 'Инвестиции', 'Премия']
        
        for i in range(30):
            date = datetime.now().replace(day=datetime.now().day - i).strftime('%Y-%m-%d')
            
            # Доход
            cursor.execute('''
            INSERT INTO transactions (user_id, amount, type, category, date)
            VALUES (?, ?, 'income', ?, ?)
            ''', (user_id, random.randint(1000, 5000), random.choice(sources), date))
            
            # Расход
            cursor.execute('''
            INSERT INTO transactions (user_id, amount, type, category, date)
            VALUES (?, ?, 'expense', ?, ?)
            ''', (user_id, random.randint(500, 3000), random.choice(categories), date))
        
        return {"message": "Тестовые данные созданы", "user_id": user_id}

# Запуск сервера
if __name__ == "__main__":
    print("=" * 50)
    print("🚀 Запуск сервера Финансового Тамагоччи")
    print("=" * 50)
    
    # Инициализация БД
    init_db()
    
    print(f"📁 База данных: {os.path.abspath(DATABASE_FILE)}")
    print(f"🌐 API доступно по адресу: http://localhost:8000")
    print(f"📚 Документация: http://localhost:8000/docs")
    print("=" * 50)
    
    uvicorn.run(app, host="0.0.0.0", port=8000)