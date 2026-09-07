# 🧠 Claude.md — demo-dotnet-blazor

## 🏛️ Posture et méthode d'exécution

Tu es un expert cloud senior, rigoureux, structuré et orienté exécution.

Ta mission est de proposer la solution la plus cohérente, la plus pérenne et la plus simple à maintenir, avec une contrainte absolue :
- tout doit être fait exclusivement dans le cloud,
- uniquement via la console cloud,
- sans usage du local,
- sans contournement,
- sans dépendance à un poste développeur,
- sans proposer de manipulation hors plateforme.

Tu dois raisonner avec fermeté : ne propose pas plusieurs pistes floues si une option s'impose clairement. Tu analyses d'abord, tu compares rapidement les options réalistes, puis tu retiens la meilleure approche selon les critères suivants :
1. simplicité d'exploitation,
2. pérennité de l'architecture,
3. facilité d'évolution / upgrade,
4. cohérence technique,
5. faisabilité immédiate dans la console cloud,
6. réduction maximale des risques de blocage.

**Contraintes strictes :**
- ne jamais proposer de solution locale ;
- ne jamais demander d'exécuter une commande sur une machine personnelle ;
- ne jamais recommander un workflow "temporaire" si ce n'est pas industrialisable ;
- ne jamais laisser une réponse au milieu en disant "à toi de voir" ou "choisis parmi ces options" ;
- tu dois trancher et recommander une solution principale ;
- si une idée n'est pas compatible avec une exécution 100 % cloud console, tu l'écartes explicitement ;
- tu privilégies la solution la plus robuste et la plus simple à reprendre plus tard.

**Méthode de réponse obligatoire :**
1. Reformuler brièvement le besoin.
2. Identifier les contraintes bloquantes.
3. Lister les options réellement possibles dans le cadre 100 % cloud console.
4. Écarter clairement les mauvaises options avec justification.
5. Retenir une seule stratégie recommandée.
6. Donner un plan d'exécution concret, ordonné, sans trous.
7. Préciser les points de vigilance.
8. Donner le résultat attendu une fois la mise en place terminée.

**Format attendu :** Réponse structurée, phrases claires, ton ferme, professionnel, décisionnel. Pas de blabla, pas d'hésitation, pas de théorie inutile, pas de proposition hors périmètre.

> Toute recommandation doit être pensée pour être durable, propre techniquement, et directement applicable dans le cloud sans blocage ni dépendance cachée.

---

## 🎯 Contexte du projet

App **Blazor Server** .NET 8 de démonstration pour Clever Cloud.
Page d'accueil « Clever Brand Kit » (héro, bloc certification Clever Cloud Academy, panneau « Vu depuis Clever Cloud »), page `/counter` interactive.
Rendu 100% côté serveur via SignalR — pas de WebAssembly.

Cible de déploiement : **Clever Cloud**, runtime .NET natif (aucun Dockerfile). Aucune application n'est maintenue en service pour cette démo : elle se recrée à la demande depuis le dépôt.

---

## ☁️ Déploiement Clever Cloud

- **Type d'app** : .NET
- **Sélection du projet** : variable `CC_DOTNET_PROJ=cc-dotnet-demo` (mécanisme officiel Clever) ; `clevercloud/dotnet.json` conservé mais non documenté par Clever
- **Port** : 8080 obligatoire — `ASPNETCORE_URLS=http://0.0.0.0:8080` posée par la plateforme (redondant avec `Urls` dans `appsettings.json`, gardé pour le local)
- **TLS** : terminé au niveau du reverse proxy Clever Cloud — l'app ne gère pas le HTTPS ni le HSTS
- **Health check** : `GET /health` → `200 ok` ; déclarer `CC_HEALTH_CHECK_PATH=/health`
- **Scaling** : min = max = 1 instance (circuits Blazor Server en mémoire, pas de backplane SignalR)

### Points de configuration critiques
- `UseHttpsRedirection()` et `UseHsts()` **supprimés** de `Program.cs` — redirect loops / HSTS inopérant derrière le proxy Clever Cloud
- `appsettings.json` bind sur `0.0.0.0:8080` — compatible Clever Cloud
- En-têtes de sécurité émis par un middleware inline dans `Program.cs` : `X-Content-Type-Options: nosniff`, `Referrer-Policy: strict-origin-when-cross-origin`, `X-Frame-Options: DENY` (pas de CSP : à tester en Report-Only avec SignalR)

### Variables d'environnement
| Variable | Valeur | Statut |
|---|---|---|
| `ASPNETCORE_URLS` | `http://0.0.0.0:8080` | posée par Clever Cloud (8080 obligatoire) |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | `true` | **recommandée** — active le middleware ForwardedHeaders côté hôte (`Request.IsHttps`, IP client depuis `X-Forwarded-*`) |
| `ASPNETCORE_ENVIRONMENT` | `Production` | recommandée (défaut si absente) |
| `CC_HEALTH_CHECK_PATH` | `/health` | recommandée |
| `CC_DOTNET_PROJ` | `cc-dotnet-demo` | optionnelle (mécanisme officiel de sélection du projet) |
| `AllowedHosts` | domaines de l'app | optionnelle — `*` par défaut, à restreindre seulement si des URL absolues sont générées |

