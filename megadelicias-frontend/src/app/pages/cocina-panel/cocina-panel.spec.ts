import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CocinaPanel } from './cocina-panel';

describe('CocinaPanel', () => {
  let component: CocinaPanel;
  let fixture: ComponentFixture<CocinaPanel>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CocinaPanel]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CocinaPanel);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
