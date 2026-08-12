<template>
  <NavBar />

  <!-- HERO — tamaño y proporciones originales -->
  <section class="donate-hero">
    <div class="hero-container">
      <div class="hero-left">
        <h1>
          Un pequeño aporte puede
          salvar una gran
          historia
        </h1>
        <p class="hero-text">
          Tu donación ayuda a rescatar, alimentar y brindar
          atención veterinaria a perros y gatos que buscan
          una segunda oportunidad.
        </p>
      </div>
    </div>
  </section>

  <!-- DONACIONES -->
  <section class="donation-section">
    <div class="container">

      <div class="section-header">
        <h2>Elige la forma que prefieras</h2>
        <p>Puedes apoyarnos mediante donaciones, transferencias o suministros.</p>
      </div>

      <!-- MÉTODOS — BENTO -->
      <div class="methods-bento">

        <!-- PAYPAL -->
        <div class="paypal-card">
          <svg class="paypal-blob" viewBox="0 0 200 200"><path fill="#92A894" fill-opacity="0.15" d="M45,-58C58,-49,68,-34,71,-17C75,0,72,20,61,35C51,50,33,60,13,65C-7,71,-29,72,-45,62C-61,52,-71,32,-73,11C-75,-10,-68,-33,-53,-47C-39,-62,-17,-68,3,-70C22,-72,32,-67,45,-58Z" transform="translate(100 100)"/></svg>
          <h3>Donar con PayPal</h3>
          <p>PayPal es una forma rápida y segura de ayudarnos desde cualquier parte del mundo. Cada contribución cubre alimento, rescates, medicamentos y tratamientos veterinarios.</p>
          <div class="paypal-chips">
            <a href="https://www.paypal.me/FundacionAnhelo/5" target="_blank" class="paypal-chip">Donar $5</a>
            <a href="https://www.paypal.me/FundacionAnhelo/10" target="_blank" class="paypal-chip">Donar $10</a>
            <a href="https://www.paypal.me/FundacionAnhelo/25" target="_blank" class="paypal-chip">Donar $25</a>
            <a href="https://www.paypal.me/FundacionAnhelo/50" target="_blank" class="paypal-chip">Donar $50</a>
            <a href="https://www.paypal.me/FundacionAnhelo" target="_blank" class="paypal-chip paypal-chip-solid">Otro monto →</a>
          </div>
        </div>

        <!-- SINPE -->
        <div class="sinpe-card">
          <div class="sinpe-top">
            <h3>SINPE Móvil</h3>
            <p>La forma más sencilla de ayudarnos desde Costa Rica.</p>
          </div>
          <div class="sinpe-bottom">
            <div class="sinpe-number">+506 8840 334</div>
            <div class="sinpe-holder">Shirley Valverde Aguilar</div>
            <button class="copy-btn" :class="{ copied: copiedId === 'sinpe' }" @click="copyToClipboard('+50688840334', 'sinpe')">
              {{ copiedId === 'sinpe' ? '✓ Copiado' : 'Copiar número' }}
            </button>
          </div>
        </div>

      </div>

      <!-- CUENTAS — franja compacta -->
      <div class="accounts-strip">
        <div class="account-cell">
          <span class="account-label">Cuenta BCR (colones)</span>
          <strong class="account-number">CR28015202280000590991</strong>
          <button class="strip-copy-btn" :class="{ copied: copiedId === 'bcr' }" @click="copyToClipboard('CR28015202280000590991', 'bcr')">
            {{ copiedId === 'bcr' ? '✓ Copiado' : 'Copiar IBAN' }}
          </button>
        </div>
        <div class="account-cell">
          <span class="account-label">Cuenta en dólares</span>
          <strong class="account-number">CR37015202001328713097</strong>
          <button class="strip-copy-btn" :class="{ copied: copiedId === 'usd' }" @click="copyToClipboard('CR37015202001328713097', 'usd')">
            {{ copiedId === 'usd' ? '✓ Copiado' : 'Copiar cuenta' }}
          </button>
        </div>
        <div class="account-cell">
          <span class="account-label">Coopealianza</span>
          <strong class="account-number">CR98081300210001059638</strong>
          <button class="strip-copy-btn" :class="{ copied: copiedId === 'coope' }" @click="copyToClipboard('CR98081300210001059638', 'coope')">
            {{ copiedId === 'coope' ? '✓ Copiado' : 'Copiar cuenta' }}
          </button>
        </div>
      </div>

      <!-- CTA REGISTRO -->
      <div class="registro-cta">
        <svg class="registro-blob" viewBox="0 0 200 200"><path fill="#D4B06A" fill-opacity="0.15" d="M39,-51C50,-42,58,-28,62,-12C66,4,66,22,58,36C50,50,35,60,17,66C0,72,-20,74,-37,66C-54,58,-68,41,-72,22C-77,3,-71,-18,-59,-34C-48,-50,-31,-61,-13,-65C6,-70,28,-60,39,-51Z" transform="translate(100 100)"/></svg>
        <div class="registro-content">
          <h2>¿Ya realizaste tu donación?</h2>
          <p>
            Si realizaste una transferencia, SINPE o depósito,
            registra tu comprobante para que podamos validar tu aporte.
          </p>
          <button class="btn-abrir-modal" @click="modalFormulario = true">
            Registrar comprobante
            <span class="btn-arrow">→</span>
          </button>
        </div>
      </div>

      <!-- MENSAJE ÉXITO -->
      <transition name="fade-up">
        <div v-if="exito" class="exito-card">
          <div class="exito-card-left">
            <div class="exito-card-icon">✓</div>
            <div>
              <h3>Donación registrada</h3>
              <p>
                Revisaremos tu comprobante y validaremos tu aporte pronto.
              </p>
            </div>
          </div>
        </div>
      </transition>

      <!-- ══════════════════════════════════════════ -->
      <!-- MODAL DEL FORMULARIO                       -->
      <!-- ══════════════════════════════════════════ -->
      <transition name="modal-fade">
        <div v-if="modalFormulario" class="modal-overlay" @click.self="cerrarModalForm">
          <div class="modal-form-box">

            <!-- Header del modal -->
            <div class="modal-form-header">
              <div class="modal-header-icon">
                <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#3A473C" stroke-width="1.5">
                  <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/>
                </svg>
              </div>
              <div class="modal-header-text">
                <h3>Registrar donación</h3>
                <p>Completa los datos para que nuestro equipo valide tu aporte.</p>
              </div>
              <button class="modal-form-close" @click="cerrarModalForm">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                  <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
                </svg>
              </button>
            </div>

            <form @submit.prevent="enviarDonacion" novalidate class="modal-form-body">

              <!-- ── SECCIÓN 1: Información personal ── -->
              <div class="form-section">
                <div class="form-section-header">
                  <div class="form-section-icon">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/>
                    </svg>
                  </div>
                  <span>Información personal</span>
                </div>

                <div class="form-row">
                  <div class="form-group" :class="{ 'has-error': errores.nombre }">
                    <label class="form-label">Nombre completo <span class="req">*</span></label>
                    <div class="input-wrap">
                      <svg class="input-icon" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
                        <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/>
                      </svg>
                      <input v-model="form.nombre" type="text" placeholder="Tu nombre completo" class="reg-input" />
                    </div>
                    <span v-if="errores.nombre" class="error-msg">{{ errores.nombre }}</span>
                  </div>

                  <div class="form-group" :class="{ 'has-error': errores.correo }">
                    <label class="form-label">Correo electrónico <span class="req">*</span></label>
                    <div class="input-wrap">
                      <svg class="input-icon" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
                        <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/>
                        <polyline points="22,6 12,13 2,6"/>
                      </svg>
                      <input v-model="form.correo" type="email" placeholder="correo@ejemplo.com" class="reg-input" />
                    </div>
                    <span v-if="errores.correo" class="error-msg">{{ errores.correo }}</span>
                  </div>
                </div>

                <div class="form-row">
                  <div class="form-group" :class="{ 'has-error': errores.telefono }">
                    <label class="form-label">Teléfono <span class="req">*</span></label>
                    <div class="input-wrap">
                      <svg class="input-icon" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
                        <path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07A19.5 19.5 0 0 1 4.69 13a19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 3.6 2.22h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z"/>
                      </svg>
                      <input v-model="form.telefono" type="tel" placeholder="+506 8888-8888" class="reg-input" />
                    </div>
                    <span v-if="errores.telefono" class="error-msg">{{ errores.telefono }}</span>
                  </div>

                  <div class="form-group" :class="{ 'has-error': errores.fechaDonacion }">
                    <label class="form-label">Fecha de la donación <span class="req">*</span></label>
                    <div class="input-wrap">
                      <svg class="input-icon" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
                        <rect x="3" y="4" width="18" height="18" rx="2" ry="2"/>
                        <line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/>
                      </svg>
                      <input v-model="form.fechaDonacion" type="date" class="reg-input" :max="hoy" />
                    </div>
                    <span v-if="errores.fechaDonacion" class="error-msg">{{ errores.fechaDonacion }}</span>
                  </div>
                </div>
              </div>

              <!-- ── SECCIÓN 2: Información del aporte ── -->
              <div class="form-section">
                <div class="form-section-header">
                  <div class="form-section-icon">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <rect x="2" y="5" width="20" height="14" rx="2"/><line x1="2" y1="10" x2="22" y2="10"/>
                    </svg>
                  </div>
                  <span>Información del aporte</span>
                </div>

                <div class="form-row">
                  <div class="form-group" :class="{ 'has-error': errores.metodo }">
                    <label class="form-label">Método de donación <span class="req">*</span></label>
                    <div class="select-wrap">
                      <select v-model="form.metodo" class="reg-input reg-select" @change="sugerirMoneda">
                        <option value="" disabled>Selecciona un método</option>
                        <option value="PayPal">PayPal</option>
                        <option value="SINPE Móvil">SINPE Móvil</option>
                        <option value="BCR">BCR</option>
                        <option value="Cuenta USD">Cuenta USD</option>
                        <option value="Coopealianza">Coopealianza</option>
                      </select>
                      <svg class="select-arrow" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <polyline points="6 9 12 15 18 9"/>
                      </svg>
                    </div>
                    <span v-if="errores.metodo" class="error-msg">{{ errores.metodo }}</span>
                  </div>

                  <div class="form-group" :class="{ 'has-error': errores.moneda }">
                    <label class="form-label">Moneda <span class="req">*</span></label>
                    <div class="moneda-toggle">
                      <button
                        type="button"
                        class="moneda-btn"
                        :class="{ active: form.moneda === 'CRC' }"
                        @click="setMoneda('CRC')"
                      >
                        <span class="moneda-symbol">₡</span> Colones (CRC)
                      </button>
                      <button
                        type="button"
                        class="moneda-btn"
                        :class="{ active: form.moneda === 'USD' }"
                        @click="setMoneda('USD')"
                      >
                        <span class="moneda-symbol">$</span> Dólares (USD)
                      </button>
                    </div>
                    <transition name="fade-hint">
                      <div v-if="monedaSugerida" class="moneda-sugerida">
                        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                          <circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>
                        </svg>
                        Sugerido para {{ form.metodo }}
                      </div>
                    </transition>
                    <span v-if="errores.moneda" class="error-msg">{{ errores.moneda }}</span>
                  </div>
                </div>

                <!-- Monto -->
                <div class="form-group" :class="{ 'has-error': errores.monto }">
                  <label class="form-label">Monto donado <span class="req">*</span></label>
                  <div class="monto-field">
                    <span class="monto-prefix">{{ form.moneda === 'USD' ? '$' : '₡' }}</span>
                    <input
                      v-model="form.monto"
                      type="text"
                      inputmode="decimal"
                      :placeholder="form.moneda === 'USD' ? '0.00' : '0'"
                      class="monto-input"
                      @input="onMontoInput"
                      @blur="onMontoBlur"
                    />
                    <span class="monto-suffix">{{ form.moneda || 'CRC' }}</span>
                  </div>

                  <!-- Montos rápidos -->
                  <div class="montos-rapidos">
                    <button
                      v-for="m in montosRapidos"
                      :key="m.valor"
                      type="button"
                      class="monto-chip"
                      @click="seleccionarMonto(m.valor)"
                    >{{ m.etiqueta }}</button>
                  </div>
                  <span v-if="errores.monto" class="error-msg">{{ errores.monto }}</span>
                </div>
              </div>

              <!-- ── SECCIÓN 3: Comprobante ── -->
              <div class="form-section">
                <div class="form-section-header">
                  <div class="form-section-icon">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <path d="M21.44 11.05l-9.19 9.19a6 6 0 0 1-8.49-8.49l9.19-9.19a4 4 0 0 1 5.66 5.66l-9.2 9.19a2 2 0 0 1-2.83-2.83l8.49-8.48"/>
                    </svg>
                  </div>
                  <span>Comprobante de pago <span class="req">*</span></span>
                </div>

                <div class="form-group" :class="{ 'has-error': errores.comprobante }">
                  <!-- Zona drag & drop vacía -->
                  <div
                    v-if="!archivoNombre"
                    class="upload-zone"
                    :class="{ 'upload-dragging': dragActivo, 'upload-error-border': errores.comprobante }"
                    @click="$refs.fileInput.click()"
                    @dragover.prevent="dragActivo = true"
                    @dragleave.prevent="dragActivo = false"
                    @drop.prevent="onDrop"
                  >
                    <input
                      ref="fileInput"
                      type="file"
                      accept=".jpg,.jpeg,.png,.webp,.pdf"
                      style="display:none"
                      @change="onFileChange"
                    />
                    <div class="upload-zone-icon">
                      <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                        <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/>
                        <polyline points="17 8 12 3 7 8"/>
                        <line x1="12" y1="3" x2="12" y2="15"/>
                      </svg>
                    </div>
                    <p class="upload-titulo">Arrastra tu comprobante aquí</p>
                    <p class="upload-sub">o <span class="upload-link">selecciona un archivo</span></p>
                    <p class="upload-tipos">JPG · PNG · WEBP · PDF</p>
                  </div>

                  <!-- Vista previa con archivo cargado -->
                  <div v-else class="upload-preview-card">
                    <input
                      ref="fileInput"
                      type="file"
                      accept=".jpg,.jpeg,.png,.webp,.pdf"
                      style="display:none"
                      @change="onFileChange"
                    />

                    <!-- Imagen preview -->
                    <div v-if="previewUrl && !esPDF" class="preview-img-wrap">
                      <img :src="previewUrl" class="preview-img-thumb" alt="Vista previa" />
                    </div>

                    <!-- PDF preview -->
                    <div v-else class="preview-pdf-wrap">
                      <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#B93C3C" stroke-width="1.5">
                        <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>
                        <polyline points="14 2 14 8 20 8"/>
                        <line x1="16" y1="13" x2="8" y2="13"/>
                        <line x1="16" y1="17" x2="8" y2="17"/>
                        <polyline points="10 9 9 9 8 9"/>
                      </svg>
                    </div>

                    <div class="preview-info">
                      <div class="preview-nombre">{{ archivoNombre }}</div>
                      <div class="preview-tamano">{{ archivoTamano }}</div>
                      <button type="button" class="btn-cambiar" @click="$refs.fileInput.click()">Cambiar archivo</button>
                    </div>

                    <button type="button" class="btn-remover-x" @click.stop="removerArchivo" title="Eliminar">
                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                        <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
                      </svg>
                    </button>
                  </div>
                  <span v-if="errores.comprobante" class="error-msg">{{ errores.comprobante }}</span>
                </div>
              </div>

              <!-- ── SECCIÓN 4: Mensaje opcional ── -->
              <div class="form-section form-section-last">
                <div class="form-section-header">
                  <div class="form-section-icon">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/>
                    </svg>
                  </div>
                  <span>Mensaje opcional</span>
                </div>
                <div class="form-group">
                  <textarea
                    v-model="form.mensaje"
                    placeholder="¿Quieres dejar algún mensaje para el equipo de Anhelo Pets?"
                    class="reg-input reg-textarea"
                    rows="3"
                    maxlength="300"
                  ></textarea>
                  <div class="textarea-count">{{ form.mensaje.length }}/300</div>
                </div>
              </div>

              <!-- Acciones -->
              <span v-if="errorEnvio" class="error-msg">{{ errorEnvio }}</span>
              <div class="modal-form-footer">
                <button type="button" class="btn-cancelar" @click="cerrarModalForm">Cancelar</button>
                <button type="submit" class="btn-enviar" :disabled="enviando">
                  <span v-if="enviando" class="spinner"></span>
                  <svg v-else width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/>
                  </svg>
                  {{ enviando ? 'Registrando...' : 'Enviar donación' }}
                </button>
              </div>

            </form>
          </div>
        </div>
      </transition>

      <!-- SUMINISTROS -->
      <div class="supplies-wrapper">
        <div class="supplies-section">
          <div class="supply-header">
            <h2>Donar suministros</h2>
            <p>También puedes ayudarnos donando artículos esenciales para perros y gatos rescatados. Cada aporte hace una gran diferencia.</p>
          </div>
          <div class="supplies-grid">
            <div class="supply-box">
              <span class="supply-index">01</span>
              <h4>Alimento</h4>
              <p>Comida para perros y gatos, adultos, cachorros y gatitos.</p>
            </div>
            <div class="supply-box">
              <span class="supply-index">02</span>
              <h4>Medicamentos</h4>
              <p>Antipulgas, vitaminas, desparasitantes y productos veterinarios básicos.</p>
            </div>
            <div class="supply-box">
              <span class="supply-index">03</span>
              <h4>Accesorios</h4>
              <p>Correas, arneses, juguetes, camas, mantas y transportadoras.</p>
            </div>
            <div class="supply-box">
              <span class="supply-index">04</span>
              <h4>Limpieza</h4>
              <p>Jabón, desinfectantes, bolsas, detergente y artículos de limpieza.</p>
            </div>
          </div>
        </div>
      </div>

    </div>
  </section>

  <!-- FINAL -->
  <section class="final-section">
    <svg class="final-blob" viewBox="0 0 200 200"><path fill="#D4B06A" fill-opacity="0.10" d="M39,-51C50,-42,58,-28,62,-12C66,4,66,22,58,36C50,50,35,60,17,66C0,72,-20,74,-37,66C-54,58,-68,41,-72,22C-77,3,-71,-18,-59,-34C-48,-50,-31,-61,-13,-65C6,-70,28,-60,39,-51Z" transform="translate(100 100)"/></svg>
    <div class="final-box">
      <h2>Gracias por ayudar</h2>
      <p>
        Tu apoyo significa rescates, alimento, tratamientos médicos y nuevas oportunidades para
        animales que merecen una vida llena de amor y seguridad.
      </p>
    </div>
  </section>

  <FooterBar />
