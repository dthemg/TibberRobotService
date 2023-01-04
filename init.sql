CREATE TABLE executions (
id                      UUID                DEFAULT gen_random_uuid(),
timestamp               TIMESTAMPTZ         NOT NULL,
duration                FLOAT               NOT NULL,
commands                INT                 NOT NULL,
result                  INT                 NOT NULL
);