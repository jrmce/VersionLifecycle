import { TestBed, ComponentFixture } from '@angular/core/testing';
import { DataTableComponent, TableColumn } from './data-table.component';

describe('DataTableComponent', () => {
  let component: DataTableComponent;
  let fixture: ComponentFixture<DataTableComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DataTableComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DataTableComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should show loading state when loading is true', () => {
    component.loading = true;
    component.loadingMessage = 'Loading data...';
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    const loadingText = compiled.querySelector('.text-gray-600');
    expect(loadingText?.textContent).toContain('Loading data...');
  });

  it('should show empty state when data is empty', () => {
    component.loading = false;
    component.data = [];
    component.emptyMessage = 'No items found';
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    const emptyText = compiled.querySelector('.text-gray-500');
    expect(emptyText?.textContent).toContain('No items found');
  });

  it('should render table with data', () => {
    const columns: TableColumn[] = [
      { key: 'name', label: 'Name' },
      { key: 'age', label: 'Age' },
    ];
    const data = [
      { name: 'John', age: 30 },
      { name: 'Jane', age: 25 },
    ];
    
    component.columns = columns;
    component.data = data;
    component.loading = false;
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    const table = compiled.querySelector('table');
    expect(table).toBeTruthy();
    
    // Check headers
    const headers = compiled.querySelectorAll('th');
    expect(headers.length).toBe(2);
    expect(headers[0].textContent).toContain('Name');
    expect(headers[1].textContent).toContain('Age');
    
    // Check rows
    const rows = compiled.querySelectorAll('tbody tr');
    expect(rows.length).toBe(2);
  });

  it('should show pagination controls when showPagination is true', () => {
    component.data = [{ id: 1 }];
    component.showPagination = true;
    component.currentPage = 0;
    component.totalPages = 2;
    component.loading = false;
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    const paginationButtons = compiled.querySelectorAll('button');
    expect(paginationButtons.length).toBeGreaterThan(0);
  });

  it('should disable previous button on first page', () => {
    component.data = [{ id: 1 }];
    component.showPagination = true;
    component.currentPage = 0;
    component.totalPages = 2;
    component.loading = false;
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    const prevButton = Array.from(compiled.querySelectorAll('button')).find(
      btn => btn.textContent?.includes('Previous')
    ) as HTMLButtonElement;
    expect(prevButton?.disabled).toBe(true);
  });

  it('should disable next button on last page', () => {
    component.data = [{ id: 1 }];
    component.showPagination = true;
    component.currentPage = 1;
    component.totalPages = 2;
    component.loading = false;
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    const nextButton = Array.from(compiled.querySelectorAll('button')).find(
      btn => btn.textContent?.includes('Next')
    ) as HTMLButtonElement;
    expect(nextButton?.disabled).toBe(true);
  });

  it('should get nested value correctly', () => {
    const obj = { user: { name: 'John', address: { city: 'NYC' } } };
    expect(component.getNestedValue(obj, 'user.name')).toBe('John');
    expect(component.getNestedValue(obj, 'user.address.city')).toBe('NYC');
  });

  it('should emit previousPage event when previous button clicked', () => {
    spyOn(component.previousPage, 'emit');
    component.data = [{ id: 1 }];
    component.showPagination = true;
    component.currentPage = 1;
    component.totalPages = 2;
    component.loading = false;
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    const prevButton = Array.from(compiled.querySelectorAll('button')).find(
      btn => btn.textContent?.includes('Previous')
    ) as HTMLButtonElement;
    prevButton.click();
    
    expect(component.previousPage.emit).toHaveBeenCalled();
  });

  it('should emit nextPage event when next button clicked', () => {
    spyOn(component.nextPage, 'emit');
    component.data = [{ id: 1 }];
    component.showPagination = true;
    component.currentPage = 0;
    component.totalPages = 2;
    component.loading = false;
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    const nextButton = Array.from(compiled.querySelectorAll('button')).find(
      btn => btn.textContent?.includes('Next')
    ) as HTMLButtonElement;
    nextButton.click();
    
    expect(component.nextPage.emit).toHaveBeenCalled();
  });
});
