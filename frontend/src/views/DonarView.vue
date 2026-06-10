<template>

  <NavBar />

  <!-- HERO -->

  <section class="donate-hero">

    <div class="hero-container">

      <div class="hero-left">

        <h1>
          Un pequeño aporte puede
          salvar una gran
          historia
        </h1>

        <p class="hero-text">
          Anhelo Pets funciona únicamente gracias
          al apoyo de personas que aman a los animales.
          No recibimos ayuda del gobierno ni patrocinio
          de grandes empresas.
        </p>

        <p class="hero-text second">
          Cada donación nos ayuda a cubrir rescates,
          alimento, tratamientos veterinarios,
          medicamentos, transporte y cuidado diario
          para perros y gatos que necesitan una
          segunda oportunidad.
        </p>

      </div>

      <div class="hero-right">

        <img
          src="/img-donacion/Dona.JPG"
          alt="Dog"
          class="hero-image"
        >

      </div>

    </div>

  </section>

  <!-- DONACIONES -->

  <section class="donation-section">

    <div class="container">

      <div class="section-header">

        <h2>
          Maneras en las que puedes ayudar
        </h2>

        <p>
          Puedes apoyarnos mediante donaciones,
          transferencias o suministros.
        </p>

      </div>

      <!-- PAYPAL -->

      <div class="paypal-section">

        <div class="paypal-left">

          <h3>
            Donar con PayPal
          </h3>

          <p>
            PayPal es una forma rápida y segura
            de ayudarnos desde cualquier parte
            del mundo.
          </p>

          <p>
            Cada contribución nos ayuda a cubrir
            alimento, rescates, medicamentos
            y tratamientos veterinarios.
          </p>

        </div>

        <div class="paypal-right">

          <div class="paypal-grid">

            <a
              href="https://www.paypal.me/FundacionAnhelo/5"
              target="_blank"
              class="paypal-btn"
            >
              Donar $5
            </a>

            <a
              href="https://www.paypal.me/FundacionAnhelo/10"
              target="_blank"
              class="paypal-btn"
            >
              Donar $10
            </a>

            <a
              href="https://www.paypal.me/FundacionAnhelo/25"
              target="_blank"
              class="paypal-btn"
            >
              Donar $25
            </a>

            <a
              href="https://www.paypal.me/FundacionAnhelo/50"
              target="_blank"
              class="paypal-btn"
            >
              Donar $50
            </a>

          </div>

          <a
            href="https://www.paypal.me/FundacionAnhelo"
            target="_blank"
            class="other-amount"
          >
            Elegir otro monto
          </a>

        </div>

      </div>

      <!-- CUENTAS -->

      <div class="accounts-grid">

        <!-- SINPE -->

        <div class="account-card">

          <h3>
            SINPE Móvil
          </h3>

          <p>
            La forma más sencilla de ayudarnos
            desde Costa Rica.
          </p>

          <div class="info-box">

            <span>
              Número SINPE
            </span>

            <strong>
              +506 8840334
            </strong>

          </div>

          <div class="info-box">

            <span>
              Titular
            </span>

            <strong>
              Shirley Valverde Aguilar
            </strong>

          </div>

        </div>

        <!-- BCR -->

        <div class="account-card">

          <h3>
            Cuenta BCR
          </h3>

          <p>
            Transferencias bancarias
            en colones.
          </p>

          <div class="info-box">

            <span>
              Cuenta IBAN
            </span>

            <strong>
              CR28015202280000590991
            </strong>

          </div>

        </div>

        <!-- USD -->

        <div class="account-card">

          <h3>
            Cuenta en dólares
          </h3>

          <p>
            Para donaciones
            internacionales.
          </p>

          <div class="info-box">

            <span>
              Cuenta USD
            </span>

            <strong>
              CR37015202001328713097
            </strong>

          </div>

        </div>

        <!-- COOPE -->

        <div class="account-card">

          <h3>
            Coopealianza
          </h3>

          <p>
            Otra opción segura
            para apoyarnos.
          </p>

          <div class="info-box">

            <span>
              Cuenta
            </span>

            <strong>
              CR98081300210001059638
            </strong>

          </div>

        </div>

      </div>

      <!-- SUMINISTROS -->

      <div class="supplies-section">

        <div class="supply-header">

          <h2>
            Donar suministros
          </h2>

          <p>
            También puedes ayudarnos donando artículos
            esenciales para perros y gatos rescatados.
            Cada aporte hace una gran diferencia.
          </p>

        </div>

        <div class="supplies-grid">

          <div class="supply-box">

            <h4>
              Alimento
            </h4>

            <p>
              Comida para perros y gatos,
              adultos, cachorros y gatitos.
            </p>

          </div>

          <div class="supply-box">

            <h4>
              Medicamentos
            </h4>

            <p>
              Antipulgas, vitaminas,
              desparasitantes y productos
              veterinarios básicos.
            </p>

          </div>

          <div class="supply-box">

            <h4>
              Accesorios
            </h4>

            <p>
              Correas, arneses,
              juguetes, camas,
              mantas y transportadoras.
            </p>

          </div>

          <div class="supply-box">

            <h4>
              Limpieza
            </h4>

            <p>
              Jabón, desinfectantes,
              bolsas, detergente
              y artículos de limpieza.
            </p>

          </div>

        </div>

      </div>

    </div>

  </section>

  <!-- FINAL -->

  <section class="final-section">

    <div class="final-box">

      <h2>
        Gracias por ayudar
      </h2>

      <p>
        Tu apoyo significa rescates,
        alimento, tratamientos médicos
        y nuevas oportunidades para
        animales que merecen una vida
        llena de amor y seguridad.
      </p>

    </div>

  </section>

  <FooterBar />