```bash
clever env set ASPNETCORE_FORWARDEDHEADERS_ENABLED true
clever env set ASPNETCORE_ENVIRONMENT Production
clever env set CC_HEALTH_CHECK_PATH /health
```

---

## 🛠️ Stack

| Élément | Valeur |
|---|---|
| .NET | 8.0 |
| Type | Blazor Server (Interactive Server Components) |
| Dépendances NuGet | Aucune (SDK Web uniquement) |
| Base de données | Aucune |

---

## 📁 Structure clé

```
Program.cs                    → point d'entrée, pipeline HTTP (en-têtes de sécurité, /health)
cc-dotnet-demo.csproj         → fichier projet .NET
Components/Pages/             → pages Blazor (Home, Counter, Error)
Components/Layout/            → MainLayout, CleverTopbar, CleverFooter
Components/Shared/            → CleverLogo, CleverBadge (SVG inline), CleverCert, PlatformPanel
wwwroot/cc-brand.css          → Clever Brand Kit — copié tel quel, ne pas modifier
wwwroot/app.css               → styles spécifiques (compteur, erreur, #blazor-error-ui)
docs/superpowers/specs/       → spec du Clever Brand Kit
appsettings.json              → config (Urls: http://0.0.0.0:8080)
clevercloud/dotnet.json       → indication du .csproj (legacy ; officiel = CC_DOTNET_PROJ)
```

---

## 🎨 Design — Clever Brand Kit

- Système visuel unique partagé par les démos Clever Cloud : `wwwroot/cc-brand.css` (tokens `--cc-*`, `.cc-topbar`, `.cc-hero`, `.cc-cert`, `.cc-platform`, `.cc-card`, `.cc-btn`, `.cc-footer`). Spec : `docs/superpowers/specs/2026-09-06-clever-brand-design.md`.
- Polices : Plus Jakarta Sans + JetBrains Mono (Google Fonts, repli système). Thème navy `#13172e`, dégradé Clever `#f57461 → #cb1c42 → #a51050`.
- Structure de la page d'accueil : topbar → héro → bloc certification → contenu démo + `PlatformPanel` → « Ce que Clever Cloud fait » → footer.
- `PlatformPanel` lit `CC_APP_NAME`, `APP_ID`, `INSTANCE_NUMBER`, `INSTANCE_TYPE`, `CC_PRETTY_INSTANCE_NAME`, `CC_COMMIT_ID`, `CC_DEPLOYMENT_ID` ; `APP_ID` absent → « Local · hors Clever Cloud ».
- Pas de Bootstrap, pas de `.razor.css` isolé, aucun package NuGet : les styles propres à la démo vont dans `wwwroot/app.css`, jamais dans `cc-brand.css`.
- Piège Razor : dans `App.razor`, les `@` de l'URL Google Fonts sont échappés en `@@`.

---

## 🚀 Déployer une modification

```bash
git add .
git commit -m "description"
git push
```

Une fois le dépôt lié à une application .NET, Clever Cloud redéploie automatiquement à chaque push.

---

## ⚠️ Points de vigilance

- **Ne pas réactiver `UseHttpsRedirection()`** — Clever Cloud gère le HTTPS au proxy, l'app ne reçoit que du HTTP en interne
- **Ne pas réactiver `UseHsts()`** — même raison (retiré de `Program.cs`)
- Blazor Server nécessite une connexion SignalR persistante — vérifier que le timeout de la plateforme est suffisant
- **1 instance seulement** : circuits Blazor Server en mémoire, pas de scaling horizontal sans backplane (Redis)
- Le fichier projet s'appelle `cc-dotnet-demo.csproj` — à garder cohérent avec `CC_DOTNET_PROJ` et `clevercloud/dotnet.json`
- **.NET 8 : fin de support le 10 novembre 2026** — planifier la migration `net10.0` (LTS jusqu'en novembre 2028, `CC_DOTNET_VERSION` sur Clever)

---

## 🔍 Diagnostic rapide

| Symptôme | Cause probable | Correction |
|---|---|---|
| Redirect loop | `UseHttpsRedirection()` actif | Vérifier `Program.cs` — doit être absent |
| App non trouvée au build | `.csproj` mal référencé | Poser `CC_DOTNET_PROJ=cc-dotnet-demo` (mécanisme officiel) ; `clevercloud/dotnet.json` n'est qu'un reliquat |
| Page blanche / SignalR KO | Timeout connexion | Vérifier les logs runtime Clever Cloud |
| Port non écouté | `appsettings.json` mal configuré | Doit contenir `"Urls": "http://0.0.0.0:8080"` (ou `ASPNETCORE_URLS` posée) |
| Health check en échec | `CC_HEALTH_CHECK_PATH` absent ou faux | Définir `/health` |
