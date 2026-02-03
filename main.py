from datetime import datetime, date
from typing import Optional, List
from fastapi import FastAPI, HTTPException, Depends, status
from pydantic import BaseModel
from sqlalchemy import create_engine, Column, Integer, String, Float, Date, Boolean, ForeignKey, DateTime, text
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import sessionmaker, relationship, Session
import os
from dotenv import load_dotenv
import logging

# Настройка логгирования
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Загрузка переменных окружения
load_dotenv()

# Получаем параметры подключения из переменных окружения
DB_USER = os.getenv("DB_USER", "postgres")
DB_PASSWORD = os.getenv("DB_PASSWORD", "postgres")
DB_HOST = os.getenv("DB_HOST", "localhost")
DB_PORT = os.getenv("DB_PORT", "5432")
DB_NAME = os.getenv("DB_NAME", "finance_tamagotchi")

# URL для подключения к PostgreSQL серверу (без указания базы данных)
POSTGRES_SERVER_URL = f"postgresql://{DB_USER}:{DB_PASSWORD}@{DB_HOST}:{DB_PORT}/postgres"

# URL для подключения к конкретной базе данных
DATABASE_URL = f"postgresql://{DB_USER}:{DB_PASSWORD}@{DB_HOST}:{DB_PORT}/{DB_NAME}"

def create_database_if_not_exists():
    """Создает базу данных если она не существует"""
    try:
        # Подключаемся к серверу PostgreSQL
        engine = create_engine(POSTGRES_SERVER_URL, isolation_level="AUTOCOMMIT")
        
        with engine.connect() as conn:
            # Проверяем существование базы данных
            result = conn.execute(text(f"SELECT 1 FROM pg_database WHERE datname = '{DB_NAME}'"))
            exists = result.scalar() is not None
            
            if not exists:
                logger.info(f"Создаем базу данных '{DB_NAME}'...")
                conn.execute(text(f"CREATE DATABASE {DB_NAME}"))
                logger.info(f"База данных '{DB_NAME}' успешно создана")
            else:
                logger.info(f"База данных '{DB_NAME}' уже существует")
        
        engine.dispose()
        
    except Exception as e:
        logger.error(f"Ошибка при создании базы данных: {e}")
        raise

# Создаем базу данных если нужно
create_database_if_not_exists()

# Создание движка SQLAlchemy для подключения к нашей базе данных
engine = create_engine(DATABASE_URL)
SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)
Base = declarative_base()

# Модели базы данных (согласно схеме)
class User(Base):
    __tablename__ = "users"
    
    user_id = Column(Integer, primary_key=True, index=True)
    username = Column(String(50), unique=True, nullable=False)
    email = Column(String(100), unique=True, nullable=False)
    password_hash = Column(String(255), nullable=False)
    registration_date = Column(Date, default=date.today)
    last_login = Column(DateTime)
    is_active = Column(Boolean, default=True)
    
    # Связи
    pets = relationship("Pet", back_populates="owner", cascade="all, delete-orphan")
    transactions = relationship("Transaction", back_populates="user", cascade="all, delete-orphan")
    categories = relationship("Category", back_populates="user", cascade="all, delete-orphan")
    budgets = relationship("Budget", back_populates="user", cascade="all, delete-orphan")
    goals = relationship("Goal", back_populates="user", cascade="all, delete-orphan")
    pet_actions = relationship("PetAction", back_populates="user", cascade="all, delete-orphan")

class Pet(Base):
    __tablename__ = "pets"
    
    pet_id = Column(Integer, primary_key=True, index=True)
    user_id = Column(Integer, ForeignKey("users.user_id", ondelete="CASCADE"), nullable=False)
    name = Column(String(50), nullable=False)
    pet_type = Column(String(20), nullable=False)  # Например: кошка, собака, дракон
    health = Column(Integer, default=100)
    happiness = Column(Integer, default=100)
    hunger = Column(Integer, default=0)
    created_at = Column(Date, default=date.today)
    last_fed = Column(DateTime)
    last_played = Column(DateTime)
    
    # Связи
    owner = relationship("User", back_populates="pets")
    actions = relationship("PetAction", back_populates="pet", cascade="all, delete-orphan")