</template>

<script setup>
import NavBar from '../components/NavBar.vue'
import FooterBar from '../components/FooterBar.vue'
</script>

<style scoped>

.container {
  max-width: 1180px;
  margin: auto;
  padding: 0 24px;
}

/* HERO */

.donate-hero {

  min-height: 82vh;

  background:
    linear-gradient(
      180deg,
      #F4F6F4 0%,
      #FAFAFA 100%
    );

  display: flex;

  align-items: center;

  padding: 90px 24px 70px;
}

.hero-container {

  max-width: 1220px;

  margin: auto;

  width: 100%;

  display: grid;

  grid-template-columns: 1fr 1fr;

  gap: 70px;

  align-items: center;
}

.hero-left h1 {

  font-size: 76px;

  line-height: 0.92;

  letter-spacing: -4px;

  font-weight: 800;

  color: #2F3B31;

  margin-bottom: 30px;
}

.hero-text {

  font-size: 17px;

  line-height: 1.95;

  color: #667085;

  margin-bottom: 20px;

  max-width: 560px;
}

.hero-image {

  width: 100%;

  height: 650px;

  object-fit: cover;

  border-radius: 40px;

  box-shadow:
    0 24px 60px rgba(0,0,0,0.10);
}

/* SECTION */

.donation-section {

  background: #FAFAFA;

  padding:
    40px 0
    120px;
}

.section-header {

  text-align: center;

  margin-bottom: 60px;
}

.section-header h2 {

  font-size: 58px;

  font-weight: 800;

  color: #2F3B31;

  margin-bottom: 18px;

  letter-spacing: -3px;
}

.section-header p {

  font-size: 17px;

  color: #667085;

  line-height: 1.8;
}

/* PAYPAL */

.paypal-section {

  background:
    linear-gradient(
      135deg,
      #859B87,
      #708472
    );

  border-radius: 40px;

  padding: 55px;

  display: grid;

  grid-template-columns: 1fr 1fr;

  gap: 50px;

  margin-bottom: 34px;
}

.paypal-left h3 {

  font-size: 48px;

  font-weight: 800;

  color: white;

  margin-bottom: 24px;

  line-height: 1;
}

.paypal-left p {

  color:
    rgba(255,255,255,0.85);

  line-height: 1.9;

  font-size: 16px;

  margin-bottom: 18px;
}

.paypal-grid {

  display: grid;

  grid-template-columns: repeat(2,1fr);

  gap: 14px;

  margin-bottom: 18px;
}

.paypal-btn {

  height: 62px;

  border-radius: 18px;

  background:
    rgba(255,255,255,0.14);

  color: white;

  text-decoration: none;

  font-weight: 700;

  display: flex;

  align-items: center;

  justify-content: center;

  transition: 0.25s ease;
}

.paypal-btn:hover {

  background: white;

  color: #2F3B31;
}

.other-amount {

  width: 100%;

  height: 60px;

  border-radius: 18px;

  background: white;

  color: #2F3B31;

  text-decoration: none;

  font-weight: 800;

  display: flex;

  align-items: center;

  justify-content: center;
}

/* ACCOUNTS */

.accounts-grid {

  display: grid;

  grid-template-columns: repeat(4,1fr);

  gap: 22px;

  margin-bottom: 34px;
}

