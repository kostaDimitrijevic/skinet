import { Component, ElementRef, Input, OnInit, Self, ViewChild } from '@angular/core';
import { ControlValueAccessor, NgControl } from '@angular/forms';

@Component({
  selector: 'app-text-input',
  templateUrl: './text-input.component.html',
  styleUrls: ['./text-input.component.scss']
})
export class TextInputComponent implements OnInit, ControlValueAccessor {

  @ViewChild('input', {static: true}) input: ElementRef; // sluzi za detekciju promene vrednosti ovog objekta
  @Input() type = 'text';
  @Input() label: string;

  constructor(@Self() public controlDir: NgControl) { // The @Self decorator instructs Angular to look for the dependency only in the local injector.
                                                      //The local injector is the injector that is part of the current component or directive.

    this.controlDir.valueAccessor = this;
  }

  ngOnInit(): void {
    const control = this.controlDir.control;
    const validators = control.validator ? [control.validator] : []; // ovo je sinhroni validator
    const asyncValidators = control.asyncValidator ? [control.asyncValidator] : []; // ovo je ako hocemo nesto da posaljemo apiju za proveru

    control.setValidators(validators);
    control.setAsyncValidators(asyncValidators);
    control.updateValueAndValidity(); // ovo ce da update-uje i validira nasu formu na inicijalizaciji
  }

  onChange(event){}

  onTouched(){}

  writeValue(obj: any): void {
    this.input.nativeElement.value = obj || '';
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }
  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

}