class Category(Base):
    __tablename__ = "categories"
    
    category_id = Column(Integer, primary_key=True, index=True)
    user_id = Column(Integer, ForeignKey("users.user_id", ondelete="CASCADE"), nullable=False)
    name = Column(String(50), nullable=False)
    type = Column(String(10), nullable=False)  # 'income' или 'expense'
    description = Column(String(255))
    
    # Связи
    user = relationship("User", back_populates="categories")
    transactions = relationship("Transaction", back_populates="category", cascade="all, delete-orphan")
    budgets = relationship("Budget", back_populates="category", cascade="all, delete-orphan")

class Transaction(Base):
    __tablename__ = "transactions"
    
    transaction_id = Column(Integer, primary_key=True, index=True)
    user_id = Column(Integer, ForeignKey("users.user_id", ondelete="CASCADE"), nullable=False)
    category_id = Column(Integer, ForeignKey("categories.category_id", ondelete="CASCADE"), nullable=False)
    amount = Column(Float, nullable=False)
    description = Column(String(255))
    date = Column(Date, default=date.today)
    is_recurring = Column(Boolean, default=False)
    recurring_frequency = Column(String(20))  # daily, weekly, monthly
    
    # Связи
    user = relationship("User", back_populates="transactions")
    category = relationship("Category", back_populates="transactions")

class Budget(Base):
    __tablename__ = "budgets"
    
    budget_id = Column(Integer, primary_key=True, index=True)
    user_id = Column(Integer, ForeignKey("users.user_id", ondelete="CASCADE"), nullable=False)
    category_id = Column(Integer, ForeignKey("categories.category_id", ondelete="CASCADE"), nullable=False)
    amount = Column(Float, nullable=False)
    period = Column(String(20), nullable=False)  # monthly, weekly, yearly
    start_date = Column(Date, nullable=False)
    end_date = Column(Date, nullable=False)
    
    # Связи
    user = relationship("User", back_populates="budgets")
    category = relationship("Category", back_populates="budgets")

class Goal(Base):
    __tablename__ = "goals"
    
    goal_id = Column(Integer, primary_key=True, index=True)
    user_id = Column(Integer, ForeignKey("users.user_id", ondelete="CASCADE"), nullable=False)
    name = Column(String(100), nullable=False)
    target_amount = Column(Float, nullable=False)
    current_amount = Column(Float, default=0)
    deadline = Column(Date)
    is_completed = Column(Boolean, default=False)
    created_at = Column(Date, default=date.today)
    
    # Связи
    user = relationship("User", back_populates="goals")

class PetAction(Base):
    __tablename__ = "pet_actions"
    
    action_id = Column(Integer, primary_key=True, index=True)
    pet_id = Column(Integer, ForeignKey("pets.pet_id", ondelete="CASCADE"), nullable=False)
    user_id = Column(Integer, ForeignKey("users.user_id", ondelete="CASCADE"), nullable=False)
    action_type = Column(String(20), nullable=False)  # feed, play, heal, etc.
    action_date = Column(DateTime, default=datetime.now)
    happiness_change = Column(Integer, default=0)
    health_change = Column(Integer, default=0)
    hunger_change = Column(Integer, default=0)
    
    # Связи
    pet = relationship("Pet", back_populates="actions")
    user = relationship("User", back_populates="pet_actions")

# Создание таблиц
try:
    Base.metadata.create_all(bind=engine)
    logger.info("Таблицы базы данных успешно созданы")
except Exception as e:
    logger.error(f"Ошибка при создании таблиц: {e}")
    raise

# Pydantic модели для запросов и ответов
class UserCreate(BaseModel):
    username: str
    email: str
    password: str

class UserResponse(BaseModel):
    user_id: int
    username: str
    email: str
    registration_date: date
    is_active: bool
    
    class Config:
        from_attributes = True

