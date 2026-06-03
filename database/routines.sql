BEGIN;

CREATE SCHEMA IF NOT EXISTS ANHELOPETS;
SET search_path TO ANHELOPETS, public;

ALTER TABLE user_profiles
    ADD COLUMN IF NOT EXISTS national_id varchar(50),
    ADD COLUMN IF NOT EXISTS nationality varchar(100);

CREATE UNIQUE INDEX IF NOT EXISTS ux_user_profiles_national_id
    ON user_profiles (national_id)
    WHERE national_id IS NOT NULL;

CREATE TABLE IF NOT EXISTS rescue_records (
    rescue_id      bigint GENERATED ALWAYS AS IDENTITY,
    animal_id      bigint,
    rescue_date    date NOT NULL,
    location       text NOT NULL,
    description    text NOT NULL,
    status         varchar(30) NOT NULL DEFAULT 'Activo',
    foster_home_id bigint,
    created_at     timestamptz,
    created_by     varchar(100),
    modified_at    timestamptz,
    modified_by    varchar(100),
    CONSTRAINT pk_rescue_records PRIMARY KEY (rescue_id),
    CONSTRAINT ck_rescue_records_date_not_future CHECK (rescue_date <= CURRENT_DATE),
    CONSTRAINT ck_rescue_records_status CHECK (status IN ('Activo', 'Cerrado'))
);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'fk_rescue_records_animal_id'
    ) THEN
        ALTER TABLE rescue_records
            ADD CONSTRAINT fk_rescue_records_animal_id
            FOREIGN KEY (animal_id)
            REFERENCES animals (animal_id)
            ON UPDATE CASCADE
            ON DELETE SET NULL;
    END IF;
END;
$$;

ALTER TABLE foster_homes
    ALTER COLUMN volunteer_id DROP NOT NULL,
    ADD COLUMN IF NOT EXISTS name varchar(150),
    ADD COLUMN IF NOT EXISTS address text,
    ADD COLUMN IF NOT EXISTS phone varchar(30),
    ADD COLUMN IF NOT EXISTS responsible varchar(150),
    ADD COLUMN IF NOT EXISTS active boolean NOT NULL DEFAULT true;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'fk_rescue_records_foster_home_id'
    ) THEN
        ALTER TABLE rescue_records
            ADD CONSTRAINT fk_rescue_records_foster_home_id
            FOREIGN KEY (foster_home_id)
            REFERENCES foster_homes (foster_home_id)
            ON UPDATE CASCADE
            ON DELETE SET NULL;
    END IF;
END;
$$;

-- ============================================================
-- Validation helpers
-- ============================================================

CREATE OR REPLACE FUNCTION fn_is_valid_email(p_email text)
RETURNS boolean
LANGUAGE sql
IMMUTABLE
AS $$
    SELECT p_email IS NOT NULL
       AND btrim(p_email) ~* '^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$';
$$;

CREATE OR REPLACE FUNCTION fn_is_valid_phone(p_phone text)
RETURNS boolean
LANGUAGE sql
IMMUTABLE
AS $$
    SELECT p_phone IS NOT NULL
       AND btrim(p_phone) ~ '^\+?[0-9][0-9[:space:]()-]{6,29}$';
$$;

-- ============================================================
-- Users, auth, and password hash storage
-- Password hashing and verification are owned by the .NET API.
-- ============================================================

CREATE OR REPLACE FUNCTION fn_create_user_account(
    p_username varchar(100),
    p_password_hash text,
    p_first_name varchar(100),
    p_middle_name varchar(100),
    p_last_name varchar(100),
    p_second_last_name varchar(100),
    p_birth_date date,
    p_email varchar(255),
    p_phone_primary varchar(30),
    p_phone_secondary varchar(30),
    p_city varchar(100),
    p_town varchar(100),
    p_address_line text,
    p_created_by varchar(100) DEFAULT 'api',
    p_national_id varchar(50) DEFAULT NULL,
    p_nationality varchar(100) DEFAULT NULL
)
RETURNS bigint
LANGUAGE plpgsql
AS $$
DECLARE
    v_user_id bigint;
