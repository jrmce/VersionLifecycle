import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'humanizeStatus',
  standalone: true,
  pure: true,
})
export class HumanizeStatusPipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    if (value == null) return '';
    const s = String(value);
    // Replace underscores and hyphens with spaces
    let result = s.replace(/[_-]+/g, ' ');
    // Insert spaces between camelCase and PascalCase boundaries
    result = result.replace(/([a-z0-9])([A-Z])/g, '$1 $2');
    // Handle sequences of capital letters followed by a capital+lowercase (e.g., APIStatus -> API Status)
    result = result.replace(/([A-Z]+)([A-Z][a-z])/g, '$1 $2');
    return result.trim();
  }
}
