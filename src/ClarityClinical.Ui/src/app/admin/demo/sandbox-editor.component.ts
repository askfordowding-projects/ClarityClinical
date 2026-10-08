import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DemoAdminService } from './demo-admin.service';
import { DemoSandboxSnapshot, DemoSandboxValue } from './demo-admin.models';

@Component({ selector: 'app-sandbox-editor', standalone: true, imports: [RouterLink], templateUrl: './sandbox-editor.component.html', styleUrl: './sandbox-editor.component.scss', changeDetection: ChangeDetectionStrategy.OnPush })
export class SandboxEditorComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly service = inject(DemoAdminService);
  protected readonly sandbox = signal<DemoSandboxSnapshot | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly savedMessage = signal<string | null>(null);
  protected readonly givenName = signal(''); protected readonly familyName = signal(''); protected readonly dateOfBirth = signal('');
  protected readonly conditionsText = signal(''); protected readonly allergiesText = signal(''); protected readonly medicationsText = signal('');
  protected readonly scenarioName = signal(''); protected readonly description = signal(''); protected readonly setting = signal(''); protected readonly sourceLanguage = signal(''); protected readonly clinicianLanguage = signal('');
  private scenarioKey = '';

  ngOnInit(): void {
    this.scenarioKey = this.route.snapshot.paramMap.get('scenarioKey') ?? '';
    if (!this.scenarioKey) { this.error.set('A scenario key is required.'); return; }
    this.service.createSandbox(this.scenarioKey).subscribe({ next: value => this.load(value), error: () => this.error.set('Your sandbox copy could not be created.') });
  }

  protected update(signalValue: { set(value: string): void }, event: Event): void { signalValue.set((event.target as HTMLInputElement | HTMLTextAreaElement).value); }

  protected savePatient(): void {
    this.savedMessage.set(null); this.error.set(null);
    this.service.updateSandboxPatient(this.scenarioKey, { givenName: this.givenName(), familyName: this.familyName(), dateOfBirth: this.dateOfBirth(), conditions: this.parseValues(this.conditionsText()), allergies: this.parseValues(this.allergiesText()), medications: this.parseValues(this.medicationsText()) }).subscribe({ next: value => { this.load(value); this.savedMessage.set('Sandbox patient saved.'); }, error: () => this.error.set('Sandbox patient could not be saved.') });
  }

  protected saveScenario(): void {
    this.savedMessage.set(null); this.error.set(null);
    this.service.updateSandboxScenario(this.scenarioKey, { name: this.scenarioName(), description: this.description(), setting: this.setting(), sourceLanguage: this.sourceLanguage(), clinicianLanguage: this.clinicianLanguage() }).subscribe({ next: value => { this.load(value); this.savedMessage.set('Sandbox scenario saved.'); }, error: () => this.error.set('Sandbox scenario could not be saved.') });
  }

  private load(value: DemoSandboxSnapshot): void {
    this.sandbox.set(value);
    this.givenName.set(value.patient.givenName); this.familyName.set(value.patient.familyName); this.dateOfBirth.set(value.patient.dateOfBirth);
    this.conditionsText.set(this.formatValues(value.patient.conditions)); this.allergiesText.set(this.formatValues(value.patient.allergies)); this.medicationsText.set(this.formatValues(value.patient.medications));
    this.scenarioName.set(value.scenario.name); this.description.set(value.scenario.description); this.setting.set(value.scenario.setting); this.sourceLanguage.set(value.scenario.sourceLanguage); this.clinicianLanguage.set(value.scenario.clinicianLanguage);
  }
  private formatValues(values: DemoSandboxValue[]): string { return values.map(v => `${v.code} | ${v.displayName}`).join('\n'); }
  private parseValues(text: string): DemoSandboxValue[] { return text.split(/\r?\n/).map(line => line.trim()).filter(Boolean).map(line => { const [code, ...rest] = line.split('|'); return { code: code.trim(), displayName: rest.join('|').trim() || code.trim() }; }); }
}