BEGIN
    IF p_username IS NULL OR btrim(p_username) = '' THEN
        RAISE EXCEPTION 'Username is required';
    END IF;

    IF p_password_hash IS NULL OR btrim(p_password_hash) = '' THEN
        RAISE EXCEPTION 'Password hash is required';
    END IF;

    IF p_first_name IS NULL OR btrim(p_first_name) = '' THEN
        RAISE EXCEPTION 'First name is required';
    END IF;

    IF p_last_name IS NULL OR btrim(p_last_name) = '' THEN
        RAISE EXCEPTION 'Last name is required';
    END IF;

    IF p_birth_date IS NULL OR p_birth_date > CURRENT_DATE THEN
        RAISE EXCEPTION 'Birth date is required and cannot be in the future';
    END IF;

    IF NOT fn_is_valid_email(p_email) THEN
        RAISE EXCEPTION 'Email is invalid';
    END IF;

    IF NOT fn_is_valid_phone(p_phone_primary) THEN
        RAISE EXCEPTION 'Primary phone is invalid';
    END IF;

    IF p_phone_secondary IS NOT NULL AND btrim(p_phone_secondary) <> ''
       AND NOT fn_is_valid_phone(p_phone_secondary) THEN
        RAISE EXCEPTION 'Secondary phone is invalid';
    END IF;

    IF p_city IS NULL OR btrim(p_city) = '' THEN
        RAISE EXCEPTION 'City is required';
    END IF;

    IF p_town IS NULL OR btrim(p_town) = '' THEN
        RAISE EXCEPTION 'Town is required';
    END IF;

    IF p_address_line IS NULL OR btrim(p_address_line) = '' THEN
        RAISE EXCEPTION 'Address is required';
    END IF;

    IF p_national_id IS NOT NULL AND btrim(p_national_id) = '' THEN
        RAISE EXCEPTION 'National id cannot be empty';
    END IF;

    INSERT INTO users (username, password_hash, created_at, created_by)
    VALUES (btrim(p_username), p_password_hash, now(), COALESCE(NULLIF(btrim(p_created_by), ''), 'api'))
    RETURNING user_id INTO v_user_id;

    INSERT INTO user_profiles (
        user_id, national_id, first_name, middle_name, last_name, second_last_name, birth_date, nationality,
        created_at, created_by
    )
    VALUES (
        v_user_id, NULLIF(btrim(p_national_id), ''), btrim(p_first_name), NULLIF(btrim(p_middle_name), ''),
        btrim(p_last_name), NULLIF(btrim(p_second_last_name), ''), p_birth_date, NULLIF(btrim(p_nationality), ''),
        now(), COALESCE(NULLIF(btrim(p_created_by), ''), 'api')
    );

    INSERT INTO user_contacts (
        user_id, email, phone_primary, phone_secondary, city, town, address_line,
        created_at, created_by
    )
    VALUES (
        v_user_id, lower(btrim(p_email)), btrim(p_phone_primary), NULLIF(btrim(p_phone_secondary), ''),
        btrim(p_city), btrim(p_town), btrim(p_address_line),
        now(), COALESCE(NULLIF(btrim(p_created_by), ''), 'api')
    );

    RETURN v_user_id;
END;
$$;

CREATE OR REPLACE FUNCTION fn_get_auth_user(p_username_or_email text)
RETURNS TABLE (
    user_id bigint,
    username varchar(100),
    email varchar(255),
    password_hash text,
    first_name varchar(100),
    last_name varchar(100),
    is_volunteer boolean,
    volunteer_active boolean,
    volunteer_validation_status varchar(20),
    roles text[]
)
LANGUAGE sql
STABLE
AS $$
    SELECT
        u.user_id,
        u.username,
        uc.email,
        u.password_hash,
        up.first_name,
        up.last_name,
        (v.volunteer_id IS NOT NULL) AS is_volunteer,
        COALESCE(v.active, false) AS volunteer_active,
        v.validation_status AS volunteer_validation_status,
        COALESCE(array_agg(DISTINCT r.role_name) FILTER (WHERE r.role_name IS NOT NULL), ARRAY[]::text[]) AS roles
    FROM users u
    JOIN user_profiles up ON up.user_id = u.user_id
    JOIN user_contacts uc ON uc.user_id = u.user_id
    LEFT JOIN volunteers v ON v.user_id = u.user_id
    LEFT JOIN user_roles ur ON ur.user_id = u.user_id
    LEFT JOIN roles r ON r.role_id = ur.role_id
    WHERE lower(u.username) = lower(btrim(p_username_or_email))
       OR lower(uc.email) = lower(btrim(p_username_or_email))
    GROUP BY
        u.user_id, u.username, uc.email, u.password_hash, up.first_name, up.last_name,
        v.volunteer_id, v.active, v.validation_status;
$$;

CREATE OR REPLACE FUNCTION fn_update_password_hash(
    p_user_id bigint,
    p_password_hash text,
    p_modified_by varchar(100) DEFAULT 'api'
)
RETURNS void
LANGUAGE plpgsql
AS $$
BEGIN
    IF p_user_id IS NULL THEN
        RAISE EXCEPTION 'User id is required';
    END IF;

    IF p_password_hash IS NULL OR btrim(p_password_hash) = '' THEN
        RAISE EXCEPTION 'Password hash is required';
    END IF;

    UPDATE users
    SET password_hash = p_password_hash,
        modified_at = now(),
        modified_by = COALESCE(NULLIF(btrim(p_modified_by), ''), 'api')
    WHERE user_id = p_user_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'User % does not exist', p_user_id;
    END IF;
END;
$$;

-- ============================================================
-- Volunteers and administrative validation
-- ============================================================

CREATE OR REPLACE FUNCTION fn_register_volunteer(
    p_user_id bigint,
    p_national_id varchar(50),
    p_volunteer_type varchar(100),
    p_motivation text DEFAULT NULL,
    p_created_by varchar(100) DEFAULT 'api'
)
RETURNS bigint
LANGUAGE plpgsql
AS $$
DECLARE
    v_volunteer_id bigint;
