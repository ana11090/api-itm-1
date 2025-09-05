using Microsoft.EntityFrameworkCore;

public static class DbIdRagesEmployeesSetup
{
    public static Task EnsureAsync(DbContext db)
    {
        var sql = @"
-- === Create idsreges table if it doesn't exist (id auto-increment) ===
-- (legacy comment kept; effective table is now 'idsreges_salariat'. See RENAME PHASE below.)

-- === If the table already existed and 'id' is NOT identity, attach a sequence default (no drop) ===
-- (legacy comment kept; sequence safety still ensured below for 'idsreges_salariat'.)

-- === Indexes (no-ops if already exist) ===
-- (legacy comment kept; index names updated to the new table.)

-- === Trigger to update 'updated_at' on row modification (no-op if exists) ===
-- (legacy comment kept; trigger/function renamed accordingly.)

-- === RENAME PHASE: idsreges -> idsreges_salariat (idempotent, keeps data) ===
DO $$
BEGIN
    IF to_regclass('idsreges_salariat') IS NULL
       AND to_regclass('idsreges') IS NOT NULL THEN
        ALTER TABLE idsreges RENAME TO idsreges_salariat;
    END IF;
END$$;

-- === RENAME OLD INDEXES TO NEW NAMES (no-op if absent) ===
DO $$
BEGIN
    IF to_regclass('ux_idsreges_id_raspuns_mesaj') IS NOT NULL THEN
        ALTER INDEX ux_idsreges_id_raspuns_mesaj
            RENAME TO ux_idsreges_salariat_id_raspuns_mesaj;
    END IF;

    IF to_regclass('ux_idsreges_id_rezultat_mesaj') IS NOT NULL THEN
        ALTER INDEX ux_idsreges_id_rezultat_mesaj
            RENAME TO ux_idsreges_salariat_id_rezultat_mesaj;
    END IF;

    IF to_regclass('ux_idsreges_reges_salariat_id') IS NOT NULL THEN
        ALTER INDEX ux_idsreges_reges_salariat_id
            RENAME TO ux_idsreges_salariat_reges_salariat_id;
    END IF;

    IF to_regclass('ix_idsreges_status') IS NOT NULL THEN
        ALTER INDEX ix_idsreges_status RENAME TO ix_idsreges_salariat_status;
    END IF;

    IF to_regclass('ix_idsreges_idpersoana') IS NOT NULL THEN
        ALTER INDEX ix_idsreges_idpersoana RENAME TO ix_idsreges_salariat_idpersoana;
    END IF;
END$$;

-- === RENAME TRIGGER + FUNCTION IF STILL OLD-NAMED (no-op if absent) ===
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM pg_trigger
        WHERE tgname = 'trg_set_updated_at_idsreges'
          AND tgrelid = 'idsreges_salariat'::regclass
    ) THEN
        ALTER TRIGGER trg_set_updated_at_idsreges
            ON idsreges_salariat
            RENAME TO trg_set_updated_at_idsreges_salariat;
    END IF;

    IF EXISTS (SELECT 1 FROM pg_proc WHERE proname = 'set_updated_at_idsreges') THEN
        ALTER FUNCTION set_updated_at_idsreges() RENAME TO set_updated_at_idsreges_salariat;
    END IF;
END$$;

-- === CREATE TABLE idsreges_salariat IF NOT EXISTS (final schema) ===
CREATE TABLE IF NOT EXISTS idsreges_salariat (
    id                 INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY, -- auto 1,2,3...
    idpersoana         INTEGER NULL,
    idutilizator       INTEGER NULL,
    id_raspuns_mesaj   UUID NULL,
    id_rezultat_mesaj  UUID NULL,
    idautor            UUID NULL,
    reges_salariat_id  UUID NULL,
    status             VARCHAR(50) NOT NULL DEFAULT 'Pending',
    error_message      TEXT NULL,
    created_at         TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at         TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW()
);

-- === If the table already existed and 'id' is NOT identity, attach a sequence default (no drop) ===
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name   = 'idsreges_salariat'
          AND column_name  = 'id'
          AND is_identity  = 'YES'
    )
    AND NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name   = 'idsreges_salariat'
          AND column_name  = 'id'
          AND column_default LIKE 'nextval(%'
    )
    THEN
        EXECUTE format('CREATE SEQUENCE IF NOT EXISTS %I.idsreges_salariat_id_seq', current_schema());
        EXECUTE format('ALTER TABLE %I.idsreges_salariat ALTER COLUMN id SET DEFAULT nextval(''%I.idsreges_salariat_id_seq'')', current_schema(), current_schema());
        EXECUTE format('ALTER SEQUENCE %I.idsreges_salariat_id_seq OWNED BY %I.idsreges_salariat.id', current_schema(), current_schema());
        EXECUTE format('SELECT setval(''%I.idsreges_salariat_id_seq'', COALESCE((SELECT MAX(id) FROM %I.idsreges_salariat), 0) + 1, false)', current_schema(), current_schema());
    END IF;
END$$;

-- === Indexes (no-ops if already exist) ===
CREATE UNIQUE INDEX IF NOT EXISTS ux_idsreges_salariat_id_raspuns_mesaj
    ON idsreges_salariat (id_raspuns_mesaj) WHERE id_raspuns_mesaj IS NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS ux_idsreges_salariat_id_rezultat_mesaj
    ON idsreges_salariat (id_rezultat_mesaj) WHERE id_rezultat_mesaj IS NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS ux_idsreges_salariat_reges_salariat_id
    ON idsreges_salariat (reges_salariat_id) WHERE reges_salariat_id IS NOT NULL;
 
CREATE INDEX IF NOT EXISTS ix_idsreges_salariat_status      ON idsreges_salariat (status);
CREATE INDEX IF NOT EXISTS ix_idsreges_salariat_idpersoana  ON idsreges_salariat (idpersoana);

-- === Trigger to update 'updated_at' on row modification (no-op if exists) ===
CREATE OR REPLACE FUNCTION set_updated_at_idsreges_salariat()
RETURNS TRIGGER AS $f$
BEGIN
    NEW.updated_at := NOW();
    RETURN NEW;
END;
$f$ LANGUAGE plpgsql;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_trigger
        WHERE tgname = 'trg_set_updated_at_idsreges_salariat'
          AND tgrelid = 'idsreges_salariat'::regclass
    ) THEN
        CREATE TRIGGER trg_set_updated_at_idsreges_salariat
        BEFORE UPDATE ON idsreges_salariat
        FOR EACH ROW EXECUTE FUNCTION set_updated_at_idsreges_salariat();
    END IF;
END$$;
";
        return db.Database.ExecuteSqlRawAsync(sql);
    }
}
