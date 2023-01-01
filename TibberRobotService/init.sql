CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE "MovementSummaries" (
id                      UUID                DEFAULT uuid_generate_v4(),
timestamp               TIMESTAMPTZ         NOT NULL,
duration                FLOAT               NOT NULL,
commands                INT                 NOT NULL,
result                  INT                 NOT NULL
);