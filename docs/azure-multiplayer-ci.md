# Multiplayer deployment to Azure

`.github/workflows/multiplayer.yml` runs separately from the Unity/Itch.io workflow.
It checks out `yusuken64/CardBattleEngine`, runs `GameServer.Test`, publishes the
.NET 8 server with its card data, and uploads the package and test results.
Deployment uses that tested package and checks the live SignalR negotiation endpoint.
The workflow records the server commit in the run summary and package.

Builds run on pushes to this repository's `main`, pull requests changing the workflow,
and manual runs. Deployment runs only on `main`, after tests pass, when the repository
variable `AZURE_MULTIPLAYER_DEPLOY_ENABLED` is `true`.

## One-time Azure setup

The setup was completed on 2026-09-12 for `yusuken64/CardBattleUnityClient`:

- Resource group: `slayqueen-multiplayer`, Central US.
- Web App: `slayqueen-multiplayer-yusuken64`.
- Server URL: `https://slayqueen-multiplayer-yusuken64.azurewebsites.net`.
- Plan: `slayqueen-multiplayer-yusuken64-plan`, Linux Basic B1, one instance.
- Deployment identity: `slayqueen-multiplayer-yusuken64-github`, with Website
  Contributor scoped to this app and federation to `multiplayer-production`.
- GitHub environment, its three Azure secrets, app/resource-group variables, and
  deployment toggle are configured. `MULTIPLAYER_SERVER_REF` uses the default `main`.
  The server repository is public, so no checkout token is needed.

Azure resources are defined in `infra/multiplayer.azure.json`. To reproduce the
resource setup with Azure CLI (creates a paid B1 hosting plan):

```powershell
az group create --name slayqueen-multiplayer --location centralus
az deployment group create --resource-group slayqueen-multiplayer --name multiplayer-setup --template-file infra/multiplayer.azure.json --parameters appName=slayqueen-multiplayer-yusuken64
```

The workflow must be committed and pushed to this repository's `main` before the
first CI deployment can run. Creating the Azure resources does not publish server
code. The existing `SuperBattlerGame` resources are separate from this setup.

For a fresh setup or another repository:

1. Create an Azure App Service **Web App**, using **Code**, **Linux**, and **.NET 8**.
   Choose a **Basic B1** plan for initial playtesting, with **one instance** and no
   scale-out. The workflow expects an existing app; it does not create paid resources.
2. Configure GitHub OpenID Connect using a Microsoft Entra application/service
   principal or a user-assigned managed identity. Grant **Website Contributor** on
   this Web App. Use these federated credential values:

   ```text
   Issuer:   https://token.actions.githubusercontent.com
   Subject:  repo:yusuken64/CardBattleUnityClient:environment:multiplayer-production
   Audience: api://AzureADTokenExchange
   ```

3. Create the GitHub environment `multiplayer-production` in this Unity repository.
   Restrict its deployment branches to `main`. Add these environment secrets:

   | Secret | Value |
   | --- | --- |
   | `AZURE_CLIENT_ID` | Application or managed identity client ID |
   | `AZURE_TENANT_ID` | Azure directory tenant ID |
   | `AZURE_SUBSCRIPTION_ID` | Subscription containing the Web App |

4. Add these GitHub **repository variables** under Settings > Secrets and variables > Actions:

   | Variable | Value |
   | --- | --- |
   | `AZURE_MULTIPLAYER_APP_NAME` | Existing Web App resource name |
   | `AZURE_MULTIPLAYER_RESOURCE_GROUP` | Resource group containing the app |
   | `MULTIPLAYER_SERVER_REF` | Server commit SHA, tag, or branch; defaults to `main` |
   | `AZURE_MULTIPLAYER_DEPLOY_ENABLED` | `true` when ready to enable deployment |

   Prefer a server commit SHA for reproducible releases. Push any local server
   changes to `CardBattleEngine` first; CI cannot deploy uncommitted local files.

5. If `CardBattleEngine` is private, add the **repository secret**
   `CARD_BATTLE_ENGINE_READ_TOKEN`: a fine-grained GitHub token with Contents: Read
   access to that repository. The default workflow token cannot read another private
   repository. Fork pull requests do not receive this secret and cannot build a
   private server repository; use a trusted branch run to validate those changes.
6. Run **Build and Deploy Multiplayer Server** from the Actions tab on `main`.
   The workflow configures Always On, WebSockets, HTTPS-only, and the Linux startup
   command `dotnet GameServer.dll` before deploying.

Microsoft's [App Service GitHub Actions guide](https://learn.microsoft.com/en-us/azure/app-service/deploy-github-actions)
describes OIDC and app-scoped role setup. The
[App Service configuration reference](https://learn.microsoft.com/en-us/azure/app-service/configure-common)
describes the runtime settings used here.

## Releases and client configuration

Changes pushed only to `CardBattleEngine` do not trigger this repository's workflow.
After a server update, set `MULTIPLAYER_SERVER_REF` to the desired revision and run
the workflow manually on `main`, or trigger it with a subsequent client `main` push.
To roll back, select an earlier server revision and run it again.

Desktop builds include `client-config.json` beside the executable (beside the `.app`
bundle on macOS). Edit this file to select a server without rebuilding:

```json
{
  "serverUrl": "https://slayqueen-multiplayer-yusuken64.azurewebsites.net"
}
```

Use the base URL without `/hubs/match`; the client appends that path. For local
testing, use `http://localhost:5299`. `Common.Awake` explicitly initializes its
`NetworkManager` child, which reads the file once and keeps the address for all
multiplayer sessions. Restart the game after editing
the file (restart Play mode in the Editor). In the Unity Editor the
file is read from the project root. The build hook copies that project-root template
beside the executable for both local builds and CI/Itch.io artifacts.

For Editor testing, select `Common/NetworkManager` in `Common.unity` and enable **Use Editor Server
Override** under **Multiplayer testing (Editor only)**. Set **Editor Server Url**
(defaults to `http://localhost:5299`) before entering Play mode. This takes priority
over the config file for that Play session; invalid overrides warn and retain the
configured address. Both fields and the override logic are excluded from player
builds. Leave the toggle disabled to test the normal config-file behavior.

If the file or `serverUrl` property is absent, the client uses the Azure default
in `NetworkClientConfig`. Malformed JSON, unreadable files, or invalid URLs
log a warning and use the same fallback. Config values override both the story-mode
dialog and direct game-scene connections. Rebuild once to include config support;
subsequent server address changes only require editing the file and restarting.

Matches and matchmaking currently live in one server process. Deployments or restarts
lose active matches, and multiple instances require changes to match routing/state
management. The workflow serializes deployments but does not drain active matches.
The smoke check verifies negotiation; use two Unity clients to verify gameplay after
the first deployment. Azure SignalR Service is not required for this configuration.
