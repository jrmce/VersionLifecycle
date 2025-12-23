import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-how-to-use',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './how-to-use.component.html',
  styleUrls: ['./how-to-use.component.css']
})
export class HowToUseComponent {
  // Sections for table of contents
  sections = [
    { id: 'overview', title: 'Overview', icon: 'info' },
    { id: 'workflow', title: 'Complete Workflow', icon: 'flow' },
    { id: 'applications', title: 'Managing Applications', icon: 'app' },
    { id: 'versions', title: 'Version Tracking', icon: 'version' },
    { id: 'environments', title: 'Environment Setup', icon: 'env' },
    { id: 'deployments', title: 'Deployment Process', icon: 'deploy' },
    { id: 'webhooks', title: 'Webhook Integration', icon: 'webhook' },
    { id: 'examples', title: 'Real-Life Examples', icon: 'example' }
  ];

  scrollToSection(sectionId: string): void {
    const element = document.getElementById(sectionId);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }
}
