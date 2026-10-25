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

App **Blazor Server** .NET 8 de démonstration pour Clever Cloud : une « salle en direct ».
Chaque navigateur qui ouvre la page apparaît en temps réel (orbite autour de l'instance), le serveur pousse son battement
toutes les `PULSE_MS`, un bouton envoie un signal à tous les navigateurs. Rendu 100 % serveur via SignalR, pas de WebAssembly,
aucun package NuGet, pas de base de données.

Cible : **Clever Cloud**, runtime .NET natif (aucun Dockerfile). Aucune application n'est maintenue en service : elle se recrée
à la demande depuis le dépôt.

---

## ☁️ Déploiement Clever Cloud

- **Type d'app** : `--type dotnet` ; build `dotnet publish --framework net8.0 --configuration Release` (doc Clever)
- **Sélection du projet** : un seul `.csproj` à la racine, détecté automatiquement ; `CC_DOTNET_PROJ=cc-dotnet-demo` seulement s'il y en a plusieurs. `clevercloud/dotnet.json` supprimé (non documenté)
- **Port** : 8080, `ASPNETCORE_URLS=http://0.0.0.0:8080` posée par la plateforme (identique au `Urls` de `appsettings.json`)
- **TLS** : terminé au reverse proxy Clever ; ni `UseHttpsRedirection()` ni `UseHsts()`
- **Health check** : `GET /health` → `200 ok`
- **Scaling** : **1 instance fixe** (XS ; nano exigerait un build dédié). La salle et les circuits vivent en mémoire : deux instances = deux salles. Ne pas vanter le scaling horizontal

### Variables à poser (citations de la doc Clever)
| Variable | Valeur | Preuve |
|---|---|---|
| `CC_HEALTH_CHECK_PATH` | `/health` | « If these paths don't return a 2xx response code, the deployment fails. » |
| `CC_CACHE_DEPENDENCIES` | `true` | Référence : « Enable caching of your build dependencies to speed up following builds. » (défaut `false` dans la référence, alors que la page .NET dit « cached by default » : poser `true` lève l'ambiguïté ; gain faible, aucun package NuGet) |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | `true` | Variable ASP.NET Core (Microsoft), pas Clever : schéma et IP client depuis `X-Forwarded-*` |
| `CC_DOTNET_PROJ` | `cc-dotnet-demo` | Optionnelle : « If multiple project files are present in your repository » |

Inutiles ici : `CC_DOTNET_TFM` (un seul framework), `CC_DOTNET_PROFILE` (défaut `Release`). Pas de ReadyToRun (RuntimeIdentifier + packs à télécharger au build, démarrage déjà < 1 s à chaud), pas de trimming (non supporté par Blazor Server), pas d'`InvariantGlobalization` (formats fr-FR utilisés).

### Variables de démo (onglet Variables de la console)
| Variable | Défaut | Effet visible |
|---|---|---|
| `APP_ENV` | `local` | Pastille de la barre d'instance ; dégradé Clever si `production` |
| `ANNOUNCEMENT` | vide | Bandeau en haut de toutes les pages (`MainLayout.razor`) |
| `ROOM_NAME` | `Salle de démo` | « Salle : … » au-dessus du titre (coupé à 60 caractères) |
| `PULSE_MS` | `1000` | Cadence du battement serveur (barres, points lumineux, horloge) ; hors 250–10 000 ou invalide → `1000` |

Lues une fois au démarrage (`Live/DemoSettings.cs`) : changer → `clever restart` → recharger.

### Scénario vidéo
1. Ouvrir la page : « 1 navigateur en direct. », le point « vous » en orbite, les barres qui défilent.
2. Ouvrir un 2ᵉ onglet (ou le téléphone) : le titre passe à 2 dans les deux onglets, le journal note « #2 s'est connecté ».
3. Cliquer « Envoyer un signal à tous » : onde depuis l'instance, pic rouge dans les barres, ligne dans le journal de chaque onglet.
4. Fermer un onglet : « s'est déconnecté » immédiatement (coupure du WebSocket, `OnConnectionDownAsync`).
5. Console Clever → Variables : `ROOM_NAME`, `PULSE_MS=300`, `ANNOUNCEMENT` → `clever restart` → recharger : bandeau, nom de salle, battement plus rapide.
6. Bouton « Se certifier Clever Cloud ↓ » → bloc Academy.

Au redémarrage, les circuits sont perdus (état en mémoire) : Blazor propose de recharger, c'est attendu.

---

## 🛠️ Stack

| Élément | Valeur |
|---|---|
| .NET | 8.0 (SDK local 8.0.131 ; .NET 10 non installé localement) |
| Type | Blazor Server (Interactive Server Components) |
| Dépendances NuGet | Aucune |
| Base de données | Aucune |

---

## 📁 Structure clé

```
Program.cs                          → pipeline HTTP, en-têtes de sécurité, /health, services de la salle
Live/DemoSettings.cs                → variables de démo + variables Clever
Live/LiveRoom.cs                    → état partagé en mémoire (navigateurs, journal, battement, signaux)
Live/VisitorCircuit.cs              → CircuitHandler scoped : Join/Leave à la connexion/coupure du WebSocket
Live/PulseService.cs                → BackgroundService, PeriodicTimer(PULSE_MS)
Components/Shared/LiveBoard.razor   → premier écran, rendu InteractiveServer (pré-rendu puis abonnement dans OnAfterRender)
Components/Shared/PlatformPanel.razor → variables de démo + variables Clever
Components/Shared/CleverCert.razor  → bloc certification (badge SVG avec id : une seule inclusion par page)
Components/Pages/                   → Home, Counter, Error
wwwroot/cc-brand.css                → Clever Brand Kit — ne pas modifier
wwwroot/app.css                     → styles propres (préfixe lb-)
```

---

## 🎨 Design

- Kit `cc-brand.css` intact ; styles propres dans `wwwroot/app.css` (préfixe `lb-`), dégradé assombri `#d14836 → #cb1c42 → #a51050` pour le texte blanc.
- Premier écran = le produit : titre « N navigateurs en direct. », orbite des navigateurs autour de l'instance, battement serveur, journal.
- Couleurs des navigateurs : `.lb-hue--0..5` (contraste ≥ 4,5:1 sur navy). `prefers-reduced-motion` coupe pulsations, ondes et rotation du badge.
- Piège Razor : dans `App.razor`, les `@` de l'URL Google Fonts sont échappés en `@@`.

---

## 🚀 Déployer une modification

```bash
git add <fichiers>
git commit -m "description"
clever deploy
```

---

## ⚠️ Points de vigilance

- Ne pas réactiver `UseHttpsRedirection()` / `UseHsts()` (proxy Clever)
- **1 instance** : pas de backplane SignalR ; le compteur « mises à jour poussées » et la salle repartent à zéro à chaque redémarrage
- `Changed` de `LiveRoom` est invoqué hors verrou, chaque composant repasse par `InvokeAsync` ; signaux limités à un toutes les 400 ms par navigateur
- Captures headless : `--virtual-time-budget` ne laisse pas le WebSocket se connecter (seul le pré-rendu apparaît) ; piloter Chrome par CDP pour capturer l'état connecté
- **.NET 8 : fin de support le 10 novembre 2026** — doc Clever : `10.0 (LTS)` accepté via `CC_DOTNET_VERSION` ; migrer `net10.0` dès qu'un SDK 10 est disponible pour tester

---

## 🔍 Diagnostic rapide

| Symptôme | Cause probable | Correction |
|---|---|---|
| Redirect loop | `UseHttpsRedirection()` actif | Le retirer de `Program.cs` |
| Health check en échec | `CC_HEALTH_CHECK_PATH` absent ou faux | `/health` |
| « Connexion en direct… » reste affiché | WebSocket bloqué / circuit non établi | Logs runtime (`clever logs`), console navigateur |
| Deux salles différentes selon l'onglet | Plus d'une instance | `clever scale --min-instances 1 --max-instances 1` |
| Variable de démo sans effet | Lue au démarrage | `clever restart` |
