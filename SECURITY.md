# Security Policy

## Reporting a vulnerability

If you discover a security issue in this portfolio project, please open a private security advisory on GitHub rather than publishing credentials or exploit details in a public issue.

## Secrets

The JWT key committed in `appsettings.json` is a development placeholder only. Real deployments should override it with the `Jwt__Key` environment variable or a managed secret store.

Do not commit production credentials, connection strings containing passwords, access tokens, or private keys.
