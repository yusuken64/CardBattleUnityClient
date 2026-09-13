# Steam beta deployment

`.github/workflows/main.yml` builds the Windows client once and shares that artifact
with the Itch.io and Steam jobs. Steam uploads run only for `main`, after the Unity
build succeeds, when repository variable `STEAM_BETA_DEPLOY_ENABLED` is `true`.
Pull requests and other branches never upload to Steam. Manual runs on `main` are
also available from **Build and Publish Client** in GitHub Actions.

The destination is app **4345100**, Windows depot **4345101**, branch **beta**.
These IDs come from the Steam SDK's `app_4345100.vdf` and `depot_4345101.vdf`.
The older `Builds/Steam/slayqueen.vdf` is not used by CI. The upload action generates
its own manifest and excludes debug symbols and Unity's DoNotShip directories.

The executable is named `SlayQueenTheGateKeeper.exe`. The job checks that it, the
managed client assembly, and `client-config.json` exist before uploading. Steam's
launch options must point to this executable. The config contains the Azure server
address. Beta clients receive new builds automatically through Steam; promotion to
the default branch remains a Steamworks portal action.

## Authentication

The GitHub environment **steam-beta** must allow deployments from `main` only and
contain these secrets:

| Secret | Value |
| --- | --- |
| `STEAM_USERNAME` | Steam build account name |
| `STEAM_CONFIG_VDF` | Base64-encoded SteamCMD `config/config.vdf` after Steam Guard login |

Use an account with upload/publish access to this app. The pinned v3 deployment
action supports cached-session login without storing the account password in CI.
Never commit the Steam config or its encoded contents: both are credentials.

When Steam reports an expired login or asks for Steam Guard, refresh the login in
SteamCMD, exit with `quit`, and replace `STEAM_CONFIG_VDF`. Validate the saved login
locally with `steamcmd +login <username> +quit` before copying the session to GitHub.

Set `STEAM_BETA_DEPLOY_ENABLED` to `false` to pause Steam uploads. Upload jobs are
serialized, and a build whose commit is no longer at the tip of `main` is skipped.
The action's Steam Build ID is recorded in the run summary.

References: [GameCI Steam deploy v3](https://github.com/game-ci/steam-deploy/tree/v3),
[Valve's SteamPipe guide](https://partner.steamgames.com/doc/sdk/uploading).
