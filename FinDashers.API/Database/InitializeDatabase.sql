-- Create webhook_credentials table if not exists
CREATE TABLE IF NOT EXISTS webhook_credentials (
    id SERIAL PRIMARY KEY,
    username VARCHAR(255) NOT NULL,
    password VARCHAR(255) NOT NULL,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW()
);

-- Create adyen_transactions table if not exists
CREATE TABLE IF NOT EXISTS adyen_transactions (
    id                 BIGSERIAL PRIMARY KEY,
    psp_reference      VARCHAR(64)    NOT NULL UNIQUE,
    merchant_reference VARCHAR(128),
    event_code         VARCHAR(64)    NOT NULL,
    event_date         TIMESTAMPTZ    NOT NULL,
    approved_amount    NUMERIC(12,2)  NOT NULL,
    currency           VARCHAR(8)     NOT NULL,
    merchant_account   VARCHAR(128),
    payment_method     VARCHAR(64),
    reason             TEXT,
    success            BOOLEAN       NOT NULL DEFAULT false,
    location_id        VARCHAR(64),
    company_id         INTEGER,
    terminal_id        VARCHAR(128),
    tender_reference   VARCHAR(128),
    raw_event          JSONB         NOT NULL,
    created_at         TIMESTAMPTZ   NOT NULL DEFAULT now()
);

-- Create index on psp_reference for faster lookups
CREATE INDEX IF NOT EXISTS idx_adyen_transactions_psp_reference ON adyen_transactions(psp_reference);

-- Create index on event_date for time-based queries
CREATE INDEX IF NOT EXISTS idx_adyen_transactions_event_date ON adyen_transactions(event_date);

-- Create index on merchant_reference for merchant lookups
CREATE INDEX IF NOT EXISTS idx_adyen_transactions_merchant_reference ON adyen_transactions(merchant_reference);
