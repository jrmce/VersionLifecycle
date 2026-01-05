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

  describe('ControlValueAccessor', () => {
    it('should implement writeValue correctly', () => {
      const testValue = 'test-value';
      component.writeValue(testValue);
      fixture.detectChanges();
      
      // writeValue encodes the value, so it should match the encoded version
      expect(component.value).toBe(testValue); // String values are returned as-is
      
      const compiled = fixture.nativeElement as HTMLElement;
      const selectElement = compiled.querySelector('select') as HTMLSelectElement;
      expect(selectElement.value).toBe(testValue);
    });

    it('should handle null value in writeValue', () => {
      component.writeValue(null);
      fixture.detectChanges();
      
      // Null values are converted to empty string by encodeValue
      expect(component.value).toBe('');
    });

    it('should handle undefined value in writeValue', () => {
      component.writeValue(undefined);
      fixture.detectChanges();
      
      // Undefined values are converted to empty string by encodeValue
      expect(component.value).toBe('');
    });

    it('should register onChange callback', () => {
      const onChangeSpy = jasmine.createSpy('onChange');
      component.registerOnChange(onChangeSpy);
      
      const options: SelectOption[] = [
        { label: 'Option 1', value: '1' },
        { label: 'Option 2', value: '2' },
      ];
      component.options = options;
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      const selectElement = compiled.querySelector('select') as HTMLSelectElement;
      selectElement.value = '2';
      selectElement.dispatchEvent(new Event('change'));
      
      expect(onChangeSpy).toHaveBeenCalledWith('2');
    });

    it('should register onTouched callback', () => {
      const onTouchedSpy = jasmine.createSpy('onTouched');
      component.registerOnTouched(onTouchedSpy);
      
      const compiled = fixture.nativeElement as HTMLElement;
      const selectElement = compiled.querySelector('select') as HTMLSelectElement;
      selectElement.dispatchEvent(new Event('blur'));
      
      expect(onTouchedSpy).toHaveBeenCalled();
    });

    it('should set disabled state via setDisabledState', () => {
      component.setDisabledState(true);
      fixture.detectChanges();
      
      expect(component.disabled).toBe(true);
      
      const compiled = fixture.nativeElement as HTMLElement;
      const selectElement = compiled.querySelector('select') as HTMLSelectElement;
      expect(selectElement.disabled).toBe(true);
    });

    it('should enable via setDisabledState', () => {
      component.disabled = true;
      fixture.detectChanges();
      
      component.setDisabledState(false);
      fixture.detectChanges();
      
      expect(component.disabled).toBe(false);
      
      const compiled = fixture.nativeElement as HTMLElement;
      const selectElement = compiled.querySelector('select') as HTMLSelectElement;
      expect(selectElement.disabled).toBe(false);
    });

    it('should emit valueChange when value changes', () => {
      const valueChangeSpy = jasmine.createSpy('valueChange');
      component.valueChange.subscribe(valueChangeSpy);
      
      const options: SelectOption[] = [
        { label: 'Option 1', value: '1' },
        { label: 'Option 2', value: '2' },
      ];
      component.options = options;
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      const selectElement = compiled.querySelector('select') as HTMLSelectElement;
      selectElement.value = '2';
      selectElement.dispatchEvent(new Event('change'));
      
      expect(valueChangeSpy).toHaveBeenCalledWith('2');
    });

    it('should handle null option values correctly with NULL_VALUE_MARKER', () => {
      const options: SelectOption[] = [
        { label: 'Never', value: null },
        { label: '30 days', value: 30 },
      ];
      component.options = options;
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      const selectElement = compiled.querySelector('select') as HTMLSelectElement;
      const firstOption = selectElement.querySelector('option') as HTMLOptionElement;
      
      // Should use NULL_VALUE_MARKER for null values (defined as '__NULL__')
      expect(firstOption.value).toBe('__NULL__');
      
      // When selected, should emit null
      const onChangeSpy = jasmine.createSpy('onChange');
      component.registerOnChange(onChangeSpy);
      
      selectElement.value = '__NULL__';
      selectElement.dispatchEvent(new Event('change'));
      
      expect(onChangeSpy).toHaveBeenCalledWith(null);
    });

    it('should work with reactive forms', () => {
      // This test would typically be done in an integration test with FormControl
      // but we can verify the basic functionality here
      const onChangeSpy = jasmine.createSpy('onChange');
      const onTouchedSpy = jasmine.createSpy('onTouched');
      
      component.registerOnChange(onChangeSpy);
      component.registerOnTouched(onTouchedSpy);
      
      // Simulate form control setting value
      component.writeValue('test-value');
      expect(component.value).toBe('test-value');
      
      // Simulate user interaction
      const options: SelectOption[] = [
        { label: 'Option 1', value: 'test-value' },
        { label: 'Option 2', value: 'other-value' },
      ];
      component.options = options;
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      const selectElement = compiled.querySelector('select') as HTMLSelectElement;
      selectElement.value = 'other-value';
      selectElement.dispatchEvent(new Event('change'));
      selectElement.dispatchEvent(new Event('blur'));
      
      expect(onChangeSpy).toHaveBeenCalledWith('other-value');
      expect(onTouchedSpy).toHaveBeenCalled();
    });
  });
});
