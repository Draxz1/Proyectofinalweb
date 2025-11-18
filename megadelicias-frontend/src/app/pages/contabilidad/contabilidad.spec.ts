import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Contabilidad } from './contabilidad';

describe('Contabilidad', () => {
  let component: Contabilidad;
  let fixture: ComponentFixture<Contabilidad>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Contabilidad]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Contabilidad);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