</template>

<script setup>
import { ref, reactive, computed, onMounted, watch } from 'vue'
import NavBar from '../components/NavBar.vue'
import FooterBar from '../components/FooterBar.vue'
import { submitDonation } from '../services/donationServices'
import { useAuthStore } from '../stores/useAuthStore'

const authStore = useAuthStore()

// ─── Clipboard ──────────────────────────────────────────
const copiedId = ref(null)
async function copyToClipboard(text, id) {
  try {
    await navigator.clipboard.writeText(text)
    copiedId.value = id
    setTimeout(() => { copiedId.value = null }, 2000)
  } catch {
    const el = document.createElement('textarea')
    el.value = text
    el.style.position = 'fixed'; el.style.opacity = '0'
    document.body.appendChild(el); el.select()
    document.execCommand('copy'); document.body.removeChild(el)
    copiedId.value = id
    setTimeout(() => { copiedId.value = null }, 2000)
  }
}

// ─── Fecha ────────────────────────────────────────────────
const hoy = new Date().toISOString().split('T')[0]

// ─── Modal ───────────────────────────────────────────────
const modalFormulario = ref(false)

function cerrarModalForm() {
  modalFormulario.value = false
}

// ─── Formulario ──────────────────────────────────────────
const form = reactive({
  nombre: '', correo: '', telefono: '',
  metodo: '', moneda: 'CRC', monto: '',
  fechaDonacion: '', mensaje: ''
})

