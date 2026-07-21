# Proyecto AnheloPets - Contexto Completo

## Resumen del Proyecto
Plataforma web de gestión para fundación/animal shelter "Anhelo Pets". Stack: Vue 3 (frontend) + .NET 10 (backend) + PostgreSQL.

## Estado Actual (Jul 2026)

### Backend - AnheloPets.API
- **Compila**: ✅ 0 warnings, 0 errors
- **Tabla usada**: `animal_medical_records` (existente, NO crear tablas nuevas)
- **Entidad**: `AnimalMedicalRecord` → `animal_medical_records`
- **DTO**: `AnimalMedicalRecordDto` (animalMedicalRecordId, animalId, veterinarianId, diagnosis, treatment, notes, visitDate)
- **Controller**: `MedicalRecordsController` → `/api/MedicalRecords`
- **Endpoints**:
  - `GET /api/MedicalRecords` - Listar
  - `POST /api/MedicalRecords` - Crear
  - `GET /api/Animals` - Listar mascotas (para selector)

### Frontend - Vista SaludAdminView.vue
**Archivo**: `frontend/src/views/admin/SaludAdminView.vue` (1760 líneas)

#### Funcionalidades implementadas:
1. **Wizard por pasos** (3 pasos):
   - Paso 1: Tipo de registro (Historial/Vacuna/Tratamiento) + Mascota + Veterinario
   - Paso 2: Formulario dinámico según tipo
   - Paso 3: Confirmación

2. **Carga de mascotas desde API** (`getAnimals()` → `animales` ref, llamado en `onMounted`)

3. **Selector de mascota con "Agregar mascota" inline**:
   - Botón "+ Agregar mascota" junto al dropdown
   - Formulario inline: Nombre, Especie (Perro/Gato/Otro), Sexo (Macho/Hembra)
   - Llama a `createAnimals()` → recarga lista → auto-selecciona

4. **Serialización de tipo de registro**:
   - Backend no tiene campo `recordType`
   - Se guarda prefijo `[H]`/`[V]`/`[T]` en campo `treatment` + datos extra separados por `|`

5. **Persistencia**: localStorage key `anhelo_health_records` + llamada API

### Estructura de Datos Backend (animal_medical_records)
```sql
animal_medical_record_id (bigserial PK)
animal_id (bigint FK → animals.animal_id)
veterinarian_id (bigint FK → veterinarians.veterinarian_id, NOT NULL)
diagnosis (text)
treatment (text)  -- aquí va [H]|Dr.Pérez|Clínica|12.5 etc
notes (text)
visit_date (date)
```

### Puntos Críticos / Technical Debt
1. `veterinarian_id` se envía como `0` (FK NOT NULL en BD → posible error si no existe veterinario 0)
2. `animal_id` es `bigint` en BD pero frontend usa strings tipo `ANM-001` → conversión implícita
3. `cargarDatos()` usa localStorage, NO la API (`getHealthRecords()` no se usa)
4. No hay campo `recordType` en BD → lógica de prefijos frágil
5. Falta `btn-secondary` en CSS (se usa `btn-cancel` como secundario)

### Comandos Útiles
```bash
# Backend build
dotnet build Backend/AnheloPets.API

# Frontend dev
cd frontend && npm run dev

# Frontend build
cd frontend && npm run build
```

### Archivos Clave Modificados Recientemente
- `Backend/AnheloPets.API/Models/HealthRecord.cs` → `AnimalMedicalRecord`
- `Backend/AnheloPets.API/Dtos/HealthRecordDto.cs` → `AnimalMedicalRecordDto`
- `Backend/AnheloPets.API/Controllers/HealthRecordsController.cs` → `MedicalRecordsController`
- `Backend/AnheloPets.API/Data/AnheloPetsDbContext.cs`
- `Backend/AnheloPets.API/Program.cs`
- `frontend/src/services/healthServices.js` (rutas /api/MedicalRecords)
- `frontend/src/services/petServices.js` (getAnimals, createAnimals)
- `frontend/src/views/admin/SaludAdminView.vue` (wizard + inline add pet)