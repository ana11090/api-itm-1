using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace api_itm.Infrastructure
{
    /// <summary>
    /// Creează/actualizează tabela idsreges pentru tracking REGES.
    /// Poți apela în fiecare startup: await DbIdRagesSetup.EnsureAsync(db);
    /// </summary>
    public static class DbIdRagesSetup
    {
        public static Task EnsureAsync(DbContext db)
        {
            var sql = @"
-- === Create idsreges table if it doesn't exist (id auto-increment) ===
CREATE TABLE IF NOT EXISTS idsreges (
    id                 INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY, -- auto 1,2,3...
    idpersoana         INTEGER NULL,
    idutilizator        INTEGER NULL,
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
    -- If NOT identity and also no nextval default, create & attach a sequence as default
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name   = 'idsreges'
          AND column_name  = 'id'
          AND is_identity  = 'YES'
    )
    AND NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name   = 'idsreges'
          AND column_name  = 'id'
          AND column_default LIKE 'nextval(%'
    )
    THEN
        -- Create sequence if missing
        EXECUTE format('CREATE SEQUENCE IF NOT EXISTS %I.idsreges_id_seq', current_schema());
        -- Attach default
        EXECUTE format('ALTER TABLE %I.idsreges ALTER COLUMN id SET DEFAULT nextval(''%I.idsreges_id_seq'')', current_schema(), current_schema());
        -- Own the sequence to column
        EXECUTE format('ALTER SEQUENCE %I.idsreges_id_seq OWNED BY %I.idsreges.id', current_schema(), current_schema());
        -- Sync sequence to current max(id)
        EXECUTE format('SELECT setval(''%I.idsreges_id_seq'', COALESCE((SELECT MAX(id) FROM %I.idsreges), 0) + 1, false)', current_schema(), current_schema());
    END IF;
END$$;

-- === Indexes (no-ops if already exist) ===
CREATE UNIQUE INDEX IF NOT EXISTS ux_idsreges_id_raspuns_mesaj
    ON idsreges (id_raspuns_mesaj) WHERE id_raspuns_mesaj IS NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS ux_idsreges_id_rezultat_mesaj
    ON idsreges (id_rezultat_mesaj) WHERE id_rezultat_mesaj IS NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS ux_idsreges_reges_salariat_id
    ON idsreges (reges_salariat_id) WHERE reges_salariat_id IS NOT NULL;
 
CREATE INDEX IF NOT EXISTS ix_idsreges_status      ON idsreges (status);
CREATE INDEX IF NOT EXISTS ix_idsreges_idpersoana  ON idsreges (idpersoana);

-- === Trigger to update 'updated_at' on row modification (no-op if exists) ===
CREATE OR REPLACE FUNCTION set_updated_at_idsreges()
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
        WHERE tgname = 'trg_set_updated_at_idsreges'
          AND tgrelid = 'idsreges'::regclass
    ) THEN
        CREATE TRIGGER trg_set_updated_at_idsreges
        BEFORE UPDATE ON idsreges
        FOR EACH ROW EXECUTE FUNCTION set_updated_at_idsreges();
    END IF;
END$$;
";
            return db.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
