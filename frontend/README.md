# BlindMatch PAS Frontend

Angular SPA for BlindMatch PAS.

## Scripts

```bash
# Start dev server
npm run start

# Production build
npm run build

# Watch build
npm run watch

# Unit tests (interactive)
npm run test

# Unit tests (CI/headless)
npm run test:ci
```

## Notes

- API base URL is configured in `src/app/core/services/api.config.ts`.
- Scripts include increased Node heap settings to reduce JS heap OOM issues in large builds.
- Role-based routes are defined in `src/app/app.routes.ts`.
