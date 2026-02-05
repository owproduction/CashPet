from fastapi import FastAPI, HTTPException, Depends, status
from pydantic import BaseModel, Field
from typing import List, Optional
from datetime import datetime, date
import sqlite3
import uvicorn
from contextlib import contextmanager
import os
from contextlib import asynccontextmanager

# Путь к директории, где должна создаваться БД
PROJECT_DIR = r"D:\Kisl\CashPet"
DATABASE_FILE = os.path.join(PROJECT_DIR, "financial_tamagotchi.db")

# Создаем директорию, если она не существует
os.makedirs(PROJECT_DIR, exist_ok=True)

# Инициализация FastAPI приложения с lifespan
@asynccontextmanager
async def lifespan(app: FastAPI):
    """Lifespan контекст для инициализации и очистки ресурсов"""
    print("=" * 50)
    print("Запуск Финансового Тамагоччи API")
    print("=" * 50)
    
    # Инициализация базы данных
    init_db()
    
    print(f"API доступно по адресу: http://localhost:8000")
    print(f"Документация: http://localhost:8000/docs")
    print(f"База данных создана: {DATABASE_FILE}")
    print("=" * 50)
    
    yield  # Приложение работает
    
    print("Остановка сервера...")
    # Здесь можно добавить код для очистки ресурсов

app = FastAPI(
    title="Финансовый Тамагоччи API",
    description="API для управления финансовым тамагоччи",
    version="1.0.0",
    lifespan=lifespan
)

# Модели Pydantic
class UserCreate(BaseModel):
    name: str = Field(..., min_length=2, max_length=100)
    email: str = Field(..., max_length=100)
    registration_date: Optional[date] = None
    total_saved: Optional[float] = 0.0
    current_balance: Optional[float] = 0.0

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

    class Config:
        from_attributes = True

class ExpenseCreate(BaseModel):
    user_id: int
    amount: float = Field(..., gt=0)
    category: str = Field(..., max_length=50)
    description: Optional[str] = Field(None, max_length=200)
    date: date
    is_planned: Optional[bool] = False

class ExpenseResponse(ExpenseCreate):
    expense_id: int

    class Config:
        from_attributes = True

class GoalCreate(BaseModel):
    user_id: int
    target_amount: float = Field(..., gt=0)
    current_amount: Optional[float] = 0.0
    name: str = Field(..., max_length=100)
    deadline: Optional[date] = None
    is_completed: Optional[bool] = False

class GoalResponse(GoalCreate):
    goal_id: int

    class Config:
        from_attributes = True

class IncomeCreate(BaseModel):
    user_id: int
    amount: float = Field(..., gt=0)
    source: str = Field(..., max_length=100)
    date: date
    is_recurring: Optional[bool] = False

class IncomeResponse(IncomeCreate):
    income_id: int

    class Config:
        from_attributes = True

class TransactionCreate(BaseModel):
    user_id: int
    amount: float
    type: str = Field(..., max_length=20)  # 'income' or 'expense'
    category: str = Field(..., max_length=50)
    date: datetime
    description: Optional[str] = Field(None, max_length=200)

class TransactionResponse(TransactionCreate):
    transaction_id: int

    class Config:
        from_attributes = True

# Утилиты для работы с БД
def delete_db_if_exists():
    """Удаляет существующую БД, если она есть"""
    if os.path.exists(DATABASE_FILE):
        os.remove(DATABASE_FILE)
        print(f"Удалена существующая БД: {DATABASE_FILE}")
        return True
    return False

@contextmanager
def get_db_connection():
    """Контекстный менеджер для подключения к БД"""
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

def check_table_exists(table_name: str) -> bool:
    """Проверяет существование таблицы в БД"""
    with get_db_connection() as conn:
        cursor = conn.cursor()
        cursor.execute(f'''
        SELECT name FROM sqlite_master 
        WHERE type='table' AND name='{table_name}'
        ''')
        return cursor.fetchone() is not None