const errores      = reactive({})
const enviando     = ref(false)
const exito        = ref(false)
const errorEnvio   = ref('')
const fileInput  = ref(null)
const archivoNombre = ref('')
const archivoTamano = ref('')
const previewUrl    = ref('')
const archivoBase64 = ref('')
const esPDF         = ref(false)
const dragActivo    = ref(false)
const monedaSugerida = ref(false)

// ─── Moneda inteligente ───────────────────────────────────
const metodosCRC = ['SINPE Móvil', 'BCR', 'Coopealianza']
const metodosUSD = ['PayPal', 'Cuenta USD']

function sugerirMoneda() {
  if (metodosCRC.includes(form.metodo)) {
    form.moneda = 'CRC'; monedaSugerida.value = true
  } else if (metodosUSD.includes(form.metodo)) {
    form.moneda = 'USD'; monedaSugerida.value = true
  } else {
    monedaSugerida.value = false
  }
  form.monto = '' // resetear monto al cambiar método
}

function setMoneda(moneda) {
  form.moneda = moneda
  form.monto = ''
  monedaSugerida.value = false
}

// ─── Montos rápidos ───────────────────────────────────────
const montosRapidos = computed(() => {
  if (form.moneda === 'USD') {
    return [
      { valor: 5,   etiqueta: '$ 5' },
      { valor: 10,  etiqueta: '$ 10' },
      { valor: 25,  etiqueta: '$ 25' },
      { valor: 50,  etiqueta: '$ 50' },
      { valor: 100, etiqueta: '$ 100' },
    ]
  }
  return [
    { valor: 5000,   etiqueta: '₡ 5.000' },
    { valor: 10000,  etiqueta: '₡ 10.000' },
    { valor: 25000,  etiqueta: '₡ 25.000' },
    { valor: 50000,  etiqueta: '₡ 50.000' },
    { valor: 100000, etiqueta: '₡ 100.000' },
  ]
})