class PetCreate(BaseModel):
    name: str
    pet_type: str

class PetResponse(BaseModel):
    pet_id: int
    user_id: int
    name: str
    pet_type: str
    health: int
    happiness: int
    hunger: int
    created_at: date
    
    class Config:
        from_attributes = True

class CategoryCreate(BaseModel):
    name: str
    type: str
    description: Optional[str] = None

class CategoryResponse(BaseModel):
    category_id: int
    user_id: int
    name: str
    type: str
    description: Optional[str]
    
    class Config:
        from_attributes = True

class TransactionCreate(BaseModel):
    category_id: int
    amount: float
    description: Optional[str] = None
    date: Optional[date] = None
    is_recurring: bool = False
    recurring_frequency: Optional[str] = None

class TransactionResponse(BaseModel):
    transaction_id: int
    user_id: int
    category_id: int
    amount: float
    description: Optional[str]
    date: date
    is_recurring: bool
    recurring_frequency: Optional[str]
    
    class Config:
        from_attributes = True

class BudgetCreate(BaseModel):
    category_id: int
    amount: float
    period: str
    start_date: date
    end_date: date

class BudgetResponse(BaseModel):
    budget_id: int
    user_id: int
    category_id: int
    amount: float
    period: str
    start_date: date
    end_date: date
    
    class Config:
        from_attributes = True

class GoalCreate(BaseModel):
    name: str
    target_amount: float
    deadline: Optional[date] = None

class GoalResponse(BaseModel):
    goal_id: int
    user_id: int
    name: str
    target_amount: float
    current_amount: float
    deadline: Optional[date]
    is_completed: bool
    created_at: date
    
    class Config:
        from_attributes = True

class PetActionCreate(BaseModel):
    pet_id: int
    action_type: str
    happiness_change: Optional[int] = 0
    health_change: Optional[int] = 0
    hunger_change: Optional[int] = 0

class PetActionResponse(BaseModel):
    action_id: int
    pet_id: int
    user_id: int
    action_type: str
    action_date: datetime
    happiness_change: int
    health_change: int
    hunger_change: int
    
    class Config:
        from_attributes = True

# Зависимость для получения сессии БД
def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()

# Создание приложения FastAPI
app = FastAPI(
    title="Финансовый Тамагоччи API",
    version="1.0.0",
    description="API для приложения финансовый тамагоччи с автоматическим созданием базы данных PostgreSQL"
)

# Корневой эндпоинт с информацией о базе данных
@app.get("/")
def read_root():
    return {
        "message": "Добро пожаловать в API Финансовый Тамагоччи!",
        "database": {
            "name": DB_NAME,
            "host": DB_HOST,
            "port": DB_PORT,
            "status": "connected" if engine else "disconnected"
        },
        "documentation": "/docs или /redoc",
        "endpoints": {
            "users": "/users",
            "pets": "/users/{user_id}/pets",
            "transactions": "/users/{user_id}/transactions",
            "goals": "/users/{user_id}/goals",
            "budgets": "/users/{user_id}/budgets",
            "categories": "/users/{user_id}/categories",
            "financial_summary": "/users/{user_id}/financial-summary"
        }
    }

# Роуты для пользователей
@app.post("/users/", response_model=UserResponse, status_code=status.HTTP_201_CREATED)
def create_user(user: UserCreate, db: Session = Depends(get_db)):
    # Проверяем существование пользователя с таким же username или email
    existing_user = db.query(User).filter(
        (User.username == user.username) | (User.email == user.email)
    ).first()
    
    if existing_user:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Username or email already exists"
        )
    
    # В реальном приложении нужно хэшировать пароль!
    db_user = User(
        username=user.username,
        email=user.email,
        password_hash=user.password,  # Внимание: в реальном приложении используйте bcrypt!
        registration_date=date.today()
    )
    db.add(db_user)
    db.commit()
    db.refresh(db_user)
    return db_user