BEGIN
    IF p_user_id IS NULL THEN
        RAISE EXCEPTION 'User id is required';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM users WHERE users.user_id = p_user_id) THEN
        RAISE EXCEPTION 'User % does not exist', p_user_id;
    END IF;

    IF p_national_id IS NULL OR btrim(p_national_id) = '' THEN
        RAISE EXCEPTION 'National id is required';
    END IF;

    IF p_volunteer_type IS NULL OR btrim(p_volunteer_type) = '' THEN
        RAISE EXCEPTION 'Volunteer type is required';
    END IF;

    INSERT INTO volunteers (
        user_id, active, national_id, volunteer_type, motivation, validation_status,
        created_at, created_by
    )
    VALUES (
        p_user_id, false, btrim(p_national_id), btrim(p_volunteer_type), NULLIF(btrim(p_motivation), ''),
        'Pendiente', now(), COALESCE(NULLIF(btrim(p_created_by), ''), 'api')
    )
    ON CONFLICT (user_id) DO UPDATE
    SET national_id = EXCLUDED.national_id,
        volunteer_type = EXCLUDED.volunteer_type,
        motivation = EXCLUDED.motivation,
        validation_status = 'Pendiente',
        validation_notes = NULL,
        validated_at = NULL,
        validated_by_user_id = NULL,
        active = false,
        modified_at = now(),
        modified_by = COALESCE(NULLIF(btrim(p_created_by), ''), 'api')
    RETURNING volunteer_id INTO v_volunteer_id;

    RETURN v_volunteer_id;
END;
$$;

CREATE OR REPLACE FUNCTION fn_validate_volunteer(
    p_volunteer_id bigint,
    p_validation_status varchar(20),
    p_validated_by_user_id bigint,
    p_validation_notes text DEFAULT NULL,
    p_modified_by varchar(100) DEFAULT 'api'
)
RETURNS void
LANGUAGE plpgsql
AS $$
BEGIN
    IF p_volunteer_id IS NULL THEN
        RAISE EXCEPTION 'Volunteer id is required';
    END IF;

    IF p_validation_status NOT IN ('Pendiente', 'Aprobado', 'Rechazado') THEN
        RAISE EXCEPTION 'Volunteer validation status is invalid';
    END IF;

    IF p_validated_by_user_id IS NULL
       OR NOT EXISTS (SELECT 1 FROM users WHERE users.user_id = p_validated_by_user_id) THEN
        RAISE EXCEPTION 'Validator user is required and must exist';
    END IF;

    UPDATE volunteers
    SET validation_status = p_validation_status,
        validation_notes = NULLIF(btrim(p_validation_notes), ''),
        validated_at = CASE WHEN p_validation_status = 'Pendiente' THEN NULL ELSE now() END,
        validated_by_user_id = CASE WHEN p_validation_status = 'Pendiente' THEN NULL ELSE p_validated_by_user_id END,
        active = (p_validation_status = 'Aprobado'),
        modified_at = now(),
        modified_by = COALESCE(NULLIF(btrim(p_modified_by), ''), 'api')
    WHERE volunteer_id = p_volunteer_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Volunteer % does not exist', p_volunteer_id;
    END IF;
END;
$$;

CREATE OR REPLACE FUNCTION fn_set_volunteer_active(
    p_volunteer_id bigint,
    p_active boolean,
    p_modified_by varchar(100) DEFAULT 'api'
)
RETURNS void
LANGUAGE plpgsql
AS $$
BEGIN
    IF p_volunteer_id IS NULL THEN
        RAISE EXCEPTION 'Volunteer id is required';
    END IF;

    IF p_active IS NULL THEN
        RAISE EXCEPTION 'Active flag is required';
    END IF;

    UPDATE volunteers
    SET active = p_active,
        modified_at = now(),
        modified_by = COALESCE(NULLIF(btrim(p_modified_by), ''), 'api')
    WHERE volunteer_id = p_volunteer_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Volunteer % does not exist', p_volunteer_id;
    END IF;
END;
$$;

CREATE OR REPLACE FUNCTION fn_get_volunteers_admin()
RETURNS TABLE (
    volunteer_id bigint,
    user_id bigint,
    full_name text,
    national_id varchar(50),
    volunteer_type varchar(100),
    motivation text,
    email varchar(255),
    phone_primary varchar(30),
    city varchar(100),
    town varchar(100),
    active boolean,
    validation_status varchar(20),
    validation_notes text,
    validated_at timestamptz,
    validated_by_user_id bigint
)
LANGUAGE sql
STABLE
AS $$
    SELECT
        v.volunteer_id,
        u.user_id,
        concat_ws(' ', up.first_name, up.middle_name, up.last_name, up.second_last_name) AS full_name,
        v.national_id,
        v.volunteer_type,
        v.motivation,
        uc.email,
        uc.phone_primary,
        uc.city,
        uc.town,
        v.active,
        v.validation_status,
        v.validation_notes,
        v.validated_at,
        v.validated_by_user_id
    FROM volunteers v
    JOIN users u ON u.user_id = v.user_id
    JOIN user_profiles up ON up.user_id = u.user_id
    JOIN user_contacts uc ON uc.user_id = u.user_id
    ORDER BY v.created_at DESC NULLS LAST, v.volunteer_id DESC;
