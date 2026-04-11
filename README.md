# .NET Blazor Server — SignalR app on Clever Cloud

> A .NET 8 Blazor Server app using SignalR for real-time UI updates, deployed on Clever Cloud's .NET runtime.

---

## Deploy on Clever Cloud

1. Fork this repository
2. In the Clever Cloud console, create a new **.NET** application — connect your forked repo
3. No add-on needed
4. No environment variables to set manually
5. Push → Clever Cloud builds with `dotnet publish` and deploys automatically

**Configuration file:** `clevercloud/dotnet.json`

```json
{ "deploy": { "project": "cc-dotnet-demo.csproj" } }
```

---

## Stack

| Layer       | Technology         |
|-------------|--------------------|
| Runtime     | .NET 8             |
| Framework   | Blazor Server      |
| Real-time   | SignalR            |
| Design      | Track Night (Bebas Neue, orange #FF5A1F, dark background) |

---

## Features

- Blazor Server with real-time UI updates via SignalR
- Track Night design system
- Responsive layout

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

---

## Environment Variables

| Variable | Required | Description                                   |
|----------|----------|-----------------------------------------------|
| `PORT`   | auto     | Injected by Clever Cloud (default: 8080)      |

No variables need to be set manually.

---

## Deployment Notes

- `UseHttpsRedirection()` is removed — HTTPS is terminated at the Clever Cloud proxy
- `appsettings.json` binds the app to `http://0.0.0.0:8080`
- `clevercloud/dotnet.json` points to the `.csproj` file — required for multi-project repos
- SignalR works out of the box — Clever Cloud supports WebSocket connections
