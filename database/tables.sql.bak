BEGIN;

CREATE SCHEMA IF NOT EXISTS ANHELOPETS;
SET search_path TO ANHELOPETS, public;


-- ============================================================
-- Users and security
-- ============================================================

CREATE TABLE users (
    user_id 	   text DEFAULT generate_user_id() PRIMARY KEY,
    username       varchar(100) NOT NULL,
    password_hash  text NOT NULL,
    active         boolean NOT NULL DEFAULT true,

    created_at     timestamptz,
    created_by     varchar(100),
    modified_at    timestamptz,
    modified_by    varchar(100),

    CONSTRAINT uq_users_username UNIQUE (username)
);

CREATE TABLE user_profiles (
    user_profile_id bigint GENERATED ALWAYS AS IDENTITY,
    user_id         varchar(100) NOT NULL,
    national_id     varchar(50),
    first_name      varchar(100) NOT NULL,
    middle_name     varchar(100),
    last_name       varchar(100) NOT NULL,
    second_last_name varchar(100),
    birth_date      date,
    nationality     varchar(100),

    created_at      timestamptz,
    created_by      varchar(100),
    modified_at     timestamptz,
    modified_by     varchar(100),

    CONSTRAINT pk_user_profiles PRIMARY KEY (user_profile_id),
    CONSTRAINT uq_user_profiles_user_id UNIQUE (user_id),
    CONSTRAINT fk_user_profiles_user_id
        FOREIGN KEY (user_id)
        REFERENCES users (user_id)
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

CREATE UNIQUE INDEX ux_user_profiles_national_id
    ON user_profiles (national_id)
    WHERE national_id IS NOT NULL;

CREATE TABLE user_contacts (
    user_contact_id bigint GENERATED ALWAYS AS IDENTITY,
    user_id         varchar(100) NOT NULL,
    email           varchar(255) NOT NULL,
    phone_primary   varchar(30) NOT NULL,
    phone_secondary varchar(30),
    city            varchar(100),
    town            varchar(100),
    district        varchar(100),
    address_line    text,

    created_at      timestamptz,
    created_by      varchar(100),
    modified_at     timestamptz,
    modified_by     varchar(100),

    CONSTRAINT pk_user_contacts PRIMARY KEY (user_contact_id),
    CONSTRAINT uq_user_contacts_user_id UNIQUE (user_id),
    CONSTRAINT uq_user_contacts_email UNIQUE (email),
    CONSTRAINT fk_user_contacts_user_id
        FOREIGN KEY (user_id)
        REFERENCES users (user_id)
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

CREATE TABLE roles (
    role_id     bigint GENERATED ALWAYS AS IDENTITY,
    role_name   varchar(100) NOT NULL,
    role_access varchar(100) NOT NULL,
    description text,

    created_at  timestamptz,
    created_by  varchar(100),
    modified_at timestamptz,
    modified_by varchar(100),

    CONSTRAINT pk_roles PRIMARY KEY (role_id),
    CONSTRAINT uq_roles_role_name UNIQUE (role_name)
);

CREATE TABLE user_roles (
    user_role_id bigint GENERATED ALWAYS AS IDENTITY,
    user_id      text NOT NULL,
    role_id      bigint NOT NULL,
    description  text,

    created_at   timestamptz,
    created_by   varchar(100),
    modified_at  timestamptz,
    modified_by  varchar(100),

    CONSTRAINT pk_user_roles PRIMARY KEY (user_role_id),
    CONSTRAINT uq_user_roles_user_role UNIQUE (user_id, role_id),
    CONSTRAINT fk_user_roles_user_id
        FOREIGN KEY (user_id)
        REFERENCES users (user_id)
        ON UPDATE CASCADE
        ON DELETE CASCADE,
    CONSTRAINT fk_user_roles_role_id
        FOREIGN KEY (role_id)
        REFERENCES roles (role_id)
        ON UPDATE CASCADE
        ON DELETE RESTRICT
);

-- Roles base: coinciden con los valores que ya usaba el frontend
INSERT INTO roles (role_name, role_access, description, created_at, created_by)
VALUES
    ('Admin',      'admin',      'Acceso total al panel de administración', now(), 'system'),
    ('Voluntario', 'voluntario', 'Usuario con perfil de voluntario activo', now(), 'system'),
    ('Usuario',    'usuario',    'Cuenta estándar sin privilegios administrativos', now(), 'system')
ON CONFLICT (role_name) DO NOTHING;

-- ============================================================
-- ANHELOPETS domain
-- ============================================================

CREATE TABLE animals (
    animal_id     text DEFAULT generate_id('ANM') PRIMARY KEY,
    species       varchar(100) NOT NULL,
    breed         varchar(100),
    animal_name   varchar(100),
    animal_status varchar(50) NOT NULL,
    health_status varchar(50) NOT NULL,
    birth_date    date,
    sex           char(1),
    description   text,

    created_at    timestamptz DEFAULT CURRENT_TIMESTAMP,
    created_by    varchar(100),
    modified_at   timestamptz DEFAULT CURRENT_TIMESTAMP,
    modified_by   varchar(100)
);

CREATE TABLE animal_photos (
    animal_photo_id bigint GENERATED ALWAYS AS IDENTITY,
    animal_id       text NOT NULL,
    photo_url       text NOT NULL,
    description     text,
    is_primary      boolean NOT NULL DEFAULT false,
    display_order   integer NOT NULL DEFAULT 0,

    created_at      timestamptz,
    created_by      varchar(100),
    modified_at     timestamptz,
    modified_by     varchar(100),

    CONSTRAINT pk_animal_photos PRIMARY KEY (animal_photo_id),
    CONSTRAINT ck_animal_photos_photo_url_not_empty CHECK (btrim(photo_url) <> ''),
    CONSTRAINT ck_animal_photos_display_order_non_negative CHECK (display_order >= 0),
    CONSTRAINT fk_animal_photos_animal_id
        FOREIGN KEY (animal_id)
        REFERENCES animals (animal_id)
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

CREATE TABLE animal_intakes (
    animal_intake_id bigint GENERATED ALWAYS AS IDENTITY,
    animal_id        text NOT NULL,
    intake_type      varchar(50) NOT NULL,
    reported_by_user_id text NOT NULL,
    intake_address   text,
    notes            text,
    intake_at        timestamptz NOT NULL,

    created_at       timestamptz,
    created_by       varchar(100),
    modified_at      timestamptz,
    modified_by      varchar(100),

    CONSTRAINT pk_animal_intakes PRIMARY KEY (animal_intake_id),
    CONSTRAINT fk_animal_intakes_animal_id
        FOREIGN KEY (animal_id)
        REFERENCES animals (animal_id)
        ON UPDATE CASCADE
        ON DELETE RESTRICT,
    CONSTRAINT fk_animal_intakes_reported_by_user_id
        FOREIGN KEY (reported_by_user_id)
        REFERENCES users (user_id)
        ON UPDATE CASCADE
        ON DELETE RESTRICT
);

CREATE TABLE rescue_records (
    rescue_id      bigint GENERATED ALWAYS AS IDENTITY,
    animal_id      text,
    rescue_date    date NOT NULL,
    location       text NOT NULL,
    description    text NOT NULL,
    status         varchar(30) NOT NULL DEFAULT 'Activo',
    foster_home_id text,
    volunteer_id   text,

    created_at     timestamptz,
    created_by     varchar(100),
    modified_at    timestamptz,
    modified_by    varchar(100),

    CONSTRAINT pk_rescue_records PRIMARY KEY (rescue_id),
    CONSTRAINT ck_rescue_records_date_not_future CHECK (rescue_date <= CURRENT_DATE),
    CONSTRAINT ck_rescue_records_status CHECK (status IN ('Activo', 'Cerrado')),
    CONSTRAINT fk_rescue_records_animal_id
        FOREIGN KEY (animal_id)
        REFERENCES animals (animal_id)
        ON UPDATE CASCADE
        ON DELETE SET NULL
);

CREATE TABLE volunteers (
    volunteer_id text DEFAULT generate_id('VOL') PRIMARY KEY,
    user_id      text NOT NULL,
    active       boolean NOT NULL,
    national_id  varchar(50),
    volunteer_type varchar(100),
    motivation   text,
    application_details text,
    validation_status varchar(20) NOT NULL DEFAULT 'Pendiente',
    validation_notes text,
    validated_at timestamptz,
    validated_by_user_id text,

    created_at   timestamptz,
    created_by   varchar(100),
    modified_at  timestamptz,
    modified_by  varchar(100),

    CONSTRAINT fk_volunteers_user_id
        FOREIGN KEY (user_id)
        REFERENCES users (user_id)
        ON UPDATE CASCADE
        ON DELETE CASCADE,
    CONSTRAINT ck_volunteers_validation_status
        CHECK (validation_status IN ('Pendiente', 'Aprobado', 'Rechazado'))
);

CREATE TABLE veterinarians (
    veterinarian_id text DEFAULT generate_id('VET') PRIMARY KEY,
    volunteer_id    TEXT NOT NULL,
    specialty       varchar(100) NOT NULL,

    created_at      timestamptz,
    created_by      varchar(100),
    modified_at     timestamptz,
    modified_by     varchar(100),

    CONSTRAINT uq_veterinarians_volunteer_id UNIQUE (volunteer_id),
    CONSTRAINT fk_veterinarians_volunteer_id
        FOREIGN KEY (volunteer_id)
        REFERENCES volunteers (volunteer_id)
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

CREATE TABLE animal_medical_records (
    animal_medical_record_id bigint GENERATED ALWAYS AS IDENTITY,
    animal_id                text NOT NULL,
    veterinarian_id          text NOT NULL,
    diagnosis                text NOT NULL,
    treatment                text NOT NULL,
    notes                    text,
    visit_date               date NOT NULL,

    created_at               timestamptz,
    created_by               varchar(100),
    modified_at              timestamptz,
    modified_by              varchar(100),

    CONSTRAINT pk_animal_medical_records PRIMARY KEY (animal_medical_record_id),
    CONSTRAINT fk_animal_medical_records_animal_id
        FOREIGN KEY (animal_id)
        REFERENCES animals (animal_id)
        ON UPDATE CASCADE
        ON DELETE RESTRICT,
    CONSTRAINT fk_animal_medical_records_veterinarian_id
        FOREIGN KEY (veterinarian_id)
        REFERENCES veterinarians (veterinarian_id)
        ON UPDATE CASCADE
        ON DELETE RESTRICT
);

CREATE TABLE animal_care_schedules (
    animal_care_schedule_id  bigint GENERATED ALWAYS AS IDENTITY,
    animal_id                bigint NOT NULL,
    animal_medical_record_id bigint,
    care_type                varchar(100) NOT NULL,
    care_condition           varchar(100) NOT NULL,
    frequency                varchar(100) NOT NULL,
    last_care_at             timestamptz NOT NULL,
    next_due_date            date NOT NULL,
    notes                    text,

    created_at               timestamptz,
    created_by               varchar(100),
    modified_at              timestamptz,
    modified_by              varchar(100),

    CONSTRAINT pk_animal_care_schedules PRIMARY KEY (animal_care_schedule_id),
    CONSTRAINT fk_animal_care_schedules_animal_id
        FOREIGN KEY (animal_id)
        REFERENCES animals (animal_id)
        ON UPDATE CASCADE
        ON DELETE RESTRICT,
    CONSTRAINT fk_animal_care_schedules_medical_record_id
        FOREIGN KEY (animal_medical_record_id)
        REFERENCES animal_medical_records (animal_medical_record_id)
        ON UPDATE CASCADE
        ON DELETE SET NULL
);

CREATE TABLE foster_homes (
    foster_home_id text DEFAULT generate_id('FHM') PRIMARY KEY,
    volunteer_id   TEXT,
    name           varchar(150) NOT NULL,
    address        text NOT NULL,
    phone          varchar(30) NOT NULL,
    responsible    varchar(150) NOT NULL,
    capacity       integer NOT NULL,
    active         boolean NOT NULL DEFAULT true,

    created_at     timestamptz,
    created_by     varchar(100),
    modified_at    timestamptz,
    modified_by    varchar(100),

    CONSTRAINT ck_foster_homes_capacity_positive CHECK (capacity > 0),
    CONSTRAINT fk_foster_homes_volunteer_id
        FOREIGN KEY (volunteer_id)
        REFERENCES volunteers (volunteer_id)
        ON UPDATE CASCADE
        ON DELETE RESTRICT
);

CREATE TABLE animal_foster_placements (
    animal_foster_placement_id bigint GENERATED ALWAYS AS IDENTITY,
    animal_id                  bigint NOT NULL,
    foster_home_id             text NOT NULL,
    start_date                 date NOT NULL,
    end_date                   date,
    notes                      text,

    created_at                 timestamptz,
    created_by                 varchar(100),
    modified_at                timestamptz,
    modified_by                varchar(100),

    CONSTRAINT pk_animal_foster_placements PRIMARY KEY (animal_foster_placement_id),
    CONSTRAINT ck_animal_foster_placements_date_range
        CHECK (end_date IS NULL OR end_date >= start_date),
    CONSTRAINT fk_animal_foster_placements_animal_id
        FOREIGN KEY (animal_id)
        REFERENCES animals (animal_id)
        ON UPDATE CASCADE
        ON DELETE RESTRICT,
    CONSTRAINT fk_animal_foster_placements_foster_home_id
        FOREIGN KEY (foster_home_id)
        REFERENCES foster_homes (foster_home_id)
        ON UPDATE CASCADE
        ON DELETE RESTRICT
);

ALTER TABLE rescue_records
    ADD CONSTRAINT fk_rescue_records_foster_home_id
    FOREIGN KEY (foster_home_id)
    REFERENCES foster_homes (foster_home_id)
    ON UPDATE CASCADE
    ON DELETE SET NULL;

-- FK comentada: volunteer_id almacena user_id del localStorage
-- ALTER TABLE rescue_records
--     ADD CONSTRAINT fk_rescue_records_volunteer_id
--     FOREIGN KEY (volunteer_id)
--     REFERENCES volunteers (volunteer_id)
--     ON UPDATE CASCADE
--     ON DELETE SET NULL;



-- ============================================================
-- Indexes for foreign keys and common access patterns
-- ============================================================

CREATE INDEX ix_user_profiles_user_id
    ON user_profiles (user_id);

CREATE INDEX ix_user_contacts_user_id
    ON user_contacts (user_id);

CREATE INDEX ix_user_roles_user_id
    ON user_roles (user_id);

CREATE INDEX ix_user_roles_role_id
    ON user_roles (role_id);

CREATE INDEX ix_animal_intakes_animal_id
    ON animal_intakes (animal_id);

CREATE INDEX ix_animal_intakes_reported_by_user_id
    ON animal_intakes (reported_by_user_id);

CREATE INDEX ix_animal_intakes_intake_at
    ON animal_intakes (intake_at);

CREATE INDEX ix_animal_photos_animal_id
    ON animal_photos (animal_id);

CREATE UNIQUE INDEX ux_animal_photos_one_primary_per_animal
    ON animal_photos (animal_id)
    WHERE is_primary;

CREATE INDEX ix_volunteers_user_id
    ON volunteers (user_id);

CREATE INDEX ix_volunteers_validation_status
    ON volunteers (validation_status);

CREATE INDEX ix_volunteers_validated_by_user_id
    ON volunteers (validated_by_user_id);

CREATE UNIQUE INDEX ux_volunteers_national_id
    ON volunteers (national_id)
    WHERE national_id IS NOT NULL;

CREATE INDEX ix_veterinarians_volunteer_id
    ON veterinarians (volunteer_id);

CREATE INDEX ix_animal_medical_records_animal_id
    ON animal_medical_records (animal_id);

CREATE INDEX ix_animal_medical_records_veterinarian_id
    ON animal_medical_records (veterinarian_id);

CREATE INDEX ix_animal_medical_records_visit_date
    ON animal_medical_records (visit_date);

CREATE INDEX ix_animal_care_schedules_animal_id
    ON animal_care_schedules (animal_id);

CREATE INDEX ix_animal_care_schedules_medical_record_id
    ON animal_care_schedules (animal_medical_record_id);

CREATE INDEX ix_animal_care_schedules_next_due_date
    ON animal_care_schedules (next_due_date);

CREATE INDEX ix_foster_homes_volunteer_id
    ON foster_homes (volunteer_id);

CREATE INDEX ix_rescue_records_date
    ON rescue_records (rescue_date);

CREATE INDEX ix_animal_foster_placements_animal_id
    ON animal_foster_placements (animal_id);

CREATE INDEX ix_animal_foster_placements_foster_home_id
    ON animal_foster_placements (foster_home_id);

CREATE INDEX ix_animal_foster_placements_dates
    ON animal_foster_placements (start_date, end_date);

-- Optional case-insensitive uniqueness without requiring the citext extension.
-- Use these if your business rules require case-insensitive usernames/emails.
-- If enabled, remove or reconsider the plain UNIQUE constraints above.
-- CREATE UNIQUE INDEX ux_users_username_lower ON users (lower(username));
-- CREATE UNIQUE INDEX ux_user_contacts_email_lower ON user_contacts (lower(email));

COMMIT;
