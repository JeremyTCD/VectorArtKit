import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ManageAccountComponent } from './manage-account.component';
import { ManageAccountRouting } from './manage-account.routing';
import { ManageAccountGuard } from './manage-account.guard';
import { ChangeAlternativeEmailComponent } from './change-alternative-email/change-alternative-email.component';
import { ChangeEmailComponent } from './change-email/change-email.component';
import { ChangeDisplayNameComponent } from './change-display-name/change-display-name.component';
import { DynamicFormGuard } from '../shared/dynamic-forms/dynamic-form/dynamic-form.guard';
import { DynamicFormsModule } from '../shared/dynamic-forms/dynamic-forms.module';

@NgModule({
    imports: [
        ManageAccountRouting,
        CommonModule,
        DynamicFormsModule
    ],
    providers: [
        ManageAccountGuard,
        DynamicFormGuard
    ],
    declarations: [
        ManageAccountComponent,
        ChangeAlternativeEmailComponent,
        ChangeEmailComponent,
        ChangeDisplayNameComponent
    ]
})
export class ManageAccountModule { }