@app.get("/users/{user_id}", response_model=UserResponse)
def get_user(user_id: int, db: Session = Depends(get_db)):
    user = db.query(User).filter(User.user_id == user_id).first()
    if not user:
        raise HTTPException(status_code=404, detail="User not found")
    return user

@app.get("/users/", response_model=List[UserResponse])
def get_all_users(db: Session = Depends(get_db)):
    users = db.query(User).all()
    return users

# Роуты для питомцев
@app.post("/users/{user_id}/pets/", response_model=PetResponse, status_code=status.HTTP_201_CREATED)
def create_pet(user_id: int, pet: PetCreate, db: Session = Depends(get_db)):
    # Проверяем существование пользователя
    user = db.query(User).filter(User.user_id == user_id).first()
    if not user:
        raise HTTPException(status_code=404, detail="User not found")
    
    db_pet = Pet(
        user_id=user_id,
        name=pet.name,
        pet_type=pet.pet_type,
        created_at=date.today()
    )
    db.add(db_pet)
    db.commit()
    db.refresh(db_pet)
    return db_pet

@app.get("/users/{user_id}/pets/", response_model=List[PetResponse])
def get_user_pets(user_id: int, db: Session = Depends(get_db)):
    pets = db.query(Pet).filter(Pet.user_id == user_id).all()
    return pets

@app.get("/pets/{pet_id}", response_model=PetResponse)
def get_pet(pet_id: int, db: Session = Depends(get_db)):
    pet = db.query(Pet).filter(Pet.pet_id == pet_id).first()
    if not pet:
        raise HTTPException(status_code=404, detail="Pet not found")
    return pet

# Роуты для категорий
@app.post("/users/{user_id}/categories/", response_model=CategoryResponse, status_code=status.HTTP_201_CREATED)
def create_category(user_id: int, category: CategoryCreate, db: Session = Depends(get_db)):
    user = db.query(User).filter(User.user_id == user_id).first()
    if not user:
        raise HTTPException(status_code=404, detail="User not found")
    
    if category.type not in ["income", "expense"]:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Category type must be 'income' or 'expense'"
        )
    
    db_category = Category(
        user_id=user_id,
        name=category.name,
        type=category.type,
        description=category.description
    )
    db.add(db_category)
    db.commit()
    db.refresh(db_category)
    return db_category

@app.get("/users/{user_id}/categories/", response_model=List[CategoryResponse])
def get_user_categories(user_id: int, db: Session = Depends(get_db)):
    categories = db.query(Category).filter(Category.user_id == user_id).all()
    return categories

# Роуты для транзакций
@app.post("/users/{user_id}/transactions/", response_model=TransactionResponse, status_code=status.HTTP_201_CREATED)
def create_transaction(user_id: int, transaction: TransactionCreate, db: Session = Depends(get_db)):
    user = db.query(User).filter(User.user_id == user_id).first()
    if not user:
        raise HTTPException(status_code=404, detail="User not found")
    
    # Проверяем существование категории
    category = db.query(Category).filter(Category.category_id == transaction.category_id).first()
    if not category:
        raise HTTPException(status_code=404, detail="Category not found")
    
    # Проверяем, что категория принадлежит пользователю
    if category.user_id != user_id:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Category does not belong to user"
        )
    
    db_transaction = Transaction(
        user_id=user_id,
        category_id=transaction.category_id,
        amount=transaction.amount,
        description=transaction.description,
        date=transaction.date or date.today(),
        is_recurring=transaction.is_recurring,
        recurring_frequency=transaction.recurring_frequency
    )
    db.add(db_transaction)
    db.commit()
    db.refresh(db_transaction)
    return db_transaction

@app.get("/users/{user_id}/transactions/", response_model=List[TransactionResponse])
def get_user_transactions(user_id: int, db: Session = Depends(get_db)):
    transactions = db.query(Transaction).filter(Transaction.user_id == user_id).all()
    return transactions

