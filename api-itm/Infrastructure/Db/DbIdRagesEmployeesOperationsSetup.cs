using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace api_itm.Infrastructure.Db
{
    public static class DbIdRagesEmployeesOperationsSetup
    {
        public static Task EnsureAsync(DbContext db)
        {
            var sql = @"

-- Create table ru.idsreges_salariat_modificari (idempotent)
CREATE TABLE IF NOT EXISTS ru.idsreges_operation (
    id                 INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    idcontract         INTEGER NULL,
    idutilizator       INTEGER NULL,
    id_raspuns_mesaj   UUID NULL,
    id_rezultat_mesaj  UUID NULL,
    idautor            UUID NULL,
    reges_contract_id  UUID NULL,
    status             VARCHAR(50) NOT NULL DEFAULT 'Pending',
    error_message      TEXT NULL,
    created_at         TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at         TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW()
);

-- Non-unique indexes
CREATE INDEX IF NOT EXISTS ix_idsreges_operation_id_raspuns
  ON ru.idsreges_operation (id_raspuns_mesaj);
CREATE INDEX IF NOT EXISTS ix_idsreges_operation_id_rezultat
  ON ru.idsreges_operation (id_rezultat_mesaj);
CREATE INDEX IF NOT EXISTS ix_idsreges_operation_reges_id
  ON ru.idsreges_operation (reges_contract_id);
CREATE INDEX IF NOT EXISTS ix_idsreges_operation_status
  ON ru.idsreges_operation (status);
CREATE INDEX IF NOT EXISTS ix_idsreges_operation_idcontract
  ON ru.idsreges_operation (idcontract);

-- updated_at trigger
CREATE OR REPLACE FUNCTION ru.set_updated_at_idsreges_operation()
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
    WHERE tgname = 'trg_set_updated_at_idsreges_operation'
      AND tgrelid = 'ru.idsreges_operation'::regclass
  ) THEN
    CREATE TRIGGER trg_set_updated_at_idsreges_operation
      BEFORE UPDATE ON ru.idsreges_operation
      FOR EACH ROW EXECUTE FUNCTION ru.set_updated_at_idsreges_operation();
  END IF;
END$$;

-- === Rename the employee table and add idoperatie (idempotent) ===
ALTER TABLE IF EXISTS ru.idsreges_salariat_modificari
RENAME TO idsreges_salariat_operatii;

ALTER TABLE IF EXISTS ru.idsreges_salariat_operatii
  ADD COLUMN IF NOT EXISTS idoperatie INTEGER NULL;

CREATE INDEX IF NOT EXISTS ix_idsreges_salariat_operatii_idoperatie
  ON ru.idsreges_salariat_operatii (idoperatie);

DO $$
BEGIN
  IF EXISTS (
    SELECT 1 FROM pg_trigger
    WHERE tgname = 'trg_set_updated_at_idsreges_salariat_modificari'
      AND tgrelid = 'ru.idsreges_salariat_operatii'::regclass
  ) THEN
    EXECUTE 'ALTER TRIGGER trg_set_updated_at_idsreges_salariat_modificari ON ru.idsreges_salariat_operatii RENAME TO trg_set_updated_at_idsreges_salariat_operatii';
  END IF;
END$$;
";
            return db.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
