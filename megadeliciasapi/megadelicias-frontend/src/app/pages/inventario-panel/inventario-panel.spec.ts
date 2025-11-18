import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InventarioPanel } from './inventario-panel';

describe('InventarioPanel', () => {
  let component: InventarioPanel;
  let fixture: ComponentFixture<InventarioPanel>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InventarioPanel]
    })
    .compileComponents();

    fixture = TestBed.createComponent(InventarioPanel);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
