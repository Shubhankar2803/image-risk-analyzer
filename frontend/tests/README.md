Frontend tests (Jest)

This folder contains a minimal Jest + TypeScript scaffold for frontend tests.

Quick start:

1. From the `frontend` folder, install dev dependencies (recommended to keep deps scoped to frontend):

```bash
cd frontend
npm install --save-dev jest ts-jest @types/jest
```

2. Run tests:

```bash
cd frontend/tests
npx jest
```

Notes and next steps:
- This scaffold provides a working `jest.config.cjs` and a `sample.test.ts` file. Replace or extend with component/service tests that import from `../src/app/...`.
- For Angular projects, consider using `@angular-builders/jest` or `jest-preset-angular` for component testing with templates.
- If you prefer Karma/Jasmine, adapt accordingly and add `ng test` configuration.
