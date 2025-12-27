# Project Tasks

This is a living document tracking ongoing work, planned features, and maintenance tasks for the Version Lifecycle Management application.

## Task Management Workflow

1. **Add Tasks**: When new work is identified, add it to the appropriate section below
2. **Work on Tasks**: Mark tasks as `[IN PROGRESS]` when actively working on them
3. **Complete Tasks**: Mark as `[✓]` when finished
4. **Commit**: After completing each task, commit and push changes to the repository

---

## Current Sprint

### Active Tasks
- [ ] Tests: unit/integration for tenant creation and registration tenant validation
- [ ] Docs: update README/DEVELOPMENT with tenant setup and seeding policy (dev-only demo tenant)

### Completed Tasks (Current Sprint)
- [✓] Environments UI: Tenant-level management interface (Dec 27, 2025)
  - [✓] Created EnvironmentsStore with SignalStore pattern (async/await)
  - [✓] Built list view with inline editing (name, description, order)
  - [✓] Added create form with order field for timeline sequencing
  - [✓] Implemented empty state with create button
  - [✓] Error display with dismissible notifications
  - [✓] Added /environments navigation link
  - [✓] Sorted environments by order property
  - [✓] Full CRUD operations: create, update (inline), delete
- [✓] Admin portal UI for tenant management (Dec 24, 2025)
  - [✓] Create admin route guard (require Admin role)
  - [✓] Tenant list page with search/filter (active status, subscription plan)
  - [✓] Create tenant form (shows generated code after creation)
    - [✓] Edit tenant page (name, description, subscription plan with read-only code display)
  - [✓] Display tenant statistics (user count, application count, version count, deployment count)
  - [✓] Add /admin navigation item (visible only to Admin role)
  - [✓] Backend: add GET /api/tenants/{id}/stats endpoint
  - [✓] Frontend: TenantStore methods for admin operations (create, update, loadStats)
- [✓] Tenant API: admin-only POST /api/tenants (name, description, plan) and GET /api/tenants with active filter (Dec 24, 2025)
- [✓] Registration guard: enforce tenant exists/active; update AuthController + validators to reject invalid TenantId (Dec 24, 2025)
- [✓] Tenant code requirement: add tenant codes/invite tokens and require them during registration (Dec 24, 2025)
- [✓] Frontend registration: fetch tenant list from API, show empty-state if none, submit selected tenant (via TenantStore) (Dec 24, 2025)
- [✓] Add version and environment creation UI (Dec 23, 2025)
  - Added forms to application detail page for creating versions
  - Added forms to application detail page for creating environments
  - Wired up VersionService and EnvironmentService in container
  - Users can now create versions and environments before creating deployments
- [✓] Style application with Tailwind CSS (Dec 23, 2025)
  - ✓ Navigation bar and layout
  - ✓ Authentication pages (login/register)
  - ✓ Dashboard component
  - ✓ Applications list and detail pages
  - ✓ Deployments list, detail, and timeline pages
- [✓] Add Tailwind CSS for frontend styling (Dec 23, 2025)
  - Installed Tailwind CSS v4 with PostCSS and Autoprefixer
  - Converted all SCSS files to CSS (10 files)
  - Updated angular.json to use CSS as default
  - Cleared existing component styles for Tailwind class usage
  - Added Tailwind directives to global styles.css
- [✓] Documentation consolidation (Dec 23, 2025)
  - Created ARCHITECTURE.md with comprehensive design documentation
  - Updated README.md to mark all phases complete
  - Updated DEVELOPMENT.md with .NET 10 and SQLite information
  - Archived obsolete phase documentation to /docs/archive/
  - Added development credentials section to README
- [✓] Added frontend state management documentation to copilot instructions (Dec 23, 2025)
  - Documented SignalStore pattern and all four stores (AuthStore, ApplicationsStore, DeploymentsStore, DashboardStore)
  - Explained presentational/container component pattern
  - Added RxJS interop with rxMethod examples
  - Documented critical rules for effect creation and root-provided stores

---

## Backlog

### High Priority
- [ ] Add unit tests for backend services
- [ ] Add component tests for Angular features
- [ ] Add integration tests for API endpoints
- [ ] Tenant invite/tenant-code flow (optional hardening)

### Medium Priority
- [ ] Implement webhook delivery system (background job)
- [ ] Add email notifications for deployment events
- [ ] Create user management UI for admin role
- [ ] Add deployment rollback functionality

### Low Priority
- [ ] Add dark mode support to frontend
- [ ] Add export/import functionality for configurations
- [ ] Create deployment scheduling feature
- [ ] Add deployment approval workflow for production environments

### Future Enhancements
- [ ] Redis caching for frequently accessed data
- [ ] SignalR for real-time deployment status updates
- [ ] Kubernetes deployment configurations
- [ ] CI/CD pipeline with GitHub Actions
- [ ] Audit logging dashboard
- [ ] Metrics and monitoring integration

---

## Bug Fixes

### Open
- [ ] 

### Resolved
- [✓] Fixed foreign key constraint error during data seeding (Dec 23, 2025)
- [✓] Fixed Angular effect injection context error in applications detail (Dec 23, 2025)
- [✓] Fixed 404 errors for versions/environments endpoints (Dec 23, 2025)
- [✓] Fixed authentication guard redirect to /login (Dec 23, 2025)

---

## Technical Debt

- [ ] Add comprehensive error logging with correlation IDs
- [ ] Implement request/response logging middleware
- [ ] Add health checks for database and external dependencies
- [ ] Create database migration rollback procedures
- [ ] Document API versioning strategy
- [✓] Create pre-PR checklist to enforce frontend patterns (SignalStore usage, no direct service injection in components, container/presentational split)

---

## Notes

- Use `git commit -m "task: <description>"` for task completion commits
- Reference task items in commit messages for traceability
- Review and update this document weekly
- Archive completed sprint tasks monthly to keep document manageable
