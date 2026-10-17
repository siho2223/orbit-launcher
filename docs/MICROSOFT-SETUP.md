# ORBIT Microsoft authentication — developer setup

Status: protocol code implemented and tested with mocked services; no ORBIT application registration, client ID, or successful live login is available in 0.11.0. Do not advertise this preview as supporting verified Microsoft sign-in.

1. Register your own ORBIT application in Microsoft Entra admin center → App registrations → New registration. Enable personal Microsoft accounts, because Minecraft players use consumer accounts.
2. In Authentication → Advanced settings, enable Allow public client flows for device authorization. This implementation uses the consumers authority and XboxLive.signin/offline_access scopes. It does not use a client secret or require a redirect URI.
3. Copy the Application (client) ID, a public GUID. Never copy Millida's ID. Do not put passwords or client secrets in the launcher, chat, repository or release.
4. Confirm that this new application is permitted to access Minecraft Services. A client ID alone is not proof of permission; an upstream 403 must be resolved through the application's own Microsoft/Minecraft approval process, not another launcher's ID.
5. Build and publish with `-p:OrbitMicrosoftClientId=YOUR_PUBLIC_GUID` passed to dotnet. Players do not enter application credentials in Settings. Then validate an actual owned Minecraft Java account through the Microsoft browser page, ownership/profile exchange, refresh, sign-out and game launch before claiming support.

Only the user-facing device code enters the local WebView. Access and refresh tokens remain native and are stored encrypted with Windows DPAPI CurrentUser under ORBIT/auth/microsoft.bin. Tokens are omitted from the on-disk Java argument file. The Microsoft account's official skin is used in-game; offline local skin injection is disabled for this account type.

References:
- https://learn.microsoft.com/en-us/azure/active-directory/develop/scenario-desktop-app-registration
- https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth2-device-code
- https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.protecteddata
