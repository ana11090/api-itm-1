using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace api_itm.Infrastructure.Db
{
    public static class DbIdRagesEmployeesModificariSetup
    {
        public static Task EnsureAsync(DbContext db)
        {
            var sql = @"
-- Ensure schema ru exists
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = 'ru') THEN
    EXECUTE 'CREATE SCHEMA ru';
  END IF;
END$$;

-- Create table ru.idsreges_salariat_modificari (idempotent)
CREATE TABLE IF NOT EXISTS ru.idsreges_salariat_modificari (
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

-- Indexes (non-unique, except PK)
CREATE INDEX IF NOT EXISTS ix_idsreges_salariat_modif_id_raspuns
  ON ru.idsreges_salariat_modificari (id_raspuns_mesaj);

CREATE INDEX IF NOT EXISTS ix_idsreges_salariat_modif_id_rezultat
  ON ru.idsreges_salariat_modificari (id_rezultat_mesaj);

CREATE INDEX IF NOT EXISTS ix_idsreges_salariat_modif_reges_id
  ON ru.idsreges_salariat_modificari (reges_salariat_id);

CREATE INDEX IF NOT EXISTS ix_idsreges_salariat_modif_status
  ON ru.idsreges_salariat_modificari (status);

CREATE INDEX IF NOT EXISTS ix_idsreges_salariat_modif_idpersoana
  ON ru.idsreges_salariat_modificari (idpersoana);

-- Trigger function + trigger for updated_at
CREATE OR REPLACE FUNCTION ru.set_updated_at_idsreges_salariat_modificari()
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
    WHERE tgname = 'trg_set_updated_at_idsreges_salariat_modificari'
      AND tgrelid = 'ru.idsreges_salariat_modificari'::regclass
  ) THEN
    CREATE TRIGGER trg_set_updated_at_idsreges_salariat_modificari
      BEFORE UPDATE ON ru.idsreges_salariat_modificari
      FOR EACH ROW EXECUTE FUNCTION ru.set_updated_at_idsreges_salariat_modificari();
  END IF;
END$$;
";
            return db.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
