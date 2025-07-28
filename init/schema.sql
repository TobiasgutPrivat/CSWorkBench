-- DROP TABLE IF EXISTS "Attachment";
-- DROP TABLE IF EXISTS "Object";

CREATE TABLE IF NOT EXISTS "Object" (
    "id" SERIAL PRIMARY KEY,
    "class" VARCHAR(255) NOT NULL,
    "data" TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS "Attachment" (
    "parent_id" INTEGER NOT NULL,
    "path" VARCHAR(255) NOT NULL,
    "name" VARCHAR(255) NOT NULL,
    "object_id" INTEGER NOT NULL,
    PRIMARY KEY ("parent_id", "path", "name"),
    FOREIGN KEY ("parent_id") REFERENCES "Object" ("id") ON DELETE CASCADE,
    FOREIGN KEY ("object_id") REFERENCES "Object" ("id") ON DELETE CASCADE
);
