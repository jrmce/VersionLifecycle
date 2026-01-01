# Frontend PR Checklist

Use this checklist before submitting any frontend pull request to ensure compliance with project architecture patterns.

## State Management

- [ ] All state is managed via SignalStore (no local component state for shared data)
- [ ] Stores are provided at root level: `{ providedIn: 'root' }`
- [ ] Async operations use `rxMethod` from `@ngrx/signals/rxjs-interop`
- [ ] State updates use `patchState()` not direct mutation
- [ ] No manual `subscribe()` calls (use `rxMethod` or Angular async pipe)

## Component Architecture

### Presentational Components
- [ ] Accept data via `@Input()` (signals or plain values)
- [ ] Emit events via `@Output()`
- [ ] NO service or store injections (except common utilities like Router if absolutely needed)
- [ ] Easily testable in isolation
- [ ] Named descriptively (e.g., `applications-list.component.ts`)

### Container Components
- [ ] Inject SignalStores for data orchestration
- [ ] Pass store signals to presentational components via inputs
- [ ] Handle `@Output()` events and dispatch store actions
- [ ] Named with `.container.ts` suffix (e.g., `applications-list.container.ts`)
- [ ] Route configuration loads containers, not presentational components

## Signal Usage

- [ ] Templates use `()` syntax to read signals: `store.loading()`, `store.items()`
- [ ] Control flow uses new syntax: `@if`, `@for`, `@switch`
- [ ] Effects created as class fields, not in lifecycle hooks:
  ```typescript
  private myEffect = effect(() => { /* ... */ }); // ✅ Correct
  // NOT: ngOnInit() { effect(() => { /* ... */ }); } // ❌ Wrong
  ```

## Code Quality

- [ ] No `any` types (use proper typing)
- [ ] Const for immutable values, let only when reassignment needed
- [ ] No unused imports or variables
- [ ] Tailwind CSS classes for styling (no inline styles or CSS files)
- [ ] Error handling in place for all async operations
- [ ] Loading states shown to user during data fetching

## Testing

- [ ] Component tests exist and pass
- [ ] Store tests cover all actions/methods
- [ ] Edge cases handled (empty states, errors, loading)

## Documentation

- [ ] Complex logic has explanatory comments
- [ ] Store purpose documented in file header
- [ ] README updated if new patterns introduced
- [ ] New UI components follow [UX Style Guide](UX_STYLE_GUIDE.md)

## Route Configuration

- [ ] Routes use lazy loading: `loadComponent: () => import(...)`
- [ ] Guards use `inject()` pattern for dependencies
- [ ] Route data properly typed

## Performance

- [ ] OnPush change detection strategy where applicable
- [ ] Large lists use trackBy functions
- [ ] No unnecessary re-renders (verified with Angular DevTools)

## Accessibility

- [ ] Form inputs have labels
- [ ] Buttons have descriptive text or aria-labels
- [ ] Keyboard navigation works
- [ ] Color contrast meets WCAG standards

## Design & UX

- [ ] Follows [UX Style Guide](UX_STYLE_GUIDE.md) patterns and conventions
- [ ] Uses approved color palette (purple-600 for primary, semantic colors for status)
- [ ] Typography scale is consistent (text-3xl for h1, text-2xl for h2, etc.)
- [ ] Spacing follows standard scale (4, 6, 8 for common gaps)
- [ ] Components match established patterns (cards, buttons, forms, tables)
- [ ] Loading states, error states, and empty states are properly designed
- [ ] Responsive design works across mobile, tablet, and desktop breakpoints

---

**Reviewer Notes:**
- Verify SignalStore usage - no direct service injection in presentational components
- Check container/presentational split is maintained
- Confirm effects are created in injection context (class fields)
- Look for proper signal syntax in templates: `signal()`

**Common Violations:**
- Injecting services directly into presentational components → Move to container
- Creating effects in `ngOnInit()` → Use class field initialization
- Manual `subscribe()` calls → Use `rxMethod()` or async pipe
- Local component state for shared data → Use SignalStore