$$;

-- ============================================================
-- Animals, photos, and public catalog
-- ============================================================

CREATE OR REPLACE FUNCTION fn_create_animal(
    p_species varchar(100),
    p_breed varchar(100),
    p_animal_name varchar(100),
    p_animal_status varchar(50),
    p_health_status varchar(50),
    p_birth_date date DEFAULT NULL,
    p_sex varchar(20) DEFAULT NULL,
    p_description text DEFAULT NULL,
    p_primary_photo_url text DEFAULT NULL,
    p_primary_photo_description text DEFAULT NULL,
    p_created_by varchar(100) DEFAULT 'api'
)
RETURNS bigint
LANGUAGE plpgsql
AS $$
DECLARE
    v_animal_id bigint;
    v_created_by varchar(100) := COALESCE(NULLIF(btrim(p_created_by), ''), 'api');
BEGIN
    IF p_species IS NULL OR btrim(p_species) NOT IN ('Perro', 'Gato') THEN
        RAISE EXCEPTION 'Animal species is invalid';
    END IF;

    IF p_animal_name IS NULL OR btrim(p_animal_name) = '' THEN
        RAISE EXCEPTION 'Animal name is required';
    END IF;

    IF p_animal_status IS NULL OR btrim(p_animal_status) NOT IN ('Disponible', 'En proceso', 'Adoptada') THEN
        RAISE EXCEPTION 'Animal status is invalid';
    END IF;

    IF p_health_status IS NULL OR btrim(p_health_status) = '' THEN
        RAISE EXCEPTION 'Health status is required';
    END IF;

    IF p_birth_date IS NOT NULL AND p_birth_date > CURRENT_DATE THEN
        RAISE EXCEPTION 'Animal birth date cannot be in the future';
    END IF;

    IF p_sex IS NOT NULL AND btrim(p_sex) <> '' AND btrim(p_sex) NOT IN ('Macho', 'Hembra') THEN
        RAISE EXCEPTION 'Animal sex is invalid';
    END IF;

    INSERT INTO animals (
        species, breed, animal_name, animal_status, health_status, birth_date, sex, description,
        created_at, created_by
    )
    VALUES (
        btrim(p_species), NULLIF(btrim(p_breed), ''), btrim(p_animal_name), btrim(p_animal_status),
        btrim(p_health_status), p_birth_date, NULLIF(btrim(p_sex), ''), NULLIF(btrim(p_description), ''),
        now(), v_created_by
    )
    RETURNING animal_id INTO v_animal_id;

    IF p_primary_photo_url IS NOT NULL AND btrim(p_primary_photo_url) <> '' THEN
        INSERT INTO animal_photos (
            animal_id, photo_url, description, is_primary, display_order,
            created_at, created_by
        )
        VALUES (
            v_animal_id, btrim(p_primary_photo_url), NULLIF(btrim(p_primary_photo_description), ''),
            true, 0, now(), v_created_by
        );
    END IF;

    RETURN v_animal_id;
END;
$$;

CREATE OR REPLACE FUNCTION fn_update_animal(
    p_animal_id bigint,
    p_species varchar(100),
    p_breed varchar(100),
    p_animal_name varchar(100),
    p_animal_status varchar(50),
    p_health_status varchar(50),
    p_birth_date date DEFAULT NULL,
    p_sex varchar(20) DEFAULT NULL,
    p_description text DEFAULT NULL,
    p_modified_by varchar(100) DEFAULT 'api'
)
RETURNS void
LANGUAGE plpgsql
AS $$
BEGIN
    IF p_animal_id IS NULL THEN
        RAISE EXCEPTION 'Animal id is required';
    END IF;

    IF p_species IS NULL OR btrim(p_species) NOT IN ('Perro', 'Gato') THEN
        RAISE EXCEPTION 'Animal species is invalid';
    END IF;

    IF p_animal_name IS NULL OR btrim(p_animal_name) = '' THEN
        RAISE EXCEPTION 'Animal name is required';
    END IF;

    IF p_animal_status IS NULL OR btrim(p_animal_status) NOT IN ('Disponible', 'En proceso', 'Adoptada') THEN
        RAISE EXCEPTION 'Animal status is invalid';
    END IF;

    IF p_health_status IS NULL OR btrim(p_health_status) = '' THEN
        RAISE EXCEPTION 'Health status is required';
    END IF;

    IF p_birth_date IS NOT NULL AND p_birth_date > CURRENT_DATE THEN
        RAISE EXCEPTION 'Animal birth date cannot be in the future';
    END IF;

    IF p_sex IS NOT NULL AND btrim(p_sex) <> '' AND btrim(p_sex) NOT IN ('Macho', 'Hembra') THEN
        RAISE EXCEPTION 'Animal sex is invalid';
    END IF;

    UPDATE animals
    SET species = btrim(p_species),
        breed = NULLIF(btrim(p_breed), ''),
        animal_name = btrim(p_animal_name),
        animal_status = btrim(p_animal_status),
        health_status = btrim(p_health_status),
        birth_date = p_birth_date,
        sex = NULLIF(btrim(p_sex), ''),
        description = NULLIF(btrim(p_description), ''),
        modified_at = now(),
        modified_by = COALESCE(NULLIF(btrim(p_modified_by), ''), 'api')
    WHERE animal_id = p_animal_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Animal % does not exist', p_animal_id;
    END IF;