def init_db():
    """Инициализация базы данных и создание всех таблиц"""
    print(f"Инициализация базы данных...")
    print(f"Путь к БД: {DATABASE_FILE}")
    
    # Проверяем, существует ли директория
    db_dir = os.path.dirname(DATABASE_FILE)
    if not os.path.exists(db_dir):
        os.makedirs(db_dir, exist_ok=True)
        print(f"Создана директория: {db_dir}")
    
    with get_db_connection() as conn:
        cursor = conn.cursor()
        
        # Проверяем и создаем таблицу Users, если она не существует
        if not check_table_exists('Users'):
            cursor.execute('''
            CREATE TABLE Users (
                user_id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                email TEXT UNIQUE NOT NULL,
                registration_date DATE DEFAULT CURRENT_DATE,
                total_saved REAL DEFAULT 0,
                current_balance REAL DEFAULT 0
            )
            ''')
            print("✓ Таблица Users создана")
        else:
            print("✓ Таблица Users уже существует")
        
        # Проверяем и создаем таблицу Budget, если она не существует
        if not check_table_exists('Budget'):
            cursor.execute('''
            CREATE TABLE Budget (
                budget_id INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id INTEGER NOT NULL,
                category TEXT NOT NULL,
                amount REAL NOT NULL,
                period TEXT NOT NULL,
                start_date DATE NOT NULL,
                end_date DATE,
                FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE
            )
            ''')
            print("✓ Таблица Budget создана")
        else:
            print("✓ Таблица Budget уже существует")
        
        # Проверяем и создаем таблицу Expenses, если она не существует
        if not check_table_exists('Expenses'):
            cursor.execute('''
            CREATE TABLE Expenses (
                expense_id INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id INTEGER NOT NULL,
                amount REAL NOT NULL,
                category TEXT NOT NULL,
                description TEXT,
                date DATE NOT NULL,
                is_planned BOOLEAN DEFAULT FALSE,
                FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE
            )
            ''')
            print("✓ Таблица Expenses создана")
        else:
            print("✓ Таблица Expenses уже существует")
        
        # Проверяем и создаем таблицу FinancialGoals, если она не существует
        if not check_table_exists('FinancialGoals'):
            cursor.execute('''
            CREATE TABLE FinancialGoals (
                goal_id INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id INTEGER NOT NULL,
                target_amount REAL NOT NULL,
                current_amount REAL DEFAULT 0,
                name TEXT NOT NULL,
                deadline DATE,
                is_completed BOOLEAN DEFAULT FALSE,
                FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE
            )
            ''')
            print("✓ Таблица FinancialGoals создана")
        else:
            print("✓ Таблица FinancialGoals уже существует")
        
        # Проверяем и создаем таблицу Income, если она не существует
        if not check_table_exists('Income'):
            cursor.execute('''
            CREATE TABLE Income (
                income_id INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id INTEGER NOT NULL,
                amount REAL NOT NULL,
                source TEXT NOT NULL,
                date DATE NOT NULL,
                is_recurring BOOLEAN DEFAULT FALSE,
                FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE
            )
            ''')
            print("✓ Таблица Income создана")
        else:
            print("✓ Таблица Income уже существует")
        
        # Проверяем и создаем таблицу Transactions, если она не существует
        if not check_table_exists('Transactions'):
            cursor.execute('''
            CREATE TABLE Transactions (
                transaction_id INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id INTEGER NOT NULL,
                amount REAL NOT NULL,
                type TEXT NOT NULL,
                category TEXT NOT NULL,
                date DATETIME NOT NULL,
                description TEXT,
                FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE
            )
            ''')
            print("✓ Таблица Transactions создана")
        else:
            print("✓ Таблица Transactions уже существует")
        
        print("✅ Все таблицы проверены/созданы")
        
        # Создание индексов для ускорения запросов
        indexes = [
            ('idx_users_email', 'CREATE INDEX IF NOT EXISTS idx_users_email ON Users(email)'),
            ('idx_expenses_user_date', 'CREATE INDEX IF NOT EXISTS idx_expenses_user_date ON Expenses(user_id, date)'),
            ('idx_income_user_date', 'CREATE INDEX IF NOT EXISTS idx_income_user_date ON Income(user_id, date)'),
            ('idx_transactions_user_date', 'CREATE INDEX IF NOT EXISTS idx_transactions_user_date ON Transactions(user_id, date)'),
            ('idx_goals_user_completed', 'CREATE INDEX IF NOT EXISTS idx_goals_user_completed ON FinancialGoals(user_id, is_completed)'),
        ]
        
        for idx_name, idx_query in indexes:
            try:
                cursor.execute(idx_query)
                print(f"✓ Индекс {idx_name} создан")
            except Exception as e:
                print(f"⚠ Ошибка при создании индекса {idx_name}: {e}")
        
        # Создание триггеров
        try:
            # Удаляем старые триггеры, если они существуют
            cursor.execute("DROP TRIGGER IF EXISTS update_balance_after_expense")
            cursor.execute("DROP TRIGGER IF EXISTS update_balance_after_income")
            
            # Создаем новые триггеры
            cursor.execute('''
            CREATE TRIGGER update_balance_after_expense
            AFTER INSERT ON Expenses
            FOR EACH ROW
            BEGIN
                UPDATE Users 
                SET current_balance = current_balance - NEW.amount
                WHERE user_id = NEW.user_id;
                
                INSERT INTO Transactions (user_id, amount, type, category, date, description)
                VALUES (NEW.user_id, NEW.amount, 'expense', NEW.category, 
                        datetime(NEW.date || ' 00:00:00'), NEW.description);
            END;
            ''')
            print("✓ Триггер update_balance_after_expense создан")
            
            cursor.execute('''
            CREATE TRIGGER update_balance_after_income
            AFTER INSERT ON Income
            FOR EACH ROW
            BEGIN
                UPDATE Users 
                SET current_balance = current_balance + NEW.amount
                WHERE user_id = NEW.user_id;
                
                INSERT INTO Transactions (user_id, amount, type, category, date, description)
                VALUES (NEW.user_id, NEW.amount, 'income', NEW.source, 
                        datetime(NEW.date || ' 00:00:00'), 
                        'Income from ' || NEW.source);
            END;
            ''')
            print("✓ Триггер update_balance_after_income создан")
        except Exception as e:
            print(f"⚠ Ошибка при создании триггеров: {e}")
        
        print(f"✅ База данных инициализирована: {DATABASE_FILE}")
        print(f"✅ Размер файла: {os.path.getsize(DATABASE_FILE)} байт")

