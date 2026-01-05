# Shared Components

This directory contains reusable UI components that are used throughout the application.

## Components

### SelectInputComponent

A styled dropdown select component with consistent styling across the application.

**Features:**
- Consistent styling with dropdown icon
- Label, placeholder, and help text support
- Error state with custom error messages
- Disabled state
- Required field indicator

**Usage:**

```typescript
import { SelectInputComponent } from '../../../shared/components';
import type { SelectOption } from '../../../shared/components';

// In your component
statusOptions: SelectOption[] = [
  { label: 'Active', value: 'active' },
  { label: 'Inactive', value: 'inactive' }
];
```

```html
<app-select-input
  id="status"
  label="Status"
  [options]="statusOptions"
  [value]="selectedStatus"
  (valueChange)="onStatusChange($event)"
  placeholder="Select a status"
  [disabled]="loading"
  [hasError]="submitted && !selectedStatus"
  errorMessage="Status is required"
  helpText="Choose the current status"
/>
```

**Props:**
- `id` (string) - Unique identifier for the select element
- `label` (string) - Label text displayed above the select
- `placeholder` (string) - Placeholder option text
- `options` (SelectOption[]) - Array of {label, value} objects
- `value` (any) - Currently selected value
- `disabled` (boolean) - Whether the select is disabled
- `required` (boolean) - Shows asterisk indicator
- `hasError` (boolean) - Applies error styling
- `errorMessage` (string) - Error message to display
- `helpText` (string) - Help text below the select

**Events:**
- `valueChange` - Emits when selection changes

---

### DataTableComponent

A data table component with pagination, loading states, and empty states.

**Features:**
- Column configuration
- Pagination controls
- Loading state
- Empty state with custom message
- Action buttons with conditional rendering
- Nested property access (e.g., 'user.name')

**Usage:**

```typescript
import { DataTableComponent } from '../../../shared/components';
import type { TableColumn, TableAction } from '../../../shared/components';

// In your component
columns: TableColumn[] = [
  { key: 'name', label: 'Name' },
  { key: 'email', label: 'Email' },
  { key: 'user.role', label: 'Role' } // Nested property
];

actions: TableAction[] = [
  {
    label: 'Edit',
    callback: (row) => this.onEdit(row),
    class: 'px-3 py-1 bg-blue-100 text-blue-700 rounded hover:bg-blue-200'
  },
  {
    label: 'Delete',
    callback: (row) => this.onDelete(row),
    class: 'px-3 py-1 bg-red-100 text-red-700 rounded hover:bg-red-200',
    showCondition: (row) => row.canDelete // Optional conditional rendering
  }
];
```

```html
<app-data-table
  [columns]="columns"
  [data]="items"
  [actions]="actions"
  [loading]="loading"
  [currentPage]="currentPage"
  [totalPages]="totalPages"
  (previousPage)="onPreviousPage()"
  (nextPage)="onNextPage()"
  loadingMessage="Loading data..."
  emptyMessage="No items found"
  emptyActionLabel="Create first item"
  (emptyAction)="onCreate()"
  [trackBy]="trackById"
/>
```

**Props:**
- `columns` (TableColumn[]) - Array of column definitions
- `data` (any[]) - Array of row data
- `actions` (TableAction[]) - Array of action button definitions
- `loading` (boolean) - Shows loading spinner
- `loadingMessage` (string) - Loading state message
- `emptyMessage` (string) - Empty state message
- `emptyActionLabel` (string) - Empty state action button label
- `emptyStateIcon` (string) - SVG path for empty state icon
- `showPagination` (boolean) - Shows/hides pagination controls
- `currentPage` (number) - Current page (0-indexed)
- `totalPages` (number) - Total number of pages
- `trackBy` (function) - TrackBy function for *ngFor

**Events:**
- `previousPage` - Emits when previous button clicked
- `nextPage` - Emits when next button clicked
- `emptyAction` - Emits when empty state action clicked

**Types:**

```typescript
interface TableColumn {
  key: string;           // Property key or nested path (e.g., 'user.name')
  label: string;         // Column header label
  sortable?: boolean;    // Future: sortable column
  customTemplate?: TemplateRef<any>; // Future: custom cell template
}

interface TableAction {
  label: string;                    // Button label
  callback: (row: any) => void;     // Click handler
  class?: string;                   // Custom CSS classes
  showCondition?: (row: any) => boolean; // Optional: show/hide logic
}

interface SelectOption {
  label: string;  // Display text
  value: any;     // Value
}
```

## Design Philosophy

These components follow the **Container/Presentational** pattern:
- They are **presentational** - accept data via @Input, emit events via @Output
- They have **no service injections** - pure UI components
- They use **consistent Tailwind styling** - match application design
- They are **easily testable** - isolated from business logic

## Examples in Codebase

- **SelectInputComponent**: Used in `deployments-list` (status filter) and `api-token-create` (expiration selector)
- **DataTableComponent**: Ready to use in list views (applications, deployments, environments)
