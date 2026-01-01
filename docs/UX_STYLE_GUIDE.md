# UX/Design Style Guide

Version Lifecycle Management System  
Last Updated: January 1, 2026

---

## Table of Contents

1. [Design Philosophy](#design-philosophy)
2. [Color System](#color-system)
3. [Typography](#typography)
4. [Spacing & Layout](#spacing--layout)
5. [Components](#components)
6. [Dependencies](#dependencies)
7. [Implementation Best Practices](#implementation-best-practices)
8. [Accessibility Guidelines](#accessibility-guidelines)
9. [Responsive Design](#responsive-design)
10. [Animation & Transitions](#animation--transitions)

---

## Design Philosophy

### Core Principles

**1. Clarity Over Cleverness**
- Prioritize user comprehension over visual complexity
- Use clear, concise labels and descriptive feedback messages
- Avoid jargon; use plain language where possible

**2. Progressive Disclosure**
- Show essential information first, reveal details on demand
- Use expandable sections, modals, and drill-down navigation
- Prevent information overload on initial views

**3. Consistency**
- Maintain uniform patterns across all features
- Reuse established components and patterns
- Follow the same interaction models throughout the application

**4. Feedback & Guidance**
- Provide immediate visual feedback for all user actions
- Show loading states during async operations
- Display clear success/error messages with actionable guidance

**5. Performance as UX**
- Fast load times and smooth interactions are non-negotiable
- Use optimistic UI updates where appropriate
- Lazy load resources to improve perceived performance

### Design Goals

- **Enterprise-Ready**: Professional appearance suitable for B2B SaaS
- **Developer-Friendly**: Technical users expect efficiency and detail
- **Scalable**: Design system that grows with application complexity
- **Accessible**: WCAG 2.1 AA compliance minimum

---

## Color System

### Brand Colors

Our color palette is built on purple and indigo as primary brand colors, conveying trust, reliability, and innovation.

#### Primary Colors
```css
/* Purple Palette */
purple-50:  #faf5ff   /* Backgrounds, subtle highlights */
purple-100: #f3e8ff   /* Hover states on light backgrounds */
purple-200: #e9d5ff   /* Borders, dividers */
purple-300: #d8b4fe   /* Disabled states */
purple-400: #c084fc   /* Secondary actions */
purple-500: #a855f7   /* Icons, secondary buttons */
purple-600: #9333ea   /* Primary brand color, main CTAs */
purple-700: #7e22ce   /* Hover states on primary buttons */
purple-800: #6b21a8   /* Active states */
purple-900: #581c87   /* Text on light backgrounds */

/* Indigo Palette (Complementary) */
indigo-600: #4f46e5   /* Accent color, gradients */
indigo-700: #4338ca   /* Hover states for accents */
```

#### Semantic Colors
```css
/* Success (Operations succeeded) */
green-50:  #f0fdf4   /* Background for success messages */
green-600: #16a34a   /* Success text, icons, borders */
green-700: #15803d   /* Hover states */

/* Warning (Attention needed) */
yellow-50:  #fffbeb  /* Background for warnings */
yellow-600: #ca8a04  /* Warning text, icons */
yellow-700: #a16207  /* Hover states */

/* Error (Operations failed) */
red-50:  #fef2f2     /* Background for error messages */
red-600: #dc2626     /* Error text, icons, borders */
red-700: #b91c1c     /* Hover states, destructive actions */

/* Info (Neutral information) */
blue-50:  #eff6ff    /* Background for info messages */
blue-600: #2563eb    /* Info text, icons */
blue-700: #1d4ed8    /* Hover states */
```

#### Neutral Colors (Grays)
```css
/* Gray Palette - UI foundation */
gray-50:  #f9fafb   /* Page backgrounds */
gray-100: #f3f4f6   /* Card backgrounds, table headers */
gray-200: #e5e7eb   /* Borders, dividers */
gray-300: #d1d5db   /* Input borders, disabled borders */
gray-400: #9ca3af   /* Placeholder text, disabled text */
gray-500: #6b7280   /* Secondary text, icons */
gray-600: #4b5563   /* Body text */
gray-700: #374151   /* Emphasized text */
gray-800: #1f2937   /* Headings */
gray-900: #111827   /* Primary headings, important text */
```

### Color Usage Guidelines

**Do:**
- Use `purple-600` for primary CTAs (Create, Save, Submit)
- Use semantic colors for status indicators (green=success, red=error, yellow=warning)
- Use gray-900 for headings, gray-600 for body text
- Create gradients with `from-purple-600 to-indigo-600` for premium feel
- Use `bg-gray-50` for page backgrounds, `bg-white` for cards

**Don't:**
- Mix purple-600 and red-600 in the same context (confusing)
- Use bright colors for body text (readability issues)
- Apply color without semantic meaning
- Override default text colors without good reason

---

## Typography

### Font Stack

```css
/* System Font Stack (Tailwind Default) */
font-family: ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, 
             "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
```

**Rationale**: Native system fonts provide optimal rendering, familiarity, and performance across platforms.

### Type Scale

```css
/* Headings */
text-3xl: 30px / 36px   /* Page titles (h1) */
text-2xl: 24px / 32px   /* Section headings (h2) */
text-xl:  20px / 28px   /* Sub-section headings (h3) */
text-lg:  18px / 28px   /* Emphasized content (h4) */

/* Body Text */
text-base: 16px / 24px  /* Primary body text */
text-sm:   14px / 20px  /* Secondary text, table cells */
text-xs:   12px / 16px  /* Labels, captions, timestamps */
```

### Font Weights

```css
font-normal:    400  /* Body text */
font-medium:    500  /* Buttons, emphasized inline text */
font-semibold:  600  /* Card titles, table headers */
font-bold:      700  /* Page headings */
```

### Typography Patterns

**Page Titles**
```html
<h1 class="text-3xl font-bold text-gray-900 mb-2">Dashboard</h1>
<p class="text-gray-600">Welcome back! Here's an overview...</p>
```

**Section Headings**
```html
<h2 class="text-2xl font-semibold text-gray-900 mb-6">Recent Deployments</h2>
```

**Card Titles**
```html
<h3 class="text-lg font-semibold text-gray-900 mb-2">{{ application.name }}</h3>
```

**Body Text**
```html
<p class="text-sm text-gray-600">{{ description }}</p>
```

**Labels**
```html
<label class="block text-sm font-medium text-gray-700 mb-2">Email Address</label>
```

---

## Spacing & Layout

### Spacing Scale

Use Tailwind's spacing scale consistently:

```css
/* Common spacing values */
1:  4px    /* Tight spacing between related elements */
2:  8px    /* Default gap in flex/grid layouts */
3:  12px   /* Small padding inside components */
4:  16px   /* Standard padding, gaps */
6:  24px   /* Section spacing */
8:  32px   /* Large section breaks */
12: 48px   /* Major section divisions */
16: 64px   /* Page-level spacing */
```

### Layout Containers

**Page Container** (max-width with responsive padding)
```html
<div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
  <!-- Page content -->
</div>
```

**Responsive Grid**
```html
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
  <!-- Grid items -->
</div>
```

**Card Layout**
```html
<div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
  <!-- Card content -->
</div>
```

### Responsive Breakpoints

```css
/* Tailwind breakpoints */
sm:  640px   /* Small tablets, large phones */
md:  768px   /* Tablets */
lg:  1024px  /* Laptops, desktops */
xl:  1280px  /* Large desktops */
2xl: 1536px  /* Extra large screens */
```

**Usage Pattern**:
```html
<!-- Mobile-first: 1 column on mobile, 2 on tablet, 3 on desktop -->
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
```

---

## Components

### Buttons

#### Primary Button
```html
<button class="px-4 py-2 bg-purple-600 text-white rounded-lg 
               hover:bg-purple-700 transition-colors font-medium text-sm">
  Create Application
</button>
```

#### Primary Button (Gradient)
```html
<a class="px-4 py-2 bg-linear-to-r from-purple-600 to-indigo-600 text-white 
          rounded-lg hover:from-purple-700 hover:to-indigo-700 transition-all 
          shadow-md font-medium">
  + New Application
</a>
```

#### Secondary Button
```html
<button class="px-4 py-2 bg-gray-100 text-gray-700 rounded-lg 
               hover:bg-gray-200 transition-colors font-medium text-sm">
  Cancel
</button>
```

#### Destructive Button
```html
<button class="px-3 py-1 bg-red-100 text-red-700 rounded 
               hover:bg-red-200 transition-colors font-medium">
  Delete
</button>
```

#### Button States
- **Normal**: Default appearance
- **Hover**: Darker shade (e.g., `purple-600` → `purple-700`)
- **Active**: Pressed appearance (can use `active:` prefix)
- **Disabled**: `disabled:opacity-50 disabled:cursor-not-allowed`

### Forms

#### Text Input
```html
<div class="mb-4">
  <label for="name" class="block text-sm font-medium text-gray-700 mb-2">
    Application Name
  </label>
  <input
    id="name"
    type="text"
    class="w-full px-4 py-3 border border-gray-300 rounded-lg 
           focus:ring-2 focus:ring-purple-600 focus:border-transparent transition-all"
    [class.border-red-500]="hasError"
    placeholder="My Application"
  />
  @if (hasError) {
    <div class="mt-1 text-sm text-red-600">
      <span>This field is required</span>
    </div>
  }
</div>
```

#### Select Dropdown
```html
<label class="block text-sm font-medium text-gray-700 mb-2">Environment</label>
<select class="w-full px-4 py-3 border border-gray-300 rounded-lg 
               focus:ring-2 focus:ring-purple-600 focus:border-transparent transition-all">
  <option value="">Select environment...</option>
  <option value="prod">Production</option>
  <option value="staging">Staging</option>
</select>
```

#### Form Validation
- Show error states on submit or after user leaves field
- Use red border (`border-red-500`) for invalid fields
- Display error message below field in `text-red-600`
- Mark required fields with visual indicator

### Cards

#### Standard Card
```html
<div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
  <h3 class="text-lg font-semibold text-gray-900 mb-2">Card Title</h3>
  <p class="text-gray-600 text-sm mb-4">Card description text...</p>
  <a href="#" class="text-purple-600 hover:text-purple-700 font-medium">
    View Details →
  </a>
</div>
```

#### Interactive Card (Hoverable)
```html
<div class="border border-gray-200 rounded-lg p-6 
            hover:shadow-lg transition-shadow cursor-pointer">
  <!-- Card content -->
</div>
```

### Tables

#### Standard Data Table
```html
<div class="overflow-x-auto">
  <table class="min-w-full divide-y divide-gray-200">
    <thead class="bg-gray-50">
      <tr>
        <th class="px-6 py-3 text-left text-xs font-medium 
                   text-gray-500 uppercase tracking-wider">
          Name
        </th>
        <!-- More headers -->
      </tr>
    </thead>
    <tbody class="bg-white divide-y divide-gray-200">
      @for (item of items; track item.id) {
        <tr class="hover:bg-gray-50 transition-colors">
          <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
            {{ item.name }}
          </td>
          <!-- More cells -->
        </tr>
      }
    </tbody>
  </table>
</div>
```

### Alerts & Notifications

#### Error Alert
```html
<div class="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700">
  {{ errorMessage }}
</div>
```

#### Success Alert
```html
<div class="mb-6 p-4 bg-green-50 border border-green-200 rounded-lg text-green-700">
  Operation completed successfully!
</div>
```

#### Info Alert
```html
<div class="mb-6 p-4 bg-blue-50 border border-blue-200 rounded-lg text-blue-700">
  <span class="font-medium">Note:</span> Additional information here.
</div>
```

### Loading States

#### Spinner
```html
<div class="flex items-center justify-center py-12">
  <div class="text-center">
    <div class="inline-block animate-spin rounded-full h-12 w-12 
                border-b-2 border-purple-600 mb-4"></div>
    <p class="text-gray-600">Loading...</p>
  </div>
</div>
```

#### Skeleton Loaders
For table rows or card placeholders during initial load:
```html
<div class="animate-pulse">
  <div class="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
  <div class="h-4 bg-gray-200 rounded w-1/2"></div>
</div>
```

### Empty States

#### Standard Empty State
```html
<div class="text-center py-16 bg-white rounded-xl shadow-sm border border-gray-200">
  <svg class="mx-auto h-16 w-16 text-gray-400 mb-4" fill="none" 
       viewBox="0 0 24 24" stroke="currentColor">
    <!-- Icon path -->
  </svg>
  <p class="text-gray-500 text-lg">No applications found.</p>
  <a href="#" class="inline-block mt-4 text-purple-600 hover:text-purple-700 
                     font-medium">
    Create your first application →
  </a>
</div>
```

### Status Badges

#### Deployment Status
```html
<!-- Success -->
<span class="px-2 py-1 bg-green-100 text-green-800 rounded-full text-xs font-medium">
  Completed
</span>

<!-- Pending -->
<span class="px-2 py-1 bg-yellow-100 text-yellow-800 rounded-full text-xs font-medium">
  Pending
</span>

<!-- Failed -->
<span class="px-2 py-1 bg-red-100 text-red-800 rounded-full text-xs font-medium">
  Failed
</span>

<!-- In Progress -->
<span class="px-2 py-1 bg-blue-100 text-blue-800 rounded-full text-xs font-medium">
  In Progress
</span>
```

### Pagination

```html
<div class="px-6 py-4 bg-gray-50 border-t border-gray-200 
            flex items-center justify-between">
  <button 
    (click)="onPreviousPage()" 
    [disabled]="currentPage === 0"
    class="px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium 
           text-gray-700 hover:bg-gray-100 disabled:opacity-50 
           disabled:cursor-not-allowed transition-colors"
  >
    ← Previous
  </button>
  <span class="text-sm text-gray-600">
    Page <span class="font-semibold">{{ currentPage + 1 }}</span> 
    of <span class="font-semibold">{{ totalPages || 1 }}</span>
  </span>
  <button 
    (click)="onNextPage()" 
    [disabled]="currentPage >= totalPages - 1"
    class="px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium 
           text-gray-700 hover:bg-gray-100 disabled:opacity-50 
           disabled:cursor-not-allowed transition-colors"
  >
    Next →
  </button>
</div>
```

---

## Dependencies

### Core UI Framework

**TailwindCSS v4.1.18**
- Utility-first CSS framework
- PostCSS plugin architecture (v4+ uses CSS-first config)
- JIT compilation for optimal bundle size

**Installation**:
```bash
npm install tailwindcss@^4.1.18 @tailwindcss/postcss@^4.1.18 postcss@^8.5.6
```

**Configuration**: Tailwind v4 uses `@import` in CSS rather than JS config files. See `src/styles.css`:
```css
@import "tailwindcss";

@theme {
  /* Custom theme extensions go here if needed */
}
```

### Icon Library

**Heroicons** (recommended, matches Tailwind design language)
- SVG icons designed by Tailwind Labs
- Two styles: outline (default) and solid
- Copy SVG code directly into templates

**Alternative**: FontAwesome, Material Icons (must import separately)

**Usage Example**:
```html
<svg class="h-6 w-6 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
        d="M12 4v16m8-8H4" />
</svg>
```

**Resources**:
- [Heroicons](https://heroicons.com)
- [Tabler Icons](https://tabler.io/icons) (alternative)

### Angular Version

**Angular 21.0.0**
- Standalone components (no NgModules)
- Signals API for reactive state
- New control flow syntax (`@if`, `@for`, `@switch`)

### State Management

**NgRx Signals 21.0.1**
- Lightweight signal-based state management
- Used via `signalStore()` pattern
- See [FRONTEND_PR_CHECKLIST.md](FRONTEND_PR_CHECKLIST.md) for usage patterns

---

## Implementation Best Practices

### 1. Use Utility Classes, Not Custom CSS

**Do:**
```html
<button class="px-4 py-2 bg-purple-600 text-white rounded-lg 
               hover:bg-purple-700 transition-colors">
  Submit
</button>
```

**Don't:**
```html
<!-- Avoid inline styles -->
<button style="background: purple; padding: 8px 16px;">Submit</button>

<!-- Avoid custom CSS classes -->
<button class="custom-button">Submit</button>
```

**Rationale**: Tailwind utilities are optimized, purged in production, and maintain consistency.

### 2. Component Composition Over Duplication

If you're repeating the same markup pattern, extract it into a reusable component:

```typescript
// shared/components/status-badge.component.ts
@Component({
  selector: 'app-status-badge',
  standalone: true,
  template: `
    <span [class]="badgeClass">{{ label }}</span>
  `
})
export class StatusBadgeComponent {
  @Input() status!: 'success' | 'warning' | 'error' | 'info';
  @Input() label!: string;

  get badgeClass(): string {
    const baseClass = 'px-2 py-1 rounded-full text-xs font-medium';
    const variants = {
      success: 'bg-green-100 text-green-800',
      warning: 'bg-yellow-100 text-yellow-800',
      error: 'bg-red-100 text-red-800',
      info: 'bg-blue-100 text-blue-800'
    };
    return `${baseClass} ${variants[this.status]}`;
  }
}
```

### 3. Maintain Container/Presentational Pattern

**Presentational components** (pure display):
- Accept data via `@Input()`
- Emit events via `@Output()`
- No service/store injections
- Tailwind classes for all styling

**Container components** (orchestration):
- Inject SignalStore
- Handle business logic
- Pass data to presentational components
- Handle events from presentational components

See [FRONTEND_PR_CHECKLIST.md](FRONTEND_PR_CHECKLIST.md) for detailed requirements.

### 4. Responsive Design First

Always consider mobile → tablet → desktop progression:

```html
<!-- Mobile: stack vertically, Desktop: side-by-side -->
<div class="flex flex-col md:flex-row gap-4">
  <div class="w-full md:w-1/2">Left content</div>
  <div class="w-full md:w-1/2">Right content</div>
</div>
```

### 5. Semantic HTML

Use proper HTML5 elements:

```html
<!-- Good: semantic -->
<main class="max-w-7xl mx-auto">
  <article class="bg-white rounded-lg">
    <header>
      <h2>Article Title</h2>
    </header>
    <section>
      <p>Content...</p>
    </section>
  </article>
</main>

<!-- Bad: divitis -->
<div class="main">
  <div class="article">
    <div class="header">
      <div>Article Title</div>
    </div>
  </div>
</div>
```

### 6. Loading & Error States

**Always** handle these states:
1. **Loading**: Show spinner or skeleton
2. **Error**: Display user-friendly message with retry option
3. **Empty**: Provide guidance and CTA
4. **Success**: Show confirmation feedback

```html
@if (store.loading()) {
  <app-spinner message="Loading applications..." />
}

@if (store.error()) {
  <app-alert type="error" [message]="store.error()" />
}

@if (!store.loading() && store.items().length === 0) {
  <app-empty-state 
    title="No applications found"
    message="Get started by creating your first application."
    ctaText="Create Application"
    (ctaClick)="onCreate()"
  />
}

@if (!store.loading() && store.items().length > 0) {
  <!-- Display data -->
}
```

### 7. Optimize Bundle Size

- Use lazy loading for routes
- Import only needed RxJS operators
- Avoid large icon libraries (inline SVGs instead)
- Use Tailwind's purge in production (automatic in v4)

### 8. Dark Mode Considerations

While not currently implemented, plan for dark mode:

```html
<!-- Use semantic classes that can be themed -->
<div class="bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100">
  Content
</div>
```

Current app uses light mode only, but structure allows future dark mode via Tailwind's `dark:` prefix.

---

## Accessibility Guidelines

### WCAG 2.1 AA Compliance

#### Color Contrast
- **Body text**: Minimum 4.5:1 ratio (gray-600 on white = 7:1 ✓)
- **Large text**: Minimum 3:1 ratio
- **Interactive elements**: Minimum 3:1 ratio for borders/icons

**Tools**: Use [WebAIM Contrast Checker](https://webaim.org/resources/contrastchecker/)

#### Keyboard Navigation

All interactive elements must be keyboard accessible:

```html
<!-- Buttons are natively keyboard accessible -->
<button class="..." (click)="doAction()">Action</button>

<!-- Links for navigation -->
<a routerLink="/path" class="...">Navigate</a>

<!-- Custom elements need tabindex and keyboard handlers -->
<div role="button" tabindex="0" (click)="action()" 
     (keydown.enter)="action()" (keydown.space)="action()">
  Custom Button
</div>
```

**Tab Order**:
- Natural DOM order should be logical
- Use `tabindex="0"` to include in tab order
- Use `tabindex="-1"` to exclude but allow programmatic focus
- Avoid `tabindex > 0` (disrupts natural order)

#### Form Accessibility

**Always** include labels for form inputs:

```html
<!-- Explicit label association -->
<label for="email" class="block text-sm font-medium text-gray-700 mb-2">
  Email Address
</label>
<input id="email" type="email" ... />

<!-- Or implicit (label wraps input) -->
<label class="block">
  <span class="block text-sm font-medium text-gray-700 mb-2">Email</span>
  <input type="email" ... />
</label>
```

**Error Announcements**:
```html
<input 
  id="email" 
  type="email"
  aria-describedby="email-error"
  [attr.aria-invalid]="hasError ? 'true' : null"
/>
@if (hasError) {
  <div id="email-error" role="alert" class="text-red-600 text-sm mt-1">
    Email is required
  </div>
}
```

#### ARIA Attributes

Use sparingly; native HTML is usually better:

```html
<!-- Good: native button -->
<button (click)="close()">Close</button>

<!-- Necessary: custom modal -->
<div role="dialog" aria-labelledby="modal-title" aria-modal="true">
  <h2 id="modal-title">Confirm Delete</h2>
  <!-- Modal content -->
</div>

<!-- Loading state announcement -->
<div role="status" aria-live="polite" [attr.aria-busy]="loading">
  @if (loading) { Loading... }
</div>
```

#### Focus Management

Trap focus in modals, return focus after closing:

```typescript
export class ModalComponent implements OnInit, OnDestroy {
  private previouslyFocused: HTMLElement | null = null;

  ngOnInit() {
    this.previouslyFocused = document.activeElement as HTMLElement;
    // Focus first interactive element in modal
  }

  ngOnDestroy() {
    // Return focus to previously focused element
    this.previouslyFocused?.focus();
  }
}
```

#### Screen Reader Support

- Use `aria-label` for icon-only buttons
- Provide text alternatives for images
- Announce dynamic content changes

```html
<!-- Icon-only button needs label -->
<button aria-label="Delete application" (click)="delete()">
  <svg class="h-5 w-5"><!-- trash icon --></svg>
</button>

<!-- Image with alt text -->
<img src="logo.png" alt="Version Lifecycle logo" />

<!-- Decorative images -->
<svg aria-hidden="true"><!-- decorative icon --></svg>
```

---

## Responsive Design

### Mobile-First Approach

Start with mobile layout, progressively enhance for larger screens:

```html
<!-- Stacks vertically on mobile, horizontal on tablet+ -->
<div class="flex flex-col md:flex-row gap-4">
  <div class="w-full md:w-1/3">Sidebar</div>
  <div class="w-full md:w-2/3">Main content</div>
</div>
```

### Breakpoint Strategy

```
Mobile:  < 640px   (base styles, no prefix)
Tablet:  640-1024px (sm:, md: prefixes)
Desktop: > 1024px  (lg:, xl: prefixes)
```

### Common Responsive Patterns

#### Responsive Grid
```html
<!-- 1 column mobile, 2 tablet, 3 desktop -->
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
```

#### Responsive Padding
```html
<!-- Less padding on mobile, more on desktop -->
<div class="px-4 sm:px-6 lg:px-8 py-8">
```

#### Hide/Show Elements
```html
<!-- Show on mobile only -->
<div class="block md:hidden">Mobile menu</div>

<!-- Hide on mobile, show on tablet+ -->
<div class="hidden md:block">Desktop sidebar</div>
```

#### Responsive Typography
```html
<!-- Smaller heading on mobile, larger on desktop -->
<h1 class="text-2xl md:text-3xl lg:text-4xl font-bold">
```

### Touch Target Size

Minimum 44x44 pixels (Apple) or 48x48 pixels (Material) for touch targets:

```html
<!-- Mobile-friendly button -->
<button class="px-4 py-3 min-h-[44px] text-base ...">
  Tap Me
</button>
```

---

## Animation & Transitions

### Transition Classes

Use Tailwind's transition utilities for smooth interactions:

```html
<!-- Basic transition -->
<button class="transition-colors hover:bg-purple-700">

<!-- Multiple properties -->
<div class="transition-all duration-300 hover:shadow-lg">

<!-- Transform transitions -->
<div class="transform transition-transform hover:scale-105">
```

### Animation Guidelines

**Do:**
- Use subtle transitions (200-300ms) for state changes
- Animate opacity, transform, and background-color
- Provide animations for loading states (spinners)
- Use `transition-all` for hover effects on cards

**Don't:**
- Animate layout properties (width, height) unless necessary
- Use overly long durations (> 500ms feels sluggish)
- Animate multiple properties without `will-change` optimization
- Force animations on reduced-motion users

### Reduced Motion

Respect user preferences:

```html
<div class="transition-all motion-reduce:transition-none">
  <!-- Animated on default, static for users with motion sensitivity -->
</div>
```

### Common Animations

**Spinner (Loading)**:
```html
<div class="animate-spin rounded-full h-8 w-8 border-b-2 border-purple-600"></div>
```

**Fade In (Skeleton)**:
```html
<div class="animate-pulse bg-gray-200 h-4 w-3/4 rounded"></div>
```

**Hover Scale**:
```html
<div class="transform transition-transform hover:scale-105">
```

**Hover Shadow**:
```html
<div class="shadow-sm hover:shadow-lg transition-shadow">
```

---

## Design Checklist

Before marking a UI task complete, verify:

### Visual Design
- [ ] Follows color system (purple-600 for primary actions)
- [ ] Uses correct typography scale and weights
- [ ] Maintains consistent spacing (4, 6, 8 for common gaps)
- [ ] Cards use `rounded-xl shadow-sm border border-gray-200`
- [ ] Buttons have hover/disabled states

### Functionality
- [ ] Loading states display spinners
- [ ] Error states show user-friendly messages
- [ ] Empty states provide guidance and CTAs
- [ ] Forms have proper validation feedback

### Accessibility
- [ ] All form inputs have labels
- [ ] Interactive elements are keyboard accessible
- [ ] Color contrast meets WCAG AA (4.5:1 for text)
- [ ] Icon-only buttons have `aria-label`

### Responsive Design
- [ ] Layout works on mobile (< 640px)
- [ ] Touch targets are minimum 44px height
- [ ] Tables scroll horizontally on small screens
- [ ] Text is readable without zooming

### Performance
- [ ] Uses Tailwind utilities (no inline styles)
- [ ] Components follow container/presentational pattern
- [ ] Images have `alt` attributes
- [ ] Routes are lazy loaded

---

## Resources & References

### Official Documentation
- [TailwindCSS v4 Docs](https://tailwindcss.com/docs)
- [Angular Docs](https://angular.io/docs)
- [NgRx Signals](https://ngrx.io/guide/signals)
- [Heroicons](https://heroicons.com)

### Design Inspiration
- [Tailwind UI](https://tailwindui.com) - Premium component examples
- [Headless UI](https://headlessui.com) - Unstyled accessible components
- [Flowbite](https://flowbite.com) - Tailwind component library

### Accessibility
- [WebAIM Contrast Checker](https://webaim.org/resources/contrastchecker/)
- [WAVE Browser Extension](https://wave.webaim.org/extension/)
- [axe DevTools](https://www.deque.com/axe/devtools/)

### Internal Documentation
- [FRONTEND_PR_CHECKLIST.md](FRONTEND_PR_CHECKLIST.md) - Required patterns
- [ARCHITECTURE.md](ARCHITECTURE.md) - System architecture
- [DEVELOPMENT.md](DEVELOPMENT.md) - Setup guide

---

## Questions or Suggestions?

This style guide is a living document. If you encounter patterns not covered here or have suggestions for improvement:

1. Discuss with the team
2. Create a pull request with proposed changes
3. Update this guide after consensus

**Maintainer**: Frontend Team  
**Last Review**: January 1, 2026