# Роуты для бюджетов
@app.post("/users/{user_id}/budgets/", response_model=BudgetResponse, status_code=status.HTTP_201_CREATED)
def create_budget(user_id: int, budget: BudgetCreate, db: Session = Depends(get_db)):
    user = db.query(User).filter(User.user_id == user_id).first()
    if not user:
        raise HTTPException(status_code=404, detail="User not found")
    
    category = db.query(Category).filter(Category.category_id == budget.category_id).first()
    if not category:
        raise HTTPException(status_code=404, detail="Category not found")
    
    if category.user_id != user_id:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Category does not belong to user"
        )
    
    if budget.period not in ["monthly", "weekly", "yearly"]:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Period must be 'monthly', 'weekly', or 'yearly'"
        )
    
    db_budget = Budget(
        user_id=user_id,
        category_id=budget.category_id,
        amount=budget.amount,
        period=budget.period,
        start_date=budget.start_date,
        end_date=budget.end_date
    )
    db.add(db_budget)
    db.commit()
    db.refresh(db_budget)
    return db_budget

@app.get("/users/{user_id}/budgets/", response_model=List[BudgetResponse])
def get_user_budgets(user_id: int, db: Session = Depends(get_db)):
    budgets = db.query(Budget).filter(Budget.user_id == user_id).all()
    return budgets

# Роуты для целей
@app.post("/users/{user_id}/goals/", response_model=GoalResponse, status_code=status.HTTP_201_CREATED)
def create_goal(user_id: int, goal: GoalCreate, db: Session = Depends(get_db)):
    user = db.query(User).filter(User.user_id == user_id).first()
    if not user:
        raise HTTPException(status_code=404, detail="User not found")
    
    db_goal = Goal(
        user_id=user_id,
        name=goal.name,
        target_amount=goal.target_amount,
        deadline=goal.deadline,
        created_at=date.today()
    )
    db.add(db_goal)
    db.commit()
    db.refresh(db_goal)
    return db_goal

@app.get("/users/{user_id}/goals/", response_model=List[GoalResponse])
def get_user_goals(user_id: int, db: Session = Depends(get_db)):
    goals = db.query(Goal).filter(Goal.user_id == user_id).all()
    return goals

# Роуты для действий с питомцем
@app.post("/pet_actions/", response_model=PetActionResponse, status_code=status.HTTP_201_CREATED)
def create_pet_action(action: PetActionCreate, db: Session = Depends(get_db)):
    # Проверяем существование питомца
    pet = db.query(Pet).filter(Pet.pet_id == action.pet_id).first()
    if not pet:
        raise HTTPException(status_code=404, detail="Pet not found")
    
    db_action = PetAction(
        pet_id=action.pet_id,
        user_id=pet.user_id,
        action_type=action.action_type,
        happiness_change=action.happiness_change,
        health_change=action.health_change,
        hunger_change=action.hunger_change,
        action_date=datetime.now()
    )
    
    # Обновляем состояние питомца
    pet.happiness = min(100, max(0, pet.happiness + action.happiness_change))
    pet.health = min(100, max(0, pet.health + action.health_change))
    pet.hunger = min(100, max(0, pet.hunger + action.hunger_change))
    
    # Обновляем время последнего взаимодействия
    if action.action_type == "feed":
        pet.last_fed = datetime.now()
    elif action.action_type == "play":
        pet.last_played = datetime.now()
    
    db.add(db_action)
    db.commit()
    db.refresh(db_action)
    return db_action

@app.get("/pets/{pet_id}/actions/", response_model=List[PetActionResponse])
def get_pet_actions(pet_id: int, db: Session = Depends(get_db)):
    actions = db.query(PetAction).filter(PetAction.pet_id == pet_id).all()
    return actions

