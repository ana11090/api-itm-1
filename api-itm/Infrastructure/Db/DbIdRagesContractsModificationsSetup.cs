using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace api_itm.Infrastructure.Db
{
    public class DbIdRagesContractsModificationsSetup
    {
        public static Task EnsureAsync(DbContext db)
        {
            var sql = @"
CREATE TABLE IF NOT EXISTS ru.idsreges_contracte_modificari (
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

-- ensure no uniqueness on idcontract (drop any unique index created earlier)
DROP INDEX IF EXISTS ru.uq_idsreges_contracte_modif_idcontract_future;
ALTER TABLE ru.idsreges_contracte_modificari ADD COLUMN IF NOT EXISTS operation VARCHAR(100) NULL;

CREATE INDEX IF NOT EXISTS ix_idsreges_contracte_modif_idcontract
  ON ru.idsreges_contracte_modificari (idcontract);

CREATE INDEX IF NOT EXISTS ix_idsreges_contracte_modif_id_raspuns
  ON ru.idsreges_contracte_modificari (id_raspuns_mesaj);

CREATE INDEX IF NOT EXISTS ix_idsreges_contracte_modif_id_rezultat
  ON ru.idsreges_contracte_modificari (id_rezultat_mesaj);

CREATE INDEX IF NOT EXISTS ix_idsreges_contracte_modif_reges_id
  ON ru.idsreges_contracte_modificari (reges_contract_id);

CREATE INDEX IF NOT EXISTS ix_idsreges_contracte_modif_status
  ON ru.idsreges_contracte_modificari (status);

CREATE OR REPLACE FUNCTION ru.set_updated_at_idsreges_contracte_modificari()
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
    WHERE tgname = 'trg_set_updated_at_idsreges_contracte_modificari'
      AND tgrelid = 'ru.idsreges_contracte_modificari'::regclass
  ) THEN
    CREATE TRIGGER trg_set_updated_at_idsreges_contracte_modificari
      BEFORE UPDATE ON ru.idsreges_contracte_modificari
      FOR EACH ROW EXECUTE FUNCTION ru.set_updated_at_idsreges_contracte_modificari();
  END IF;
END$$;
";
            return db.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
