-- Script de creación de la base de datos para Fya Créditos.
-- Ejecutar contra una base PostgreSQL vacía:
--   psql -h localhost -U fya -d fya_creditos -f db/init.sql
-- (docker-compose lo ejecuta automáticamente al levantar el contenedor de Postgres)

CREATE TABLE IF NOT EXISTS creditos (
    id               UUID PRIMARY KEY,
    nombre_cliente   VARCHAR(200) NOT NULL,
    cedula           VARCHAR(30)  NOT NULL,
    valor_credito    NUMERIC(14,2) NOT NULL CHECK (valor_credito > 0),
    tasa_interes     NUMERIC(6,3)  NOT NULL CHECK (tasa_interes >= 0),
    plazo_meses      INTEGER NOT NULL CHECK (plazo_meses > 0),
    nombre_comercial VARCHAR(200) NOT NULL,
    fecha_registro   TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_creditos_nombre_cliente   ON creditos (nombre_cliente);
CREATE INDEX IF NOT EXISTS idx_creditos_cedula           ON creditos (cedula);
CREATE INDEX IF NOT EXISTS idx_creditos_nombre_comercial ON creditos (nombre_comercial);
CREATE INDEX IF NOT EXISTS idx_creditos_fecha_registro   ON creditos (fecha_registro);
CREATE INDEX IF NOT EXISTS idx_creditos_valor_credito    ON creditos (valor_credito);

-- Outbox de notificaciones por correo: una fila por crédito registrado.
-- La procesa en segundo plano el EmailOutboxBackgroundService de la API.
CREATE TABLE IF NOT EXISTS notificaciones_correo (
    id           UUID PRIMARY KEY,
    credito_id   UUID NOT NULL REFERENCES creditos(id) ON DELETE CASCADE,
    estado       VARCHAR(20) NOT NULL DEFAULT 'PENDIENTE'
                 CHECK (estado IN ('PENDIENTE', 'ENVIADO', 'ERROR')),
    intentos     INTEGER NOT NULL DEFAULT 0,
    ultimo_error TEXT,
    creado_en    TIMESTAMPTZ NOT NULL DEFAULT now(),
    enviado_en   TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS idx_notif_estado ON notificaciones_correo (estado);
