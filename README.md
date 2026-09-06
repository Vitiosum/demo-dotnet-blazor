# .NET Blazor Server — SignalR app on Clever Cloud

> A .NET 8 Blazor Server app using SignalR for real-time UI updates, deployed on Clever Cloud's .NET runtime — dressed with the Clever Brand Kit and a "Vu depuis Clever Cloud" panel showing what the platform injects.

---

## Deploy on Clever Cloud

1. Fork this repository
2. In the Clever Cloud console, create a new **.NET** application — connect your forked repo
3. No add-on needed
4. No environment variables to set manually
5. Push → Clever Cloud builds with `dotnet publish` and deploys automatically

**Configuration file:** `clevercloud/dotnet.json`

```json
{ "deploy": { "projectFile": "cc-dotnet-demo.csproj" } }
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

---

## Certification Clever Cloud

The home page puts the **Clever Cloud Academy** certification right under the hero: two official tracks (*Cloud Computing Fundamentals*, *Advanced Deployment*) and a direct call to action. The digital badge is issued automatically once a track is validated.

→ https://academy.clever.cloud/

---

## Project structure

```
Program.cs                         → entry point, HTTP pipeline (no HTTPS redirection)
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
clevercloud/dotnet.json            → Clever Cloud deployment config
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

| Variable | Required | Description                                   |
|----------|----------|-----------------------------------------------|
| `PORT`   | auto     | Injected by Clever Cloud (default: 8080)      |

No variables need to be set manually. The platform panel reads the variables Clever Cloud injects on every instance — see the [reference](https://www.clever.cloud/developers/doc/reference/reference-environment-variables/).

---

## Deployment Notes

- `UseHttpsRedirection()` is removed — HTTPS is terminated at the Clever Cloud proxy
- `appsettings.json` binds the app to `http://0.0.0.0:8080`
- `clevercloud/dotnet.json` points to the `.csproj` file — required for multi-project repos
- SignalR works out of the box — Clever Cloud supports WebSocket connections
