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
-- Ensure schema ru exists (works even if it already exists)
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = 'ru') THEN
    EXECUTE 'CREATE SCHEMA ru';
  END IF;
END$$;

-- Try to migrate any legacy tables to ru
DO $$
BEGIN
  -- Case A: ru.idsreges exists -> rename inside ru
  IF to_regclass('ru.idsreges_salariat') IS NULL
     AND to_regclass('ru.idsreges') IS NOT NULL THEN
    ALTER TABLE ru.idsreges RENAME TO idsreges_salariat;
  END IF;

  -- Case B: public.idsreges_salariat exists -> move to ru
  IF to_regclass('ru.idsreges_salariat') IS NULL
     AND to_regclass('public.idsreges_salariat') IS NOT NULL THEN
    ALTER TABLE public.idsreges_salariat SET SCHEMA ru;
  END IF;

  -- Case C: public.idsreges exists -> move then rename in ru
  IF to_regclass('ru.idsreges_salariat') IS NULL
     AND to_regclass('public.idsreges') IS NOT NULL THEN
    ALTER TABLE public.idsreges SET SCHEMA ru;
    ALTER TABLE ru.idsreges RENAME TO idsreges_salariat;
  END IF;
END$$;

-- Create table in ru if still missing
CREATE TABLE IF NOT EXISTS ru.idsreges_salariat (
    id                 INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
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

-- Indexes (all safe no-ops if already present)
CREATE UNIQUE INDEX IF NOT EXISTS ux_idsreges_salariat_id_raspuns_mesaj
  ON ru.idsreges_salariat (id_raspuns_mesaj) WHERE id_raspuns_mesaj IS NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS ux_idsreges_salariat_id_rezultat_mesaj
  ON ru.idsreges_salariat (id_rezultat_mesaj) WHERE id_rezultat_mesaj IS NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS ux_idsreges_salariat_reges_salariat_id
  ON ru.idsreges_salariat (reges_salariat_id) WHERE reges_salariat_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS ix_idsreges_salariat_status
  ON ru.idsreges_salariat (status);

CREATE INDEX IF NOT EXISTS ix_idsreges_salariat_idpersoana
  ON ru.idsreges_salariat (idpersoana);

-- Trigger function in ru
CREATE OR REPLACE FUNCTION ru.set_updated_at_idsreges_salariat()
RETURNS TRIGGER AS $f$
BEGIN
  NEW.updated_at := NOW();
  RETURN NEW;
END;
$f$ LANGUAGE plpgsql;

-- Trigger on ru.idsreges_salariat (guarded)
DO $$
BEGIN
  IF to_regclass('ru.idsreges_salariat') IS NOT NULL
     AND NOT EXISTS (
       SELECT 1 FROM pg_trigger
       WHERE tgname = 'trg_set_updated_at_idsreges_salariat'
         AND tgrelid = to_regclass('ru.idsreges_salariat')
     )
  THEN
    CREATE TRIGGER trg_set_updated_at_idsreges_salariat
      BEFORE UPDATE ON ru.idsreges_salariat
      FOR EACH ROW EXECUTE FUNCTION ru.set_updated_at_idsreges_salariat();
  END IF;
END$$;
";
            return db.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
