// src/composables/useAuditLog.js
//
// Composable centralizado de auditoría para Anhelo Pets.
// Cualquier módulo del sistema debe usar `registrarAuditoria(...)`
// (o los helpers específicos) para dejar constancia de una acción.
// El historial se persiste en localStorage y lo consume Auditoria.vue
// a través de `getAuditLog()`.

const AUDIT_LOG_KEY = 'anhelo_audit_log'
const USUARIO_ACTUAL_KEY = 'anhelo_usuario_actual'

// ─────────────────────────────────────────────
// Constantes (deben coincidir con lo que usa Auditoria.vue)
// ─────────────────────────────────────────────
export const AUDIT_MODULOS = [
  'Mascotas',
  'Adopciones',
  'Rescates',
  'Salud',
  'Usuarios',
  'Donaciones',
  'Voluntarios',
  'Autenticación',
]

export const AUDIT_TIPOS_ACCION = [
  'crear',
  'editar',
  'eliminar',
  'aprobar',
  'rechazar',
  'estado',
  'asignar',
  'sesion',
  'password',
]

// ─────────────────────────────────────────────
// Helpers internos
// ─────────────────────────────────────────────
function getUsuarioActual() {
  try {
    return JSON.parse(localStorage.getItem(USUARIO_ACTUAL_KEY)) || null
  } catch {
    return null
  }
}

function leerLog() {
  try {
    const raw = localStorage.getItem(AUDIT_LOG_KEY)
    return raw ? JSON.parse(raw) : []
  } catch (e) {
    console.error('Auditoría: log corrupto en localStorage, se reinicia', e)
    return []
  }
}

function guardarLog(lista) {
  try {
    localStorage.setItem(AUDIT_LOG_KEY, JSON.stringify(lista))
  } catch (e) {
    console.error('Auditoría: no se pudo guardar el registro', e)
  }
}

function generarId() {
  return 'aud_' + Date.now().toString(36) + '_' + Math.random().toString(36).slice(2, 8)
}

function formatearFechaHora(date) {
  const fecha = date.toISOString().slice(0, 10) // YYYY-MM-DD
  const hora = date.toTimeString().slice(0, 8)   // HH:MM:SS
  return { fecha, hora }
}

// ─────────────────────────────────────────────
// API pública
// ─────────────────────────────────────────────

/**
 * Registra un evento de auditoría. Úsalo desde cualquier módulo.
 *
 * @param {Object} datos
 * @param {string} datos.modulo         - Uno de AUDIT_MODULOS (ej. 'Mascotas')
 * @param {string} datos.accion         - Texto legible (ej. 'Creó una mascota')
 * @param {string} datos.tipoAccion     - Uno de AUDIT_TIPOS_ACCION
 * @param {string} [datos.elemento]     - Nombre del elemento afectado
 * @param {string|number} [datos.elementoId]
 * @param {string} [datos.descripcion]  - Detalle adicional de la acción
 * @param {'Exitoso'|'Fallido'} [datos.estado='Exitoso']
 * @param {Object|null} [datos.valoresAnteriores] - Solo para ediciones
 * @param {Object|null} [datos.valoresNuevos]     - Solo para ediciones
 * @param {Object} [datos.usuario]      - Override manual del usuario (útil en login,
 *                                        donde el usuario aún no está en localStorage)
 * @returns {Object} el registro creado
 */
export function registrarAuditoria(datos) {
  const usuarioActual = datos.usuario || getUsuarioActual()
  const ahora = new Date()
  const { fecha, hora } = formatearFechaHora(ahora)

  const registro = {
    id: generarId(),
    fecha,
    hora,
    timestamp: ahora.getTime(),
    usuario: usuarioActual?.nombre || usuarioActual?.email || 'Sistema',
    usuarioId: usuarioActual?.id ?? usuarioActual?._id ?? null,
    rol: usuarioActual?.rol || 'Desconocido',
    modulo: datos.modulo || 'General',
    accion: datos.accion || '',
    tipoAccion: datos.tipoAccion || 'editar',
    elemento: datos.elemento || '—',
    elementoId: datos.elementoId ?? null,
    descripcion: datos.descripcion || '',
    estado: datos.estado || 'Exitoso',
    valoresAnteriores: datos.valoresAnteriores || null,
    valoresNuevos: datos.valoresNuevos || null,
  }

  const lista = leerLog()
  lista.push(registro)
  guardarLog(lista)
  return registro
}

/** Devuelve todos los registros, más recientes primero. */
export function getAuditLog() {
  return leerLog().sort((a, b) => (b.timestamp || 0) - (a.timestamp || 0))
}

/** Vacía completamente el historial (uso administrativo). */
export function limpiarAuditLog() {
  guardarLog([])
}

// ─────────────────────────────────────────────
// Helpers de conveniencia para casos comunes
// ─────────────────────────────────────────────

export function registrarLogin(usuario, exitoso = true, motivoFallo = '') {
  return registrarAuditoria({
    modulo: 'Autenticación',
    accion: exitoso ? 'Inicio de sesión' : 'Intento de inicio de sesión fallido',
    tipoAccion: 'sesion',
    elemento: usuario?.nombre || usuario?.email || 'Usuario',
    elementoId: usuario?.id ?? usuario?._id ?? null,
    descripcion: exitoso
      ? 'El usuario inició sesión correctamente.'
      : motivoFallo || 'Credenciales inválidas.',
    estado: exitoso ? 'Exitoso' : 'Fallido',
    usuario,
  })
}

export function registrarLogout(usuario) {
  return registrarAuditoria({
    modulo: 'Autenticación',
    accion: 'Cierre de sesión',
    tipoAccion: 'sesion',
    elemento: usuario?.nombre || usuario?.email || 'Usuario',
    elementoId: usuario?.id ?? usuario?._id ?? null,
    descripcion: 'El usuario cerró sesión.',
    estado: 'Exitoso',
    usuario,
  })
}

export function registrarCambioPassword(usuario, exitoso = true) {
  return registrarAuditoria({
    modulo: 'Usuarios',
    accion: 'Cambio de contraseña',
    tipoAccion: 'password',
    elemento: usuario?.nombre || usuario?.email || 'Usuario',
    elementoId: usuario?.id ?? usuario?._id ?? null,
    descripcion: exitoso ? 'La contraseña fue actualizada.' : 'Falló el cambio de contraseña.',
    estado: exitoso ? 'Exitoso' : 'Fallido',
  })
}

export function registrarCambioEstado({ modulo, elemento, elementoId, estadoAnterior, estadoNuevo, descripcion }) {
  return registrarAuditoria({
    modulo,
    accion: 'Cambio de estado',
    tipoAccion: 'estado',
    elemento,
    elementoId,
    descripcion: descripcion || `Estado cambiado de "${estadoAnterior}" a "${estadoNuevo}".`,
    valoresAnteriores: { estado: estadoAnterior },
    valoresNuevos: { estado: estadoNuevo },
  })
}

/**
 * Composable-style export, por si se prefiere usar con destructuring
 * en <script setup>: const { registrarAuditoria } = useAuditLog()
 */
export function useAuditLog() {
  return {
    AUDIT_MODULOS,
    AUDIT_TIPOS_ACCION,
    registrarAuditoria,
    getAuditLog,
    limpiarAuditLog,
    registrarLogin,
    registrarLogout,
    registrarCambioPassword,
    registrarCambioEstado,
  }
}