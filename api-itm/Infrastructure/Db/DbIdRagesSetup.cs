using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace api_itm.Infrastructure
{
    /// <summary>
    /// Creates/updates the idsreges table (PostgreSQL) without migrations.
    /// No extensions required. Call once at startup: await DbIdRagesSetup.EnsureAsync(db);
    /// </summary>
    public static class DbIdRagesSetup
    {
        public static Task EnsureAsync(DbContext db)
        {
            var sql = @"
-- === idsreges (no extensions, no DB-side UUID generator) ===
CREATE TABLE IF NOT EXISTS idsreges (
    id UUID PRIMARY KEY,                            -- app must provide Guid
    employee_id UUID NULL,
    message_response_id UUID NULL,
    message_result_id UUID NULL,
    reges_salariat_id UUID NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    error_message TEXT NULL,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW()
);

-- Idempotent column guards (safe to run every launch)
ALTER TABLE idsreges ADD COLUMN IF NOT EXISTS employee_id UUID NULL;
ALTER TABLE idsreges ADD COLUMN IF NOT EXISTS message_response_id UUID NULL;
ALTER TABLE idsreges ADD COLUMN IF NOT EXISTS message_result_id UUID NULL;
ALTER TABLE idsreges ADD COLUMN IF NOT EXISTS reges_salariat_id UUID NULL;
ALTER TABLE idsreges ADD COLUMN IF NOT EXISTS status VARCHAR(50) NOT NULL DEFAULT 'Pending';
ALTER TABLE idsreges ADD COLUMN IF NOT EXISTS error_message TEXT NULL;
ALTER TABLE idsreges ADD COLUMN IF NOT EXISTS created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW();
ALTER TABLE idsreges ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW();

-- Unique only when values are present
CREATE UNIQUE INDEX IF NOT EXISTS ux_idsreges_message_response_id
    ON idsreges (message_response_id)
    WHERE message_response_id IS NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS ux_idsreges_message_result_id
    ON idsreges (message_result_id)
    WHERE message_result_id IS NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS ux_idsreges_reges_salariat_id
    ON idsreges (reges_salariat_id)
    WHERE reges_salariat_id IS NOT NULL;

-- Helper indexes
CREATE INDEX IF NOT EXISTS ix_idsreges_employee_id
    ON idsreges (employee_id);
CREATE INDEX IF NOT EXISTS ix_idsreges_status
    ON idsreges (status);

-- Function to auto-update updated_at
CREATE OR REPLACE FUNCTION set_updated_at_idsreges()
RETURNS TRIGGER AS $f$
BEGIN
    NEW.updated_at := NOW();
    RETURN NEW;
END;
$f$ LANGUAGE plpgsql;

-- Create trigger once
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_trigger
        WHERE tgname = 'trg_set_updated_at_idsreges'
    ) THEN
        CREATE TRIGGER trg_set_updated_at_idsreges
        BEFORE UPDATE ON idsreges
        FOR EACH ROW EXECUTE FUNCTION set_updated_at_idsreges();
    END IF;
END$$;

-- Optional FK (fill your real table/column and uncomment if you want it enforced)
-- ALTER TABLE idsreges
--   ADD CONSTRAINT fk_salariat_employee
--   FOREIGN KEY (employee_id) REFERENCES employees(id) ON DELETE SET NULL;
";
            return db.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