def create_sample_data():
    """Создание тестовых данных для демонстрации"""
    with get_db_connection() as conn:
        cursor = conn.cursor()
        
        try:
            # Проверяем, есть ли уже пользователи
            cursor.execute('SELECT COUNT(*) as count FROM Users')
            if cursor.fetchone()['count'] > 0:
                print("✓ Тестовые данные уже существуют")
                return
            
            # Создаем тестового пользователя
            cursor.execute('''
            INSERT INTO Users (name, email, total_saved, current_balance)
            VALUES ('Иван Иванов', 'ivan@example.com', 50000, 150000)
            ''')
            
            # Создаем бюджет
            cursor.execute('''
            INSERT INTO Budget (user_id, category, amount, period, start_date, end_date)
            VALUES (1, 'Продукты', 20000, 'monthly', '2024-01-01', '2024-12-31')
            ''')
            
            # Создаем расход
            cursor.execute('''
            INSERT INTO Expenses (user_id, amount, category, description, date)
            VALUES (1, 1500, 'Продукты', 'Покупки в магазине', '2024-01-15')
            ''')
            
            # Создаем финансовую цель
            cursor.execute('''
            INSERT INTO FinancialGoals (user_id, target_amount, current_amount, name, deadline)
            VALUES (1, 100000, 50000, 'Новый автомобиль', '2024-12-31')
            ''')
            
            # Создаем доход
            cursor.execute('''
            INSERT INTO Income (user_id, amount, source, date, is_recurring)
            VALUES (1, 50000, 'Зарплата', '2024-01-30', 1)
            ''')
            
            print("✅ Тестовые данные созданы")
            
        except Exception as e:
            print(f"⚠ Ошибка при создании тестовых данных: {e}")

# API Endpoints для Users
@app.post("/users/", response_model=UserResponse, status_code=status.HTTP_201_CREATED)
def create_user(user: UserCreate):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        try:
            cursor.execute('''
            INSERT INTO Users (name, email, registration_date, total_saved, current_balance)
            VALUES (?, ?, COALESCE(?, CURRENT_DATE), ?, ?)
            ''', (user.name, user.email, user.registration_date, 
                  user.total_saved, user.current_balance))
            user_id = cursor.lastrowid
            
            cursor.execute('SELECT * FROM Users WHERE user_id = ?', (user_id,))
            return dict(cursor.fetchone())
        except sqlite3.IntegrityError:
            raise HTTPException(status_code=400, detail="Email уже существует")

@app.get("/users/", response_model=List[UserResponse])
def get_users():
    with get_db_connection() as conn:
        cursor = conn.cursor()
        cursor.execute('SELECT * FROM Users')
        return [dict(row) for row in cursor.fetchall()]

