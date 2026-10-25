# Salle en direct : Blazor Server + SignalR sur Clever Cloud

> App .NET 8 Blazor Server de démonstration, déployée sur le runtime .NET natif de Clever Cloud (sans Dockerfile, sans add-on).
> Chaque navigateur qui ouvre la page apparaît en direct dans une « salle » partagée, reliée à l'instance par sa connexion SignalR.

---

## Ce que montre la page

- **Premier écran** : « N navigateurs en direct. », compteur de mises à jour poussées, battement serveur (une barre par tick),
  journal des connexions/déconnexions/signaux, bouton « Envoyer un signal à tous ». À droite, l'instance Clever Cloud au centre
  et les navigateurs connectés en orbite (un point lumineux part du serveur vers chaque navigateur à chaque tick), puis les
  variables de démo et les variables injectées par la plateforme.
- **Certification Clever Cloud Academy** juste en dessous (bouton d'ancre dès le premier écran) : deux parcours, bouton
  « Passer la certification ».
- **Un clic, un aller-retour SignalR** : la séquence clic → serveur → diff, et pourquoi la démo tourne sur une seule instance.
- `/counter` : compteur InteractiveServer. `/health` : `200 ok`.

Tout est rendu côté serveur : l'état de la salle (`Live/LiveRoom.cs`) vit en mémoire dans le processus, un
`CircuitHandler` (`Live/VisitorCircuit.cs`) voit chaque connexion et chaque coupure de WebSocket, un `BackgroundService`
(`Live/PulseService.cs`) pousse l'état toutes les `PULSE_MS`. Aucun JavaScript métier, aucun package NuGet.

---

## Variables de démo (onglet Variables de la console Clever)

| Variable | Défaut | Effet visible |
|---|---|---|
| `APP_ENV` | `local` | Pastille dans la barre de l'instance ; dégradé Clever si `production` |
| `ANNOUNCEMENT` | vide | Bandeau en haut de toutes les pages ; vide = pas de bandeau |
| `ROOM_NAME` | `Salle de démo` | « Salle : … » au-dessus du titre (60 caractères max) |
| `PULSE_MS` | `1000` | Cadence du battement serveur : vitesse des barres, des points lumineux et de l'horloge. Hors 250–10 000 ou non numérique → `1000` |

Les variables sont lues au démarrage : `clever restart` puis recharger la page.

Variables Clever affichées : `CC_APP_NAME`, `INSTANCE_NUMBER` + `CC_PRETTY_INSTANCE_NAME`, `INSTANCE_TYPE`, `CC_COMMIT_ID`,
`CC_DEPLOYMENT_ID`, `APP_ID` (pastille « Live on Clever Cloud ») ; en local : « injecté au déploiement ».

---

## Déployer sur Clever Cloud

```bash
clever create --type dotnet demo-dotnet-blazor --region par
clever scale --flavor XS --min-instances 1 --max-instances 1     # circuits en mémoire : 1 instance fixe ; nano exigerait un build dédié
clever env set CC_HEALTH_CHECK_PATH /health
clever env set ASPNETCORE_FORWARDEDHEADERS_ENABLED true
clever env set CC_CACHE_DEPENDENCIES true
clever env set APP_ENV production
clever deploy
```

Application en service du 2026-10-25 au 2026-11-05 (suppression planifiée après le tournage vidéo du 2026-11-04) : `app_e06ed3e5-51d5-42cd-a363-60a456060a97`, https://app-e06ed3e5-51d5-42cd-a363-60a456060a97.cleverapps.io/. Après cette date, recréer avec la procédure ci-dessus.

Déployer une modification : `git push` (sauvegarde GitHub) puis `clever deploy`. Pas de liaison GitHub → Clever Cloud : un `git push` seul ne redéploie pas.

| Variable | Valeur | Pourquoi (doc Clever) |
|---|---|---|
| `CC_HEALTH_CHECK_PATH` | `/health` | « If these paths don't return a 2xx response code, the deployment fails. » |
| `CC_CACHE_DEPENDENCIES` | `true` | « Enable caching of your build dependencies to speed up following builds. » (gain faible ici : aucun package NuGet) |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | `true` | Variable ASP.NET Core : `Request.IsHttps` et IP client depuis `X-Forwarded-*` du proxy Clever |
| `CC_DOTNET_PROJ` | `cc-dotnet-demo` | Optionnelle : « If multiple project files are present in your repository » ; un seul `.csproj` ici, détecté automatiquement |

Non nécessaires : `CC_DOTNET_TFM` (un seul framework cible), `CC_DOTNET_PROFILE` (défaut `Release`), `ASPNETCORE_URLS`
(posée par Clever : `http://0.0.0.0:8080`). Pas de ReadyToRun ni de trimming : il faudrait un RuntimeIdentifier et des packs
téléchargés au build, pour une app qui démarre déjà en moins d'une seconde ; le trimming n'est pas supporté par Blazor Server.

**.NET 8 : fin de support le 10 novembre 2026.** La doc Clever accepte `10.0 (LTS)` (`CC_DOTNET_VERSION`). Migration à faire :
`<TargetFramework>net10.0</TargetFramework>` + `clever env set CC_DOTNET_VERSION 10.0`, à tester avec un SDK .NET 10 local.

---

## Lancer en local

```bash
dotnet run -- --urls http://localhost:8103
# avec les variables de démo
APP_ENV=production ROOM_NAME="Tournage" PULSE_MS=600 ANNOUNCEMENT="Maintenance ce soir" dotnet run -- --urls http://localhost:8103
```

Ouvrez la page dans deux onglets : chaque onglet apparaît dans la salle de l'autre.

---

## Structure

```
Program.cs                         → pipeline HTTP, en-têtes de sécurité, /health, services de la salle
Live/DemoSettings.cs               → variables de démo et variables Clever (lues au démarrage)
Live/LiveRoom.cs                   → état partagé : navigateurs, journal, battement, signaux
Live/VisitorCircuit.cs             → CircuitHandler : connexion / coupure du WebSocket de chaque navigateur
Live/PulseService.cs               → battement serveur toutes les PULSE_MS
Components/Shared/LiveBoard.razor  → premier écran (InteractiveServer)
Components/Shared/PlatformPanel.razor → variables de démo et variables Clever
Components/Shared/CleverCert.razor → bloc certification Academy
Components/Pages/Home.razor, Counter.razor, Error.razor
wwwroot/cc-brand.css               → Clever Brand Kit (copie, ne pas modifier)
wwwroot/app.css                    → styles propres à la démo
```