END;
$$;

CREATE OR REPLACE FUNCTION fn_add_animal_photo(
    p_animal_id bigint,
    p_photo_url text,
    p_description text DEFAULT NULL,
    p_is_primary boolean DEFAULT false,
    p_display_order integer DEFAULT 0,
    p_created_by varchar(100) DEFAULT 'api'
)
RETURNS bigint
LANGUAGE plpgsql
AS $$
DECLARE
    v_animal_photo_id bigint;
    v_created_by varchar(100) := COALESCE(NULLIF(btrim(p_created_by), ''), 'api');
BEGIN
    IF p_animal_id IS NULL OR NOT EXISTS (SELECT 1 FROM animals WHERE animals.animal_id = p_animal_id) THEN
        RAISE EXCEPTION 'Animal is required and must exist';
    END IF;

    IF p_photo_url IS NULL OR btrim(p_photo_url) = '' THEN
        RAISE EXCEPTION 'Photo URL is required';
    END IF;

    IF p_display_order IS NULL OR p_display_order < 0 THEN
        RAISE EXCEPTION 'Display order must be zero or greater';
    END IF;

    IF COALESCE(p_is_primary, false) THEN
        UPDATE animal_photos
        SET is_primary = false,
            modified_at = now(),
            modified_by = v_created_by
        WHERE animal_id = p_animal_id
          AND is_primary;
    END IF;

    INSERT INTO animal_photos (
        animal_id, photo_url, description, is_primary, display_order,
        created_at, created_by
    )
    VALUES (
        p_animal_id, btrim(p_photo_url), NULLIF(btrim(p_description), ''),
        COALESCE(p_is_primary, false), p_display_order, now(), v_created_by
    )
    RETURNING animal_photo_id INTO v_animal_photo_id;

    RETURN v_animal_photo_id;
END;
$$;

CREATE OR REPLACE FUNCTION fn_set_primary_animal_photo(
    p_animal_photo_id bigint,
    p_modified_by varchar(100) DEFAULT 'api'
)
RETURNS void
LANGUAGE plpgsql
AS $$
DECLARE
    v_animal_id bigint;
    v_modified_by varchar(100) := COALESCE(NULLIF(btrim(p_modified_by), ''), 'api');
BEGIN
    SELECT animal_id
    INTO v_animal_id
    FROM animal_photos
    WHERE animal_photo_id = p_animal_photo_id;

    IF v_animal_id IS NULL THEN
        RAISE EXCEPTION 'Animal photo % does not exist', p_animal_photo_id;
    END IF;

    UPDATE animal_photos
    SET is_primary = false,
        modified_at = now(),
        modified_by = v_modified_by
    WHERE animal_id = v_animal_id
      AND is_primary;

    UPDATE animal_photos
    SET is_primary = true,
        modified_at = now(),
        modified_by = v_modified_by
    WHERE animal_photo_id = p_animal_photo_id;
END;
$$;

CREATE OR REPLACE FUNCTION fn_get_pet_catalog(
    p_species text DEFAULT NULL,
    p_status text DEFAULT 'Disponible',
    p_search text DEFAULT NULL
)
RETURNS TABLE (
    animal_id bigint,
    animal_name varchar(100),
    species varchar(100),
    breed varchar(100),
    birth_date date,
    age_years integer,
    age_months integer,
    sex varchar(20),
    animal_status varchar(50),
    health_status varchar(50),
    description text,
    photo_url text
)
LANGUAGE sql
STABLE
AS $$
    SELECT
        a.animal_id,
        a.animal_name,
        a.species,
        a.breed,
        a.birth_date,
        CASE
            WHEN a.birth_date IS NULL THEN NULL
            ELSE date_part('year', age(CURRENT_DATE, a.birth_date))::integer
        END AS age_years,
        CASE
            WHEN a.birth_date IS NULL THEN NULL
            ELSE (
                date_part('year', age(CURRENT_DATE, a.birth_date))::integer * 12
                + date_part('month', age(CURRENT_DATE, a.birth_date))::integer
            )
        END AS age_months,
        a.sex,
        a.animal_status,
        a.health_status,
        a.description,
        ap.photo_url
    FROM animals a
    LEFT JOIN LATERAL (
        SELECT animal_photos.photo_url
        FROM animal_photos
        WHERE animal_photos.animal_id = a.animal_id
        ORDER BY animal_photos.is_primary DESC, animal_photos.display_order, animal_photos.animal_photo_id
        LIMIT 1
    ) ap ON true
    WHERE (p_species IS NULL OR btrim(p_species) = '' OR a.species = btrim(p_species))
      AND (p_status IS NULL OR btrim(p_status) = '' OR p_status = 'Todos' OR a.animal_status = btrim(p_status))
      AND (
          p_search IS NULL OR btrim(p_search) = ''
          OR a.animal_name ILIKE '%' || btrim(p_search) || '%'
          OR a.breed ILIKE '%' || btrim(p_search) || '%'
      )
    ORDER BY a.created_at DESC NULLS LAST, a.animal_id DESC;