@app.get("/users/{user_id}", response_model=UserResponse)
def get_user(user_id: int):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        cursor.execute('SELECT * FROM Users WHERE user_id = ?', (user_id,))
        user = cursor.fetchone()
        if not user:
            raise HTTPException(status_code=404, detail="Пользователь не найден")
        return dict(user)

@app.put("/users/{user_id}", response_model=UserResponse)
def update_user(user_id: int, user: UserCreate):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        cursor.execute('''
        UPDATE Users 
        SET name = ?, email = ?, registration_date = ?, total_saved = ?, current_balance = ?
        WHERE user_id = ?
        ''', (user.name, user.email, user.registration_date, 
              user.total_saved, user.current_balance, user_id))
        
        if cursor.rowcount == 0:
            raise HTTPException(status_code=404, detail="Пользователь не найден")
        
        cursor.execute('SELECT * FROM Users WHERE user_id = ?', (user_id,))
        return dict(cursor.fetchone())

@app.delete("/users/{user_id}", status_code=status.HTTP_204_NO_CONTENT)
def delete_user(user_id: int):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        cursor.execute('DELETE FROM Users WHERE user_id = ?', (user_id,))
        if cursor.rowcount == 0:
            raise HTTPException(status_code=404, detail="Пользователь не найден")

# API Endpoints для Budget
@app.post("/budgets/", response_model=BudgetResponse, status_code=status.HTTP_201_CREATED)
def create_budget(budget: BudgetCreate):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        cursor.execute('''
        INSERT INTO Budget (user_id, category, amount, period, start_date, end_date)
        VALUES (?, ?, ?, ?, ?, ?)
        ''', (budget.user_id, budget.category, budget.amount, 
              budget.period, budget.start_date, budget.end_date))
        budget_id = cursor.lastrowid
        
        cursor.execute('SELECT * FROM Budget WHERE budget_id = ?', (budget_id,))
        return dict(cursor.fetchone())

@app.get("/budgets/", response_model=List[BudgetResponse])
def get_budgets(user_id: Optional[int] = None):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        if user_id:
            cursor.execute('SELECT * FROM Budget WHERE user_id = ?', (user_id,))
        else:
            cursor.execute('SELECT * FROM Budget')
        return [dict(row) for row in cursor.fetchall()]

# API Endpoints для Expenses
@app.post("/expenses/", response_model=ExpenseResponse, status_code=status.HTTP_201_CREATED)
def create_expense(expense: ExpenseCreate):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        cursor.execute('''
        INSERT INTO Expenses (user_id, amount, category, description, date, is_planned)
        VALUES (?, ?, ?, ?, ?, ?)
        ''', (expense.user_id, expense.amount, expense.category, 
              expense.description, expense.date, expense.is_planned))
        expense_id = cursor.lastrowid
        
        cursor.execute('SELECT * FROM Expenses WHERE expense_id = ?', (expense_id,))
        return dict(cursor.fetchone())

@app.get("/expenses/", response_model=List[ExpenseResponse])
def get_expenses(user_id: Optional[int] = None, start_date: Optional[date] = None, 
                 end_date: Optional[date] = None):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        query = 'SELECT * FROM Expenses WHERE 1=1'
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

# API Endpoints для FinancialGoals
@app.post("/goals/", response_model=GoalResponse, status_code=status.HTTP_201_CREATED)
def create_goal(goal: GoalCreate):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        cursor.execute('''
        INSERT INTO FinancialGoals (user_id, target_amount, current_amount, name, deadline, is_completed)
        VALUES (?, ?, ?, ?, ?, ?)
        ''', (goal.user_id, goal.target_amount, goal.current_amount, 
              goal.name, goal.deadline, goal.is_completed))
        goal_id = cursor.lastrowid
        
        cursor.execute('SELECT * FROM FinancialGoals WHERE goal_id = ?', (goal_id,))
        return dict(cursor.fetchone())

@app.get("/goals/", response_model=List[GoalResponse])
def get_goals(user_id: Optional[int] = None, completed: Optional[bool] = None):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        query = 'SELECT * FROM FinancialGoals WHERE 1=1'
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

