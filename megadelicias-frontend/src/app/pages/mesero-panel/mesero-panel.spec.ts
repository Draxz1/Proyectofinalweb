import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MeseroPanel } from './mesero-panel';

describe('MeseroPanel', () => {
  let component: MeseroPanel;
  let fixture: ComponentFixture<MeseroPanel>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MeseroPanel]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MeseroPanel);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