$$;

CREATE OR REPLACE FUNCTION fn_get_rescues_admin()
RETURNS TABLE (
    rescue_id bigint,
    animal_id bigint,
    animal_name varchar(100),
    rescue_date date,
    location text,
    description text,
    status varchar(30),
    foster_home_id bigint,
    foster_home_name varchar(150)
)
LANGUAGE sql
STABLE
AS $$
    SELECT
        r.rescue_id,
        r.animal_id,
        a.animal_name,
        r.rescue_date,
        r.location,
        r.description,
        r.status,
        r.foster_home_id,
        fh.name AS foster_home_name
    FROM rescue_records r
    LEFT JOIN animals a ON a.animal_id = r.animal_id
    LEFT JOIN foster_homes fh ON fh.foster_home_id = r.foster_home_id
    ORDER BY r.rescue_date DESC, r.rescue_id DESC;
$$;

CREATE OR REPLACE FUNCTION fn_create_rescue(
    p_animal_id bigint,
    p_rescue_date date,
    p_location text,
    p_description text,
    p_status varchar(30) DEFAULT 'Activo',
    p_foster_home_id bigint DEFAULT NULL,
    p_created_by varchar(100) DEFAULT 'api'
)
RETURNS bigint
LANGUAGE plpgsql
AS $$
DECLARE
    v_rescue_id bigint;
    v_created_by varchar(100) := COALESCE(NULLIF(btrim(p_created_by), ''), 'api');
BEGIN
    IF p_animal_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM animals WHERE animal_id = p_animal_id) THEN
        RAISE EXCEPTION 'Animal % does not exist', p_animal_id;
    END IF;

    IF p_rescue_date IS NULL OR p_rescue_date > CURRENT_DATE THEN
        RAISE EXCEPTION 'Rescue date is required and cannot be in the future';
    END IF;

    IF p_location IS NULL OR btrim(p_location) = '' THEN
        RAISE EXCEPTION 'Rescue location is required';
    END IF;

    IF p_description IS NULL OR btrim(p_description) = '' THEN
        RAISE EXCEPTION 'Rescue description is required';
    END IF;

    IF p_status IS NULL OR btrim(p_status) NOT IN ('Activo', 'Cerrado') THEN
        RAISE EXCEPTION 'Rescue status is invalid';
    END IF;

    IF p_foster_home_id IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM foster_homes WHERE foster_home_id = p_foster_home_id) THEN
        RAISE EXCEPTION 'Foster home % does not exist', p_foster_home_id;
    END IF;

    INSERT INTO rescue_records (
        animal_id, rescue_date, location, description, status, foster_home_id,
        created_at, created_by
    )
    VALUES (
        p_animal_id, p_rescue_date, btrim(p_location), btrim(p_description),
        btrim(p_status), p_foster_home_id, now(), v_created_by
    )
    RETURNING rescue_id INTO v_rescue_id;

    RETURN v_rescue_id;
END;
$$;

CREATE OR REPLACE FUNCTION fn_update_rescue(
    p_rescue_id bigint,
    p_animal_id bigint,
    p_rescue_date date,
    p_location text,
    p_description text,
    p_status varchar(30),
    p_foster_home_id bigint DEFAULT NULL,
    p_modified_by varchar(100) DEFAULT 'api'
)
RETURNS void
LANGUAGE plpgsql
AS $$
DECLARE
    v_modified_by varchar(100) := COALESCE(NULLIF(btrim(p_modified_by), ''), 'api');
BEGIN
    IF p_rescue_id IS NULL THEN
        RAISE EXCEPTION 'Rescue id is required';
    END IF;

    IF p_animal_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM animals WHERE animal_id = p_animal_id) THEN
        RAISE EXCEPTION 'Animal % does not exist', p_animal_id;
    END IF;

    IF p_rescue_date IS NULL OR p_rescue_date > CURRENT_DATE THEN
        RAISE EXCEPTION 'Rescue date is required and cannot be in the future';
    END IF;

    IF p_location IS NULL OR btrim(p_location) = '' THEN
        RAISE EXCEPTION 'Rescue location is required';
    END IF;

    IF p_description IS NULL OR btrim(p_description) = '' THEN
        RAISE EXCEPTION 'Rescue description is required';
    END IF;

    IF p_status IS NULL OR btrim(p_status) NOT IN ('Activo', 'Cerrado') THEN
        RAISE EXCEPTION 'Rescue status is invalid';
    END IF;

    IF p_foster_home_id IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM foster_homes WHERE foster_home_id = p_foster_home_id) THEN
        RAISE EXCEPTION 'Foster home % does not exist', p_foster_home_id;
    END IF;

    UPDATE rescue_records
    SET animal_id = p_animal_id,
        rescue_date = p_rescue_date,
        location = btrim(p_location),
        description = btrim(p_description),
        status = btrim(p_status),
        foster_home_id = p_foster_home_id,
        modified_at = now(),
        modified_by = v_modified_by
    WHERE rescue_id = p_rescue_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Rescue % does not exist', p_rescue_id;
    END IF;
