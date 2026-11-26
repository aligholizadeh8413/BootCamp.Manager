CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "Instructors" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Instructors" PRIMARY KEY AUTOINCREMENT,
    "FullName" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "Expertise" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL
);

CREATE TABLE "Students" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Students" PRIMARY KEY AUTOINCREMENT,
    "FullName" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL
);

CREATE TABLE "Sessions" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Sessions" PRIMARY KEY AUTOINCREMENT,
    "Title" TEXT NOT NULL,
    "Date" TEXT NOT NULL,
    "InstructorId" INTEGER NOT NULL,
    "TaskDescription" TEXT NOT NULL,
    "ProgressPercent" INTEGER NOT NULL,
    "IsCompleted" INTEGER NOT NULL,
    CONSTRAINT "CK_Sessions_Progress" CHECK (ProgressPercent BETWEEN 0 AND 100),
    CONSTRAINT "FK_Sessions_Instructors_InstructorId" FOREIGN KEY ("InstructorId") REFERENCES "Instructors" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "SessionParticipants" (
    "SessionId" INTEGER NOT NULL,
    "StudentId" INTEGER NOT NULL,
    "Status" TEXT NOT NULL,
    CONSTRAINT "PK_SessionParticipants" PRIMARY KEY ("SessionId", "StudentId"),
    CONSTRAINT "CK_Participants_Status" CHECK (Status IN ('Pending', 'Present', 'Absent')),
    CONSTRAINT "FK_SessionParticipants_Sessions_SessionId" FOREIGN KEY ("SessionId") REFERENCES "Sessions" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_SessionParticipants_Students_StudentId" FOREIGN KEY ("StudentId") REFERENCES "Students" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Evaluations" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Evaluations" PRIMARY KEY AUTOINCREMENT,
    "SessionId" INTEGER NOT NULL,
    "StudentId" INTEGER NOT NULL,
    "InstructorId" INTEGER NOT NULL,
    "Score" INTEGER NOT NULL,
    "Notes" TEXT NOT NULL,
    CONSTRAINT "CK_Evaluations_Score" CHECK (Score BETWEEN 0 AND 100),
    CONSTRAINT "FK_Evaluations_Instructors_InstructorId" FOREIGN KEY ("InstructorId") REFERENCES "Instructors" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Evaluations_SessionParticipants_SessionId_StudentId" FOREIGN KEY ("SessionId", "StudentId") REFERENCES "SessionParticipants" ("SessionId", "StudentId") ON DELETE CASCADE
);

CREATE INDEX "IX_Evaluations_InstructorId" ON "Evaluations" ("InstructorId");

CREATE UNIQUE INDEX "IX_Evaluations_SessionId_StudentId" ON "Evaluations" ("SessionId", "StudentId");

CREATE UNIQUE INDEX "IX_Instructors_Email" ON "Instructors" ("Email");

CREATE INDEX "IX_SessionParticipants_StudentId" ON "SessionParticipants" ("StudentId");

CREATE INDEX "IX_Sessions_InstructorId" ON "Sessions" ("InstructorId");

CREATE UNIQUE INDEX "IX_Students_Email" ON "Students" ("Email");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261126103017_InitialCreate', '10.0.14');

COMMIT;

