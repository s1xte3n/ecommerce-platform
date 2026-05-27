-- Database initialization script
-- Runs automatically on first container start

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";
CREATE EXTENSION IF NOT EXISTS "pg_stat_statements";

-- Set defaults
SET TIMEZONE TO 'UTC';
ALTER DATABASE ecommerce_dev SET timezone TO 'UTC';

-- Create audit schema
CREATE SCHEMA IF NOT EXISTS audit;

-- Audit log table
CREATE TABLE IF NOT EXISTS audit.audit_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    entity_type VARCHAR(100) NOT NULL,
    entity_id UUID NOT NULL,
    action VARCHAR(50) NOT NULL,
    changes JSONB,
    performed_by UUID,
    performed_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    correlation_id VARCHAR(100),
    ip_address INET,
    user_agent TEXT
);

CREATE INDEX IF NOT EXISTS idx_audit_logs_entity 
    ON audit.audit_logs(entity_type, entity_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_performed_at 
    ON audit.audit_logs(performed_at DESC);
CREATE INDEX IF NOT EXISTS idx_audit_logs_correlation 
    ON audit.audit_logs(correlation_id);

-- Outbox pattern table for reliable event publishing
CREATE SCHEMA IF NOT EXISTS messaging;

CREATE TABLE IF NOT EXISTS messaging.outbox_messages (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    type VARCHAR(255) NOT NULL,
    payload JSONB NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    processed_at TIMESTAMPTZ,
    error TEXT,
    retry_count INT NOT NULL DEFAULT 0,
    correlation_id VARCHAR(100)
);

CREATE INDEX IF NOT EXISTS idx_outbox_unprocessed 
    ON messaging.outbox_messages(created_at) 
    WHERE processed_at IS NULL;

-- Grant permissions
GRANT ALL PRIVILEGES ON DATABASE ecommerce_dev TO ecommerce_user;
GRANT ALL PRIVILEGES ON SCHEMA audit TO ecommerce_user;
GRANT ALL PRIVILEGES ON SCHEMA messaging TO ecommerce_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA audit TO ecommerce_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA messaging TO ecommerce_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA audit GRANT ALL ON TABLES TO ecommerce_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA messaging GRANT ALL ON TABLES TO ecommerce_user;