END;
$$;

CREATE OR REPLACE FUNCTION fn_get_foster_homes_admin()
RETURNS TABLE (
    foster_home_id bigint,
    volunteer_id bigint,
    name varchar(150),
    address text,
    phone varchar(30),
    responsible varchar(150),
    capacity integer,
    active boolean
)
LANGUAGE sql
STABLE
AS $$
    SELECT
        foster_home_id,
        volunteer_id,
        name,
        address,
        phone,
        responsible,
        capacity,
        active
    FROM foster_homes
    ORDER BY active DESC, name;
$$;

CREATE OR REPLACE FUNCTION fn_create_foster_home(
    p_volunteer_id bigint,
    p_name varchar(150),
    p_address text,
    p_phone varchar(30),
    p_responsible varchar(150),
    p_capacity integer,
    p_created_by varchar(100) DEFAULT 'api'
)
RETURNS bigint
LANGUAGE plpgsql
AS $$
DECLARE
    v_foster_home_id bigint;
    v_created_by varchar(100) := COALESCE(NULLIF(btrim(p_created_by), ''), 'api');
BEGIN
    IF p_volunteer_id IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM volunteers WHERE volunteer_id = p_volunteer_id) THEN
        RAISE EXCEPTION 'Volunteer % does not exist', p_volunteer_id;
    END IF;

    IF p_name IS NULL OR btrim(p_name) = '' THEN
        RAISE EXCEPTION 'Foster home name is required';
    END IF;

    IF p_address IS NULL OR btrim(p_address) = '' THEN
        RAISE EXCEPTION 'Foster home address is required';
    END IF;

    IF NOT fn_is_valid_phone(p_phone) THEN
        RAISE EXCEPTION 'Foster home phone is invalid';
    END IF;

    IF p_responsible IS NULL OR btrim(p_responsible) = '' THEN
        RAISE EXCEPTION 'Foster home responsible person is required';
    END IF;

    IF p_capacity IS NULL OR p_capacity <= 0 THEN
        RAISE EXCEPTION 'Foster home capacity must be greater than zero';
    END IF;

    INSERT INTO foster_homes (
        volunteer_id, name, address, phone, responsible, capacity, active,
        created_at, created_by
    )
    VALUES (
        p_volunteer_id, btrim(p_name), btrim(p_address), btrim(p_phone),
        btrim(p_responsible), p_capacity, true, now(), v_created_by
    )
    RETURNING foster_home_id INTO v_foster_home_id;

    RETURN v_foster_home_id;
END;
$$;

CREATE OR REPLACE FUNCTION fn_update_foster_home(
    p_foster_home_id bigint,
    p_volunteer_id bigint,
    p_name varchar(150),
    p_address text,
    p_phone varchar(30),
    p_responsible varchar(150),
    p_capacity integer,
    p_active boolean,
    p_modified_by varchar(100) DEFAULT 'api'
)
RETURNS void
LANGUAGE plpgsql
AS $$
DECLARE
    v_modified_by varchar(100) := COALESCE(NULLIF(btrim(p_modified_by), ''), 'api');
BEGIN
    IF p_foster_home_id IS NULL THEN
        RAISE EXCEPTION 'Foster home id is required';
    END IF;

    IF p_volunteer_id IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM volunteers WHERE volunteer_id = p_volunteer_id) THEN
        RAISE EXCEPTION 'Volunteer % does not exist', p_volunteer_id;
    END IF;

    IF p_name IS NULL OR btrim(p_name) = '' THEN
        RAISE EXCEPTION 'Foster home name is required';
    END IF;

    IF p_address IS NULL OR btrim(p_address) = '' THEN
        RAISE EXCEPTION 'Foster home address is required';
    END IF;

    IF NOT fn_is_valid_phone(p_phone) THEN
        RAISE EXCEPTION 'Foster home phone is invalid';
    END IF;

    IF p_responsible IS NULL OR btrim(p_responsible) = '' THEN
        RAISE EXCEPTION 'Foster home responsible person is required';
    END IF;

    IF p_capacity IS NULL OR p_capacity <= 0 THEN
        RAISE EXCEPTION 'Foster home capacity must be greater than zero';
    END IF;

    UPDATE foster_homes
    SET volunteer_id = p_volunteer_id,
        name = btrim(p_name),
        address = btrim(p_address),
        phone = btrim(p_phone),
        responsible = btrim(p_responsible),
        capacity = p_capacity,
        active = COALESCE(p_active, true),
        modified_at = now(),
        modified_by = v_modified_by
    WHERE foster_home_id = p_foster_home_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Foster home % does not exist', p_foster_home_id;
    END IF;
