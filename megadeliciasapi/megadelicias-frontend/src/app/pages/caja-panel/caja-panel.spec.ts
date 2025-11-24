import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CajaPanelComponent } from './caja-panel';

describe('CajaPanelComponent', () => {
  let component: CajaPanelComponent;
  let fixture: ComponentFixture<CajaPanelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CajaPanelComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(CajaPanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
