# .NET Blazor Server — SignalR app on Clever Cloud

> A .NET 8 Blazor Server app using SignalR for real-time UI updates, deployed on Clever Cloud's .NET runtime — dressed with the Clever Brand Kit and a "Vu depuis Clever Cloud" panel showing what the platform injects.

---

## Deploy on Clever Cloud

1. Fork this repository
2. In the Clever Cloud console, create a new **.NET** application — connect your forked repo
3. No add-on needed
4. Recommended environment variables (see table below): `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`, `ASPNETCORE_ENVIRONMENT=Production`, `CC_HEALTH_CHECK_PATH=/health`
5. Scaling: keep **min = max = 1 instance** (Blazor Server circuits live in memory — no horizontal scaling without a SignalR backplane)
6. Push → Clever Cloud builds with `dotnet publish` and deploys automatically

**Project file selection:** the official Clever Cloud mechanism is the `CC_DOTNET_PROJ` environment variable (project name without extension). The repository also ships `clevercloud/dotnet.json`, kept for compatibility but not documented by Clever Cloud:

```bash
clever env set CC_DOTNET_PROJ cc-dotnet-demo   # optional: single .csproj at the root, auto-detected
```

---

## Stack

| Layer       | Technology         |
|-------------|--------------------|
| Runtime     | .NET 8             |
| Framework   | Blazor Server      |
| Real-time   | SignalR            |
| Design      | Clever Brand Kit (Plus Jakarta Sans, navy #13172e, dégradé Clever) |

---

## Features

- Blazor Server with real-time UI updates via SignalR (`/counter`, `InteractiveServer` render mode)
- Clever Brand Kit: topbar, hero, certification block, platform panel, footer — one CSS file, no Bootstrap, no JS framework
- "Vu depuis Clever Cloud" panel reading the variables injected by the platform (`CC_APP_NAME`, `APP_ID`, `INSTANCE_NUMBER`, `INSTANCE_TYPE`, `CC_PRETTY_INSTANCE_NAME`, `CC_COMMIT_ID`, `CC_DEPLOYMENT_ID`) — shows "Local · hors Clever Cloud" when they are absent
- Responsive layout (desktop and mobile)
- `GET /health` → `200 ok` (plain text) for the Clever Cloud health check
- Security headers on every response: `X-Content-Type-Options: nosniff`, `Referrer-Policy: strict-origin-when-cross-origin`, `X-Frame-Options: DENY` (no CSP yet — to be tested in Report-Only mode with SignalR)

---

## Certification Clever Cloud

The home page puts the **Clever Cloud Academy** certification right under the hero: two official tracks (*Cloud Computing Fundamentals*, *Advanced Deployment*) and a direct call to action. The digital badge is issued automatically once a track is validated.

→ https://academy.clever.cloud/

---

## Project structure

```
Program.cs                         → entry point, HTTP pipeline (no HTTPS redirection, no HSTS), security headers, /health
cc-dotnet-demo.csproj              → project file (no NuGet dependency)
Components/App.razor               → <head>: Google Fonts, cc-brand.css, app.css, favicon
Components/Layout/MainLayout.razor → CleverTopbar + @Body + CleverFooter
Components/Layout/CleverTopbar.razor, CleverFooter.razor
Components/Shared/CleverLogo.razor, CleverBadge.razor   → inline SVG (official logo, certification badge)
Components/Shared/CleverCert.razor  → certification block
Components/Shared/PlatformPanel.razor → "Vu depuis Clever Cloud"
Components/Pages/                  → Home, Counter, Error
wwwroot/cc-brand.css               → Clever Brand Kit (copied as-is, do not edit)
wwwroot/app.css                    → demo-specific styles only
clevercloud/dotnet.json            → project file hint (legacy; official mechanism = CC_DOTNET_PROJ)
docs/superpowers/specs/            → design spec of the Clever Brand Kit
```

---

## Local Development

### Prerequisites

- .NET 8 SDK

### Run

```bash
git clone https://github.com/Vitiosum/demo-dotnet-blazor
cd demo-dotnet-blazor
dotnet run
# → http://localhost:8080
```

Locally the topbar shows **Local** and the platform panel shows **Local · hors Clever Cloud**: the `APP_ID` variable only exists on Clever Cloud.

---

## Environment Variables

| Variable | Status | Description |
|----------|--------|-------------|
| `ASPNETCORE_URLS` | set by Clever Cloud | `http://0.0.0.0:8080` — port 8080 is mandatory on the .NET runtime; overrides the `Urls` value of `appsettings.json` |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | **recommended** = `true` | Enables the ForwardedHeaders middleware on the host: `Request.IsHttps` and the client IP are read from the `X-Forwarded-*` headers set by the Clever Cloud proxy |
| `ASPNETCORE_ENVIRONMENT` | recommended = `Production` | Locks the environment (default when absent is already `Production`) |
| `CC_HEALTH_CHECK_PATH` | recommended = `/health` | Clever Cloud deployment health check on the `/health` endpoint |
| `CC_DOTNET_PROJ` | optional = `cc-dotnet-demo` | Official project selection mechanism (multi-project repos) |
| `AllowedHosts` | optional | Restrict the `Host` header to the app domains if absolute URLs are ever generated (`*` by default) |

```bash
clever env set ASPNETCORE_FORWARDEDHEADERS_ENABLED true
clever env set ASPNETCORE_ENVIRONMENT Production
clever env set CC_HEALTH_CHECK_PATH /health
```

The app never reads `PORT`: the listening address comes from `ASPNETCORE_URLS` (injected) or, locally, from `appsettings.json`. The platform panel reads the variables Clever Cloud injects on every instance — see the [reference](https://www.clever.cloud/developers/doc/reference/reference-environment-variables/).

---

## Deployment Notes

- `UseHttpsRedirection()` and `UseHsts()` are removed — HTTPS and HSTS are handled by the Clever Cloud proxy; the app only receives plain HTTP internally
- `appsettings.json` binds the app to `http://0.0.0.0:8080` (redundant with the injected `ASPNETCORE_URLS`, kept for local runs)
- `CC_DOTNET_PROJ` selects the `.csproj` in multi-project repos (`clevercloud/dotnet.json` is kept but not documented by Clever Cloud)
- SignalR works out of the box — Clever Cloud supports WebSocket connections
- **One instance only**: Blazor Server keeps each circuit in memory; scaling to several instances requires a SignalR backplane (Redis) — not part of this demo
- **.NET 8 end of support: 10 November 2026** — plan the move to `net10.0` (LTS until November 2028, available on Clever Cloud via `CC_DOTNET_VERSION`)