@app.post("/goals/{goal_id}/add_money/", response_model=GoalResponse)
def add_money_to_goal(goal_id: int, amount: float):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        cursor.execute('SELECT * FROM FinancialGoals WHERE goal_id = ?', (goal_id,))
        goal = cursor.fetchone()
        if not goal:
            raise HTTPException(status_code=404, detail="Цель не найдена")
        
        cursor.execute('''
        UPDATE FinancialGoals 
        SET current_amount = current_amount + ?
        WHERE goal_id = ?
        ''', (amount, goal_id))
        
        cursor.execute('''
        UPDATE FinancialGoals 
        SET is_completed = CASE WHEN current_amount >= target_amount THEN TRUE ELSE FALSE END
        WHERE goal_id = ?
        ''', (goal_id,))
        
        cursor.execute('''
        UPDATE Users 
        SET total_saved = total_saved + ?
        WHERE user_id = ?
        ''', (amount, goal['user_id']))
        
        cursor.execute('SELECT * FROM FinancialGoals WHERE goal_id = ?', (goal_id,))
        return dict(cursor.fetchone())

# API Endpoints для Income
@app.post("/income/", response_model=IncomeResponse, status_code=status.HTTP_201_CREATED)
def create_income(income: IncomeCreate):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        cursor.execute('''
        INSERT INTO Income (user_id, amount, source, date, is_recurring)
        VALUES (?, ?, ?, ?, ?)
        ''', (income.user_id, income.amount, income.source, 
              income.date, income.is_recurring))
        income_id = cursor.lastrowid
        
        cursor.execute('SELECT * FROM Income WHERE income_id = ?', (income_id,))
        return dict(cursor.fetchone())

@app.get("/income/", response_model=List[IncomeResponse])
def get_income(user_id: Optional[int] = None, start_date: Optional[date] = None, 
               end_date: Optional[date] = None):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        query = 'SELECT * FROM Income WHERE 1=1'
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

# API Endpoints для Transactions
@app.get("/transactions/", response_model=List[TransactionResponse])
def get_transactions(user_id: Optional[int] = None, start_date: Optional[datetime] = None,
                     end_date: Optional[datetime] = None, transaction_type: Optional[str] = None):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        query = 'SELECT * FROM Transactions WHERE 1=1'
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
        if transaction_type:
            query += ' AND type = ?'
            params.append(transaction_type)
        
        query += ' ORDER BY date DESC'
        cursor.execute(query, params)
        return [dict(row) for row in cursor.fetchall()]

# Статистика пользователя
@app.get("/users/{user_id}/stats")
def get_user_stats(user_id: int):
    with get_db_connection() as conn:
        cursor = conn.cursor()
        
        cursor.execute('SELECT * FROM Users WHERE user_id = ?', (user_id,))
        user = cursor.fetchone()
        if not user:
            raise HTTPException(status_code=404, detail="Пользователь не найден")
        
        cursor.execute('''
        SELECT 
            SUM(amount) as total_expenses,
            COUNT(*) as expense_count
        FROM Expenses 
        WHERE user_id = ? 
        AND date >= date('now', 'start of month')
        ''', (user_id,))
        expense_stats = cursor.fetchone()
        
        cursor.execute('''
        SELECT 
            SUM(amount) as total_income,
            COUNT(*) as income_count
        FROM Income 
        WHERE user_id = ? 
        AND date >= date('now', 'start of month')
        ''', (user_id,))
        income_stats = cursor.fetchone()
        
        cursor.execute('''
        SELECT COUNT(*) as active_goals 
        FROM FinancialGoals 
        WHERE user_id = ? AND is_completed = FALSE
        ''', (user_id,))
        goals_stats = cursor.fetchone()
        
        cursor.execute('''
        SELECT COUNT(*) as active_budgets 
        FROM Budget 
        WHERE user_id = ? AND (end_date IS NULL OR end_date >= date('now'))
        ''', (user_id,))
        budget_stats = cursor.fetchone()
        
        return {
            "user": dict(user),
            "current_month": {
                "expenses": {
                    "total": expense_stats["total_expenses"] or 0,
                    "count": expense_stats["expense_count"] or 0
                },
                "income": {
                    "total": income_stats["total_income"] or 0,
                    "count": income_stats["income_count"] or 0
                }
            },
            "goals": {
                "active": goals_stats["active_goals"] or 0
            },
            "budgets": {
                "active": budget_stats["active_budgets"] or 0
            }
        }

# Управление БД
@app.post("/db/reset")
def reset_database():
    """Сброс базы данных (для тестирования)"""
    deleted = delete_db_if_exists()
    if deleted:
        init_db()
        return {"message": "База данных сброшена и пересоздана", "path": DATABASE_FILE}
    else:
        init_db()
        return {"message": "Создана новая база данных", "path": DATABASE_FILE}

