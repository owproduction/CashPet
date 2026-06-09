from fastapi import FastAPI, HTTPException, Depends, status
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field
from typing import List, Optional
from datetime import datetime, date
import asyncpg
import uvicorn
import os
import random
from contextlib import asynccontextmanager

# Конфигурация PostgreSQL
DATABASE_URL = os.getenv("DATABASE_URL", "postgresql://postgres:123Secret_a@postgres:5432/financial_tamagotchi")

# Глобальный пул соединений
db_pool = None

# Модели Pydantic для API
class UserCreate(BaseModel):
    name: str = Field(..., min_length=2, max_length=100)
    email: str = Field(..., max_length=100)
    registration_date: Optional[date] = None
    total_saved: Optional[float] = 0.0
    current_balance: Optional[float] = 0.0
    food_currency: Optional[int] = 100
    pet_energy: Optional[int] = 80

class UserResponse(UserCreate):
    user_id: int

    class Config:
        from_attributes = True

class BudgetCreate(BaseModel):
    user_id: int
    category: str = Field(..., max_length=50)
    amount: float = Field(..., gt=0)
    period: str = Field(..., max_length=20)
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
    type: str = Field(..., max_length=20)
    category: str = Field(..., max_length=50)
    date: datetime
    description: Optional[str] = Field(None, max_length=200)

class TransactionResponse(TransactionCreate):
    transaction_id: int

# Функции для работы с БД
async def init_db():
    """Инициализация базы данных"""
    global db_pool
    db_pool = await asyncpg.create_pool(DATABASE_URL, min_size=1, max_size=10)
    
    async with db_pool.acquire() as conn:
        # Таблица пользователей
        await conn.execute('''
        CREATE TABLE IF NOT EXISTS users (
            user_id SERIAL PRIMARY KEY,
            name TEXT NOT NULL,
            email TEXT UNIQUE NOT NULL,
            registration_date DATE DEFAULT CURRENT_DATE,
            total_saved REAL DEFAULT 0,
            current_balance REAL DEFAULT 0,
            food_currency INTEGER DEFAULT 100,
            pet_energy INTEGER DEFAULT 80,
            last_feed_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        )
        ''')
        
        # Таблица целей
        await conn.execute('''
        CREATE TABLE IF NOT EXISTS goals (
            goal_id SERIAL PRIMARY KEY,
            user_id INTEGER NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
            target_amount REAL NOT NULL,
            current_amount REAL DEFAULT 0,
            name TEXT NOT NULL,
            deadline DATE,
            is_completed BOOLEAN DEFAULT FALSE,
            reward_amount INTEGER DEFAULT 50,
            reward_claimed BOOLEAN DEFAULT FALSE
        )
        ''')
        
        # Таблица расходов
        await conn.execute('''
        CREATE TABLE IF NOT EXISTS expenses (
            expense_id SERIAL PRIMARY KEY,
            user_id INTEGER NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
            amount REAL NOT NULL,
            category TEXT NOT NULL,
            description TEXT,
            date DATE NOT NULL,
            is_planned BOOLEAN DEFAULT FALSE
        )
        ''')
        
        # Таблица доходов
        await conn.execute('''
        CREATE TABLE IF NOT EXISTS incomes (
            income_id SERIAL PRIMARY KEY,
            user_id INTEGER NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
            amount REAL NOT NULL,
            source TEXT NOT NULL,
            date DATE NOT NULL,
            is_recurring BOOLEAN DEFAULT FALSE
        )
        ''')
        
        # Таблица транзакций
        await conn.execute('''
        CREATE TABLE IF NOT EXISTS transactions (
            transaction_id SERIAL PRIMARY KEY,
            user_id INTEGER NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
            amount REAL NOT NULL,
            type TEXT NOT NULL,
            category TEXT NOT NULL,
            date TIMESTAMP NOT NULL,
            description TEXT
        )
        ''')
        
        # Таблица бюджетов
        await conn.execute('''
        CREATE TABLE IF NOT EXISTS budgets (
            budget_id SERIAL PRIMARY KEY,
            user_id INTEGER NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
            category TEXT NOT NULL,
            amount REAL NOT NULL,
            period TEXT NOT NULL,
            start_date DATE NOT NULL,
            end_date DATE
        )
        ''')
        
        # Индексы
        await conn.execute('CREATE INDEX IF NOT EXISTS idx_users_email ON users(email)')
        await conn.execute('CREATE INDEX IF NOT EXISTS idx_transactions_user_date ON transactions(user_id, date)')
        await conn.execute('CREATE INDEX IF NOT EXISTS idx_goals_user ON goals(user_id)')
        
        print("✅ PostgreSQL база данных инициализирована")