END;
$$;

CREATE OR REPLACE FUNCTION fn_get_foster_placements_admin()
RETURNS TABLE (
    animal_foster_placement_id bigint,
    animal_id bigint,
    animal_name varchar(100),
    foster_home_id bigint,
    foster_home_name varchar(150),
    start_date date,
    end_date date,
    notes text
)
LANGUAGE sql
STABLE
AS $$
    SELECT
        afp.animal_foster_placement_id,
        afp.animal_id,
        a.animal_name,
        afp.foster_home_id,
        fh.name AS foster_home_name,
        afp.start_date,
        afp.end_date,
        afp.notes
    FROM animal_foster_placements afp
    JOIN animals a ON a.animal_id = afp.animal_id
    JOIN foster_homes fh ON fh.foster_home_id = afp.foster_home_id
    ORDER BY afp.start_date DESC, afp.animal_foster_placement_id DESC;
$$;

CREATE OR REPLACE FUNCTION fn_assign_animal_foster_home(
    p_animal_id bigint,
    p_foster_home_id bigint,
    p_start_date date,
    p_end_date date DEFAULT NULL,
    p_notes text DEFAULT NULL,
    p_created_by varchar(100) DEFAULT 'api'
)
RETURNS bigint
LANGUAGE plpgsql
AS $$
DECLARE
    v_placement_id bigint;
    v_created_by varchar(100) := COALESCE(NULLIF(btrim(p_created_by), ''), 'api');
BEGIN
    IF p_animal_id IS NULL OR NOT EXISTS (SELECT 1 FROM animals WHERE animal_id = p_animal_id) THEN
        RAISE EXCEPTION 'Animal is required and must exist';
    END IF;

    IF p_foster_home_id IS NULL OR NOT EXISTS (
        SELECT 1 FROM foster_homes WHERE foster_home_id = p_foster_home_id AND active
    ) THEN
        RAISE EXCEPTION 'Active foster home is required and must exist';
    END IF;

    IF p_start_date IS NULL THEN
        RAISE EXCEPTION 'Start date is required';
    END IF;

    IF p_end_date IS NOT NULL AND p_end_date < p_start_date THEN
        RAISE EXCEPTION 'End date cannot be before start date';
    END IF;

    UPDATE animal_foster_placements
    SET end_date = p_start_date,
        modified_at = now(),
        modified_by = v_created_by
    WHERE animal_id = p_animal_id
      AND end_date IS NULL;

    INSERT INTO animal_foster_placements (
        animal_id, foster_home_id, start_date, end_date, notes,
        created_at, created_by
    )
    VALUES (
        p_animal_id, p_foster_home_id, p_start_date, p_end_date,
        NULLIF(btrim(p_notes), ''), now(), v_created_by
    )
    RETURNING animal_foster_placement_id INTO v_placement_id;

    RETURN v_placement_id;
END;
$$;

CREATE OR REPLACE FUNCTION fn_update_foster_placement(
    p_animal_foster_placement_id bigint,
    p_animal_id bigint,
    p_foster_home_id bigint,
    p_start_date date,
    p_end_date date DEFAULT NULL,
    p_notes text DEFAULT NULL,
    p_modified_by varchar(100) DEFAULT 'api'
)
RETURNS void
LANGUAGE plpgsql
AS $$
DECLARE
    v_modified_by varchar(100) := COALESCE(NULLIF(btrim(p_modified_by), ''), 'api');
BEGIN
    IF p_animal_foster_placement_id IS NULL THEN
        RAISE EXCEPTION 'Placement id is required';
    END IF;

    IF p_animal_id IS NULL OR NOT EXISTS (SELECT 1 FROM animals WHERE animal_id = p_animal_id) THEN
        RAISE EXCEPTION 'Animal is required and must exist';
    END IF;

    IF p_foster_home_id IS NULL OR NOT EXISTS (SELECT 1 FROM foster_homes WHERE foster_home_id = p_foster_home_id) THEN
        RAISE EXCEPTION 'Foster home is required and must exist';
    END IF;

    IF p_start_date IS NULL THEN
        RAISE EXCEPTION 'Start date is required';
    END IF;

    IF p_end_date IS NOT NULL AND p_end_date < p_start_date THEN
        RAISE EXCEPTION 'End date cannot be before start date';
    END IF;

    UPDATE animal_foster_placements
    SET animal_id = p_animal_id,
        foster_home_id = p_foster_home_id,
        start_date = p_start_date,
        end_date = p_end_date,
        notes = NULLIF(btrim(p_notes), ''),
        modified_at = now(),
        modified_by = v_modified_by
    WHERE animal_foster_placement_id = p_animal_foster_placement_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Foster placement % does not exist', p_animal_foster_placement_id;
    END IF;
END;
$$;

COMMIT;