@app.get("/db/info")
def get_database_info():
    """Получение информации о БД"""
    db_exists = os.path.exists(DATABASE_FILE)
    
    if not db_exists:
        return {
            "database_file": DATABASE_FILE,
            "status": "not_exists",
            "message": "База данных еще не создана",
            "project_directory": PROJECT_DIR
        }
    
    with get_db_connection() as conn:
        cursor = conn.cursor()
        
        # Получаем список всех таблиц
        cursor.execute("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name")
        tables = [row["name"] for row in cursor.fetchall()]
        
        # Получаем количество записей в каждой таблице
        table_counts = {}
        for table in tables:
            cursor.execute(f"SELECT COUNT(*) as count FROM {table}")
            table_counts[table] = cursor.fetchone()["count"]
        
        # Получаем размер файла
        file_size = os.path.getsize(DATABASE_FILE)
        
        # Получаем информацию о таблицах
        tables_info = []
        for table in tables:
            cursor.execute(f"PRAGMA table_info({table})")
            columns = [{"name": col[1], "type": col[2]} for col in cursor.fetchall()]
            tables_info.append({
                "name": table,
                "columns": columns,
                "row_count": table_counts[table]
            })
        
        return {
            "database_file": DATABASE_FILE,
            "project_directory": PROJECT_DIR,
            "status": "exists",
            "file_size_bytes": file_size,
            "file_size_mb": round(file_size / (1024 * 1024), 2),
            "tables_count": len(tables),
            "tables": tables_info
        }

# Создание тестовых данных
@app.post("/db/create_sample_data")
def create_sample_data_endpoint():
    """Создание тестовых данных"""
    create_sample_data()
    return {"message": "Тестовые данные созданы", "path": DATABASE_FILE}

# Проверка пути к БД
@app.get("/db/path")
def get_database_path():
    """Получение пути к базе данных"""
    return {
        "database_file": DATABASE_FILE,
        "project_directory": PROJECT_DIR,
        "exists": os.path.exists(DATABASE_FILE),
        "absolute_path": os.path.abspath(DATABASE_FILE),
        "directory_exists": os.path.exists(PROJECT_DIR)
    }

# Корневой endpoint
@app.get("/")
def read_root():
    return {
        "message": "Добро пожаловать в API Финансового Тамагоччи!",
        "version": "1.0.0",
        "database": {
            "path": DATABASE_FILE,
            "project_directory": PROJECT_DIR,
            "info_endpoint": "/db/info",
            "path_endpoint": "/db/path"
        },
        "endpoints": {
            "users": {
                "create": "POST /users/",
                "list": "GET /users/",
                "get": "GET /users/{user_id}",
                "update": "PUT /users/{user_id}",
                "delete": "DELETE /users/{user_id}",
                "stats": "GET /users/{user_id}/stats"
            },
            "budgets": {
                "create": "POST /budgets/",
                "list": "GET /budgets/"
            },
            "expenses": {
                "create": "POST /expenses/",
                "list": "GET /expenses/"
            },
            "goals": {
                "create": "POST /goals/",
                "list": "GET /goals/",
                "add_money": "POST /goals/{goal_id}/add_money/"
            },
            "income": {
                "create": "POST /income/",
                "list": "GET /income/"
            },
            "transactions": {
                "list": "GET /transactions/"
            },
            "database": {
                "info": "GET /db/info",
                "path": "GET /db/path",
                "reset": "POST /db/reset",
                "create_sample": "POST /db/create_sample_data"
            }
        }
    }

if __name__ == "__main__":
    print("=" * 60)
    print("ЗАПУСК СЕРВЕРА ФИНАНСОВОГО ТАМАГОЧЧИ")
    print("=" * 60)
    print(f"Проектная директория: {PROJECT_DIR}")
    print(f"Файл базы данных: {DATABASE_FILE}")
    print(f"Абсолютный путь: {os.path.abspath(DATABASE_FILE)}")
    print("=" * 60)
    print("Используйте Ctrl+C для остановки сервера\n")
    
    # Проверяем и создаем директорию
    if not os.path.exists(PROJECT_DIR):
        print(f"Создаю директорию: {PROJECT_DIR}")
        os.makedirs(PROJECT_DIR, exist_ok=True)
    
    uvicorn.run(app, host="0.0.0.0", port=8000, log_level="info")