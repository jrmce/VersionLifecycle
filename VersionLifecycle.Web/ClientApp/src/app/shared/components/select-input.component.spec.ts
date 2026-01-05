import { TestBed, ComponentFixture } from '@angular/core/testing';
import { SelectInputComponent, SelectOption } from './select-input.component';

describe('SelectInputComponent', () => {
  let component: SelectInputComponent;
  let fixture: ComponentFixture<SelectInputComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SelectInputComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(SelectInputComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render label when provided', () => {
    component.label = 'Test Label';
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    const label = compiled.querySelector('label');
    expect(label?.textContent).toContain('Test Label');
  });

  it('should render options correctly', () => {
    const options: SelectOption[] = [
      { label: 'Option 1', value: '1' },
      { label: 'Option 2', value: '2' },
    ];
    component.options = options;
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    const selectElement = compiled.querySelector('select');
    const optionElements = selectElement?.querySelectorAll('option');
    
    // Should have 2 options
    expect(optionElements?.length).toBe(2);
  });

  it('should show placeholder when provided', () => {
    component.placeholder = 'Select an option';
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    const firstOption = compiled.querySelector('option');
    expect(firstOption?.textContent).toBe('Select an option');
  });

  it('should show error message when hasError is true', () => {
    component.hasError = true;
    component.errorMessage = 'This field is required';
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    const errorDiv = compiled.querySelector('.text-red-600');
    expect(errorDiv?.textContent).toContain('This field is required');
  });

  it('should disable select when disabled is true', () => {
    component.disabled = true;
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    const selectElement = compiled.querySelector('select') as HTMLSelectElement;
    expect(selectElement.disabled).toBe(true);
  });

  it('should show required indicator when required is true', () => {
    component.label = 'Required Field';
    component.required = true;
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    const requiredSpan = compiled.querySelector('.text-red-500');
    expect(requiredSpan?.textContent).toContain('*');
  });

  it('should show help text when provided and no error', () => {
    component.helpText = 'This is a helpful message';
    component.hasError = false;
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    const helpText = compiled.querySelector('.text-gray-500');
    expect(helpText?.textContent).toContain('This is a helpful message');
  });
});
