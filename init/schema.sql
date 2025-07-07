-- drop table if exists "Attachment";
-- drop table if exists "Object";

Create Table if not exists "Object" (
    "id" SERIAL PRIMARY KEY,
    "class" VARCHAR(255) NOT NULL,
    "data" TEXT NOT NULL,
);

Create Table if not exists "Attachment" (
    "parent_id" SERIAL NOT NULL,
    "path" VARCHAR(255) NOT NULL,
    "name" VARCHAR(255) NOT NULL,
    "object_id" SERIAL NOT NULL,
    PRIMARY KEY ("parent_id", "path", "name"),
    FOREIGN KEY ("parent_id") REFERENCES "Object" ("id") ON DELETE CASCADE,
    FOREIGN KEY ("object_id") REFERENCES "Object" ("id") ON DELETE CASCADE
)