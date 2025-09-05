using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace api_itm.Infrastructure
{
    /// <summary>
    /// Creează/actualizează tabela idsreges_contracte pentru tracking REGES (contracte).
    /// Poți apela în fiecare startup: await DbIdRagesContractsSetup.EnsureAsync(db);
    /// </summary>
    public static class DbIdRagesContractsSetup
    {
        public static Task EnsureAsync(DbContext db)
        {
            var sql = @"
-- === CREATE TABLE idsreges_contracte IF NOT EXISTS (final schema) ===
CREATE TABLE IF NOT EXISTS idsreges_contracte (
    id                 INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY, -- auto 1,2,3...
    idcontract         INTEGER NULL,
    idutilizator       INTEGER NULL,
    id_raspuns_mesaj   UUID NULL,
    id_rezultat_mesaj  UUID NULL,
    idautor            UUID NULL,
    reges_contract_id  UUID NULL,                                        -- referință contract
    status             VARCHAR(50) NOT NULL DEFAULT 'Pending',
    error_message      TEXT NULL,
    created_at         TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at         TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW()
);

-- === If the table already existed and 'id' is NOT identity, attach a sequence default (no drop) ===
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name   = 'idsreges_contracte'
          AND column_name  = 'id'
          AND is_identity  = 'YES'
    )
    AND NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name   = 'idsreges_contracte'
          AND column_name  = 'id'
          AND column_default LIKE 'nextval(%'
    )
    THEN
        EXECUTE format('CREATE SEQUENCE IF NOT EXISTS %I.idsreges_contracte_id_seq', current_schema());
        EXECUTE format('ALTER TABLE %I.idsreges_contracte ALTER COLUMN id SET DEFAULT nextval(''%I.idsreges_contracte_id_seq'')', current_schema(), current_schema());
        EXECUTE format('ALTER SEQUENCE %I.idsreges_contracte_id_seq OWNED BY %I.idsreges_contracte.id', current_schema(), current_schema());
        EXECUTE format('SELECT setval(''%I.idsreges_contracte_id_seq'', COALESCE((SELECT MAX(id) FROM %I.idsreges_contracte), 0) + 1, false)', current_schema(), current_schema());
    END IF;
END$$;

-- === Indexes (no-ops if already exist) ===
CREATE UNIQUE INDEX IF NOT EXISTS ux_idsreges_contracte_id_raspuns_mesaj
    ON idsreges_contracte (id_raspuns_mesaj) WHERE id_raspuns_mesaj IS NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS ux_idsreges_contracte_id_rezultat_mesaj
    ON idsreges_contracte (id_rezultat_mesaj) WHERE id_rezultat_mesaj IS NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS ux_idsreges_contracte_reges_contract_id
    ON idsreges_contracte (reges_contract_id) WHERE reges_contract_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS ix_idsreges_contracte_status
    ON idsreges_contracte (status);

CREATE INDEX IF NOT EXISTS ix_idsreges_contracte_idcontract
    ON idsreges_contracte (idcontract);

-- === Trigger to update 'updated_at' on row modification (no-op if exists) ===
CREATE OR REPLACE FUNCTION set_updated_at_idsreges_contracte()
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
        WHERE tgname = 'trg_set_updated_at_idsreges_contracte'
          AND tgrelid = 'idsreges_contracte'::regclass
    ) THEN
        CREATE TRIGGER trg_set_updated_at_idsreges_contracte
        BEFORE UPDATE ON idsreges_contracte
        FOR EACH ROW EXECUTE FUNCTION set_updated_at_idsreges_contracte();
    END IF;
END$$;
";
            return db.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