function seleccionarMonto(valor) {
  if (form.moneda === 'CRC') {
    // Formatear con separadores de miles estilo es-CR (puntos)
    form.monto = valor.toLocaleString('es-CR')
  } else {
    form.monto = String(valor)
  }
}

// ─── Manejo del input de monto ────────────────────────────
// Estrategia: guardar en form.monto el valor formateado visualmente.
// Al guardar, parsear correctamente.

function onMontoInput(e) {
  if (form.moneda === 'CRC') {
    // Solo dígitos, luego aplica separadores de miles
    const raw = e.target.value.replace(/[^\d]/g, '')
    if (raw === '') {
      form.monto = ''
    } else {
      // Formatear con separadores: es-CR usa puntos como separador de miles
      form.monto = parseInt(raw, 10).toLocaleString('es-CR')
    }
  }
  // USD: permitir números y punto decimal libremente
  // No transformamos aquí para no interferir con la escritura del punto
}

function onMontoBlur() {
  if (form.moneda === 'USD' && form.monto) {
    // Normalizar al hacer blur: reemplazar coma por punto si usa coma decimal
    const clean = form.monto.replace(/,/g, '.')
    const num = parseFloat(clean)
    if (!isNaN(num)) {
      form.monto = num.toString()
    }
  }
}

/**
 * Convierte el valor visual del campo monto en número real.
 * CRC: "35.000" → 35000, "100.000" → 100000
 * USD: "25" → 25, "25.50" → 25.5
 */
function montoNumerico() {
  if (!form.monto) return 0
  const str = String(form.monto).trim()

  if (form.moneda === 'CRC') {
    // es-CR usa punto como separador de miles. Eliminarlos todos.
    const sinPuntos = str.replace(/\./g, '').replace(/,/g, '')
    const num = parseInt(sinPuntos, 10)
    return isNaN(num) ? 0 : num
  } else {
    // USD: puede tener coma decimal (algunos teclados) o punto decimal
    const normalizado = str.replace(/,/g, '.')
    const num = parseFloat(normalizado)
    return isNaN(num) ? 0 : num
  }
}

// ─── Archivo ─────────────────────────────────────────────
function onFileChange(e) {
  const file = e.target.files[0]
  if (file) procesarArchivo(file)
}

function onDrop(e) {
  dragActivo.value = false
  const file = e.dataTransfer.files[0]
  if (file) procesarArchivo(file)
}

function formatBytes(bytes) {
  if (bytes < 1024) return bytes + ' B'
  if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB'
  return (bytes / (1024 * 1024)).toFixed(1) + ' MB'
}

function procesarArchivo(file) {
  const tipos = ['image/jpeg', 'image/png', 'image/webp', 'application/pdf']
  if (!tipos.includes(file.type)) {
    errores.comprobante = 'Formato no válido. Usa JPG, PNG, WEBP o PDF.'
    return
  }
  delete errores.comprobante
  archivoNombre.value = file.name
  archivoTamano.value = formatBytes(file.size)
  esPDF.value = file.type === 'application/pdf'

  const reader = new FileReader()
  reader.onload = (ev) => {
    archivoBase64.value = ev.target.result
    previewUrl.value = esPDF.value ? '' : ev.target.result
  }
  reader.readAsDataURL(file)
}

function removerArchivo() {
  archivoNombre.value = ''; archivoTamano.value = ''
  previewUrl.value = ''; archivoBase64.value = ''
  esPDF.value = false
  if (fileInput.value) fileInput.value.value = ''
}

// ─── Validación ──────────────────────────────────────────
function validar() {
  Object.keys(errores).forEach(k => delete errores[k])
  let ok = true

  if (!form.nombre.trim())       { errores.nombre        = 'El nombre es obligatorio.'; ok = false }
  if (!form.correo.trim())       { errores.correo        = 'El correo es obligatorio.'; ok = false }
  else if (!/\S+@\S+\.\S+/.test(form.correo)) { errores.correo = 'Ingresa un correo válido.'; ok = false }
  if (!form.telefono.trim())     { errores.telefono      = 'El teléfono es obligatorio.'; ok = false }
  if (!form.metodo)              { errores.metodo        = 'Selecciona un método de pago.'; ok = false }
  if (!form.moneda)              { errores.moneda        = 'Selecciona una moneda.'; ok = false }
  if (!form.monto || montoNumerico() <= 0) { errores.monto = 'El monto debe ser mayor a cero.'; ok = false }
  if (!form.fechaDonacion)       { errores.fechaDonacion = 'La fecha es obligatoria.'; ok = false }
  if (!archivoBase64.value)      { errores.comprobante   = 'El comprobante es obligatorio.'; ok = false }

  return ok
}

// ─── Pre-llenar usuario ──────────────────────────────────
function prellenarDesdeSesion() {
  const u = authStore.user
  if (!u) return
  const nombre = [u.firstName, u.lastName].filter(Boolean).join(' ')
  if (nombre)        form.nombre = nombre
  if (u.email)       form.correo = u.email
  if (u.phonePrimary) form.telefono = u.phonePrimary
}

