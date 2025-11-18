import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CajaPanel } from './caja-panel';

describe('CajaPanel', () => {
  let component: CajaPanel;
  let fixture: ComponentFixture<CajaPanel>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CajaPanel]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CajaPanel);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
