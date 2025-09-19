using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace api_itm.Infrastructure.Db
{
    public static class DbIdRagesEmployeesModificationsSetup
    {
        public static Task EnsureAsync(DbContext db)
        {
            var sql = @"

-- Create table ru.idsreges_salariat_modificari (idempotent)
CREATE TABLE IF NOT EXISTS ru.idsreges_contract (
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
CREATE INDEX IF NOT EXISTS ix_idsreges_contract_id_raspuns
  ON ru.idsreges_contract (id_raspuns_mesaj);
CREATE INDEX IF NOT EXISTS ix_idsreges_contract_id_rezultat
  ON ru.idsreges_contract (id_rezultat_mesaj);
CREATE INDEX IF NOT EXISTS ix_idsreges_contract_reges_id
  ON ru.idsreges_contract (reges_contract_id);
CREATE INDEX IF NOT EXISTS ix_idsreges_contract_status
  ON ru.idsreges_contract (status);
CREATE INDEX IF NOT EXISTS ix_idsreges_contract_idcontract
  ON ru.idsreges_contract (idcontract);

-- updated_at trigger
CREATE OR REPLACE FUNCTION ru.set_updated_at_idsreges_contract()
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
    WHERE tgname = 'trg_set_updated_at_idsreges_contract'
      AND tgrelid = 'ru.idsreges_contract'::regclass
  ) THEN
    CREATE TRIGGER trg_set_updated_at_idsreges_contract
      BEFORE UPDATE ON ru.idsreges_contract
      FOR EACH ROW EXECUTE FUNCTION ru.set_updated_at_idsreges_contract();
  END IF;
END$$;
";
            return db.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
