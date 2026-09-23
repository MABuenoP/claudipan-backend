namespace ClaudipanAPI.Helpers;

public static class NotFoundPageHelper
{
    public static string Render(string frontendUrl)
    {
        var cleanUrl = string.IsNullOrWhiteSpace(frontendUrl) ? "/" : frontendUrl.TrimEnd('/');
        var catalogUrl = cleanUrl == "/" ? "/catalog" : $"{cleanUrl}/catalog";
        var year = DateTime.UtcNow.Year;

        return $$"""
        <!DOCTYPE html>
        <html lang="es">
        <head>
          <meta charset="UTF-8" />
          <meta name="viewport" content="width=device-width, initial-scale=1.0" />
          <title>404 - Receta No Encontrada | Claudipan API</title>
          <link rel="preconnect" href="https://fonts.googleapis.com">
          <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
          <link href="https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700;800;900&display=swap" rel="stylesheet">
          <style>
            :root {
              --bg: #FAF6ED;
              --card-bg: rgba(255, 255, 255, 0.94);
              --card-border: rgba(245, 158, 11, 0.4);
              --text-main: #1c1917;
              --text-muted: #57534e;
              --amber-50: #fffbeb;
              --amber-100: #fef3c7;
              --amber-200: #fde68a;
              --amber-500: #f59e0b;
              --amber-600: #d97706;
              --amber-700: #b45309;
              --amber-800: #92400e;
              --orange-500: #f97316;
              --shadow: 0 25px 50px -12px rgba(180, 83, 9, 0.15);
            }

            @media (prefers-color-scheme: dark) {
              :root {
                --bg: #0c0a09;
                --card-bg: rgba(28, 25, 23, 0.94);
                --card-border: rgba(245, 158, 11, 0.25);
                --text-main: #f5f5f4;
                --text-muted: #a8a29e;
                --amber-50: rgba(41, 37, 36, 0.8);
                --amber-100: rgba(120, 53, 15, 0.3);
                --amber-200: rgba(217, 119, 6, 0.2);
                --amber-800: #fde68a;
                --shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.6);
              }
            }

            * {
              box-sizing: border-box;
              margin: 0;
              padding: 0;
              font-family: 'Plus Jakarta Sans', system-ui, -apple-system, sans-serif;
            }

            body {
              background-color: var(--bg);
              color: var(--text-main);
              min-height: 100vh;
              display: flex;
              align-items: center;
              justify-content: center;
              padding: 1.5rem;
              position: relative;
              overflow-x: hidden;
            }

            .ambient-glow-1 {
              position: absolute;
              top: 15%;
              left: 50%;
              transform: translateX(-50%);
              width: 28rem;
              height: 28rem;
              background: radial-gradient(circle, rgba(245, 158, 11, 0.18) 0%, rgba(245, 158, 11, 0) 70%);
              border-radius: 9999px;
              pointer-events: none;
              filter: blur(40px);
            }

            .ambient-glow-2 {
              position: absolute;
              bottom: 10%;
              right: 15%;
              width: 20rem;
              height: 20rem;
              background: radial-gradient(circle, rgba(249, 115, 22, 0.12) 0%, rgba(249, 115, 22, 0) 70%);
              border-radius: 9999px;
              pointer-events: none;
              filter: blur(35px);
            }

            .card {
              position: relative;
              z-index: 10;
              background: var(--card-bg);
              backdrop-filter: blur(16px);
              -webkit-backdrop-filter: blur(16px);
              border: 1px solid var(--card-border);
              border-radius: 1.75rem;
              padding: 2.75rem 2rem;
              max-width: 42rem;
              width: 100%;
              text-align: center;
              box-shadow: var(--shadow);
              animation: fadeIn 0.4s ease-out;
            }

            @keyframes fadeIn {
              from { opacity: 0; transform: translateY(12px); }
              to { opacity: 1; transform: translateY(0); }
            }

            .header-badge-row {
              display: flex;
              align-items: center;
              justify-content: center;
              gap: 1rem;
              margin-bottom: 1.25rem;
            }

            .chef-avatar {
              width: 3.5rem;
              height: 3.5rem;
              border-radius: 1rem;
              background: linear-gradient(135deg, #f59e0b, #d97706);
              display: flex;
              align-items: center;
              justify-content: center;
              color: white;
              box-shadow: 0 8px 16px -4px rgba(245, 158, 11, 0.3);
            }

            .badge-wrap {
              text-align: left;
            }

            .badge {
              display: inline-flex;
              align-items: center;
              gap: 0.4rem;
              padding: 0.3rem 0.8rem;
              border-radius: 9999px;
              font-size: 0.72rem;
              font-weight: 800;
              text-transform: uppercase;
              letter-spacing: 0.05em;
              background: var(--amber-100);
              color: var(--amber-800);
              border: 1px solid var(--card-border);
            }

            .badge-subtitle {
              font-size: 0.75rem;
              color: var(--text-muted);
              font-weight: 600;
              margin-top: 0.2rem;
            }

            .code-404 {
              position: relative;
              margin: 1rem 0;
              user-select: none;
            }

            .code-404 h1 {
              font-size: clamp(5.5rem, 15vw, 8.5rem);
              font-weight: 900;
              line-height: 1;
              letter-spacing: -0.04em;
              background: linear-gradient(135deg, #d97706 0%, #f59e0b 50%, #f97316 100%);
              -webkit-background-clip: text;
              -webkit-text-fill-color: transparent;
            }

            .title {
              font-size: clamp(1.4rem, 3.5vw, 1.85rem);
              font-weight: 800;
              color: var(--text-main);
              line-height: 1.25;
              margin-bottom: 0.85rem;
            }

            .description {
              font-size: 1.05rem;
              color: var(--text-muted);
              line-height: 1.6;
              max-width: 32rem;
              margin: 0 auto 1.5rem auto;
            }

            .oven-box {
              background: var(--amber-50);
              border: 1px solid var(--amber-200);
              border-radius: 1rem;
              padding: 0.85rem 1.25rem;
              font-size: 0.88rem;
              font-weight: 600;
              color: var(--amber-800);
              display: inline-flex;
              align-items: center;
              gap: 0.6rem;
              margin-bottom: 2rem;
              max-width: 32rem;
            }

            .actions {
              display: flex;
              flex-wrap: wrap;
              justify-content: center;
              gap: 0.85rem;
              padding-top: 1.5rem;
              border-top: 1px solid var(--amber-200);
            }

            .btn {
              display: inline-flex;
              align-items: center;
              justify-content: center;
              gap: 0.5rem;
              padding: 0.85rem 1.75rem;
              border-radius: 0.85rem;
              font-size: 0.95rem;
              font-weight: 700;
              text-decoration: none;
              cursor: pointer;
              transition: all 0.2s ease;
              border: none;
            }

            .btn-primary {
              background: linear-gradient(135deg, #d97706, #b45309);
              color: #ffffff;
              box-shadow: 0 10px 20px -5px rgba(217, 119, 6, 0.35);
            }

            .btn-primary:hover {
              background: linear-gradient(135deg, #b45309, #92400e);
              transform: translateY(-2px);
              box-shadow: 0 14px 24px -5px rgba(217, 119, 6, 0.45);
            }

            .btn-outline {
              background: transparent;
              color: var(--text-main);
              border: 1.5px solid var(--card-border);
            }

            .btn-outline:hover {
              background: var(--amber-100);
              border-color: var(--amber-500);
              transform: translateY(-2px);
            }

            .footer-copy {
              margin-top: 2rem;
              font-size: 0.75rem;
              color: var(--text-muted);
              font-weight: 500;
            }

            svg {
              width: 1.2rem;
              height: 1.2rem;
              flex-shrink: 0;
            }
          </style>
        </head>
        <body>
          <div class="ambient-glow-1"></div>
          <div class="ambient-glow-2"></div>

          <div class="card">
            <!-- Header with Claudipan branding -->
            <div class="header-badge-row">
              <div class="chef-avatar">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="width: 2rem; height: 2rem;">
                  <path d="M6 13.87A4 4 0 0 1 7.41 6a5.11 5.11 0 0 1 1.05-1.54 5 5 0 0 1 7.08 0A5.11 5.11 0 0 1 16.59 6 4 4 0 0 1 18 13.87V21H6Z"/>
                  <line x1="6" y1="17" x2="18" y2="17"/>
                </svg>
              </div>
              <div class="badge-wrap">
                <div class="badge">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <circle cx="12" cy="12" r="10"/>
                    <line x1="12" y1="8" x2="12" y2="12"/>
                    <line x1="12" y1="16" x2="12.01" y2="16"/>
                  </svg>
                  Error 404 • Receta No Encontrada
                </div>
                <div class="badge-subtitle">Panadería & Pastelería Claudipan</div>
              </div>
            </div>

            <!-- 404 Giant typography -->
            <div class="code-404">
              <h1>404</h1>
            </div>

            <!-- Exact User Prompt Message -->
            <h2 class="title">
              ¡Upsss! Estás tratando de ir a un archivo que no se encuentra.
            </h2>

            <p class="description">
              Usa nuestro enlace para regresarte a nuestra página de inicio y gracias por tu visita.
            </p>

            <!-- Warm Bakery Oven Note -->
            <div class="oven-box">
              <svg viewBox="0 0 24 24" fill="none" stroke="#d97706" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="m12 3-1.912 5.813a2 2 0 0 1-1.275 1.275L3 12l5.813 1.912a2 2 0 0 1 1.275 1.275L12 21l1.912-5.813a2 2 0 0 1 1.275-1.275L21 12l-5.813-1.912a2 2 0 0 1-1.275-1.275L12 3Z"/>
              </svg>
              <span>Nuestros hornos siguen encendidos con pan fresco y las mejores delicias del día.</span>
            </div>

            <!-- Action Buttons -->
            <div class="actions">
              <a href="{{cleanUrl}}" class="btn btn-primary" id="btn-inicio">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="m3 9 9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/>
                  <polyline points="9 22 9 12 15 12 15 22"/>
                </svg>
                Ir a la Página de Inicio
              </a>

              <a href="{{catalogUrl}}" class="btn btn-outline" id="btn-catalogo">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="m2 7 4.41-4.41A2 2 0 0 1 7.83 2h8.34a2 2 0 0 1 1.42.59L22 7"/>
                  <path d="M4 12v8a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-8"/>
                  <path d="M15 22v-4a2 2 0 0 0-2-2h-2a2 2 0 0 0-2 2v4"/>
                  <path d="M2 7h20"/>
                </svg>
                Ver Catálogo
              </a>
            </div>

            <!-- Polite Footer -->
            <div class="footer-copy">
              © {{year}} Claudipan • SENA ADSO. ¡Esperamos verte pronto de nuevo!
            </div>
          </div>
        </body>
        </html>
        """;
    }
}
