-- Additive schema update for the existing PostgreSQL tables.
-- Existing rows are preserved; new reference columns are nullable for backfill.

ALTER TABLE "Orders"
    ADD COLUMN IF NOT EXISTS "CustomerId" integer,
    ADD COLUMN IF NOT EXISTS "OrderStatusId" integer;

ALTER TABLE "OrderPoints"
    ADD COLUMN IF NOT EXISTS "ContactPersonId" integer;

ALTER TABLE "Cargos"
    ADD COLUMN IF NOT EXISTS "VolumeM3" double precision,
    ADD COLUMN IF NOT EXISTS "MinTemperatureC" double precision,
    ADD COLUMN IF NOT EXISTS "MaxTemperatureC" double precision;

ALTER TABLE "Vehicles"
    ADD COLUMN IF NOT EXISTS "ExteriorLengthMeters" double precision,
    ADD COLUMN IF NOT EXISTS "ExteriorWidthMeters" double precision,
    ADD COLUMN IF NOT EXISTS "ExteriorHeightMeters" double precision,
    ADD COLUMN IF NOT EXISTS "InteriorLengthMeters" double precision,
    ADD COLUMN IF NOT EXISTS "InteriorWidthMeters" double precision,
    ADD COLUMN IF NOT EXISTS "InteriorHeightMeters" double precision,
    ADD COLUMN IF NOT EXISTS "EmptyWeightKg" double precision,
    ADD COLUMN IF NOT EXISTS "MaxGrossWeightKg" double precision,
    ADD COLUMN IF NOT EXISTS "VehicleStatusId" integer;

ALTER TABLE "Users"
    ADD COLUMN IF NOT EXISTS "EmployeeId" integer;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Orders_Customers_CustomerId' AND conrelid = '"Orders"'::regclass) THEN
        ALTER TABLE "Orders" ADD CONSTRAINT "FK_Orders_Customers_CustomerId"
            FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE SET NULL NOT VALID;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Orders_OrderStatuses_OrderStatusId' AND conrelid = '"Orders"'::regclass) THEN
        ALTER TABLE "Orders" ADD CONSTRAINT "FK_Orders_OrderStatuses_OrderStatusId"
            FOREIGN KEY ("OrderStatusId") REFERENCES "OrderStatuses" ("Id") ON DELETE RESTRICT NOT VALID;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_OrderPoints_ContactPersons_ContactPersonId' AND conrelid = '"OrderPoints"'::regclass) THEN
        ALTER TABLE "OrderPoints" ADD CONSTRAINT "FK_OrderPoints_ContactPersons_ContactPersonId"
            FOREIGN KEY ("ContactPersonId") REFERENCES "ContactPersons" ("Id") ON DELETE SET NULL NOT VALID;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Users_Employees_EmployeeId' AND conrelid = '"Users"'::regclass) THEN
        ALTER TABLE "Users" ADD CONSTRAINT "FK_Users_Employees_EmployeeId"
            FOREIGN KEY ("EmployeeId") REFERENCES "Employees" ("Id") ON DELETE SET NULL NOT VALID;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Users_Roles_Role' AND conrelid = '"Users"'::regclass) THEN
        ALTER TABLE "Users" ADD CONSTRAINT "FK_Users_Roles_Role"
            FOREIGN KEY ("Role") REFERENCES "Roles" ("Id") ON DELETE RESTRICT NOT VALID;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Vehicles_VehicleStatuses_VehicleStatusId' AND conrelid = '"Vehicles"'::regclass) THEN
        ALTER TABLE "Vehicles" ADD CONSTRAINT "FK_Vehicles_VehicleStatuses_VehicleStatusId"
            FOREIGN KEY ("VehicleStatusId") REFERENCES "VehicleStatuses" ("Id") ON DELETE RESTRICT NOT VALID;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'CK_Cargos_VolumeM3_NonNegative' AND conrelid = '"Cargos"'::regclass) THEN
        ALTER TABLE "Cargos" ADD CONSTRAINT "CK_Cargos_VolumeM3_NonNegative"
            CHECK ("VolumeM3" IS NULL OR "VolumeM3" >= 0) NOT VALID;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'CK_Cargos_TemperatureRange' AND conrelid = '"Cargos"'::regclass) THEN
        ALTER TABLE "Cargos" ADD CONSTRAINT "CK_Cargos_TemperatureRange"
            CHECK ("MinTemperatureC" IS NULL OR "MaxTemperatureC" IS NULL OR "MinTemperatureC" <= "MaxTemperatureC") NOT VALID;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'CK_Vehicles_Dimensions_NonNegative' AND conrelid = '"Vehicles"'::regclass) THEN
        ALTER TABLE "Vehicles" ADD CONSTRAINT "CK_Vehicles_Dimensions_NonNegative"
            CHECK (
                ("ExteriorLengthMeters" IS NULL OR "ExteriorLengthMeters" >= 0) AND
                ("ExteriorWidthMeters" IS NULL OR "ExteriorWidthMeters" >= 0) AND
                ("ExteriorHeightMeters" IS NULL OR "ExteriorHeightMeters" >= 0) AND
                ("InteriorLengthMeters" IS NULL OR "InteriorLengthMeters" >= 0) AND
                ("InteriorWidthMeters" IS NULL OR "InteriorWidthMeters" >= 0) AND
                ("InteriorHeightMeters" IS NULL OR "InteriorHeightMeters" >= 0) AND
                ("EmptyWeightKg" IS NULL OR "EmptyWeightKg" >= 0) AND
                ("MaxGrossWeightKg" IS NULL OR "MaxGrossWeightKg" >= 0)
            ) NOT VALID;
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS "IX_Orders_CustomerId" ON "Orders" ("CustomerId");
CREATE INDEX IF NOT EXISTS "IX_Orders_OrderStatusId" ON "Orders" ("OrderStatusId");
CREATE INDEX IF NOT EXISTS "IX_OrderPoints_ContactPersonId" ON "OrderPoints" ("ContactPersonId");
CREATE INDEX IF NOT EXISTS "IX_Vehicles_VehicleStatusId" ON "Vehicles" ("VehicleStatusId");

-- EmployeeId is nullable so legacy accounts can be linked after this update.
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_EmployeeId" ON "Users" ("EmployeeId");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Username" ON "Users" ("Username");