.account-card {

  background: white;

  border-radius: 30px;

  padding: 32px;

  border:
    1px solid rgba(146,168,148,0.10);

  box-shadow:
    0 10px 24px rgba(0,0,0,0.04);
}

.account-card h3 {

  font-size: 26px;

  font-weight: 800;

  color: #2F3B31;

  margin-bottom: 14px;
}

.account-card p {

  font-size: 14px;

  line-height: 1.8;

  color: #667085;

  margin-bottom: 24px;
}

.info-box {

  background: #F8FAF8;

  border-radius: 18px;

  padding: 16px;

  margin-bottom: 14px;
}

.info-box span {

  display: block;

  font-size: 12px;

  font-weight: 700;

  color: #92A894;

  margin-bottom: 8px;
}

.info-box strong {

  color: #2F3B31;

  font-size: 14px;

  line-height: 1.7;

  word-break: break-word;
}

/* SUPPLIES */

.supplies-section {

  background: white;

  border-radius: 40px;

  padding: 80px 60px;

  border:
    1px solid rgba(146,168,148,0.10);

  box-shadow:
    0 12px 30px rgba(0,0,0,0.04);
}

.supply-header {

  text-align: center;

  margin-bottom: 60px;
}

.supply-header h2 {

  font-size: 56px;

  font-weight: 800;

  color: #2F3B31;

  margin-bottom: 18px;

  letter-spacing: -3px;
}

.supply-header p {

  font-size: 17px;

  color: #667085;

  line-height: 1.8;

  max-width: 700px;

  margin: auto;
}

.supplies-grid {

  display: grid;

  grid-template-columns: repeat(4,1fr);

  gap: 22px;
}

.supply-box {

  background: #F8FAF8;

  border-radius: 28px;

  padding: 34px 24px;

  text-align: center;

  border:
    1px solid rgba(146,168,148,0.08);

  transition: 0.25s ease;
}

.supply-box:hover {

  transform: translateY(-6px);

  background: #F4F6F4;

  box-shadow:
    0 14px 30px rgba(0,0,0,0.05);
}

.supply-box h4 {

  font-size: 22px;

  font-weight: 800;

  color: #2F3B31;

  margin-bottom: 14px;
}

.supply-box p {

  font-size: 14px;

  line-height: 1.8;

  color: #667085;
}

/* FINAL */

.final-section {

  background:
    linear-gradient(
      180deg,
      #F4F6F4 0%,
      #EEF2EE 100%
    );

  padding:
    40px 24px
    120px;
}

.final-box {

  max-width: 1150px;

  margin: auto;

  background:
    linear-gradient(
      135deg,
      #859B87,
      #708472
    );

  border-radius: 42px;

  padding: 90px 70px;

  text-align: center;

  overflow: hidden;

  position: relative;

  box-shadow:
    0 24px 60px rgba(0,0,0,0.10);
}

.final-box::before {

  content: '';

  position: absolute;

  width: 420px;

  height: 420px;

  border-radius: 50%;

  background:
    rgba(255,255,255,0.06);

  top: -180px;

  right: -120px;
}

.final-box::after {

  content: '';

  position: absolute;

  width: 260px;

  height: 260px;

  border-radius: 50%;

  background:
    rgba(255,255,255,0.05);

  bottom: -120px;

  left: -60px;
}

.final-box h2 {

  font-size: 68px;

  font-weight: 800;

  line-height: 0.95;

  color: white;

  margin-bottom: 28px;

  letter-spacing: -4px;

  position: relative;

  z-index: 2;
}

.final-box p {

  font-size: 18px;

  line-height: 1.95;

  color:
    rgba(255,255,255,0.86);

  max-width: 760px;

  margin: auto;

  position: relative;

  z-index: 2;
}

/* RESPONSIVE */

@media (max-width: 1100px) {

  .hero-container,
  .paypal-section {

    grid-template-columns: 1fr;
  }

  .accounts-grid {

    grid-template-columns: repeat(2,1fr);
  }

  .supplies-grid {

    grid-template-columns: repeat(2,1fr);
  }

}

@media (max-width: 700px) {

  .hero-left h1 {

    font-size: 48px;
  }

  .section-header h2,
  .supply-header h2,
  .final-box h2 {

    font-size: 40px;
  }

  .paypal-grid,
  .accounts-grid,
  .supplies-grid {

    grid-template-columns: 1fr;
  }

  .paypal-section,
  .supplies-section,
  .final-box {

    padding: 34px;
  }

}
</style>