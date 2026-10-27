-- 1. Создание таблиц
CREATE TABLE IF NOT EXISTS workshops (
    id SERIAL PRIMARY KEY,
    name VARCHAR NOT NULL,
    head_full_name VARCHAR NOT NULL
);

CREATE TABLE IF NOT EXISTS workers (
    id SERIAL PRIMARY KEY,
    full_name VARCHAR NOT NULL,
    position VARCHAR NOT NULL,
    discount_percent NUMERIC(5,2) NOT NULL DEFAULT 0
        CHECK (discount_percent >= 0 AND discount_percent <= 100),
    workshop_id INT NOT NULL REFERENCES workshops (id)
);

CREATE TABLE IF NOT EXISTS workwears (
    id SERIAL PRIMARY KEY,
    type VARCHAR NOT NULL,
    wear_months INT NOT NULL CHECK (wear_months > 0),
    unit_cost NUMERIC(12,2) NOT NULL CHECK (unit_cost >= 0)
);

CREATE TABLE IF NOT EXISTS receipts (
    id SERIAL PRIMARY KEY,
    worker_id INT NOT NULL REFERENCES workers (id),
    workwear_id INT NOT NULL REFERENCES workwears (id),
    received_at DATE NOT NULL,
    signed BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS users (
    login VARCHAR PRIMARY KEY,
    password VARCHAR NOT NULL
);

-- 2. Индексы
CREATE INDEX IF NOT EXISTS workers_workshop_id_idx ON workers (workshop_id);
CREATE INDEX IF NOT EXISTS receipts_worker_id_idx ON receipts (worker_id);
CREATE INDEX IF NOT EXISTS receipts_workwear_id_idx ON receipts (workwear_id);

-- 3. Пользователь для JWT (вход в API)
INSERT INTO users (login, password)
VALUES ('admin', 'admin')
ON CONFLICT (login) DO NOTHING;

-- 4. Тестовые данные
INSERT INTO workshops (id, name, head_full_name) VALUES
    (1, 'Механический цех', 'Иванов Иван Иванович'),
    (2, 'Сборочный цех', 'Петрова Анна Сергеевна')
ON CONFLICT (id) DO NOTHING;

INSERT INTO workers (id, full_name, position, discount_percent, workshop_id) VALUES
    (1, 'Сидоров Алексей Петрович', 'Токарь', 10, 1),
    (2, 'Кузнецов Дмитрий Олегович', 'Слесарь', 5, 1),
    (3, 'Морозова Елена Викторовна', 'Инженер', 0, 2)
ON CONFLICT (id) DO NOTHING;

INSERT INTO workwears (id, type, wear_months, unit_cost) VALUES
    (1, 'Халат хлопчатобумажный', 12, 1500.00),
    (2, 'Ботинки кожаные', 18, 3200.00)
ON CONFLICT (id) DO NOTHING;

INSERT INTO receipts (id, worker_id, workwear_id, received_at, signed) VALUES
    (1, 1, 1, DATE '2026-01-15', TRUE),
    (2, 2, 2, DATE '2026-02-03', TRUE),
    (3, 1, 2, DATE '2026-03-10', FALSE)
ON CONFLICT (id) DO NOTHING;

-- 5. Синхронизация ID
SELECT setval('workshops_id_seq', COALESCE((SELECT MAX(id) FROM workshops), 1), true);
SELECT setval('workers_id_seq',   COALESCE((SELECT MAX(id) FROM workers), 1), true);
SELECT setval('workwears_id_seq', COALESCE((SELECT MAX(id) FROM workwears), 1), true);
SELECT setval('receipts_id_seq',  COALESCE((SELECT MAX(id) FROM receipts), 1), true);
-- Создаем пользователя user с паролем secret
CREATE USER "user" WITH PASSWORD 'secret';

-- Даем права на базу и таблицы
GRANT ALL PRIVILEGES ON DATABASE specodezhda TO "user";
GRANT ALL ON SCHEMA public TO "user";
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO "user";
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO "user";