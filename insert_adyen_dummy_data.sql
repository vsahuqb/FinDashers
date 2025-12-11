-- =====================================================
-- Adyen Dummy Data Insert Script
-- =====================================================
-- This script will insert sample Adyen transaction data 
-- into the adyen_transactions table for testing the dashboard
-- 
-- BEFORE RUNNING:
-- 1. Ensure PostgreSQL is running
-- 2. Ensure the adyen_transactions table exists
-- 3. Backup existing data if needed
-- 4. Update connection details if different from default
-- =====================================================

-- Create table if it doesn't exist
CREATE TABLE IF NOT EXISTS adyen_transactions (
    id BIGSERIAL PRIMARY KEY,
    psp_reference VARCHAR(255) NOT NULL,
    merchant_reference VARCHAR(255),
    event_code VARCHAR(100) NOT NULL,
    event_date TIMESTAMP NOT NULL,
    approved_amount DECIMAL(10,2) NOT NULL DEFAULT 0,
    currency VARCHAR(10) NOT NULL DEFAULT 'USD',
    merchant_account VARCHAR(255),
    payment_method VARCHAR(100),
    reason VARCHAR(255),
    success BOOLEAN NOT NULL DEFAULT false,
    location_id VARCHAR(100),
    company_id INTEGER,
    terminal_id VARCHAR(100),
    tender_reference VARCHAR(255),
    raw_event JSONB,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Clear existing data (OPTIONAL - remove this line if you want to keep existing data)
-- TRUNCATE TABLE adyen_transactions;

-- Insert sample data from CSV
INSERT INTO adyen_transactions (
    psp_reference, merchant_reference, event_code, event_date, approved_amount, 
    currency, merchant_account, payment_method, reason, success, location_id, 
    company_id, terminal_id, tender_reference, raw_event, created_at
) VALUES 
('PSP82501904', '63191987', 'CANCELLATION', '2025-08-03T20:52:06.826844', 65.19, 'USD', 'QuBeyond_Integration_TEST', 'mc', 'CAPTURED', false, '666', 421, '47045', 'TND6621063', '{"amount": {"value": 6519, "currency": "USD"}, "eventCode": "CANCELLATION", "eventDate": "2025-08-03T20:52:06.826844", "merchantAccountCode": "QuBeyond_Integration_TEST", "paymentMethod": "mc", "pspReference": "PSP82501904", "reason": "CAPTURED", "success": "false"}', '2025-12-11T08:47:01.929247+00:00'),
('PSP23113244', '31903589', 'CAPTURE', '2025-09-06T03:27:06.826844', 139.91, 'USD', 'QuBeyond_Integration_TEST', 'paypal', 'CAPTURED', true, '587', 584, '43541', 'TND9676019', '{"amount": {"value": 13991, "currency": "USD"}, "eventCode": "CAPTURE", "eventDate": "2025-09-06T03:27:06.826844", "merchantAccountCode": "QuBeyond_Integration_TEST", "paymentMethod": "paypal", "pspReference": "PSP23113244", "reason": "CAPTURED", "success": "true"}', '2025-12-11T08:47:01.929247+00:00'),
('PSP15592283', '46276849', 'REFUNDED_REVERSED', '2025-11-26T18:55:06.826844', 101.86, 'USD', 'QuBeyond_Integration_TEST', 'mc', 'FRAUD_SUSPECTED', true, '270', 381, '43943', 'TND4316160', '{"amount": {"value": 10186, "currency": "USD"}, "eventCode": "REFUNDED_REVERSED", "eventDate": "2025-11-26T18:55:06.826844", "merchantAccountCode": "QuBeyond_Integration_TEST", "paymentMethod": "mc", "pspReference": "PSP15592283", "reason": "FRAUD_SUSPECTED", "success": "true"}', '2025-12-11T08:47:01.929247+00:00'),
('PSP10591124', '59167903', 'CHARGEBACK_REVERSED', '2025-12-03T23:23:06.826844', 166.10, 'USD', 'QuBeyond_Integration_TEST', 'paypal', 'INSUFFICIENT_FUNDS', true, '317', 459, '41951', 'TND3782514', '{"amount": {"value": 16610, "currency": "USD"}, "eventCode": "CHARGEBACK_REVERSED", "eventDate": "2025-12-03T23:23:06.826844", "merchantAccountCode": "QuBeyond_Integration_TEST", "paymentMethod": "paypal", "pspReference": "PSP10591124", "reason": "INSUFFICIENT_FUNDS", "success": "true"}', '2025-12-11T08:47:01.929247+00:00'),
('PSP43293022', '56228078', 'REQUEST_FOR_INFORMATION', '2025-11-26T13:09:06.826844', 178.09, 'USD', 'QuBeyond_Integration_TEST', 'paypal', 'FRAUD_SUSPECTED', true, '387', 637, '48503', 'TND5891197', '{"amount": {"value": 17809, "currency": "USD"}, "eventCode": "REQUEST_FOR_INFORMATION", "eventDate": "2025-11-26T13:09:06.826844", "merchantAccountCode": "QuBeyond_Integration_TEST", "paymentMethod": "paypal", "pspReference": "PSP43293022", "reason": "FRAUD_SUSPECTED", "success": "true"}', '2025-12-11T08:47:01.929247+00:00'),
('PSP10334081', '53661712', 'REFUNDED_REVERSED', '2025-07-27T13:29:06.826844', 60.52, 'USD', 'QuBeyond_Integration_TEST', 'visa', 'INSUFFICIENT_FUNDS', true, '378', 352, '43861', 'TND7246516', '{"amount": {"value": 6052, "currency": "USD"}, "eventCode": "REFUNDED_REVERSED", "eventDate": "2025-07-27T13:29:06.826844", "merchantAccountCode": "QuBeyond_Integration_TEST", "paymentMethod": "visa", "pspReference": "PSP10334081", "reason": "INSUFFICIENT_FUNDS", "success": "true"}', '2025-12-11T08:47:01.929247+00:00'),
('PSP52610318', '50875348', 'REQUEST_FOR_INFORMATION', '2025-08-06T11:34:06.826844', 88.31, 'USD', 'QuBeyond_Integration_TEST', 'discover', 'REFUSED', true, '713', 626, '45167', 'TND1426199', '{"amount": {"value": 8831, "currency": "USD"}, "eventCode": "REQUEST_FOR_INFORMATION", "eventDate": "2025-08-06T11:34:06.826844", "merchantAccountCode": "QuBeyond_Integration_TEST", "paymentMethod": "discover", "pspReference": "PSP52610318", "reason": "REFUSED", "success": "true"}', '2025-12-11T08:47:01.929247+00:00'),
('PSP97012185', '56734381', 'REFUNDED_REVERSED', '2025-09-25T00:06:06.826844', 92.97, 'USD', 'QuBeyond_Integration_TEST', 'mc', 'INSUFFICIENT_FUNDS', true, '140', 450, '44861', 'TND2201953', '{"amount": {"value": 9297, "currency": "USD"}, "eventCode": "REFUNDED_REVERSED", "eventDate": "2025-09-25T00:06:06.826844", "merchantAccountCode": "QuBeyond_Integration_TEST", "paymentMethod": "mc", "pspReference": "PSP97012185", "reason": "INSUFFICIENT_FUNDS", "success": "true"}', '2025-12-11T08:47:01.929247+00:00'),
('PSP86599311', '49859456', 'REFUND', '2025-11-06T18:18:06.826844', 60.94, 'USD', 'QuBeyond_Integration_TEST', 'amex', 'INSUFFICIENT_FUNDS', true, '238', 625, '45301', 'TND1177493', '{"amount": {"value": 6094, "currency": "USD"}, "eventCode": "REFUND", "eventDate": "2025-11-06T18:18:06.826844", "merchantAccountCode": "QuBeyond_Integration_TEST", "paymentMethod": "amex", "pspReference": "PSP86599311", "reason": "INSUFFICIENT_FUNDS", "success": "true"}', '2025-12-11T08:47:01.929247+00:00'),
('PSP98503257', '52744976', 'CAPTURE', '2025-11-25T14:01:06.826844', 79.88, 'USD', 'QuBeyond_Integration_TEST', 'visa', 'APPROVED', true, '103', 834, '42178', 'TND1177464', '{"amount": {"value": 7988, "currency": "USD"}, "eventCode": "CAPTURE", "eventDate": "2025-11-25T14:01:06.826844", "merchantAccountCode": "QuBeyond_Integration_TEST", "paymentMethod": "visa", "pspReference": "PSP98503257", "reason": "APPROVED", "success": "true"}', '2025-12-11T08:47:01.929247+00:00'),
-- Adding AUTHORISATION events for success rate calculations
('PSP11111111', '11111111', 'AUTHORISATION', '2025-12-10T10:00:00.000000', 100.00, 'USD', 'QuBeyond_Integration_TEST', 'visa', 'APPROVED', true, '115', 100, '40001', 'TND1111111', '{"amount": {"value": 10000, "currency": "USD"}, "eventCode": "AUTHORISATION", "eventDate": "2025-12-10T10:00:00.000000", "merchantAccountCode": "QuBeyond_Integration_TEST", "paymentMethod": "visa", "pspReference": "PSP11111111", "reason": "APPROVED", "success": "true"}', NOW()),
('PSP22222222', '22222222', 'AUTHORISATION', '2025-12-10T11:00:00.000000', 150.00, 'USD', 'QuBeyond_Integration_TEST', 'mc', 'APPROVED', true, '115', 100, '40002', 'TND2222222', '{"amount": {"value": 15000, "currency": "USD"}, "eventCode": "AUTHORISATION", "eventDate": "2025-12-10T11:00:00.000000", "merchantAccountCode": "QuBeyond_Integration_TEST", "paymentMethod": "mc", "pspReference": "PSP22222222", "reason": "APPROVED", "success": "true"}', NOW()),
('PSP33333333', '33333333', 'AUTHORISATION', '2025-12-10T12:00:00.000000', 75.50, 'USD', 'QuBeyond_Integration_TEST', 'amex', 'DECLINED', false, '115', 100, '40003', 'TND3333333', '{"amount": {"value": 7550, "currency": "USD"}, "eventCode": "AUTHORISATION", "eventDate": "2025-12-10T12:00:00.000000", "merchantAccountCode": "QuBeyond_Integration_TEST", "paymentMethod": "amex", "pspReference": "PSP33333333", "reason": "DECLINED", "success": "false"}', NOW()),
('PSP44444444', '44444444', 'AUTHORISATION', '2025-12-10T13:00:00.000000', 200.25, 'USD', 'QuBeyond_Integration_TEST', 'paypal', 'APPROVED', true, '115', 100, '40004', 'TND4444444', '{"amount": {"value": 20025, "currency": "USD"}, "eventCode": "AUTHORISATION", "eventDate": "2025-12-10T13:00:00.000000", "merchantAccountCode": "QuBeyond_Integration_TEST", "paymentMethod": "paypal", "pspReference": "PSP44444444", "reason": "APPROVED", "success": "true"}', NOW()),
('PSP55555555', '55555555', 'AUTHORISATION', '2025-12-10T14:00:00.000000', 89.99, 'USD', 'QuBeyond_Integration_TEST', 'discover', 'APPROVED', true, '115', 100, '40005', 'TND5555555', '{"amount": {"value": 8999, "currency": "USD"}, "eventCode": "AUTHORISATION", "eventDate": "2025-12-10T14:00:00.000000", "merchantAccountCode": "QuBeyond_Integration_TEST", "paymentMethod": "discover", "pspReference": "PSP55555555", "reason": "APPROVED", "success": "true"}', NOW());

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_adyen_transactions_event_code ON adyen_transactions(event_code);
CREATE INDEX IF NOT EXISTS idx_adyen_transactions_event_date ON adyen_transactions(event_date);
CREATE INDEX IF NOT EXISTS idx_adyen_transactions_location_id ON adyen_transactions(location_id);
CREATE INDEX IF NOT EXISTS idx_adyen_transactions_success ON adyen_transactions(success);
CREATE INDEX IF NOT EXISTS idx_adyen_transactions_payment_method ON adyen_transactions(payment_method);

-- Verify the data was inserted
SELECT 
    COUNT(*) as total_records,
    COUNT(CASE WHEN event_code = 'AUTHORISATION' THEN 1 END) as authorisation_count,
    COUNT(CASE WHEN success = true THEN 1 END) as success_count,
    COUNT(CASE WHEN success = false THEN 1 END) as failure_count,
    SUM(CASE WHEN success = true THEN approved_amount ELSE 0 END) as total_sales
FROM adyen_transactions;

-- Show sample records
SELECT psp_reference, event_code, event_date, approved_amount, success, location_id 
FROM adyen_transactions 
ORDER BY event_date DESC 
LIMIT 10;