onMounted(prellenarDesdeSesion)
watch(() => authStore.user, prellenarDesdeSesion)

// ─── Envío ───────────────────────────────────────────────
async function enviarDonacion() {
  if (!validar()) return
  enviando.value = true
  errorEnvio.value = ''

  try {
    await submitDonation({
      donorName: form.nombre.trim(),
      email: form.correo.trim(),
      phone: form.telefono.trim(),
      method: form.metodo,
      currency: form.moneda,
      amount: montoNumerico(), // ← número real, sin separadores
      donatedAt: form.fechaDonacion,
      message: form.mensaje.trim(),
      proofFile: archivoBase64.value,
    })
  } catch (error) {
    console.error(error)
    errorEnvio.value = 'No se pudo registrar la donación. Intenta de nuevo.'
    enviando.value = false
    return
  }

  enviando.value = false
  exito.value = true
  cerrarModalForm()

  // Reset
  Object.assign(form, { nombre: '', correo: '', telefono: '', metodo: '', moneda: 'CRC', monto: '', fechaDonacion: '', mensaje: '' })
  removerArchivo()

  // Re-llenar desde usuario
  prellenarDesdeSesion()

  setTimeout(() => { exito.value = false }, 6000)
}
</script>

<style scoped>
.container {
  max-width: 1100px;
  margin: auto;
  padding: 0 24px;
}

/* HERO — tamaño y proporciones originales, sin cambios */
.donate-hero {
  height: 430px;
  background-image: url('/img-donacion/donahero.PNG');
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
  display: flex;
  align-items: center;
  position: relative;
  overflow: hidden;
  padding: 0 24px;
}

.donate-hero::before {
  content: '';
  position: absolute;
  inset: 0;
  background: linear-gradient(90deg, rgba(0,0,0,0.72) 0%, rgba(0,0,0,0.45) 35%, rgba(0,0,0,0.12) 70%, rgba(0,0,0,0) 100%);
}

.hero-container {
  position: relative;
  z-index: 2;
  max-width: 1400px;
  width: 100%;
  margin: 0 auto;
  padding: 0 80px;
}

.hero-left { max-width: 560px; }

.hero-left h1 {
  font-size: 62px;
  line-height: 0.95;
  letter-spacing: -3px;
  font-weight: 800;
  color: white;
  margin-bottom: 24px;
}

.hero-text {
  font-size: 16px;
  line-height: 1.7;
  color: rgba(255,255,255,0.92);
  max-width: 420px;
}

