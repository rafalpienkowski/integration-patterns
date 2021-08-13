CREATE SCHEMA IF NOT EXISTS customercare;

CREATE TABLE IF NOT  EXISTS customercare."Customers" (
    "Id" UUID NOT NULL,
    "FirstName" VARCHAR(255) NOT NULL,
    "LastName" VARCHAR(255) NOT NULL,
    "Address" VARCHAR(255) NOT NULL,
    CONSTRAINT customers_pkey PRIMARY KEY ("Id")
  );

CREATE SCHEMA IF NOT EXISTS customercarerecipients;
CREATE TABLE IF NOT EXISTS customercarerecipients."Recipients" (
    "Id" UUID NOT NULL,
    "Name" VARCHAR(255) NOT NULL,
    "Queue" VARCHAR(255) NOT NULL,
    CONSTRAINT recipients_pkey PRIMARY KEY ("Id")
);

INSERT INTO customercarerecipients."Recipients" ("Id", "Name", "Queue") VALUES('01bf3cd0-4c59-4570-aabb-974a58fa6581', 'Scheduling', 'scheduling-create-customer') ON CONFLICT DO NOTHING;

CREATE SCHEMA IF NOT EXISTS scheduling;
CREATE TABLE IF NOT  EXISTS scheduling."Customers" (
    "Id" UUID NOT NULL,
    "Name" VARCHAR(255) NOT NULL,
    CONSTRAINT customers_pkey PRIMARY KEY ("Id")
  );
CREATE TABLE IF NOT EXISTS scheduling."Doctors"(
    "Id" UUID NOT NULL,
    "Name" VARCHAR(255) NOT NULL,
    CONSTRAINT doctors_pkey PRIMARY KEY ("Id")
  );

INSERT INTO scheduling."Doctors"("Id", "Name") VALUES('9387c5d9-2b46-440f-945e-325e5a2a5d00', 'The best doctor ever') ON CONFLICT DO NOTHING;
INSERT INTO scheduling."Doctors"("Id", "Name") VALUES('7b979b2a-f020-438a-855d-fcc2f48d3ff6', 'An average doctor') ON CONFLICT DO NOTHING;

CREATE TABLE IF NOT EXISTS scheduling."Appointments" (
    "Id" UUID NOT NULL,
    "CustomerId" UUID NOT NULL,
    "DoctorId" UUID NOT NULL,
    "Date" TIMESTAMP NOT NULL,
    CONSTRAINT appointments_pkey PRIMARY KEY ("Id"),
    CONSTRAINT appointments_customers_fkey FOREIGN KEY ("CustomerId") REFERENCES scheduling."Customers"("Id"),
    CONSTRAINT appointments_doctors_fkey FOREIGN KEY ("DoctorId") REFERENCES scheduling."Doctors"("Id")
  );