# Дополнительные роуты
@app.get("/users/{user_id}/financial-summary/")
def get_financial_summary(user_id: int, db: Session = Depends(get_db)):
    """Получение финансовой сводки пользователя"""
    # Сумма доходов
    income_result = db.query(Transaction).join(Category).filter(
        Transaction.user_id == user_id,
        Category.type == 'income'
    ).with_entities(db.func.sum(Transaction.amount)).scalar()
    income = float(income_result) if income_result else 0.0
    
    # Сумма расходов
    expense_result = db.query(Transaction).join(Category).filter(
        Transaction.user_id == user_id,
        Category.type == 'expense'
    ).with_entities(db.func.sum(Transaction.amount)).scalar()
    expense = float(expense_result) if expense_result else 0.0
    
    # Баланс
    balance = income - expense
    
    # Прогресс по целям
    goals = db.query(Goal).filter(
        Goal.user_id == user_id,
        Goal.is_completed == False
    ).all()
    
    goals_progress = [
        {
            "goal_id": goal.goal_id,
            "name": goal.name,
            "current_amount": goal.current_amount,
            "target_amount": goal.target_amount,
            "progress_percentage": round((goal.current_amount / goal.target_amount * 100) if goal.target_amount > 0 else 0, 2)
        }
        for goal in goals
    ]
    
    return {
        "user_id": user_id,
        "total_income": income,
        "total_expense": expense,
        "balance": balance,
        "goals_progress": goals_progress
    }

@app.put("/goals/{goal_id}/add-money/", response_model=GoalResponse)
def add_money_to_goal(goal_id: int, amount: float, db: Session = Depends(get_db)):
    """Добавление денег к цели"""
    goal = db.query(Goal).filter(Goal.goal_id == goal_id).first()
    if not goal:
        raise HTTPException(status_code=404, detail="Goal not found")
    
    goal.current_amount += amount
    
    # Проверяем, достигнута ли цель
    if goal.current_amount >= goal.target_amount:
        goal.is_completed = True
        goal.current_amount = goal.target_amount
    
    db.commit()
    db.refresh(goal)
    return goal

@app.get("/database/info")
def get_database_info():
    """Информация о базе данных"""
    try:
        with engine.connect() as conn:
            # Получаем информацию о таблицах
            result = conn.execute(text("""
                SELECT table_name, table_type 
                FROM information_schema.tables 
                WHERE table_schema = 'public'
                ORDER BY table_name
            """))
            tables = [{"name": row[0], "type": row[1]} for row in result]
            
            # Получаем количество записей в каждой таблице
            table_counts = {}
            for table in tables:
                if table["type"] == "BASE TABLE":
                    count_result = conn.execute(text(f"SELECT COUNT(*) FROM {table['name']}"))
                    table_counts[table["name"]] = count_result.scalar()
            
            return {
                "database_name": DB_NAME,
                "host": DB_HOST,
                "port": DB_PORT,
                "user": DB_USER,
                "tables": tables,
                "table_counts": table_counts,
                "status": "connected"
            }
    except Exception as e:
        return {
            "database_name": DB_NAME,
            "host": DB_HOST,
            "port": DB_PORT,
            "user": DB_USER,
            "status": "error",
            "error": str(e)
        }

@app.get("/health")
def health_check():
    try:
        with engine.connect() as conn:
            conn.execute(text("SELECT 1"))
        db_status = "connected"
    except Exception as e:
        db_status = f"disconnected: {str(e)}"
    
    return {
        "status": "healthy", 
        "timestamp": datetime.now(),
        "database": db_status,
        "app": "Финансовый Тамагоччи"
    }

if __name__ == "__main__":
    import uvicorn
    
    print("=" * 50)
    print("🚀 Запуск сервера Финансовый Тамагоччи")
    print("=" * 50)
    print(f"📊 База данных: {DB_NAME}")
    print(f"🔗 Хост: {DB_HOST}:{DB_PORT}")
    print(f"👤 Пользователь: {DB_USER}")
    print("-" * 50)
    print("📚 Документация доступна по адресам:")
    print("   http://localhost:8000/docs (Swagger UI)")
    print("   http://localhost:8000/redoc (ReDoc)")
    print("=" * 50)
    
    # Запускаем сервер
    uvicorn.run(
        app, 
        host="0.0.0.0", 
        port=8000,
        reload=True  # Автоматическая перезагрузка при изменениях
    )