/* SECTION */
.donation-section { background: #FAFAFA; padding: 64px 0 110px; }

.section-header { text-align: center; margin-bottom: 48px; }
.section-header h2 { font-size: 34px; font-weight: 800; color: #3A473C; margin-bottom: 10px; letter-spacing: -0.5px; }
.section-header p  { font-size: 14px; color: #6C756D; line-height: 1.7; }

/* MÉTODOS — BENTO */
.methods-bento {
  display: grid;
  grid-template-columns: 1.3fr 1fr;
  gap: 20px;
  margin-bottom: 20px;
}

.paypal-card {
  background: linear-gradient(135deg, #3A473C, #232B25);
  border-radius: 28px;
  padding: 44px;
  position: relative;
  overflow: hidden;
}

.paypal-blob {
  position: absolute;
  top: -40px;
  right: -40px;
  width: 220px;
  height: 220px;
  pointer-events: none;
}

.paypal-card h3 {
  font-size: 26px;
  font-weight: 800;
  color: white;
  margin: 0 0 10px;
  position: relative;
  z-index: 1;
}

.paypal-card p {
  font-size: 13.5px;
  color: rgba(255,255,255,0.68);
  line-height: 1.7;
  margin: 0 0 26px;
  max-width: 380px;
  position: relative;
  z-index: 1;
}

.paypal-chips {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
  position: relative;
  z-index: 1;
}

.paypal-chip {
  background: rgba(255,255,255,0.10);
  color: white;
  text-decoration: none;
  padding: 12px 22px;
  border-radius: 12px;
  font-size: 13px;
  font-weight: 700;
  transition: 0.25s ease;
}

.paypal-chip:hover { background: rgba(255,255,255,0.18); }

.paypal-chip-solid {
  background: white;
  color: #3A473C;
  font-weight: 800;
}

.paypal-chip-solid:hover { background: #EDEFED; }

.sinpe-card {
  background: white;
  border: 1px solid rgba(146,168,148,0.14);
  border-radius: 28px;
  padding: 32px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  box-shadow: 0 4px 18px rgba(0,0,0,0.03);
}

.sinpe-top h3 { font-size: 19px; font-weight: 800; color: #3A473C; margin: 0 0 6px; }
.sinpe-top p  { font-size: 12.5px; color: #6C756D; margin: 0 0 18px; line-height: 1.6; }

.sinpe-number { font-size: 21px; font-weight: 800; color: #3A473C; margin-bottom: 3px; white-space: nowrap; }
.sinpe-holder { font-size: 11.5px; color: #92A894; margin-bottom: 16px; }

.copy-btn {
  width: 100%;
  height: 42px;
  border-radius: 12px;
  border: 1.5px solid rgba(58,71,60,0.18);
  background: transparent;
  color: #3A473C;
  font-size: 13px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.22s ease;
}

.copy-btn:hover, .copy-btn.copied { background: #3A473C; color: white; border-color: #3A473C; }

/* CUENTAS — franja compacta */
.accounts-strip {
  background: white;
  border: 1px solid rgba(146,168,148,0.14);
  border-radius: 28px;
  box-shadow: 0 4px 18px rgba(0,0,0,0.03);
  overflow: hidden;
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  margin-bottom: 24px;
}

.account-cell {
  padding: 28px 30px;
  border-right: 1px solid #F1F3F1;
  display: flex;
  flex-direction: column;
}

.account-cell:last-child { border-right: none; }

.account-label {
  font-size: 10.5px;
  font-weight: 700;
  color: #92A894;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 10px;
}

.account-number {
  font-size: 13.5px;
  font-weight: 700;
  color: #3A473C;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  margin-bottom: 16px;
}

.strip-copy-btn {
  margin-top: auto;
  align-self: flex-start;
  background: none;
  border: none;
  padding: 0;
  color: #3A473C;
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 6px;
  transition: 0.2s ease;
}

.strip-copy-btn::before {
  content: '';
  width: 13px;
  height: 13px;
  flex-shrink: 0;
  background: currentColor;
  -webkit-mask: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='black' stroke-width='2'%3E%3Crect x='9' y='9' width='13' height='13' rx='2'/%3E%3Cpath d='M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1'/%3E%3C/svg%3E") center / contain no-repeat;
  mask: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='black' stroke-width='2'%3E%3Crect x='9' y='9' width='13' height='13' rx='2'/%3E%3Cpath d='M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1'/%3E%3C/svg%3E") center / contain no-repeat;
}

.strip-copy-btn:hover { color: #2D372F; }
.strip-copy-btn.copied { color: #92A894; }

/* REGISTRO CTA — protagonismo total */
.registro-cta {
  background: linear-gradient(120deg, #232B25, #3A473C 60%, #4b5c4d);
  border-radius: 32px;
  padding: 60px;
  position: relative;
  overflow: hidden;
  text-align: center;
  margin-bottom: 24px;
  box-shadow: 0 20px 50px rgba(58,71,60,0.18);
}

.registro-blob {
  position: absolute;
  top: -60px;
  left: -60px;
  width: 260px;
  height: 260px;
  pointer-events: none;
}

.registro-content { position: relative; z-index: 1; max-width: 500px; margin: 0 auto; }

.registro-cta h2 {
  font-size: 32px;
  font-weight: 800;
  color: white;
  letter-spacing: -1px;
  margin: 0 0 12px;
  line-height: 1.15;
}

.registro-cta p {
  font-size: 14.5px;
  color: rgba(255,255,255,0.68);
  line-height: 1.7;
  margin: 0 0 30px;
}

.btn-abrir-modal {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  padding: 16px 32px;
  border-radius: 14px;
  border: none;
  background: white;
  color: #3A473C;
  font-size: 14px;
  font-weight: 800;
  cursor: pointer;
  transition: all 0.25s ease;
}

.btn-abrir-modal:hover {
  transform: translateY(-1px) scale(1.02);
  box-shadow: 0 10px 26px rgba(0,0,0,0.20);
}

.btn-arrow { font-size: 16px; }

/* Banner de éxito */
.exito-card {
  background: #F7F4EC;
  border: 1px solid #E5D8BC;
  border-radius: 24px;
  padding: 24px 30px;
  margin-bottom: 24px;
  box-shadow: 0 4px 14px rgba(0,0,0,0.03);
}

.exito-card-left {
  display: flex;
  align-items: center;
  gap: 16px;
}

.exito-card-icon {
  width: 46px;
  height: 46px;
  border-radius: 14px;
  background: #D4B06A;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
  font-weight: 800;
  flex-shrink: 0;
}

.exito-card h3 { margin: 0 0 4px; font-size: 20px; font-weight: 800; color: #3A473C; }
.exito-card p  { margin: 0; font-size: 14px; color: #6C756D; line-height: 1.5; }

/* SUPPLIES */
.supplies-wrapper { display: flex; justify-content: center; }

.supplies-section {
  background: transparent;
  padding: 90px 0 0;
  width: 100%;
}

.supply-header { text-align: center; margin-bottom: 40px; }
.supply-header h2 { font-size: 32px; font-weight: 800; color: #3A473C; letter-spacing: -1px; margin-bottom: 10px; }
.supply-header p  { font-size: 13.5px; color: #6C756D; line-height: 1.8; max-width: 480px; margin: 0 auto; }

.supplies-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; }

.supply-box {
  background: white;
  border: 1px solid rgba(146,168,148,0.14);
  border-radius: 22px;
  padding: 30px 22px;
  text-align: center;
  transition: 0.25s ease;
}

.supply-box:hover { transform: translateY(-6px); box-shadow: 0 14px 30px rgba(0,0,0,0.05); }

.supply-index {
  display: block;
  font-size: 11px;
  font-weight: 800;
  color: #92A894;
  letter-spacing: 1px;
  margin-bottom: 10px;
}

.supply-box h4 { font-size: 15px; font-weight: 800; color: #3A473C; margin-bottom: 6px; }
.supply-box p  { font-size: 11.5px; line-height: 1.6; color: #6C756D; }

/* FINAL */
.final-section {
  position: relative;
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 100px 24px;
  overflow: hidden;
}

.final-section::before {
  content: '';
  position: absolute;
  top: -30px; left: -30px; right: -30px; bottom: -30px;
  background-image: url('/img-donacion/fondodonar.PNG');
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
  filter: blur(2px);
  transform: scale(1.08);
  z-index: 0;
}

.final-section::after {
  content: '';
  position: absolute;
  inset: 0;
  background: linear-gradient(135deg, rgba(92,108,94,0.75), rgba(45,55,47,0.80));
  z-index: 1;
}

.final-blob {
  position: absolute;
  bottom: -100px;
  right: -60px;
  width: 360px;
  height: 360px;
  pointer-events: none;
  z-index: 2;
}

.final-box {
  max-width: 640px;
  width: 100%;
  margin: 0 auto;
  text-align: center;
  position: relative;
  z-index: 2;
}

.final-box h2 { font-size: 34px; font-weight: 800; line-height: 1.1; letter-spacing: -1px; margin-bottom: 16px; color: white; }
.final-box p  { font-size: 14.5px; line-height: 1.85; color: rgba(255,255,255,0.75); max-width: 560px; margin: auto; }

/* ════════════════════════════════════
   MODAL DEL FORMULARIO
════════════════════════════════════ */

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(10,16,10,0.50);
  backdrop-filter: blur(8px);
  z-index: 1000;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
}

.modal-form-box {
  background: white;
  border-radius: 24px;
  width: 100%;
  max-width: 680px;
  max-height: 92vh;
  overflow-y: auto;
  box-shadow: 0 40px 100px rgba(0,0,0,0.20);
  display: flex;
  flex-direction: column;
}

/* Scrollbar suave */
.modal-form-box::-webkit-scrollbar { width: 6px; }
.modal-form-box::-webkit-scrollbar-track { background: transparent; }
.modal-form-box::-webkit-scrollbar-thumb { background: #D4DED5; border-radius: 3px; }

/* Header del modal */
.modal-form-header {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 28px 32px 20px;
  border-bottom: 1px solid #F0F2F0;
  position: sticky;
  top: 0;
  background: white;
  z-index: 10;
  border-radius: 24px 24px 0 0;
}

.modal-header-icon {
  width: 52px; height: 52px;
  border-radius: 14px;
  background: #F0F4F0;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.modal-header-text { flex: 1; }
.modal-header-text h3 { font-size: 20px; font-weight: 800; color: #3A473C; margin-bottom: 3px; }
.modal-header-text p  { font-size: 13px; color: #6C756D; line-height: 1.5; }

.modal-form-close {
  width: 34px; height: 34px;
  border-radius: 50%;
  border: none;
  background: #F4F6F4;
  color: #6C756D;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: 0.2s;
  flex-shrink: 0;
}

.modal-form-close:hover { background: #3A473C; color: white; }

/* Cuerpo del formulario */
.modal-form-body { padding: 24px 32px 28px; display: flex; flex-direction: column; gap: 0; }

/* Secciones del formulario */
.form-section {
  padding-bottom: 24px;
  margin-bottom: 24px;
  border-bottom: 1px solid #F0F4F0;
}

.form-section-last { border-bottom: none; margin-bottom: 0; padding-bottom: 0; }

.form-section-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 18px;
  font-size: 13px;
  font-weight: 700;
  color: #3A473C;
}

.form-section-icon {
  width: 28px; height: 28px;
  border-radius: 10px;
  background: #EEF1EC;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #3A473C;
  flex-shrink: 0;
}

/* Filas y grupos */
.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin-bottom: 16px; }
.form-row:last-child { margin-bottom: 0; }

.form-group { display: flex; flex-direction: column; gap: 6px; }

.form-label { font-size: 13px; font-weight: 700; color: #3A473C; }
.req { color: #B93C3C; font-weight: 700; }

/* Input con icono */
.input-wrap { position: relative; }

.input-icon {
  position: absolute;
  left: 13px;
  top: 50%;
  transform: translateY(-50%);
  color: #92A894;
  pointer-events: none;
}

.reg-input {
  width: 100%;
  padding: 12px 14px 12px 38px;
  border-radius: 12px;
  border: 1.5px solid #EAEEEA;
  background: #F6F8F6;
  font-size: 14px;
  color: #3A473C;
  transition: all 0.2s ease;
  outline: none;
  box-sizing: border-box;
  font-family: inherit;
}

.reg-input:focus { background: white; border-color: #3A473C; box-shadow: 0 0 0 3px rgba(58,71,60,0.08); }

.has-error .reg-input { border-color: #E07070; background: #FFF8F8; }
.has-error .upload-zone { border-color: #E07070; }

.error-msg { font-size: 12px; color: #B93C3C; font-weight: 600; }

/* Select */
.select-wrap { position: relative; }

.reg-select {
  padding-left: 14px;
  appearance: none;
  cursor: pointer;
}

.select-arrow {
  position: absolute;
  right: 12px;
  top: 50%;
  transform: translateY(-50%);
  color: #92A894;
  pointer-events: none;
}

.reg-textarea { padding: 12px 14px; resize: vertical; min-height: 88px; }

/* Moneda toggle */
.moneda-toggle {
  display: flex;
  gap: 8px;
}

.moneda-btn {
  flex: 1;
  padding: 10px 12px;
  border-radius: 12px;
  border: 1.5px solid #EAEEEA;
  background: #F6F8F6;
  color: #6C756D;
  font-size: 13px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  font-family: inherit;
}

.moneda-btn:hover { border-color: #92A894; background: white; color: #3A473C; }

.moneda-btn.active {
  background: #3A473C;
  color: white;
  border-color: #3A473C;
}

.moneda-symbol { font-size: 15px; font-weight: 800; }

.moneda-sugerida {
  display: flex;
  align-items: center;
  gap: 5px;
  font-size: 12px;
  color: #3A473C;
  font-weight: 600;
  padding: 5px 10px;
  background: #EEF7F0;
  border-radius: 10px;
  width: fit-content;
}

/* Campo de monto */
.monto-field {
  display: flex;
  align-items: center;
  border-radius: 12px;
  border: 1.5px solid #EAEEEA;
  background: #F6F8F6;
  overflow: hidden;
  transition: all 0.2s ease;
}

.monto-field:focus-within { background: white; border-color: #3A473C; box-shadow: 0 0 0 3px rgba(58,71,60,0.08); }

.monto-prefix {
  padding: 0 12px;
  font-size: 18px;
  font-weight: 800;
  color: #3A473C;
  background: #EAEEEA;
  height: 100%;
  display: flex;
  align-items: center;
  align-self: stretch;
  flex-shrink: 0;
}

.monto-input {
  flex: 1;
  padding: 12px 14px;
  border: none;
  background: transparent;
  font-size: 18px;
  font-weight: 700;
  color: #3A473C;
  outline: none;
  font-family: inherit;
  min-width: 0;
}

.monto-input::placeholder { font-size: 16px; font-weight: 400; color: #B0BAB2; }

.monto-suffix {
  padding: 0 14px;
  font-size: 12px;
  font-weight: 700;
  color: #92A894;
  text-transform: uppercase;
  flex-shrink: 0;
}

/* Montos rápidos */
.montos-rapidos { display: flex; gap: 6px; flex-wrap: wrap; margin-top: 10px; }

.monto-chip {
  padding: 5px 12px;
  border-radius: 20px;
  border: 1.5px solid #EAEEEA;
  background: #F6F8F6;
  font-size: 12px;
  font-weight: 700;
  color: #3A473C;
  cursor: pointer;
  transition: 0.2s;
  font-family: inherit;
}

.monto-chip:hover { background: #3A473C; color: white; border-color: #3A473C; }

/* Upload zone */
.upload-zone {
  border: 2px dashed #D4DED5;
  border-radius: 16px;
  background: #F8FAF8;
  padding: 36px 24px;
  cursor: pointer;
  transition: all 0.25s ease;
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
}

.upload-zone:hover { border-color: #3A473C; background: #F0F4F0; }
.upload-dragging   { border-color: #3A473C; background: #EEF1EC; border-style: solid; }
.upload-error-border { border-color: #E07070; background: #FFF8F8; }

.upload-zone-icon {
  width: 60px; height: 60px;
  border-radius: 16px;
  background: #EEF1EC;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #3A473C;
  margin-bottom: 6px;
}

.upload-titulo { font-size: 15px; font-weight: 700; color: #3A473C; margin: 0; }
.upload-sub    { font-size: 13px; color: #6C756D; margin: 0; }
.upload-link   { color: #3A473C; font-weight: 700; text-decoration: underline; cursor: pointer; }
.upload-tipos  { font-size: 11px; color: #9CA8A0; font-weight: 600; letter-spacing: 0.3px; margin: 4px 0 0; }

/* Vista previa de archivo */
.upload-preview-card {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px 18px;
  border-radius: 14px;
  border: 1.5px solid #D4DED5;
  background: #F8FAF8;
  position: relative;
}

.preview-img-wrap {
  width: 60px; height: 60px;
  border-radius: 10px;
  overflow: hidden;
  border: 1px solid #D4DED5;
  flex-shrink: 0;
}

.preview-img-thumb { width: 100%; height: 100%; object-fit: cover; }

.preview-pdf-wrap {
  width: 60px; height: 60px;
  border-radius: 10px;
  background: #FDECEC;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.preview-info { flex: 1; display: flex; flex-direction: column; gap: 4px; min-width: 0; }
.preview-nombre { font-size: 14px; font-weight: 600; color: #3A473C; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.preview-tamano { font-size: 12px; color: #6C756D; }

.btn-cambiar {
  font-size: 12px;
  font-weight: 700;
  color: #3A473C;
  background: none;
  border: 1px solid #D4DED5;
  border-radius: 8px;
  padding: 4px 10px;
  cursor: pointer;
  transition: 0.2s;
  width: fit-content;
  font-family: inherit;
}

.btn-cambiar:hover { border-color: #3A473C; background: #EEF1EC; }

.btn-remover-x {
  position: absolute;
  top: 10px; right: 10px;
  width: 26px; height: 26px;
  border-radius: 50%;
  border: none;
  background: #F4F6F4;
  color: #6C756D;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: 0.2s;
}

.btn-remover-x:hover { background: #B93C3C; color: white; }

/* Textarea count */
.textarea-count { font-size: 11px; color: #9CA8A0; text-align: right; }

/* Footer del modal */
.modal-form-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding-top: 20px;
  border-top: 1px solid #F0F4F0;
  margin-top: 20px;
}

.btn-cancelar {
  padding: 13px 24px;
  border-radius: 12px;
  border: 1.5px solid #EAEEEA;
  background: #F6F8F6;
  color: #6C756D;
  font-size: 14px;
  font-weight: 700;
  cursor: pointer;
  transition: 0.2s;
  font-family: inherit;
}

.btn-cancelar:hover { background: #EAEEEA; color: #3A473C; }


.btn-enviar {
  padding: 13px 20px;
  border-radius: 12px;
  border: none;
  background: #3A473C;
  color: white;
  font-size: 14px;
  font-weight: 800;
  cursor: pointer;
  transition: all 0.25s ease;
  display: flex;
  align-items: center;
  gap: 8px;
  font-family: inherit;
}

.btn-enviar:hover:not(:disabled) { background: #2D372F; transform: translateY(-1px); box-shadow: 0 6px 20px rgba(58,71,60,0.25); }
.btn-enviar:disabled { opacity: 0.6; cursor: not-allowed; transform: none; }

.spinner {
  width: 16px; height: 16px;
  border: 2px solid rgba(255,255,255,0.35);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.7s linear infinite;
}

@keyframes spin { to { transform: rotate(360deg); } }

/* Animaciones */
.modal-fade-enter-active { transition: opacity 0.3s ease, transform 0.3s ease; }
.modal-fade-leave-active { transition: opacity 0.2s ease, transform 0.2s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; transform: scale(0.97) translateY(10px); }

.fade-up-enter-active { transition: all 0.4s ease; }
.fade-up-leave-active { transition: all 0.3s ease; }
.fade-up-enter-from, .fade-up-leave-to { opacity: 0; transform: translateY(-10px); }

.fade-hint-enter-active, .fade-hint-leave-active { transition: all 0.25s ease; }
.fade-hint-enter-from, .fade-hint-leave-to { opacity: 0; transform: translateY(-4px); }

/* RESPONSIVE */
@media (max-width: 1100px) {
  .methods-bento { grid-template-columns: 1fr; }
  .accounts-strip { grid-template-columns: 1fr; }
  .account-cell { border-right: none; border-bottom: 1px solid #F1F3F1; }
  .account-cell:last-child { border-bottom: none; }
  .supplies-grid { grid-template-columns: repeat(2, 1fr); }
}

@media (max-width: 768px) {
  .form-row { grid-template-columns: 1fr; }
  .modal-form-header { padding: 20px 20px 16px; }
  .modal-form-body   { padding: 20px 20px 24px; }
}

@media (max-width: 700px) {
  .hero-left h1 { font-size: 48px; }
  .section-header h2, .supply-header h2, .final-box h2 { font-size: 32px; }
  .supplies-grid { grid-template-columns: 1fr; }
  .paypal-card, .registro-cta { padding: 30px; }
  .moneda-toggle { flex-direction: column; }
}

/* ── MOBILE RESPONSIVE ── */
@media (max-width: 768px) {
  .donate-hero {
    height: 320px;
    padding: 0 20px;
  }

  .hero-container {
    padding: 0 20px;
  }

  .hero-left h1 {
    font-size: 36px;
    letter-spacing: -1.5px;
    margin-bottom: 14px;
  }

  .hero-text {
    font-size: 14px;
  }

  .donation-section {
    padding: 40px 0 80px;
  }

  .section-header {
    margin-bottom: 32px;
    padding: 0 16px;
  }

  .section-header h2 {
    font-size: 26px;
  }

  .paypal-card {
    padding: 26px 22px;
    border-radius: 22px;
  }

  .paypal-card h3 { font-size: 22px; }

  .sinpe-card { padding: 26px 22px; border-radius: 22px; }
  .sinpe-number { font-size: 18px; white-space: normal; }

  .account-cell {
    padding: 22px 20px;
  }

  .account-number {
    white-space: normal;
    overflow: visible;
    text-overflow: clip;
  }

  .registro-cta {
    padding: 34px 22px;
    border-radius: 22px;
  }

  .registro-cta h2 { font-size: 24px; }

  .btn-abrir-modal {
    width: 100%;
    justify-content: center;
  }

  .exito-card {
    padding: 18px 16px;
    border-radius: 18px;
  }

  .exito-card-left {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }

  .supplies-section { padding: 56px 0 0; }
  .supply-header h2 { font-size: 22px; }

  .supplies-grid {
    grid-template-columns: 1fr;
    gap: 12px;
  }

  .final-section {
    padding: 56px 20px;
  }

  .final-box h2 {
    font-size: 26px;
    letter-spacing: -0.5px;
    margin-bottom: 12px;
  }

  .final-box p {
    font-size: 14px;
  }

  /* Modal donación */
  .modal-overlay {
    padding: 0;
    align-items: flex-end;
  }

  .modal-form-box {
    border-radius: 24px 24px 0 0;
    max-height: 95vh;
    max-width: 100%;
  }

  .modal-form-header {
    padding: 16px 18px 14px;
    gap: 10px;
  }

  .modal-header-icon {
    width: 40px;
    height: 40px;
    border-radius: 12px;
    flex-shrink: 0;
  }

  .modal-header-text h3 {
    font-size: 16px;
  }

  .modal-header-text p {
    font-size: 12px;
  }

  .modal-form-body {
    padding: 16px 18px 24px;
  }

  .form-row {
    grid-template-columns: 1fr;
    gap: 12px;
  }

  .moneda-toggle {
    flex-direction: row;
    gap: 8px;
  }

  .moneda-btn {
    font-size: 12px;
    padding: 8px 10px;
  }

  .montos-rapidos {
    gap: 5px;
  }

  .monto-chip {
    font-size: 11px;
    padding: 5px 10px;
  }

  .upload-zone {
    padding: 24px 16px;
  }

  .modal-form-footer {
    flex-direction: column;
    gap: 8px;
    padding-top: 16px;
  }

  .btn-cancelar,
  .btn-enviar {
    width: 100%;
    justify-content: center;
  }
}

@media (max-width: 480px) {
  .donate-hero {
    height: 280px;
  }

  .hero-left h1 {
    font-size: 28px;
  }
}
</style>