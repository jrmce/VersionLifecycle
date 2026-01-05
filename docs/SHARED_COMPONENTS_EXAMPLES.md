# Shared Components - Visual Examples

This document shows visual examples of the shared components in use throughout the application.

## SelectInputComponent Examples

### 1. Deployments List - Status Filter
Location: `/deployments`

The status filter dropdown uses the SelectInput component:

```typescript
// Component code
statusOptions: SelectOption[] = [
  { label: 'Pending', value: 'Pending' },
  { label: 'In Progress', value: 'InProgress' },
  { label: 'Success', value: 'Success' },
  { label: 'Failed', value: 'Failed' },
  { label: 'Cancelled', value: 'Cancelled' }
];
```

```html
<app-select-input
  id="status-filter"
  label="Filter by Status"
  [options]="statusOptions"
  [value]="selectedStatus"
  (valueChange)="onStatusChange($event)"
  placeholder="All Statuses"
/>
```

**Features demonstrated:**
- Label with descriptive text
- Placeholder text ("All Statuses")
- Options array with humanized labels
- Value change event handling
- Clean, consistent styling

---

### 2. API Token Create - Expiration Selector
Location: `/api-tokens/create`

The expiration dropdown uses the SelectInput component:

```typescript
// Component code
expirationOptions: SelectOption[] = [
  { label: '30 days', value: 30 },
  { label: '90 days (recommended)', value: 90 },
  { label: '180 days', value: 180 },
  { label: '1 year', value: 365 },
  { label: 'Never (not recommended)', value: null }
];
```

```html
<app-select-input
  id="expiresInDays"
  label="Expires In (Days)"
  [options]="expirationOptions"
  [value]="expiresInDays"
  (valueChange)="onExpirationChange($event)"
  [disabled]="loading"
  helpText="Token expiration helps limit security risks"
/>
```

**Features demonstrated:**
- Label ("Expires In (Days)")
- Help text below the select
- Disabled state (when loading)
- Number and null value support
- Parenthetical hints in option labels

---

## DataTableComponent Example

The DataTable component is ready to be used in list views. Here's an example configuration:

### Applications List (Potential Usage)

```typescript
// Column configuration
columns: TableColumn[] = [
  { key: 'name', label: 'Name' },
  { key: 'description', label: 'Description' },
  { key: 'repositoryUrl', label: 'Repository' },
  { key: 'createdAt', label: 'Created' }
];

// Action buttons
actions: TableAction[] = [
  {
    label: 'Edit',
    callback: (row) => this.router.navigate(['/applications', row.id]),
    class: 'px-3 py-1 bg-gray-100 text-gray-700 rounded hover:bg-gray-200'
  },
  {
    label: 'Delete',
    callback: (row) => this.onDelete(row.id),
    class: 'px-3 py-1 bg-red-100 text-red-700 rounded hover:bg-red-200'
  }
];
```

```html
<app-data-table
  [columns]="columns"
  [data]="applications"
  [actions]="actions"
  [loading]="loading"
  [currentPage]="currentPage"
  [totalPages]="totalPages"
  (previousPage)="onPreviousPage()"
  (nextPage)="onNextPage()"
  loadingMessage="Loading applications..."
  emptyMessage="No applications found."
  emptyActionLabel="Create your first application"
  (emptyAction)="onCreate()"
/>
```

**Features:**
- ✅ Configurable columns with clean headers
- ✅ Responsive table with hover effects
- ✅ Loading spinner with custom message
- ✅ Empty state with icon and call-to-action
- ✅ Pagination with disabled states
- ✅ Action buttons per row
- ✅ Nested property support (e.g., 'user.profile.name')

---

## Styling Consistency

Both components use the application's Tailwind CSS design system:

### Colors
- Primary: `purple-600` / `indigo-600` (buttons, focus rings)
- Success: `green-100` / `green-700` (success actions)
- Warning: `yellow-100` / `yellow-700` (warnings)
- Error: `red-100` / `red-700` (errors, validation)
- Neutral: `gray-50` through `gray-900` (text, borders, backgrounds)

### Border Radius
- Standard: `rounded-lg` (8px)
- Pills: `rounded-full` (for badges)

### Spacing
- Padding: `px-4 py-2` for inputs, `px-6 py-3` for buttons
- Gaps: `space-x-4`, `gap-2` for flex containers

### Focus States
- Focus ring: `focus:ring-2 focus:ring-purple-600 focus:border-transparent`
- Consistent across all interactive elements

---

## Integration Patterns

### Pattern 1: Simple Value Binding
```typescript
selectedValue: string = '';

onValueChange(newValue: string): void {
  this.selectedValue = newValue;
  // Trigger any side effects
}
```

### Pattern 2: With Form Validation
```typescript
hasError(): boolean {
  return this.submitted && !this.selectedValue;
}
```

```html
<app-select-input
  [hasError]="hasError()"
  errorMessage="This field is required"
/>
```

### Pattern 3: Dynamic Options
```typescript
get filteredOptions(): SelectOption[] {
  return this.allOptions.filter(opt => 
    opt.label.toLowerCase().includes(this.searchTerm)
  );
}
```

---

## Testing Examples

Both components have comprehensive unit tests:

### SelectInputComponent Tests
- ✅ Renders correctly
- ✅ Shows label when provided
- ✅ Renders options from array
- ✅ Shows placeholder
- ✅ Displays error message
- ✅ Disables when disabled prop is true
- ✅ Shows required indicator
- ✅ Shows help text

### DataTableComponent Tests
- ✅ Shows loading state
- ✅ Shows empty state
- ✅ Renders table with data
- ✅ Shows pagination controls
- ✅ Disables buttons correctly
- ✅ Accesses nested values
- ✅ Emits events correctly

---

## Benefits Realized

1. **Code Reduction**: ~50 lines of duplicated markup removed per usage
2. **Consistency**: All selects have identical styling and behavior
3. **Accessibility**: Proper label associations, ARIA support built-in
4. **Maintainability**: Changes propagate to all usages
5. **Developer Experience**: Clear TypeScript interfaces, comprehensive docs

---

## Future Enhancements

### SelectInputComponent
- [ ] Add ControlValueAccessor for reactive forms
- [ ] Add multi-select support
- [ ] Add search/filter functionality
- [ ] Add custom option templates

### DataTableComponent
- [ ] Add column sorting
- [ ] Add row selection (checkboxes)
- [ ] Add custom cell templates
- [ ] Add column resizing
- [ ] Add export functionality (CSV, Excel)
- [ ] Add inline editing