async def get_db():
    """Получение соединения с БД"""
    async with db_pool.acquire() as conn:
        yield conn


# Инициализация FastAPI приложения
@asynccontextmanager
async def lifespan(app: FastAPI):
    # Startup
    await init_db()
    yield
    # Shutdown
    if db_pool:
        await db_pool.close()

app = FastAPI(
    title="Финансовый Тамагоччи API",
    description="API для игры Финансовый Тамагоччи",
    version="1.0.0",
    lifespan=lifespan
)

# Настройка CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


# ========== API Endpoints ==========

@app.get("/")
async def read_root():
    return {
        "message": "Финансовый Тамагоччи API",
        "version": "1.0.0",
        "database": "PostgreSQL",
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
async def create_user(user: UserCreate):
    async with db_pool.acquire() as conn:
        try:
            row = await conn.fetchrow('''
            INSERT INTO users (name, email, registration_date, total_saved, current_balance, food_currency, pet_energy)
            VALUES ($1, $2, COALESCE($3, CURRENT_DATE), $4, $5, $6, $7)
            RETURNING *
            ''', user.name, user.email, user.registration_date, 
               user.total_saved, user.current_balance, user.food_currency, user.pet_energy)
            
            if row:
                return dict(row)
        except Exception as e:
            if "23505" in str(e):  # Unique violation
                raise HTTPException(status_code=400, detail="Email уже существует")
            raise HTTPException(status_code=400, detail=str(e))


@app.get("/users/", response_model=List[UserResponse])
async def get_users():
    async with db_pool.acquire() as conn:
        rows = await conn.fetch('SELECT * FROM users ORDER BY user_id')
        return [dict(row) for row in rows]


@app.get("/users/{user_id}", response_model=UserResponse)
async def get_user(user_id: int):
    async with db_pool.acquire() as conn:
        row = await conn.fetchrow('SELECT * FROM users WHERE user_id = $1', user_id)
        if not row:
            raise HTTPException(status_code=404, detail="Пользователь не найден")
        return dict(row)


@app.put("/users/{user_id}", response_model=UserResponse)
async def update_user(user_id: int, user: UserCreate):
    async with db_pool.acquire() as conn:
        result = await conn.execute('''
        UPDATE users 
        SET name = $1, email = $2, registration_date = $3, total_saved = $4, 
            current_balance = $5, food_currency = $6, pet_energy = $7
        WHERE user_id = $8
        ''', user.name, user.email, user.registration_date, 
           user.total_saved, user.current_balance, user.food_currency, 
           user.pet_energy, user_id)
        
        if result == "UPDATE 0":
            raise HTTPException(status_code=404, detail="Пользователь не найден")
        
        row = await conn.fetchrow('SELECT * FROM users WHERE user_id = $1', user_id)
        return dict(row)


@app.delete("/users/{user_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_user(user_id: int):
    async with db_pool.acquire() as conn:
        result = await conn.execute('DELETE FROM users WHERE user_id = $1', user_id)
        if result == "DELETE 0":
            raise HTTPException(status_code=404, detail="Пользователь не найден")


# ========== РАСХОДЫ ==========
@app.post("/expenses/", response_model=ExpenseResponse, status_code=status.HTTP_201_CREATED)
async def create_expense(expense: ExpenseCreate):
    async with db_pool.acquire() as conn:
        async with conn.transaction():
            # Проверяем баланс
            user = await conn.fetchrow('SELECT current_balance, pet_energy FROM users WHERE user_id = $1', expense.user_id)
            if not user:
                raise HTTPException(status_code=404, detail="Пользователь не найден")
            
            if user['current_balance'] < expense.amount:
                raise HTTPException(status_code=400, detail="Недостаточно средств")
            
            # Добавляем расход
            row = await conn.fetchrow('''
            INSERT INTO expenses (user_id, amount, category, description, date, is_planned)
            VALUES ($1, $2, $3, $4, $5, $6)
            RETURNING *
            ''', expense.user_id, expense.amount, expense.category, 
               expense.description, expense.date, expense.is_planned)
            
            # Обновляем баланс и энергию
            new_energy = max(0, user['pet_energy'] - 5)
            await conn.execute('''
            UPDATE users 
            SET current_balance = current_balance - $1,
                pet_energy = $2
            WHERE user_id = $3
            ''', expense.amount, new_energy, expense.user_id)
            
            # Добавляем транзакцию
            await conn.execute('''
            INSERT INTO transactions (user_id, amount, type, category, date, description)
            VALUES ($1, $2, 'expense', $3, $4, $5)
            ''', expense.user_id, expense.amount, expense.category, 
               datetime.combine(expense.date, datetime.min.time()), expense.description)
            
            return dict(row)


@app.get("/expenses/", response_model=List[ExpenseResponse])
async def get_expenses(user_id: Optional[int] = None, start_date: Optional[date] = None, end_date: Optional[date] = None):
    async with db_pool.acquire() as conn:
        query = 'SELECT * FROM expenses WHERE 1=1'
        params = []
        param_index = 1
        
        if user_id:
            query += f' AND user_id = ${param_index}'
            params.append(user_id)
            param_index += 1
        if start_date:
            query += f' AND date >= ${param_index}'
            params.append(start_date)
            param_index += 1
        if end_date:
            query += f' AND date <= ${param_index}'
            params.append(end_date)
            param_index += 1
        
        query += ' ORDER BY date DESC'
        rows = await conn.fetch(query, *params)
        return [dict(row) for row in rows]


# ========== ДОХОДЫ ==========
@app.post("/incomes/", response_model=IncomeResponse, status_code=status.HTTP_201_CREATED)
async def create_income(income: IncomeCreate):
    async with db_pool.acquire() as conn:
        async with conn.transaction():
            user = await conn.fetchrow('SELECT pet_energy FROM users WHERE user_id = $1', income.user_id)
            if not user:
                raise HTTPException(status_code=404, detail="Пользователь не найден")
            
            # Добавляем доход
            row = await conn.fetchrow('''
            INSERT INTO incomes (user_id, amount, source, date, is_recurring)
            VALUES ($1, $2, $3, $4, $5)
            RETURNING *
            ''', income.user_id, income.amount, income.source, income.date, income.is_recurring)
            
            # Обновляем баланс, энергию и корм
            new_energy = min(100, user['pet_energy'] + 10)
            await conn.execute('''
            UPDATE users 
            SET current_balance = current_balance + $1,
                pet_energy = $2,
                food_currency = food_currency + 10
            WHERE user_id = $3
            ''', income.amount, new_energy, income.user_id)
            
            # Добавляем транзакцию
            await conn.execute('''
            INSERT INTO transactions (user_id, amount, type, category, date, description)
            VALUES ($1, $2, 'income', $3, $4, $5)
            ''', income.user_id, income.amount, income.source, 
               datetime.combine(income.date, datetime.min.time()), f"Доход от {income.source}")
            
            return dict(row)


@app.get("/incomes/", response_model=List[IncomeResponse])
async def get_incomes(user_id: Optional[int] = None, start_date: Optional[date] = None, end_date: Optional[date] = None):
    async with db_pool.acquire() as conn:
        query = 'SELECT * FROM incomes WHERE 1=1'
        params = []
        param_index = 1
        
        if user_id:
            query += f' AND user_id = ${param_index}'
            params.append(user_id)
            param_index += 1
        if start_date:
            query += f' AND date >= ${param_index}'
            params.append(start_date)
            param_index += 1
        if end_date:
            query += f' AND date <= ${param_index}'
            params.append(end_date)
            param_index += 1
        
        query += ' ORDER BY date DESC'
        rows = await conn.fetch(query, *params)
        return [dict(row) for row in rows]


# ========== ЦЕЛИ ==========
@app.post("/goals/", response_model=GoalResponse, status_code=status.HTTP_201_CREATED)
async def create_goal(goal: GoalCreate):
    async with db_pool.acquire() as conn:
        row = await conn.fetchrow('''
        INSERT INTO goals (user_id, target_amount, current_amount, name, deadline, is_completed, reward_amount, reward_claimed)
        VALUES ($1, $2, $3, $4, $5, $6, $7, $8)
        RETURNING *
        ''', goal.user_id, goal.target_amount, goal.current_amount, 
           goal.name, goal.deadline, goal.is_completed, goal.reward_amount, goal.reward_claimed)
        return dict(row)


@app.get("/goals/", response_model=List[GoalResponse])
async def get_goals(user_id: Optional[int] = None, completed: Optional[bool] = None):
    async with db_pool.acquire() as conn:
        query = 'SELECT * FROM goals WHERE 1=1'
        params = []
        param_index = 1
        
        if user_id:
            query += f' AND user_id = ${param_index}'
            params.append(user_id)
            param_index += 1
        if completed is not None:
            query += f' AND is_completed = ${param_index}'
            params.append(completed)
            param_index += 1
        
        query += ' ORDER BY deadline ASC'
        rows = await conn.fetch(query, *params)
        return [dict(row) for row in rows]


@app.post("/goals/{goal_id}/add_money")
async def add_money_to_goal(goal_id: int, amount: float, user_id: int):
    async with db_pool.acquire() as conn:
        async with conn.transaction():
            goal = await conn.fetchrow('SELECT * FROM goals WHERE goal_id = $1', goal_id)
            if not goal:
                raise HTTPException(status_code=404, detail="Цель не найдена")
            
            user = await conn.fetchrow('SELECT current_balance FROM users WHERE user_id = $1', user_id)
            if user['current_balance'] < amount:
                raise HTTPException(status_code=400, detail="Недостаточно средств")
            
            new_amount = goal['current_amount'] + amount
            await conn.execute('''
            UPDATE goals 
            SET current_amount = $1,
                is_completed = $2 >= target_amount
            WHERE goal_id = $3
            ''', new_amount, new_amount, goal_id)
            
            await conn.execute('''
            UPDATE users SET current_balance = current_balance - $1 WHERE user_id = $2
            ''', amount, user_id)
            
            if random.random() < 0.2:
                bonus = random.randint(1, 5)
                await conn.execute('''
                UPDATE users SET food_currency = food_currency + $1 WHERE user_id = $2
                ''', bonus, user_id)
                
                result = await conn.fetchrow('SELECT * FROM goals WHERE goal_id = $1', goal_id)
                result_dict = dict(result)
                result_dict['bonus'] = bonus
                return result_dict
            
            result = await conn.fetchrow('SELECT * FROM goals WHERE goal_id = $1', goal_id)
            return dict(result)


@app.post("/goals/{goal_id}/claim_reward")
async def claim_goal_reward(goal_id: int, user_id: int):
    async with db_pool.acquire() as conn:
        async with conn.transaction():
            goal = await conn.fetchrow('SELECT * FROM goals WHERE goal_id = $1 AND user_id = $2', goal_id, user_id)
            if not goal:
                raise HTTPException(status_code=404, detail="Цель не найдена")
            
            if not goal['is_completed']:
                raise HTTPException(status_code=400, detail="Цель ещё не выполнена")
            
            if goal['reward_claimed']:
                raise HTTPException(status_code=400, detail="Награда уже получена")
            
            await conn.execute('''
            UPDATE users SET food_currency = food_currency + $1 WHERE user_id = $2
            ''', goal['reward_amount'], user_id)
            
            await conn.execute('''
            UPDATE goals SET reward_claimed = TRUE WHERE goal_id = $1
            ''', goal_id)
            
            return {"message": "Награда получена", "reward": goal['reward_amount']}


# ========== ТРАНЗАКЦИИ ==========
@app.get("/transactions/", response_model=List[TransactionResponse])
async def get_transactions(user_id: int, days: Optional[int] = 30):
    async with db_pool.acquire() as conn:
        if days:
            rows = await conn.fetch('''
            SELECT * FROM transactions 
            WHERE user_id = $1 AND date >= NOW() - $2 * INTERVAL '1 day'
            ORDER BY date DESC
            ''', user_id, days)
        else:
            rows = await conn.fetch('''
            SELECT * FROM transactions 
            WHERE user_id = $1
            ORDER BY date DESC
            ''', user_id)
        
        return [dict(row) for row in rows]


@app.get("/transactions/stats/{user_id}")
async def get_transaction_stats(user_id: int):
    async with db_pool.acquire() as conn:
        totals = await conn.fetchrow('''
        SELECT 
            COALESCE(SUM(CASE WHEN type = 'income' THEN amount ELSE 0 END), 0) as total_income,
            COALESCE(SUM(CASE WHEN type = 'expense' THEN amount ELSE 0 END), 0) as total_expense
        FROM transactions WHERE user_id = $1
        ''', user_id)
        
        daily = await conn.fetch('''
        SELECT 
            date(date) as day,
            COALESCE(SUM(CASE WHEN type = 'income' THEN amount ELSE 0 END), 0) as income,
            COALESCE(SUM(CASE WHEN type = 'expense' THEN amount ELSE 0 END), 0) as expense
        FROM transactions 
        WHERE user_id = $1 AND date >= CURRENT_DATE - 7
        GROUP BY date(date)
        ORDER BY date(date) DESC
        ''', user_id)
        
        categories = await conn.fetch('''
        SELECT category, COALESCE(SUM(amount), 0) as total
        FROM transactions 
        WHERE user_id = $1 AND type = 'expense'
        GROUP BY category
        ORDER BY total DESC
        ''', user_id)
        
        return {
            "totals": dict(totals),
            "daily": [dict(row) for row in daily],
            "categories": [dict(row) for row in categories]
        }


# ========== ПИТОМЕЦ ==========
@app.post("/pet/feed")
async def feed_pet(user_id: int, food_amount: int = 10):
    async with db_pool.acquire() as conn:
        async with conn.transaction():
            user = await conn.fetchrow('SELECT food_currency, pet_energy FROM users WHERE user_id = $1', user_id)
            if not user:
                raise HTTPException(status_code=404, detail="Пользователь не найден")
            
            if user['food_currency'] < food_amount:
                raise HTTPException(status_code=400, detail="Недостаточно корма")
            
            new_energy = min(100, user['pet_energy'] + 20)
            await conn.execute('''
            UPDATE users 
            SET food_currency = food_currency - $1,
                pet_energy = $2,
                last_feed_time = CURRENT_TIMESTAMP
            WHERE user_id = $3
            ''', food_amount, new_energy, user_id)
            
            bonus = 0
            if random.random() < 0.3:
                bonus = random.randint(5, 15)
                await conn.execute('''
                UPDATE users SET food_currency = food_currency + $1 WHERE user_id = $2
                ''', bonus, user_id)
            
            result = await conn.fetchrow('SELECT food_currency, pet_energy FROM users WHERE user_id = $1', user_id)
            
            return {
                "food_currency": result['food_currency'],
                "pet_energy": result['pet_energy'],
                "bonus": bonus
            }


@app.get("/pet/status/{user_id}")
async def get_pet_status(user_id: int):
    async with db_pool.acquire() as conn:
        user = await conn.fetchrow('''
        SELECT food_currency, pet_energy, last_feed_time 
        FROM users WHERE user_id = $1
        ''', user_id)
        
        if not user:
            raise HTTPException(status_code=404, detail="Пользователь не найден")
        
        hours_since_feed = (datetime.now() - user['last_feed_time']).total_seconds() / 3600
        hunger_loss = int(hours_since_feed * 10)
        
        return {
            "food_currency": user['food_currency'],
            "pet_energy": max(0, user['pet_energy'] - hunger_loss),
            "hours_without_food": round(hours_since_feed, 1)
        }


# ========== БЮДЖЕТ ==========
@app.post("/budgets/", response_model=BudgetResponse, status_code=status.HTTP_201_CREATED)
async def create_budget(budget: BudgetCreate):
    async with db_pool.acquire() as conn:
        row = await conn.fetchrow('''
        INSERT INTO budgets (user_id, category, amount, period, start_date, end_date)
        VALUES ($1, $2, $3, $4, $5, $6)
        RETURNING *
        ''', budget.user_id, budget.category, budget.amount, 
           budget.period, budget.start_date, budget.end_date)
        return dict(row)


@app.get("/budgets/", response_model=List[BudgetResponse])
async def get_budgets(user_id: Optional[int] = None):
    async with db_pool.acquire() as conn:
        if user_id:
            rows = await conn.fetch('SELECT * FROM budgets WHERE user_id = $1', user_id)
        else:
            rows = await conn.fetch('SELECT * FROM budgets')
        return [dict(row) for row in rows]


# ========== ТЕСТОВЫЕ ДАННЫЕ ==========
@app.post("/test/create_sample_user")
async def create_sample_user():
    async with db_pool.acquire() as conn:
        async with conn.transaction():
            count = await conn.fetchval('SELECT COUNT(*) FROM users')
            if count > 0:
                return {"message": "Пользователи уже существуют"}
            
            await conn.execute('''
            INSERT INTO users (name, email, total_saved, current_balance, food_currency, pet_energy)
            VALUES ('Иван Иванов', 'ivan@example.com', 50000, 150000, 100, 80)
            ''')
            
            user_id = 1
            
            await conn.execute('''
            INSERT INTO goals (user_id, target_amount, current_amount, name, deadline, reward_amount)
            VALUES 
            ($1, 50000, 25000, 'Новый ноутбук', CURRENT_DATE + INTERVAL '3 months', 60),
            ($1, 100000, 30000, 'Отпуск на море', CURRENT_DATE + INTERVAL '6 months', 80),
            ($1, 1000000, 150000, 'Машина мечты', CURRENT_DATE + INTERVAL '1 year', 100)
            ''', user_id)
            
            categories = ['Продукты', 'Транспорт', 'Жильё', 'Одежда', 'Здоровье', 'Развлечения']
            sources = ['Зарплата', 'Фриланс', 'Подарок', 'Инвестиции', 'Премия']
            
            for i in range(30):
                trans_date = datetime.now().replace(day=datetime.now().day - i)
                
                await conn.execute('''
                INSERT INTO transactions (user_id, amount, type, category, date)
                VALUES ($1, $2, 'income', $3, $4)
                ''', user_id, random.randint(1000, 5000), random.choice(sources), trans_date)
                
                await conn.execute('''
                INSERT INTO transactions (user_id, amount, type, category, date)
                VALUES ($1, $2, 'expense', $3, $4)
                ''', user_id, random.randint(500, 3000), random.choice(categories), trans_date)
            
            return {"message": "Тестовые данные созданы", "user_id": user_id}


# Запуск сервера
# Запуск сервера
if __name__ == "__main__":
    print("=" * 50)
    print("🚀 Запуск сервера Финансового Тамагоччи (PostgreSQL)")
    print("=" * 50)
    print(f"📁 База данных: PostgreSQL")
    print(f"🌐 API доступно по адресу: http://localhost:8000")  # Изменено с 5555 на 8000
    print(f"📚 Документация: http://localhost:8000/docs")      # Изменено с 5555 на 8000
    print("=" * 50)
    
    uvicorn.run(app, host="0.0.0.0", port=8000)  # Изменено с 5555 на 8000